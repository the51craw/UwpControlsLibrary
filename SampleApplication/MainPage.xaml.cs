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

namespace SampleApplication
{
    public sealed partial class MainPage : Page
    {
        // The Controls object:
        Controls Controls;

        // This enum must contain one entry for each control!
        // You can use it in handlers to identify what action to take when control is used.
        public enum ControlId
        {
            STATICIMAGE1,       // 0 
            STATICIMAGE2,       // 1 
            INDICATOR1,         // 2 
            INDICATOR2,         // 3
            KNOB,               // 4
            MOMENTARYBUTTON,    // 5
            MOMENTARYBUTTON2,   // 6
            VERTICAL_SLIDER,    // 7
            HORIZONTAL_SLIDER,  // 8
            LABEL,              // 9
            JOYSTICK,           // 10
            SELECTOR,           // 11
            KEYBOARD,           // 12
        }

        public Point PointerPosition { get; set; }

        public Double ExtraMarginLeft = 0;
        public Double ExtraMarginTop = 0;
        public Double WidthSizeRatio = 1;
        public Double HeightSizeRatio = 1;
        Boolean leftButtonIsPressed;
        Boolean rightButtonIsPressed;
        Boolean otherButtonIsPressed;
        Boolean ShiftIsPressed;
        Int32 currentMouseButton = 0;
        Point pointerPositionAtPointerPressed;
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
            Controls.InitControls(gridControls);

            // Create all controls:
            Controls.AddStaticImage((int)ControlId.STATICIMAGE1, gridControls, new Image[] { imgStaticImage }, new Point(20, 411));
            Controls.AddStaticImage((int)ControlId.STATICIMAGE2, gridControls, new Image[] { imgStaticImage }, new Point(20, 465));
            Controls.AddIndicator((int)ControlId.INDICATOR1, gridControls, new Image[] { imgIndicatorOn/*, imgIndicatorOff*/ }, new Point(575, 28));
            Controls.AddIndicator((int)ControlId.INDICATOR2, gridControls, new Image[] { imgIndicatorOn, imgIndicatorOff }, new Point(608, 28));
            Controls.AddKnob((int)ControlId.KNOB, gridControls, new Image[] { imgKnob }, new Point(120, 300), false, 0, 127, 30, 330);
            Controls.AddMomentaryButton((int)ControlId.MOMENTARYBUTTON, gridControls, new Image[] { imgMomentaryButtonUp, imgMomentaryButtonDown }, 
                new Point(236, 19));
            Controls.AddMomentaryButton((int)ControlId.MOMENTARYBUTTON2, gridControls, new Image[] { imgMomentaryButtonUp, imgMomentaryButtonDown },
                new Point(20, 19));
            Controls.AddVerticalSlider((int)ControlId.VERTICAL_SLIDER, gridControls, new Image[] { imgVerticalSliderBackground, imgVerticalSliderHandle }, 
                new Rect(270, 134, 121,399), false);
            Controls.AddHorizontalSlider((int)ControlId.HORIZONTAL_SLIDER, gridControls, new Image[] { imgHorizontalSliderBackground, imgHorizontalSliderHandle }, 
                new Rect(427, 424, 399, 121), 0, 127, false);
            Controls.AddLabel((int)ControlId.LABEL, gridControls, new Rect(new Point(660, 30), new Point(837, 53)), tbText, 12);
            Controls.AddJoystick((int)ControlId.JOYSTICK, gridControls, 
                new Image[] { imgJoystickBackground, imgStick1, imgStick2, imgStick3, imgJoystickHandle }, new Rect(495, 164, 199, 199), -64, 63, -64, 63, false);
            Controls.AddRotator((int)ControlId.SELECTOR, gridControls, 
                new Image[] { imgSelection0, imgSelection1, imgSelection2, imgSelection3 }, new Point(236, 73));
            Controls.AddKeyBoard((int)ControlId.KEYBOARD, gridKeyboard, imgWhiteKey, imgBlackKey, new Point(144, 560), 36, 60);
            foreach (Octave octave in ((Keyboard)Controls.GetControl((Int32)ControlId.KEYBOARD)).Octaves)
            {
                foreach (Key key in octave.Keys)
                {
                    key.Image.PointerMoved += Keyboard_PointerMoved;
                    key.Image.PointerPressed += Keyboard_PointerPressed;
                    key.Image.PointerReleased += Keyboard_PointerReleased;
                }
            }

