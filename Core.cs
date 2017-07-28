using com.PixelismGames.CSLibretro.Libretro;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace com.PixelismGames.CSLibretro
{
    // TODO : figure out if I can find the PC, ROM, and whether I can write to it or not
    public class Core
    {
        private APIVersionSignature _apiVersion;
        private GetMemoryDataSignature _getMemoryData;
        private GetMemorySizeSignature _getMemorySize;
        private GetSystemAVInfoSignature _getSystemAVInfo;
        private GetSystemInfoSignature _getSystemInfo;
        private InitSignature _init;
        private LoadGameSignature _loadGame;
        private RunSignature _run;
        private SerializeSignature _serialize;
        private SerializeSizeSignature _serializeSize;
        private SetAudioSampleSignature _setAudioSample;
        private SetAudioSampleBatchSignature _setAudioSampleBatch;
        private SetEnvironmentSignature _setEnvironment;
        private SetInputPollSignature _setInputPoll;
        private SetInputStateSignature _setInputState;
        private SetVideoRefreshSignature _setVideoRefresh;
        private UnserializeSignature _unserialize;

        private AudioSampleHandler _audioSampleHandler;
        private AudioSampleBatchHandler _audioSampleBatchHandler;
        private EnvironmentHandler _environmentHandler;
        private InputPollHandler _inputPollHandler;
        private InputStateHandler _inputStateHandler;
        private VideoRefreshHandler _videoRefreshHandler;

        private string _libretroDLLPath;
        private IntPtr _libretroDLL;

        private Stopwatch _frameTimer;
        private long _framePeriodNanoseconds;
        private long _frameLeftoverNanoseconds;

        private SystemInfo _systemInfo;
        private SystemAVInfo _systemAVInfo;

        private IntPtr _ramAddress;
        private int _ramSize;

        public bool IsRunning;
        public long FrameCount = 0;

        public PixelFormat PixelFormat = PixelFormat.Unknown;

        public event Action<short, short> AudioSampleHandler;
        public event Action AudioSampleBatchHandler;
        public event Action InputPollHandler;
        public event Action InputStateHandler;
        public event Action<LogLevel, string> LogHandler;
        public event Action<int, int, byte[]> VideoFrameHandler;

        public AudioSampleBatchHandler AudioSampleBatchPassthroughHandler;
        public InputStateHandler InputStatePassthroughHandler;
        public LogHandler LogPassthroughHandler;
        public VideoRefreshHandler VideoFramePassthroughHandler;

        #region Properties

        public int APIVersion
        {
            get { return ((int)_apiVersion()); }
        }

        public double AudioSampleRate
        {
            get { return (_systemAVInfo.Timing.SampleRate); }
        }

        public double FramePeriodNanoseconds
        {
            get { return (_framePeriodNanoseconds); }
        }

        public double FrameRate
        {
            get { return (_systemAVInfo.Timing.FPS); }
        }

        public string Name
        {
            get { return (_systemInfo.LibraryName); }
        }

        public string ValidExtensions
        {
            get { return (_systemInfo.ValidExtensions); }
        }

        public string Version
        {
            get { return (_systemInfo.LibraryVersion); }
        }

        #endregion

        public Core(string libretroDLLPath)
        {
            _frameTimer = new Stopwatch();

            _libretroDLLPath = libretroDLLPath;
            _libretroDLL = Win32API.LoadLibrary(libretroDLLPath);

            _apiVersion = GetDelegate<APIVersionSignature>("retro_api_version");
            _getMemoryData = GetDelegate<GetMemoryDataSignature>("retro_get_memory_data");
            _getMemorySize = GetDelegate<GetMemorySizeSignature>("retro_get_memory_size");
            _getSystemAVInfo = GetDelegate<GetSystemAVInfoSignature>("retro_get_system_av_info");
            _getSystemInfo = GetDelegate<GetSystemInfoSignature>("retro_get_system_info");
            _init = GetDelegate<InitSignature>("retro_init");
            _loadGame = GetDelegate<LoadGameSignature>("retro_load_game");
            _run = GetDelegate<RunSignature>("retro_run");
            _serialize = GetDelegate<SerializeSignature>("retro_serialize");
            _serializeSize = GetDelegate<SerializeSizeSignature>("retro_serialize_size");
            _setAudioSample = GetDelegate<SetAudioSampleSignature>("retro_set_audio_sample");
            _setAudioSampleBatch = GetDelegate<SetAudioSampleBatchSignature>("retro_set_audio_sample_batch");
            _setEnvironment = GetDelegate<SetEnvironmentSignature>("retro_set_environment");
            _setInputPoll = GetDelegate<SetInputPollSignature>("retro_set_input_poll");
            _setInputState = GetDelegate<SetInputStateSignature>("retro_set_input_state");
            _setVideoRefresh = GetDelegate<SetVideoRefreshSignature>("retro_set_video_refresh");
            _unserialize = GetDelegate<UnserializeSignature>("retro_unserialize");
        }

        #region Load

        public void Load(string romPath)
        {
            if (AudioSampleBatchPassthroughHandler == null)
                _audioSampleBatchHandler = new AudioSampleBatchHandler(audioSampleBatchCallback);
            else
                _audioSampleBatchHandler = AudioSampleBatchPassthroughHandler;

            if (InputStatePassthroughHandler == null)
                _inputStateHandler = new InputStateHandler(inputStateCallback);
            else
                _inputStateHandler = InputStatePassthroughHandler;

            if (VideoFramePassthroughHandler == null)
                _videoRefreshHandler = new VideoRefreshHandler(videoRefreshCallback);
            else
                _videoRefreshHandler = VideoFramePassthroughHandler;

            _audioSampleHandler = new AudioSampleHandler(audioSampleCallback);
            _environmentHandler = new EnvironmentHandler(environmentCallback); // options for passing through some commands need to be done here
            _inputPollHandler = new InputPollHandler(inputPollCallback);

            _setEnvironment(_environmentHandler);
            _setVideoRefresh(_videoRefreshHandler);
            _setAudioSample(_audioSampleHandler);
            _setAudioSampleBatch(_audioSampleBatchHandler);
            _setInputPoll(_inputPollHandler);
            _setInputState(_inputStateHandler);

            _init();

            GameInfo gameInfo = new GameInfo() { Path = romPath, Data = IntPtr.Zero, Size = 0, Meta = null };
            _loadGame(ref gameInfo);

            _systemInfo = new SystemInfo();
            _getSystemInfo(out _systemInfo);
            _systemInfo.LibraryName = Marshal.PtrToStringAnsi(_systemInfo.LibraryNameAddress);
            _systemInfo.LibraryVersion = Marshal.PtrToStringAnsi(_systemInfo.LibraryVersionAddress);
            _systemInfo.ValidExtensions = Marshal.PtrToStringAnsi(_systemInfo.ValidExtensionsAddress);

            _systemAVInfo = new SystemAVInfo();
            _getSystemAVInfo(out _systemAVInfo);

            _framePeriodNanoseconds = (long)(1000000000 / _systemAVInfo.Timing.FPS);

            _ramAddress = _getMemoryData(MemoryType.RAM);
            _ramSize = (int)_getMemorySize(MemoryType.RAM);
        }

        #endregion

        #region Run

        public void Run()
        {
            IsRunning = true;

            while (IsRunning)
            {
                StartFrameTiming();

                _run();

                FrameCount++;

                StopFrameTiming();
                SleepRemainingFrameTime();
            }
        }

        public void RunFrame()
        {
            _run();
        }

        #endregion

        #region Frame Timing

        public void StartFrameTiming()
        {
            _frameTimer.Reset();
            _frameTimer.Start();
        }

        public void StopFrameTiming()
        {
            _frameTimer.Stop();
        }

        public void SleepRemainingFrameTime()
        {
            long frameElapsedNanoseconds = (long)(((double)_frameTimer.ElapsedTicks / (double)Stopwatch.Frequency) * 1000000000);
            _frameLeftoverNanoseconds += _framePeriodNanoseconds - frameElapsedNanoseconds;

            if (_frameLeftoverNanoseconds > 0)
            {
                Thread.Sleep((int)(_frameLeftoverNanoseconds / 1000000));
                _frameLeftoverNanoseconds %= 1000000;
            }
            else
            {
                _frameLeftoverNanoseconds = 0;
            }
        }

        #endregion

        #region Memory

        public byte[] ReadRAM(int offset = 0, int? length = null)
        {
            if (!length.HasValue)
                length = _ramSize - offset;

            byte[] ram = new byte[length.Value];

            IntPtr ramAddressOffset = (IntPtr)((long)_ramAddress + offset);
            Marshal.Copy(ramAddressOffset, ram, 0, length.Value);

            return (ram);
        }

        public void WriteRAM(byte[] data, int offset = 0)
        {
            IntPtr ramAddressOffset = (IntPtr)((long)_ramAddress + offset);
            Marshal.Copy(data, 0, ramAddressOffset, data.Length);
        }

        #endregion

        #region Handlers

        private void audioSampleCallback(short left, short right)
        {
            if (AudioSampleHandler != null)
                AudioSampleHandler(left, right);
        }

        private uint audioSampleBatchCallback(IntPtr data, uint frames)
        {
            uint returnValue = 0;

            if (AudioSampleBatchHandler != null)
            {
                returnValue = 0; // do stuff here presumably

                AudioSampleBatchHandler();
            }

            return (returnValue);
        }

        // build a way for the user of core to passthrough their choice of commands to handle
        private bool environmentCallback(uint command, IntPtr data)
        {
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
                    if (LogPassthroughHandler == null)
                        logCallbackStruct.Log = logCallback;
                    else
                        logCallbackStruct.Log = LogPassthroughHandler;
                    Marshal.StructureToPtr(logCallbackStruct, data, false);
                    return (true);

                default:
                    return (false);
            }
        }

        private void inputPollCallback()
        {
            if (InputPollHandler != null)
                InputPollHandler();
        }

        private short inputStateCallback(uint port, uint device, uint index, uint id)
        {
            short returnValue = 0;

            if (InputStateHandler != null)
            {
                returnValue = 0; // do stuff here presumably

                InputStateHandler();
            }

            return (returnValue);
        }

        private void logCallback(LogLevel level, string fmt, params IntPtr[] arguments)
        {
            if (LogHandler != null)
            {
                StringBuilder logMessage = new StringBuilder(256);

                do
                {
                    int length = Win32API._snprintf(logMessage, (uint)logMessage.Capacity, fmt, arguments);

                    if ((length <= 0) || (length >= logMessage.Capacity))
                    {
                        logMessage.Capacity *= 2;
                        continue;
                    }

                    logMessage.Length = length;
                    break;
                } while (logMessage.Length >= logMessage.Capacity);

                LogHandler(level, logMessage.ToString());
            }
        }

        private void videoRefreshCallback(IntPtr data, uint width, uint height, uint pitch)
        {
            if (VideoFrameHandler != null)
            {
                int rowSize = (int)width * sizeof(short); // this will be different depending on pixel format
                int size = (int)height * rowSize;
                byte[] frameBuffer = new byte[size];

                if (rowSize == (int)pitch)
                {
                    Marshal.Copy(data, frameBuffer, 0, size);
                }
                else
                {
                    // if the data also contains the back buffer, we have to rip out just the first frame

                    for (int i = 0; i < height; i++)
                    {
                        IntPtr rowAddress = (IntPtr)((long)data + (i * (int)pitch));
                        int newRowIndex = i * rowSize;
                        Marshal.Copy(rowAddress, frameBuffer, newRowIndex, rowSize);
                    }
                }

                VideoFrameHandler((int)width, (int)height, frameBuffer);
            }
        }

        #endregion

        #region Delegates

        public T GetDelegate<T>(string libretroFunctionName)
        {
            return ((T)Convert.ChangeType(Marshal.GetDelegateForFunctionPointer(Win32API.GetProcAddress(_libretroDLL, libretroFunctionName), typeof(T)), typeof(T)));
        }

        #endregion
    }
}
