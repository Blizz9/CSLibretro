using com.PixelismGames.CSLibretro;
using com.PixelismGames.CSLibretro.Libretro;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace CSLibretro
{
    public partial class MainWindow : Window
    {
        //private const string DLL_NAME = "snes9x_libretro.dll";
        private const string DLL_NAME = "fceumm_libretro.dll";
        //private const string DLL_NAME = "gambatte_libretro.dll";

        //private const string ROM_NAME = "smw.sfc";
        private const string ROM_NAME = "smb.nes";
        //private const string ROM_NAME = "sml.gb";

        private Core _core;

        public MainWindow()
        {
            InitializeComponent();

            _core = new Core(DLL_NAME);

            //_core.LogPassthroughHandler = logHandlerRaw;
            _core.LogHandler += logHandler;
            //_core.VideoFramePassthroughHandler = videoFrameHandlerRaw;
            _core.VideoFrameHandler += videoFrameHandler;

            _core.Load(ROM_NAME);

            Task task = Task.Run(new Action(() => { _core.Run(); }));
        }

        private void logHandlerRaw(LogLevel level, string formatString, params IntPtr[] arguments)
        {
        }

        private void logHandler(LogLevel level, string message)
        {
            Debug.WriteLine(string.Format("[{0}] {1}", (int)level, message));
        }

        private void videoFrameHandlerRaw(IntPtr data, uint width, uint height, uint stride)
        {
        }

        private void videoFrameHandler(int width, int height, byte[] frameBuffer)
        {
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                _screen.Source = BitmapSource.Create(width, height, 300, 300, PixelFormats.Bgr565, BitmapPalettes.Gray256, frameBuffer, (width * 2));
            }));
        }

        //public void SetScreen(Bitmap bitmap)
        //{
        //    Application.Current.Dispatcher.Invoke(new Action(() =>
        //    {
        //        using (MemoryStream memoryStream = new MemoryStream())
        //        {
        //            bitmap.Save(memoryStream, ImageFormat.Png);
        //            memoryStream.Position = 0;

        //            BitmapImage bitmapImage = new BitmapImage();
        //            bitmapImage.BeginInit();
        //            bitmapImage.StreamSource = memoryStream;
        //            bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
        //            bitmapImage.EndInit();

        //            _screen.Source = bitmapImage;
        //        }
        //    }));
        //}

        //public void GetInputs(List<Tuple<Key, int, bool>> inputs)
        //{
        //    Application.Current.Dispatcher.Invoke(new Action(() =>
        //    {
        //        for (int i = (inputs.Count - 1); i >= 0; i--)
        //        {
        //            if (Keyboard.IsKeyDown(inputs[i].Item1))
        //            {
        //                inputs.Add(new Tuple<Key, int, bool>(inputs[i].Item1, inputs[i].Item2, true));
        //                inputs.RemoveAt(i);
        //            }
        //        }
        //    }));
        //}
    }
}
