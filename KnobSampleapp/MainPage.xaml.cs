using System;
using UwpControlsLibrary;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace KnobSampleapp
{
    public sealed partial class MainPage : Page
    {
        // The Controls object:
        Controls Controls;

        // This enum must contain one entry for each control!
        // You can use it in handlers to identify what action to take when control is used.
        public enum ControlId
        {
            STATICIMAGE,        // 0 
            KNOB,               // 1
            LABEL,              // 2
        }

        public Double WidthSizeRatio = 1;
        public Double HeightSizeRatio = 1;
        private bool initDone = false;

        public MainPage()
        {
            this.InitializeComponent();
        }

        // When imgClickArea is opened it has also got its size, so now
        // we can create the controls object:
        private void imgClickArea_ImageOpened(object sender, RoutedEventArgs e)
        {
            // Create the controls object:
            Controls = new Controls(Window.Current.Bounds, imgClickArea);
            Controls.Init(gridControls);

            // Create all controls:
            Controls.AddStaticImage((int)ControlId.STATICIMAGE, gridControls, new Image[] { imgMrMartin }, new Point(10, 10));
            Controls.AddKnob((int)ControlId.KNOB, gridControls, new Image[] { imgArrowknob_75 }, new Point(180, 80));
            Controls.AddLabel((int)ControlId.LABEL, gridControls, new Rect(142, 130, 80, 30), "", 20,
                TextAlignment.Center, ControlBase.ControlTextWeight.BOLD);


            // Make sure all controls has the correct size and position:
            Controls.ResizeControls(gridControls, Window.Current.Bounds);
            Controls.SetControlsUniform(gridControls);
            UpdateLayout();
            initDone = true;
        }

        private void UpdateLabel()
        {
            if (((Knob)Controls.ControlsList[(int)ControlId.KNOB]).IsSelected)
            {
                ((Label)Controls.ControlsList[(int)ControlId.LABEL]).Text =
                    ((Knob)Controls.ControlsList[(int)ControlId.KNOB]).Value.ToString();
            }
        }

        // When the pointer is moved over the click-area, ask the Controls
        // object if, and if so which control the pointer is over:
        private void imgClickArea_PointerMoved(object sender, PointerRoutedEventArgs e)
        { 
            if (initDone && Controls != null)
            {
                Controls.PointerMoved(sender, e);
                if (((Knob)Controls.ControlsList[(int)ControlId.KNOB]).IsSelected)
                {
                    UpdateLabel();
                }
            }
        }

        private void imgClickArea_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            if (initDone && Controls != null)
            {
                Controls.PointerPressed(sender, e);
                UpdateLabel();
            }
        }

        private void imgClickArea_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            if (initDone && Controls != null)
            {
                Controls.PointerReleased(sender, e);
            }
        }

        private void imgClickArea_PointerWheelChanged(object sender, PointerRoutedEventArgs e)
        {
            if (initDone && Controls != null)
            {
                Controls.PointerWheelChanged(sender, e);
                UpdateLabel();
            }
        }

        // Tapped event, handlers for controls that are supposed to be tapped:
        private void imgClickArea_Tapped(object sender, TappedRoutedEventArgs e)
        {
        }

        // Right tapped event, handlers for controls that are supposed to be tapped:
        private void imgClickArea_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
        }

        // When app size is changed, all controls must also be resized,
        // ask the Controls object to do it:
        private void gridControls_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (initDone && Controls != null)
            {
                Controls.ResizeControls(gridMain, Window.Current.Bounds);
            }
        }
    }
}
