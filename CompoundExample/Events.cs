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

namespace CompositeExample
{
    public sealed partial class MainPage : Page
    {

        public Point PointerPosition { get; set; }
        public Double ExtraMarginLeft = 0;
        public Double ExtraMarginTop = 0;
        public object currentControl;
        private void imgClickArea_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            GetAdjustedMousePosition(e);
            if (Controls != null)
            {
                Controls.OnPointerMoved(sender, e);
                DisplayControlsHit();
                Controls.PointerMoved(sender, e);
                PointerPoint pp = e.GetCurrentPoint(imgClickArea);
                Int32 previousControl = currentControlItem;
                currentControlItem = Controls.FindControl(pp.Position);
            }
        }

        private void DisplayControlsHit()
        {
            String txt = "";
            if (Controls.ControlsHit != null)
            {
                foreach (object control in Controls.ControlsHit.CompoundControls)
                {
                    txt += "Compound control ";
                    switch ((Int32)((Controls.CompoundControlsUnderPointer)control).SubType)
                    {
                        case (int)type.OSCILLATOR:
                            txt += " Oscillator ";
                            break;
                        case (int)type.FILTER:
                            txt += " Filter ";
                            break;
                        case (int)type.ADSR:
                            txt += " ADSR ";
                            break;
                        case (int)type.DISPLAY:
                            txt += " Display ";
                            break;
                    }
                    txt += ((Controls.CompoundControlsUnderPointer)control).Id.ToString();
                    foreach (object subControl in ((Controls.CompoundControlsUnderPointer)control).Controls)
                    {
                        txt += " SubControl " + ((Int32)subControl).ToString();
                    }
                }
                txt += " ";
                foreach (object control in Controls.ControlsHit.Controls)
                {
                    txt += "Control " + ((Int32)control).ToString() + " "; 
                }
            }
            text.Text = txt;
        }

        // Tapped event, handlers for controls that are supposed to be tapped:
        private void imgClickArea_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (currentControlItem > -1)
            {
                Controls.Tapped(sender, e);

                switch (currentControlItem)
                {
                    case (Int32)_controlId.SELECT_LAYOUT:
                        layout = ((UwpControlsLibrary.Rotator)Controls.ControlsList[0]).Selection;
                        Controls.ControlsList.RemoveRange(1, Controls.ControlsList.Count - 1);
                        CreateControls();
                        break;
                    case 5:
                        foreach (object control in Controls.ControlsHit.CompoundControls)
                        {
                            switch ((Int32)((Controls.CompoundControlsUnderPointer)control).SubType)
                            {
                                case (int)type.OSCILLATOR:
                                    break;
                                case (int)type.FILTER:
                                    break;
                                case (int)type.ADSR:
                                    ((Graph)adsrs[0].SubControls.ControlsList[(int)AdsrControls.ADSR_GRAPH]).Draw();
                                    break;
                                case (int)type.DISPLAY:
                                    break;
                            }
                            foreach (object subControl in ((Controls.CompoundControlsUnderPointer)control).Controls)
                            {
                                //txt += " SubControl " + ((Int32)subControl).ToString();
                            }
                        }
                        break;
                }

                //switch ((Int32)((Controls.CompoundControlsUnderPointer)control).SubType)
                //{
                //    case (int)type.OSCILLATOR:
                //        break;
                //    case (int)type.FILTER:
                //        break;
                //    case (int)type.ADSR:
                //        break;
                //    case (int)type.DISPLAY:
                //        break;
                //        case (int)type.
                //}
            }
        }

        private void imgClickArea_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            if (Controls.controlsHit.CompoundControls.Count > 0)
            {
                foreach (Controls.CompoundControlsUnderPointer compoundControl in Controls.controlsHit.CompoundControls)
                {
                    if (compoundControl.Controls.Count > 0)
                    {
                        foreach (Int32 control in compoundControl.Controls)
                        {
                            if (compoundControl.SubType == (Int32)type.OSCILLATOR)
                            {
                                if (control == 5) // Keyboard/LFO
                                {
                                    //((Graph)displays[0].SubControls.ControlsList[0]).Draw(250f);
                                    //display.Frequency = 250f;
                                }
                            }
                            //else if (compoundControl.SubType == (Int32)type.ADSR)
                            //{
                            //    if (control == 4) // Pedal hold
                            //    {
                            //        ((Graph)displays[0].SubControls.ControlsList[0]).Draw(new Point[] 
                            //        { new Point(25, 25), new Point(50, 25), new Point(50, 50), new Point(25, 50), new Point(25, 25)});
                            //    }
                            //}
                        }
                    }
                }
            }
            if (currentControlItem > -1)
            {
                Controls.PointerPressed(currentControlItem, e);
            }
        }

        private void imgClickArea_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            ((Graph)displays[0].SubControls.ControlsList[0]).Erase();
            if (currentControlItem > -1)
            {
                Controls.PointerReleased(currentControlItem, e);
            }
        }

        private void imgClickArea_PointerWheelChanged(object sender, PointerRoutedEventArgs e)
        {
            Controls.PointerWheelChanged(sender, e);
        }
    }
}
