using libretro;
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
        public MainWindow()
        {
            InitializeComponent();

            Task task = Task.Run((Action)runProgram);

            //using (var memory = new MemoryStream())
            //{
            //    bitmap.Save(memory, ImageFormat.Png);
            //    memory.Position = 0;

            //    var bitmapImage = new BitmapImage();
            //    bitmapImage.BeginInit();
            //    bitmapImage.StreamSource = memory;
            //    bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
            //    bitmapImage.EndInit();

            //    return bitmapImage;
            //}

            //BitmapImage bmpi = new BitmapImage(); bmpi.BeginInit(); bmpi.StreamSource = new MemoryStream(ByteArray); bmpi.EndInit(); image1.Source = bmpi;

            //_screen.Source = new BitmapImage()
        }

        private void runProgram()
        {
            Program.MainBak(this);
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
