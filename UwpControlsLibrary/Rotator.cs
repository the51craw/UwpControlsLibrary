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
    /// Rotator class.
    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    public class Rotator : ControlBase
    {
        public int Selection
        {
            get { return selection; }
            set
            {
                selection = value;
                ShowSelection();
                //for (int i = 0; i < ImageList.Length; i++)
                //{
                //    ImageList[i].Visibility = Visibility.Collapsed;
                //}
                //ImageList[selection].Visibility = Visibility.Visible;
            }
        }

        private int selection;

        public Rotator(Controls controls, int Id, Grid gridMain, Image[] imageList, Point Position)
        {
            this.Id = Id;
            GridControls = gridMain;
            Double width;
            Double height;
            this.HitTarget = HitTarget;
            if (VerifyImageList(imageList))
            {
                ImageList = imageList;
            }

            if (imageList != null)
            {
                if (imageList.Length > 1)
                {
                    for (int i = 1; i < imageList.Length; i++)
                    {
                        if (imageList[i - 1].ActualWidth != imageList[i].ActualWidth
                            || imageList[i - 1].ActualWidth != imageList[i].ActualWidth)
                        {
                            throw new Exception("A Rotator must have a list of images of the same size.");
                        }
                    }
                }
                width = imageList[0].ActualWidth;
                height = imageList[0].ActualHeight;
            }
            else
            {
                throw new Exception("A Rotator must have a list of images of the same size.");
            }

            HitArea = new Rect(Position.X, Position.Y, width, height);
            CopyImages(imageList);
            ControlSizing = new ControlSizing(controls, this);
            Selection = 0;
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
            Selection = selection + 1 >= ImageList.Length ? 0 : selection + 1;
        }

        public void PointerReleased(PointerRoutedEventArgs e)
        {

        }

        public Rotator(Rotator selector)
        {
            Id = selector.Id;
            ControlSizing = selector.ControlSizing;
            GridControls = selector.GridControls;
            HitArea = new Rect(selector.HitArea.Left, selector.HitArea.Top, selector.HitArea.Width, selector.HitArea.Height);
            HitTarget = selector.HitTarget;
            CopyImages(selector.ImageList);
            MaxValue = selector.MaxValue;
            MinValue = selector.MinValue;
            selection = selector.Selection;
            Tag = selector.Tag;
            TextBlock = null;
        }

        public void Tapped()
        {
            Selection = selection + 1 >= ImageList.Length ? 0 : selection + 1;
            ShowSelection();
        }

        public void RightTapped()
        {
            Selection = selection - 1 < 0 ? ImageList.Length - 1 : selection - 1;
            ShowSelection();
        }

        public int PointerWheelChanged(int delta)
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

        public int DecrementValue(int delta)
        {
            if (selection < ImageList.Length - 1)
            {
                selection += delta;
                Selection = selection > ImageList.Length - 1 ? ImageList.Length - 1 : selection;
                return selection;
            }
            return ImageList.Length - 1;
        }

        public int IncrementValue(int delta)
        {
            if (selection > 0)
            {
                selection -= delta;
                Selection = selection < 0 ? 0 : selection;
                return selection;
            }
            return 0;
        }

        public void ShowSelection()
        {
            if (base.ImageList != null)
            {
                if (base.ImageList.Length > 1)
                {
                    for (int i = 0; i < base.ImageList.Length; i++)
                    {
                        if (i == selection)
                        {
                            base.ImageList[i].Visibility = Visibility.Visible;
                        }
                        else
                        {
                            base.ImageList[i].Visibility = Visibility.Collapsed;
                        }
                    }
                }
            }
        }
    }
}
