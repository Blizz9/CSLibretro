using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Input;

namespace CSLibretro
{
    public class Wrapper
    {
        private const string DLL_NAME = "snes9x_libretro.dll";
        //private const string DLL_NAME = "nestopia_libretro.dll";
        //private const string DLL_NAME = "gambatte_libretro.dll";

        private const string ROM_NAME = "smw.sfc";
        //private const string ROM_NAME = "smb.nes";
        //private const string ROM_NAME = "sml.gb";
        
        private APIVersionPrototype _apiVersion;
        private GetSystemAVInfoPrototype _getSystemAVInfo;
        private GetSystemInfoPrototype _getSystemInfo;
        private InitPrototype _init;
        private LoadGamePrototype _loadGame;
        private RunPrototype _run;
        private SetAudioSamplePrototype _setAudioSample;
        private SetAudioSampleBatchPrototype _setAudioSampleBatch;
        private SetEnvironmentPrototype _setEnvironment;
        private SetInputPollPrototype _setInputPoll;
        private SetInputStatePrototype _setInputState;
        private SetVideoRefreshPrototype _setVideoRefresh;

        private IntPtr _libretroDLL;

        private Action<Bitmap> _frameCallback;
        private Action<List<Tuple<Key, int, bool>>> _inputCallback;

        private List<Tuple<Key, int, bool>> _inputs;

        public long FrameCount = 0;
        public PixelFormat PixelFormat = PixelFormat.Unknown;
        public SystemInfo SystemInfo;
        public SystemAVInfo SystemAVInfo;

        public Wrapper(Action<Bitmap> frameCallback, Action<List<Tuple<Key, int, bool>>> inputCallback)
        {
            _frameCallback = frameCallback;
            _inputCallback = inputCallback;

            _libretroDLL = Win32API.LoadLibrary(DLL_NAME);

            _apiVersion = getDelegate<APIVersionPrototype>("retro_api_version");
            _getSystemAVInfo = getDelegate<GetSystemAVInfoPrototype>("retro_get_system_av_info");
            _getSystemInfo = getDelegate<GetSystemInfoPrototype>("retro_get_system_info");
            _init = getDelegate<InitPrototype>("retro_init");
            _loadGame = getDelegate<LoadGamePrototype>("retro_load_game");
            _run = getDelegate<RunPrototype>("retro_run");
            _setAudioSample = getDelegate<SetAudioSamplePrototype>("retro_set_audio_sample");
            _setAudioSampleBatch = getDelegate<SetAudioSampleBatchPrototype>("retro_set_audio_sample_batch");
            _setEnvironment = getDelegate<SetEnvironmentPrototype>("retro_set_environment");
            _setInputPoll = getDelegate<SetInputPollPrototype>("retro_set_input_poll");
            _setInputState = getDelegate<SetInputStatePrototype>("retro_set_input_state");
            _setVideoRefresh = getDelegate<SetVideoRefreshPrototype>("retro_set_video_refresh");

            Debug.WriteLine(_apiVersion());
        }

        #region Run

        public void Run()
        {
            _setEnvironment(environmentHandler);
            _setVideoRefresh(videoRefreshHandler);
            _setAudioSample(audioSampleHandler);
            _setAudioSampleBatch(audioSampleBatchHandler);
            _setInputPoll(inputPollHandler);
            _setInputState(inputStateHandler);

            _init();

            GameInfo gameInfo = new GameInfo() { Path = ROM_NAME, Data = IntPtr.Zero, Size = UIntPtr.Zero, Meta = null };
            _loadGame(ref gameInfo);

            SystemInfo = new SystemInfo();
            _getSystemInfo(out SystemInfo);
            SystemInfo.LibraryName = Marshal.PtrToStringAnsi(SystemInfo.LibraryNamePointer);
            SystemInfo.LibraryVersion = Marshal.PtrToStringAnsi(SystemInfo.LibraryVersionPointer);
            SystemInfo.ValidExtensions = Marshal.PtrToStringAnsi(SystemInfo.ValidExtensionsPointer);

            SystemAVInfo = new SystemAVInfo();
            _getSystemAVInfo(out SystemAVInfo);

            double targetNanoseconds = 1 / SystemAVInfo.Timing.FPS * 1000000000;
            double leftoverNanoseconds = 0;

            while (FrameCount <= 78000)
            {
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();

                _run();

                FrameCount++;
                stopwatch.Stop();

                double elapsedNanoseconds = ((double)stopwatch.ElapsedTicks / (double)Stopwatch.Frequency) * 1000000000;
                leftoverNanoseconds += targetNanoseconds - elapsedNanoseconds;
                if (leftoverNanoseconds > 0)
                {
                    Thread.Sleep((int)(leftoverNanoseconds / 1000000));
                    leftoverNanoseconds %= 1000000;
                }
                else
                {
                    leftoverNanoseconds = 0;
                    Debug.WriteLine("HERE");
                }

                //double elapsedNanoseconds = ((double)stopwatch.ElapsedTicks / (double)Stopwatch.Frequency) * 1000000000;
                //double sleepNanoseconds = targetNanoseconds - elapsedNanoseconds;
                //if (sleepNanoseconds > 0)
                //    Thread.Sleep((int)(sleepNanoseconds / 1000000));
            }
        }

