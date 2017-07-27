using System;
using System.Runtime.InteropServices;

namespace CSLibretro
{
    [StructLayout(LayoutKind.Sequential)]
    public struct GameInfo
    {
        public string Path;
        public IntPtr Data;
        public uint Size;
        public string Meta;
    }
}
