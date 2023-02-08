using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Security.Cryptography.X509Certificates;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
//using static System.Net.Mime.MediaTypeNames;

namespace UwpControlsLibrary
{
    /// <summary>
    /// PopupMenuButton is a container for PopupMenuButton objects.
    /// PopupMenuButton can act as an on/off button or only as a button to show menu(s).
    /// If a PopupMenuButton is based on a TextBlock rather than an image it can also be editable by adding a TextBox to it.
    /// PopupMenuButton can have up to five menus activated by different pointer buttons, four if it also acts as a button
    /// or can be edited, of three if it also acts as a butto _and_ can be edited.
    /// A PopupMenuButton without any images will act as a 'normal' popup menu and show up where the pointer is clicked.
    /// If a PopupMenuButton has images, the last one is for hover effect. If you do not want hover effect, just supply a 
    /// transparent image as last image.
    /// Usage of pointer buttons can be remapped.
    /// PopupMenuItems are also object of type PopupMenuButton. The only thing that makes them PopupMenuItems is that
    /// they are present in a list of PopupMenuButtons in a PopupMenuButton object. This is how the menu hierarchy is built.
    /// PopupMenuButton objects can have different styles:
    /// Button: A simple button to call some function to perform a task.
    /// Slider: A compound style control with a horizontal slider.
    /// PopupMenuButton objects has a background image that defines its size.
    /// The background image hights defines the distance between PopupMenuButton
    /// objects vertical position. PopupMenu stacks them with no spacing.
    /// PopupMenuButton objects also has a label for a text, and the text
    /// can be changed to reflect a value.
    /// PopupMenuButton objects also has a TextBox object that can be used
    /// by the user to rename a menu item.
    /// A left click on a Button style PopupMenuButton objects will call some function.
    /// A left button down and drag will move the handle of a Slider style PopupMenuButton.
    /// A Slider style PopupMenuButton also reacts to mousewheel events.
    /// A right click on a PopupMenuButton objects opens the TextBox for editing the
    /// PopupMenuButton text. Esc key cancels change, Enter key saves the change.
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

        public bool IsEditing;

        public int SelectedIndex = -1;

        public PopupMenuButton Parent { get; set; }

        public List<List<PopupMenuButton>> Children; // Popup menus and/or Popup menu items
        Label label;
        bool isOn;
        public PointerButton[] ButtonMap;
        string Text;
        int hoverImage = -1;
        int offImage = -1;
        int onImage = -1;
        Controls controls;
        Grid gridControls;
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

        private double xOffset, yOffset, ySpacing;

