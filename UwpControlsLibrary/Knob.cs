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
    /// Knob class.
    /// <summary>
    /// The Knob klass accepts one image of a knob. Area around the knob must be transparent. The knob indicator
    /// must point down. When creating the knob object set position where the knob center should be and supply the
    /// image in an image array of one. Also supply minimum and maximum value as well ass start and end angles.
    /// If a second image is supplied it must be the same size, be transparent except for marikings and acts as
    /// a background image rather than painting the markings on the application background image.
    /// </summary>
    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    public class Knob : ControlBase
    {
        //public Grid gridControls;
        //public Image Image;
        //public Point CenterPoint;
        public int AngleStart; // Image must have marker facing down. 
        public int AngleEnd;   // Values range from lowest at AngleStart and highest at AngleEnd.
        public int Value { get { return value; } set { this.value = value; SetRotationFromValue(); } }
        public Double StepSize;
        private int value;
        private double doubleValue;
        private double startPosition;
        private Double PreviousPosition;
        private bool wrapsAround;

        public Knob(Controls controls, int Id, Grid gridControls, Image[] imageList, Point CenterPoint, 
            int MinValue = 0, int MaxValue = 127,
            int AngleStart = 45, int AngleEnd = 315, Double StepSize = 1, bool WrapsAround = false)
        {
            this.AngleStart = AngleStart;
            this.AngleEnd = AngleEnd;
            this.MinValue = MinValue;
            this.MaxValue = MaxValue;
            this.value = 0;
            this.startPosition = 0;
            PreviousPosition = 0;
            this.StepSize = StepSize;
            this.wrapsAround = WrapsAround;
            this.Id = Id;
            GridControls = gridControls;
            HitArea = new Rect(
                CenterPoint.X - imageList[0].ActualWidth / 2,
                CenterPoint.Y - imageList[0].ActualHeight / 2,
                imageList[0].ActualWidth, imageList[0].ActualHeight);
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
            this.HitArea = new Rect(knob.HitArea.Left, knob.HitArea.Top, knob.HitArea.Width, knob.HitArea.Height);
            CopyImages(knob.ImageList);
            this.ImageList = null;
            this.MaxValue = knob.MaxValue;
            this.MinValue = knob.MinValue;
            this.Tag = knob.Tag;
            this.TextBlock = null;
            this.value = knob.value;
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
            if (wrapsAround)
            {
                if (doubleValue > MaxValue)
                {
                    doubleValue -= MaxValue;
                }
                if (doubleValue < MinValue)
                {
                    doubleValue += MaxValue;
                }
            }
            else
            {
                if (doubleValue > MaxValue)
                {
                    doubleValue = MaxValue;
                }
                if (doubleValue < MinValue)
                {
                    doubleValue = MinValue;
                }
            }
            value = (int)Math.Round(doubleValue, 0);
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

        public void HandleEvent(Point pointerPosition, EventType eventType, List<PointerButton> pointerButtons, int delta = 0)
        {
            switch (eventType)
            {
                case EventType.POINTER_MOVED:
                    HandlePointerMovedEvent(pointerPosition, pointerButtons);
                    break;
                case EventType.POINTER_PRESSED:
                    HandlePointerPressedEvent(pointerPosition, pointerButtons);
                    break;
                case EventType.POINTER_WHEEL_CHANGED:
                    HandlePointerWheelChangedEvent(pointerButtons, delta);
                    break;
            }
        }

        public int HandlePointerMovedEvent(Point pointerPosition, List<PointerButton> pointerButtonStates)
        {
            if (pointerButtonStates != null && pointerButtonStates.Contains(PointerButton.LEFT))
            {
                SetValue(pointerPosition);
            }
            return value;
        }

        public void HandlePointerPressedEvent(Point pointerPosition, List<PointerButton> pointerButtonStates)
        {
            if (pointerButtonStates != null && pointerButtonStates.Contains(PointerButton.LEFT))
            {
                startPosition = pointerPosition.Y;
            }
        }

        public void HandlePointerWheelChangedEvent(List<PointerButton> pointerButtonStates, int delta)
        {
            if (pointerButtonStates.Contains(PointerButton.LEFT))
            {
                delta *= 4;
            }
            if (pointerButtonStates.Contains(PointerButton.RIGHT))
            {
                delta *= 8;
            }
            value += delta;
            if (wrapsAround)
            {
                value = value > MaxValue ? value - MaxValue : value;
                value = value < MinValue ? value + MaxValue : value;
            }
            else
            {
                value = value > MaxValue ? MaxValue : value;
                value = value < MinValue ? MinValue : value;
            }
            doubleValue = value;
            SetRotationFromValue();
            //return value;
        }
    }
}
