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
        public Point ImageSize { get; set; }
        public Double OriginalImageWidth { get; set; }
        public Double OriginalImageHeight { get; set; }

        private Int32 value;
        public Double RelativeValue;

        public HorizontalSlider(Controls controls, Int32 Id, Grid gridMain, Image[] imageList, Rect HitArea,
            Int32 MinValue = 0, Int32 MaxValue = 127)
        {
            GridControls = gridMain;
            ImageSize = new Point(imageList[imageList.Length - 1].ActualWidth,
                imageList[imageList.Length - 1].ActualHeight);
            OriginalImageWidth = imageList[imageList.Length - 1].ActualWidth;
            OriginalImageHeight = imageList[imageList.Length - 1].ActualHeight;
            this.MinValue = MinValue;
            this.MaxValue = MaxValue;
            this.Id = Id;
            Double width = HitArea.Width;
            Double height = HitArea.Height;

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

        //public Int32 Handle(EventType eventType, PointerRoutedEventArgs e)
        //{
        //    switch (eventType)
        //    {
        //        case EventType.POINTER_MOVED:
        //            return PointerMoved(e);
        //        case EventType.POINTER_PRESSED:
        //            PointerPressed(e);
        //            break;
        //        case EventType.POINTER_RELEASED:
        //            PointerReleased(e);
        //            break;
        //    }
        //    return -1;
        //}

        //private Int32 PointerMoved(PointerRoutedEventArgs e)
        //{
        //    return -1;
        //}

        //public void PointerPressed(PointerRoutedEventArgs e)
        //{

        //}

        //public void PointerReleased(PointerRoutedEventArgs e)
        //{

        //}

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

        public Int32 PointerWheelChanged(Int32 delta)
        {
            Value += delta;
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

        public void SetDeSelected()
        {
        }

        public void HandleEvent(PointerRoutedEventArgs e, EventType eventType, Point pointerPosition, List<PointerButton> pointerButtonStates, int delta)
        {
            switch (eventType)
            {
                case EventType.POINTER_MOVED:
                    HandlePointerMovedEvent(pointerPosition, pointerButtonStates);
                    break;
                case EventType.POINTER_PRESSED:
                    HandlePointerPressedEvent(e);
                    break;
                case EventType.POINTER_RELEASED:
                    HandlePointerReleasedEvent(pointerPosition, pointerButtonStates);
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

        public void HandlePointerPressedEvent(PointerRoutedEventArgs e)
        {
        }

        public void HandlePointerReleasedEvent(Point pointerPosition, List<PointerButton> pointerButtonStates)
        {
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
            //return value;
        }

        public void HandlePointerTapped(PointerRoutedEventArgs e)
        {
        }

        public void HandlePointerRightTapped(PointerRoutedEventArgs e)
        {
        }
    }
}
