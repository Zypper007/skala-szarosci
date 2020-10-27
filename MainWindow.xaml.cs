using Microsoft.Win32;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Drawing;
using System.Diagnostics;
using System.Drawing.Imaging;

namespace skala_szarosci
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Bitmap bitmap;

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
        }

        private void BrowserBtn_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog OpenDialog = new OpenFileDialog();
            OpenDialog.InitialDirectory = Directory.GetCurrentDirectory();
            OpenDialog.Filter = "obrazki|*.png";
            OpenDialog.CheckFileExists = true;

            if (OpenDialog.ShowDialog() == true)
            {
                OrginalImage.Source = null;
                ImageMethod1.Source = null;
                ImageMethod2.Source = null;

                FilePathLabel.Content = OpenDialog.FileName;
                ActionBtn.Content = "Szarzej!";

                Task.Run(async () => 
                {
                    bitmap = new Bitmap(OpenDialog.FileName);
                    var img = await ToBitmapImage(bitmap);

                    Dispatcher.Invoke(() =>
                    {
                        OrginalImage.Source = img;
                        ActionBtn.IsEnabled = true;
                    });
                });
            }
            else
                ActionBtn.IsEnabled = false;
        }

        private void ActionBtn_Click(object sender, RoutedEventArgs e)
        {
            (sender as Button).IsEnabled = false;
            Task.Run(async () => 
            {
                Stopwatch timer = new Stopwatch();
                
                timer.Start();
                var img1 = await ToBitmapImage(await GrayScaleMethod1(bitmap));
                var img2 = await ToBitmapImage(await GrayScaleMethod2(bitmap));
                timer.Stop();

                Dispatcher.Invoke(() => 
                {
                    ImageMethod2.Source = img1;
                    ImageMethod1.Source = img2;
                    (sender as Button).Content = timer.ElapsedMilliseconds.ToString() + " ms";
                });
            });
        }

        private async Task<Bitmap> GrayScaleMethod1(Bitmap source)
        {
            var bitmap = new Bitmap(source.Width, source.Height);

            for (int x = 0; x < source.Width; x++)
                for (int y = 0; y < source.Height; y++)
                {
                    var colorPixel = source.GetPixel(x, y);
                    byte gray = (byte)((colorPixel.R + colorPixel.G + colorPixel.B) / 3);
                    bitmap.SetPixel(x, y, System.Drawing.Color.FromArgb(colorPixel.A, gray, gray, gray));
                }
            return bitmap;
        }

        private async Task<Bitmap> GrayScaleMethod2(Bitmap source)
        {
            var bitmap = new Bitmap(source.Width, source.Height);

            for (int x = 0; x < source.Width; x++)
                for (int y = 0; y < source.Height; y++)
                {
                    var colorPixel = source.GetPixel(x, y);
                    byte R = (byte)(colorPixel.R * 0.299);
                    byte G = (byte)(colorPixel.G * 0.587);
                    byte B = (byte)(colorPixel.G * 0.114);
                    byte gray = (byte)(R + G + B);
                    bitmap.SetPixel(x, y, System.Drawing.Color.FromArgb(colorPixel.A, gray, gray, gray));
                }
            return bitmap;
        }

        private async Task<BitmapImage> ToBitmapImage(Bitmap bitmap)
        {
            using (var memory = new MemoryStream())
            {
                bitmap.Save(memory, ImageFormat.Png);
                memory.Position = 0;

                var bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memory;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
                bitmapImage.Freeze();

                return bitmapImage;
            }
        }
    }
}
