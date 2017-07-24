using System;
using System.Runtime.InteropServices;

namespace CSLibretro
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct GameInfo
    {
        public string Path;
        public IntPtr Data;
        public UIntPtr Size;
        public string Meta;
    }
}
