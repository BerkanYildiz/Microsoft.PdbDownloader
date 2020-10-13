namespace Microsoft.PdbDownloader.Logic.Pe.Structures
{
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential, Size = 8)]
    public struct SYSTEM_MODULE_INFORMATION
    {
        public uint NumberOfModules;
    }
}
