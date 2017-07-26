using System;
using System.Runtime.InteropServices;

namespace CSLibretro
{
    [StructLayout(LayoutKind.Sequential)]
    public struct SystemAVInfo
    {
        public GameGeometry Geometry;
        public SystemTiming Timing;
    }
}
