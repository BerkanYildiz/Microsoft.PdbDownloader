namespace Microsoft.PdbDownloader.Logic.Pe
{
    using System;
    using System.Diagnostics;
    using System.Runtime.InteropServices;

    using Microsoft.PdbDownloader.Logic.Pe.Structures;

    internal static class PeUtils
    {
        internal static unsafe bool TryGetPdbMetadata(byte[] Buffer, out string PdbName, out string PdbHash)
        {
            PdbName = null;
            PdbHash = null;

            // 
            // Build a pointer to the managed buffer.
            // 

            fixed (byte* FixedBuffer = Buffer)
            {
                var DosHeader = (IMAGE_DOS_HEADER*) FixedBuffer;
                var NtHeaders = (IMAGE_NT_HEADERS64*) IntPtr.Add(new IntPtr(FixedBuffer), DosHeader->e_lfanew).ToPointer();

                // 
                // Parse the debug directory.
                // 

                var DebugDirectory = &NtHeaders->OptionalHeader.Debug;

                Debug.WriteLine("[*] DebugDirectory.VirtualAddress: 0x" + DebugDirectory->VirtualAddress.ToString("X8"));
                Debug.WriteLine("[*] DebugDirectory.Size: " + DebugDirectory->Size);

                if (DebugDirectory->VirtualAddress != 0)
                {
                    var DebugEntries = (IMAGE_DEBUG_DIRECTORY*) VaToRva(Buffer, IntPtr.Add(new IntPtr(FixedBuffer), (int)DebugDirectory->VirtualAddress).ToPointer());
                    var DebugEntriesSizeRead = 0;
                    var DebugEntriesIndex = 0;

                    for (var DebugEntry = DebugEntries;
                        DebugEntriesSizeRead < DebugDirectory->Size;
                        DebugEntriesSizeRead += sizeof(IMAGE_DEBUG_DIRECTORY), DebugEntry++)
                    {
                        // 
                        // Check if this is a CodeView debug entry.
                        // 

                        if (DebugEntry->Type == 2)
                        {
                            var DebugData = (IMAGE_DEBUG_DATA*) IntPtr.Add(new IntPtr(FixedBuffer), (int) DebugEntry->PointerToRawData);

                            // 
                            // Set the returned values.
                            // 

                            PdbName = Marshal.PtrToStringAnsi(new IntPtr(&DebugData->PdbFileName));
                            PdbHash = $"{DebugData->Guid:N}{DebugData->Age}".ToUpper();
                            break;
                        }
                    }
                }
            }

            return !string.IsNullOrEmpty(PdbName) && !string.IsNullOrEmpty(PdbHash);
        }

        internal static unsafe void* VaToRva(byte[] Buffer, void* VirtualAddress)
        {
            // 
            // Build a pointer to the managed buffer.
            // 

            fixed (byte* FixedBuffer = Buffer)
            {
                var DosHeader = (IMAGE_DOS_HEADER*) FixedBuffer;
                var NtHeaders = (IMAGE_NT_HEADERS64*) IntPtr.Add(new IntPtr(FixedBuffer), DosHeader->e_lfanew).ToPointer();

                // 
                // Parse the sections.
                // 

                var CurrentSection = (IMAGE_SECTION_HEADER*) IntPtr.Add(new IntPtr(NtHeaders), sizeof(IMAGE_NT_HEADERS64)).ToPointer();

                for (var I = 0; I < NtHeaders->FileHeader.NumberOfSections; I++, CurrentSection++)
                {
                    var BeginSection = IntPtr.Add(new IntPtr(FixedBuffer), (int) CurrentSection->VirtualAddress).ToPointer();
                    var EndSection = IntPtr.Add(new IntPtr(BeginSection), (int) CurrentSection->VirtualSize).ToPointer();

                    if (VirtualAddress >= BeginSection && VirtualAddress < EndSection)
                    {
                        return IntPtr.Add(IntPtr.Subtract(new IntPtr(VirtualAddress), (int) CurrentSection->VirtualAddress), (int) CurrentSection->PointerToRawData).ToPointer();
                    }
                }
            }

            return null;
        }
    }
}
