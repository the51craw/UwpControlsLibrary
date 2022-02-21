using System;
using System.Collections.Generic;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

namespace UwpControlsLibrary
{
    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    /// MomentaryButton class.
    /// The momentarybutton uses two images showing the switch in the 'on' and 'off' state.
    /// It enters 'on' state on mouse down and 'off' state on mouse up.
    /// It can be set programmatically to state 'on' or 'off'.
    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    public class MomentaryButton : ControlBase
    {
        public Boolean IsOn
        {
            get { return isOn; }
            set
            {
                isOn = value;
                ControlSizing.ImageList[0].Visibility = isOn ? Visibility.Collapsed : Visibility.Visible;
                ControlSizing.ImageList[1].Visibility = isOn ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        private Boolean isOn;

        public MomentaryButton(Controls controls, Int32 Id, Grid gridMain, Image[] imageList, Point Position)
        {
            isOn = false;
            this.Id = Id;
            GridControls = gridMain;
            Double width;
            Double height;
            this.HitTarget = HitTarget;

            if (imageList == null || imageList.Length != 2
                || imageList[0].ActualWidth != imageList[1].ActualWidth
                || imageList[0].ActualHeight != imageList[1].ActualHeight)
            {
                throw new Exception("A MomentaryButton must have two images of the same size!");
            }
            else
            {
                width = imageList[0].ActualWidth;
                height = imageList[0].ActualHeight;
            }

            HitArea = new Rect(Position.X, Position.Y, width, height);
            CopyImages(imageList);
            ControlSizing = new ControlSizing(controls, this);
            ImageList[1].Visibility = Visibility.Collapsed;
        }

        public Int32 Handle(EventType eventType, PointerRoutedEventArgs e)
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

        private Int32 PointerMoved(PointerRoutedEventArgs e)
        {
            return -1;
        }

        public void PointerPressed(PointerRoutedEventArgs e)
        {

        }

        public void PointerReleased(PointerRoutedEventArgs e)
        {

        }

        public void Toggle()
        {
            IsOn = !IsOn;
        }
        public void MouseDown()
        {
            ControlSizing.ImageList[1].Visibility = Visibility.Visible;
            ControlSizing.ImageList[0].Visibility = Visibility.Collapsed;
        }

        public void MouseUp()
        {
            ControlSizing.ImageList[0].Visibility = Visibility.Visible;
            ControlSizing.ImageList[1].Visibility = Visibility.Collapsed;
        }
    }
}
