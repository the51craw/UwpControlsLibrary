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
using static UwpControlsLibrary.ControlBase;
using Windows.UI.Text;
using static UwpControlsLibrary.ImageButton;
using Windows.Devices.Midi;

namespace UwpControlsLibrary
{
    public partial class Controls
    {
        public ControlsUnderPointer ControlsHit { get { return controlsHit; } }
        public ControlsUnderPointer controlsHit;

        /// <summary>
        /// To avoid changing value other controls than the intended control,
        /// set Current to true on PointerPressed and false on PointerReleased.
        /// Then do not send PointerMoved to controls that are not the Current one.
        /// </summary>
        public int Current { get; set; }

        /// <summary>
        /// ExtraMarginX are the empty space to the left and right of
        /// the background image when application size is set to a height/width
        /// ratio less than the height/width ratio of imgClickArea. Used to
        /// calculate extra left and right space for control images Margin property.
        /// </summary>
        public Double ExtraMarginX { get; set; }

        /// <summary>
        /// ExtraMarginY are the empty space at top and bottom of
        /// the background image when application size is set to a height/width
        /// ratio greater than the height/width ratio of imgClickArea. Used to
        /// calculate extra top and bottom space for control images Margin property.
        /// </summary>
        public Double ExtraMarginY { get; set; }

        /// <summary>
        /// Original width of imgClickArea.
        /// </summary>
        public Double OriginalWidth { get; set; }

        /// <summary>
        /// Original height of imgClickArea.
        /// </summary>
        public Double OriginalHeight { get; set; }

        /// <summary>
        /// When application size is changed, imgClickArea changes because it
        /// is set to have Stretch = Uniform. Control images margin property
        /// is set accordingly using their RelativeHitAreaSize setting.
        /// </summary>
        public Image imgClickArea { get; set; }

        public static Rect AppSize { get; set; }

        public List<Object> ControlsList;
        //private List<Image> imagesToHide;
        //private List<TextBlock> textBlocksToHide;

        public Controls(Rect AppSize, Image ClickArea)
        {
            AppSize = new Rect(AppSize.Left, AppSize.Top, AppSize.Width, AppSize.Height);
            imgClickArea = ClickArea;
            OriginalWidth = imgClickArea.ActualWidth;
            OriginalHeight = imgClickArea.ActualHeight;
            ControlsList = new List<Object>();
            //imagesToHide = new List<Image>();
            //textBlocksToHide = new List<TextBlock>();
        }

        public void Init(Grid gridMain)
        {
            imgClickArea.Stretch = Stretch.Uniform;
            imgClickArea.UpdateLayout();
            CalculateExtraMargins(AppSize);
            //AddControlsToHide(gridMain);
        }

        public StaticImage AddStaticImage(int Id, Grid gridMain, Image[] imageList, Point position)
        {
            StaticImage staticImage = new StaticImage(this, Id, gridMain, imageList, position);
            //StaticImage staticImage = new StaticImage(this, Id, image, position);
            ControlsList.Add(staticImage);
            return staticImage;
        }

        public Rotator AddRotator(int Id, Grid gridMain, Image[] imageList, Point position,
            string text = "", Int32 fontSize = 8, TextAlignment textAlignment = TextAlignment.Center,
            ControlTextWeight textWeight = ControlTextWeight.NORMAL,
            TextWrapping textWrapping = TextWrapping.NoWrap, Brush foreground = null)
        {
            Rotator rotator = new Rotator(this, Id, gridMain, imageList, position, text, fontSize,
                textAlignment, textWeight, textWrapping, foreground);
            ControlsList.Add(rotator);
            return rotator;
        }

        public ImageButton AddImageButton(int Id, Grid gridMain, Image[] imageList, Point position, ImageButton.ImageButtonFunction function = ImageButtonFunction.TOGGLE)
        {
            ImageButton button = new ImageButton(this, Id, gridMain, imageList, position, function);
            ControlsList.Add(button);
            return button;
        }

        public Indicator AddIndicator(int Id, Grid gridMain, Image[] imageList, Point position)
        {
            Indicator control = new Indicator(this, Id, gridMain, imageList, position);
            ControlsList.Add(control);
            return control;
        }

        public Knob AddKnob(int Id, Grid gridMain, Image[] imageList, Point position,
            int MinValue = 0, int MaxValue = 127, int AngleStart = 45, int AngleEnd = 315, Double Sensitivity = 1)
        {
            Knob control = new Knob(this, Id, gridMain, imageList, position, MinValue, MaxValue, AngleStart, AngleEnd, Sensitivity);
            ControlsList.Add(control);
            return control;
        }

