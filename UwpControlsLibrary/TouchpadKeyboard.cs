using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace UwpControlsLibrary
{
    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    /// Rotator class.
    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    public class TouchpadKeyboard : ControlBase
    {
        private Int32[] blackKeyNumbers = new Int32[] { 1, 3, 6, 8, 10 };
        
        public enum Keylayout
        {
            WHITE_TO_WHITE,
            WHITE_TO_BLACK,
            BLACK_TO_WHITE,
            BLACK_TO_BLACK,
        }

        public ControlBase[] WhiteKeyList { get; set; }
        public ControlBase[] BlackKeyList { get; set; }
        public byte Octave { get; set; }
        public Keylayout keylayout { get; set; }

        public TouchpadKeyboard(Controls controls, Int32 Id, Grid gridMain, Image[] imageList, Point Position, byte LowKey, byte HighKey)
        {
            this.Id = Id;
            GridControls = gridMain;
            Double width;
            Double height;
            Octave = (byte)(LowKey / 12);
            Int32 numberOfBlackKeys = 0;
            Int32 numberOfWhiteKeys = 0;
            Int32 numberOfKeys = 1 + HighKey - LowKey;

            if (imageList == null || imageList.Length != 2)
            {
                throw new Exception("TouchpadKeyboard needs two images in ImageList: white key image, black key image!");
            }

            for (Int32 key = 0; key < numberOfKeys; key++)
            {
                if (blackKeyNumbers.Contains((byte)key))
                {
                    numberOfBlackKeys++;
                }
                else
                {
                    numberOfWhiteKeys++;
                }
            }

            width = imageList[0].ActualWidth > imageList[1].ActualWidth ? imageList[0].ActualWidth : imageList[1].ActualWidth;

            if (blackKeyNumbers.Contains(LowKey) && blackKeyNumbers.Contains(HighKey))
            {
                // Starts and ends with a black key
                width = (numberOfWhiteKeys + 1) * width;
                keylayout = Keylayout.BLACK_TO_BLACK;
            }
            else if (blackKeyNumbers.Contains(LowKey))
            {
                // Startes with a black key and ends with a white key
                width = (numberOfWhiteKeys + 0.5) * width;
                keylayout = Keylayout.BLACK_TO_WHITE;
            }
            else if (blackKeyNumbers.Contains(HighKey))
            {
                // Starts with a white key and ends with a black key
                width = (numberOfWhiteKeys + 0.5) * width;
                keylayout = Keylayout.WHITE_TO_BLACK;
            }
            else
            {
                // Starts and ends with a white key
                width = (numberOfWhiteKeys) * width;
                keylayout = Keylayout.WHITE_TO_WHITE;
            }

            height = imageList[0].ActualHeight + imageList[1].ActualHeight;

            Point upperLeftCorner = new Point(Position.X - width / 2f, Position.Y - height / 2f);

            HitArea = new Rect(upperLeftCorner, new Size(width, height));

            WhiteKeyList = new ControlBase[numberOfWhiteKeys];
            BlackKeyList = new ControlBase[numberOfBlackKeys];
            ImageCopy imageCopy;

            for (Int32 i = 0; i < WhiteKeyList.Length; i++)
            {
                WhiteKeyList[i] = new ControlBase();
                imageCopy = new ImageCopy(imageList[0]);
                WhiteKeyList[i].ImageList[0] = imageCopy.Image;
                WhiteKeyList[i].Id = Id;
                WhiteKeyList[i].GridControls = gridMain;
                WhiteKeyList[i].HitArea = CalcualteHitAreaWhite(i, upperLeftCorner,
                    imageList[0].ActualSize, imageList[1].ActualSize, (byte)((LowKey) % 12));
                WhiteKeyList[i].ControlSizing = new ControlSizing(controls, WhiteKeyList[i]);
            }

            for (Int32 i = 0; i < BlackKeyList.Length; i++)
            {
                BlackKeyList[i] = new ControlBase();
                imageCopy = new ImageCopy(imageList[1]);
                BlackKeyList[i].ImageList[1] = imageCopy.Image;
                BlackKeyList[i].Id = Id;
                BlackKeyList[i].GridControls = gridMain;
                BlackKeyList[i].HitArea = CalcualteHitAreaBlack(i, upperLeftCorner,
                    imageList[0].ActualSize, imageList[1].ActualSize, (byte)((LowKey) % 12));
                BlackKeyList[i].ControlSizing = new ControlSizing(controls, BlackKeyList[i]);
            }

            numberOfWhiteKeys = 0;
            numberOfBlackKeys = 0;
            byte lowKey = (byte)(LowKey % 12);
            byte highKey = (byte)(lowKey + HighKey - LowKey);
            for (Int32 key = 0; key < numberOfKeys; key++)
            {
                if (blackKeyNumbers.Contains((byte)key))
                {
                    BlackKeyList[numberOfBlackKeys++].Tag = lowKey + key;
                }
                else
                {
                    WhiteKeyList[numberOfWhiteKeys++].Tag = lowKey + key;
                }
            }

            ImageList = null;

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

        private Int32 PointerMoved(PointerRoutedEventArgs e)
        {
            return -1;
        }

        public byte PointerPressed(Point Position)
        {
            return (byte)(Key(Position) + Octave * 12);
        }

        public byte PointerReleased(Point Position)
        {
            return (byte)(Key(Position) + Octave * 12);
        }

        public void Tapped()
        {
        }

        public Object PointerWheelChanged(Int32 delta)
        {
            if (delta > 0)
            {
                return IncrementValue(delta);
            }
            else
            {
                return DecrementValue(-delta);
            }
        }

        public Object DecrementValue(Int32 delta)
        {
            //if (selection < ImageList.Length - 1)
            //{
            //    selection += delta;
            //    Selection = selection > ImageList.Length - 1 ? ImageList.Length - 1 : selection;
            //    return selection;
            //}
            return null;
        }

        public Object IncrementValue(Int32 delta)
        {
            //if (selection > 0)
            //{
            //    selection -= delta;
            //    Selection = selection < 0 ? 0 : selection;
            //    return selection;
            //}
            return null;
        }

        public void Move(TouchpadKeyboard touchpadKeyboard, Point clickareaSize, Point from, Point to)
        {
            touchpadKeyboard.ControlSizing.HitArea = new Rect(
                touchpadKeyboard.ControlSizing.HitArea.Left + (to.X - from.X),
                touchpadKeyboard.ControlSizing.HitArea.Top + (to.Y - from.Y),
                touchpadKeyboard.ControlSizing.HitArea.Width,
                touchpadKeyboard.ControlSizing.HitArea.Height
                );
            touchpadKeyboard.ControlSizing.RelativeHitArea = new Rect(
                touchpadKeyboard.ControlSizing.HitArea.Left / clickareaSize.X,
                touchpadKeyboard.ControlSizing.HitArea.Top / clickareaSize.Y,
                touchpadKeyboard.ControlSizing.HitArea.Width / clickareaSize.X,
                touchpadKeyboard.ControlSizing.HitArea.Height / clickareaSize.Y
                );
            touchpadKeyboard.ControlSizing.UpdatePositions();

            foreach (ControlBase whiteKey in touchpadKeyboard.WhiteKeyList)
            {
                whiteKey.ControlSizing.HitArea = new Rect(
                    whiteKey.ControlSizing.HitArea.Left + (to.X - from.X),
                    whiteKey.ControlSizing.HitArea.Top + (to.Y - from.Y),
                    whiteKey.ControlSizing.HitArea.Width,
                    whiteKey.ControlSizing.HitArea.Height
                    );
                whiteKey.ControlSizing.RelativeHitArea = new Rect(
                    whiteKey.ControlSizing.HitArea.Left / clickareaSize.X,
                    whiteKey.ControlSizing.HitArea.Top / clickareaSize.Y,
                    whiteKey.ControlSizing.HitArea.Width / clickareaSize.X,
                    whiteKey.ControlSizing.HitArea.Height / clickareaSize.Y
                    );
                whiteKey.ControlSizing.UpdatePositions();
            }

            foreach (ControlBase blackKey in touchpadKeyboard.BlackKeyList)
            {
                blackKey.ControlSizing.HitArea = new Rect(
                    blackKey.ControlSizing.HitArea.Left + (to.X - from.X),
                    blackKey.ControlSizing.HitArea.Top + (to.Y - from.Y),
                    blackKey.ControlSizing.HitArea.Width,
                    blackKey.ControlSizing.HitArea.Height
                    );
                blackKey.ControlSizing.RelativeHitArea = new Rect(
                    blackKey.ControlSizing.HitArea.Left / clickareaSize.X,
                    blackKey.ControlSizing.HitArea.Top / clickareaSize.Y,
                    blackKey.ControlSizing.HitArea.Width / clickareaSize.X,
                    blackKey.ControlSizing.HitArea.Height / clickareaSize.Y
                    );
                blackKey.ControlSizing.UpdatePositions();
            }
        }

        public Int32 Key(Point Position)
        {
            foreach (ControlBase whiteKey in WhiteKeyList)
            {
                if (whiteKey.ControlSizing.IsHit(Position))
                {
                    return (Int32)whiteKey.Tag;
                }
            }
            foreach (ControlBase blackKey in BlackKeyList)
            {
                if (blackKey.ControlSizing.IsHit(Position))
                {
                    return (Int32)blackKey.Tag;
                }
            }
            return 0;
        }

        private Rect CalcualteHitAreaWhite(Int32 i, Point UpperLeftCorner, Vector2 WhiteKeySize, Vector2 BlackKeySize, byte LowKey)
        {
            Double left = 0f;
            Double top = 0f;
            Double right = 0f;
            Double bottom = 0f;
            Double width = 0f;
            Double extra = 0f;

            width = WhiteKeySize.X > BlackKeySize.X ? WhiteKeySize.X : BlackKeySize.X;

            switch (keylayout)
            {
                case Keylayout.WHITE_TO_WHITE:
                case Keylayout.WHITE_TO_BLACK:
                    left = UpperLeftCorner.X + i * width;
                    right = left + width;
                    top = UpperLeftCorner.Y + BlackKeySize.Y;
                    bottom = top + BlackKeySize.Y;
                    break;
                case Keylayout.BLACK_TO_WHITE:
                case Keylayout.BLACK_TO_BLACK:
                    left = UpperLeftCorner.X + (i + 0.5) * width;
                    top = UpperLeftCorner.Y;
                    bottom = top + BlackKeySize.Y;
                    right = left + width;
                    break;
            }

            return new Rect(new Point(left, top), new Point(right, bottom));
        }

        private Rect CalcualteHitAreaBlack(Int32 i, Point UpperLeftCorner, Vector2 WhiteKeySize, Vector2 BlackKeySize, byte LowKey)
        {
            Double left = 0f;
            Double top = 0f;
            Double right = 0f;
            Double bottom = 0f;
            Double width = 0f;
            Double extra = 0f;

            width = WhiteKeySize.X > BlackKeySize.X ? WhiteKeySize.X : BlackKeySize.X;

            // First key is KeyNumber - i.
            // Left is UpperLeftCorner.X + i * width / 2 BUT
            // for each time the range LowKey to i contains two adjacent white keys,
            // add width / 2!
            extra = 0f;

            switch (keylayout)
            {
                case Keylayout.WHITE_TO_WHITE:
                case Keylayout.WHITE_TO_BLACK:
                    extra = 0.5f;
                    break;
            }

            for (Int32 k = LowKey; k < i + 1; k++)
            {
                if (k % 12 == 2 || k % 5 == 11)
                {
                    extra += width;
                }
            }

            left = UpperLeftCorner.X + (i + 0.5) * width + extra;
            right = left + width;
            top = UpperLeftCorner.Y;
            bottom = top + BlackKeySize.Y;

            return new Rect(new Point(left, top), new Point(right, bottom));
        }

        public void ShowSelection()
        {
            if (base.ImageList != null)
            {
                if (base.ImageList.Length > 1)
                {
                    for (Int32 i = 0; i < base.ImageList.Length; i++)
                    {
                        //if (i == selection)
                        //{
                        //    base.ImageList[i].Visibility = Visibility.Visible;
                        //}
                        //else
                        //{
                        //    base.ImageList[i].Visibility = Visibility.Collapsed;
                        //}
                    }
                }
            }
        }
    }

    /// <summary>
    /// Images in TouchpadKeyboard must act as their own controls
    /// in order to be sized and resized properly.
    /// </summary>
}
