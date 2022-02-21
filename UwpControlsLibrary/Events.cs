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

namespace UwpControlsLibrary
{
    public partial class Controls
    {
        /// <summary>
        /// While a mouse button is held, current control sticks
        /// to last control found on mouse down even if mouse
        /// exits its hit area. This allows for larger value
        /// ranges than the hight/width of the hit area.
        /// </summary>
        public int CurrentControl;
        private Boolean leftButtonPressed;
        private Boolean rightButtonPressed;
        private Boolean otherButtonPressed;

        public Object PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            PointerPoint pp = e.GetCurrentPoint(imgClickArea);
            PointerPointProperties ppp = pp.Properties;
            leftButtonPressed = ppp.IsLeftButtonPressed;
            rightButtonPressed = ppp.IsRightButtonPressed;
            otherButtonPressed = !leftButtonPressed && !rightButtonPressed ? true : false;

            // Whenever any button is pressed all controls under the mouse pointer are selected:
            foreach (Object control in ControlsList)
            {
                ((ControlBase)control).IsSelected = ((ControlBase)control).ControlSizing.IsHit(pp.Position);
                if (((ControlBase)control).IsSelected)
                {
                    if (control.GetType() == typeof(Knob))
                    {
                        ((Knob)control).PointerPressed(pp.Position);
                    }
                    else if (control.GetType() == typeof(MomentaryButton))
                    {
                        ((MomentaryButton)control).IsOn = true;
                        return ((MomentaryButton)control).IsOn;
                    }
                    else if (control.GetType() == typeof(TouchpadKeyboard))
                    {
                        return ((TouchpadKeyboard)control).PointerPressed(pp.Position);
                    }
                    else if (control.GetType() == typeof(UwpControlsLibrary.CompoundControl))
                    {
                        foreach (object subControl in ((UwpControlsLibrary.CompoundControl)control).SubControls.ControlsList)
                        {
                            ((ControlBase)subControl).IsSelected =
                                ((ControlBase)subControl).ControlSizing.IsHit(pp.Position);
                        }
                    }
                }
            }
            return null;
        }

        public Object PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            PointerPoint pp = e.GetCurrentPoint(imgClickArea);
            PointerPointProperties ppp = pp.Properties;
            otherButtonPressed = !ppp.IsLeftButtonPressed && !ppp.IsRightButtonPressed ? false : true;
            leftButtonPressed = ppp.IsLeftButtonPressed;
            rightButtonPressed = ppp.IsRightButtonPressed;

