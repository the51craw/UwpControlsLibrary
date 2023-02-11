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
using System.Diagnostics;
using MathNet.Numerics;

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
        public Point PointerPosition;

        public int CurrentControl;

        public int GetPointerButtonStatesAndWheelDelta(PointerRoutedEventArgs e)
        {
            PointerPoint pp = e.GetCurrentPoint(imgClickArea);
            PointerPosition = pp.Position;
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
            if (ppp.IsBarrelButtonPressed)
            {
                PointerButtonStates.Add(ControlBase.PointerButton.BARREL);
            }
            if (ppp.IsXButton1Pressed)
            {
                PointerButtonStates.Add(ControlBase.PointerButton.EXTRA1);
            }
            if (ppp.IsXButton2Pressed)
            {
                PointerButtonStates.Add(ControlBase.PointerButton.EXTRA2);
            }
            if (ppp.IsMiddleButtonPressed || ppp.IsBarrelButtonPressed
                || ppp.IsXButton1Pressed || ppp.IsXButton2Pressed)
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

        public object CallEvent(object sender, PointerRoutedEventArgs e, EventType eventType, int delta)
        {
            PointerPoint pp = e.GetCurrentPoint(imgClickArea);

            if (PointerButtonStates.Count != 0)
            {
                int controlsHit = 0;
                foreach (Object control in ControlsList)
                {
                    ((ControlBase)control).IsSelected = ((ControlBase)control).ControlSizing.IsHit(pp.Position);
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
            return SelectAndCall(this.ControlsList, pp, sender, e, eventType, delta);
        }

        private object SelectAndCall(List<object> controlsList, PointerPoint pp, object sender, PointerRoutedEventArgs e, EventType eventType, int delta)
        {
            // When controls overlay and multiple controls are hit, the last one
            // added is the topmost one, and should be the one handled. This
            // allows e.g. PopupMenuButtons to temporary overlap and obscure other
            // controls. Otherwise overlapping controls should be avoided in design.
            Object control = null;
            foreach (Object ctrl in controlsList)
            {
                ((ControlBase)ctrl).IsSelected = ((ControlBase)ctrl).ControlSizing.IsHit(pp.Position);
                if (((ControlBase)ctrl).IsSelected)
                {
                    if (((ControlBase)ctrl).GetType() != typeof(PopupMenuButton))
                    {
                        control = ctrl;
                    }
                    else if (((PopupMenuButton)ctrl).Visibility == Visibility.Visible)
                    {
                        control = ctrl;
                    }
                }
            }
            if (control != null)
            { 
                Current = ((ControlBase)control).Id;
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
                        return control;
                    }
                    else if (control.GetType() == typeof(ImageButton))
                    {
                        ((ImageButton)control).HandleEvent(e, eventType);
                        return control;
                    }
                    else if (control.GetType() == typeof(Rotator))
                    {
                        ((Rotator)control).HandleEvent(PointerButtonStates, delta);
                        return control;
                    }
                    else if (control.GetType() == typeof(VerticalSlider))
                    {
                        ((VerticalSlider)control).HandleEvent(e, eventType, pp.Position, PointerButtonStates, delta);
                        return control;
                    }
                    else if (control.GetType() == typeof(HorizontalSlider))
                    {
                        ((HorizontalSlider)control).HandleEvent(e, eventType, pp.Position, PointerButtonStates, delta);
                        return control;
                    }
                    else if (control.GetType() == typeof(Joystick))
                    {
                        ((Joystick)control).HandleEvent(e, eventType, pp.Position, PointerButtonStates);
                        return control;
                    }
                    else if (control.GetType() == typeof(PopupMenuButton))
                    {
                        ((PopupMenuButton)control).HandleEvent(e, eventType, pp.Position, PointerButtonStates, delta);
                        return control;
                    }
                    else if (control.GetType() == typeof(TouchpadKeyboard))
                    {
                        return control;
                    }
                    else if (control.GetType() == typeof(Keyboard))
                    {
                        Key key = (Key)((Image)sender).Tag;
                        ((Keyboard)control).HandleEvent(sender, e, eventType, pp.Position, PointerButtonStates, key);
                        return control;
                    }
                    else if (control.GetType() == typeof(CompoundControl))
                    {
                        SelectAndCall(((CompoundControl)control).SubControls.ControlsList, pp, sender, e, eventType, delta);
                        return control;
                    }
                }
            }
            return null;
        }

        public void ClosePopupMenu(PopupMenuButton menu)
        {
            for (int i = 0; i < menu.Children.Count; i++)
            {
                foreach (PopupMenuButton Folder in menu.Children[i])
                {
                    ClosePopupMenu(Folder);
                    Folder.Visibility = Visibility.Collapsed;
                }
            }
        }

        public void CallEvent(object sender, TappedRoutedEventArgs e, ControlBase.EventType eventType)
        {
        }

        public void CallEvent(object sender, RightTappedRoutedEventArgs e, ControlBase.EventType eventType)
        {
        }

        public object PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            return CallEvent(sender, e, ControlBase.EventType.POINTER_MOVED, GetPointerButtonStatesAndWheelDelta(e));
        }

        public Object PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            return CallEvent(sender, e, ControlBase.EventType.POINTER_PRESSED, GetPointerButtonStatesAndWheelDelta(e));
        }

        public Object PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            return CallEvent(sender, e, ControlBase.EventType.POINTER_RELEASED, GetPointerButtonStatesAndWheelDelta(e));
        }

        public void Tapped(object sender, TappedRoutedEventArgs e)
        {
            //Point point = e.GetPosition(imgClickArea);
            //PointerButtonStates = new List<ControlBase.PointerButton>();
            //PointerButtonStates.Add(ControlBase.PointerButton.LEFT);
            //CallEvent(sender, e, ControlBase.EventType.POINTER_TAPPED);
        }

        public void RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            //Point point = e.GetPosition(imgClickArea);
            //PointerButtonStates = new List<ControlBase.PointerButton>();
            //PointerButtonStates.Add(ControlBase.PointerButton.RIGHT);
            //CallEvent(sender, e, ControlBase.EventType.POINTER_RIGHT_TAPPED);
        }

        public Object PointerWheelChanged(object sender, PointerRoutedEventArgs e)
        {
            return CallEvent(sender, e, ControlBase.EventType.POINTER_WHEEL_CHANGED, GetPointerButtonStatesAndWheelDelta(e));
        }
    }
}
