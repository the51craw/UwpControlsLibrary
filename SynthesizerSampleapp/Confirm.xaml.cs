using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Content Dialog item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace SynthesizerSampleapp
{
    public sealed partial class Confirm : ContentDialog
    {
        public bool Ok;

        public Confirm(string title)
        {
            this.InitializeComponent();
            this.Title = title;
        }

        private void btnOk_Tapped(object sender, TappedRoutedEventArgs e)
        {
            Ok = true;
            Hide();
        }

        private void btnCancel_Tapped(object sender, TappedRoutedEventArgs e)
        {
            Ok = false;
            Hide();
        }
    }
}