        public DigitalDisplay AddDigitalDisplay(int Id, Grid gridControls, Image[] imageList, Point position, int numberOfDigits, int numberOfDecimals)
        {
            DigitalDisplay digitalDisplay = new DigitalDisplay(this, Id, gridControls, imageList, position, numberOfDigits, numberOfDecimals);
            ControlsList.Add(digitalDisplay);
            return digitalDisplay;
        }

        public TouchpadKeyboard AddTouchpadKeyboard(int Id, Grid gridMain,
            Image[] imageList, Point position, byte lowKey, byte highKey)
        {
            TouchpadKeyboard control = new TouchpadKeyboard(this, Id, gridMain, imageList, position, lowKey, highKey);
            ControlsList.Add(control);
            return control;
        }

        public VerticalSlider AddVerticalSlider(int Id, Grid gridMain, Image[] imageList, Rect HitArea,
            int MinValue = 0, int MaxValue = 127)
        {
            VerticalSlider control = new VerticalSlider(this, Id, gridMain, imageList, HitArea, MinValue, MaxValue);
            ControlsList.Add(control);
            return control;
        }

        public HorizontalSlider AddHorizontalSlider(int Id, Grid gridMain, Image[] imageList, Rect HitArea,
            int MinValue = 0, int MaxValue = 127)
        {
            HorizontalSlider control = new HorizontalSlider(this, Id, gridMain, imageList, HitArea, MinValue, MaxValue);
            ControlsList.Add(control);
            return control;
        }

        public Joystick AddJoystick(int Id, Grid gridMain, Image[] imageList, Rect hitArea,
            int MinValueX = 0, int MaxValueX = 127, int MinValueY = 0, int MaxValueY = 127)
        {
            Joystick control = new Joystick(this, Id, gridMain, imageList, hitArea, MinValueX, MaxValueX, MinValueY, MaxValueY);
            ControlsList.Add(control);
            return control;
        }

        public Label AddLabel(int Id, Grid gridMain, Rect HitArea, string text, int fontSize = 8,
            TextAlignment textAlignment = TextAlignment.Center, ControlTextWeight textWeight = ControlTextWeight.NORMAL,
            TextWrapping textWrapping = TextWrapping.NoWrap, Brush foreground = null)
        {
            Label control = new Label(this, Id, gridMain, HitArea, text, fontSize, textAlignment, textWeight, textWrapping, foreground);
            ControlsList.Add(control);
            return control;
        }

        public Graph AddGraph(int Id, Grid gridControls, Image[] imageList, Point Position, Brush Color, int LineWidth = 2)
        {
            Graph control = new Graph(this, Id, gridControls, imageList, Position, Color, LineWidth);
            ControlsList.Add(control);
            return control;
        }

        public TreeViewBase AddTreeView(int Id, Grid gridControls, Image[] imageList, Point position)
        {
            TreeViewBase control = new TreeViewBase(this, Id, gridControls, imageList, position);
            ControlsList.Add(control);
            return control;
        }

        public PopupMenuButton AddPopupMenuButton(int Id, Grid gridControls, Image[] imageList, Point position,
            PopupMenuButtonStyle style, PointerButton[] buttons = null, string text = null, 
            int fontSize = 8, ControlTextWeight textWeight = ControlTextWeight.NORMAL,
            ControlTextAlignment textAlignment = ControlTextAlignment.LEFT, Brush textOnColor = null, Brush textOffColor = null)
        {
            if (buttons == null)
            {
                buttons = new ControlBase.PointerButton[] { ControlBase.PointerButton.LEFT };
            }

            PopupMenuButton control = new PopupMenuButton(this, Id, gridControls, imageList, position,
                style, buttons, text, fontSize, textWeight, textAlignment, textOnColor, textOffColor);
            ControlsList.Add(control);
            return control;
        }

        public ToolTips AddToolTips(int Id, Grid gridToolTip, int timeOut = 2, int fontSize = 8,
            ControlTextWeight textWeight = ControlTextWeight.NORMAL,
            Brush BackgroundColor = null)
        {
            ToolTips control = new ToolTips(this, Id, gridToolTip, timeOut, fontSize, textWeight, BackgroundColor);
            ControlsList.Add(control);
            return control;
        }


        public UwpControlsLibrary.CompoundControl AddCompoundControl(Rect AppSize, Image ClickArea, int Id, int SubType, Grid gridMain, Image[] imageList, Rect HitArea)
        {
            UwpControlsLibrary.CompoundControl control = new UwpControlsLibrary.CompoundControl(this, AppSize, ClickArea, Id, SubType, gridMain, imageList, HitArea);
            ControlsList.Add(control);
            return control;
        }

        public Keyboard AddKeyBoard(int Id, Grid gridMain, Image[] imageList, Point Position, int lowKey, int highKey)
        {
            Keyboard control = new Keyboard(this, Id, gridMain, imageList, Position, lowKey, highKey);
            ControlsList.Add(control);
            return control;
        }

