using System;
using System.Collections.Generic;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;

namespace UwpControlsLibrary
{
    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    /// Slider class.
    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    public class HorizontalSlider : ControlBase
    {
        public Int32 Value { get { return value; } set { this.value = value; SetPositionFromValue(); } }
        public Boolean HighPrecision { get; set; }
        public Point ImageSize { get; set; }
        public Double OriginalImageWidth { get; set; }
        public Double OriginalImageHeight { get; set; }

        private Int32 value;
        public Double RelativeValue;

        public HorizontalSlider(Controls controls, Int32 Id, Grid gridMain, Image[] imageList, Rect HitArea,
            Int32 MinValue = 0, Int32 MaxValue = 127, Boolean HighPrecision = false)
        {
            GridControls = gridMain;
            ImageSize = new Point(imageList[imageList.Length - 1].ActualWidth,
                imageList[imageList.Length - 1].ActualHeight);
            OriginalImageWidth = imageList[imageList.Length - 1].ActualWidth;
            OriginalImageHeight = imageList[imageList.Length - 1].ActualHeight;
            this.HighPrecision = HighPrecision;
            this.MinValue = MinValue;
            this.MaxValue = MaxValue;
            this.Id = Id;
            Double width = HitArea.Width;
            Double height = HitArea.Height;
            HitTarget = true;

            //if (width == 0)
            //{
            //    if (image != null)
            //    {
            //        width = image.ActualWidth;
            //    }
            //}

            //if (height == 0)
            //{
            //    if (image != null)
            //    {
            //        height = image.ActualHeight;
            //    }
            //}

            //this.HitArea = new Rect(HitArea.Left, HitArea.Top, width, height);

            if (imageList == null || imageList.Length < 1 || imageList[0] == null)
            {
                this.HitArea = new Rect(HitArea.Left, HitArea.Top, HitArea.Width, HitArea.Height);
            }
            else
            {
                this.HitArea = new Rect(HitArea.Left, HitArea.Top, imageList[0].ActualWidth, imageList[0].ActualHeight);
            }

            CopyImages(imageList);
            ControlSizing = new ControlSizing(controls, this);
        }

        public Int32 Handle(EventType eventType, PointerRoutedEventArgs e)
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
            }
            return -1;
        }

        private Int32 PointerMoved(PointerRoutedEventArgs e)
        {
            return -1;
        }

        public void PointerPressed(PointerRoutedEventArgs e)
        {

        }

        public void PointerReleased(PointerRoutedEventArgs e)
        {

        }

        public Int32 SetValue(Point position)
        {
            Int32 left = (Int32)(ControlSizing.HitArea.Left + ImageList[ImageList.Length - 1].ActualWidth / 2);
            Int32 right = (Int32)(ControlSizing.HitArea.Right - ImageList[ImageList.Length - 1].ActualHeight / 2);
            if (HighPrecision)
            {
                if (right - left < (MaxValue - MinValue))
                {
                    Int32 correction = ((MaxValue - MinValue) - (right - left)) / 2;
                    right += correction;
                    left -= correction;
                }
            }
            Value = MaxValue - (Int32)(((float)right - (float)position.X) / ((float)right - (float)left) * (1.0 + (float)MaxValue - (float)MinValue + 1));
            Value = Value > MaxValue ? MaxValue : Value;
            Value = Value < MinValue ? MinValue : Value;
            SetPositionFromValue();
            return Value;
        }

        public Int32 PointerWheelChanged(Int32 delta)
        {
            Value += delta;
            Value = Value > MaxValue ? MaxValue : Value;
            Value = Value < MinValue ? MinValue : Value;
            return Value;
        }

        public void SetPositionFromValue()
        {
            if (ControlGraphicsFollowsValue)
            {
                RelativeValue = ((Double)value - (Double)MinValue) / ((Double)MaxValue - (Double)MinValue);
                ControlSizing.Controls.CalculateExtraMargins(ControlSizing.Controls.AppSize);
                ControlSizing.UpdatePositions();
            }
        }
    }
}
