using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using UwpControlsLibrary;
using Windows.UI.Input;
using System.Diagnostics;
using System.Threading.Tasks;

// The Content Dialog item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace ContentDialogExample
{
    public sealed partial class ContentDialog1 : ContentDialog
    {
        MainPage mainPage;
        Controls Controls;
        Image[] oscillatorImage;
        int previousControl = -1;
        int currentControl;

        //Patch patch;
        Image[] channelImages;
        int oscillator;
        //Settings settings;

        CompoundControl[] guis;

        public ContentDialog1()
        {
            this.InitializeComponent();
            UpdateLayout();
        }

        // When imgClickArea is opened it has also got its size, so now
        // we can create the controls object:
        private void imgClickArea_ImageOpened(object sender, RoutedEventArgs e)
        {
            oscillatorImage = new Image[] { imgOscillator1, imgOscillator2, imgOscillator3, imgOscillator4, imgOscillator5,
                imgOscillator6, imgOscillator7, imgOscillator8, imgOscillator9, imgOscillator10, imgOscillator11, imgOscillator12 };

            // Create the controls object:
            Controls = new Controls(new Rect(20, 20, imgClickArea.ActualWidth, imgClickArea.ActualHeight), imgClickArea);
            Controls.Init(gridControls);
            guis = new CompoundControl[12];

            // Create all controls:
            int spaceX = 200;
            int spaceY = 150;

            Windows.UI.Xaml.Controls.Button button = new Windows.UI.Xaml.Controls.Button();
            button.Content = "Buttoncontent";
            gridControls.Children.Add(button);
            //Controls.AddRotator(0, gridControls, new Image[] { imgMidiSettingBackground, imgMidiSettingBackground }, new Point(14, 14));
            ImageCopy imageCopy = new ImageCopy(imgMidiSettingBackground);
            gridControls.Children.Add(imageCopy.Image);

            //Controls.AddStaticImage(0, gridControls, new Image[] { imgMidiSettingBackground }, new Point(14, 14));

            //for (int osc = 0; osc < 12; osc++)
            //{
            //    //Image temp = new ImageCopy(imgMidiSettingBackground).Image;
            //    //Controls.AddRotator(osc, gridControls, new Image[] { imgMidiSettingBackground, imgMidiSettingBackground }, new Point(14 + (osc % 4) * spaceX, 14 + (osc / 4) * spaceY));
            //    guis[osc] = Controls.AddCompoundControl(new Rect(0, 0, imgMidiSettingBackground.ActualWidth, imgMidiSettingBackground.ActualHeight),
            //        imgClickArea, osc, 0, gridControls, new Image[] { imgMidiSettingBackground },
            //        new Rect(14 + (osc % 4) * spaceX, 14 + (osc / 4) * spaceY, imgMidiSettingBackground.ActualWidth, imgMidiSettingBackground.ActualHeight));
            //    //guis[osc].AddIndicator(-1, gridControls, new Image[] { oscillatorImage[osc] }, new Point(14, 14));
            //}

            // Make sure all controls has the correct size and position:
            //Controls.HideOriginalControls();
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
                object obj = null;
                PointerPoint pp = e.GetCurrentPoint(imgClickArea);
                Double x = pp.Position.X;
                Double y = pp.Position.Y;

                Int32 previousControl = currentControl;
                currentControl = Controls.FindControl(pp.Position);
                if (currentControl > -1)
                {
                    Debug.WriteLine(currentControl.ToString());
                    PointerPoint currentPoint = e.GetCurrentPoint(null);
                    PointerPointProperties props = currentPoint.Properties;
                    if (props.IsLeftButtonPressed)
                    {
                        //obj = ((ControlBase)Controls.ControlsList[currentControl]).PointerMoved(new Point(x, y));
                    }
                }
            }
        }

        // Tapped event, handlers for controls that are supposed to be tapped:
        private void imgClickArea_Tapped(object sender, TappedRoutedEventArgs e)
        {
            Controls.Tapped(sender, e);
            //for (Int32 i = 0; i < gridControls.Children.Count; i++)
            //{
            //    if (gridControls.Children[i].GetType() == typeof(Image))
            //    {
            //        if (((Image)gridControls.Children[i]).Stretch == Stretch.Uniform)
            //        {
            //            ((Image)gridControls.Children[i]).Stretch = Stretch.Fill;
            //        }
            //        else
            //        {
            //            ((Image)gridControls.Children[i]).Stretch = Stretch.Uniform;
            //        }
            //    }
            //}

            //if (currentControl > -1)
            //{
            //    Object indicator;
            //    //Object label = Controls.ControlsList[(Int32)ControlId.LABEL];
            //    Object control = Controls.ControlsList[currentControl];
            //    Boolean isOn;

            //    switch (currentControl)
            //    {
            //        //case (Int32)ControlId.PUSHBUTTON:
            //        //    indicator = ((ControlBase)Controls.ControlsList[(Int32)ControlId.INDICATOR1]);
            //        //    if (indicator != null)
            //        //    {
            //        //        ((Indicator)indicator).IsOn = !((Indicator)indicator).IsOn;
            //        //        ((Label)label).TextBlock.Text = ((Indicator)indicator).IsOn.ToString();
            //        //    }
            //        //    break;
            //        //case (Int32)ControlId.SELECTOR:
            //        //    ((ControlBase)control).Tapped(sender, e);
            //        //    ((Label)Controls.ControlsList[(Int32)ControlId.LABEL]).TextBlock.Text =
            //        //        ((UwpControlsLibrary.Rotator)Controls.ControlsList[(Int32)ControlId.SELECTOR]).Selection.ToString();
            //        //    break;
            //    }
            //}
        }

        private void imgClickArea_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            //if (currentControl > -1)
            {
                Controls.PointerPressed(sender, e);
                //switch (currentControl)
                //{
                //}
            }
        }

        private void imgClickArea_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            Controls.PointerReleased(sender, e);
        }

        //private void imgClickArea_Tapped(object sender, TappedRoutedEventArgs e)
        //{

        //}

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


        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
        }

        private void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
        }
    }
}