        /// <summary>
        /// <param name="controls">Reference to Controls in UwpControlsLibrary</param>
        /// <param name="Id">Unique Id that may be used to identify the control</param>
        /// <param name="gridControls"></param>
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
        public PopupMenuButton(Controls controls, int Id, Grid gridControls, Image[] imageList, Point position,
            PopupMenuButtonStyle style, PointerButton[] buttons = null, string text = null,
            int fontSize = 16, bool edit = false, ControlTextWeight textWeight = ControlTextWeight.NORMAL, 
            ControlTextAlignment textAlignment = ControlTextAlignment.LEFT,
            Brush textOnColor = null, Brush textOffColor = null)
        {
            Parent = null;
            Style = style;
            Double width;
            Double height;
            xOffset = yOffset = ySpacing = 0.0;
            IsEditing = false;

            this.controls = controls;
            this.gridControls = gridControls;
            this.Id = Id;
            this.parent = null;
            GridControls = gridControls;
            this.fontSize = fontSize;
            if (buttons == null)
            {
                this.ButtonMap = new PointerButton[] { PointerButton.LEFT };
            }
            else
            {
                this.ButtonMap = buttons;
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
            Children = new List<List<PopupMenuButton>>();
            if (buttons != null)
            {
                this.ButtonMap = buttons;
            }
            else
            {
                this.ButtonMap = new PointerButton[] { PointerButton.LEFT, PointerButton.RIGHT, PointerButton.MIDDLE, PointerButton.EXTRA1, PointerButton.EXTRA2 };
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
                if (edit)
                {
                    TextBox = new TextBox();
                    TextBox.VerticalAlignment = VerticalAlignment.Center;
                    TextBox.Visibility = Visibility.Collapsed;
                    TextBox.KeyDown += PopupMenu_KeyDown;
                }
            }

            ControlSizing = new ControlSizing(controls, this);
            isOn = false;
        }

        public void PopupMenu_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Escape)
            {
                TextBox.Visibility = Visibility.Collapsed;
                TextBlock.Visibility = Visibility.Visible;
                IsEditing = false;
            }
            else if (e.Key == Windows.System.VirtualKey.Enter)
            {
                TextBlock.Text = TextBox.Text;
                TextBox.Visibility = Visibility.Collapsed;
                TextBlock.Visibility = Visibility.Visible;
                IsEditing = false;
            }
        }

        public int AddMenu()
        {
            Children.Add(new List<PopupMenuButton>());
            return Children.Count - 1;
        }

        public PopupMenuButton AddMenuItem(int menuNumber, int itemNumber, Image[] imageList,
            PopupMenuButtonStyle style, PointerButton[] buttons = null, string text = null, int fontSize = 16, bool edit = false, 
            ControlTextWeight textWeight = ControlTextWeight.NORMAL, ControlTextAlignment textAlignment = ControlTextAlignment.LEFT,
            double xOffset = -1.0, double yOffset = 0.0, double ySpacing = 0.0, Brush textOnColor = null, Brush textOffColor = null)
        {
            Menu = menuNumber;
            Point pos = new Point(HitArea.X + xOffset * HitArea.Width, HitArea.Y + yOffset * HitArea.Height + itemNumber * (1.0 + ySpacing) * HitArea.Height);
            PopupMenuButton control = new PopupMenuButton(controls, itemNumber, gridControls, imageList, 
                pos, style, buttons, text, fontSize, edit, textWeight, textAlignment, textOnColor, textOffColor);
            this.xOffset = xOffset;
            this.yOffset = yOffset;
            this.ySpacing = ySpacing;
            control.Visibility = Visibility.Collapsed;
            control.parent = this;
            Children[menuNumber].Add(control);
            controls.ControlsList.Add(control);
            return control;
        }

        private void ShowSubMenu(int menu)
        {
            foreach (PopupMenuButton menuItem in Children[menu])
            {
                menuItem.Visibility = Visibility.Visible;
            }
        }

        public void HideAllMenus()
        {
            foreach (List<PopupMenuButton> menuItems in Children)
            {
                foreach (PopupMenuButton menuItem in menuItems)
                {
                    HideAllSubMenus(menuItem);
                    menuItem.Visibility = Visibility.Collapsed;
                }
            }
        }

        public void HideAllSubMenus(PopupMenuButton popupMenuButton)
        {
            foreach (List<PopupMenuButton> menuItems in popupMenuButton.Children)
            {
                foreach (PopupMenuButton menuItem in menuItems)
                {
                    HideAllSubMenus(menuItem);
                    menuItem.Visibility = Visibility.Collapsed;
                }
            }
        }

        public void HideAllSiblingsSubMenus(PopupMenuButton popupMenuButton)
        {
            if (popupMenuButton.Parent != null)
            {
                foreach (List<PopupMenuButton> menuItems in popupMenuButton.Parent.Children)
                {
                    foreach (PopupMenuButton menuItem in menuItems)
                    {
                        HideAllSubMenus(menuItem);
                    }
                }
            }
        }

        /// <summary>
        /// Scrolls a menu up or down.
        /// </summary>
        /// <param name="menu">One of the menu items representing all the items to scroll.</param>
        /// <param name="offset">1: scroll up one menu item height. -1: scroll down one menu item height.</param>
        private void ScrollMenu(int menu, double offset)
        {
            if (parent != null)
            {
                if (menu < parent.Children.Count)
                {
                    // Last menu item top must not be less or equal to than parents top AND
                    // first menu item top must be less than parent top AND menu item must
                    // fit in imgClickarea:
                    if ((offset > 0 // Scrolling up
                            && parent.Children[menu][parent.Children[menu].Count - 1].HitArea.Top > parent.HitArea.Top
                            && parent.Children[menu][0].HitArea.Top > parent.Children[menu][0].HitArea.Height
                            && parent.Children[menu][0].HitArea.Top > parent.Children[menu][0].HitArea.Height + 34)
                      || offset < 0 // Scrolling down
                            && parent.Children[menu][0].HitArea.Top < parent.HitArea.Top
                            && parent.Children[menu][parent.Children[menu].Count - 1].HitArea.Bottom > parent.Children[menu][0].HitArea.Height)
                    {
                        offset *= parent.HitArea.Height;
                        //Point pos = new Point(HitArea.X + xOffset * HitArea.Width, HitArea.Y + yOffset * HitArea.Height + itemNumber * (1.0 + ySpacing) * HitArea.Height);
                        foreach (PopupMenuButton menuItem in parent.Children[menu])
                        {
                            menuItem.HitArea = new Rect(menuItem.HitArea.Left, menuItem.HitArea.Top - offset, menuItem.HitArea.Width, menuItem.HitArea.Height);
                            Thickness thickness = new Thickness(menuItem.HitArea.Left, menuItem.HitArea.Top,
                                Controls.AppSize.Width - menuItem.HitArea.Left - menuItem.HitArea.Width,
                                Controls.AppSize.Height - menuItem.HitArea.Top - menuItem.HitArea.Height);
                            menuItem.ImageList[0].Margin = thickness;
                            menuItem.TextBlock.Margin = thickness;
                            menuItem.ImageList[ImageList.Length - 1].Margin = thickness;

                            menuItem.ControlSizing.RelativeHitArea = new Rect(
                                menuItem.HitArea.Left / controls.OriginalWidth,
                                menuItem.HitArea.Top / controls.OriginalHeight,
                                menuItem.HitArea.Width / controls.OriginalWidth,
                                menuItem.HitArea.Height / controls.OriginalHeight);
                            menuItem.ControlSizing.UpdatePositions();
                        }
                    }
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
            bool closed = false;

            // Pointer mapping is in buttons.
            // Actual button pressed is in PointerButtonStates.
            // First button is for the button to be handle as a
            // button if style is BUTTON, else to open first menu.
            // Last button is for edit text, if available.
            // Rest of buttons are for opening menus.
            if (TextBox != null && Text != null && PointerButtonStates.Contains(ButtonMap[ButtonMap.Length - 1]))
            {
                // Show edit text box:
                TextBox.Visibility = Visibility.Visible;
                TextBox.Focus(FocusState.Programmatic);
                IsEditing = true;
            }
            else
            {
                // Get button pressed:
                int buttonPressed = -1;
                for (int button = 0; button < PointerButtonStates.Count; button++)
                {
                    if (PointerButtonStates.Count > 0 && PointerButtonStates[button] == ButtonMap[0])
                    {
                        buttonPressed = button;
                        break;
                    }
                }

                if (buttonPressed > -1)
                {
                    if (Style == PopupMenuButtonStyle.BUTTON)
                    {
                        if (buttonPressed == 0)
                        {

                            // Toggle button:
                            Toggle();
                        }
                        else
                        {
                            if (Children != null && Children.Count > 0)
                            {
                                // If any of the items in the menu given is visible, then all of them are.
                                closed = Children[buttonPressed - 1][0].Visibility == Visibility.Collapsed;

                                // Hide any previously visible sub menus:
                                HideAllSubMenus(this);

                                if (closed)
                                {
                                    // Show menu:
                                    ShowSubMenu(buttonPressed - 1);
                                }
                            }
                        }
                    }
                    else if (Style == PopupMenuButtonStyle.MENU)
                    {
                        // If any of the items in the menu given is visible, then all of them are.
                        if (Children != null && Children.Count > 0)
                        {
                            closed = Children[buttonPressed][0].Visibility == Visibility.Collapsed;

                            // Hide any previously visible sub menus:
                            HideAllSiblingsSubMenus(this);
                            HideAllSubMenus(this);

                            if (closed)
                            {
                                // Show menu:
                                ShowSubMenu(buttonPressed);
                            }
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
