using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;

namespace UwpControlsLibrary
{
    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    /// Composite Control class.
    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    public class CompoundControl : ControlBase
    {
        /// <summary>
        /// CompoundControl is a control that contains sub-controls.
        /// First create the CompoundControl.
        /// Then create all sub-controls with coordinates relative to the
        /// CompoundControl HitArea using CompoundControl.Add... rather than using
        /// the Add<control type>(...) in the Controls object.
        /// </summary>
        /// 
        public Controls SubControls { get; set; }

        /// <summary>
        /// Use SubType to distinguish between your own types of Compound controls when
        /// creating more than one type of CompoundControl, like different types of
        /// panels of controls.
        /// </summary>
        public int SubType { get; set; }

        private Controls controls;

        public CompoundControl(Controls controls, Rect AppSize, Image ClickArea, int Id, int subType, Grid gridControls, Image[] imageList, Rect HitArea)
        {
            this.Id = Id;
            GridControls = gridControls;
            this.controls = controls;
            SubType = subType;

            if (imageList != null)
            {
                this.HitArea = new Rect(HitArea.Left, HitArea.Top, imageList[0].ActualWidth, imageList[0].ActualHeight);
            }
            else
            {
                this.HitArea = new Rect(HitArea.Left, HitArea.Top, HitArea.Width, HitArea.Height);
            }

            CopyImages(imageList);
            SubControls = new Controls(AppSize, ClickArea);
            ControlSizing = new ControlSizing(controls, this);
        }

        public int Handle(EventType eventType, PointerRoutedEventArgs e)
        {
            switch (eventType)
            {
                case EventType.POINTER_MOVED:
                    return PointerMoved(e);
                case EventType.POINTER_PRESSED:
                    PointerPressed(e);
                    break;
                case EventType.POINTER_RELEASED:
                    PointerReleased(e);
                    break;
                case EventType.POINTER_TAPPED:
                    break;
                case EventType.POINTER_RIGHT_TAPPED:
                    break;
                case EventType.POINTER_WHEEL_CHANGED:
                    break;
            }
            return -1;
        }

        private int PointerMoved(PointerRoutedEventArgs e)
        {
            return -1;
        }

        public void PointerPressed(PointerRoutedEventArgs e)
        {

        }

        public void PointerReleased(PointerRoutedEventArgs e)
        {

        }

        public StaticImage AddStaticImage(int Id, Grid gridControls, Image[] imageList, Point position)
        {
            StaticImage staticImage = new StaticImage(controls, Id, gridControls, imageList,
                new Point(this.HitArea.Left + position.X, this.HitArea.Top + position.Y));
            SubControls.ControlsList.Add(staticImage);
            return staticImage;
        }

        public Rotator AddRotator(int Id, Grid gridControls, Image[] imageList, Point position)
        {
            Rotator imageSelector = new Rotator(controls, Id, gridControls, imageList, 
                new Point(this.HitArea.Left + position.X, this.HitArea.Top + position.Y));
            SubControls.ControlsList.Add(imageSelector);
            return imageSelector;
        }

        public Indicator AddIndicator(int Id, Grid gridControls, Image[] imageList, Point position)
        {
            Indicator control = new Indicator(controls, Id, gridControls, imageList, 
                new Point(this.HitArea.Left + position.X, this.HitArea.Top + position.Y));
            SubControls.ControlsList.Add(control);
            return control;
        }

        public Knob AddKnob(int Id, Grid gridControls, Image[] imageList, Point position,
            int MinValue = 0, int MaxValue = 127, int AngleStart = 45, int AngleEnd = 315, Double StepSize = 1)
        {
            Knob control = new Knob(controls, Id, gridControls, imageList,
                new Point(this.HitArea.Left + position.X, this.HitArea.Top + position.Y),
                MinValue, MaxValue, AngleStart, AngleEnd, StepSize);
            SubControls.ControlsList.Add(control);
            return control;
        }

        public DigitalDisplay AddDigitalDisplay(int Id, Grid gridControls, Image[] imageList, Point position, int numberOfDigits, int numberOfDecimals)
        {
            DigitalDisplay digitalDisplay = new DigitalDisplay(controls, Id, gridControls, imageList,
                new Point(this.HitArea.Left + position.X, this.HitArea.Top + position.Y),
                numberOfDigits, numberOfDecimals);
            SubControls.ControlsList.Add(digitalDisplay);
            return digitalDisplay;
        }

        public VerticalSlider AddVerticalSlider(int Id, Grid gridControls, Image[] imageList, 
            Rect hitArea,
            int MinValue = 0, int MaxValue = 127)
        {
            VerticalSlider control = new VerticalSlider(controls, Id, gridControls, imageList,
                new Rect(this.HitArea.Left + hitArea.Left, this.HitArea.Top + hitArea.Top, hitArea.Width, hitArea.Height),
                MinValue, MaxValue);
            SubControls.ControlsList.Add(control);
            return control;
        }

        public HorizontalSlider AddHorizontalSlider(int Id, Grid gridControls, Image[] imageList, Rect hitArea,
            int MinValue = 0, int MaxValue = 127)
        {
            HorizontalSlider control = new HorizontalSlider(controls, Id, gridControls, imageList,
                new Rect(this.HitArea.Left + hitArea.Left, this.HitArea.Top + hitArea.Top, hitArea.Width, hitArea.Height),
                MinValue, MaxValue);
            SubControls.ControlsList.Add(control);
            return control;
        }

        public Joystick AddJoystick(int Id, Grid gridControls, Image[] imageList, Rect hitArea,
            int MinValueX = 0, int MaxValueX = 127, int MinValueY = 0, int MaxValueY = 127)
        {
            Joystick control = new Joystick(controls, Id, gridControls, imageList,
                new Rect(this.HitArea.Left + hitArea.Left, this.HitArea.Top + hitArea.Top, hitArea.Width, hitArea.Height),
                MinValueX, MaxValueX, MinValueY, MaxValueY);
            SubControls.ControlsList.Add(control);
            return control;
        }

        public Label AddLabel(int Id, Grid gridControls, Rect hitArea, string text, int fontSize,
            HorizontalAlignment alignment = HorizontalAlignment.Center, TextAlignment textAlignment = TextAlignment.Center,
            ControlTextWeight textWeight = ControlTextWeight.NORMAL, TextWrapping textWrapping = TextWrapping.NoWrap, Brush foreground = null)
        {
            Label control = new Label(controls, Id, gridControls,
                new Rect(this.HitArea.Left + hitArea.Left, this.HitArea.Top + hitArea.Top, hitArea.Width, hitArea.Height),
                text, fontSize, textAlignment, textWeight);
            SubControls.ControlsList.Add(control);
            return control;
        }

        public Graph AddGraph(int Id, Grid gridControls, Image[] imageList, Point position, Brush Color, int LineWidth = 2)
        {
            Graph control = new Graph(controls, Id, gridControls, imageList, 
                new Point(this.HitArea.Left + position.X, this.HitArea.Top + position.Y), Color, LineWidth);
            SubControls.ControlsList.Add(control);
            return control;
        }

        //public void Tapped(object sender, TappedEventHandler e)
        //{

        //}
    }
}
