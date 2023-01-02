using System;
using System.Collections.Generic;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;

namespace UwpControlsLibrary
{
    public class ToolTips : ControlBase
    {
        public string Text
        {
            get { return TextBlock.Text; }
            set { TextBlock.Text = value; }
        }

		public TextBlock TextBlock;
        private DispatcherTimer timer;
        private Grid gridToolTip;
	
        public ToolTips(Controls controls, Int32 Id, Grid gridToolTip, int timeOut = 2, int fontSize = 8, 
            ControlTextWeight textWeight = ControlTextWeight.NORMAL, 
            Brush BackgroundColor = null)
        {
            this.Id = Id;
            this.gridToolTip = gridToolTip;
            timer = new DispatcherTimer();
            timer.Interval = new TimeSpan(0, 0, timeOut);
            timer.Tick += Timer_Tick;
            TextBlock = new TextBlock();
            TextBlock.Text = "";
            TextBlock.FontSize = fontSize;
            TextBlock.HorizontalAlignment = HorizontalAlignment.Center;
            TextBlock.VerticalAlignment = VerticalAlignment.Center;
            if (textWeight == ControlTextWeight.BOLD)
            {
                TextBlock.FontWeight = Windows.UI.Text.FontWeights.Bold;
            }
            OriginalFontSize = fontSize;
            if (BackgroundColor != null)
            {
                gridToolTip.Background = BackgroundColor;
            }
            this.gridToolTip.Children.Add(TextBlock);
            this.gridToolTip.Visibility = Visibility.Collapsed;
            ControlSizing = new ControlSizing(controls, this);
        }

        private void Timer_Tick(object sender, object e)
        {
            timer.Stop();
            gridToolTip.Visibility = Visibility.Visible;
        }

        public void Show(object sender, PointerRoutedEventArgs e, string Text)
        {
            if (gridToolTip.Visibility == Visibility.Collapsed && !timer.IsEnabled)
            {
                PointerPoint pp = e.GetCurrentPoint((Image)sender);
                TextBlock.Text = Text;
                double width = 100;
                double height = 40;
                Rect rect = new Rect(pp.Position.X - width / 2, pp.Position.Y - height, width, height);
                gridToolTip.Margin = new Thickness(rect.X, rect.Y,
                    Controls.AppSize.Width - rect.X - rect.Width,
                    Controls.AppSize.Height - rect.Y - rect.Height);
                timer.Start();
            }
        }

        public void Hide()
        {
            gridToolTip.Visibility = Visibility.Collapsed;
        }
    }
}
