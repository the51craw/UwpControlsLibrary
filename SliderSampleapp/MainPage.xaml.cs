using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using UwpControlsLibrary;

namespace SliderSampleapp
{
    /// <summary>
    /// In this example I use control objects to access the controls.
    /// ControlId is really not used at all, but are still declared
    /// since it is possible to use both kinds of access mixed.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        // The Controls object:
        Controls Controls;

        // This enum must contain one entry for each control!
        // You can use it in handlers to identify what action to take when control is used.
        public enum ControlId
        {
            STATICIMAGE,        // 0 
            VERTICAL_SLIDER,  // 1
        }

        public VerticalSlider slider;
        public StaticImage logotype;

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
            // Create and initiate the controls object:
            Controls = new Controls(Window.Current.Bounds, imgClickArea);
            Controls.Init(gridControls);

            // Create all controls:
            logotype = Controls.AddStaticImage((int)ControlId.STATICIMAGE, gridControls, new Image[] { imgMrMartin }, new Point(10, 260));
            logotype.ImageList[0].Opacity = 0.0;
            slider = Controls.AddVerticalSlider((int)ControlId.VERTICAL_SLIDER, gridControls, new Image[] { imgSliderHandle }, new Rect(216, 24, 102, 365));

            // Make sure all controls has the correct size and position:
            Controls.ResizeControls(gridControls, Window.Current.Bounds);
            Controls.SetControlsUniform(gridControls);
            UpdateLayout();
            initDone = true;
        }

        private void UpdateLogotype()
        {
            if (slider.IsSelected)
            {
                logotype.ImageList[0].Opacity = (double)slider.Value / 127.0;
            }
        }

        // When the pointer is moved over the click-area, ask the Controls
        // object if, and if so which control the pointer is over:
        private void imgClickArea_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            if (initDone && Controls != null)
            {
                Controls.PointerMoved(sender, e);
                if (slider.IsSelected)
                {
                    UpdateLogotype();
                }
            }
        }

        private void imgClickArea_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            if (initDone && Controls != null)
            {
                Controls.PointerPressed(sender, e);
                if (slider.IsSelected)
                {
                    UpdateLogotype();
                }
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
                UpdateLogotype();
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
