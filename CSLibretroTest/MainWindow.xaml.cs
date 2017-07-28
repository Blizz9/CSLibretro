using com.PixelismGames.CSLibretro;
using com.PixelismGames.CSLibretro.Libretro;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace CSLibretro
{
    public partial class MainWindow : Window
    {
        private Core _core;

        public MainWindow()
        {
            InitializeComponent();

            _core = new Core("snes9x_libretro.dll");
            //_core.VideoFramePassthroughHandler = videoFrameHandlerRaw;
            _core.VideoFrameHandler += videoFrameHandler;
            _core.Initialize("smw.sfc");

            Task task = Task.Run(new Action(() => { _core.Run(); }));
        }

        private void videoFrameHandlerRaw(IntPtr data, uint width, uint height, uint stride)
        {
        }

        private void videoFrameHandler(byte[] frameBuffer)
        {
        }

        private void logCallback(LogLevel level, string formatString, params IntPtr[] arguments)
        {
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

        public void GetInputs(List<Tuple<Key, int, bool>> inputs)
        {
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                for (int i = (inputs.Count - 1); i >= 0; i--)
                {
                    if (Keyboard.IsKeyDown(inputs[i].Item1))
                    {
                        inputs.Add(new Tuple<Key, int, bool>(inputs[i].Item1, inputs[i].Item2, true));
                        inputs.RemoveAt(i);
                    }
                }
            }));
        }
    }
}
