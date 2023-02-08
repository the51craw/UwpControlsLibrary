using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using UwpControlsLibrary;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace AddRemoveAddControls
{
    /// <summary>
    /// Removing and re-inserting a control involves more than
    /// just the control. UwpControlsLibrary adds objects to
    /// grids too, and those must also be removed and re-inserted.
    /// </summary>
	public sealed partial class MainPage : Page
    {
        // The Controls object:
        Controls Controls;

        public Double WidthSizeRatio = 1;
        public Double HeightSizeRatio = 1;
        private bool initDone = false;
        Indicator indicator;
        CompoundControl compoundControl1;
        CompoundControl compoundControl2;
        List<CompoundControl> compoundControls;
        DispatcherTimer timer = new DispatcherTimer();

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
            //Show();

            timer.Interval = new TimeSpan(0, 0, 0, 1);
            timer.Tick += Timer_Tick;
            //timer.Start();

            Window.Current.CoreWindow.KeyDown += CoreWindow_KeyDown; ;

            // Make sure all controls has the correct size and position:
            Controls.ResizeControls(gridControls, Window.Current.Bounds);
            Controls.SetControlsUniform(gridControls);
            UpdateLayout();
            initDone = true;
        }

        private void CoreWindow_KeyDown(Windows.UI.Core.CoreWindow sender, Windows.UI.Core.KeyEventArgs args)
        {
            switch (args.VirtualKey)
            {
                case VirtualKey.F1:
                    Show();
                    timer.Start();
                    break;
                case VirtualKey.F2:
                    timer.Stop();
                    Hide();
                    break;
            }
        }

        private void Timer_Tick(object sender, object e)
        {
            if (indicator != null
                && compoundControls != null
                && compoundControls.Count == 2
                && compoundControls[0].SubControls.ControlsList != null
                && compoundControls[0].SubControls.ControlsList.Count == 2)
            {
                indicator.IsOn = !indicator.IsOn;
                ((Indicator)compoundControls[0].SubControls.ControlsList[0]).IsOn = !indicator.IsOn;
                ((Indicator)compoundControls[1].SubControls.ControlsList[0]).IsOn = indicator.IsOn;
                ((Indicator)compoundControl1.SubControls.ControlsList[0]).IsOn = indicator.IsOn;
                ((Indicator)compoundControl2.SubControls.ControlsList[0]).IsOn = !indicator.IsOn;
            }
        }

        private void Show()
        {
            // Create all controls:
            int i = 0;
            indicator = Controls.AddIndicator(i++, gridControls,
                new Image[] { imgLedOn, imgLedOff }, new Point(50, 12));

            Controls.AddRotator(i++, gridControls,
                new Image[] { imgSelectSquare, imgSelectSawUp, imgSelectSawDwn, imgSelectTriangle, imgSelectSine },
                new Point(100, 10));

            compoundControl1 = Controls.AddCompoundControl(Window.Current.Bounds, imgClickArea, i++, 0,
                gridControls, new Image[] { imgControlPanelBackground }, new Rect(330, 50, 0, 0));
            compoundControl2 = Controls.AddCompoundControl(Window.Current.Bounds, imgClickArea, i++, 0,
                gridControls, new Image[] { imgControlPanelBackground }, new Rect(330, 270, 0, 0));

            compoundControl1.AddIndicator(i++, gridControls,
                new Image[] { imgLedOn, imgLedOff }, new Point(20, 22));
            compoundControl1.AddRotator(i++, gridControls,
                new Image[] { imgSelectSquare, imgSelectSawUp, imgSelectSawDwn, imgSelectTriangle, imgSelectSine },
                new Point(70, 20));

            compoundControl2.AddIndicator(i++, gridControls,
                new Image[] { imgLedOn, imgLedOff }, new Point(20, 22));
            compoundControl2.AddRotator(i++, gridControls,
                new Image[] { imgSelectSquare, imgSelectSawUp, imgSelectSawDwn, imgSelectTriangle, imgSelectSine },
                new Point(70, 20));

            compoundControls = new List<CompoundControl>();

            compoundControls.Add(Controls.AddCompoundControl(Window.Current.Bounds, imgClickArea, i++, 0,
                gridControls, new Image[] { imgControlPanelBackground }, new Rect(30, 50, 0, 0)));
            compoundControls.Add(Controls.AddCompoundControl(Window.Current.Bounds, imgClickArea, i++, 0,
                gridControls, new Image[] { imgControlPanelBackground }, new Rect(30, 270, 0, 0)));

            foreach (CompoundControl compoundControl in compoundControls)
            {
                compoundControl.AddIndicator(i++, gridControls,
                    new Image[] { imgLedOn, imgLedOff }, new Point(20, 22));
                compoundControl.AddRotator(i++, gridControls,
                    new Image[] { imgSelectSquare, imgSelectSawUp, imgSelectSawDwn, imgSelectTriangle, imgSelectSine },
                    new Point(70, 20));
            }

            // Make sure all controls has the correct size and position:
            Controls.ResizeControls(gridControls, Window.Current.Bounds);
            Controls.SetControlsUniform(gridControls);
            UpdateLayout();

            gridOther.Visibility = Visibility.Visible;
        }

        private void Hide()
        {
            while (Controls.ControlsList.Count > 0)
            {
                Controls.RemoveControl(Controls.ControlsList[0]);
            }
            gridOther.Visibility = Visibility.Collapsed;
        }

        // When the pointer is moved over the click-area, ask the Controls
        // object if, and if so which control the pointer is over:
        private void imgClickArea_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            if (initDone && Controls != null)
            {
                Controls.PointerMoved(sender, e);
            }
        }

        private void imgClickArea_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            if (initDone && Controls != null)
            {
                Controls.PointerPressed(sender, e);
            }
        }

        private void imgClickArea_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            Controls.PointerReleased(sender, e);
        }

        private void imgClickArea_PointerWheelChanged(object sender, PointerRoutedEventArgs e)
        {
            Controls.PointerWheelChanged(sender, e);
        }

        // Tapped event, handlers for controls that are supposed to be tapped:
        private void imgClickArea_Tapped(object sender, TappedRoutedEventArgs e)
        {
        }

        // Right tapped event, handlers for controls that are supposed to be tapped:
        private void imgClickArea_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            Show();
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

        //private void gridControls_PointerMoved(object sender, PointerRoutedEventArgs e)
        //{

        //}

        //private void gridControls_PointerPressed(object sender, PointerRoutedEventArgs e)
        //{

        //}

        //private void gridControls_PointerReleased(object sender, PointerRoutedEventArgs e)
        //{

        //}
    }
}