        public Keyboard AddKeyBoard(int Id, Grid gridMain, Image whiteKey, Image blackKey, Point Position, int lowKey, int highKey)
        {
            Keyboard control = new Keyboard(this, Id, gridMain, new Image[] { whiteKey, blackKey }, Position, lowKey, highKey);
            ControlsList.Add(control);
            return control;
        }

        public void AddMIDI(int Id, TypedEventHandler<MidiInPort, MidiMessageReceivedEventArgs> inPort_MessageReceived)
        {
            ControlsList.Add(new MIDI(inPort_MessageReceived));
        }

        //public void AddControlsToHide(Grid gridControls)
        //{
        //    imagesToHide.Clear();
        //    textBlocksToHide.Clear();
        //    foreach (Object obj in gridControls.Children)
        //    {
        //        if (obj.GetType() == typeof(Image))
        //        {
        //            imagesToHide.Add((Image)obj);
        //        }
        //        else if (obj.GetType() == typeof(TextBlock))
        //        {
        //            textBlocksToHide.Add((TextBlock)obj);
        //        }
        //    }
        //}

        public void ResizeControls(Grid mainGrid, Rect AppSize)
        {
            CalculateExtraMargins(AppSize);
            foreach (Object control in ControlsList)
            {
                ((ControlBase)control).ControlSizing.UpdatePositions();
            }
        }

        public void SetControlsUniform(Grid mainGrid)
        {
            foreach (Object control in mainGrid.Children)
            {
                if (control.GetType() == typeof(Image))
                {
                    ((Image)control).Stretch = Stretch.Uniform;
                }
            }

            foreach (Object control in ControlsList)
            {
                if (control.GetType() == typeof(Keyboard))
                {
                    for (int octave = 0; octave < ((Keyboard)control).Octaves.Length; octave++)
                    {
                        for (int key = 0; key < ((Keyboard)control).Octaves[octave].Keys.Length; key++)
                        {
                            for (int i = 0; i < ((Keyboard)control).Octaves[octave].Keys[key].Images.Length; i++)
                            {
                                ((Keyboard)control).Octaves[octave].Keys[key].Images[i].Stretch = Stretch.Uniform;
                            }
                        }
                    }
                }
                else if (control.GetType() == typeof(Label))
                {
                    ((Label)control).ControlSizing.TextBlock.HorizontalAlignment = HorizontalAlignment.Stretch;
                    ((Label)control).ControlSizing.TextBlock.VerticalAlignment = VerticalAlignment.Stretch;
                }
                else if (control.GetType() == typeof(UwpControlsLibrary.CompoundControl))
                {
                    foreach (object ctrl in ((UwpControlsLibrary.CompoundControl)control).SubControls.ControlsList)
                    {
                        if (ctrl.GetType() == typeof(Image))
                        {
                            ((Image)ctrl).Stretch = Stretch.Uniform;
                        }
                        //else if (ctrl.GetType() == typeof(Canvas))
                        //{
                        //    ((Canvas)ctrl). = Stretch.Uniform;
                        //}

                    }
                }
                else
                {
                    if (((ControlBase)control).ControlSizing.ImageList != null)
                    {
                        foreach (Image image in ((ControlBase)control).ControlSizing.ImageList)
                        {
                            image.Stretch = Stretch.Uniform;
                        }
                    }
                }
            }
        }

        public int FindControl(Point mousePosition)
        {
            foreach (Object control in ControlsList)
            {
                if (control.GetType() == typeof(UwpControlsLibrary.CompoundControl))
                {
                    foreach (Object subControl in ((UwpControlsLibrary.CompoundControl)control).SubControls.ControlsList)
                    {
                        if (((ControlBase)subControl).ControlSizing.IsHit(mousePosition))
                        {
                            return ((ControlBase)subControl).Id;
                        }
                    }
                }
                if (((ControlBase)control).ControlSizing.IsHit(mousePosition))
                {
                    return ((ControlBase)control).Id;
                }
            }
            return -1;
        }

        public Object GetControl(int control)
        {
            if (control < ControlsList.Count)
            {
                return ControlsList[control];
            }
            return null;
        }

        public void CalculateExtraMargins(Rect AppSize)
        {
            // Calculate the extra margins outside the workarea (when the window is set to
            // a size where the background and clickarea images leaves space at top and bottom
            // or at left and right side):
            Controls.AppSize = AppSize;
            ExtraMarginX = 0;
            ExtraMarginY = 0;
            if (imgClickArea.ActualWidth < AppSize.Width)
            {
                ExtraMarginX = (AppSize.Width - imgClickArea.ActualWidth) / 2;
            }
            if (imgClickArea.ActualHeight < AppSize.Height)
            {
                ExtraMarginY = (AppSize.Height - imgClickArea.ActualHeight) / 2;
            }
        }
    }
}
