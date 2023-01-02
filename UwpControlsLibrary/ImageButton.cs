using System;
using System.Linq;
using Windows.ApplicationModel.Preview.Holographic;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

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
        public Boolean IsOn;

        private int stateCount;
        private int hoverImageCount;
        private bool hover;

        public ImageButton(Controls controls, int Id, Grid gridMain, Image[] imageList, Point Position, ImageButtonFunction function, bool hover = false)
        {
            Double width;
            Double height;

            this.Id = Id;
            GridControls = gridMain;
            this.hover = hover;

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

            hoverImageCount = imageList.Count() % 2;
            stateCount = imageList.Count() - hoverImageCount;
            if (stateCount == 4 && function == ImageButtonFunction.MOMENTARY)
            {
                throw new Exception("A momentary ImageButton can only have two or three images.");
            }

            width = imageList[0].ActualWidth;
            height = imageList[0].ActualHeight;

            HitArea = new Rect(Position.X, Position.Y, width, height);
            CopyImages(imageList);
            ControlSizing = new ControlSizing(controls, this);
            IsOn = false;

            ResetHovering();
            ImageList[1].Visibility = Visibility.Collapsed;
            if (stateCount == 4)
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

        private void ShowImage(bool down)
        {
            for (int i = 0; i < stateCount; i++)
            {
                ImageList[i].Visibility= Visibility.Collapsed;
            }

            if (down)
            {
                if (IsOn)
                {
                    if (stateCount == 2)
                    {
                        ControlSizing.ImageList[1].Visibility = Visibility.Visible;
                    }
                    else
                    {
                        ControlSizing.ImageList[3].Visibility = Visibility.Visible;
                    }
                }
                else
                {
                    if (stateCount == 2)
                    {
                        ControlSizing.ImageList[1].Visibility = Visibility.Visible;
                    }
                    else
                    {
                        ControlSizing.ImageList[1].Visibility = Visibility.Visible;
                    }
                }
            }
            else
            {
                if (IsOn)
                {
                    if (stateCount == 2)
                    {
                        ControlSizing.ImageList[1].Visibility = Visibility.Visible;
                    }
                    else
                    {
                        ControlSizing.ImageList[2].Visibility = Visibility.Visible;
                    }
                }
                else
                {
                    if (stateCount == 2)
                    {
                        ControlSizing.ImageList[1].Visibility = Visibility.Visible;
                    }
                    else
                    {
                        ControlSizing.ImageList[0].Visibility = Visibility.Visible;
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
                case EventType.POINTER_TAPPED:
                    HandlePointerWheelChangedEvent(e);
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
            IsOn = !IsOn;
            ShowImage(true);
        }

        public void HandlePointerReleasedEvent(PointerRoutedEventArgs e)
        {
            ShowImage(false);
        }

        public void HandlePointerWheelChangedEvent(PointerRoutedEventArgs e)
        {
        }

        public void HandlePointerTapped(PointerRoutedEventArgs e)
        {
        }

        public void HandlePointerRightTapped(PointerRoutedEventArgs e)
        {
        }
    }
}