            // Whenever all mouse buttons are released, all controls are deselected:
            if (!leftButtonPressed && !rightButtonPressed && !otherButtonPressed)
            {
                foreach (Object control in ControlsList)
                {
                    ((ControlBase)control).IsSelected = false;
                    if (control.GetType() == typeof(MomentaryButton))
                    {
                        ((MomentaryButton)control).IsOn = false;
                        //return ((MomentaryButton)control).IsOn;
                    }
                    else if (control.GetType() == typeof(TouchpadKeyboard))
                    {
                        return ((TouchpadKeyboard)control).PointerReleased(pp.Position);
                    }
                    else if (control.GetType() == typeof(UwpControlsLibrary.CompoundControl))
                    {
                        foreach (object subControl in ((UwpControlsLibrary.CompoundControl)control).SubControls.ControlsList)
                        {
                            ((ControlBase)subControl).IsSelected = false;
                        }
                    }
                }
            }
            return false;
        }

        public void PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            PointerPoint pp = e.GetCurrentPoint(imgClickArea);
            PointerPointProperties ppp = pp.Properties;
            leftButtonPressed = ppp.IsLeftButtonPressed;
            rightButtonPressed = ppp.IsRightButtonPressed;

            foreach (Object control in ControlsList)
            {
                if (leftButtonPressed || rightButtonPressed || otherButtonPressed)
                {
                    if (((ControlBase)control).Enabled && ((ControlBase)control).IsSelected)
                    {
                        if (control.GetType() == typeof(Knob))
                        {
                            ((Knob)control).PointerMoved(pp.Position);
                        }
                        else if (control.GetType() == typeof(HorizontalSlider))
                        {
                            ((HorizontalSlider)control).SetValue(pp.Position);
                        }
                        else if (control.GetType() == typeof(VerticalSlider))
                        {
                            ((VerticalSlider)control).SetValue(pp.Position);
                        }
                        else if (control.GetType() == typeof(Joystick))
                        {
                            ((Joystick)control).SetValue(pp.Position);
                        }
                        else if (control.GetType() == typeof(Graph))
                        {
                            ((Graph)control).SetValue(pp.Position);
                        }
                        else if (control.GetType() == typeof(UwpControlsLibrary.CompoundControl))
                        {
                            foreach (Object subControl in ((UwpControlsLibrary.CompoundControl)control).SubControls.ControlsList)
                            {
                                if (((ControlBase)subControl).Enabled && ((ControlBase)subControl).IsSelected)
                                {
                                    if (subControl.GetType() == typeof(Knob))
                                    {
                                        ((Knob)subControl).PointerMoved(pp.Position);
                                    }
                                    else if (subControl.GetType() == typeof(HorizontalSlider))
                                    {
                                        ((HorizontalSlider)subControl).SetValue(pp.Position);
                                    }
                                    else if (subControl.GetType() == typeof(VerticalSlider))
                                    {
                                        ((VerticalSlider)subControl).SetValue(pp.Position);
                                    }
                                    else if (subControl.GetType() == typeof(Joystick))
                                    {
                                        ((Joystick)subControl).SetValue(pp.Position);
                                    }
                                    else if (subControl.GetType() == typeof(Graph))
                                    {
                                        ((Graph)subControl).SetValue(pp.Position);
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    ((ControlBase)control).IsSelected = ((ControlBase)control).ControlSizing.IsHit(pp.Position);
                    if (control.GetType() == typeof(UwpControlsLibrary.CompoundControl))
                    {
                        foreach (Object subControl in ((UwpControlsLibrary.CompoundControl)control).SubControls.ControlsList)
                        {
                            ((ControlBase)subControl).IsSelected = ((ControlBase)subControl).ControlSizing.IsHit(pp.Position);
                        }
                    }
                }
            }
        }

        public void Tapped(object sender, TappedRoutedEventArgs e)
        {
            foreach (Object control in ControlsList)
            {
                if (((ControlBase)control).ControlSizing.IsHit(e.GetPosition((UIElement)sender)))
                {
                    if (control.GetType() == typeof(Rotator))
                    {
                        ((Rotator)control).Tapped();
                    }
                    else if (control.GetType() == typeof(CompoundControl))
                    {
                        foreach (Object subControl in ((CompoundControl)control).SubControls.ControlsList)
                        {
                            if (subControl.GetType() == typeof(Rotator))
                            {
                                if (((ControlBase)subControl).ControlSizing.IsHit(e.GetPosition((UIElement)sender)))
                                {
                                    if (subControl.GetType() == typeof(Rotator))
                                    {
                                        ((Rotator)subControl).Tapped();
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        public void RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            foreach (Object control in ControlsList)
            {
                if (((ControlBase)control).IsSelected)
                {
                    if (control.GetType() == typeof(Rotator))
                    {
                        ((Rotator)control).RightTapped();
                    }
                }
                else if (control.GetType() == typeof(CompoundControl))
                {
                    foreach (Object subControl in ((CompoundControl)control).SubControls.ControlsList)
                    {
                        if (subControl.GetType() == typeof(Rotator))
                        {
                            if (((ControlBase)subControl).ControlSizing.IsHit(e.GetPosition((UIElement)sender)))
                            {
                                if (subControl.GetType() == typeof(Rotator))
                                {
                                    ((Rotator)subControl).RightTapped();
                                }
                            }
                        }
                    }
                }
            }
        }

        public int PointerWheelChanged(object sender, PointerRoutedEventArgs e)
        {
            PointerPoint pp = e.GetCurrentPoint(imgClickArea);
            PointerPointProperties ppp = pp.Properties;
            leftButtonPressed = ppp.IsLeftButtonPressed;
            rightButtonPressed = ppp.IsRightButtonPressed;

            int delta = ppp.MouseWheelDelta > 0 ? 1 : -1;
            if (leftButtonPressed)
            {
                delta *= 4;
            }
            if (rightButtonPressed)
            {
                delta *= 16;
            }
            foreach (Object control in ControlsList)
            {
                if (((ControlBase)control).ControlSizing.IsHit(pp.Position))
                {
                    if (control.GetType() == typeof(UwpControlsLibrary.CompoundControl))
                    {
                        foreach (Object subControl in ((UwpControlsLibrary.CompoundControl)control).SubControls.ControlsList)
                        {
                            if (((ControlBase)subControl).ControlSizing.IsHit(pp.Position))
                            {
                                if (subControl.GetType() == typeof(Knob))
                                {
                                    return ((Knob)subControl).PointerWheelChanged(delta);
                                }
                                else if (subControl.GetType() == typeof(HorizontalSlider))
                                {
                                    return ((HorizontalSlider)subControl).PointerWheelChanged(delta);
                                }
                                else if (subControl.GetType() == typeof(VerticalSlider))
                                {
                                    return ((VerticalSlider)subControl).PointerWheelChanged(delta);
                                }
                                else if (subControl.GetType() == typeof(Rotator))
                                {
                                    return ((Rotator)subControl).PointerWheelChanged(delta);
                                }
                            }
                        }
                    }
                    else if (control.GetType() == typeof(Knob))
                    {
                        return ((Knob)control).PointerWheelChanged(delta);
                    }
                    else if (control.GetType() == typeof(HorizontalSlider))
                    {
                        return ((HorizontalSlider)control).PointerWheelChanged(delta);
                    }
                    else if (control.GetType() == typeof(VerticalSlider))
                    {
                        return ((VerticalSlider)control).PointerWheelChanged(delta);
                    }
                    else if (control.GetType() == typeof(Rotator))
                    {
                        return ((Rotator)control).PointerWheelChanged(delta);
                    }
                }
            }
            return 0;
        }
    }
}
