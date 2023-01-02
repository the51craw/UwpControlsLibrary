using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;

namespace UwpControlsLibrary
{
    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    /// StaticImage class.
    /// The StaticImage class shows an image on a specific position. It is mainly used to add images to the 
    /// background, but it also sends Tapped events, so it can be used as a button as well.
    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    public class StaticImage : ControlBase
    {
        public StaticImage(Controls controls, Int32 Id, Grid gridMain, Image[] imageList, Point Position)
        {
            this.Id = Id;
            GridControls = gridMain;
            Double width;
            Double height;

            if (imageList != null && imageList.Length > 0)
            {
                width = imageList[0].ActualWidth;
                height = imageList[0].ActualHeight;
            }
            else
            {
                throw new Exception("A StaticImage must have an image.");
            }

            HitArea = new Rect(Position.X, Position.Y, width, height);
            CopyImages(imageList);
            ControlSizing = new ControlSizing(controls, this);
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
    }
}
