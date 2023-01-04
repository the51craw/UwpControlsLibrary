using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
//using static System.Net.Mime.MediaTypeNames;

namespace UwpControlsLibrary
{
    /// <summary>
    /// PopupMenuButton is a container for PopupMenuItem objects.
    /// PopupMenuButton can act as an on/off button or only as a button to show the menu.
    /// PopupMenuButton can have up to five menus activated by different pointer buttons, four if it also acts as a button.
    /// A PopupMenuButton without any images will act as a 'normal' popup menu and show up where the pointer is.
    /// If a PopupMEnuButton has images, the last one is for hover effect. If you do not want hover effect, just supply a transparent image as last image.
    /// Usage of pointer buttons can be remapped.
    /// PopupMenuButton and PopupMenuItem can have an image for highlite on hover.
    /// PopupMenuItems can have have PopupMenyItems as children thus enabling a herarchy of popup menus.
    /// PopupMenuItem objects can have different styles:
    /// Button: A simple button to call some function to perform a task.
    /// Slider: A compound style control with a horizontal slider.
    /// PopupMenuItem objects has a background image that defines its size.
    /// The background image hights defines the distance between PopupMenuItem
    /// objects vertical position. PopupMenu stacks them with no spacing.
    /// PopupMenuItem objects also has a label for a text, and the text
    /// can be changed to reflect a value.
    /// PopupMenuItem objects also has a TextBox object that can be used
    /// by the user to rename a menu item.
    /// A left click on a Button style PopupMenuItem objects will call some function.
    /// A left button down and drag will move the handle of a Slider style PopupMenuItem.
    /// A Slider style PopupMenuItem also reacts to mousewheel events.
    /// A right click on a PopupMenuItem objects opens the TextBox for editing the
    /// PopupMenuItem text. Esc key cancels change, Enter key saves the change.
    /// </summary>

    public class PopupMenuButton : ControlBase
    {
        public Visibility Visibility
        {
            get
            {
                return visible;
            }
            set
            {
                foreach (Image image in ImageList)
                {
                    image.Visibility = value;
                }
                TextBlock.Visibility = value;
                visible = value;
            }
        }
        private Visibility visible;

        public bool IsOn
        {
            get { return isOn; }
            set { Toggle(); }
        }

        public int SelectedIndex = -1;

        public List<List<PopupMenuButton>> PopupMenus; // Popup menus, Popup menu items
        Label label;
        bool isOn;
        PointerButton[] buttons;
        string Text;
        int hoverImage = -1;
        int offImage = -1;
        int onImage = -1;
        Controls controls;
        Grid gridMain;
        int fontSize;

        public Brush TextOnColor
        {
            get
            {
                return textOnColor;
            }
            set
            {
                if (!string.IsNullOrEmpty(Text))
                {
                    TextBlock.Foreground = value;
                    textOnColor = value;
                }
            }
        }
        private Brush textOnColor;

        public Brush TextOffColor
        {
            get
            {
                return textOffColor;
            }
            set
            {
                if (!string.IsNullOrEmpty(Text))
                {
                    TextBlock.Foreground = value;
                    textOffColor = value;
                }
            }
        }
        private Brush textOffColor;
        public PopupMenuButton parent;
        public int Menu;

        public ControlTextWeight TextWeight;
        public ControlTextAlignment TextAlignment;

        public PopupMenuButtonStyle Style { get; set; }

