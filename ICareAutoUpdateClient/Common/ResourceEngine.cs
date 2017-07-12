using System;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ICareAutoUpdateClient.Common
{
    public static class ResourceEngine
    {
        public static ImageSource GetImageSource( string imageName)
        {
            return new BitmapImage(new Uri(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"Image\\{imageName}")));
        }
    }
}