using System;
using System.Runtime.InteropServices;

namespace CSLibretro
{
    [StructLayout(LayoutKind.Sequential)]
    public struct SystemTiming
    {
        public double FPS;
        public double SampleRate;
    }
}
