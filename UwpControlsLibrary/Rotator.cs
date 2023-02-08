using System;
using System.Collections.Generic;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;

namespace UwpControlsLibrary
{
    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    /// Rotator class.
    /// Rotator class.The Rotator uses a number of images of the same size each representing a value
    /// from zero to number of images minus one. Tapping advances one up, restarting at zero after last value.
    /// Mouse wheel scrolls up and down but does not restart at end or zero, but is limited at both ends.
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
            }
        }
        private int selection;

        public Brush Foreground
        {
            get
            {
                return foreground;
            }
            set
            {
                if (!string.IsNullOrEmpty(Text))
                {
                    TextBlock.Foreground = value;
                    foreground = value;
                }
            }
        }
        private Brush foreground;

        public string Text
        {
            get
            {
                return TextBlock.Text;
            }
            set
            {
                TextBlock.Text = value;
            }
        }

        public List<string> Texts;		
        int fontSize;
        public ControlTextWeight TextWeight;
        public TextAlignment TextAlignment;
        public Rotator(Controls controls, int Id, Grid gridControls, Image[] imageList, Point Position,
            string text = null, Int32 fontSize = 16, TextAlignment textAlignment = TextAlignment.Center,
            ControlTextWeight textWeight = ControlTextWeight.NORMAL,
            TextWrapping textWrapping = TextWrapping.NoWrap, Brush foreground = null)
        {
            this.Id = Id;
            GridControls = gridControls;
            this.fontSize = fontSize;
            Double width;
            Double height;
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
                            || imageList[i - 1].ActualHeight != imageList[i].ActualHeight)
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
                throw new Exception("A Rotator must have a list of one or more images of the same size.");
            }

            HitArea = new Rect(Position.X, Position.Y, width, height);
            CopyImages(imageList);

            Texts = new List<string>();
            if (!string.IsNullOrEmpty(text))
            {
                Texts.Add(text);
                TextBlock = new TextBlock();
                TextBlock.Text = text;
                TextBlock.VerticalAlignment = VerticalAlignment.Center;
                if (textWeight == ControlTextWeight.BOLD)
                {
                    TextBlock.FontWeight = Windows.UI.Text.FontWeights.Bold;
                }
                OriginalFontSize = fontSize;
                TextBlock.FontSize = OriginalFontSize;

                if (foreground != null)
                {
                    TextBlock.Foreground = foreground;
                }
                else
                {
                    TextBlock.Foreground = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255));
                }

                if (TextWeight == ControlTextWeight.BOLD)
                {
                    TextBlock.FontWeight = Windows.UI.Text.FontWeights.Bold;
                }

                TextBlock.TextAlignment = textAlignment;
            }

            ControlSizing = new ControlSizing(controls, this);
            Selection = 0;
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
                else
                {
                    if (selection < Texts.Count && selection > -1)
                    {
                        Text = Texts[selection];
                    }
                }
            }
        }

        public void SetDeSelected()
        {
        }

        public void HandleEvent(List<PointerButton> pointerButtonStates, int delta = 0)
        {
            if (pointerButtonStates.Contains(PointerButton.LEFT))
            {
                if (ImageList.Length > 1)
                {
                    Selection = selection + 1 >= ImageList.Length ? 0 : selection + 1;
                }
                else if (Texts.Count > 1)
                {
                    Selection = selection + 1 >= Texts.Count ? 0 : selection + 1;
                }
                ShowSelection();
            }
            else if (pointerButtonStates.Contains(PointerButton.RIGHT))
            {
                if (ImageList.Length > 1)
                {
                    Selection = selection - 1 < 0 ? ImageList.Length - 1 : selection - 1;
                }
                else if (Texts.Count > 1)
                {
                    Selection = selection - 1 < 0 ? Texts.Count - 1 : selection - 1;
                }
                ShowSelection();
            }
            else if (delta != 0)
            {
                selection += delta;
                if (ImageList.Length > 1)
                {
                    Selection = selection > ImageList.Length - 1 ? ImageList.Length - 1 : selection;
                }
                else if (Texts.Count > 1)
                {
                    Selection = selection > Texts.Count - 1 ? Texts.Count - 1 : selection;
                }
                Selection = selection < 0 ? 0 : selection;
                ShowSelection();
            }
        }
    }
}
