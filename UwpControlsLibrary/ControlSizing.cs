using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Media;

namespace UwpControlsLibrary
{
    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    // This class handles sizing for all types of controls.
    // Include a sizing object in all types of controls and call at startup
    // and when application window size has changed.
    // 
    // Note: Each control is detected from mouse position over imgClickArea. Each control has a method IsHit() to
    // detect when mouse is over the control, even if the control has something covering the imgClickArea and thus
    // has its own detection. imgClickArea_MouseMoved event handles this via each type of control.
    // Because the application window can be resized, keeping its X/Y ratio (thus causing extra margins!) we cannot
    // rely fully on pixel coordinages. We therefore have two types of hit areas stored in each control type:
    //
    // OriginalHitArea - Holds the relative Rect in respect to the size of imgClickArea, and are always less than 
    // one in all values, Left, Top, Widht and Height.
    //
    // HitArea - Holds the actual pixel Rect and is therfore always recalculated at startup and when the application
    // window changes its size. The calculation also considers any extra margin. It can therefore be used to test
    // for hit using mouse coordinates.
    // 
    // When creating controls there are two different ways:
    // 
    // Controls based on an image uses the size of the image to calculate the hit area.
    // 
    // Other controls, including sliders even though those has an image of the handle, has to be defined at design
    // time. The designer has to keep imgBackground (that has the exact same size as the imgClickArea since it is
    // actually the same image) open in e.g. Ms Paint and get coordinates by noting the mouse position.
    // 
    // Also note that knobs and sliders supplies integer values, and therefore should supply ample hight in order
    // to supply all possible values. The height can be automatically adjusted if a flag for precision is set.
    //
    // Control creation:
    // 1) Constructor is fed coordinates measured from the background image.
    // 2) If a control contains an image, it needs to be loaded before size can be determined. Use the loaded event.
    // 3) MainPage initiation loops checking all controls until all has set flag IsSized.
    // 4) MainPage calls the SizeChanged event to trigger positioning and resizing of all controls.
    // Note! All images are initially set to Stretch = Stretch.None in order to get correct size. When sizing is done
    // and just before setting flag IsSized, set Stretch to Stretch.Stretch in order to follow app window sizing.
    // 
    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    public enum PositionType
    {
        UPPERLEFTCORNER,
        CENTER,
    }

    public class ControlSizing
    {
        public Object Owner;
        public Controls Controls;
        public Image ImgClickArea;
        public Rect HitArea;

        /// <summary>
        /// Image relative size of imgClickArea original size.
        /// This is used to resize the image the same amount
        /// as the imgClickArea is resezed when user changes app size.
        /// </summary>
        public Rect RelativeHitArea;               // Original size of HitArea expressed as part of imgClickArea (values are within 0 to 1)
        public Double RelativeFontsize;
        public Point Position;
        public PositionType PositionType;
        public Point RelativeMousePosition;
        public Image[] ImageList;
        public TextBlock TextBlock;

        public Octave[] Octaves;

        /// <summary>
        /// Constructor for most controls except
        /// Keyboard
        /// </summary>
        /// <param name="controls"></param>
        /// <param name="Owner"></param>
        public ControlSizing(Controls controls, Object Owner)
        {
            Controls = controls;
            Init(Owner, ((ControlBase)Owner).HitArea,((ControlBase)Owner).ImageList, 
                ((ControlBase)Owner).TextBlock);
        }

        /// <summary>
        /// Constructor for Keyboad:
        /// </summary>
        /// <param name="controls"></param>
        /// <param name="Owner"></param>
        /// <param name="Id"></param>
        /// <param name="WhiteKey"></param>
        /// <param name="BlackKey"></param>
        /// <param name="X"></param>
        /// <param name="Y"></param>
        /// <param name="Octaves"></param>
        public ControlSizing(Controls controls, Object Owner, int Id, Image[] imageList, Double X, Double Y, Octave[] Octaves)
        {
            Controls = controls;
            this.Octaves = Octaves;
            InitKeyboard(Owner, X, Y);
        }

