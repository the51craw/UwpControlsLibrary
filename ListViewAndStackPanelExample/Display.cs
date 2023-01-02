using System;
using UwpControlsLibrary;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Input;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace ListViewAndStackPanelExample
{
    public sealed partial class MainPage : Page
    {
        Int32 displayOffsetX = 0;
        Int32 displayOffsetY = 80;
        Int32 displaysOffsetY = 121;
        Int32 displaySpacing = 31;
    }

    public class Display
    {
        public Int32 X;
        public Int32 Y;
        public Image[][] Digits { get; set; }
        public Double Frequency { get { return freq; } set { freq = value; DisplayValue(); } } // 0.1 - 10000.0
        private Double freq;

        public Display()
        {
        }

        public void DisplayValue()
        {
            Int32 digit;
            freq = freq > 10000 ? 10000 : freq;
            Int32 temp = (int)(freq * 100);
            Boolean zeroesNeeded = false;

            for (Int32 i = 0; i < 7; i++)
            {
                for (Int32 j = 0; j < 20; j++)
                {
                    Digits[i][j].Visibility = Visibility.Collapsed;
                }
            }

            if (temp > 999999)
            {
                digit = temp / 1000000;
                digit = digit % 10;
                Digits[0][digit].Visibility = Visibility.Visible;
                temp = temp % 1000000;
                zeroesNeeded = true;
            }

            if (temp > 99999 || zeroesNeeded)
            {
                digit = temp / 100000;
                digit = digit % 10;
                Digits[1][digit].Visibility = Visibility.Visible;
                temp = temp % 100000;
                zeroesNeeded = true;
            }

            if (temp > 9999 || zeroesNeeded)
            {
                digit = temp / 10000;
                digit = digit % 10;
                Digits[2][digit].Visibility = Visibility.Visible;
                temp = temp % 10000;
                zeroesNeeded = true;
            }

            if (temp > 999 || zeroesNeeded)
            {
                digit = temp / 1000;
                digit = digit % 10;
                Digits[3][digit].Visibility = Visibility.Visible;
                temp = temp % 1000;
            }
            zeroesNeeded = true;

            if (temp > 99 || zeroesNeeded)
            {
                digit = temp / 100;
                digit = digit % 10;
                digit += 10;
                Digits[4][digit].Visibility = Visibility.Visible;
                temp = temp % 100;
            }

            digit = temp / 10;
            digit = digit % 10;
            Digits[5][digit].Visibility = Visibility.Visible;
            temp = temp % 10;

            Digits[6][temp].Visibility = Visibility.Visible;
        }
    }
}
