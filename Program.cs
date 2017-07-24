using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace CSLibretro
{
    public static class Program
    {
        [DllImport("kernel32.dll", CallingConvention = CallingConvention.StdCall, SetLastError = true)]
        public static extern IntPtr LoadLibrary(string dllPath);

        [DllImport("kernel32.dll", CallingConvention = CallingConvention.StdCall, SetLastError = true)]
        public static extern IntPtr GetProcAddress(IntPtr dll, string methodName);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)] public delegate uint APIVersionDelegate();
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)] public delegate void AudioSampleDelegate(short left, short right);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)] public delegate void AudioSampleBatchDelegate(IntPtr data, UIntPtr frames);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)] [return: MarshalAs(UnmanagedType.U1)] public delegate bool EnvironmentDelegate(uint command, IntPtr data);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)] public delegate void GetSystemInfoDelegate(out SystemInfo systemInfo);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)] public delegate void GetSystemAVInfoDelegate(out SystemAVInfo systemAVInfo);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)] public delegate void InitDelegate();
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)] public delegate void InputPollDelegate();
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)] public delegate void InputStateDelegate(uint port, uint device, uint index, uint id);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)] [return: MarshalAs(UnmanagedType.U1)] public delegate bool LoadGameDelegate(ref GameInfo gameInfo);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)] public delegate void RunDelegate();
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)] public delegate void SetAudioSampleDelegate([MarshalAs(UnmanagedType.FunctionPtr)]AudioSampleDelegate audioSampleDelegate);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)] public delegate void SetAudioSampleBatchDelegate([MarshalAs(UnmanagedType.FunctionPtr)]AudioSampleBatchDelegate audioSampleBatchDelegate);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)] public delegate void SetEnvironmentDelegate([MarshalAs(UnmanagedType.FunctionPtr)]EnvironmentDelegate environmentDelegate);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)] public delegate void SetInputPollDelegate([MarshalAs(UnmanagedType.FunctionPtr)]InputPollDelegate inputPollDelegate);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)] public delegate void SetInputStateDelegate([MarshalAs(UnmanagedType.FunctionPtr)]InputStateDelegate inputStateDelegate);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)] public delegate void SetVideoRefreshDelegate([MarshalAs(UnmanagedType.FunctionPtr)]VideoRefreshDelegate videoRefreshDelegate);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)] public delegate void VideoRefreshDelegate(IntPtr data, uint width, uint height, UIntPtr pitch);

        private static MainWindow _mainWindow;
        private static long _frameCount;

        //disgusting stuff. since C# doesn't support varargs, I'll assume it's not
        // called with more than 8 arguments and forward all eight to sprintf.
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void log_printf_t(int level, IntPtr fmt,
                            IntPtr arg1, IntPtr arg2, IntPtr arg3, IntPtr arg4,
                            IntPtr arg5, IntPtr arg6, IntPtr arg7, IntPtr arg8);

        [StructLayout(LayoutKind.Sequential)]
        public struct log_callback
        {
            public log_printf_t log;
        }

        [DllImport("msvcrt.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        private static extern int _snprintf([MarshalAs(UnmanagedType.LPStr)] System.Text.StringBuilder str, IntPtr length, IntPtr format,
                                        IntPtr arg1, IntPtr arg2, IntPtr arg3, IntPtr arg4,
                                        IntPtr arg5, IntPtr arg6, IntPtr arg7, IntPtr arg8);

        private static void log_printf_cb(int level, IntPtr fmt,
            IntPtr arg1, IntPtr arg2, IntPtr arg3, IntPtr arg4,
            IntPtr arg5, IntPtr arg6, IntPtr arg7, IntPtr arg8)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder(256);
            while (true)
            { // somewhat weird, but that's the best I could do without duplicating anything
                int len = _snprintf(sb, new IntPtr(sb.Capacity), fmt, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8);
                //stupid _snprintf, returning negative values on overflow instead of the length. do it properly.
                if (len <= 0 || len >= sb.Capacity)
                {
                    sb.Capacity *= 2;
                    continue;
                }
                sb.Length = len;
                break;
            } while (sb.Length >= sb.Capacity) ;

            //log_cb((LogLevel)level, sb.ToString());
        }

        public static void MainBak(MainWindow mainWindow)
        {
            _mainWindow = mainWindow;

            IntPtr dll = LoadLibrary("snes9x_libretro.dll");
            //IntPtr dll = LoadLibrary("nestopia_libretro.dll");

            APIVersionDelegate apiVersion = (APIVersionDelegate)Marshal.GetDelegateForFunctionPointer(GetProcAddress(dll, "retro_api_version"), typeof(APIVersionDelegate));
            Debug.WriteLine(apiVersion());

            SetEnvironmentDelegate setEnvironment = (SetEnvironmentDelegate)Marshal.GetDelegateForFunctionPointer(GetProcAddress(dll, "retro_set_environment"), typeof(SetEnvironmentDelegate));
            setEnvironment(environmentCallback);

            SetVideoRefreshDelegate setVideoRefresh = (SetVideoRefreshDelegate)Marshal.GetDelegateForFunctionPointer(GetProcAddress(dll, "retro_set_video_refresh"), typeof(SetVideoRefreshDelegate));
            setVideoRefresh(videoRefreshCallback);

            SetAudioSampleDelegate setAudioSample = (SetAudioSampleDelegate)Marshal.GetDelegateForFunctionPointer(GetProcAddress(dll, "retro_set_audio_sample"), typeof(SetAudioSampleDelegate));
            setAudioSample(audioSampleCallback);

            SetAudioSampleBatchDelegate setAudioSampleBatch = (SetAudioSampleBatchDelegate)Marshal.GetDelegateForFunctionPointer(GetProcAddress(dll, "retro_set_audio_sample_batch"), typeof(SetAudioSampleBatchDelegate));
            setAudioSampleBatch(audioSampleBatchCallback);

            SetInputPollDelegate setInputPoll = (SetInputPollDelegate)Marshal.GetDelegateForFunctionPointer(GetProcAddress(dll, "retro_set_input_poll"), typeof(SetInputPollDelegate));
            setInputPoll(inputPollCallback);

            SetInputStateDelegate setInputState = (SetInputStateDelegate)Marshal.GetDelegateForFunctionPointer(GetProcAddress(dll, "retro_set_input_state"), typeof(SetInputStateDelegate));
            setInputState(inputStateCallback);

            InitDelegate init = (InitDelegate)Marshal.GetDelegateForFunctionPointer(GetProcAddress(dll, "retro_init"), typeof(InitDelegate));
            init();

            LoadGameDelegate loadGame = (LoadGameDelegate)Marshal.GetDelegateForFunctionPointer(GetProcAddress(dll, "retro_load_game"), typeof(LoadGameDelegate));
            GameInfo gameInfo = new GameInfo() { Path = "sf2.sfc", Data = IntPtr.Zero, Size = UIntPtr.Zero, Meta = null };
            //GameInfo gameInfo = new GameInfo() { Path = "smb.nes", Data = IntPtr.Zero, Size = UIntPtr.Zero, Meta = null };
            loadGame(ref gameInfo);

            GetSystemInfoDelegate getSystemInfo = (GetSystemInfoDelegate)Marshal.GetDelegateForFunctionPointer(GetProcAddress(dll, "retro_get_system_info"), typeof(GetSystemInfoDelegate));
            SystemInfo systemInfo = new SystemInfo();
            getSystemInfo(out systemInfo);
            systemInfo.LibraryName = Marshal.PtrToStringAnsi(systemInfo.LibraryNamePointer);
            systemInfo.LibraryVersion = Marshal.PtrToStringAnsi(systemInfo.LibraryVersionPointer);
            systemInfo.ValidExtensions = Marshal.PtrToStringAnsi(systemInfo.ValidExtensionsPointer);

            GetSystemAVInfoDelegate getSystemAVInfo = (GetSystemAVInfoDelegate)Marshal.GetDelegateForFunctionPointer(GetProcAddress(dll, "retro_get_system_av_info"), typeof(GetSystemAVInfoDelegate));
            SystemAVInfo systemAVInfo = new SystemAVInfo();
            getSystemAVInfo(out systemAVInfo);

            RunDelegate run = (RunDelegate)Marshal.GetDelegateForFunctionPointer(GetProcAddress(dll, "retro_run"), typeof(RunDelegate));

            for (int i = 0; i < 6000; i++)
            {
                run();
                _frameCount++;
            }
        }

        private static bool environmentCallback(uint command, IntPtr data)
        {
            Debug.WriteLine("Environment " + command);

            if (command == 27)
            {
                log_callback log = new log_callback();
                log.log = log_printf_cb;
                Marshal.StructureToPtr(log, data, false);
                return (true);
            }

            return (false);
        }

        private static void videoRefreshCallback(IntPtr data, uint width, uint height, UIntPtr pitch)
        {
            //if (_frameCount % 60 == 0)
            //{
                Bitmap bitmap = new Bitmap((int)width, (int)height, (int)pitch, PixelFormat.Format16bppArgb1555, data);
                //bitmap.Save("output" + _frameCount / 60 + ".bmp", ImageFormat.Bmp);
                _mainWindow.SetScreen(bitmap);
            //}
        }

        private static void audioSampleCallback(short left, short right)
        {
            Debug.WriteLine("Audio Sample");
        }

        private static void audioSampleBatchCallback(IntPtr data, UIntPtr frames)
        {
            Debug.WriteLine("Audio Sample Batch");
        }

        private static void inputPollCallback()
        {
            Debug.WriteLine("Input Poll");
        }

        private static void inputStateCallback(uint port, uint device, uint index, uint id)
        {
            //Debug.WriteLine("Input State");
        }

        private static Delegate LoadFromDLL(string name, Type type, IntPtr DllHandle)
        {
            IntPtr func = GetProcAddress(DllHandle, name);
            if (func == IntPtr.Zero) throw new ArgumentException("The given DLL is not a libretro core");
            return Marshal.GetDelegateForFunctionPointer(func, type);
        }
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct GameInfo
    {
        public string Path;
        public IntPtr Data;
        public UIntPtr Size;
        public string Meta;
    };

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct SystemInfo
    {
        public IntPtr LibraryNamePointer;
        public IntPtr LibraryVersionPointer;
        public IntPtr ValidExtensionsPointer;
        //[MarshalAs(UnmanagedType.LPStr)] public string LibraryName; // <-- I still think there is a way to make this work
        [MarshalAs(UnmanagedType.U1)] public bool NeedFullpath;
        [MarshalAs(UnmanagedType.U1)] public bool BlockExtract;
        public string LibraryName;
        public string LibraryVersion;
        public string ValidExtensions;
    };

    [StructLayout(LayoutKind.Sequential)]
    public struct SystemAVInfo
    {
        [MarshalAs(UnmanagedType.Struct)] public GameGeometry Geometry;
        [MarshalAs(UnmanagedType.Struct)] public SystemTiming Timing;
    };

    [StructLayout(LayoutKind.Sequential)]
    public struct GameGeometry
    {
        public uint BaseWidth;
        public uint BaseHeight;
        public uint MaxWidth;
        public uint MaxHeight;
        public float AspectRatio;
    };

    [StructLayout(LayoutKind.Sequential)]
    public struct SystemTiming
    {
        public double FPS;
        public double SampleRate;
    };
}
