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
    public delegate bool LoadGamePrototype(ref GameInfo gameInfo);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void RunPrototype();

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate uint SerializeSizePrototype();

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void SetAudioSamplePrototype(AudioSampleHandler audioSampleHandler);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void SetAudioSampleBatchPrototype(AudioSampleBatchHandler audioSampleBatchHandler);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void SetEnvironmentPrototype(EnvironmentHandler environmentHandler);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void SetInputPollPrototype(InputPollHandler inputPollHandler);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void SetInputStatePrototype(InputStateHandler inputStateHandler);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void SetVideoRefreshPrototype(VideoRefreshHandler videoRefreshHandler);



    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void AudioSampleHandler(short left, short right);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void AudioSampleBatchHandler(IntPtr data, UIntPtr frames);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate bool EnvironmentHandler(uint command, IntPtr data);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void InputPollHandler();

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate short InputStateHandler(uint port, uint device, uint index, uint id);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void LogHandler(int level, IntPtr fmt, params IntPtr[] arguments);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void VideoRefreshHandler(IntPtr data, uint width, uint height, UIntPtr pitch);
}