            // Make sure all controls has the correct size and position:
            Controls.ResizeControls(gridControls, Window.Current.Bounds);
            Controls.SetControlsUniform(gridControls);
            Controls.HideOriginalControls();
            UpdateLayout();
            initDone = true;
        }

        private void imgClickArea_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            GetAdjustedMousePosition(e);
            GetMouseButtonPressed(e);

            pointerPositionAtPointerPressed = CopyPoint(PointerPosition);

            foreach (object control in Controls.ControlsList)
            {
                if (((ControlBase)control).ControlSizing.IsHit(pointerPositionAtPointerPressed))
                {
                    ((ControlBase)control).IsSelected = true;
                    Controls.PointerPressed(sender, e);
                    switch (((ControlBase)control).Id)
                    {
                        case (Int32)ControlId.MOMENTARYBUTTON:
                            ((Label)Controls.ControlsList[(Int32)ControlId.LABEL]).TextBlock.Text = "Button down";
                            break;
                    }
                }
            }
        }

        private void imgClickArea_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            GetAdjustedMousePosition(e);
            GetMouseButtonPressed(e);

            foreach (object control in Controls.ControlsList)
            {
                ((ControlBase)control).IsSelected = true;
                if (((ControlBase)control).ControlSizing.IsHit(PointerPosition))
                {
                    Controls.PointerReleased(sender, e);
                    switch (((ControlBase)control).Id)
                    {
                        case (Int32)ControlId.MOMENTARYBUTTON:
                            ((Label)Controls.ControlsList[(Int32)ControlId.LABEL]).TextBlock.Text = "Button up";
                            break;
                    }
                }
                else
                {
                    ((ControlBase)control).IsSelected = false;
                }
            }
        }

        // When the pointer is moved over the click-area, ask the Controls
        // object if and if so which control the pointer is over:
        private void imgClickArea_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            if (initDone)
            {
                GetAdjustedMousePosition(e);
                GetMouseButtonPressed(e);
                ((Label)Controls.ControlsList[(Int32)ControlId.LABEL]).TextBlock.Text = "---";

                if (Controls != null)
                {
                    Controls.PointerMoved(sender, e);
                    if (leftButtonIsPressed || rightButtonIsPressed || otherButtonIsPressed)
                    {
                        foreach (object control in Controls.ControlsList)
                        {
                            if (((ControlBase)control).IsSelected)
                            {
                                switch (((ControlBase)control).Id)
                                {
                                    case (Int32)ControlId.KNOB:
                                        ((Label)Controls.ControlsList[(Int32)ControlId.LABEL]).TextBlock.Text = "Value " +
                                        ((Knob)Controls.ControlsList[(Int32)ControlId.KNOB]).Value.ToString();
                                        break;
                                    case (Int32)ControlId.HORIZONTAL_SLIDER:
                                        ((Label)Controls.ControlsList[(Int32)ControlId.LABEL]).TextBlock.Text = "Value " +
                                        ((HorizontalSlider)Controls.ControlsList[(Int32)ControlId.HORIZONTAL_SLIDER]).Value.ToString();
                                        break;
                                    case (Int32)ControlId.VERTICAL_SLIDER:
                                        ((Label)Controls.ControlsList[(Int32)ControlId.LABEL]).TextBlock.Text = "Value " +
                                        ((VerticalSlider)Controls.ControlsList[(Int32)ControlId.VERTICAL_SLIDER]).Value.ToString();
                                        break;
                                    case (Int32)ControlId.JOYSTICK:
                                        ((Label)Controls.ControlsList[(Int32)ControlId.LABEL]).TextBlock.Text = "X: " +
                                        ((Joystick)Controls.ControlsList[(Int32)ControlId.JOYSTICK]).ValueX.ToString() + ", Y: " +
                                        ((Joystick)Controls.ControlsList[(Int32)ControlId.JOYSTICK]).ValueY.ToString();
                                        break;
                                }
                            }
                        }
                    }
                }
            }
        }

        // Tapped event, handlers for controls that are supposed to be tapped:
        private void imgClickArea_Tapped(object sender, TappedRoutedEventArgs e)
        {
            Controls.Tapped(sender, e);
            foreach (object control in Controls.ControlsList)
            {
                if (((ControlBase)control).IsSelected)
                {
                    switch (((ControlBase)control).Id)
                    {
                        case (Int32)ControlId.MOMENTARYBUTTON:
                            ((Label)Controls.ControlsList[(Int32)ControlId.LABEL]).TextBlock.Text = "Button tapped";
                            break;
                        case (Int32)ControlId.SELECTOR:
                            ((Label)Controls.ControlsList[(Int32)ControlId.LABEL]).TextBlock.Text = "Selected " +
                            ((UwpControlsLibrary.Rotator)Controls.ControlsList[(Int32)ControlId.SELECTOR]).Selection.ToString();
                                break;
                    }
                }
            }
        }

        private void imgClickArea_PointerWheelChanged(object sender, PointerRoutedEventArgs e)
        {
            Controls.PointerWheelChanged(sender, e);
            foreach (object control in Controls.ControlsList)
            {
                if (((ControlBase)control).IsSelected)
                {
                    switch (((ControlBase)control).Id)
                    {
                        case (Int32)ControlId.KNOB:
                            ((Label)Controls.ControlsList[(Int32)ControlId.LABEL]).TextBlock.Text = "Value " +
                            ((Knob)Controls.ControlsList[(Int32)ControlId.KNOB]).Value.ToString();
                            break;
                        case (Int32)ControlId.HORIZONTAL_SLIDER:
                            ((Label)Controls.ControlsList[(Int32)ControlId.LABEL]).TextBlock.Text = "Value " +
                            ((HorizontalSlider)Controls.ControlsList[(Int32)ControlId.HORIZONTAL_SLIDER]).Value.ToString();
                            break;
                        case (Int32)ControlId.VERTICAL_SLIDER:
                            ((Label)Controls.ControlsList[(Int32)ControlId.LABEL]).TextBlock.Text = "Value " +
                            ((VerticalSlider)Controls.ControlsList[(Int32)ControlId.VERTICAL_SLIDER]).Value.ToString();
                            break;
                        case (Int32)ControlId.JOYSTICK:
                            ((Label)Controls.ControlsList[(Int32)ControlId.LABEL]).TextBlock.Text = "X: " +
                            ((Joystick)Controls.ControlsList[(Int32)ControlId.JOYSTICK]).ValueX.ToString() + ", Y: " +
                            ((Joystick)Controls.ControlsList[(Int32)ControlId.JOYSTICK]).ValueY.ToString();
                            break;
                    }
                }
            }
        }

        private void GetAdjustedMousePosition(PointerRoutedEventArgs e)
        {
            PointerPoint MousePositionInWindow = e.GetCurrentPoint(this);
            PointerPosition = new Point(MousePositionInWindow.Position.X - ExtraMarginLeft,
                MousePositionInWindow.Position.Y - ExtraMarginTop);
        }

        public Point CopyPoint(Point f)
        {
            return new Point(f.X, f.Y);
        }

        // When app size is changed, all controls must also be resized,
        // ask the Controls object to do it:
        private void gridControls_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (Controls != null)
            {
                Controls.ResizeControls(gridMain, Window.Current.Bounds);
            }
        }

        private void GetMouseButtonPressed(PointerRoutedEventArgs e)
        {
            PointerPointProperties pointerPointProperties = e.GetCurrentPoint(this).Properties;
            leftButtonIsPressed = pointerPointProperties.IsLeftButtonPressed;
            rightButtonIsPressed = pointerPointProperties.IsRightButtonPressed;

            //if (leftButtonIsPressed)
            //{
            //    buttonPressed = 0;
            //}
            //else if (rightButtonIsPressed)
            //{
            //    buttonPressed = 1;
            //}
            //else if (pointerPointProperties.IsBarrelButtonPressed || pointerPointProperties.IsMiddleButtonPressed
            //    || pointerPointProperties.IsXButton1Pressed || pointerPointProperties.IsXButton2Pressed)
            //{
            //    buttonPressed = 2;
            //}
            //else
            //{
            //    buttonPressed = -1;
            //}
        }

        private void Keyboard_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            PointerPoint pp = e.GetCurrentPoint((Image)sender);
            Int32 velocity = (Int32)(127 * (pp.Position.Y / ((Key)((Image)sender).Tag).Image.ActualHeight / 0.8));
            velocity = velocity > 127 ? 127 : velocity;
            ((Label)Controls.ControlsList[(Int32)ControlId.LABEL]).TextBlock.Text =
                ((Key)((Image)sender).Tag).KeyName + " on, velocity: " + velocity.ToString();
            ((Indicator)Controls.ControlsList[(int)ControlId.INDICATOR1]).IsOn = true;
        }

        private void Keyboard_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            PointerPoint pp = e.GetCurrentPoint((Image)sender);
            ((Label)Controls.ControlsList[(Int32)ControlId.LABEL]).TextBlock.Text =
                ((Key)((Image)sender).Tag).KeyName + " off";
            ((Indicator)Controls.ControlsList[(int)ControlId.INDICATOR1]).IsOn = false;
        }

        private void Keyboard_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            PointerPoint pp = e.GetCurrentPoint((Image)sender);
            ((Key)((Image)sender).Tag).Velocity = (Int32)(pp.Position.Y / ((Key)((Image)sender).Tag).Image.ActualHeight);
            Int32 velocity = (Int32)(127 * (pp.Position.Y / ((Key)((Image)sender).Tag).Image.ActualHeight / 0.8));
            velocity = velocity > 127 ? 127 : velocity;
            ((Label)Controls.ControlsList[(Int32)ControlId.LABEL]).TextBlock.Text =
                ((Key)((Image)sender).Tag).KeyName + " velocity: " + velocity.ToString();
        }

        public void ClearValue()
        {
            ((Label)Controls.ControlsList[(Int32)ControlId.LABEL]).TextBlock.Text = "";
        }

        private void DisplayValue(Object obj)
        {
            if (obj != null)
            {
                if (obj.GetType() == typeof(WhiteKey) || obj.GetType() == typeof(BlackKey))
                {
                    ((Label)Controls.ControlsList[(Int32)ControlId.LABEL]).TextBlock.Text = ((Key)obj).KeyName + " Key number: " + ((Key)obj).KeyNumber.ToString() + " Velocity: " + ((Key)obj).Velocity.ToString();
                }
                else
                {
                    foreach (Object control in Controls.ControlsList)
                    {
                        if (((ControlBase)control).IsSelected)
                        {
                            Controls.CurrentControl = ((ControlBase)control).Id;
                        }

                        switch (Controls.CurrentControl)
                        {
                            case (Int32)ControlId.JOYSTICK:
                                if (obj != null && obj.GetType() == typeof(Int32[]))
                                {
                                    Int32[] values = (Int32[])obj;
                                    ((Label)Controls.ControlsList[(Int32)ControlId.LABEL]).TextBlock.Text = values[0].ToString() + ", " + values[1].ToString();
                                }
                                else
                                {
                                    ((Label)Controls.ControlsList[(Int32)ControlId.LABEL]).TextBlock.Text = ((ControlId)Controls.CurrentControl).ToString();
                                }
                                break;
                            case (Int32)ControlId.VERTICAL_SLIDER:
                                if (obj != null && obj.GetType() == typeof(Int32))
                                {
                                    Int32 value = (Int32)obj;
                                    ((Label)Controls.ControlsList[(Int32)ControlId.LABEL]).TextBlock.Text = value.ToString();
                                }
                                else
                                {
                                    ((Label)Controls.ControlsList[(Int32)ControlId.LABEL]).TextBlock.Text = ((ControlId)Controls.CurrentControl).ToString();
                                }
                                break;
                            case (Int32)ControlId.HORIZONTAL_SLIDER:
                                if (obj != null && obj.GetType() == typeof(Int32))
                                {
                                    Int32 value = (Int32)obj;
                                    ((Label)Controls.ControlsList[(Int32)ControlId.LABEL]).TextBlock.Text = value.ToString();
                                }
                                else
                                {
                                    ((Label)Controls.ControlsList[(Int32)ControlId.LABEL]).TextBlock.Text = ((ControlId)Controls.CurrentControl).ToString();
                                }
                                break;
                            case (Int32)ControlId.KNOB:
                                if (obj != null && obj.GetType() == typeof(Int32))
                                {
                                    Int32 value = (Int32)obj;
                                    ((Label)Controls.ControlsList[(Int32)ControlId.LABEL]).TextBlock.Text = value.ToString();
                                }
                                else
                                {
                                    ((Label)Controls.ControlsList[(Int32)ControlId.LABEL]).TextBlock.Text = ((ControlId)Controls.CurrentControl).ToString();
                                }
                                break;
                            case (Int32)ControlId.KEYBOARD:
                                if (obj != null && obj.GetType() == typeof(Key))
                                {
                                    Int32 velocity = ((Key)obj).Velocity;
                                    Int32 key = ((Key)obj).KeyNumber;
                                    String name = ((Key)obj).KeyName;
                                    ((Label)Controls.ControlsList[(Int32)ControlId.LABEL]).TextBlock.Text = name + " Key number " + key.ToString() + " Velocity " + velocity.ToString();
                                }
                                break;
                        }
                    }
                }
            }
        }
    }
}