        /// <summary>
        /// <param name="controls">Reference to Controls in UwpControlsLibrary</param>
        /// <param name="Id">Unique Id that may be used to identify the control</param>
        /// <param name="gridMain"></param>
        /// <param name="imageList">List of images to use to display the control. 1 to 3 images: Off On Hover or Off Hover or, if text is not null, Background and hover (max two images)</param>
        /// <param name="position">Top-left corner where the control will be placed.</param>
        /// <param name="style">POPUP opens a menu when right clicking anywhere. BUTTON also acts as a button. MENU can only open menus. SLIDER also acts as a slider.</param>
        /// <param name="buttons">List of pointer buttons to use. First = button on/off or first popup menu, the rest are for other popup menus.</param>
        /// <param name="text">Can be used to display a text on the control.</param>
        /// <param name="fontSize">Size of text font. (Color can be set after creation.)</param>
        /// <param name="fontWeight">Bold or normal</param>
        /// <param name="center">Text is centered if true, else left adjusted padded on left side.</param>
        /// <param name="textOnColor">Color of text.</param>
        /// <param name="textOffColor">Color of text when acting as a button and the button is off.</param>
        /// <exception cref="Exception">Throws error if images are not present or not all the same size.</exception>
        /// </summary>
        public PopupMenuButton(Controls controls, int Id, Grid gridMain, Image[] imageList, Point position,
            PopupMenuButtonStyle style, PointerButton[] buttons = null, string text = null,
            int fontSize = 8, ControlTextWeight textWeight = ControlTextWeight.NORMAL, ControlTextAlignment textAlignment = ControlTextAlignment.LEFT,
            Brush textOnColor = null, Brush textOffColor = null)
        {
            Style = style;
            Double width;
            Double height;

            this.controls = controls;
            this.gridMain = gridMain;
            this.Id = Id;
            this.parent = null;
            GridControls = gridMain;
            this.fontSize = fontSize;
            if (buttons == null)
            {
                this.buttons = new PointerButton[] { PointerButton.LEFT };
            }
            else
            {
                this.buttons = buttons;
            }

            if (imageList != null && VerifyImageList(imageList))
            {
                width = imageList[0].ActualWidth;
                height = imageList[0].ActualHeight;
                HitArea = new Rect(position.X, position.Y, width, height);
                CopyImages(imageList);

                if (ImageList.Length > 1)
                {
                    for (int i = 1; i < ImageList.Length; i++)
                    {
                        if (ImageList[i - 1].ActualWidth != ImageList[i].ActualWidth
                            || ImageList[i - 1].ActualHeight != ImageList[i].ActualHeight)
                        {
                            throw new Exception("A Button must have a list of images of the same size or no images at all.");
                        }
                    }
                    if (string.IsNullOrEmpty(text))
                    {
                        onImage = 0;
                        hoverImage = 1;
                        if (ImageList.Length > 2)
                        {
                            onImage = 1;
                            offImage = 0;
                            hoverImage = 2;
                        }
                    }
                    else
                    {
                        offImage = 0;
                        hoverImage = 1;
                    }
                    if (hoverImage > -1)
                    {
                        ImageList[hoverImage].Visibility = Visibility.Collapsed;
                    }
                    if (onImage > -1)
                    {
                        ImageList[onImage].Visibility = Visibility.Collapsed;
                    }
                }
            }
            else
            {
                position = new Point(0, 0);
                width = Window.Current.Bounds.Width;
                height = Window.Current.Bounds.Height;
            }

            TextWeight = textWeight;
            TextAlignment = textAlignment;
            PopupMenus = new List<List<PopupMenuButton>>();
            if (buttons != null)
            {
                this.buttons = buttons;
            }
            else
            {
                this.buttons = new PointerButton[] { PointerButton.LEFT, PointerButton.RIGHT, PointerButton.MIDDLE, PointerButton.EXTRA1, PointerButton.EXTRA2 };
            }

            Text = text;
            if (text != null)
            {
                TextBlock = new TextBlock();
                if (textOnColor != null)
                {
                    TextOnColor = textOnColor;
                }
                else
                {
                    TextOnColor = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255));
                }
                if (textOffColor != null)
                {
                    TextOffColor = textOffColor;
                }
                else
                {
                    TextOffColor = new SolidColorBrush(Color.FromArgb(255, 127, 127, 127));
                }
                TextBlock.Text = text;
                if (TextAlignment == ControlTextAlignment.CENTER)
                {
                    TextBlock.HorizontalAlignment = HorizontalAlignment.Center;
                }
                else
                {
                    TextBlock.HorizontalAlignment = HorizontalAlignment.Left;
                    TextBlock.Padding = new Thickness(fontSize, 0, 0, 0);
                }
                TextBlock.VerticalAlignment = VerticalAlignment.Center;
                OriginalFontSize = fontSize;
                TextBlock.FontSize = OriginalFontSize;
                TextBlock.Foreground = TextOffColor;
                if (TextWeight == ControlTextWeight.BOLD)
                {
                    TextBlock.FontWeight = Windows.UI.Text.FontWeights.Bold;
                }
            }

