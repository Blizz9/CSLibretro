using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Text;

namespace CSLibretro
{
    public class Wrapper
    {
        private const string DLL_NAME = "snes9x_libretro.dll";
        //private const string DLL_NAME = "nestopia_libretro.dll";
        //private const string DLL_NAME = "gambatte_libretro.dll";

        private const string ROM_NAME = "sf2.sfc";
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
        private long _frameCount;

        public Wrapper()
        {
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
        }

        #region Run

        public void Run()
        {
            //Debug.WriteLine(_apiVersion());

            _setEnvironment(environmentHandler);
            _setVideoRefresh(videoRefreshHandler);
            _setAudioSample(audioSampleHandler);
            _setAudioSampleBatch(audioSampleBatchHandler);
            _setInputPoll(inputPollHandler);
            _setInputState(inputStateHandler);

            _init();

            GameInfo gameInfo = new GameInfo() { Path = ROM_NAME, Data = IntPtr.Zero, Size = UIntPtr.Zero, Meta = null };
            _loadGame(ref gameInfo);

            //SystemInfo systemInfo = new SystemInfo();
            //_getSystemInfo(out systemInfo);
            //systemInfo.LibraryName = Marshal.PtrToStringAnsi(systemInfo.LibraryNamePointer);
            //systemInfo.LibraryVersion = Marshal.PtrToStringAnsi(systemInfo.LibraryVersionPointer);
            //systemInfo.ValidExtensions = Marshal.PtrToStringAnsi(systemInfo.ValidExtensionsPointer);

            //SystemAVInfo systemAVInfo = new SystemAVInfo();
            //_getSystemAVInfo(out systemAVInfo);

            while (_frameCount <= 780)
            {
                _run();
                _frameCount++;
            }
        }

        #endregion

        #region Handlers

        private void audioSampleHandler(short left, short right)
        {
            Debug.WriteLine("Audio Sample");
        }

        private void audioSampleBatchHandler(IntPtr data, UIntPtr frames)
        {
            Debug.WriteLine("Audio Sample Batch");
        }

        private bool environmentHandler(uint command, IntPtr data)
        {
            Debug.WriteLine("Environment " + command);

            if (command == 27)
            {
                LogCallback logCallbackStruct = new LogCallback();
                logCallbackStruct.Log = logCallback;
                Marshal.StructureToPtr(logCallbackStruct, data, false);
                return (true);
            }

            return (false);
        }

        private void inputPollHandler()
        {
            Debug.WriteLine("Input Poll");
        }

        private void inputStateHandler(uint port, uint device, uint index, uint id)
        {
            //Debug.WriteLine("Input State");
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
            if (_frameCount % 60 == 0)
            {
                Bitmap bitmap = new Bitmap((int)width, (int)height, (int)pitch, PixelFormat.Format16bppArgb1555, data);
                bitmap.Save("output" + _frameCount / 60 + ".bmp", ImageFormat.Bmp);
                //_mainWindow.SetScreen(bitmap);
            }
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
