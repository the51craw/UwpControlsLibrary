using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace UwpControlsLibrary
{
    public class Label : ControlBase
    {
        public string Text { get { return TextBlock.Text; } set { TextBlock.Text = value; } }
        public Label(Controls controls, Int32 Id, Grid gridMain, Rect hitArea, string text = "", Int32 fontSize = 8,
            TextAlignment textAlignment = TextAlignment.Center,
            ControlTextWeight textWeight = ControlTextWeight.NORMAL,
            TextWrapping textWrapping = TextWrapping.NoWrap, Brush foreground = null)
        {
            this.Id = Id;
            GridControls = gridMain;
            Double width = hitArea.Width;
            Double height = hitArea.Height;

            TextBlock = new TextBlock();
            TextBlock.Text = text;
            TextBlock.FontSize = fontSize;
            if (textWeight == ControlTextWeight.BOLD)
            {
                TextBlock.FontWeight = Windows.UI.Text.FontWeights.Bold;
            }
            if (foreground != null)
            {
                TextBlock.Foreground = foreground;
            }
            else
            {
                TextBlock.Foreground = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255));
            }
            TextBlock.VerticalAlignment = VerticalAlignment.Center;
            TextBlock.TextAlignment = textAlignment;
            TextBlock.TextWrapping = textWrapping;
            OriginalFontSize = fontSize;

            HitArea = new Rect(hitArea.Left, hitArea.Top, width, height);
            ControlSizing = new ControlSizing(controls, this);
        }
    }
}
