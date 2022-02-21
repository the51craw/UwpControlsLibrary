using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace MakeImageList
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        String template;
        String imageTemplate;
        String images;

        public MainPage()
        {
            this.InitializeComponent();
            Init();
        }

        private async void Init()
        {
            template = "\t<Grid x:Name = \"gridMain\" >\n" +
                "<images>" +
                "\t\t\t<Image x:Name = \"imgBackground\" Source = \"ms-appx:///Images/Background.png\" Stretch = \"Uniform\" />\n" +
                "\t\t<Grid x:Name = \"gridControls\" SizeChanged = \"gridMain_SizeChanged\" />\n" +
                "\t\t\t<Image x:Name = \"imgClickArea\" Source = \"ms-appx:///Images/Background.png\" Stretch = \"None\" Opacity = \"0\"\n" +
                "\t\t\t\tPointerMoved = \"imgClickArea_PointerMoved\"\n" +
                "\t\t\t\tImageOpened = \"imgClickArea_ImageOpened\"\n" +
                "\t\t\t\tPointerWheelChanged = \"imgClickArea_PointerWheelChanged\"\n" +
                "\t\t\t\tPointerReleased = \"imgClickArea_PointerReleased\"\n" +
                "\t\t\t\tPointerPressed = \"imgClickArea_PointerPressed\"\n" +
                "\t\t\t\tTapped = \"imgClickArea_Tapped\" />\n" + 
                "\t\t</Grid >";

            imageTemplate = "\t\t<Image x:Name = \"img<displayName>\" Source = \"ms-appx:///Images/<displayName>.png\" Stretch = \"None\" />\n";

            images = "";

            await GetFiles();

            DataPackage dataPackage = new DataPackage();
            dataPackage.SetText(template.Replace("<images>", images));
            Clipboard.SetContent(dataPackage);
        }

        private async Task GetFiles()
        {
            try
            {
                FolderPicker folderPicker = new FolderPicker();
                folderPicker.FileTypeFilter.Add(".png");
                StorageFolder folder = await folderPicker.PickSingleFolderAsync();

                if (folder != null)
                {
                    //await folder.GetFilesAsync();
                    IReadOnlyList<StorageFile> files = await folder.GetFilesAsync();
                    foreach (StorageFile file in files)
                    {
                        if (file.DisplayName != "Background")
                        images += imageTemplate.Replace("<displayName>", file.DisplayName);
                    }
                }
            }
            catch (Exception e) { }
        }
    }
}
