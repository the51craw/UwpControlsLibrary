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
using Windows.ApplicationModel.Calls;
using System.Reflection;
using Windows.UI.Core;
using static UwpControlsLibrary.ControlBase;

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

        public List<ControlBase.PointerButton> PointerButtonStates;

        public int CurrentControl;

        public int GetPointerButtonStatesAndWheelDelta(PointerRoutedEventArgs e)
        {
            PointerPoint pp = e.GetCurrentPoint(imgClickArea);
            PointerPointProperties ppp = pp.Properties;
            PointerButtonStates = new List<ControlBase.PointerButton>();

            if (ppp.IsLeftButtonPressed)
            {
                PointerButtonStates.Add(ControlBase.PointerButton.LEFT);
            }
            if (ppp.IsRightButtonPressed)
            {
                PointerButtonStates.Add(ControlBase.PointerButton.RIGHT);
            }
            if (ppp.IsMiddleButtonPressed)
            {
                PointerButtonStates.Add(ControlBase.PointerButton.MIDDLE);
            }
            if (ppp.IsBarrelButtonPressed || ppp.IsXButton1Pressed || ppp.IsXButton2Pressed)
            {
                PointerButtonStates.Add(ControlBase.PointerButton.OTHER);
            }
            if (ppp.MouseWheelDelta > 0)
            {
                return 1;
            }
            else if (ppp.MouseWheelDelta < 0)
            {
                return -1;
            }
            else
            {
                return 0;
            }
        }

        public void CallEvent(object sender, PointerRoutedEventArgs e, ControlBase.EventType eventType, int delta = 0)
        {
            PointerPoint pp = e.GetCurrentPoint(imgClickArea);

            if (PointerButtonStates.Count != 0)
            {
                int controlsHit = 0;
                foreach (Object control in ControlsList)
                {
                    if (((ControlBase)control).IsSelected)
                    {
                        controlsHit++;
                        break;
                    }
                }
                if (controlsHit == 0)
                {
                    foreach (Object ctrl in ControlsList)
                    {
                        if (ctrl.GetType() == typeof(PopupMenuButton))
                        {
                            ((PopupMenuButton)ctrl).HideAllMenus();
                        }
                    }
                }
            }

            foreach (Object control in ControlsList)
            {
                if (control.GetType() == typeof(PopupMenuButton))
                {
                    ((PopupMenuButton)control).ResetHovering();
                }
                else if (control.GetType() == typeof(ImageButton))
                {
                    ((ImageButton)control).ResetHovering();
                }

                ((ControlBase)control).IsSelected = ((ControlBase)control).ControlSizing.IsHit(pp.Position);
                if (((ControlBase)control).IsSelected)
                {
                    if (control.GetType() == typeof(Knob))
                    {
                        ((Knob)control).HandleEvent(pp.Position, eventType, PointerButtonStates, delta);
                    }
                    else if (control.GetType() == typeof(ImageButton))
                    {
                        ((ImageButton)control).HandleEvent(e, eventType);
                    }
                    else if (control.GetType() == typeof(MomentaryButton))
                    {
                    }
                    else if (control.GetType() == typeof(Rotator))
                    {
                        ((Rotator)control).HandleEvent(PointerButtonStates, delta);
                    }
                    else if (control.GetType() == typeof(VerticalSlider))
                    {
                        ((VerticalSlider)control).HandleEvent(e, eventType, pp.Position, PointerButtonStates, delta);
                    }
                    else if (control.GetType() == typeof(HorizontalSlider))
                    {
                        ((HorizontalSlider)control).HandleEvent(e, eventType, pp.Position, PointerButtonStates, delta);
                    }
                    else if (control.GetType() == typeof(Joystick))
                    {
                        ((Joystick)control).HandleEvent(e, eventType, pp.Position, PointerButtonStates);
                    }
                    else if (control.GetType() == typeof(PopupMenuButton)
                        && ((PopupMenuButton)control).Visibility == Visibility.Visible
                        && ((PopupMenuButton)control).IsSelected)
                    {
                        ((PopupMenuButton)control).SetHovering();
                        int menu = 0;
                        if (((PopupMenuButton)control).Tag != null)
                        {
                            menu = ((PopupMenuButton)((PopupMenuButton)control).Tag).Menu;
                        }
                        ((PopupMenuButton)control).HandleEvent(e, eventType, PointerButtonStates, delta, menu);
                        //((PopupMenuButton)control).IsSelected = true;
                        //if (((PopupMenuButton)control).ImageList != null)
                        //{
                        //    if (((PopupMenuButton)control).ImageList.Length == 1)
                        //    {
                        //        // Only one image, show on PointerPressed:
                        //        ((PopupMenuButton)control).ImageList[0].Visibility = Visibility.Visible;
                        //    }
                        //    if (((PopupMenuButton)control).ImageList.Length > 1)
                        //    {
                        //        // Two images or three images, show second and hide first on PointerPressed:
                        //        ((PopupMenuButton)control).ImageList[1].Visibility = Visibility.Visible;
                        //        ((PopupMenuButton)control).ImageList[0].Visibility = Visibility.Collapsed;
                        //    }
                        //}
                    }
                    else if (control.GetType() == typeof(TouchpadKeyboard))
                    {
                        //return ((TouchpadKeyboard)control).PointerPressed(pp.Position);
                    }
                    else if (control.GetType() == typeof(Keyboard))
                    {
                        Key key = (Key)((Image)sender).Tag;
                        ((Keyboard)control).HandleEvent(sender, e, eventType, pp.Position, PointerButtonStates, key);
                        //if (key.Images.Length > 1)
                        //{
                        //    key.Images[1].Visibility = Visibility.Visible;
                        //}
                    }
                    else if (control.GetType() == typeof(CompoundControl))
                    {
                        //foreach (object subControl in ((CompoundControl)control).SubControls.ControlsList)
                        //{
                        //    ((ControlBase)subControl).IsSelected =
                        //        ((ControlBase)subControl).ControlSizing.IsHit(pp.Position);
                        //}
                    }
                }
            }
        }

        public void CallEvent(object sender, TappedRoutedEventArgs e, ControlBase.EventType eventType)
        {
        }

        public void CallEvent(object sender, RightTappedRoutedEventArgs e, ControlBase.EventType eventType)
        {
        }

        public void PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            CallEvent(sender, e, ControlBase.EventType.POINTER_MOVED, GetPointerButtonStatesAndWheelDelta(e));

            //PointerPoint pp = e.GetCurrentPoint(imgClickArea);
            //PointerPointProperties ppp = pp.Properties;
            //leftButtonPressed = ppp.IsLeftButtonPressed;
            //rightButtonPressed = ppp.IsRightButtonPressed;

            //foreach (Object control in ControlsList)
            //{
            //    if (leftButtonPressed || rightButtonPressed || otherButtonPressed)
            //    {
            //        if (((ControlBase)control).Enabled && ((ControlBase)control).IsSelected)
            //        {
            //            if (control.GetType() == typeof(Knob))
            //            {
            //                ((Knob)control).PointerMoved(pp.Position);
            //            }
            //            else if (control.GetType() == typeof(HorizontalSlider))
            //            {
            //                ((HorizontalSlider)control).SetValue(pp.Position);
            //            }
            //            else if (control.GetType() == typeof(VerticalSlider))
            //            {
            //                ((VerticalSlider)control).SetValue(pp.Position);
            //            }
            //            else if (control.GetType() == typeof(Joystick))
            //            {
            //                ((Joystick)control).SetValue(pp.Position);
            //            }
            //            else if (control.GetType() == typeof(Graph))
            //            {
            //                ((Graph)control).SetValue(pp.Position);
            //            }
            //            else if (control.GetType() == typeof(UwpControlsLibrary.CompoundControl))
            //            {
            //                foreach (Object subControl in ((UwpControlsLibrary.CompoundControl)control).SubControls.ControlsList)
            //                {
            //                    if (((ControlBase)subControl).Enabled && ((ControlBase)subControl).IsSelected)
            //                    {
            //                        if (subControl.GetType() == typeof(Knob))
            //                        {
            //                            ((Knob)subControl).PointerMoved(pp.Position);
            //                        }
            //                        else if (subControl.GetType() == typeof(HorizontalSlider))
            //                        {
            //                            ((HorizontalSlider)subControl).SetValue(pp.Position);
            //                        }
            //                        else if (subControl.GetType() == typeof(VerticalSlider))
            //                        {
            //                            ((VerticalSlider)subControl).SetValue(pp.Position);
            //                        }
            //                        else if (subControl.GetType() == typeof(Joystick))
            //                        {
            //                            ((Joystick)subControl).SetValue(pp.Position);
            //                        }
            //                        else if (subControl.GetType() == typeof(Graph))
            //                        {
            //                            ((Graph)subControl).SetValue(pp.Position);
            //                        }
            //                    }
            //                }
            //            }
            //        }
            //    }
            //    else
            //    {
            //        ((ControlBase)control).IsSelected = ((ControlBase)control).ControlSizing.IsHit(pp.Position);
            //        if (control.GetType() == typeof(UwpControlsLibrary.CompoundControl))
            //        {
            //            foreach (Object subControl in ((UwpControlsLibrary.CompoundControl)control).SubControls.ControlsList)
            //            {
            //                ((ControlBase)subControl).IsSelected = ((ControlBase)subControl).ControlSizing.IsHit(pp.Position);
            //            }
            //        }
            //    }
            //}
            //foreach (Object control in ControlsList)
            //{
            //    if (control.GetType() == typeof(PopupMenuButton))
            //    {
            //        ((PopupMenuButton)control).HandleEvent(e, EventType.POINTER_MOVED);
            //    }
            //    else if (control.GetType() == typeof(PopupMenuItem))
            //    {
            //        ((PopupMenuItem)control).HandleEvent(e, EventType.POINTER_MOVED);
            //    }
            //}
        }

        public Object PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            GetPointerButtonStatesAndWheelDelta(e);
            CallEvent(sender, e, ControlBase.EventType.POINTER_PRESSED);

            //PointerPoint pp = e.GetCurrentPoint(imgClickArea);
            //PointerPointProperties ppp = pp.Properties;
            //leftButtonPressed = ppp.IsLeftButtonPressed;
            //rightButtonPressed = ppp.IsRightButtonPressed;
            //otherButtonPressed = !leftButtonPressed && !rightButtonPressed ? true : false;

            //// Whenever any button is pressed all controls under the mouse pointer are selected:
            //foreach (Object control in ControlsList)
            //{
            //    ((ControlBase)control).IsSelected = ((ControlBase)control).ControlSizing.IsHit(pp.Position);
            //    if (((ControlBase)control).IsSelected)
            //    {
            //        if (control.GetType() == typeof(Knob))
            //        {
            //            ((Knob)control).PointerPressed(pp.Position);
            //        }
            //        else if (control.GetType() == typeof(Button))
            //        {
            //            ((Button)control).IsDown = true;
            //            return ((Button)control).IsDown;
            //        }
            //        else if (control.GetType() == typeof(MomentaryButton))
            //        {
            //            ((MomentaryButton)control).IsOn = true;
            //            return ((MomentaryButton)control).IsOn;
            //        }
            //        else if (control.GetType() == typeof(PopupMenuButton))
            //        {
            //            ((PopupMenuButton)control).IsSelected = true;
            //            if (((PopupMenuButton)control).ImageList != null)
            //            {
            //                if (((PopupMenuButton)control).ImageList.Length == 1)
            //                {
            //                    // Only one image, show on PointerPressed:
            //                    ((PopupMenuButton)control).ImageList[0].Visibility = Visibility.Visible;
            //                }
            //                if (((PopupMenuButton)control).ImageList.Length > 1)
            //                {
            //                    // Two images or three images, show second and hide first on PointerPressed:
            //                    ((PopupMenuButton)control).ImageList[1].Visibility = Visibility.Visible;
            //                    ((PopupMenuButton)control).ImageList[0].Visibility = Visibility.Collapsed;
            //                }
            //                return ((PopupMenuButton)control).IsSelected;
            //            }
            //        }
            //        else if (control.GetType() == typeof(TouchpadKeyboard))
            //        {
            //            return ((TouchpadKeyboard)control).PointerPressed(pp.Position);
            //        }
            //        else if (control.GetType() == typeof(Keyboard))
            //        {
            //            Key key = (Key)((Image)sender).Tag;
            //            if (key.Images.Length > 1)
            //            {
            //                key.Images[1].Visibility = Visibility.Visible;
            //            }
            //            //((Key)((Keyboard)control).GetKey(imgClickArea, e.GetCurrentPoint(imgClickArea).Position))
            //            //    .Images[((Key)((Keyboard)control).GetKey(imgClickArea, e.GetCurrentPoint(imgClickArea).Position))
            //            //    .Images.Length - 1].Visibility = Visibility.Visible;
            //            return null;
            //        }
            //        else if (control.GetType() == typeof(CompoundControl))
            //        {
            //            foreach (object subControl in ((CompoundControl)control).SubControls.ControlsList)
            //            {
            //                ((ControlBase)subControl).IsSelected =
            //                    ((ControlBase)subControl).ControlSizing.IsHit(pp.Position);
            //            }
            //        }
            //    }
            //}
            //foreach (Object control in ControlsList)
            //{
            //    if (control.GetType() == typeof(PopupMenuButton))
            //    {
            //        ((PopupMenuButton)control).HandleEvent(e, EventType.POINTER_PRESSED);
            //    }
            //    else if (control.GetType() == typeof(PopupMenuItem))
            //    {
            //        ((PopupMenuItem)control).HandleEvent(e, EventType.POINTER_PRESSED);
            //    }
            //}
            return null;
        }

        public Object PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            GetPointerButtonStatesAndWheelDelta(e);
            CallEvent(sender, e, ControlBase.EventType.POINTER_RELEASED);

            //PointerPoint pp = e.GetCurrentPoint(imgClickArea);
            //PointerPointProperties ppp = pp.Properties;
            //otherButtonPressed = !ppp.IsLeftButtonPressed && !ppp.IsRightButtonPressed ? false : true;
            //leftButtonPressed = ppp.IsLeftButtonPressed;
            //rightButtonPressed = ppp.IsRightButtonPressed;

            //// Whenever all mouse buttons are released, all controls are deselected:
            //if (!leftButtonPressed && !rightButtonPressed && !otherButtonPressed)
            //{
            //    foreach (Object control in ControlsList)
            //    {
            //        ((ControlBase)control).IsSelected = false;
            //        if (control.GetType() == typeof(Button))
            //        {
            //            ((Button)control).IsDown = false;
            //            return ((Button)control).IsDown;
            //        }
            //        else if (control.GetType() == typeof(MomentaryButton))
            //        {
            //            ((MomentaryButton)control).IsOn = false;
            //        }
            //        else if (control.GetType() == typeof(PopupMenuButton))
            //        {
            //            ((PopupMenuButton)control).IsSelected = true;
            //            if (((PopupMenuButton)control).ImageList != null)
            //            {
            //                if (((PopupMenuButton)control).ImageList.Length == 1)
            //                {
            //                    // Only one image, hide on PointerReleased:
            //                    ((PopupMenuButton)control).ImageList[0].Visibility = Visibility.Collapsed;
            //                }
            //                if (((PopupMenuButton)control).ImageList.Length > 1)
            //                {
            //                    // Two images or three images, show first and hide second on PointerPressed:
            //                    ((PopupMenuButton)control).ImageList[0].Visibility = Visibility.Visible;
            //                    ((PopupMenuButton)control).ImageList[1].Visibility = Visibility.Collapsed;
            //                }
            //                return ((PopupMenuButton)control).IsSelected;
            //            }
            //        }
            //        else if (control.GetType() == typeof(TouchpadKeyboard))
            //        {
            //            return ((TouchpadKeyboard)control).PointerReleased(pp.Position);
            //        }
            //        else if (control.GetType() == typeof(Keyboard) && ((Keyboard)control).IsSelected)
            //        {
            //            Key key = (Key)((Image)sender).Tag;
            //            if (key.Images.Length > 1)
            //            {
            //                key.Images[1].Visibility = Visibility.Collapsed;
            //            }
            //            //((Key)((Keyboard)control).GetKey(imgClickArea, e.GetCurrentPoint(imgClickArea).Position))
            //            //    .Images[((Key)((Keyboard)control).GetKey(imgClickArea, e.GetCurrentPoint(imgClickArea).Position))
            //            //    .Images.Length - 1].Visibility = Visibility.Collapsed;
            //            return null;
            //        }
            //        else if (control.GetType() == typeof(UwpControlsLibrary.CompoundControl))
            //        {
            //            foreach (object subControl in ((UwpControlsLibrary.CompoundControl)control).SubControls.ControlsList)
            //            {
            //                ((ControlBase)subControl).IsSelected = false;
            //            }
            //        }
            //    }
            //}

            //foreach (Object control in ControlsList)
            //{
            //    if (control.GetType() == typeof(PopupMenuButton))
            //    {
            //        ((PopupMenuButton)control).HandleEvent(e, EventType.POINTER_RELEASED);
            //    }
            //    else if (control.GetType() == typeof(PopupMenuItem))
            //    {
            //        ((PopupMenuItem)control).HandleEvent(e, EventType.POINTER_RELEASED);
            //    }
            //}
            return false;
        }

        public void Tapped(object sender, TappedRoutedEventArgs e)
        {
            Point point = e.GetPosition(imgClickArea);
            PointerButtonStates = new List<ControlBase.PointerButton>();
            PointerButtonStates.Add(ControlBase.PointerButton.LEFT);
            CallEvent(sender, e, ControlBase.EventType.POINTER_TAPPED);

            //List<ControlBase.PointerButton> pointerButtons = new List<ControlBase.PointerButton>();
            //pointerButtons.Add(ControlBase.PointerButton.LEFT);

            //foreach (object obj in ControlsList)
            //{
            //    CallEvent(sender, e, ControlBase.EventType.POINTER_TAPPED);
            //}

            //foreach (Object control in ControlsList)
            //{
            //    if (((ControlBase)control).ControlSizing.IsHit(e.GetPosition((UIElement)sender)))
            //    {
            //        if (control.GetType() == typeof(Rotator))
            //        {
            //            ((Rotator)control).Tapped();
            //        }
            //        else if (control.GetType() == typeof(CompoundControl))
            //        {
            //            foreach (Object subControl in ((CompoundControl)control).SubControls.ControlsList)
            //            {
            //                if (subControl.GetType() == typeof(Rotator))
            //                {
            //                    if (((ControlBase)subControl).ControlSizing.IsHit(e.GetPosition((UIElement)sender)))
            //                    {
            //                        if (subControl.GetType() == typeof(Rotator))
            //                        {
            //                            ((Rotator)subControl).Tapped();
            //                        }
            //                    }
            //                }
            //            }
            //        }
            //    }
            //}
        }

        public void RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            Point point = e.GetPosition(imgClickArea);
            PointerButtonStates = new List<ControlBase.PointerButton>();
            PointerButtonStates.Add(ControlBase.PointerButton.RIGHT);
            CallEvent(sender, e, ControlBase.EventType.POINTER_RIGHT_TAPPED);
            //    foreach (Object control in ControlsList)
            //    {
            //        if (((ControlBase)control).IsSelected)
            //        {
            //            if (control.GetType() == typeof(Rotator))
            //            {
            //                ((Rotator)control).RightTapped();
            //            }
            //        }
            //        else if (control.GetType() == typeof(CompoundControl))
            //        {
            //            foreach (Object subControl in ((CompoundControl)control).SubControls.ControlsList)
            //            {
            //                if (subControl.GetType() == typeof(Rotator))
            //                {
            //                    if (((ControlBase)subControl).ControlSizing.IsHit(e.GetPosition((UIElement)sender)))
            //                    {
            //                        if (subControl.GetType() == typeof(Rotator))
            //                        {
            //                            ((Rotator)subControl).RightTapped();
            //                        }
            //                    }
            //                }
            //            }
            //        }
            //    }
        }

        public int PointerWheelChanged(object sender, PointerRoutedEventArgs e)
        {
            CallEvent(sender, e, ControlBase.EventType.POINTER_WHEEL_CHANGED, GetPointerButtonStatesAndWheelDelta(e));

            //PointerPoint pp = e.GetCurrentPoint(imgClickArea);
            //PointerPointProperties ppp = pp.Properties;
            //leftButtonPressed = ppp.IsLeftButtonPressed;
            //rightButtonPressed = ppp.IsRightButtonPressed;

            //int delta = ppp.MouseWheelDelta > 0 ? 1 : -1;
            //if (leftButtonPressed)
            //{
            //    delta *= 4;
            //}
            //if (rightButtonPressed)
            //{
            //    delta *= 16;
            //}
            //foreach (Object control in ControlsList)
            //{
            //    if (((ControlBase)control).ControlSizing.IsHit(pp.Position))
            //    {
            //        if (control.GetType() == typeof(UwpControlsLibrary.CompoundControl))
            //        {
            //            foreach (Object subControl in ((UwpControlsLibrary.CompoundControl)control).SubControls.ControlsList)
            //            {
            //                if (((ControlBase)subControl).ControlSizing.IsHit(pp.Position))
            //                {
            //                    if (subControl.GetType() == typeof(Knob))
            //                    {
            //                        return ((Knob)subControl).PointerWheelChanged(delta);
            //                    }
            //                    else if (subControl.GetType() == typeof(HorizontalSlider))
            //                    {
            //                        return ((HorizontalSlider)subControl).PointerWheelChanged(delta);
            //                    }
            //                    else if (subControl.GetType() == typeof(VerticalSlider))
            //                    {
            //                        return ((VerticalSlider)subControl).PointerWheelChanged(delta);
            //                    }
            //                    else if (subControl.GetType() == typeof(Rotator))
            //                    {
            //                        return ((Rotator)subControl).PointerWheelChanged(delta);
            //                    }
            //                }
            //            }
            //        }
            //        else if (control.GetType() == typeof(Knob))
            //        {
            //            return ((Knob)control).PointerWheelChanged(delta);
            //        }
            //        else if (control.GetType() == typeof(HorizontalSlider))
            //        {
            //            return ((HorizontalSlider)control).PointerWheelChanged(delta);
            //        }
            //        else if (control.GetType() == typeof(VerticalSlider))
            //        {
            //            return ((VerticalSlider)control).PointerWheelChanged(delta);
            //        }
            //        else if (control.GetType() == typeof(Rotator))
            //        {
            //            return ((Rotator)control).PointerWheelChanged(delta);
            //        }
            //    }
            //}
            return 0;
        }
    }
}
