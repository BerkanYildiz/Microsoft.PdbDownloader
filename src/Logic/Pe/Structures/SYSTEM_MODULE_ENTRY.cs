namespace Microsoft.PdbDownloader.Logic.Pe.Structures
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential, Pack = 1, Size = 296)]
    public struct SYSTEM_MODULE_ENTRY
    {
        public UIntPtr Section;
        public UIntPtr MappedBase;
        public UIntPtr ImageBase;
        public int ImageSize;
        public uint Flags;
        public ushort LoadOrderIndex;
        public ushort InitOrderIndex;
        public ushort LoadCount;
        public ushort OffsetToFileName;
        public char FullPathName;
    }
}
