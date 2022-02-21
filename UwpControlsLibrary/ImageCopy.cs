using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace UwpControlsLibrary
{
    public class ImageCopy
    {
        public Image Image { get; set; }
        public Double Width { get; set; }
        public Double Height { get; set; }

        public ImageCopy(Image from)
        {
            this.Image = new Image();
            //string uri = from.BaseUri.AbsoluteUri;
            //BitmapImage bmiImage = new BitmapImage(new Uri(uri));
            //this.Image.Source = bmiImage;

            this.Image.Source = from.Source;

            this.Image.Stretch = Stretch.None;
            this.Image.Visibility = Visibility.Visible;
            this.Image.Tag = this;
            Width = from.ActualWidth;
            Height = from.ActualHeight;
        }
    }
}
