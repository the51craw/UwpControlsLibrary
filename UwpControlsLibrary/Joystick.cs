using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using static UwpControlsLibrary.ControlBase;

namespace UwpControlsLibrary
{
    /// <summary>
    /// A Joystick uses a mandatory knob image, an optional background image
    /// and an optional set of shaft part images to mimic a joystick.
    /// The shaft part images should all be the same size and the shaft must
    /// start at the center of the image. Thre rest of the image must be
    /// transparent. The first shaft part must not be longer than that it would
    /// be covered byt the knob when in zero position. The last shaft part
    /// must not be shorter than to reach up under the knob when in full
    /// X/Y position. Depending on your graphics you might have to supply
    /// some intermediate shaft parts. The shaft parts will automatically
    /// be set visible as needed, and rotated to follow the knob. See the
    /// SampleApplication for implementation.
    /// </summary>

    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    /// Joystick class.
    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    public class Joystick : ControlBase
    {
        public Double OriginalImageWidth { get; set; }
        public Double OriginalImageHeight { get; set; }
        public Int32 MinValueX { get; set; }
        public Int32 MaxValueX { get; set; }
        public Int32 MinValueY { get; set; }
        public Int32 MaxValueY { get; set; }
        public Int32 ValueX { get { return valueX; } set { this.valueX = value; SetPositionFromValues(); } }
        public Int32 ValueY { get { return valueY; } set { this.valueY = value; SetPositionFromValues(); } }
        public Double[] StickRelativeSize { get; set; }

        private Int32 valueX;
        private Int32 valueY;
        public Double RelativeValueX;
        public Double RelativeValueY;

        /// <summary>
        /// ImageList must be: Background, shaft parts (any order) and last the knob.
        /// </summary>
        /// <param name="controls"></param>
        /// <param name="Id"></param>
        /// <param name="gridControls"></param>
        /// <param name="imageList"></param>
        /// <param name="hitArea"></param>
        /// <param name="MinValueX"></param>
        /// <param name="MaxValueX"></param>
        /// <param name="MinValueY"></param>
        /// <param name="MaxValueY"></param>
        public Joystick(Controls controls, Int32 Id, Grid gridControls, Image[] imageList, Rect hitArea,
            Int32 MinValueX = 0, Int32 MaxValueX = 127, Int32 MinValueY = 0, Int32 MaxValueY = 127)
        {
            this.MinValueX = MinValueX;
            this.MaxValueX = MaxValueX;
            this.MinValueY = MinValueY;
            this.MaxValueY = MaxValueY;
            OriginalImageWidth = imageList[imageList.Length - 1].ActualWidth;
            OriginalImageHeight = imageList[imageList.Length - 1].ActualHeight;
            this.Id = Id;
            GridControls = gridControls;
            Double width = hitArea.Width;
            Double height = hitArea.Height;
            HitArea = new Rect(hitArea.Left, hitArea.Top, width, height);
            CopyImages(imageList);
            ControlSizing = new ControlSizing(controls, this);
            ValueX = 0;
            ValueY = 0;
        }

        public Int32[] SetValue(Point position)
        {
            Int32[] value = new Int32[2];
            Int32 top = (Int32)ControlSizing.HitArea.Top;
            Int32 bottom = (Int32)ControlSizing.HitArea.Bottom;

            Int32 left = (Int32)ControlSizing.HitArea.Left;
            Int32 right = (Int32)ControlSizing.HitArea.Right;
            ValueX = MaxValueX - (Int32)(((float)right - (float)position.X) / ((float)right - (float)left) * (1.0 + (float)MaxValueX - (float)MinValueX + 1));
            ValueY = MaxValueY - (Int32)(((float)position.Y - (float)top) / ((float)bottom - (float)top) * (1.0 + (float)MaxValueY - (float)MinValueY + 1));

            ValueX = ValueX > MaxValueX ? MaxValueX : ValueX;
            ValueX = ValueX < MinValueX ? MinValueX : ValueX;
            ValueY = ValueY > MaxValueY ? MaxValueY : ValueY;
            ValueY = ValueY < MinValueY ? MinValueY : ValueY;

            SetPositionFromValues();

            value[0] = ValueX;
            value[1] = ValueY;
            return value;
        }

        public int[] HandleEvent(PointerRoutedEventArgs e, EventType eventType, Point pointerPosition, List<PointerButton> pointerButtonStates)
        {
            switch (eventType)
            {
                case EventType.POINTER_MOVED:
                    if (pointerButtonStates.Contains(PointerButton.LEFT))
                    {
                        return SetValue(pointerPosition);
                    }
                    break;
            }
            return null;
        }

        public void SetPositionFromValues()
        {
            RelativeValueX = ((Double)valueX - (Double)MinValueX) / ((Double)MaxValueX - (Double)MinValueX);
            RelativeValueY = (1 - ((Double)valueY - (Double)MinValueY) / ((Double)MaxValueY - (Double)MinValueY));
            ControlSizing.Controls.CalculateExtraMargins(Controls.AppSize);
            ControlSizing.UpdatePositions();
            SetRotationFromValue();
        }

        public void SetRotationFromValue()
        {
            if (ImageList != null && ControlGraphicsFollowsValue)
            {
                Double length = Math.Sqrt(
                    Math.Pow(valueX / ((Double)MaxValueX - (Double)MinValueX), 2) + 
                    Math.Pow(valueY / ((Double)MaxValueY - (Double)MinValueY), 2));
                if (ImageList.Length > 4)
                {
                    ImageList[3].Visibility = length > ControlSizing.RelativeHitArea.Height * 2 ?
                        Visibility.Visible : Visibility.Collapsed;
                }
                if (ImageList.Length > 3)
                {
                    ImageList[2].Visibility = length > ControlSizing.RelativeHitArea.Height ?
                        Visibility.Visible : Visibility.Collapsed;
                }
                if (ImageList.Length > 2)
                {
                    ImageList[1].Visibility = length > ControlSizing.RelativeHitArea.Height / 2 ?
                        Visibility.Visible : Visibility.Collapsed;
                }

                Double radians = Math.Atan2(valueX, valueY);
                Int32 angle = (Int32)(radians * 360 / (2 * Math.PI));

                CompositeTransform compositeTransform = new CompositeTransform();
                compositeTransform.Rotation = angle;
                for (Int32 i = 1; i < ImageList.Length - 1; i++)
                {
                    ImageList[i].RenderTransformOrigin = new Point(0.5, 0.5);
                    ImageList[i].RenderTransform = compositeTransform;
                }
            }
        }
    }
}
