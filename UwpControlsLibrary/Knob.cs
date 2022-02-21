using System;
using System.Collections.Generic;
using Windows.Foundation;
using Windows.UI.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace UwpControlsLibrary
{
    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    /// Knob classe.
    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    public class Knob : ControlBase
    {
        //public Grid GridMain;
        //public Image Image;
        //public Point CenterPoint;
        public int AngleStart; // Image must have marker facing down. 
        public int AngleEnd;   // Values range from lowest at AngleStart and highest at AngleEnd.
        public int Value { get { return value; } set { this.value = value; SetRotationFromValue(); } }
        public Double StepSize;
        public Boolean HighPrecision;

        private int value;
        private double doubleValue;
        private double startPosition;
        private Double PreviousPosition;

        public Knob(Controls controls, int Id, Grid gridMain, Image[] imageList, Point CenterPoint, 
            Boolean HighPrecision = true, int MinValue = 0, int MaxValue = 127,
            int AngleStart = 45, int AngleEnd = 315, Double StepSize = 1)
        {
            this.HighPrecision = HighPrecision;
            this.AngleStart = AngleStart;
            this.AngleEnd = AngleEnd;
            this.MinValue = MinValue;
            this.MaxValue = MaxValue;
            this.value = 0;
            this.startPosition = 0;
            PreviousPosition = 0;
            this.StepSize = StepSize;
            this.Id = Id;
            GridControls = gridMain;
            HitArea = new Rect(
                CenterPoint.X - imageList[0].ActualWidth / 2,
                CenterPoint.Y - imageList[0].ActualHeight / 2,
                imageList[0].ActualWidth, imageList[0].ActualHeight);

            if (HighPrecision && HitArea.Height < (MaxValue - MinValue + 1))
            {
                Double topCorrection = ((MaxValue - MinValue + 1) - HitArea.Height) / 2;
                HitArea = new Rect(new Point(HitArea.Left, HitArea.Top - topCorrection),
                    new Size(HitArea.Width, (MaxValue - MinValue + 1)));
            }

            CopyImages(imageList);
            ControlSizing = new ControlSizing(controls, this);
            SetRotationFromValue();
        }

        public Knob(Knob knob)
        {
            this.AngleEnd = knob.AngleEnd;
            this.AngleStart = knob.AngleStart;
            this.Id = knob.Id;
            this.ControlSizing = knob.ControlSizing;
            this.GridControls = knob.GridControls;
            this.HighPrecision = knob.HighPrecision;
            this.HitArea = new Rect(knob.HitArea.Left, knob.HitArea.Top, knob.HitArea.Width, knob.HitArea.Height);
            this.HitTarget = knob.HitTarget;
            CopyImages(knob.ImageList);
            this.ImageList = null;
            this.MaxValue = knob.MaxValue;
            this.MinValue = knob.MinValue;
            this.Tag = knob.Tag;
            this.TextBlock = null;
            this.value = knob.value;
        }

        public int PointerMoved(Point mousePosition)
        {
            SetValue(mousePosition);
            return value;
        }

        public void PointerPressed(Point mousePosition)
        {
            Double top = ControlSizing.HitArea.Top;
            Double bottom = ControlSizing.HitArea.Bottom;
            //startPosition = (1 - ((mousePosition.Y - top) / (bottom - top))) * (MaxValue - MinValue + 1);
            startPosition = mousePosition.Y;
        }

        public void PointerReleased(PointerRoutedEventArgs e)
        {

        }

        public int SetValue(Point position)
        {
            Double top = ControlSizing.HitArea.Top;
            Double bottom = ControlSizing.HitArea.Bottom;

            if (Math.Abs(PreviousPosition - position.Y) < 5)
            {
                Double step = PreviousPosition - position.Y;
                value += (int)(step * ((Double)MaxValue - (Double)MinValue) / StepSize);
            }

            PreviousPosition = position.Y;
            double movement = startPosition - position.Y;
            movement *= (MaxValue - MinValue + 1) / HitArea.Height;
            startPosition = position.Y;

            doubleValue += movement;
            if (doubleValue > MaxValue)
            {
                doubleValue = MaxValue;
            }
            if (doubleValue < MinValue)
            {
                doubleValue = MinValue;
            }
            value = (int)Math.Round(doubleValue, 0);
            SetRotationFromValue();
            return value;
        }

        public int PointerWheelChanged(int delta)
        {
            value += (int)(delta * StepSize);
            value = value > MaxValue ? MaxValue : value;
            value = value < MinValue ? MinValue : value;
            doubleValue = value;
            SetRotationFromValue();
            return value;
        }

        public void SetRotationFromValue()
        {
            if (ControlSizing.ImageList[0] != null && ControlGraphicsFollowsValue)
            {
                Double angle = AngleStart + (AngleEnd - AngleStart) * value / (MaxValue - MinValue + 1);

                CompositeTransform compositeTransform = new CompositeTransform();
                compositeTransform.Rotation = angle;
                ControlSizing.ImageList[0].RenderTransformOrigin = new Point(0.5, 0.5);
                ControlSizing.ImageList[0].RenderTransform = compositeTransform;
            }
        }
    }
}
