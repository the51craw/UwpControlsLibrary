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
    /// ImageButton class.
    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    public class Keyboard : ControlBase
    {
        public Octave[] Octaves { get; set; }
        //public Grid GridMain { get; set; }
        public Size WhiteKeySize { get; set; }
        public Size BlackKeySize { get; set; }

        private int lowKey;
        private int highKey;

        public Keyboard(Controls controls, int Id, Grid gridMain, Image whiteKey, Image blackKey, Point position, int lowKey, int highKey, Boolean evenKeySpread = false)
        {
            this.Id = Id;
            this.GridControls = gridMain;
            this.lowKey = lowKey;
            this.highKey = highKey;
            LimitKeyRange();
            int numberOfOctaves = (highKey - lowKey) / 12;
            Octaves = new Octave[numberOfOctaves];
            for (int i = 0; i < numberOfOctaves; i++)
            {
                if (i < numberOfOctaves - 1)
                {
                    Octaves[i] = new Octave(lowKey + 12 * i, whiteKey, blackKey);
                }
                else
                {
                    Octaves[i] = new Octave(lowKey + 12 * i, whiteKey, blackKey, true);
                }
            }
            WhiteKeySize = new Size(whiteKey.ActualWidth, whiteKey.ActualHeight);
            BlackKeySize = new Size(blackKey.ActualWidth, blackKey.ActualHeight);
            whiteKey.Visibility = Visibility.Collapsed;
            blackKey.Visibility = Visibility.Collapsed;
            ControlSizing = new ControlSizing(controls, this, Id, whiteKey, blackKey, position.X, position.Y, Octaves);
        }

        public int Handle(EventType eventType, PointerRoutedEventArgs e)
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

        private int PointerMoved(PointerRoutedEventArgs e)
        {
            return -1;
        }

        public void PointerPressed(PointerRoutedEventArgs e)
        {

        }

        public void PointerReleased(PointerRoutedEventArgs e)
        {

        }

        public Object GetKey(Image image, Point position)
        {
            return image.Tag;
        }

        private void LimitKeyRange()
        {
            lowKey = lowKey < 0 ? 0 : lowKey;
            lowKey += lowKey % 12;
            highKey = highKey > 120 ? 120 : highKey;
            highKey -= highKey % 12;
            highKey = highKey <= lowKey ? lowKey + 12 : highKey;
        }

        public Key SetValue(Point position)
        {
            int[] value = new int[2];
            int top = (int)ControlSizing.HitArea.Top;
            int bottom = (int)ControlSizing.HitArea.Bottom;

            int left = (int)ControlSizing.HitArea.Left;
            int right = (int)ControlSizing.HitArea.Right;
            if (this.IsSelected)
            //if (Key != null)
            {
                for (int i = lowKey; i < highKey; i++)
                {
                    Key key = KeyFromRange(i);
                    if (key.IsHit(position))
                    {
                        return key;
                    }
                }
            }
            return null;
        }

        public Key KeyFromRange(int keyNumber)
        {
            for (int octave = lowKey; octave < Octaves.Length; octave++)
            {
                for (int key = 0; key < Octaves[octave].Keys.Length; key++)
                {
                    if (Octaves[octave].Keys[key].KeyNumber == keyNumber)
                    {
                        return Octaves[octave].Keys[key];
                    }
                }
            }
            return null;
        }
    }

    public class Octave
    {
        public Key[] Keys { get; set; }

        public Octave(int BaseKey, Image WhiteKey, Image BlackKey, Boolean FullOctave = false)
        {
            if (FullOctave)
            {
                Keys = new Key[13];
            }
            else
            {
                Keys = new Key[12];
            }

            for (int i = 0; i < 12; i++)
            {
                switch (i)
                {
                    case 0:
                        Keys[i] = new WhiteKey(BaseKey + i, WhiteKey, 0.0);
                        break;
                    case 2:
                        Keys[i] = new WhiteKey(BaseKey + i, WhiteKey, 0.125);
                        break;
                    case 4:
                        Keys[i] = new WhiteKey(BaseKey + i, WhiteKey, 0.250);
                        break;
                    case 5:
                        Keys[i] = new WhiteKey(BaseKey + i, WhiteKey, 0.375);
                        break;
                    case 7:
                        Keys[i] = new WhiteKey(BaseKey + i, WhiteKey, 0.5);
                        break;
                    case 9:
                        Keys[i] = new WhiteKey(BaseKey + i, WhiteKey, 0.625);
                        break;
                    case 11:
                        Keys[i] = new WhiteKey(BaseKey + i, WhiteKey, 0.750);
                        break;
                }

            }

            for (int i = 0; i < 12; i++)
            {
                switch (i)
                {
                    case 1:
                        Keys[i] = new BlackKey(BaseKey + i, BlackKey, 0.075);
                        break;
                    case 3:
                        Keys[i] = new BlackKey(BaseKey + i, BlackKey, 0.225);
                        break;
                    case 6:
                        Keys[i] = new BlackKey(BaseKey + i, BlackKey, 0.450);
                        break;
                    case 8:
                        Keys[i] = new BlackKey(BaseKey + i, BlackKey, 0.5875);
                        break;
                    case 10:
                        Keys[i] = new BlackKey(BaseKey + i, BlackKey, 0.725);
                        break;
                }
            }

            if (FullOctave)
            {
                Keys[12] = new WhiteKey(BaseKey + 12, WhiteKey, 0.875);
            }
        }
    }

    public class Key
    {
        public int KeyNumber { get; set; }
        public Image Image { get; set; }
        public Double RelativeOffset { get; set; }
        public String KeyName { get; set; }
        public int Velocity { get; set; }

        public String[] names = new string[] { "C", "C#", "D", "D#", "E", "F", "F#", "G", "Ab", "A", "Bb", "B" };

        public Boolean IsHit(Point Point)
        {
            Boolean isHit = Point.X >= Image.ActualOffset.X && Point.X <= Image.ActualOffset.X
                && Point.Y >= Image.ActualOffset.Y + Image.ActualWidth && Point.Y <= Image.ActualOffset.Y + Image.ActualHeight;
            if (isHit)
            {
                Velocity = (int)(127 * (Point.Y - Image.ActualOffset.Y) / Image.ActualHeight);
            }
            return isHit;
        }
    }

    public class WhiteKey : Key
    {
        public WhiteKey(int KeyNumber, Image Image, Double RelativeOffset)
        {
            this.KeyNumber = KeyNumber;
            this.Image = new Image();
            this.Image.Source = Image.Source;
            this.Image.Stretch = Stretch.None;
            this.Image.Visibility = Visibility.Visible;
            this.Image.Tag = this;
            this.KeyName = names[KeyNumber % 12] + (KeyNumber / 12).ToString();
            this.RelativeOffset = RelativeOffset;
        }
    }

    public class BlackKey : Key
    {
        public BlackKey(int KeyNumber, Image Image, Double RelativeOffset)
        {
            this.KeyNumber = KeyNumber;
            this.Image = new Image();
            this.Image.Source = Image.Source;
            this.Image.Stretch = Stretch.None;
            this.Image.Visibility = Visibility.Visible;
            this.Image.Tag = this;
            this.KeyName = names[KeyNumber % 12] + (KeyNumber / 12).ToString();
            this.RelativeOffset = RelativeOffset;
        }
    }
}