        #endregion

        #region Handlers

        private void audioSampleHandler(short left, short right)
        {
            //Debug.WriteLine("Audio Sample");
        }

        private void audioSampleBatchHandler(IntPtr data, UIntPtr frames)
        {
            //Debug.WriteLine("Audio Sample Batch");
        }

        private bool environmentHandler(uint command, IntPtr data)
        {
            //Debug.WriteLine("Environment: " + (EnvironmentCommand)command);

            switch ((EnvironmentCommand)command)
            {
                case EnvironmentCommand.GetCanDupe:
                    Marshal.WriteByte(data, 0, 1);
                    return (true);

                case EnvironmentCommand.SetPixelFormat:
                    PixelFormat = (PixelFormat)Marshal.ReadInt32(data);
                    return (true);

                case EnvironmentCommand.GetLogInterface:
                    LogCallback logCallbackStruct = new LogCallback();
                    logCallbackStruct.Log = logCallback;
                    Marshal.StructureToPtr(logCallbackStruct, data, false);
                    return (true);

                default:
                    return (false);
            }
        }

        private void inputPollHandler()
        {
            _inputs = new List<Tuple<Key, int, bool>>();
            _inputs.Add(new Tuple<Key, int, bool>(Key.K, 0, false));
            _inputs.Add(new Tuple<Key, int, bool>(Key.J, 1, false));
            _inputs.Add(new Tuple<Key, int, bool>(Key.G, 2, false));
            _inputs.Add(new Tuple<Key, int, bool>(Key.H, 3, false));
            _inputs.Add(new Tuple<Key, int, bool>(Key.W, 4, false));
            _inputs.Add(new Tuple<Key, int, bool>(Key.S, 5, false));
            _inputs.Add(new Tuple<Key, int, bool>(Key.A, 6, false));
            _inputs.Add(new Tuple<Key, int, bool>(Key.D, 7, false));
            _inputs.Add(new Tuple<Key, int, bool>(Key.O, 8, false));
            _inputs.Add(new Tuple<Key, int, bool>(Key.I, 9, false));
            _inputs.Add(new Tuple<Key, int, bool>(Key.D9, 10, false));
            _inputs.Add(new Tuple<Key, int, bool>(Key.D0, 11, false));
            _inputCallback(_inputs);
        }

        private short inputStateHandler(uint port, uint device, uint index, uint id)
        {
            if ((port == 0) && (device == 1))
                if (_inputs.Where(i => (i.Item2 == id) && i.Item3).Any())
                    return (1);

            return (0);
        }

        private static void logCallback(int level, IntPtr fmt, params IntPtr[] arguments)
        {
            Debug.WriteLine("Log");

            StringBuilder logMessage = new StringBuilder(256);

            while (true)
            {
                int length = Win32API._snprintf(logMessage, new IntPtr(logMessage.Capacity), fmt, arguments);

                if ((length <= 0) || (length >= logMessage.Capacity))
                {
                    logMessage.Capacity *= 2;
                    continue;
                }

                logMessage.Length = length;
                break;
            } while (logMessage.Length >= logMessage.Capacity);

            Debug.WriteLine(logMessage.ToString());
        }

        private void videoRefreshHandler(IntPtr data, uint width, uint height, UIntPtr pitch)
        {
            //if (FrameCount % 60 == 0)
            //{
                Bitmap bitmap = new Bitmap((int)width, (int)height, (int)pitch, System.Drawing.Imaging.PixelFormat.Format16bppRgb565, data);
                //bitmap.Save("output" + FrameCount / 60 + ".png", ImageFormat.Png);
                _frameCallback(bitmap);
            //}
        }

        #endregion

        #region Delegates

        private T getDelegate<T>(string functionName)
        {
            return ((T)Convert.ChangeType(Marshal.GetDelegateForFunctionPointer(Win32API.GetProcAddress(_libretroDLL, functionName), typeof(T)), typeof(T)));
        }

        #endregion
    }
}
