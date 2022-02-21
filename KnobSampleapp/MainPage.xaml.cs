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
using Windows.UI.Input;
using Windows.UI.Popups;
using System.Threading.Tasks;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace KnobSampleapp
{
    public sealed partial class MainPage : Page
    {
        // The Controls object:
        Controls Controls;

        // PointerMoved will update this in order to let other handlers know which control is handled.
        private Int32 currentControl;

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
            Controls.InitControls(gridControls);

            // Create all controls:
            Int32 i = 0;
            Controls.AddKnob(i++, gridControls, new Image[] { imgKnob }, new Point(129, 100), true, 0, 127, 30, 330, 2);

            // Make sure all controls has the correct size and position:
            Controls.ResizeControls(gridControls, Window.Current.Bounds);
            Controls.SetControlsUniform(gridControls);
            UpdateLayout();
        }

        // When app size is changed, all controls must also be resized,
        // ask the Controls object to do it:
        private void gridMain_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (Controls != null)
            {
                Controls.ResizeControls(gridMain, Window.Current.Bounds);
            }
        }

        // When the pointer is moved over the click-area, ask the Controls
        // object if and if so which control the pointer is over:
        private void imgClickArea_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            if (Controls != null)
            {
                Controls.PointerMoved(sender, e);
                //object obj = null;
                //PointerPoint pp = e.GetCurrentPoint(imgClickArea);
                //Double x = pp.Position.X;
                //Double y = pp.Position.Y;

                //Int32 previousControl = currentControl;
                //currentControl = Controls.FindControl(pp.Position);
                //if (currentControl > -1)
                //{
                //    PointerPoint currentPoint = e.GetCurrentPoint(null);
                //    PointerPointProperties props = currentPoint.Properties;
                //    if (props.IsLeftButtonPressed)
                //    {
                //        //obj = ((ControlBase)Controls.ControlsList[currentControl]).PointerMoved(new Point(x, y));
                //    }
                //}
            }
        }

        // Tapped event, handlers for controls that are supposed to be tapped:
        private void imgClickArea_Tapped(object sender, TappedRoutedEventArgs e)
        {
            Controls.Tapped(sender, e);
            //for (Int32 i = 0; i < gridControls.Children.Count; i++)
            //{
            //    if (gridControls.Children[i].GetType() == typeof(Image))
            //    {
            //        if (((Image)gridControls.Children[i]).Stretch == Stretch.Uniform)
            //        {
            //            ((Image)gridControls.Children[i]).Stretch = Stretch.Fill;
            //        }
            //        else
            //        {
            //            ((Image)gridControls.Children[i]).Stretch = Stretch.Uniform;
            //        }
            //    }
            //}

            //if (currentControl > -1)
            //{
            //    Object indicator;
            //    //Object label = Controls.ControlsList[(Int32)ControlId.LABEL];
            //    Object control = Controls.ControlsList[currentControl];
            //    Boolean isOn;

            //    switch (currentControl)
            //    {
            //        //case (Int32)ControlId.PUSHBUTTON:
            //        //    indicator = ((ControlBase)Controls.ControlsList[(Int32)ControlId.INDICATOR1]);
            //        //    if (indicator != null)
            //        //    {
            //        //        ((Indicator)indicator).IsOn = !((Indicator)indicator).IsOn;
            //        //        ((Label)label).TextBlock.Text = ((Indicator)indicator).IsOn.ToString();
            //        //    }
            //        //    break;
            //        //case (Int32)ControlId.SELECTOR:
            //        //    ((ControlBase)control).Tapped(sender, e);
            //        //    ((Label)Controls.ControlsList[(Int32)ControlId.LABEL]).TextBlock.Text =
            //        //        ((UwpControlsLibrary.Rotator)Controls.ControlsList[(Int32)ControlId.SELECTOR]).Selection.ToString();
            //        //    break;
            //    }
            //}
        }

        private void imgClickArea_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            //if (currentControl > -1)
            {
                Controls.PointerPressed(sender, e);
                //switch (currentControl)
                //{
                //}
            }
        }

        private void imgClickArea_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            Controls.PointerReleased(sender, e);
        }

        //private void imgClickArea_Tapped(object sender, TappedRoutedEventArgs e)
        //{

        //}

        private void imgClickArea_PointerWheelChanged(object sender, PointerRoutedEventArgs e)
        {
            Controls.PointerWheelChanged(sender, e);
            //if (currentControl > -1)
            //{
            //    PointerPoint pp = e.GetCurrentPoint(imgClickArea);
            //    PointerPointProperties ppp = pp.Properties;
            //    Int32 delta = ppp.MouseWheelDelta;
            //    if (ppp.IsLeftButtonPressed)
            //    {
            //        delta = delta > 0 ? 5 : -5;
            //    }
            //    else
            //    {
            //        delta = delta > 0 ? 1 : -1;
            //    }
            //    Int32 value = (Int32)Controls.PointerWheelChanged(currentControl, delta);
            //}
        }
    }
}
