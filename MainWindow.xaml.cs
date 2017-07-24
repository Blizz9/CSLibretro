using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace CSLibretro
{
    public partial class MainWindow : Window
    {
        private Wrapper _csLibretroWrapper;

        public MainWindow()
        {
            InitializeComponent();

            _csLibretroWrapper = new Wrapper();
            Task task = Task.Run(new Action(() => { _csLibretroWrapper.Run(); }));
        }

        public void SetScreen(Bitmap bitmap)
        {
            Application.Current.Dispatcher.Invoke(new Action(() => { setScreen(bitmap); }));
        }

        private void setScreen(Bitmap bitmap)
        {
            using (MemoryStream memoryStream = new MemoryStream())
            {
                bitmap.Save(memoryStream, ImageFormat.Bmp);
                memoryStream.Position = 0;

                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memoryStream;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();

                _screen.Source = bitmapImage;
            }
        }
    }
}
