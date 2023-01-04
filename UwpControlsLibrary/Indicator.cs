using System;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;

namespace UwpControlsLibrary
{
    /// <summary>
    /// An indicator is an Image that can be turned on or off from code.
    /// This is useful e.g. an Image button needs to indicate that it has
    /// been used to toggle a program state. An ImageButton can not be
    /// used in such a case since it only toggles its own image, but the
    /// Indicator can of course be toggled also via code trigged by
    /// a MomentaryButton, just like by any control.
    /// </summary>

    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    /// Indicator class
    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    public class Indicator : ControlBase
    {
        public Boolean IsOn
        {
            get { return isOn; }
            set
            {
                isOn = value;
                if (ImageList.Length == 2)
                {
                    ControlSizing.ImageList[0].Visibility = isOn ? Visibility.Collapsed : Visibility.Visible;
                    ControlSizing.ImageList[1].Visibility = isOn ? Visibility.Visible : Visibility.Collapsed;
                }
                else if (ImageList.Length == 1)
                {
                    ControlSizing.ImageList[0].Visibility = isOn ? Visibility.Visible : Visibility.Collapsed;
                }
            }
        }

        private Boolean isOn;

        public Indicator(Controls controls, Int32 Id, Grid gridMain, Image[] imageList, Point Position)
        {
            isOn = false;
            this.Id = Id;
            GridControls = gridMain;
            Double width;
            Double height;
            // this.HitTarget = HitTarget;

            if (imageList == null)
            {
                throw new Exception("An Indicator must have one or two images!");
            }
            else if (imageList.Length == 2
                && (imageList[0].ActualWidth != imageList[1].ActualWidth
                || imageList[0].ActualHeight != imageList[1].ActualHeight))
            {
                throw new Exception("An Indicator with two images must have images of the same size!");
            }
            else
            {
                width = imageList[0].ActualWidth;
                height = imageList[0].ActualHeight;
            }

            HitArea = new Rect(Position.X, Position.Y, width, height);
            CopyImages(imageList);
            ControlSizing = new ControlSizing(controls, this);
            ImageList[imageList.Length - 1].Visibility = Visibility.Collapsed;
        }

        public void SetDeSelected()
        {
        }

        public void HandleEvent(PointerRoutedEventArgs e, EventType eventType)
        {
        }
    }
}
