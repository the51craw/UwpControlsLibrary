using System;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using UwpControlsLibrary;
using Windows.Devices.Midi;
using Windows.Foundation;
using Windows.Security.Cryptography;
using Windows.Storage.Streams;
using Windows.UI;
using Windows.UI.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace SampleApplication

 {
    public sealed partial class MainPage : Page
    {
        // The Controls object:
        Controls Controls;

        // This enum must contain one entry for each control!
        // You can use it in handlers to identify what action to take when control is used.
        public enum ControlId
        {
            STATICIMAGE,        // 0 
            GRAPH,              // 1
            DIGITAL_DISPLAY,    // 2
            INDICATOR1,         // 3 
            INDICATOR2,         // 4
            KNOB,               // 5
            VERTICAL_SLIDER,    // 6
            HORIZONTAL_SLIDER,  // 7
            LABEL,              // 8
            JOYSTICK,           // 9
            BUTTON,             // 10
            SELECTOR2,          // 11
            KEYBOARD,           // 12
            POPUP_MENU,         // 13 
        }

        public Point PointerPosition { get; set; }

        public Double WidthSizeRatio = 1;
        public Double HeightSizeRatio = 1;
        Boolean leftButtonIsPressed;
        Boolean rightButtonIsPressed;
        Boolean otherButtonIsPressed;
        Boolean ShiftIsPressed;
        Int32 currentMouseButton = 0;
        Point pointerPositionAtPointerPressed;
        private bool initDone = false;
        private Random random;
        private byte[] MidiInBuffer;
        private int midiInDeviceIndex = 0;
        private int midiOutDeviceIndex = 0;
        private int midiInChannel = 0;
        private int midiOutChannel = 0;

        DispatcherTimer graphAnimationTimer;
        double angle;
        double stepSize;
        int step;
        double x = 0, y = 0;

        MIDI midi;

        public MainPage()
        {
            this.InitializeComponent();
            random = new Random();
            midi = new MIDI(MidiInPort_MessageReceived);
        }

        // When imgClickArea is opened it has also got its size, so now
        // we can create the controls object:
        private async void imgClickArea_ImageOpened(object sender, RoutedEventArgs e)
        {
            // Create the controls object:
            Controls = new Controls(Window.Current.Bounds, imgClickArea);
            Controls.Init(gridControls);

            // Create all controls:
            Controls.AddStaticImage((int)ControlId.GRAPH, gridControls, new Image[] { imgMrMartin }, new Point(10, 560));

            Controls.AddGraph((int)ControlId.STATICIMAGE, gridControls, new Image[] { imgGraphBackground }, new Point(10, 420), new SolidColorBrush(Color.FromArgb(255, 0, 255, 0)), 2);

            Controls.AddDigitalDisplay((int)ControlId.DIGITAL_DISPLAY, gridControls,
                new Image[] { img0, img1, img2, img3, img4, img5, img6, img7, img8, img9, imgMinus, imgDot, imgGraphBackground }, new Point(10, 85), 4, 2);

            Controls.AddIndicator((int)ControlId.INDICATOR1, gridControls, new Image[] { imgIndicatorOff, imgIndicatorOn }, new Point(1118, 562));

            Controls.AddIndicator((int)ControlId.INDICATOR2, gridControls, new Image[] { imgIndicatorOff, imgIndicatorOn }, new Point(234, 28));

            Controls.AddKnob((int)ControlId.KNOB, gridControls, new Image[] { imgKnob, imgKnobBackground }, new Point(120, 320), 0, 127, 30, 330);

            Controls.AddVerticalSlider((int)ControlId.VERTICAL_SLIDER, gridControls, new Image[] { imgVerticalSliderBackground, imgVerticalSliderHandle },
                new Rect(270, 134, 121, 399));

            Controls.AddHorizontalSlider((int)ControlId.HORIZONTAL_SLIDER, gridControls, new Image[] { imgHorizontalSliderBackground, imgHorizontalSliderHandle },
                new Rect(427, 424, 399, 121), 0, 127);

            Controls.AddLabel((int)ControlId.LABEL, gridControls, new Rect(274, 80, 1500, 24), "text", 16, TextAlignment.Left);

            Controls.AddJoystick((int)ControlId.JOYSTICK, gridControls,
                new Image[] { imgJoystickBackground, imgStick1, imgStick2, imgStick3, imgJoystickHandle }, new Rect(495, 164, 199, 199), -64, 63, -64, 63);

            Controls.AddImageButton((int)ControlId.BUTTON, gridControls,
                new Image[] { imgImageButtonUpOff, imgImageButtonDownOff, imgImageButtonUpOn, imgImageButtonDownOn, imgImageButtonHover }, new Point(12, 19));

            Controls.AddRotator((int)ControlId.SELECTOR2, gridControls,
                new Image[] { imgSineWave, imgSawtoothWave, imgSquareWave, imgNoise }, new Point(273, 19));

            Controls.AddKeyBoard((int)ControlId.KEYBOARD, gridKeyboard, new Image[] { imgWhiteKey, imgBlackKey, imgWhiteKeyDown, imgBlackKeyDown }, new Point(144, 560), 36, 72);
            foreach (Octave octave in ((Keyboard)Controls.GetControl((Int32)ControlId.KEYBOARD)).Octaves)
            {
                foreach (Key key in octave.Keys)
                {
                    key.Images[key.Images.Length - 1].PointerMoved += Keyboard_PointerMoved;
                    key.Images[key.Images.Length - 1].PointerPressed += Keyboard_PointerPressed;
                    key.Images[key.Images.Length - 1].PointerReleased += Keyboard_PointerReleased;
                }
            }

            while (!midi.IsInitiated)
            {
                await Task.Delay(1);
            }


            int menu = 0;
            int menuItem = 0;
            PopupMenuButton midiMenu;
            PopupMenuButton midiInMenu;
            PopupMenuButton midiOutMenu;
            PopupMenuButton midiInChannelMenu;
            PopupMenuButton midiOutChannelMenu;
            midiMenu = Controls.AddPopupMenuButton((int)ControlId.POPUP_MENU, gridControls, new Image[] { imgPopupMenuButtonBackground, imgImageButtonHover }, new Point(512, 19),
                ControlBase.PopupMenuButtonStyle.BUTTON, new ControlBase.PointerButton[] { ControlBase.PointerButton.LEFT, ControlBase.PointerButton.RIGHT },
                "MIDI", 20, ControlBase.ControlTextWeight.BOLD, ControlBase.ControlTextAlignment.CENTER,
                new SolidColorBrush(Color.FromArgb(255, 255, 255, 255)), new SolidColorBrush(Color.FromArgb(255, 255, 255, 255))); // Always white. I will toggle text on pointer pressed instead.

            menuItem = 0;
            menu = midiMenu.AddMenu();
            midiInMenu = midiMenu.AddMenuItem(menu, menuItem++, midiMenu, new Image[] { imgPopupMenuButtonBackground, imgImageButtonHover },
                1.0, ControlBase.PopupMenuButtonStyle.MENU, new ControlBase.PointerButton[] { ControlBase.PointerButton.LEFT, ControlBase.PointerButton.RIGHT, ControlBase.PointerButton.OTHER },
                "MIDI in devices", 20, ControlBase.ControlTextWeight.BOLD, ControlBase.ControlTextAlignment.CENTER, ControlBase.PopupMenuPosition.RIGHT, 
                new SolidColorBrush(Color.FromArgb(255, 255, 255, 255)), new SolidColorBrush(Color.FromArgb(255, 255, 255, 255)));
            midiOutMenu = midiMenu.AddMenuItem(menu, menuItem++, midiMenu, new Image[] { imgPopupMenuButtonBackground, imgImageButtonHover },
                1.0, ControlBase.PopupMenuButtonStyle.MENU, new ControlBase.PointerButton[] { ControlBase.PointerButton.LEFT, ControlBase.PointerButton.RIGHT, ControlBase.PointerButton.OTHER }, 
                "MIDI out devices", 20, ControlBase.ControlTextWeight.BOLD, ControlBase.ControlTextAlignment.CENTER, ControlBase.PopupMenuPosition.RIGHT, 
                new SolidColorBrush(Color.FromArgb(255, 255, 255, 255)), new SolidColorBrush(Color.FromArgb(255, 255, 255, 255)));

            menuItem = 0;
            menu = midiInMenu.AddMenu();
            foreach (string midiInPortName in midi.MidiInPortNames)
            {
                midiInMenu.AddMenuItem(menu, menuItem++, midiInMenu, new Image[] { imgPopupMenuButtonBackground, imgImageButtonHover }, 1.0, ControlBase.PopupMenuButtonStyle.MENU,
                    new ControlBase.PointerButton[] { ControlBase.PointerButton.LEFT }, midiInPortName.ToString(), 20, ControlBase.ControlTextWeight.BOLD, ControlBase.ControlTextAlignment.LEFT);
            }

            menuItem = 0;
            menu = midiInMenu.AddMenu();
            for (int i = 0; i < 16; i++)
            {
                midiInMenu.AddMenuItem(menu, menuItem++, midiInMenu, new Image[] { imgPopupMenuButtonBackground, imgImageButtonHover }, 1.0, ControlBase.PopupMenuButtonStyle.MENU,
                    new ControlBase.PointerButton[] { ControlBase.PointerButton.LEFT }, "In ch " + (i + 1).ToString(), 20, ControlBase.ControlTextWeight.BOLD, ControlBase.ControlTextAlignment.LEFT, ControlBase.PopupMenuPosition.RIGHT);
            }

            menuItem = 0;
            menu = midiOutMenu.AddMenu();
            foreach (string midiOutPortName in midi.MidiOutPortNames)
            {
                midiOutMenu.AddMenuItem(menu, menuItem++, midiOutMenu, new Image[] { imgPopupMenuButtonBackground, imgImageButtonHover }, 1.0, ControlBase.PopupMenuButtonStyle.MENU,
                    new ControlBase.PointerButton[] { ControlBase.PointerButton.LEFT, ControlBase.PointerButton.OTHER }, midiOutPortName.ToString(), 20, ControlBase.ControlTextWeight.BOLD, ControlBase.ControlTextAlignment.LEFT);
            }

            menuItem = 0;
            menu = midiOutMenu.AddMenu();
            for (int i = 0; i < 16; i++)
            {
                midiOutMenu.AddMenuItem(menu, menuItem++, midiInMenu, new Image[] { imgPopupMenuButtonBackground, imgImageButtonHover }, 1.0, ControlBase.PopupMenuButtonStyle.MENU,
                    new ControlBase.PointerButton[] { ControlBase.PointerButton.LEFT }, "Out ch " + (i + 1).ToString(), 20, ControlBase.ControlTextWeight.BOLD, ControlBase.ControlTextAlignment.LEFT, ControlBase.PopupMenuPosition.RIGHT);
            }

            // Make sure all controls has the correct size and position:
            Controls.ResizeControls(gridControls, Window.Current.Bounds);
            Controls.SetControlsUniform(gridControls);
            //Controls.HideOriginalControls();
            UpdateLayout();
            initDone = true;

            angle = 0;
            stepSize = 0.001;
            step = 0;
            graphAnimationTimer = new DispatcherTimer();
            graphAnimationTimer.Interval = new TimeSpan(0, 0, 0, 0, 1);
            graphAnimationTimer.Tick += GraphAnimationTimer_Tick;
            graphAnimationTimer.Start();
        }

        private void GraphAnimationTimer_Tick(object sender, object e)
        {
            Point point;
            if (((ImageButton)Controls.ControlsList[(int)ControlId.BUTTON]).IsOn)
            {
                if (step == 0)
                {
                    ((Graph)Controls.ControlsList[(int)ControlId.GRAPH]).Points.Clear();
                }
                point = new Point((double)step + 5, y / 2.2 * -MakeWaveSample(((Rotator)Controls.ControlsList[(int)ControlId.SELECTOR2]).Selection) + 61);
                ((Graph)Controls.ControlsList[(int)ControlId.GRAPH]).AddPoint(point);
                angle += x * stepSize;
                angle = angle > 2 * Math.PI ? angle - 2 * Math.PI : angle;
                step++;
                step = step > 200 ? 0 : step;
                ((Graph)Controls.ControlsList[(int)ControlId.GRAPH]).Draw();

                ((DigitalDisplay)Controls.ControlsList[(int)ControlId.DIGITAL_DISPLAY])
                    .DisplayValue(y * MakeWaveSample(((Rotator)Controls.ControlsList[(int)ControlId.SELECTOR2]).Selection));
            }
        }
        public double MakeWaveSample(int waveForm)
        {
            switch (waveForm)
            {
                case 0:
                    return MakeSineWave();
                case 1:
                    return MakeSawDownWave();
                case 2:
                    return MakeSquareWave();
                case 3:
                    return MakeNoiseWave();
                default:
                    return 0;
            }
        }

        private float MakeSquareWave()
        {
            if (angle < Math.PI)
            {
                return -1.0f;
            }
            else
            {
                return 1.0f;
            }
        }

        /// <summary>
        /// Algorithm for generating a sawtooth down wave sample depending on the Angle in Radians.
        /// </summary>
        /// <param name="oscillator"></param>
        /// <returns>One sample of wave data</returns>
        private double MakeSawDownWave()
        {
            double value = 1.0 - angle / Math.PI;
            return value;
        }

        private double MakeSineWave()
        {
            return Math.Sin(angle);
        }

        private float MakeNoiseWave()
        {
            return 0.002f * (random.Next(1000) - 500);
        }


        // When the pointer is moved over the click-area, ask the Controls
        // object if and if so which control the pointer is over:
        private void imgClickArea_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            if (initDone && Controls != null)
            {
                Controls.PointerMoved(sender, e);
            }
            if (initDone)
            {
                GetMousePosition(e);
                GetMouseButtonPressed(e);

                ((Label)Controls.ControlsList[(Int32)ControlId.LABEL]).TextBlock.Text = PointerPosition.ToString();
                foreach (object control in Controls.ControlsList)
                {
                    if (((ControlBase)control).IsSelected)
                    {
                        bool hit =
                             (((ControlBase)control).HitArea.Left <= PointerPosition.X)
                          && (((ControlBase)control).HitArea.Right >= PointerPosition.X)
                          && (((ControlBase)control).HitArea.Top <= PointerPosition.Y)
                          && (((ControlBase)control).HitArea.Bottom >= PointerPosition.Y);
                        ((Label)Controls.ControlsList[(Int32)ControlId.LABEL]).TextBlock.Text =
                            ((ControlId)((ControlBase)control).Id).ToString()
                            + " (" + Math.Truncate(((ControlBase)control).HitArea.Left).ToString() + "," +
                                Math.Truncate(((ControlBase)control).HitArea.Top).ToString() + "," +
                                Math.Truncate(((ControlBase)control).HitArea.Right).ToString() + "," +
                                Math.Truncate(((ControlBase)control).HitArea.Bottom).ToString()
                            + ") (" + Math.Truncate(PointerPosition.X).ToString() + "," + Math.Truncate(PointerPosition.Y).ToString()
                            + ") " + hit.ToString();
                    }
                }

                if (Controls != null)
                {
                    Controls.PointerMoved(sender, e);
                    if (leftButtonIsPressed || rightButtonIsPressed || otherButtonIsPressed)
                    {
                        foreach (object control in Controls.ControlsList)
                        {
                            if (((ControlBase)control).IsSelected)
                            {
                                switch (((ControlBase)control).Id)
                                {
                                    case (Int32)ControlId.KNOB:
                                        y = ((Knob)Controls.ControlsList[(Int32)ControlId.KNOB]).Value;
                                        ((VerticalSlider)Controls.ControlsList[(Int32)ControlId.VERTICAL_SLIDER]).Value = (int)y;
                                        ((Joystick)Controls.ControlsList[(Int32)ControlId.JOYSTICK]).ValueY = (int)y - 64;
                                        ((Label)Controls.ControlsList[(Int32)ControlId.LABEL]).TextBlock.Text = "Value " +
                                        ((Knob)Controls.ControlsList[(Int32)ControlId.KNOB]).Value.ToString();
                                        break;
                                    case (Int32)ControlId.HORIZONTAL_SLIDER:
                                        x = ((HorizontalSlider)Controls.ControlsList[(Int32)ControlId.HORIZONTAL_SLIDER]).Value;
                                        ((Joystick)Controls.ControlsList[(Int32)ControlId.JOYSTICK]).ValueX = (int)x - 64;
                                        ((Label)Controls.ControlsList[(Int32)ControlId.LABEL]).TextBlock.Text = "Value " +
                                        ((HorizontalSlider)Controls.ControlsList[(Int32)ControlId.HORIZONTAL_SLIDER]).Value.ToString();
                                        break;
                                    case (Int32)ControlId.VERTICAL_SLIDER:
                                        y = ((VerticalSlider)Controls.ControlsList[(Int32)ControlId.VERTICAL_SLIDER]).Value;
                                        ((Knob)Controls.ControlsList[(int)ControlId.KNOB]).Value = (int)y;
                                        ((Joystick)Controls.ControlsList[(Int32)ControlId.JOYSTICK]).ValueY = (int)y - 64;
                                        ((Label)Controls.ControlsList[(Int32)ControlId.LABEL]).TextBlock.Text = "Value " +
                                        ((VerticalSlider)Controls.ControlsList[(Int32)ControlId.VERTICAL_SLIDER]).Value.ToString();
                                        break;
                                    case (Int32)ControlId.JOYSTICK:
                                        x = ((Joystick)Controls.ControlsList[(Int32)ControlId.JOYSTICK]).ValueX + 64;
                                        y = ((Joystick)Controls.ControlsList[(Int32)ControlId.JOYSTICK]).ValueY + 64;
                                        ((VerticalSlider)Controls.ControlsList[(Int32)ControlId.VERTICAL_SLIDER]).Value = (int)y;
                                        ((HorizontalSlider)Controls.ControlsList[(Int32)ControlId.HORIZONTAL_SLIDER]).Value = (int)x;
                                        ((Knob)Controls.ControlsList[(int)ControlId.KNOB]).Value = (int)y;
                                        ((Label)Controls.ControlsList[(Int32)ControlId.LABEL]).TextBlock.Text = "X: " +
                                        ((Joystick)Controls.ControlsList[(Int32)ControlId.JOYSTICK]).ValueX.ToString() + ", Y: " +
                                        ((Joystick)Controls.ControlsList[(Int32)ControlId.JOYSTICK]).ValueY.ToString();
                                        break;
                                }
                            }
                        }
                    }
                }
            }
        }

        private void imgClickArea_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            if (initDone && Controls != null)
            {
                Controls.PointerPressed(sender, e);
            }

            // Turn on indicator 2 to show mouse button is pressed:
            ((Indicator)Controls.ControlsList[(int)ControlId.INDICATOR2]).IsOn =
                ((ImageButton)Controls.ControlsList[(int)ControlId.BUTTON]).IsOn;

            // If this is the popup menu button, toggle it:
            if (Controls.Current == (int)ControlId.POPUP_MENU)
            {
                if (((PopupMenuButton)Controls.ControlsList[(int)ControlId.POPUP_MENU]).IsOn)
                {
                    ((PopupMenuButton)Controls.ControlsList[(int)ControlId.POPUP_MENU]).TextBlock.Text = "MIDI Thru";
                }
                else
                {
                    ((PopupMenuButton)Controls.ControlsList[(int)ControlId.POPUP_MENU]).TextBlock.Text = "MIDI";
                }
            }
        }

        private void imgClickArea_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            if (initDone && Controls != null)
            {
                Controls.PointerReleased(sender, e);
            }
            GetMousePosition(e);
            GetMouseButtonPressed(e);

            if (Controls != null)
            {
                Controls.PointerReleased(sender, e);
            }

            foreach (object control in Controls.ControlsList)
            {
                if (((ControlBase)control).IsSelected)
                {
                    ((ControlBase)control).IsSelected = true;
                    Controls.PointerReleased(sender, e);
                }
                else
                {
                    ((ControlBase)control).IsSelected = false;
                }
            }
        }

        private void imgClickArea_PointerWheelChanged(object sender, PointerRoutedEventArgs e)
        {
            if (initDone && Controls != null)
            {
                Controls.PointerWheelChanged(sender, e);
            }
        }

        // Tapped event, handlers for controls that are supposed to be tapped:
        private void imgClickArea_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (initDone)
            {
                if (Controls != null)
                {
                    //Controls.Tapped(sender, e);
                    foreach (object control in Controls.ControlsList)
                    {
                        if (((ControlBase)control).IsSelected)
                        {
                            switch (((ControlBase)control).Id)
                            {
                                //case (Int32)ControlId.MOMENTARYBUTTON:
                                //    ((Label)Controls.ControlsList[(Int32)ControlId.LABEL]).TextBlock.Text = "Button tapped";
                                //    break;
                                //case (Int32)ControlId.MOMENTARYBUTTON2:
                                //    ((Label)Controls.ControlsList[(Int32)ControlId.LABEL]).TextBlock.Text = "Button 2 tapped";
                                //    break;
                                //case (Int32)ControlId.SELECTOR:
                                //    ((Label)Controls.ControlsList[(Int32)ControlId.LABEL]).TextBlock.Text = "Selected " +
                                //    ((UwpControlsLibrary.Rotator)Controls.ControlsList[(Int32)ControlId.SELECTOR]).Selection.ToString();
                                //    break;
                            }
                        }
                    }
                }
            }
        }

        private void MidiInPort_MessageReceived(MidiInPort sender, MidiMessageReceivedEventArgs args)
        {
            MidiInBuffer = args.Message.RawData.ToArray();

            // See https://www.midi.org/specifications-old/item/table-1-summary-of-midi-message for a complete summary of messages.
            if (args.Message.Type == MidiMessageType.SystemExclusive)
            {
            }
            else if (args.Message.Type == MidiMessageType.ActiveSensing)
            {
            }
            else if (args.Message.Type == MidiMessageType.ChannelPressure)
            {
            }
            else if (args.Message.Type == MidiMessageType.Continue)
            {
            }
            else if (args.Message.Type == MidiMessageType.ControlChange)
            {
            }
            else if (args.Message.Type == MidiMessageType.EndSystemExclusive)
            {
            }
            else if (args.Message.Type == MidiMessageType.MidiTimeCode)
            {
            }
            else if (args.Message.Type == MidiMessageType.NoteOff)
            {
                if (((PopupMenuButton)Controls.ControlsList[(int)ControlId.POPUP_MENU]).IsOn)
                {
                    int portIndex = ((PopupMenuButton)Controls.ControlsList[(int)ControlId.POPUP_MENU]).PopupMenus[0][0].SelectedIndex;
                    int channel = ((PopupMenuButton)Controls.ControlsList[(int)ControlId.POPUP_MENU]).PopupMenus[0][1].SelectedIndex;
                    if (portIndex > -1)
                    {
                        byte[] rawData = args.Message.RawData.ToArray();
                        rawData[0] = (byte)(rawData[0] & 0xf0 | (byte)channel);
                        IBuffer buffer = CryptographicBuffer.CreateFromByteArray(rawData);
                        midi.MidiOutPorts[portIndex].SendBuffer(buffer);
                    }
                }
            }
            else if (args.Message.Type == MidiMessageType.NoteOn)
            {
                if (((PopupMenuButton)Controls.ControlsList[(int)ControlId.POPUP_MENU]).IsOn)
                {
                    int portIndex = ((PopupMenuButton)Controls.ControlsList[(int)ControlId.POPUP_MENU]).PopupMenus[0][0].SelectedIndex;
                    int channel = ((PopupMenuButton)Controls.ControlsList[(int)ControlId.POPUP_MENU]).PopupMenus[0][1].SelectedIndex;
                    if (portIndex > -1 && channel > -1)
                    {
                        byte[] rawData = args.Message.RawData.ToArray();
                        rawData[0] = (byte)(rawData[0] & 0xf0 | (byte)channel);
                        IBuffer buffer = CryptographicBuffer.CreateFromByteArray(rawData);
                        midi.MidiOutPorts[portIndex].SendBuffer(buffer);
                    }
                }
            }
            else if (args.Message.Type == MidiMessageType.PitchBendChange)
            {
            }
            else if (args.Message.Type == MidiMessageType.PolyphonicKeyPressure)
            {
            }
            else if (args.Message.Type == MidiMessageType.ProgramChange)
            {
            }
            else if (args.Message.Type == MidiMessageType.SongPositionPointer)
            {
            }
            else if (args.Message.Type == MidiMessageType.SongSelect)
            {
            }
            else if (args.Message.Type == MidiMessageType.Start)
            {
            }
            else if (args.Message.Type == MidiMessageType.Stop)
            {
            }
            else if (args.Message.Type == MidiMessageType.SystemExclusive)
            {
            }
            else if (args.Message.Type == MidiMessageType.SystemReset)
            {
            }
            else if (args.Message.Type == MidiMessageType.TimingClock)
            {
            }
            else if (args.Message.Type == MidiMessageType.TuneRequest)
            {
            }
            else if (args.Message.Type == MidiMessageType.ProgramChange)
            {
            }
            else if (args.Message.Type == MidiMessageType.ControlChange)
            {
                // See https://www.midi.org/specifications-old/item/table-3-control-change-messages-data-bytes-2 for a complete summary of control change messages.
            }
            else if (args.Message.Type == MidiMessageType.PitchBendChange)
            {
            }
        }

        private void GetMousePosition(PointerRoutedEventArgs e)
        {
            PointerPoint MousePositionInWindow = e.GetCurrentPoint(this);
            PointerPosition = new Point(MousePositionInWindow.Position.X,
                MousePositionInWindow.Position.Y);
        }

        public Point CopyPoint(Point f)
        {
            return new Point(f.X, f.Y);
        }

        // When app size is changed, all controls must also be resized,
        // ask the Controls object to do it:
        private void gridControls_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (Controls != null)
            {
                Controls.ResizeControls(gridMain, Window.Current.Bounds);
            }
        }

        private void GetMouseButtonPressed(PointerRoutedEventArgs e)
        {
            PointerPointProperties pointerPointProperties = e.GetCurrentPoint(this).Properties;
            leftButtonIsPressed = pointerPointProperties.IsLeftButtonPressed;
            rightButtonIsPressed = pointerPointProperties.IsRightButtonPressed;
        }

        private void Keyboard_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            PointerPoint pp = e.GetCurrentPoint((Image)sender);
            Controls.PointerPressed(sender, e);

            int key = ((Keyboard)Controls.ControlsList[(int)ControlId.KEYBOARD]).Key.KeyNumber;
            string keyName = ((Keyboard)Controls.ControlsList[(int)ControlId.KEYBOARD]).Key.KeyName;
            int velocity = ((Keyboard)Controls.ControlsList[(int)ControlId.KEYBOARD]).Key.Velocity;

            ((Label)Controls.ControlsList[(Int32)ControlId.LABEL]).TextBlock.Text =
                keyName + " on, velocity: " + velocity.ToString();
            ((Indicator)Controls.ControlsList[(int)ControlId.INDICATOR1]).IsOn = true;

            if (((PopupMenuButton)Controls.ControlsList[(int)ControlId.POPUP_MENU]).IsOn)
            {
                int portIndex = ((PopupMenuButton)Controls.ControlsList[(int)ControlId.POPUP_MENU]).PopupMenus[0][1].SelectedIndex;
                if (portIndex > -1)
                {
                    midi.SendNoteOn(midi.MidiOutPorts[portIndex], key, 0, velocity);
                }
            }
        }

        private void Keyboard_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            PointerPoint pp = e.GetCurrentPoint((Image)sender);
            Controls.PointerReleased(sender, e);

            int key = ((Keyboard)Controls.ControlsList[(int)ControlId.KEYBOARD]).Key.KeyNumber;
            string keyName = ((Keyboard)Controls.ControlsList[(int)ControlId.KEYBOARD]).Key.KeyName;
            int velocity = ((Keyboard)Controls.ControlsList[(int)ControlId.KEYBOARD]).Key.Velocity;

            ((Label)Controls.ControlsList[(Int32)ControlId.LABEL]).TextBlock.Text =
                keyName + " off, velocity: " + velocity.ToString();
            ((Indicator)Controls.ControlsList[(int)ControlId.INDICATOR1]).IsOn = false;

            if (((PopupMenuButton)Controls.ControlsList[(int)ControlId.POPUP_MENU]).IsOn)
            {
                int portIndex = ((PopupMenuButton)Controls.ControlsList[(int)ControlId.POPUP_MENU]).PopupMenus[0][1].SelectedIndex;
                if (portIndex > -1)
                {
                    midi.SendNoteOff(midi.MidiOutPorts[portIndex], key, 0, velocity);
                }
            }
        }

        private void Keyboard_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            PointerPoint pp = e.GetCurrentPoint((Image)sender);
            ((Key)((Image)sender).Tag).Velocity = (Int32)(pp.Position.Y / ((Key)((Image)sender).Tag)
                .Images[((Key)((Image)sender).Tag).Images.Length - 1].ActualHeight);
            Int32 velocity = (Int32)(127 * (pp.Position.Y / ((Key)((Image)sender).Tag)
                .Images[((Key)((Image)sender).Tag).Images.Length - 1].ActualHeight / 0.8));
            velocity = velocity > 127 ? 127 : velocity;
            ((Label)Controls.ControlsList[(Int32)ControlId.LABEL]).TextBlock.Text =
                ((Key)((Image)sender).Tag).KeyName + " velocity: " + velocity.ToString();
        }
    }
}
