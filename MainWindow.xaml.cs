using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace CSLibretro
{
    public partial class MainWindow : Window
    {
        private Wrapper _csLibretroWrapper;

        public MainWindow()
        {
            InitializeComponent();

            _csLibretroWrapper = new Wrapper(SetScreen, GetInputs);
            Task task = Task.Run(new Action(() => { _csLibretroWrapper.Run(); }));
        }

        public void SetScreen(Bitmap bitmap)
        {
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    bitmap.Save(memoryStream, ImageFormat.Png);
                    memoryStream.Position = 0;

                    BitmapImage bitmapImage = new BitmapImage();
                    bitmapImage.BeginInit();
                    bitmapImage.StreamSource = memoryStream;
                    bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                    bitmapImage.EndInit();

                    _screen.Source = bitmapImage;
                }
            }));
        }

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
