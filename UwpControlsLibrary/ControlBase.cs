using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media.Imaging;

namespace UwpControlsLibrary
{
    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    /// ControlBase class.
    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    public class ControlBase
    {
        public enum ImageButtonFunction
        {
            TOGGLE,
            MOMENTARY,
        }

        /// <summary>
        /// POPUP is used to open menus when you do not want any initial item visible.
        /// BUTTON is used when you want a button to toggle on/off and to open menus.
        /// MENU is used 
        /// </summary>
        public enum PopupMenuButtonStyle
        {
            POPUP,  // No button, click a listed pointer button anywhere, where there is no control, to open a menu.
            BUTTON, // A button. Act as a button on first listed pointer button. All other pointer button opens menus.
            MENU,   // A button. All listed pointer buttons may be used to open different menus.
            SLIDER, // A button with a slider on it.
        }

        public enum PopupMenuPosition
        {
            RIGHT,
            LEFT,
        }

        public enum ControlTextAlignment
        {
            LEFT,
            CENTER,
        }

        public enum ControlTextWeight
        {
            NORMAL,
            BOLD,
        }

        public enum EventType
        {
            POINTER_MOVED,
            POINTER_PRESSED,
            POINTER_RELEASED,
            POINTER_WHEEL_CHANGED,
            POINTER_TAPPED,
            POINTER_RIGHT_TAPPED,
        }

        public enum PointerButton
        {
            LEFT,
            RIGHT,
            MIDDLE,
            EXTRA1,
            EXTRA2,
            OTHER,
        }

        /// <summary>
        /// If a control is not enabled, it will not show any images.
        /// Calling application must decide wether to call events
        /// or not when a control is not enabled.
        /// </summary>
        public Boolean Enabled { get { return enabled; } set { Enable(value); } }
        private Boolean enabled = true;

        /// <summary>
        /// Id can be used in mouse event handlers to identify the control sending the event.
        /// Other mechanisms can be used if required. In its simplest form all controls are
        /// created with an Id value from a variable that counts up or uses the number of
        /// controls in Controls.ControlsList. The simplest way is to maintain an enum with
        /// constants corresponding to the Id numbers.
        /// </summary>
        public int Id;

        /// <summary>
        /// GridMain is referring to the grid that responds to the mouse events. Multiple
        /// Grid objects can be used if needed, but each one control can only belong to
        /// one Grid.
        /// </summary>
        public Grid GridControls;

        /// <summary>
        /// Mouse relative position. Set by MainPage at PointerMoved.
        /// </summary>
        public Double PointerX;
        public Double PointerY;

        /// <summary>
        /// The area the control is sensitive to. Use e.g. Paint with the background image
        /// open to easy obtain coordinates to feed to Id. Some variants on how to 
        /// supply coordinates are available for controls that has an Image object.
        /// </summary>
        public Rect HitArea;

        /// <summary>
        ///  The minimum value for a slider or a knob.
        ///  This can be negative, but must be less than the MaxValue.
        /// </summary>
        public int MinValue;

        /// <summary>
        ///  The maximum value for a slider or a knob.
        ///  This can be negative, but must be higher than the MinValue.
        /// </summary>
        public int MaxValue;

        /// <summary>
        /// Images used in a control, if any.
        /// Since they are inserted in order of appearence they need to be in
        /// Z-order from bottom upwards.
        /// If there is a background image, it is the first image.
        /// The topmost image, if any, is always the last image.
        /// Any other images appears between.
        /// </summary>
        public Image[] ImageList;

        /// <summary>
        /// TextBlock is currently only used by class Label.
        /// </summary>
        public TextBlock TextBlock;

        /// <summary>
        /// The original font size is used when the GUI is resized.
        /// </summary>
        public Double OriginalFontSize;

        /// <summary>
        /// The ControlSizing class handles all resizing.
        /// Each control has access to this since they all inherit ControlBase.
        /// </summary>
        public ControlSizing ControlSizing;

        /// <summary>
        /// Tag can be used for e.g. an alternative method of identifying a control
        /// in control handlers. It is up to the application programmer to use the
        /// Tag any way necessary.
        /// </summary>
        public Object Tag;

        /// <summary>
        /// Whenever a mouse button is pressed, all controls under the mouse
        /// pointer is selected. Whenever all mouse buttons are released,
        /// all controls are deselected. Whenever any mouse button is down
        /// and mouse pointer is moved, all selected controls are affected.
        /// </summary>
        public Boolean IsSelected;

        public Boolean ControlGraphicsFollowsValue = true; 

        private ImageCopy[] ImageCopyList;
        private ImageCopy ImageCopy;
        private ImageCopy BackgroundImageCopy;

        private void Enable(Boolean enable)
        {
            enabled = enable;
            if (enabled)
            {
                if (ImageList != null)
                {
                    foreach (Image image in ImageList)
                    {
                        image.Visibility = Visibility.Visible;
                    }
                }
            }
            else
            {
                if (ImageList != null)
                {
                    foreach (Image image in ImageList)
                    {
                        image.Visibility = Visibility.Collapsed;
                    }
                }
            }
        }

        public Boolean VerifyImageList(Image[] imageList)
        {
            try
            {
                if (imageList == null || imageList.Length < 1)
                {
                    return false;
                }
                else
                {
                    foreach (Image img in imageList)
                    {
                        if (img == null)
                        {
                            return false;
                        }
                    }
                    return true;
                }
            }
            catch (Exception e)
            {
                throw (new Exception("Imagelist is invalid, error message: " + e.Message));
            }
        }

        /// <summary>
        /// CopyImages copes any image or list of images to the control
        /// that will use them. The original image that was declared in 
        /// xaml is later set to Visibility.Collapsed an not used eny more
        /// in the application. That way one and the same image can be
        /// used by many controls.
        /// Class ImageCopy is used for the actual copying.
        /// </summary>
        /// <param name="image"></param>
        /// <param name="imageList"></param>
        public void CopyImages(Image[] imageList)
        {
            if (imageList != null)
            {
                ImageList = new Image[imageList.Length];
                ImageCopyList = new ImageCopy[imageList.Length];

                for (int i = 0; i < imageList.Length; i++)
                {
                    ImageCopy = new ImageCopy(imageList[i]);
                    ImageList[i] = ImageCopy.Image;
                    imageList[i].Visibility = Visibility.Collapsed;
                }
            }
        }

        //public void PointerPressed(Point point)
        //{
        //    PointerX = point.X;
        //    PointerY = point.Y;
        //}

        //public void PointerReleased(PointerRoutedEventArgs e)
        //{

        //}

        //public Object Moved(Point position)
        //{
        //    if (this.GetType() == typeof(Knob))
        //    {
        //        return ((Knob)this).SetValue(position);
        //    }
        //    else if (this.GetType() == typeof(HorizontalSlider))
        //    {
        //        return ((HorizontalSlider)this).SetValue(position);
        //    }
        //    else if (this.GetType() == typeof(VerticalSlider))
        //    {
        //        return ((VerticalSlider)this).SetValue(position);
        //    }
        //    else if (this.GetType() == typeof(Joystick))
        //    {
        //        return ((Joystick)this).SetValue(position);
        //    }
        //    else if (this.GetType() == typeof(Keyboard))
        //    {
        //        return ((Keyboard)this).SetValue(position);
        //    }
        //    return null;
        //}

        //public Boolean Tapped(object sender, TappedRoutedEventArgs e)
        //{
        //    if (GetType() == typeof(Rotator))
        //    {
        //        ((Rotator)this).Tapped();
        //    }
        //    else if (GetType() == typeof(StaticImage))
        //    {
        //        ((StaticImage)this).Tapped(null, null);
        //    }
        //    return false;
        //}

        //public Boolean RightTapped(object sender, TappedRoutedEventArgs e)
        //{
        //    if (GetType() == typeof(Rotator))
        //    {
        //        ((Rotator)this).RightTapped();
        //    }
        //    else if (GetType() == typeof(StaticImage))
        //    {
        //        ((StaticImage)this).RightTapped(null, null);
        //    }
        //    return false;
        //}
    }
}
