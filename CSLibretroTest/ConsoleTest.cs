using com.PixelismGames.CSLibretro;
using com.PixelismGames.CSLibretro.Libretro;
using System;

namespace CSLibretroTest
{
    public class ConsoleTest
    {
        //private const string DLL_NAME = "snes9x_libretro.dll";
        private const string DLL_NAME = "fceumm_libretro.dll";
        //private const string DLL_NAME = "gambatte_libretro.dll";

        //private const string ROM_NAME = "smw.sfc";
        private const string ROM_NAME = "smb.nes";
        //private const string ROM_NAME = "sml.gb";

        private static Core _core;

        public static void Main(string[] args)
        {
            _core = new Core(DLL_NAME);

            //_core.LogPassthroughHandler = logHandlerRaw;
            _core.LogHandler += logHandler;
            //_core.VideoFramePassthroughHandler = videoFrameHandlerRaw;
            _core.VideoFrameHandler += videoFrameHandler;

            _core.Load(ROM_NAME);

            _core.Run();
        }

        private static void logHandlerRaw(LogLevel level, string formatString, params IntPtr[] arguments)
        {
        }

        private static void logHandler(LogLevel level, string message)
        {
        }

        private static void videoFrameHandlerRaw(IntPtr data, uint width, uint height, uint stride)
        {
        }

        private static void videoFrameHandler(int width, int height, byte[] frameBuffer)
        {
        }
    }
}