            ControlSizing = new ControlSizing(controls, this);
            isOn = false;
        }

        public int AddMenu()
        {
            PopupMenus.Add(new List<PopupMenuButton>());
            return PopupMenus.Count - 1;
        }

        public PopupMenuButton AddMenuItem(int menu, int item, PopupMenuButton parent, Image[] imageList, double xOffset, 
            PopupMenuButtonStyle style, PointerButton[] buttons = null, string text = null, int fontSize = 8, 
            ControlTextWeight textWeight = ControlTextWeight.NORMAL, ControlTextAlignment textAlignment = ControlTextAlignment.LEFT,
            PopupMenuPosition position = PopupMenuPosition.RIGHT, Brush textOnColor = null, Brush textOffColor = null)
        {
            Menu = menu;
            Point pos = new Point(HitArea.X + xOffset * HitArea.Width, HitArea.Y + item * HitArea.Height);
            if (position == PopupMenuPosition.LEFT)
            {
                pos = new Point(HitArea.X - xOffset * imageList[0].ActualWidth, HitArea.Y + item * HitArea.Height);
            }
            PopupMenuButton control = new PopupMenuButton(controls, item, gridMain, imageList, 
                pos, style, buttons, text, fontSize, textWeight, textAlignment, textOnColor, textOffColor);
            control.Tag = parent;
            control.Visibility = Visibility.Collapsed;
            control.parent = this;
            PopupMenus[menu].Add(control);
            controls.ControlsList.Add(control);
            return control;
        }

        private void ShowMenu(int menu)
        {
            if (menu < PopupMenus.Count)
            {
                if (Tag != null)
                {
                    // Hide all sibling menus:
                    foreach (List<PopupMenuButton> popupMenu in ((PopupMenuButton)Tag).PopupMenus)
                    {
                        foreach (PopupMenuButton menuButton in popupMenu)
                        {
                            foreach (List<PopupMenuButton> menuItems in menuButton.PopupMenus)
                            {
                                foreach (PopupMenuButton menuItem in menuItems)
                                {
                                    menuItem.Visibility = Visibility.Collapsed;
                                }
                            }
                        }
                    }
                }

                // Show the menu asked for:
                foreach (PopupMenuButton menuItem in PopupMenus[menu])
                {
                    menuItem.Visibility = Visibility.Visible;
                }
            }
        }

        //private void HideMenu(int menu)
        //{
        //    if (menu < PopupMenus.Count)
        //    {
        //        foreach (PopupMenuButton menuItem in PopupMenus[menu])
        //        {
        //            menuItem.Visibility = Visibility.Collapsed;
        //        }
        //    }
        //}

        private void ScrollMenu(int menu, double offset)
        {
            if (parent != null)
            {
                if (menu < parent.PopupMenus.Count)
                {
                    // Last menu item top must not be less or equal to than parents top AND
                    // first menu item top must be less than parent top
                    if ((offset > 0 && parent.PopupMenus[menu][parent.PopupMenus[menu].Count - 1].HitArea.Top > parent.HitArea.Top)
                        || (offset < 0 && parent.PopupMenus[menu][0].HitArea.Top < parent.HitArea.Top))
                    {
                        offset *= parent.HitArea.Height;
                        foreach (PopupMenuButton menuItem in parent.PopupMenus[menu])
                        {
                            menuItem.HitArea = new Rect(menuItem.HitArea.Left, menuItem.HitArea.Top - offset, menuItem.HitArea.Width, menuItem.HitArea.Height);
                            Thickness thickness = new Thickness(menuItem.HitArea.Left, menuItem.HitArea.Top,
                                Controls.AppSize.Width - menuItem.HitArea.Left - menuItem.HitArea.Width,
                                Controls.AppSize.Height - menuItem.HitArea.Top - menuItem.HitArea.Height);
                            menuItem.ImageList[0].Margin = thickness;
                            menuItem.TextBlock.Margin = thickness;
                            menuItem.ImageList[ImageList.Length - 1].Margin = thickness;
                        }
                    }
                }
            }
        }

        public void HideAllMenus()
        {
            foreach (List<PopupMenuButton> menuItems in PopupMenus)
            {
                foreach (PopupMenuButton menuItem in menuItems)
                {
                    menuItem.Visibility = Visibility.Collapsed;
                }
            }
        }

        private void Toggle()
        {
            isOn = !isOn;
            if (isOn)
            {
                if (string.IsNullOrEmpty(Text))
                {
                    ImageList[onImage].Visibility = Visibility.Visible;
                }
                else
                {
                    TextBlock.Foreground = TextOnColor;
                }
            }
            else
            {
                if (string.IsNullOrEmpty(Text))
                {
                    ImageList[onImage].Visibility = Visibility.Collapsed;
                }
                else
                {
                    TextBlock.Foreground = TextOffColor;
                }
            }
        }

        public void ResetHovering()
        {
            if (hoverImage > -1)
            {
                ImageList[hoverImage].Visibility = Visibility.Collapsed;
            }
        }

        public void SetHovering()
        {
            if (hoverImage > -1 && ImageList[0].Visibility == Visibility.Visible)
            {
                ImageList[hoverImage].Visibility = Visibility.Visible;
            }
        }

        public void HandleEvent(PointerRoutedEventArgs e, EventType eventType, List<ControlBase.PointerButton> PointerButtonStates,int delta, int menuNumber = 0)
        {
            switch (eventType)
            {
                case EventType.POINTER_MOVED:
                    HandlePointerMovedEvent(e, eventType, PointerButtonStates);
                    break;
                case EventType.POINTER_PRESSED:
                    HandlePointerPressedEvent(e, eventType, PointerButtonStates);
                    break;
                case EventType.POINTER_WHEEL_CHANGED:
                    HandlePointerWheelChangedEvent(delta, menuNumber);
                    break;
            }
        }

        public void HandlePointerMovedEvent(PointerRoutedEventArgs e, EventType eventType, List<ControlBase.PointerButton> PointerButtonStates)
        {
            if (hoverImage > -1 && ImageList[0].Visibility == Visibility.Visible)
            {
                ImageList[hoverImage].Visibility = Visibility.Visible;
            }
        }

        public void HandlePointerPressedEvent(PointerRoutedEventArgs e, EventType eventType, List<ControlBase.PointerButton> PointerButtonStates)
        {
            // Pointer mapping is in buttons.
            // Actual button pressed is in PointerButtonStates.
            // First button is for the button to be handle as a
            // button if style is BUTTON, else to open first menu.
            // Rest of buttons are for opening menus.
            if (Style == PopupMenuButtonStyle.BUTTON)
            {
                for (int buttonPressed = 0; buttonPressed < PointerButtonStates.Count; buttonPressed++)
                {
                    if (PointerButtonStates.Count > 0 && PointerButtonStates[buttonPressed] == buttons[0])
                    {
                        // Toggle button:
                        Toggle();
                    }
                    else
                    {
                        // Show menu:
                        ShowMenu(buttonPressed);
                    }
                }
            }
            else if (Style == PopupMenuButtonStyle.MENU)
            {
                for (int buttonPressed = 0; buttonPressed < buttons.Length; buttonPressed++)
                {
                    if (PointerButtonStates[0] == buttons[buttonPressed])
                    {
                        if (buttonPressed == 0)
                        {
                            // Turn off previous selection:
                            foreach (List<PopupMenuButton> popupMenu in parent.PopupMenus)
                            {
                                if (popupMenu[0].visible == Visibility.Visible)
                                {
                                    foreach (PopupMenuButton popupMenuButton in popupMenu)
                                    {
                                        if (string.IsNullOrEmpty(popupMenuButton.Text))
                                        {
                                            popupMenuButton.ImageList[onImage].Visibility = Visibility.Collapsed;
                                        }
                                        else
                                        {
                                            popupMenuButton.TextBlock.Foreground = textOffColor;
                                        }
                                    }
                                }
                            }

                            //// Turn off the selected PopupMenuButton:
                            //if (((PopupMenuButton)Tag).SelectedIndex > -1)
                            //{
                            //    ImageList[((PopupMenuButton)Tag).SelectedIndex].Visibility = Visibility.Collapsed;
                            //}
                            
                            // First mouse button pressed, Set selected:
                            ((PopupMenuButton)Tag).SelectedIndex = Id;
                            if (string.IsNullOrEmpty(Text))
                            {
                                ImageList[onImage].Visibility = Visibility.Visible;
                            }
                            else
                            {
                                TextBlock.Foreground = TextOnColor;
                            }
                        }
                        else 
                        {
                            // Show menu:
                            ShowMenu(buttonPressed - 1);
                        }
                    }
                }
            }
        }

        public void HandlePointerWheelChangedEvent(int delta, int menu)
        {
            ScrollMenu(menu, delta);
        }
    }
}
