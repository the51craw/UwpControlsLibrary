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

namespace GenerateCode
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        String codeTemplate;
        String xamlTemplate;
        String imageTemplate;
        String images;
        string midiObject;
        string midiNew;
        string midiEvents;
        bool imagesFetched;

        public MainPage()
        {
            this.InitializeComponent();
            Init();
        }

        private void Init()
        {
            imagesFetched = false;

            xamlTemplate = "\t<Grid x:Name = \"gridMain\" >\n" +
                "<images>" +
                "\t\t<Image x:Name = \"imgBackground\" Source = \"ms-appx:///Images/Background.png\" Stretch = \"Uniform\" />\n" +
                "\t\t<Grid x:Name = \"gridControls\" SizeChanged = \"gridControls_SizeChanged\" />\n" +
                "\t\t\t<Image x:Name = \"imgClickArea\" Source = \"ms-appx:///Images/Background.png\" Stretch = \"None\" Opacity = \"0\"\n" +
                "\t\t\t\tPointerMoved = \"imgClickArea_PointerMoved\"\n" +
                "\t\t\t\tImageOpened = \"imgClickArea_ImageOpened\"\n" +
                "\t\t\t\tPointerWheelChanged = \"imgClickArea_PointerWheelChanged\"\n" +
                "\t\t\t\tPointerReleased = \"imgClickArea_PointerReleased\"\n" +
                "\t\t\t\tPointerPressed = \"imgClickArea_PointerPressed\"\n" +
                "\t\t\t\tTapped = \"imgClickArea_Tapped\"\n" +
                "\t\t\t\tRightTapped = \"imgClickArea_RightTapped\" />\n" +
                "\t\t</Grid >";

            imageTemplate = "\t\t<Image x:Name = \"img<displayName>\" Source = \"ms-appx:///Images/<displayName>.png\" Stretch = \"None\" />\n";

            images = "";
        }

        private void GenerateMainCode()
        {
            if (AddMidi.IsChecked == true)
            {
                midiObject = "\tMIDI midi;\n\n"
                    + "\tprivate byte[] MidiInBuffer;\r\n";
                midiNew = "\t\tmidi = new MIDI(MidiInPort_MessageReceived);\n";
                midiEvents = "\n\t\tprivate void MidiInPort_MessageReceived(MidiInPort sender, MidiMessageReceivedEventArgs args)\n"
                    + "\t\t{\n"
                    + "\t\t\tMidiInBuffer = args.Message.RawData.ToArray();\n"
                    + "\n"
                    + "\t\t\t// See https://www.midi.org/specifications-old/item/table-1-summary-of-midi-message for a complete summary of messages.\n"
                    + "\t\t\tif (args.Message.Type == MidiMessageType.SystemExclusive)\n"
                    + "\t\t\t{\n"
                    + "\t\t\t}\n"
                    + "\t\t\telse if (args.Message.Type == MidiMessageType.ActiveSensing)\n"
                    + "\t\t\t{\n"
                    + "\t\t\t}\n"
                    + "\t\t\telse if (args.Message.Type == MidiMessageType.ChannelPressure)\n"
                    + "\t\t\t{\n"
                    + "\t\t\t}\n"
                    + "\t\t\telse if (args.Message.Type == MidiMessageType.Continue)\n"
                    + "\t\t\t{\n"
                    + "\t\t\t}\n"
                    + "\t\t\telse if (args.Message.Type == MidiMessageType.ControlChange)\n"
                    + "\t\t\t{\n"
                    + "\t\t\t}\n"
                    + "\t\t\telse if (args.Message.Type == MidiMessageType.EndSystemExclusive)\n"
                    + "\t\t\t{\n"
                    + "\t\t\t}\n"
                    + "\t\t\telse if (args.Message.Type == MidiMessageType.MidiTimeCode)\n"
                    + "\t\t\t{\n"
                    + "\t\t\t}\n"
                    + "\t\t\telse if (args.Message.Type == MidiMessageType.NoteOff)\n"
                    + "\t\t\t{\n"
                    + "\t\t\t}\n"
                    + "\t\t\telse if (args.Message.Type == MidiMessageType.NoteOn)\n"
                    + "\t\t\t{\n"
                    + "\t\t\t}\n"
                    + "\t\t\telse if (args.Message.Type == MidiMessageType.PitchBendChange)\n"
                    + "\t\t\t{\n"
                    + "\t\t\t}\n"
                    + "\t\t\telse if (args.Message.Type == MidiMessageType.PolyphonicKeyPressure)\n"
                    + "\t\t\t{\n"
                    + "\t\t\t}\n"
                    + "\t\t\telse if (args.Message.Type == MidiMessageType.ProgramChange)\n"
                    + "\t\t\t{\n"
                    + "\t\t\t}\n"
                    + "\t\t\telse if (args.Message.Type == MidiMessageType.SongPositionPointer)\n"
                    + "\t\t\t{\n"
                    + "\t\t\t}\n"
                    + "\t\t\telse if (args.Message.Type == MidiMessageType.SongSelect)\n"
                    + "\t\t\t{\n"
                    + "\t\t\t}\n"
                    + "\t\t\telse if (args.Message.Type == MidiMessageType.Start)\n"
                    + "\t\t\t{\n"
                    + "\t\t\t}\n"
                    + "\t\t\telse if (args.Message.Type == MidiMessageType.Stop)\n"
                    + "\t\t\t{\n"
                    + "\t\t\t}\n"
                    + "\t\t\telse if (args.Message.Type == MidiMessageType.SystemExclusive)\n"
                    + "\t\t\t{\n"
                    + "\t\t\t}\n"
                    + "\t\t\telse if (args.Message.Type == MidiMessageType.SystemReset)\n"
                    + "\t\t\t{\n"
                    + "\t\t\t}\n"
                    + "\t\t\telse if (args.Message.Type == MidiMessageType.TimingClock)\n"
                    + "\t\t\t{\n"
                    + "\t\t\t}\n"
                    + "\t\t\telse if (args.Message.Type == MidiMessageType.TuneRequest)\n"
                    + "\t\t\t{\n"
                    + "\t\t\t}\n"
                    + "\t\t\telse if (args.Message.Type == MidiMessageType.ProgramChange)\n"
                    + "\t\t\t{\n"
                    + "\t\t\t}\n"
                    + "\t\t\telse if (args.Message.Type == MidiMessageType.ControlChange)\n"
                    + "\t\t\t{\n"
                    + "\t\t\t\t// See https://www.midi.org/specifications-old/item/table-3-control-change-messages-data-bytes-2 for a complete summary of control change messages.\n"
                    + "\t\t\t}\n"
                    + "\t\t\telse if (args.Message.Type == MidiMessageType.PitchBendChange)\n"
                    + "\t\t\t{\n"
                    + "\t\t\t}\n"
                    + "\t\t}\n";
            }
            else
            {
                midiObject = "";
                midiNew = "";
                midiEvents = "";
            }

            codeTemplate = 
                  "\tpublic sealed partial class MainPage : Page\n"
                + "\t{\n"
                + "\t\t// The Controls object:\n"
                + "\t\tControls Controls;\n"
                + "\n"
                + "\t\t// This enum must contain one entry for each control!\n"
                + "\t\t// You can use it in handlers to identify what action to take when control is used.\n"
                + "\t\tpublic enum ControlId\n"
                + "\t\t{\n"
                + "\t\t\tSTATICIMAGE,\t\t// 0 \n"
                + "\t\t}\n"
                + "\n"
                + "\t\tpublic Double WidthSizeRatio = 1;\n"
                + "\t\tpublic Double HeightSizeRatio = 1;\n"
                + "\t\tprivate bool initDone = false;\n"
                + "\n"
                + midiObject
                + "\t\tpublic MainPage()\n"
                + "\t\t{\n"
                + "\t\t\tthis.InitializeComponent();\n"
                + midiNew
                + "\t\t}\n"
                + "\n"
                + "\t\t// When imgClickArea is opened it has also got its size, so now\n"
                + "\t\t// we can create the controls object:\n"
                + "\t\tprivate void imgClickArea_ImageOpened(object sender, RoutedEventArgs e)\n"
                + "\t\t{\n"
                + "\t\t\t// Create and initiate the controls object:\n"
                + "\t\t\tControls = new Controls(Window.Current.Bounds, imgClickArea);\n"
                + "\t\t\tControls.Init(gridControls);\n"
                + "\n"
                + "\t\t\t// Create all controls:\n"
                + "\t\t\tControls.AddStaticImage((int)ControlId.STATICIMAGE, gridControls, new Image[] { imgMrMartin }, new Point(10, 560));\n"
                + "\n"
                + "\n"
                + "\t\t\t// Make sure all controls has the correct size and position:\n"
                + "\t\t\tControls.ResizeControls(gridControls, Window.Current.Bounds);\n"
                + "\t\t\tControls.SetControlsUniform(gridControls);\n"
                + "\t\t\tUpdateLayout();\n"
                + "\t\t\tinitDone = true;\n"
                + "\t\t}\n"
                + "\n"
                + "\t\t// When the pointer is moved over the click-area, ask the Controls\n"
                + "\t\t// object if, and if so which control the pointer is over:\n"
                + "\t\tprivate void imgClickArea_PointerMoved(object sender, PointerRoutedEventArgs e)\n"
                + "\t\t{\n"
                + "\t\t\tif (initDone && Controls != null)\n"
                + "\t\t\t{\n"
                + "\t\t\t\tControls.PointerMoved(sender, e);\n"
                + "\t\t\t}\n"
                + "\t\t}\n"
                + "\n"
                + "\t\tprivate void imgClickArea_PointerPressed(object sender, PointerRoutedEventArgs e)\n"
                + "\t\t{\n"
                + "\t\t\tif (initDone && Controls != null)\n"
                + "\t\t\t{\n"
                + "\t\t\t\tControls.PointerPressed(sender, e);\n"
                + "\t\t\t}\n"
                + "\t\t}\n"
                + "\n"
                + "\t\tprivate void imgClickArea_PointerReleased(object sender, PointerRoutedEventArgs e)\n"
                + "\t\t{\n"
                + "\t\t\tControls.PointerReleased(sender, e);\n"
                + "\t\t}\n"
                + "\n"
                + "\t\tprivate void imgClickArea_PointerWheelChanged(object sender, PointerRoutedEventArgs e)\n"
                + "\t\t{\n"
                + "\t\t\tControls.PointerWheelChanged(sender, e);\n"
                + "\t\t}\n"
                + "\n"
                + "\t\t// Tapped event, handlers for controls that are supposed to be tapped:\n"
                + "\t\tprivate void imgClickArea_Tapped(object sender, TappedRoutedEventArgs e)\n"
                + "\t\t{\n"
                + "\t\t}\n"
                + "\n"
                + "\t\t// Right tapped event, handlers for controls that are supposed to be tapped:\n"
                + "\t\tprivate void imgClickArea_RightTapped(object sender, RightTappedRoutedEventArgs e)\n"
                + "\t\t{\n"
                + "\t\t}\n"
                + "\n"
                + "\t\t// When app size is changed, all controls must also be resized,\n"
                + "\t\t// ask the Controls object to do it:\n"
                + "\t\tprivate void gridControls_SizeChanged(object sender, SizeChangedEventArgs e)\n"
                + "\t\t{\n"
                + "\t\t\tif (initDone && Controls != null)\n"
                + "\t\t\t{\n"
                + "\t\t\t\tControls.ResizeControls(gridMain, Window.Current.Bounds);\n"
                + "\t\t\t}\n"
                + "\t\t}\n"
                + midiEvents
                + "\t}\n";
        }

        private async void SelectImageFolder_Tapped(object sender, TappedRoutedEventArgs e)
        {
            await GetFiles();

            if (imagesFetched)
            {
                GetXamlContent.IsEnabled = true;
            }
        }

        private void GetMainPageCode_Tapped(object sender, TappedRoutedEventArgs e)
        {
            GenerateMainCode();
            DataPackage dataPackage = new DataPackage();
            dataPackage.SetText(codeTemplate);
            Clipboard.SetContent(dataPackage);
            text.Text = "MainPage.xaml.cs content for class MainPage is now in clipboard.\nReplace the MainPage class from clipboard.";
        }

        private void GetXamlContent_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (imagesFetched)
            {
                DataPackage dataPackage = new DataPackage();
                dataPackage.SetText(xamlTemplate.Replace("<images>", images));
                Clipboard.SetContent(dataPackage);
                text.Text = "MainPage.xaml page content is now in clipboard.\nReplace the page content (normally a <Grid></Grid> tag) from clipboard.";
            }
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
                        if (file.FileType == ".png" && file.DisplayName != "Background" && file.DisplayName != "ClickArea")
                        images += imageTemplate.Replace("<displayName>", file.DisplayName);
                    }
                    imagesFetched = true;
                }
            }
            catch (Exception e) { }
        }
    }
}
