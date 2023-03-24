using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Input;
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
    /// If a PopupMenuButton is based on a TextBlock rather than an image it can also be editable 
    ///     by adding a TextBox to it.
    /// PopupMenuButton can have up to five menus activated by different pointer buttons, four 
    ///     if it also acts as a button or can be edited, of three if it also acts as a butto _and_ 
    ///     can be edited.
    /// A PopupMenuButton without any images will act as a 'normal' popup menu and show up where the pointer is clicked.
    /// If a PopupMenuButton has images, the last one is for hover effect. If you do not want hover effect, just supply a 
    /// transParent image as last image.
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
                if (ImageList != null)
                {
                    foreach (Image image in ImageList)
                    {
                        image.Visibility = value;
                    }
                }
                if (TextBlock != null)
                {
                    TextBlock.Visibility = value;
                }
                visible = value;
            }
        }
        private Visibility visible;

        public bool IsOn
        {
            get { return isOn; }
            set { Toggle(); }
        }

        public Int32 Value { get { return value; } set { this.value = value; SetPositionFromValue(); } }
        public Point ImageSize { get; set; }
        public Double OriginalImageWidth { get; set; }
        public Double OriginalImageHeight { get; set; }

        private Int32 value;
        public Double RelativeValue;


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
        //public PopupMenuButton Parent;
        public int MenuNumber;
        public int MenuItemNumber;
        //public HorizontalSlider slider;
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
            PopupMenuButtonStyle style, PointerButton[] buttons = null, Int32 MinValue = 0, Int32 MaxValue = 127,
			string text = null, int fontSize = 16, bool edit = false, ControlTextWeight textWeight = ControlTextWeight.NORMAL, 
            ControlTextAlignment textAlignment = ControlTextAlignment.LEFT,
            Brush textOnColor = null, Brush textOffColor = null)
        {
            Parent = null;
            Style = style;
            MenuNumber = -1;
            MenuItemNumber = -1;
            Double width;
            Double height;
            xOffset = yOffset = ySpacing = 0.0;
            IsEditing = false;

            this.controls = controls;
            this.gridControls = gridControls;




            if (style == PopupMenuButtonStyle.SLIDER)
            {
                if (imageList.Count() < 2)
                {
                    throw new InvalidOperationException(
                        "You need two images to make a menu item with a slider. One background and one handle");
                }
                //slider = AddHorizontalSlider(Id, gridControls, 
                //    imageList, new Rect(pos, new Size(ImageList[0].Width, ImageList[0].Height)), 0, 255);

                ImageSize = new Point(imageList[1].ActualWidth,
                    imageList[1].ActualHeight);
                OriginalImageWidth = imageList[1].ActualWidth;
                OriginalImageHeight = imageList[1].ActualHeight;
                this.MinValue = MinValue;
                this.MaxValue = MaxValue;
            }



            this.Id = Id;
            this.Parent = null;
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

                if (ImageList.Length > 1 && style != PopupMenuButtonStyle.SLIDER)
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
                    else if (style != PopupMenuButtonStyle.SLIDER)
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
                this.ButtonMap = new PointerButton[] 
                { 
                    PointerButton.LEFT, PointerButton.RIGHT, 
                    PointerButton.MIDDLE, PointerButton.EXTRA1, 
                    PointerButton.EXTRA2
                };
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
            PopupMenuButtonStyle style, PointerButton[] buttons = null, 
            string text = null, int fontSize = 16, bool edit = false, 
            ControlTextWeight textWeight = ControlTextWeight.NORMAL, 
            ControlTextAlignment textAlignment = ControlTextAlignment.LEFT,
            double xOffset = -1.0, double yOffset = 0.0, double ySpacing = 0.0, 
            Brush textOnColor = null, Brush textOffColor = null)
        {
            Point pos = new Point(HitArea.X + xOffset * HitArea.Width, 
                HitArea.Y + yOffset * HitArea.Height + itemNumber * 
                (1.0 + ySpacing) * HitArea.Height);

            PopupMenuButton control = new PopupMenuButton(controls, Id, 
                gridControls, imageList, pos, style, buttons, 0, 127, text, fontSize, 
                edit, textWeight, textAlignment, textOnColor, textOffColor);

            control.MenuNumber = menuNumber;
            control.MenuItemNumber = itemNumber;

            //// Only menu ITEM style needs to be turned off
            //if (control.style != PopupMenuButtonStyle.ITEM)
            //{
            //    control.Set(true);
            //}

            this.xOffset = xOffset;
            this.yOffset = yOffset;
            this.ySpacing = ySpacing;
            control.Visibility = Visibility.Collapsed;
            control.Parent = this;
            Children[menuNumber].Add(control);
            controls.ControlsList.Add(control);
            return control;
        }

        //public HorizontalSlider AddHorizontalSlider(int Id, Grid gridControls, Image[] imageList, Rect hitArea,
        //    int MinValue = 0, int MaxValue = 127)
        //{
        //    HorizontalSlider control = new HorizontalSlider(controls, Id, gridControls, imageList,
        //        new Rect(this.HitArea.Left + hitArea.Left, this.HitArea.Top + hitArea.Top, hitArea.Width, hitArea.Height),
        //        MinValue, MaxValue);
        //    controls.ControlsList.Add(control);
        //    return control;
        //}

        public void ShowSubMenu(int menu)
        {
            foreach (PopupMenuButton menuItem in Children[menu])
            {
                menuItem.Visibility = Visibility.Visible;
            }
        }

        public void HideAllMenus()
        {
            //foreach (object obj in controls.ControlsList)
            //{
            //    if (obj.GetType() == typeof(PopupMenuButton))
            //    {
            //        HideAllSubMenus((PopupMenuButton)obj);
            //        ((PopupMenuButton)obj).Visibility = Visibility.Collapsed;
            //    }
            //}
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
            if (Parent != null)
            {
                if (menu < Parent.Children.Count)
                {
                    // Last menu item top must not be less or equal to than Parents top AND
                    // first menu item top must be less than Parent top AND menu item must
                    // fit in imgClickarea:
                    if ((offset > 0 // Scrolling up
                            && Parent.Children[menu][Parent.Children[menu].Count - 1].HitArea.Top > Parent.HitArea.Top
                            && Parent.Children[menu][0].HitArea.Top > Parent.Children[menu][0].HitArea.Height
                            && Parent.Children[menu][0].HitArea.Top > Parent.Children[menu][0].HitArea.Height + 34)
                      || offset < 0 // Scrolling down
                            && Parent.Children[menu][0].HitArea.Top < Parent.HitArea.Top
                            && Parent.Children[menu][Parent.Children[menu].Count - 1].HitArea.Bottom > Parent.Children[menu][0].HitArea.Height)
                    {
                        offset *= Parent.HitArea.Height;
                        //Point pos = new Point(HitArea.X + xOffset * HitArea.Width, HitArea.Y + yOffset * HitArea.Height + itemNumber * (1.0 + ySpacing) * HitArea.Height);
                        foreach (PopupMenuButton menuItem in Parent.Children[menu])
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
                    if (ImageList != null)
                    {
                        if (onImage > -1)
                        {
                            ImageList[onImage].Visibility = Visibility.Visible;
                        }
                        else
                        {
                            ImageList[0].Visibility = Visibility.Visible;
                        }
                    }
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
                    if (ImageList != null)
                    {
                        if (onImage > -1)
                        {
                            ImageList[onImage].Visibility = Visibility.Collapsed;
                        }
                        else
                        {
                            ImageList[0].Visibility = Visibility.Collapsed;
                        }
                    }
                }
                else
                {
                    TextBlock.Foreground = TextOffColor;
                }
            }
        }

        public void Set(bool On)
        {
            isOn = On;
            if (isOn)
            {
                if (string.IsNullOrEmpty(Text))
                {
                    if (ImageList != null)
                    {
                        if (onImage > -1)
                        {
                            ImageList[onImage].Visibility = Visibility.Visible;
                        }
                        else
                        {
                            ImageList[0].Visibility = Visibility.Visible;
                        }
                    }
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
                    if (ImageList != null)
                    {
                        if (onImage > -1)
                        {
                            ImageList[onImage].Visibility = Visibility.Collapsed;
                        }
                        else
                        {
                            ImageList[0].Visibility = Visibility.Collapsed;
                        }
                    }
                }
                else
                {
                    TextBlock.Foreground = TextOffColor;
                }
            }
        }

        public void ResetHovering()
        {
            if (Style != PopupMenuButtonStyle.SLIDER && hoverImage > -1)
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

        public Int32 SetValue(Point position)
        {
            if (ImageList.Count() > 1)
            {
                // Limit handle space to let edges reach Hitarea edges, not center of handle:
                // From background image or HitArea left to handle center:
                Int32 left = (Int32)(ControlSizing.HitArea.Left + ImageList[1].ActualWidth / 2);
                // From background image or HitArea Right to handle center:
                Int32 right = (Int32)(ControlSizing.HitArea.Right - ImageList[1].ActualWidth / 2);
                Value = MaxValue -
                    // Distance between rightmost position and pointer position:
                    (Int32)(((float)right - (float)position.X) /
                    // Total span between handle space limits:
                    ((float)right - (float)left) *
                    // Value range:
                    (1.0 + (float)MaxValue - (float)MinValue + 1));

                Value = Value > MaxValue ? MaxValue : Value;
                Value = Value < MinValue ? MinValue : Value;
                SetPositionFromValue();
            }
            return Value;
        }

        public void SetPositionFromValue()
        {
            if (ControlGraphicsFollowsValue)
            {
                RelativeValue = ((Double)value - (Double)MinValue) / ((Double)MaxValue - (Double)MinValue);
                ControlSizing.Controls.CalculateExtraMargins(Controls.AppSize);
                ControlSizing.UpdatePositions();
            }
        }

        public void HandleEvent(PointerRoutedEventArgs e, EventType eventType, Point pointerPosition, List<ControlBase.PointerButton> PointerButtonStates,int delta, int menuNumber = 0)
        {
            switch (eventType)
            {
                case EventType.POINTER_MOVED:
                    HandlePointerMovedEvent(e, eventType, pointerPosition, PointerButtonStates);
                    break;
                case EventType.POINTER_PRESSED:
                    HandlePointerPressedEvent(e, eventType, PointerButtonStates);
                    break;
                case EventType.POINTER_WHEEL_CHANGED:
                    HandlePointerWheelChangedEvent(delta, menuNumber, PointerButtonStates);
                    break;
            }
        }

        public void HandlePointerMovedEvent(PointerRoutedEventArgs e, EventType eventType, Point pointerPosition, List<ControlBase.PointerButton> PointerButtonStates)
        {
            if (PointerButtonStates.Contains(PointerButton.LEFT))
            {
                SetValue(pointerPosition);
                SetPositionFromValue();
            }

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
                    for (int map = 0; map < ButtonMap.Length; map++)
                    {
                        if (PointerButtonStates[button] == ButtonMap[map])
                        {
                            buttonPressed = map;
                            break;
                        }
                    }
                    //if (PointerButtonStates.Count > 0 && PointerButtonStates[button] == ButtonMap[0])
                    //{
                    //    buttonPressed = button;
                    //    break;
                    //}
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
                            if (Children != null && Children.Count > 0 && buttonPressed <= Children.Count)
                            {
                                // If any of the items in the menu given is visible, then all of them are.
                                closed = Children[buttonPressed - 1][0].Visibility == Visibility.Collapsed;

                                // Hide any previously visible sub menus:
                                controls.CloseAllMenuItems();
                                //HideAllSubMenus(this);

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
                        if (Children != null && Children.Count > 0 && buttonPressed < Children.Count)
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
                    else if (Style == PopupMenuButtonStyle.ITEM)
                    {
                        if (Parent != null)
                        {
                            foreach (List<PopupMenuButton> menuItems in Parent.Children)
                            {
                                for (int menu = 0; menu < menuItems.Count; menu++)
                                {
                                    if (this == menuItems[menu])
                                    {
                                        menuItems[menu].Set(true);
                                    }
                                    else
                                    {
                                        if (menuItems[menu].Style == PopupMenuButtonStyle.ITEM)
                                        menuItems[menu].Set(false);
                                    }
                                }
                            }
                        }
                        //// If any of the items in the menu given is visible, then all of them are.
                        //if (Children != null && Children.Count > 0)
                        //{
                        //    closed = Children[buttonPressed][0].Visibility == Visibility.Collapsed;

                        //    // Hide any previously visible sub menus:
                        //    HideAllSiblingsSubMenus(this);
                        //    HideAllSubMenus(this);

                        //    if (closed)
                        //    {
                        //        // Show menu:
                        //        ShowSubMenu(buttonPressed);
                        //    }
                        //}
                    }
                }
            }
        }

        public void HandlePointerWheelChangedEvent(int delta, int menu, List<ControlBase.PointerButton> PointerButtonStates)
        {
            if (Style != PopupMenuButtonStyle.SLIDER || PointerButtonStates.Contains(PointerButton.OTHER))
            {
                ScrollMenu(menu, delta);
            }

            if (Style == PopupMenuButtonStyle.SLIDER)
            {
                if (PointerButtonStates.Contains(PointerButton.LEFT))
                {
                    delta *= 4;
                }
                if (PointerButtonStates.Contains(PointerButton.RIGHT))
                {
                    delta *= 8;
                }
                value += delta;
                value = value > MaxValue ? MaxValue : value;
                value = value < MinValue ? MinValue : value;
                SetPositionFromValue();
            }
        }
    }
}
