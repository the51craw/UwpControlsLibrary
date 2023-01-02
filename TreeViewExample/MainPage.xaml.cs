using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using UwpControlsLibrary;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;


namespace TreeViewExample
{
    public sealed partial class MainPage : Page
    {
        // The Controls object:
        Controls Controls;

        TreeViewBase TreeView;

        // PointerMoved will update this in order to let other handlers know which control is handled.
        private Int32 currentControl;

        public Brush Unselected = new SolidColorBrush(Color.FromArgb(0, 255, 255, 255));
        public Brush Playing = new SolidColorBrush(Color.FromArgb(255, 0, 255, 255));
        public Brush Selected = new SolidColorBrush(Color.FromArgb(255, 255, 255, 0));

        public Point PointerPosition;
        public bool initDone;
        private bool mainPageLoaded = false;

        public MainPage()
        {
            this.InitializeComponent();

            // To cover title bar with an image, set ExtendViewIntoTitleBar to true:
            CoreApplicationViewTitleBar coreTitleBar = CoreApplication.GetCurrentView().TitleBar;
            coreTitleBar.ExtendViewIntoTitleBar = true;

            // Set colors for titlebar to be transparent:
            ApplicationViewTitleBar titleBar = ApplicationView.GetForCurrentView().TitleBar;
            titleBar.ButtonBackgroundColor = Color.FromArgb(0, 128, 128, 128);
            titleBar.ButtonForegroundColor = Color.FromArgb(0, 128, 128, 128);

            // Display wait cursor while initiating things:
            Window.Current.CoreWindow.PointerCursor = new Windows.UI.Core.CoreCursor(Windows.UI.Core.CoreCursorType.Wait, 0);

            // Hook up keyboard entries:
            Window.Current.CoreWindow.CharacterReceived += CoreWindow_CharacterReceived;
            Window.Current.CoreWindow.KeyDown += CoreWindow_KeyDown;
            // Hook up the event for page loaded in order to not do some initiations before that:
            this.Loaded += MainPage_Loaded;
        }

        private void Init()
        {
            // Create the controls object:
            Controls = new Controls(Window.Current.Bounds, imgClickArea);
            Controls.Init(gridControls);

            // Create all controls:
            Int32 i = 0;
            TreeView = Controls.AddTreeView(i++, gridControls, new Image[] { imgBackground, imgFolder, imgRun }, new Point(20, 20));

            // Make sure all controls has the correct size and position:
            Controls.ResizeControls(gridControls, Window.Current.Bounds);
            Controls.SetControlsUniform(gridControls);
            UpdateLayout();
        }

        private void imgClickArea_ImageOpened(object sender, RoutedEventArgs e)
        {
            Init();
        }

        private void imgClickArea_PointerMoved(object sender, PointerRoutedEventArgs e)
        {

        }

        private void imgClickArea_PointerPressed(object sender, PointerRoutedEventArgs e)
        {

        }

        private void imgClickArea_PointerReleased(object sender, PointerRoutedEventArgs e)
        {

        }

        private void imgClickArea_Tapped(object sender, TappedRoutedEventArgs e)
        {

        }

        private void imgClickArea_PointerWheelChanged(object sender, PointerRoutedEventArgs e)
        {

        }

        private void CoreWindow_KeyDown(CoreWindow sender, KeyEventArgs args)
        {
        }

        private void CoreWindow_CharacterReceived(CoreWindow sender, CharacterReceivedEventArgs args)
        {
        }

        private void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
        }

        private void gridControls_SizeChanged(object sender, SizeChangedEventArgs e)
        {

        }
    }
}
