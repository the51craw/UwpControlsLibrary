using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace UwpControlsLibrary
{
    /// <summary>
    /// A Digital display uses images to mimic some digital display.
    /// Supply the mandatory equal sized png images showing the digits
    /// against a transparent background. Transparent because the decimal
    /// dot will overlay one of the digits, if used. Also mandatory are
	/// the minus sign and the decimal point images of the same size.
	/// An optional background image of a size enough to display the
	/// number of images you are going to display.
    /// in respect to its parent, gridControls or a CompoundControl.
    /// </summary>

    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    /// DigitalDisplay class.
    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    public class DigitalDisplay : ControlBase
    {
        /// <summary>
        /// First dimension: NumberOfDigits display positions.
        /// Second dimention: 10 digit images (0 - 9), 
        /// one minus image (10, may be null), 
        /// one dot image (11, may be null too) of the same size,
        /// and a background image (12, may also be null).
        /// </summary>
        public Image[][] Digits;
        public int NumberOfDigits;
        public int NumberOfDecimals;
        public Double ImageWidth;
        public Point DigitsRelativeOffset;
        public Size DigitRelativeSize;

        public DigitalDisplay(Controls controls, int Id, Grid gridControls, Image[] imageList, Point Position, int NumberOfDigits, int NumberOfDecimals)
        {
            this.Id = Id;
            GridControls = gridControls;
            this.NumberOfDigits = NumberOfDigits;
            this.NumberOfDecimals = NumberOfDecimals;
            Double height;

            if (imageList != null)
            {
                if (imageList.Length > 9 && imageList.Length < 14)
                {
                    for (int i = 1; i < 12 && imageList[i] != null; i++)
                    {
                        if (imageList[i - 1].ActualWidth != imageList[i].ActualWidth
                            || imageList[i - 1].ActualWidth != imageList[i].ActualWidth)
                        {
                            throw new Exception("All digits must have the same size. If you supply a background image but no minus or dot, the minus and dot images must be given as null values.");
                        }
                    }
                }
                else
                {
                    throw new Exception("A DigitalDisplay must have a list of 10 digit images, one minus image (may be null), one dot image (may be null) of the same size, and a background image (may be null).");
                }
                ImageWidth = imageList[0].ActualWidth;
                height = imageList[0].ActualHeight;
            }
            else
            {
                throw new Exception("A DigitalDisplay must have a list of images of the same size.");
            }

            CopyImages(imageList);
            if (ImageList.Length > 12 && ImageList[12] != null)
            {
                // We have a background image, add it to gridControls:
                gridControls.Children.Add(ImageList[12]);
            }
            int numberOfPositions = NumberOfDigits + NumberOfDecimals;
            int numberOfImages = 11;
            if (ImageList[11] != null)
            {
                // Minus sign present, make room for that:
                numberOfPositions++;
                numberOfImages++;
            }
            Digits = new Image[numberOfPositions][];
            for (int position = 0; position < numberOfPositions; position++)
            {
                Digits[position] = new Image[numberOfImages];
                for (int digit = 0; digit < numberOfImages; digit++)
                {
                    ImageCopy imageCopy = new ImageCopy(ImageList[digit]);
                    Digits[position][digit] = imageCopy.Image;
                    gridControls.Children.Add(Digits[position][digit]);
                    Digits[position][digit].Visibility = Visibility.Collapsed;
                }
            }

            if (ImageList.Length > 12 && imageList[12] != null)
            {
                HitArea = new Rect(Position.X, Position.Y, imageList[12].ActualWidth, imageList[12].ActualHeight);
                DigitsRelativeOffset = new Point(
                    Position.X + HitArea.Width / 2 - imageList[0].ActualWidth / 2,
                    Position.Y + HitArea.Height / 2 - imageList[0].ActualWidth / 2);
                DigitsRelativeOffset = new Point(DigitsRelativeOffset.X / controls.OriginalWidth, DigitsRelativeOffset.Y / controls.OriginalHeight);
            }
            else
            {
                HitArea = new Rect(Position.X, Position.Y, ImageWidth * NumberOfDigits, height);
                DigitsRelativeOffset = new Point(0, 0);
            }

            DigitRelativeSize =
                new Size(imageList[0].ActualWidth / GridControls.ActualWidth,
                imageList[0].ActualHeight / GridControls.ActualHeight);

            ControlSizing = new ControlSizing(controls, this);
        }

        public void DisplayValue(Double value)
        {
            // Isolate the sign in the calculations:
            int sign = value < 0 ? -1 : 1;
            value *= sign;

            // Round up to desired number of fraction digits:
            value = Math.Pow(10 , -(double)NumberOfDecimals) * (Math.Round(value * Math.Pow(10, (double)NumberOfDecimals)));

            // Split into whole and fraction parts:
            int temp = (int)(value * 100);
            double wholeValue = Math.Truncate(value);
            double fractionValue = Math.Round(Math.Pow(10, (double)NumberOfDecimals) * (value - wholeValue), NumberOfDecimals);

            // Turn off all images:
            for (int i = 0; i < Digits.Length; i++)
            {
                for (int j = 0; j < Digits[i].Length; j++)
                {
                    Digits[i][j].Visibility = Visibility.Collapsed;
                }
            }

            // Turn on those decimals needed (the index pos works from right to left):
            int pos = Digits.Length - 1;
            if (NumberOfDecimals > 0)
            {
                for (int i = NumberOfDecimals - 1; i > -1; i--)
                {
                    fractionValue /= 10;
                    Digits[pos--][((int)Math.Round(10 * (fractionValue - Math.Truncate(fractionValue)), 1))].Visibility = Visibility.Visible;
                    fractionValue = Math.Truncate(fractionValue);
                }

                // Turn on the dot at the current pos:
                Digits[pos][11].Visibility = Visibility.Visible;
            }


            // Turn on whole numbers needed:
            int done = 1;
            for (int i = NumberOfDigits - 1; i > -1 && done > 0; i--)
            {
                wholeValue /= 10;
                Digits[pos--][((int)Math.Round(10 * (wholeValue - Math.Truncate(wholeValue)), 1))].Visibility = Visibility.Visible;
                wholeValue = Math.Truncate(wholeValue);
                if (wholeValue == 0)
                {
                    done--;
                }
            }

            // Turon on minus sign if needed:
            if (sign < 0)
            {
                Digits[pos--][10].Visibility = Visibility.Visible;
            }
        }
    }
}
