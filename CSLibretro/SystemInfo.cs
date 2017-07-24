using System;
using System.Runtime.InteropServices;

namespace CSLibretro
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct SystemInfo
    {
        public IntPtr LibraryNamePointer;
        public IntPtr LibraryVersionPointer;
        public IntPtr ValidExtensionsPointer;
        //[MarshalAs(UnmanagedType.LPStr)] public string LibraryName; // <-- I still think there is a way to make this work
        [MarshalAs(UnmanagedType.U1)] public bool NeedFullpath;
        [MarshalAs(UnmanagedType.U1)] public bool BlockExtract;
        public string LibraryName;
        public string LibraryVersion;
        public string ValidExtensions;
    }
}
