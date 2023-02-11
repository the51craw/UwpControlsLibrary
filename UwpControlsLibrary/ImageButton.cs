using System;
using System.Linq;
using Windows.ApplicationModel.Preview.Holographic;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;

namespace UwpControlsLibrary
{
    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    /// ImageButton class.
    /// <summary>
    /// ImageButton uses one or more images as a button. There are five types of images used, OffUp, OnUp, OffDown,
    /// OnDown and Hover.
    /// The number of images and flag hover controls the appearence of the button:
    /// Two images: Normal state image, pressed down image, but if hover is true, second image is hover image.
    /// Three images: Normal state image, pressed down image, hover image.
    /// four images: Normal off state image, off and pressed state image, normal on state image, pressed on state
    /// image.
    /// five images: Normal off state image, off and pressed state image, normal on state image, pressed on state
    /// image, hover image.
    /// The button function can be a normal toggle button or a momentary button. Momentary buttons only uses two
    /// or three images.
    /// </summary>
    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    public class ImageButton : ControlBase
    {
        public Boolean IsOn
        {
            get
            {
                return isOn;
            }
            set
            {
                isOn = value;
                ShowImage(isOn);
            }
        }
        private Boolean isOn;

        private int stateCount;
        private int state = 0;
        private int hoverImageCount;
        private bool hover;
        public ImageButtonFunction Function;

        public ImageButton(Controls controls, int Id, Grid gridControls, Image[] imageList,
		Point Position, ImageButtonFunction function, bool hover = false,
		    string text = null, int fontSize = 16, TextAlignment textAlignment = TextAlignment.Center,
            ControlTextWeight textWeight = ControlTextWeight.NORMAL,
            TextWrapping textWrapping = TextWrapping.NoWrap, Brush foreground = null)
        {
            Double width;
            Double height;

            this.Id = Id;
            GridControls = gridControls;
            this.hover = hover;
            this.Function = function;

            if (VerifyImageList(imageList))
            {
                ImageList = imageList;

                if (imageList.Length > 1)
                {
                    for (int i = 1; i < imageList.Length; i++)
                    {
                        if (imageList[i - 1].ActualWidth != imageList[i].ActualWidth
                            || imageList[i - 1].ActualHeight != imageList[i].ActualHeight)
                        {
                            throw new Exception("An ImageButton must have a list of images of the same size.");
                        }
                    }
                }
            }
            else
            {
                throw new Exception("An ImageButton must have a list of images of the same size.");
            }

            hoverImageCount = imageList.Count() > 1 ? imageList.Count() % 2 : 0;
            stateCount = imageList.Count() - hoverImageCount;
            if (stateCount == 4 && function == ImageButtonFunction.MOMENTARY)
            {
                throw new Exception("A momentary ImageButton can only have two or three images.");
            }

            width = imageList[0].ActualWidth;
            height = imageList[0].ActualHeight;

            if (!string.IsNullOrEmpty(text))
            {
                TextBlock = new TextBlock();
                TextBlock.Text = text;
                TextBlock.FontSize = fontSize;
                if (textWeight == ControlTextWeight.BOLD)
                {
                    TextBlock.FontWeight = Windows.UI.Text.FontWeights.Bold;
                }
                if (foreground != null)
                {
                    TextBlock.Foreground = foreground;
                }
                else
                {
                    TextBlock.Foreground = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255));
                }
                TextBlock.VerticalAlignment = VerticalAlignment.Center;
                TextBlock.TextAlignment = textAlignment;
                TextBlock.TextWrapping = textWrapping;
                OriginalFontSize = fontSize;
            }

            HitArea = new Rect(Position.X, Position.Y, width, height);
            CopyImages(imageList);
            ControlSizing = new ControlSizing(controls, this);
            isOn = false;

            if (hoverImageCount > 0)
            {
                ResetHovering();
                ImageList[1].Visibility = Visibility.Collapsed;
            }
            if (stateCount > 3)
            {
                ImageList[2].Visibility = Visibility.Collapsed;
                ImageList[3].Visibility = Visibility.Collapsed;
            }
        }

        public void ResetHovering()
        {
            if (hoverImageCount == 1)
            {
                ImageList[ImageList.Count() - 1].Visibility = Visibility.Collapsed;
            }
        }

        private void ShowImage(bool down) // down = pointer pressed, !down = pointer released
        {
            for (int i = 0; i < stateCount && i < ImageList.Count(); i++)
            {
                ImageList[i].Visibility= Visibility.Collapsed;
            }

            if (Function == ImageButtonFunction.TOGGLE)
            {
                if (ImageList.Count() > 1)
                {
                    if (isOn)
                    {
                        if (stateCount == 2)
                        {
                            ControlSizing.ImageList[1].Visibility = Visibility.Visible;
                        }
                        else
                        {
                            state++;
                            state = state % 4;
                            ControlSizing.ImageList[state].Visibility = Visibility.Visible;
                        }
                    }
                    else
                    {
                        if (stateCount == 2)
                        {
                            ControlSizing.ImageList[0].Visibility = Visibility.Visible;
                        }
                        else
                        {
                            state++;
                            state = state % 4;
                            ControlSizing.ImageList[state].Visibility = Visibility.Visible; // down, on.
                        }
                    }
                }
                else
                {
                    if (isOn)
                    {
                        ControlSizing.ImageList[0].Visibility = Visibility.Visible;
                    }
                    else
                    {
                        ControlSizing.ImageList[0].Visibility = Visibility.Collapsed;
                    }
                }
            }
            else
            {
                if (ImageList.Count() > 1)
                {
                    if (down)
                    {
                        ControlSizing.ImageList[1].Visibility = Visibility.Visible;
                    }
                    else
                    {
                        ControlSizing.ImageList[0].Visibility = Visibility.Visible;
                    }
                }
                else
                {
                    if (down)
                    {
                        ControlSizing.ImageList[0].Visibility = Visibility.Visible;
                    }
                    else
                    {
                        ControlSizing.ImageList[0].Visibility = Visibility.Collapsed;
                    }
                }
            }
        }

        public void SetDeSelected()
        {
        }

        public void HandleEvent(PointerRoutedEventArgs e, EventType eventType)
        {
            switch (eventType)
            {
                case EventType.POINTER_MOVED:
                    HandlePointerMovedEvent(e);
                    break;
                case EventType.POINTER_PRESSED:
                    HandlePointerPressedEvent(e);
                    break;
                case EventType.POINTER_RELEASED:
                    HandlePointerReleasedEvent(e);
                    break;
            }
        }

        public void HandlePointerMovedEvent(PointerRoutedEventArgs e)
        {
            if (hoverImageCount == 1)
            {
                ImageList[ImageList.Count() - 1].Visibility = Visibility.Visible;
            }
        }

        public void HandlePointerPressedEvent(PointerRoutedEventArgs e)
        {
            isOn = !isOn;
            ShowImage(true);
        }

        public void HandlePointerReleasedEvent(PointerRoutedEventArgs e)
        {
            ShowImage(false);
        }
    }
}
