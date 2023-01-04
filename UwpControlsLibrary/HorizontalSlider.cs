using System;
using System.Collections.Generic;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;

namespace UwpControlsLibrary
{
    /// <summary>
	/// A Slider uses an image of the slider handle to move.
	/// The optional background will decide the sliders hit area if present,
	/// otherwise the supplied hitarea will be used.
	/// In other words, you can have the background image supplied with the
	/// slider control, or you can have it painted on the application background image.
	/// Note that if you supply a background image with the slider control and it is
	/// higher than the handle image, the handle will be vertically centered over
	/// the background image, as would be expected.
    /// </summary>
    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    /// Slider class.
    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    public class HorizontalSlider : ControlBase
    {
        public Int32 Value { get { return value; } set { this.value = value; SetPositionFromValue(); } }
        public Point ImageSize { get; set; }
        public Double OriginalImageWidth { get; set; }
        public Double OriginalImageHeight { get; set; }

        private Int32 value;
        public Double RelativeValue;

        public HorizontalSlider(Controls controls, Int32 Id, Grid gridMain, Image[] imageList,
            Rect HitArea, Int32 MinValue = 0, Int32 MaxValue = 127)
        {
            GridControls = gridMain;
            ImageSize = new Point(imageList[imageList.Length - 1].ActualWidth,
                imageList[imageList.Length - 1].ActualHeight);
            OriginalImageWidth = imageList[imageList.Length - 1].ActualWidth;
            OriginalImageHeight = imageList[imageList.Length - 1].ActualHeight;
            this.MinValue = MinValue;
            this.MaxValue = MaxValue;
            this.Id = Id;

            if (imageList.Length == 1)
            {
                // We have only the handle image, use HitArea from arguments:
                this.HitArea = new Rect(HitArea.Left, HitArea.Top, HitArea.Width, HitArea.Height);
            }
            else
            {
                // Use first image as HitArea:
                this.HitArea = new Rect(HitArea.Left, HitArea.Top, imageList[0].ActualWidth, imageList[0].ActualHeight);
            }

            CopyImages(imageList);
            ControlSizing = new ControlSizing(controls, this);
        }

        public Int32 SetValue(Point position)
        {
            Int32 left = (Int32)(ControlSizing.HitArea.Left + ImageList[ImageList.Length - 1].ActualWidth / 2);
            Int32 right = (Int32)(ControlSizing.HitArea.Right - ImageList[ImageList.Length - 1].ActualHeight / 2);
            Value = MaxValue - (Int32)(((float)right - (float)position.X) / ((float)right - (float)left) * (1.0 + (float)MaxValue - (float)MinValue + 1));

            Value = Value > MaxValue ? MaxValue : Value;
            Value = Value < MinValue ? MinValue : Value;
            SetPositionFromValue();
            return Value;
        }

        public void SetPositionFromValue()
        {
            if (ControlGraphicsFollowsValue)
            {
                RelativeValue = ((Double)value - (Double)MinValue) / ((Double)MaxValue - (Double)MinValue);
                ControlSizing.Controls.CalculateExtraMargins(Controls.AppSize);
                ControlSizing.UpdatePositions();
            }
        }

        public void HandleEvent(PointerRoutedEventArgs e, EventType eventType, Point pointerPosition, List<PointerButton> pointerButtonStates, int delta)
        {
            switch (eventType)
            {
                case EventType.POINTER_MOVED:
                    HandlePointerMovedEvent(pointerPosition, pointerButtonStates);
                    break;
                case EventType.POINTER_WHEEL_CHANGED:
                    HandlePointerWheelChangedEvent(pointerButtonStates, delta);
                    break;
            }
        }

        public void HandlePointerMovedEvent(Point pointerPosition, List<PointerButton> pointerButtonStates)
        {
            if (pointerButtonStates.Contains(PointerButton.LEFT))
            {
                int value = SetValue(pointerPosition);
                SetPositionFromValue();
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
            value = value > MaxValue ? MaxValue : value;
            value = value < MinValue ? MinValue : value;
            double Value = value;
            SetPositionFromValue();
        }
    }
}
