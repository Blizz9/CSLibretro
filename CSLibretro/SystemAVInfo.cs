using System;
using System.Runtime.InteropServices;

namespace CSLibretro
{
    [StructLayout(LayoutKind.Sequential)]
    public struct SystemAVInfo
    {
        [MarshalAs(UnmanagedType.Struct)] public GameGeometry Geometry;
        [MarshalAs(UnmanagedType.Struct)] public SystemTiming Timing;
    }
}
