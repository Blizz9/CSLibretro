using System.Runtime.InteropServices;

namespace CSLibretro
{
    [StructLayout(LayoutKind.Sequential)]
    public struct LogCallback
    {
        public LogHandler Log;
    }
}