        private void Init(Object Owner, Rect hitArea, Image[] imageList, TextBlock textBlock)
        {
            this.Owner = Owner;
            this.ImgClickArea = Controls.imgClickArea;

            // DigitalDisplay adds its images before initializing its ControlSizing
            // since background is optional.
            if (imageList != null)
            {
                if (Owner.GetType() != typeof(DigitalDisplay))
                //{
                //    ((DigitalDisplay)Owner).DigitRelativeSize =
                //        new Size(((DigitalDisplay)Owner).ImageList[0].ActualWidth / ((ControlBase)Owner).GridControls.ActualWidth,
                //        ((DigitalDisplay)Owner).ImageList[0].ActualHeight / ((ControlBase)Owner).GridControls.ActualHeight);
                //}
                //else
                {
                    this.ImageList = imageList;
                    foreach (Image img in ImageList)
                    {
                        ((ControlBase)Owner).GridControls.Children.Add(img);
                    }
                }
            }

            if (textBlock != null)
            {
                this.TextBlock = textBlock;
                RelativeFontsize = ((ControlBase)Owner).OriginalFontSize / ((ControlBase)Owner).GridControls.ActualHeight;
                ((ControlBase)Owner).GridControls.Children.Add(textBlock);
            }

            RelativeHitArea = new Rect(
                hitArea.Left / Controls.OriginalWidth,
                hitArea.Top / Controls.OriginalHeight,
                hitArea.Width / Controls.OriginalWidth,
                hitArea.Height / Controls.OriginalHeight);
        }

        private void InitKeyboard(Object Owner, Double X, Double Y, Double Width = 0, Double Height = 0, Double RelativePosition = 0,
            PositionType PositionType = PositionType.UPPERLEFTCORNER, Image[] ImageList = null, TextBlock TextBlock = null)
        {
            this.Owner = Owner;
            this.ImgClickArea = Controls.imgClickArea;
            this.ImageList = ImageList;
            this.PositionType = PositionType;
            if (ImageList != null)
            {
                this.ImageList = ImageList;
            }

            Position = new Point(X, Y);
                Double width = (((Keyboard)Owner).Octaves.Length * 7 + 1) * ((Keyboard)Owner).WhiteKeySize.Width;
                HitArea = new Rect(X, Y, width, ((Keyboard)Owner).WhiteKeySize.Height);

            RelativeHitArea = new Rect(
                HitArea.Left / Controls.OriginalWidth,
                HitArea.Top / Controls.OriginalHeight,
                HitArea.Width / Controls.OriginalWidth,
                HitArea.Height / Controls.OriginalHeight);

            for (int octave = 0; octave < ((Keyboard)Owner).Octaves.Length; octave++)
            {
                for (int key = 0; key < ((Keyboard)Owner).Octaves[octave].Keys.Length; key++)
                {
                    if (((Keyboard)Owner).Octaves[octave].Keys[key].GetType() == typeof(WhiteKey))
                    {
                        for (int i = 0; i < ((Keyboard)Owner).Octaves[octave].Keys[key].Images.Length; i++)
                        {
                            ((Keyboard)Owner).GridControls.Children.Add(((Keyboard)Owner).Octaves[octave].Keys[key].Images[i]);
                        }
                    }
                }
                for (int key = 0; key < ((Keyboard)Owner).Octaves[octave].Keys.Length; key++)
                {
                    if (((Keyboard)Owner).Octaves[octave].Keys[key].GetType() == typeof(BlackKey))
                    {
                        for (int i = 0; i < ((Keyboard)Owner).Octaves[octave].Keys[key].Images.Length; i++)
                        {
                            ((Keyboard)Owner).GridControls.Children.Add(((Keyboard)Owner).Octaves[octave].Keys[key].Images[i]);
                        }
                    }
                }
            }
        }

