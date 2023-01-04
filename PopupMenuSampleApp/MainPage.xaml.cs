using System;
using UwpControlsLibrary;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Text;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace PopupMenuSampleApp
{
    public sealed partial class MainPage : Page
    {
        // The Controls object:
        Controls Controls;

        // Application controls:
        Knob volume;
        PopupMenuButton robot;
        PopupMenuButton megaphone;
        PopupMenuButton reverb;
        VerticalSlider verticalSlider;
        ToolTips toolTips;

        // PointerMoved will update this in order to let other handlers know which control is handled.
        private Int32 currentControl;

        public MainPage()
        {
            this.InitializeComponent();
        }

        // When imgClickArea is opened it has also got its size, so now
        // we can create the controls object:
        private void imgClickArea_ImageOpened(object sender, RoutedEventArgs e)
        {
            // Create the controls object:
            Controls = new Controls(Window.Current.Bounds, imgClickArea);
            Controls.Init(gridControls);

            // Create all controls:
            Int32 i = 0;
            volume = Controls.AddKnob(i++, gridControls, new Image[] { imgSmallKnob }, new Point(121, 158), 0, 127, 30, 330, 2);

            robot = Controls.AddPopupMenuButton(i++, gridControls, new Image[] { imgRobotOn, imgEffectButtonHover },
                new Point(69, 250), ControlBase.PopupMenuButtonStyle.BUTTON, new ControlBase.PointerButton[] { ControlBase.PointerButton.LEFT,
                ControlBase.PointerButton.RIGHT, ControlBase.PointerButton.OTHER });

            megaphone = Controls.AddPopupMenuButton(i++, gridControls, new Image[] { imgEffectButtonBackground, imgEffectButtonHover },
                new Point(214, 250), ControlBase.PopupMenuButtonStyle.BUTTON, new ControlBase.PointerButton[] { ControlBase.PointerButton.RIGHT,
                ControlBase.PointerButton.OTHER, ControlBase.PointerButton.LEFT }, "MEGAPHONE", 14, ControlBase.ControlTextWeight.BOLD,
                ControlBase.ControlTextAlignment.CENTER);
            megaphone.TextOnColor = new SolidColorBrush(Color.FromArgb(255, 129, 239, 214));
            megaphone.TextOffColor = new SolidColorBrush(Color.FromArgb(255, 98, 132, 159));

            reverb = Controls.AddPopupMenuButton(i++, gridControls, new Image[] { imgEffectButtonBackground, imgEffectButtonHover },
                new Point(960, 300), ControlBase.PopupMenuButtonStyle.BUTTON, new ControlBase.PointerButton[] { ControlBase.PointerButton.RIGHT,
                ControlBase.PointerButton.OTHER, ControlBase.PointerButton.LEFT }, "REVERB", 14, ControlBase.ControlTextWeight.BOLD, 
                ControlBase.ControlTextAlignment.CENTER);
            reverb.TextOnColor = new SolidColorBrush(Color.FromArgb(255, 129, 239, 214));
            reverb.TextOffColor = new SolidColorBrush(Color.FromArgb(255, 98, 132, 159));

            verticalSlider = Controls.AddVerticalSlider(i++, gridControls, new Image[] { imgSliderHandle }, new Rect(new Point(77, 375), new Size(88, 365)), 0, 127);

            toolTips = Controls.AddToolTips(i++, gridToolTip, 1, 14);

            int mi = 0;
            int m = robot.AddMenu();

            PopupMenuButton item;

            item = robot.AddMenuItem(m, mi++, robot, new Image[] { imgMenuBackground }, 1.0, ControlBase.PopupMenuButtonStyle.BUTTON, null, "Item 1", 14);
            item.TextBlock.Foreground = new SolidColorBrush(Color.FromArgb(255, 129, 239, 214));
            item = robot.AddMenuItem(m, mi++, robot, new Image[] { imgMenuBackground }, 1.0, ControlBase.PopupMenuButtonStyle.BUTTON, null, "Item 2", 14);
            item.TextBlock.Foreground = new SolidColorBrush(Color.FromArgb(255, 129, 239, 214));
            item = robot.AddMenuItem(m, mi++, robot, new Image[] { imgMenuBackground }, 1.0, ControlBase.PopupMenuButtonStyle.BUTTON, null, "Item 3", 14);
            item.TextBlock.Foreground = new SolidColorBrush(Color.FromArgb(255, 129, 239, 214));
            item = robot.AddMenuItem(m, mi++, robot, new Image[] { imgMenuBackground }, 1.0, ControlBase.PopupMenuButtonStyle.BUTTON, null, "Item 4", 14);
            item.TextBlock.Foreground = new SolidColorBrush(Color.FromArgb(255, 129, 239, 214));

            mi = 0;
            m = megaphone.AddMenu();
            PopupMenuButton subItem;

            item = megaphone.AddMenuItem(m, mi++, megaphone, new Image[] { imgMenuBackground }, 1.0, ControlBase.PopupMenuButtonStyle.BUTTON, null, "Item 1", 14);
            item.TextBlock.Foreground = new SolidColorBrush(Color.FromArgb(255, 129, 239, 214));
            item = megaphone.AddMenuItem(m, mi++, megaphone, new Image[] { imgMenuBackground }, 1.0, ControlBase.PopupMenuButtonStyle.BUTTON, null, "Item 2", 14);
            item.TextBlock.Foreground = new SolidColorBrush(Color.FromArgb(255, 129, 239, 214));
            subItem = item;
            item = megaphone.AddMenuItem(m, mi++, megaphone, new Image[] { imgMenuBackground }, 1.0, ControlBase.PopupMenuButtonStyle.BUTTON, null, "Item 3", 14);
            item.TextBlock.Foreground = new SolidColorBrush(Color.FromArgb(255, 129, 239, 214));
            item = megaphone.AddMenuItem(m, mi++, megaphone, new Image[] { imgMenuBackground }, 1.0, ControlBase.PopupMenuButtonStyle.BUTTON, null, "Item 4", 14);
            item.TextBlock.Foreground = new SolidColorBrush(Color.FromArgb(255, 129, 239, 214));

            mi = 0;
            m = reverb.AddMenu();

            item = reverb.AddMenuItem(m, mi++, reverb, new Image[] { imgMenuBackground }, 1.0, ControlBase.PopupMenuButtonStyle.BUTTON, null, "Item 1", 14,
                ControlBase.ControlTextWeight.BOLD, ControlBase.ControlTextAlignment.LEFT, ControlBase.PopupMenuPosition.LEFT);
            item.TextBlock.Foreground = new SolidColorBrush(Color.FromArgb(255, 129, 239, 214));
            item = reverb.AddMenuItem(m, mi++, reverb, new Image[] { imgMenuBackground }, 1.0, ControlBase.PopupMenuButtonStyle.BUTTON, null, "Item 2", 14,
                ControlBase.ControlTextWeight.BOLD, ControlBase.ControlTextAlignment.LEFT, ControlBase.PopupMenuPosition.LEFT);
            item.TextBlock.Foreground = new SolidColorBrush(Color.FromArgb(255, 129, 239, 214));
            item = reverb.AddMenuItem(m, mi++, reverb, new Image[] { imgMenuBackground }, 1.0, ControlBase.PopupMenuButtonStyle.BUTTON, null, "Item 3", 14,
                ControlBase.ControlTextWeight.BOLD, ControlBase.ControlTextAlignment.LEFT, ControlBase.PopupMenuPosition.LEFT);
            item.TextBlock.Foreground = new SolidColorBrush(Color.FromArgb(255, 129, 239, 214));
            item = reverb.AddMenuItem(m, mi++, reverb, new Image[] { imgMenuBackground }, 1.0, ControlBase.PopupMenuButtonStyle.BUTTON, null, "Item 4", 14,
                ControlBase.ControlTextWeight.BOLD, ControlBase.ControlTextAlignment.LEFT, ControlBase.PopupMenuPosition.LEFT);
            item.TextBlock.Foreground = new SolidColorBrush(Color.FromArgb(255, 129, 239, 214));

            mi = 0;
            m = subItem.AddMenu();

            item = subItem.AddMenuItem(m, mi++, subItem, new Image[] { imgMenuBackground }, 1.0, ControlBase.PopupMenuButtonStyle.BUTTON, null, "Item 1", 14);
            item.TextBlock.Foreground = new SolidColorBrush(Color.FromArgb(255, 129, 239, 214));
            item = subItem.AddMenuItem(m, mi++, subItem, new Image[] { imgMenuBackground }, 1.0, ControlBase.PopupMenuButtonStyle.BUTTON, null, "Item 2", 14);
            item.TextBlock.Foreground = new SolidColorBrush(Color.FromArgb(255, 129, 239, 214));
            item = subItem.AddMenuItem(m, mi++, subItem, new Image[] { imgMenuBackground }, 1.0, ControlBase.PopupMenuButtonStyle.BUTTON, null, "Item 3", 14);
            item.TextBlock.Foreground = new SolidColorBrush(Color.FromArgb(255, 129, 239, 214));
            item = subItem.AddMenuItem(m, mi++, subItem, new Image[] { imgMenuBackground }, 1.0, ControlBase.PopupMenuButtonStyle.BUTTON, null, "Item 4", 14);
            item.TextBlock.Foreground = new SolidColorBrush(Color.FromArgb(255, 129, 239, 214));

            // Make sure all controls has the correct size and position:
            Controls.ResizeControls(gridControls, Window.Current.Bounds);
            Controls.SetControlsUniform(gridControls);
            UpdateLayout();
        }

        // When app size is changed, all controls must also be resized,
        // ask the Controls object to do it:
        private void gridMain_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (Controls != null)
            {
                Controls.ResizeControls(gridMain, Window.Current.Bounds);
            }
        }

        // When the pointer is moved over the click-area, ask the Controls
        // object if and if so which control the pointer is over:
        private void imgClickArea_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            if (Controls != null)
            {
                Controls.PointerMoved(sender, e);
                if (robot.IsSelected)
                {
                    toolTips.Show(sender, e, "Robot!");
                }
                else
                {
                    toolTips.Hide();
                }
            }
        }

        // Tapped events, handlers for controls that are supposed to be tapped:
        private void imgClickArea_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (Controls != null)
            {
                Controls.Tapped(sender, e);
            }
        }

        private void imgClickArea_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            if (Controls != null)
            {
                Controls.RightTapped(sender, e);
            }
        }

        private void imgClickArea_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            if (Controls != null)
            {
                Controls.PointerPressed(sender, e);
            }
        }

        private void imgClickArea_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            Controls.PointerReleased(sender, e);
        }

        private void imgClickArea_PointerWheelChanged(object sender, PointerRoutedEventArgs e)
        {
            Controls.PointerWheelChanged(sender, e);
            //if (currentControl > -1)
            //{
            //    PointerPoint pp = e.GetCurrentPoint(imgClickArea);
            //    PointerPointProperties ppp = pp.Properties;
            //    Int32 delta = ppp.MouseWheelDelta;
            //    if (ppp.IsLeftButtonPressed)
            //    {
            //        delta = delta > 0 ? 5 : -5;
            //    }
            //    else
            //    {
            //        delta = delta > 0 ? 1 : -1;
            //    }
            //    Int32 value = (Int32)Controls.PointerWheelChanged(currentControl, delta);
            //}
        }
    }
}
