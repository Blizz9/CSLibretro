using System;
using System.Runtime.InteropServices;
using System.Text;

namespace CSLibretro
{
    public class Win32API
    {
        [DllImport("kernel32.dll", CallingConvention = CallingConvention.StdCall, SetLastError = true)]
        public static extern IntPtr LoadLibrary(string dllPath);

        [DllImport("kernel32.dll", CallingConvention = CallingConvention.StdCall, SetLastError = true)]
        public static extern IntPtr GetProcAddress(IntPtr dll, string methodName);

        [DllImport("msvcrt.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern int _snprintf([MarshalAs(UnmanagedType.LPStr)] StringBuilder buffer, IntPtr count, IntPtr format, params IntPtr[] arguments);
    }
}