        public void UpdatePositions()
        {
            HitArea = new Rect(
                RelativeHitArea.Left * ImgClickArea.ActualWidth,
                RelativeHitArea.Top * ImgClickArea.ActualHeight,
                RelativeHitArea.Width * ImgClickArea.ActualWidth,
                RelativeHitArea.Height * ImgClickArea.ActualHeight);

            // Margins from window borders (app window might have other ratio thus leaveing extra space).
            Double left = Controls.ExtraMarginX + HitArea.Left;
            Double top = Controls.ExtraMarginY + HitArea.Top;
            Double right = Controls.ExtraMarginX + ImgClickArea.ActualWidth - HitArea.Right;
            Double bottom = Controls.ExtraMarginY + ImgClickArea.ActualHeight - HitArea.Bottom;

            if (Owner.GetType() == typeof(HorizontalSlider))
            {
                // Set the background:
                if (((HorizontalSlider)Owner).ImageList[0] != null)
                {
                    ((HorizontalSlider)Owner).ImageList[0].Margin = new Thickness(left, top, right, bottom);
                }

                // See VerticalSlider for explanation.
                Double sizeFactor = ImgClickArea.ActualWidth / Controls.OriginalWidth;
                Double ResizedImageWidth = ((HorizontalSlider)Owner).OriginalImageWidth * sizeFactor;
                Double handleSideMargin = (RelativeHitArea.Height * ImgClickArea.ActualHeight - 
                    ((HorizontalSlider)Owner).ImageSize.Y * sizeFactor) / 2;
                Double leftOffset = (HitArea.Width - ResizedImageWidth) * (((HorizontalSlider)Owner).RelativeValue);
                Double rightOffset = (HitArea.Width - ResizedImageWidth) * (1 - ((HorizontalSlider)Owner).RelativeValue);

                // Adjust the margins with the calculated offsets:
                left += leftOffset;
                right += rightOffset;
                top += handleSideMargin;
                bottom += handleSideMargin;
                ((HorizontalSlider)Owner).ImageList[ImageList.Length - 1].Margin = new Thickness(left, top, right, bottom);
            }
            else if (Owner.GetType() == typeof(VerticalSlider))
            {
                // Set the background:
                if (((VerticalSlider)Owner).ImageList[0] != null)
                {
                    ((VerticalSlider)Owner).ImageList[0].Margin = new Thickness(left, top, right, bottom);
                }

                // Image size is not the same as HitArea, so we must do slider specific calculations
                // to get correct margins for the slider handle. It is also depending on orientation:
                // We need a factor for relative size compared to original size for the vertical calculations:
                Double sizeFactor = ImgClickArea.ActualHeight / Controls.OriginalHeight;

                // Calculate the resized hight of the handle image:
                Double ResizedImageHeight = ((VerticalSlider)Owner).OriginalImageHeight * sizeFactor; // / ImgClickArea.ActualHeight * Controls.OriginalHeight;

                // Calculate the margin between the handle side and the HitArea left and right margins
                // (shared betwee left and right side):
                Double handleSideMargin = (RelativeHitArea.Width * ImgClickArea.ActualWidth -
                    ((VerticalSlider)Owner).ImageSize.X * sizeFactor) / 2;

                // Calculate top and bottom offset for the handle within the HitArea depending on slider value:
                Double topOffset = (HitArea.Height - ResizedImageHeight) * (1 - ((VerticalSlider)Owner).RelativeValue);
                Double bottomOffset = (HitArea.Height - ResizedImageHeight) * (((VerticalSlider)Owner).RelativeValue);

                // Adjust the margins with the calculated offsets:
                left += handleSideMargin;
                right += handleSideMargin;
                top += topOffset;
                bottom += bottomOffset;
                ((VerticalSlider)Owner).ImageList[ImageList.Length - 1].Margin = new Thickness(left, top, right, bottom);
            }

            else if (Owner.GetType() == typeof(Joystick))
            {
                Double sizeFactorX = ImgClickArea.ActualWidth / Controls.OriginalWidth;
                Double ResizedKnobWidth = ((Joystick)Owner).OriginalImageWidth * ImgClickArea.ActualWidth / Controls.OriginalWidth;
                Double resizedValueX = HitArea.Width * ((Joystick)Owner).RelativeValueX;
                Double leftOffset = resizedValueX - ResizedKnobWidth / 2;
                Double rightOffset = HitArea.Width - resizedValueX - ResizedKnobWidth / 2;

                Double sizeFactorY = ImgClickArea.ActualHeight / Controls.OriginalHeight;
                Double ResizedKnobHeight = ((Joystick)Owner).OriginalImageHeight * ImgClickArea.ActualHeight / Controls.OriginalHeight;
                Double resizedValueY = HitArea.Height * (1 - ((Joystick)Owner).RelativeValueY);
                Double topOffset = HitArea.Height - resizedValueY - ResizedKnobHeight / 2;
                Double bottomOffset = resizedValueY - ResizedKnobHeight / 2;

                // The knob
                ((Joystick)Owner).ImageList[ImageList.Length - 1].Margin = new Thickness(left + leftOffset, top + topOffset, right + rightOffset, bottom + bottomOffset);

                // Background image is centered!
                ((Joystick)Owner).ImageList[0].Margin = new Thickness(left, top, right, bottom);

                // Stick
                for (int i = 1; i < ((Joystick)Owner).ImageList.Length - 1; i++)
                {
                    leftOffset = (HitArea.Width - ((Joystick)Owner).ImageList[i].ActualWidth * ImgClickArea.ActualWidth) / 2;
                    rightOffset = leftOffset;
                    topOffset = (HitArea.Height - ((Joystick)Owner).ImageList[i].ActualHeight * ImgClickArea.ActualHeight) / 2;
                    bottomOffset = topOffset;

                    if (((Joystick)Owner).ImageList[i].Visibility == Visibility.Visible)
                    {
                        ((Joystick)Owner).ImageList[i].Margin = new Thickness(left, top, right, bottom);
                    }
                }
            }
            else if (Owner.GetType() == typeof(Indicator))
            {
                if (((Indicator)Owner).ImageList != null && ((Indicator)Owner).ImageList.Length > 0)
                {
                    foreach (Image image in ((Indicator)Owner).ImageList)
                    {
                        image.Margin = new Thickness(left, top, right, bottom);
                    }
                }
            }
            else if (Owner.GetType() == typeof(PopupMenuButton))
            {
                foreach (Image image in ((PopupMenuButton)Owner).ImageList)
                {
                    image.Margin = new Thickness(left, top, right, bottom);
                }
                if (((PopupMenuButton)Owner).TextBlock != null)
                {
                    ((PopupMenuButton)Owner).TextBlock.Margin = new Thickness(left, top, right, bottom);
                    ((PopupMenuButton)Owner).TextBlock.FontSize = RelativeFontsize * ImgClickArea.ActualHeight;
                    if (((PopupMenuButton)Owner).TextAlignment == ControlBase.ControlTextAlignment.LEFT)
                    {
                        ((PopupMenuButton)Owner).TextBlock.Padding = new Thickness(RelativeFontsize * ImgClickArea.ActualHeight, 0, 0, 0);
                    }
                }
            }
            else if (Owner.GetType() == typeof(CompoundControl))
            {
                if (((CompoundControl)Owner).ImageList != null && ((CompoundControl)Owner).ImageList[0] != null)
                {
                    ((CompoundControl)Owner).ImageList[ImageList.Length - 1].Margin = new Thickness(left, top, right, bottom);
                }
                foreach (Object control in ((CompoundControl)Owner).SubControls.ControlsList)
                {
                    ((ControlBase)control).ControlSizing.UpdatePositions();
                }
            }
            else if (Owner.GetType() == typeof(Graph))
            {
                //double relativeWidth = Controls.imgClickArea.ActualWidth / Controls.OriginalWidth;
                //double relativeHeight = Controls.imgClickArea.ActualHeight / Controls.OriginalHeight;

                ((Graph)Owner).Canvas.Margin = new Thickness(
                    left + ((Graph)Owner).LineWidth,
                    top + ((Graph)Owner).LineWidth,
                    right - ((Graph)Owner).LineWidth,
                    bottom - ((Graph)Owner).LineWidth);
                if (((ControlBase)Owner).ImageList != null && ((ControlBase)Owner).ImageList[0] != null)
                {
                    ((ControlBase)Owner).ImageList[ImageList.Length - 1].Margin = new Thickness(left, top, right, bottom);
                }
                ((Graph)Owner).Draw();
            }
            else if (Owner.GetType() == typeof(DigitalDisplay))
            {
                if (((ControlBase)Owner).ImageList[12] != null)
                {
                    ((DigitalDisplay)Owner).ImageList[12].Margin =
                            new Thickness(left, top, right, bottom);
                }

                double width = ((DigitalDisplay)Owner).ImageWidth * Controls.imgClickArea.ActualWidth / Controls.OriginalWidth;

                // Make hitarea for one digit:
                Rect hitArea = new Rect(0, ((DigitalDisplay)Owner).DigitsRelativeOffset.Y * Controls.imgClickArea.ActualHeight,
                    ((DigitalDisplay)Owner).DigitRelativeSize.Width * ImgClickArea.ActualWidth,
                    ((DigitalDisplay)Owner).DigitRelativeSize.Height * ImgClickArea.ActualHeight);

                // Make margin for all digits:
                Double digitLeft = Controls.ExtraMarginX + hitArea.Left;
                Double digitTop = Controls.ExtraMarginY + hitArea.Top;
                Double digitRight = Controls.ExtraMarginX + ImgClickArea.ActualWidth - hitArea.Right * ((DigitalDisplay)Owner).NumberOfDigits;
                Double digitBottom = Controls.ExtraMarginY + ImgClickArea.ActualHeight - hitArea.Bottom;

                // Loop all positions:
                for (int position = 0; position < ((DigitalDisplay)Owner).Digits.Length; position++)
                {
                    // Loop all digits, the minus and the dot:
                    for (int digit = 0; digit < 12; digit++)
                    {
                        ((DigitalDisplay)Owner).Digits[position][digit].Margin =
                                new Thickness(
                                    digitLeft + width * position,
                                    digitTop,
                                    digitRight - width * position
                                        + (((DigitalDisplay)Owner).NumberOfDigits - 1) * width,
                                    digitBottom);
                    }
                }
            }
            else if (Owner.GetType() == typeof(TouchpadKeyboard))
            {
                foreach (Object control in ((TouchpadKeyboard)Owner).BlackKeyList)
                {
                    ((ControlBase)control).ControlSizing.UpdatePositions();
                }
                foreach (Object control in ((TouchpadKeyboard)Owner).WhiteKeyList)
                {
                    ((ControlBase)control).ControlSizing.UpdatePositions();
                }
            }
            else if (Owner.GetType() == typeof(Keyboard))
            {
                Double numberOfWhiteKeysToTheRight = ((Keyboard)Owner).Octaves.Length * 7; // First keys has this many white keys to the right of it. Used in Margin.Right calculation.
                int whiteKeysDrawn = 0;
                Double whiteKeyWidth = ((Keyboard)Owner).WhiteKeySize.Width * ImgClickArea.ActualWidth / Controls.OriginalWidth;
                Double blackKeyWidth = ((Keyboard)Owner).BlackKeySize.Width * ImgClickArea.ActualWidth / Controls.OriginalWidth;
                Double blackKeyHeight = ((Keyboard)Owner).BlackKeySize.Height * ImgClickArea.ActualHeight / Controls.OriginalHeight;
                Double octaveWidth = 8 * whiteKeyWidth;
                for (int octave = 0; octave < ((Keyboard)Owner).Octaves.Length; octave++)
                {
                    for (int key = 0; key < ((Keyboard)Owner).Octaves[octave].Keys.Length; key++)
                    {
                        if (((Keyboard)Owner).Octaves[octave].Keys[key].GetType() == typeof(WhiteKey))
                        {
                            left = Controls.ExtraMarginX + RelativeHitArea.Left * ImgClickArea.ActualWidth +
                                whiteKeyWidth * whiteKeysDrawn++;
                            top = Controls.ExtraMarginY + RelativeHitArea.Top * ImgClickArea.ActualHeight;
                            right = Controls.ExtraMarginX + ImgClickArea.ActualWidth - HitArea.Right +
                                    numberOfWhiteKeysToTheRight * whiteKeyWidth;
                            bottom = Controls.ExtraMarginY + ImgClickArea.ActualHeight - RelativeHitArea.Bottom * ImgClickArea.ActualHeight;
                            for (int i = 0; i < ((Keyboard)Owner).Octaves[octave].Keys[key].Images.Length; i++)
                            {
                                ((Keyboard)Owner).Octaves[octave].Keys[key].Images[i].Margin = new Thickness(left, top, right, bottom);
                            }
                            numberOfWhiteKeysToTheRight--;
                        }
                        else
                        {
                            Double keyOffset = ((Keyboard)Owner).Octaves[octave].Keys[key].RelativeOffset * octaveWidth + octave * (octaveWidth * 7 / 8);
                            left = RelativeHitArea.Left * ImgClickArea.ActualWidth + keyOffset;
                            top = RelativeHitArea.Top * ImgClickArea.ActualHeight;
                            right = ImgClickArea.ActualWidth - left - blackKeyWidth;
                            bottom = ImgClickArea.ActualHeight - top - blackKeyHeight;
                            for (int i = 0; i < ((Keyboard)Owner).Octaves[octave].Keys[key].Images.Length; i++)
                            {
                                ((Keyboard)Owner).Octaves[octave].Keys[key].Images[i].Margin = new Thickness(left, top, right, bottom);
                            }
                        }
                    }
                }
            }
            else // All other control types:
            {
                if (((ControlBase)Owner).ImageList != null && ((ControlBase)Owner).ImageList.Length > 0)
                {
                    foreach (Image image in ((ControlBase)Owner).ImageList)
                    {
                        image.Margin = new Thickness(left, top, right, bottom);
                    }
                }

                if (((ControlBase)Owner).TextBlock != null)
                {
                    ((ControlBase)Owner).TextBlock.FontSize = RelativeFontsize * ImgClickArea.ActualHeight;
                    ((ControlBase)Owner).TextBlock.Margin = new Thickness(left, top, right, bottom);
                }
            }
        }

        public void SetPosition(Point position)
        {
            HitArea = new Rect(position.X, position.Y, HitArea.Width, HitArea.Height);
            RelativeHitArea = new Rect(
            HitArea.Left / Controls.OriginalWidth,
            HitArea.Top / Controls.OriginalHeight,
            HitArea.Width / ImgClickArea.ActualWidth,
            HitArea.Height / ImgClickArea.ActualHeight);
            UpdatePositions();
        }

        /// <summary>
        /// Checks for Point relative to imgClickArea is within
        /// boundaries of HitArea.
        /// </summary>
        /// <param name="Point"></param>
        /// <returns></returns>
        public Boolean IsHit(Point Point)
        {
            Boolean isHit = false;
            if (Owner.GetType() != typeof(StaticImage) && Owner.GetType() != typeof(Indicator))
            {
                isHit = Point.X >= HitArea.Left && Point.X <= HitArea.Right
                    && Point.Y >= HitArea.Top && Point.Y <= HitArea.Bottom;
                if (isHit)
                {
                    RelativeMousePosition = new Point(Point.X - HitArea.Left, Point.Y - HitArea.Top);
                }
            }
            return isHit;
        }
    }
}
