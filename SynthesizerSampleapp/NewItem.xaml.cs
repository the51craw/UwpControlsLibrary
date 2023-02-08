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
    public enum MenuItemType
    {
        FOLDER,
        PATCH,
    }

    public sealed partial class NewItem : ContentDialog
    {
        public string Text;
        public bool Ok;

        public enum Type
        {
            FOLDER,
            PATCH,
        }

        public NewItem(Type type)
        {
            this.InitializeComponent();
            if (type == Type.FOLDER)
            {
                Title = "Type a new folder name";
            }
            if (type == Type.PATCH)
            {
                Title = "Type a new patch name";
            }
        }

        private void tbInput_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                Text = tbInput.Text;
                Ok = true;
                Hide();
            }
            else if (e.Key == Windows.System.VirtualKey.Escape)
            {
                Text = string.Empty;
                Ok = false;
                Hide();
            }
        }
    }
}
