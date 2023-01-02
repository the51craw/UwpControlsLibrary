using System;
using UwpControlsLibrary;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Input;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;

namespace ListViewAndStackPanelExample
{
    public sealed partial class MainPage : Page
    {
        // This enum must contain one entry for each control!
        // You can use it in handlers to identify what action to take when control is used.
        public enum _oscillatorControlId
        {
            MODULATION,
            FREQUENCY,
            FINE,
            VOLUME,
            WAVE,
            KEYBOARD,
            ADSR_PULSE,
            WIEW,
            MODULATION_SELECTOR,
            SOUNDING,
        }

        private enum _controlId
        {
            SAVE,
            LOAD,
            SELECT_LAYOUT,
            MANUAL,
        }
        private enum _type
        {
            OSCILLATOR,
            FILTER,
            ADSR,
            DISPLAY,
        }
        Int32 layout = 0;

        Display display;

        public MainPage()
        {
            this.InitializeComponent();
            Init();
        }

        private void Init()
        {
            // Fix the title bar:
            CoreApplicationViewTitleBar coreTitleBar = CoreApplication.GetCurrentView().TitleBar;
            coreTitleBar.ExtendViewIntoTitleBar = true;
            ApplicationViewTitleBar titleBar = ApplicationView.GetForCurrentView().TitleBar;
            titleBar.ButtonBackgroundColor = Color.FromArgb(0, 128, 128, 128);
            titleBar.ButtonForegroundColor = Color.FromArgb(0, 128, 128, 128);

            color = new SolidColorBrush(Windows.UI.Colors.Chartreuse);
        }

        private void imgClickArea_ImageOpened(object sender, RoutedEventArgs e)
        {
            // Create the controls object:
            //Controls = new Controls(Window.Current.Bounds, imgClickArea);
            Controls = new Controls(new Rect(0, 0, imgBackgroundUpper.ActualWidth, imgBackgroundUpper.ActualHeight), imgClickArea);
            FixedControls = new Controls(new Rect(0, 0, imgBackgroundLower.ActualWidth, imgBackgroundLower.ActualHeight), imgClickArea);
            CreateControls();
            //CreateFixedControls();
            gridMain_SizeChanged(null, null);

            // Test the display:
            ((DigitalDisplay)displays[0].SubControls.ControlsList[2]).DisplayValue(12345.67);
            ((DigitalDisplay)displays[1].SubControls.ControlsList[2]).DisplayValue(440f);
        }

        private void CreateFixedControls()
        {
            Controls.AddRotator((Int32)_controlId.SELECT_LAYOUT, gridControls,
              new Image[] { imgLayout4, imgLayout6, imgLayout8, imgLayout12 }, new Point(1658, 921));          //Controls.OnTapped.Tapped += Controls_Tapped;
        }

        // When app size is changed, all controls must also be resized,
        // ask the Controls object to do it:
        private void gridMain_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (Controls != null)
            {
                //Controls.ResizeControls(gridMain, Window.Current.Bounds);
                Controls.ResizeControls(gridControls, new Rect(0, 0, imgBackgroundUpper.ActualWidth, imgBackgroundUpper.ActualHeight));
            }
        }

        private void GetAdjustedMousePosition(PointerRoutedEventArgs e)
        {
            PointerPoint MousePositionInWindow = e.GetCurrentPoint(this);
            PointerPosition = new Point(MousePositionInWindow.Position.X - ExtraMarginLeft,
                MousePositionInWindow.Position.Y - ExtraMarginTop);
        }
    }
}
