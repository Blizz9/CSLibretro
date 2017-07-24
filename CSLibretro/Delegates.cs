using System;
using System.Runtime.InteropServices;

namespace CSLibretro
{
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate uint APIVersionPrototype();

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void GetSystemAVInfoPrototype(out SystemAVInfo systemAVInfo);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void GetSystemInfoPrototype(out SystemInfo systemInfo);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void InitPrototype();

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.U1)]
    public delegate bool LoadGamePrototype(ref GameInfo gameInfo);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void RunPrototype();

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void SetAudioSamplePrototype([MarshalAs(UnmanagedType.FunctionPtr)]AudioSampleHandler audioSampleHandler);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void SetAudioSampleBatchPrototype([MarshalAs(UnmanagedType.FunctionPtr)]AudioSampleBatchHandler audioSampleBatchHandler);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void SetEnvironmentPrototype([MarshalAs(UnmanagedType.FunctionPtr)]EnvironmentHandler environmentHandler);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void SetInputPollPrototype([MarshalAs(UnmanagedType.FunctionPtr)]InputPollHandler inputPollHandler);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void SetInputStatePrototype([MarshalAs(UnmanagedType.FunctionPtr)]InputStateHandler inputStateHandler);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void SetVideoRefreshPrototype([MarshalAs(UnmanagedType.FunctionPtr)]VideoRefreshHandler videoRefreshHandler);



    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void AudioSampleHandler(short left, short right);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void AudioSampleBatchHandler(IntPtr data, UIntPtr frames);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.U1)]
    public delegate bool EnvironmentHandler(uint command, IntPtr data);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void InputPollHandler();

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void InputStateHandler(uint port, uint device, uint index, uint id);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void LogHandler(int level, IntPtr fmt, params IntPtr[] arguments);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void VideoRefreshHandler(IntPtr data, uint width, uint height, UIntPtr pitch);
}
