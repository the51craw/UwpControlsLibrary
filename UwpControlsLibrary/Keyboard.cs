using System;
using System.Collections.Generic;
using System.Reflection;
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
        public enum KeyEvent
        {
            KEY_ON,
            KEY_OFF,
        }

        public Key Key { get; set; }
        public int Velocity { get; set; }
        public KeyEvent Event { get; set; }

        public Octave[] Octaves { get; set; }
        //public Grid GridMain { get; set; }
        public Size WhiteKeySize { get; set; }
        public Size BlackKeySize { get; set; }

        private int lowKey;
        private int highKey;
        public List<Image> ImagesToResize;

        public Keyboard(Controls controls, int Id, Grid gridMain, Image[] imageList, Point position, int lowKey, int highKey, Boolean evenKeySpread = false)
        {
            if (imageList.Length != 2 && imageList.Length != 4)
            {
                throw new ArgumentException("You can only use either a list containing one white\nkey and one black key, in that order, or a white\nkey, a black key, a depressed white\nkey and a depressed black key.");
            }

            ImagesToResize = new List<Image>();
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
                    Octaves[i] = new Octave(lowKey + 12 * i, imageList);
                }
                else
                {
                    Octaves[i] = new Octave(lowKey + 12 * i, imageList, true);
                }
                for (int key = 0; key < Octaves[i].Keys.Length; key++)
                {
                    for (int img = 0; img < Octaves[i].Keys[key].Images.Length; img++)
                    {
                        ImagesToResize.Add(Octaves[i].Keys[key].Images[img]);
                    }
                }
            }

            WhiteKeySize = new Size(imageList[0].ActualWidth, imageList[0].ActualHeight);
            BlackKeySize = new Size(imageList[1].ActualWidth, imageList[1].ActualHeight);
            //imageList[0].Visibility = Visibility.Collapsed;
            //imageList[1].Visibility = Visibility.Collapsed;
            ControlSizing = new ControlSizing(controls, this, Id, ImagesToResize.ToArray(), position.X, position.Y, Octaves);
        }

        public int HandleEvent(object sender, PointerRoutedEventArgs e, EventType eventType, Point pointerPosition, List<PointerButton> pointerButtonStates, Key key)
        {
            switch (eventType)
            {
                //case EventType.POINTER_MOVED:
                //    return PointerMovedEvent(pointerPosition);
                case EventType.POINTER_PRESSED:
                    PointerPressedEvent(sender, pointerPosition, pointerButtonStates, key);
                    break;
                case EventType.POINTER_RELEASED:
                    PointerReleasedEvent(sender, pointerPosition, pointerButtonStates, key);
                    break;
            }
            return -1;
        }

        //private int PointerMovedEvent(Point pointerPosition)
        //{
        //    return -1;
        //}

        public void PointerPressedEvent(object sender, Point pointerPosition, List<PointerButton> pointerButtonStates, Key key)
        {
            if (key.Images.Length > 1) // HBE
            {
                key.Images[1].Visibility = Visibility.Visible;
                Key = key;
                Key.Velocity = (Int32)(127 * ((pointerPosition.Y - key.Images[1].Margin.Top) / key.Images[0].ActualHeight / 0.8));
                Key.Velocity = Key.Velocity > 127 ? 127 : Key.Velocity;
            }
        }

        public void PointerReleasedEvent(object sender, Point pointerPosition, List<PointerButton> pointerButtonStates, Key key)
        {
            if (key.Images.Length > 1)
            {
                key.Images[1].Visibility = Visibility.Collapsed;
                Key = key;
                Velocity = (Int32)(127 * (pointerPosition.Y / ((Key)((Image)sender).Tag)
                .Images[((Key)((Image)sender).Tag).Images.Length - 1].ActualHeight / 0.8));
                Velocity = Velocity > 127 ? 127 : Velocity;
            }
        }

        //public Object GetKey(Image image, Point position)
        //{
        //    return image.Tag;
        //}

        private void LimitKeyRange()
        {
            lowKey = lowKey < 0 ? 0 : lowKey;
            lowKey += lowKey % 12;
            highKey = highKey > 120 ? 120 : highKey;
            highKey -= highKey % 12;
            highKey = highKey <= lowKey ? lowKey + 12 : highKey;
        }

        //public Key SetValue(Point position)
        //{
        //    int[] value = new int[2];
        //    int top = (int)ControlSizing.HitArea.Top;
        //    int bottom = (int)ControlSizing.HitArea.Bottom;

        //    int left = (int)ControlSizing.HitArea.Left;
        //    int right = (int)ControlSizing.HitArea.Right;
        //    if (this.IsSelected)
        //    //if (Key != null)
        //    {
        //        for (int i = lowKey; i < highKey; i++)
        //        {
        //            Key key = KeyFromRange(i);
        //            if (key.IsHit(position))
        //            {
        //                return key;
        //            }
        //        }
        //    }
        //    return null;
        //}

        //public Key KeyFromRange(int keyNumber)
        //{
        //    for (int octave = lowKey; octave < Octaves.Length; octave++)
        //    {
        //        for (int key = 0; key < Octaves[octave].Keys.Length; key++)
        //        {
        //            if (Octaves[octave].Keys[key].KeyNumber == keyNumber)
        //            {
        //                return Octaves[octave].Keys[key];
        //            }
        //        }
        //    }
        //    return null;
        //}
    }

    public class Octave
    {
        public Key[] Keys { get; set; }

        public Octave(int BaseKey, Image[] imageList, Boolean FullOctave = false)
        {
            if (FullOctave)
            {
                Keys = new Key[13];
            }
            else
            {
                Keys = new Key[12];
            }

            Image[] WhiteKey = new Image[imageList.Length / 2];
            Image[] BlackKey = new Image[imageList.Length / 2];
            WhiteKey[0] = imageList[0];
            BlackKey[0] = imageList[1];
            if (imageList.Length == 4)
            {
                WhiteKey[1] = imageList[2];
                BlackKey[1] = imageList[3];
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

        /// <summary>
        /// Generated in code, used as click area
        /// </summary>
        //public Image Image { get; set; } 
        public Image[] Images { get; set; }
        public Double RelativeOffset { get; set; }
        public String KeyName { get; set; }
        public int Velocity { get; set; }

        public String[] names = new string[] { "C", "C#", "D", "D#", "E", "F", "F#", "G", "Ab", "A", "Bb", "B" };

        public Boolean IsHit(Point Point)
        {
            Boolean isHit = Point.X >= Images[Images.Length - 1].ActualOffset.X && Point.X <= Images[Images.Length - 1].ActualOffset.X
                && Point.Y >= Images[Images.Length - 1].ActualOffset.Y + Images[Images.Length - 1].ActualWidth && Point.Y <= Images[Images.Length - 1].ActualOffset.Y + Images[Images.Length - 1].ActualHeight;
            if (isHit)
            {
                Velocity = (int)(127 * (Point.Y - Images[Images.Length - 1].ActualOffset.Y) / Images[Images.Length - 1].ActualHeight);
            }
            return isHit;
        }
    }

    public class WhiteKey : Key
    {
        public WhiteKey(int KeyNumber, Image[] Images, Double RelativeOffset)
        {
            this.KeyNumber = KeyNumber;

            if (Images.Length == 1)
            {
                this.Images = new Image[2];
                this.Images[0] = new Image();
                this.Images[0].Source = Images[0].Source;
                this.Images[0].Stretch = Stretch.None;
                this.Images[0].Visibility = Visibility.Visible;
                this.Images[0].Tag = this;
                this.Images[1] = new Image();
                this.Images[1].Source = Images[0].Source;
                this.Images[1].Stretch = Stretch.None;
                this.Images[1].Visibility = Visibility.Collapsed;
                this.Images[1].Opacity = 0;
                this.Images[1].Tag = this;
            }
            else
            {
                this.Images = new Image[3];
                this.Images[0] = new Image();
                this.Images[0].Source = Images[0].Source;
                this.Images[0].Stretch = Stretch.None;
                this.Images[0].Visibility = Visibility.Visible;
                this.Images[0].Tag = this;
                this.Images[1] = new Image();
                this.Images[1].Source = Images[1].Source;
                this.Images[1].Stretch = Stretch.None;
                this.Images[1].Visibility = Visibility.Collapsed;
                this.Images[1].Tag = this;
                this.Images[2] = new Image();
                this.Images[2].Source = Images[1].Source;
                this.Images[2].Stretch = Stretch.None;
                this.Images[2].Visibility = Visibility.Visible;
                this.Images[2].Opacity = 0;
                this.Images[2].Tag = this;
            }

            this.KeyName = names[KeyNumber % 12] + (KeyNumber / 12).ToString();
            this.RelativeOffset = RelativeOffset;
        }
    }

    public class BlackKey : Key
    {
        public BlackKey(int KeyNumber, Image[] Images, Double RelativeOffset)
        {
            this.KeyNumber = KeyNumber;

            if (Images.Length == 1)
            {
                this.Images = new Image[2];
                this.Images[0] = new Image();
                this.Images[0].Source = Images[0].Source;
                this.Images[0].Stretch = Stretch.None;
                this.Images[0].Visibility = Visibility.Visible;
                this.Images[0].Tag = this;
                this.Images[1] = new Image();
                this.Images[1].Source = Images[0].Source;
                this.Images[1].Stretch = Stretch.None;
                this.Images[1].Visibility = Visibility.Collapsed;
                this.Images[1].Opacity = 0;
                this.Images[1].Tag = this;
            }
            else
            {
                this.Images = new Image[3];
                this.Images[0] = new Image();
                this.Images[0].Source = Images[0].Source;
                this.Images[0].Stretch = Stretch.None;
                this.Images[0].Visibility = Visibility.Visible;
                this.Images[0].Tag = this;
                this.Images[1] = new Image();
                this.Images[1].Source = Images[1].Source;
                this.Images[1].Stretch = Stretch.None;
                this.Images[1].Visibility = Visibility.Collapsed;
                this.Images[1].Tag = this;
                this.Images[2] = new Image();
                this.Images[2].Source = Images[1].Source;
                this.Images[2].Stretch = Stretch.None;
                this.Images[2].Visibility = Visibility.Visible;
                this.Images[2].Opacity = 0;
                this.Images[2].Tag = this;
            }

            this.KeyName = names[KeyNumber % 12] + (KeyNumber / 12).ToString();
            this.RelativeOffset = RelativeOffset;
        }
    }
}
