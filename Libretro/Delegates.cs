﻿using System;
using System.Runtime.InteropServices;

namespace com.PixelismGames.CSLibretro.Libretro
{
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate uint APIVersionSignature();

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate IntPtr GetMemoryDataSignature(MemoryType id);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate uint GetMemorySizeSignature(MemoryType id);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void GetSystemAVInfoSignature(out SystemAVInfo systemAVInfo);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void GetSystemInfoSignature(out SystemInfo systemInfo);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void InitSignature();

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate bool LoadGameSignature(ref GameInfo gameInfo);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void RunSignature();

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate bool SerializeSignature(IntPtr data, uint size);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate uint SerializeSizeSignature();

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void SetAudioSampleSignature(AudioSampleHandler audioSampleHandler);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void SetAudioSampleBatchSignature(AudioSampleBatchHandler audioSampleBatchHandler);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void SetEnvironmentSignature(EnvironmentHandler environmentHandler);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void SetInputPollSignature(InputPollHandler inputPollHandler);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void SetInputStateSignature(InputStateHandler inputStateHandler);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void SetVideoRefreshSignature(VideoRefreshHandler videoRefreshHandler);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate bool UnserializeSignature(IntPtr data, uint size);



    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void AudioSampleHandler(short left, short right);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate uint AudioSampleBatchHandler(IntPtr data, uint frames);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate bool EnvironmentHandler(uint command, IntPtr data); // eventually an enum can be used here

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void InputPollHandler();

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate short InputStateHandler(uint port, uint device, uint index, uint id); // some enums can be used here

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void LogHandler(LogLevel level, string fmt, params IntPtr[] arguments);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void VideoRefreshHandler(IntPtr data, uint width, uint height, uint pitch);
}
