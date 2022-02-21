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
    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    /// DigitalDisplay class.
    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    public class DigitalDisplay : ControlBase
    {
        /// <summary>
        /// First dimension: NumberOfDigits display positions.
        /// Second dimention: 10 digits (0 - 9) plus optional 10 digits with decimals (0. - 9.).
        /// </summary>
        public Image[][] Digits; 
        public int NumberOfDigits;
        public int NumberOfDecimals;
        public Double ImageWidth;

        public DigitalDisplay(Controls controls, int Id, Grid gridControls, Image[] imageList, Point Position, int NumberOfDigits, int NumberOfDecimals)
        {
            this.Id = Id;
            GridControls = gridControls;
            this.NumberOfDigits = NumberOfDigits;
            this.NumberOfDecimals = NumberOfDecimals;
            Double height;
            this.HitTarget = HitTarget;

            if (imageList != null)
            {
                if ((imageList.Length == 10 && NumberOfDecimals == 0) || imageList.Length == 20)
                {
                    for (int i = 1; i < imageList.Length; i++)
                    {
                        if (imageList[i - 1].ActualWidth != imageList[i].ActualWidth
                            || imageList[i - 1].ActualWidth != imageList[i].ActualWidth)
                        {
                            throw new Exception("All digits must have the same size.");
                        }
                    }
                }
                else
                {
                    throw new Exception("A DigitalDisplay must have a list of images of the same size.");
                }
                ImageWidth = imageList[0].ActualWidth;
                height = imageList[0].ActualHeight;
            }
            else
            {
                throw new Exception("A DigitalDisplay must have a list of images of the same size.");
            }

            HitArea = new Rect(Position.X, Position.Y, ImageWidth * NumberOfDigits, height);

            Digits = new Image[NumberOfDigits][];
            ImageCopy imageCopy;
            for (int position = 0; position < NumberOfDigits; position++)
            {
                Digits[position] = new Image[10];
                for (int digit = 0; digit < 10; digit++)
                {
                    imageCopy = new ImageCopy(imageList[digit]);
                    Digits[position][digit] = imageCopy.Image;
                    if (position == NumberOfDigits - NumberOfDecimals - 1)
                    {
                        imageCopy = new ImageCopy(imageList[digit + 10]);
                    }
                    else
                    {
                        imageCopy = new ImageCopy(imageList[digit]);
                    }
                    Digits[position][digit] = imageCopy.Image;
                    gridControls.Children.Add(Digits[position][digit]);
                    Digits[position][digit].Visibility = Visibility.Collapsed;
                }
            }

            // Initially show 0.00:
            Digits[4][0].Visibility = Visibility.Visible;
            Digits[5][0].Visibility = Visibility.Visible;
            Digits[6][0].Visibility = Visibility.Visible;



            //for (int digit = 0; digit < NumberOfDigits; digit++)
            //{
            //    Digits[digit] = new Image[imageList.Length];
            //    for (int i = 0; i < 10; i++)
            //    {
            //        if (digit > NumberOfDigits - NumberOfDecimals)
            //        {
            //            imageCopy = new ImageCopy(imageList[digit]);
            //        }
            //        else
            //        {
            //            imageCopy = new ImageCopy(imageList[digit + 10]);
            //        }
            //        Digits[digit][i] = imageCopy.Image;
            //    }
            //}

            ControlSizing = new ControlSizing(controls, this);
        }

        public void DisplayValue(Double value)
        {
            int digit;
            int temp = (int)(value * 100);
            Boolean zeroesNeeded = false;

            for (int i = 0; i < NumberOfDigits; i++)
            {
                for (int j = 0; j < 10; j++)
                {
                    Digits[i][j].Visibility = Visibility.Collapsed;
                }
            }

            for (int i = NumberOfDigits - 1; i > -1; i--)
            {
                if (temp >= Math.Pow(10, i))
                {
                    digit = temp / (int)Math.Pow(10, i);
                    Digits[NumberOfDigits - i - 1][digit].Visibility = Visibility.Visible;
                    temp = temp % (int)Math.Pow(10, i);
                    zeroesNeeded = true;
                }
                else if(zeroesNeeded || i < NumberOfDecimals + 1)
                {
                    Digits[NumberOfDigits - i - 1][0].Visibility = Visibility.Visible;
                }
            }
        }
    }
}
