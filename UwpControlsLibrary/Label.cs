using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Xaml.Controls;

namespace UwpControlsLibrary
{
    public class Label : ControlBase
    {
        public Label(Controls controls, Int32 Id, Grid gridMain, Rect hitArea, TextBlock textBlock, Int32 FontSize)
        {
            this.Id = Id;
            GridControls = gridMain;
            Double width = hitArea.Width;
            Double height = hitArea.Height;
            HitTarget = true;

            if (textBlock != null)
            {
                TextBlock = new TextBlock();
                TextBlock.Text = textBlock.Text;
                TextBlock.FontSize = FontSize;
                TextBlock.FontWeight = textBlock.FontWeight;
                TextBlock.Foreground = textBlock.Foreground;
                TextBlock.TextAlignment = textBlock.TextAlignment;
                TextBlock.TextWrapping = textBlock.TextWrapping;
                OriginalFontSize = FontSize;
            }

            HitArea = new Rect(hitArea.Left, hitArea.Top, width, height);
            ControlSizing = new ControlSizing(controls, this);
        }
    }
}
