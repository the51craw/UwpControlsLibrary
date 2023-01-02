using System;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using UwpControlsLibrary;
using Windows.Devices.Midi;
using Windows.Foundation;
using Windows.Security.Cryptography;
using Windows.Storage.Streams;
using Windows.UI.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace MIDI_SampleApp
{
    public sealed partial class MainPage : Page
    {
        // The Controls object:
        Controls Controls;

        // This enum must contain one entry for each control!
        // You can use it in handlers to identify what action to take when control is used.
        public enum ControlId
        {
            SELECTOR_MIDI_IN,   // 0
            SELECTOR_MIDI_OUT,  // 1
            KEYBOARD,           // 2
        }

        public Double WidthSizeRatio = 1;
        public Double HeightSizeRatio = 1;
        private bool initDone = false;

        MIDI midi;
        private byte[] MidiInBuffer;

        public MainPage()
        {
            this.InitializeComponent();
            midi = new MIDI(MidiInPort_MessageReceived);
        }

        //////////////////////////////////////////////////////////////////////////////////////////////////////
        // When imgClickArea is opened it has also got its size, so now
        // we can create the controls object:
        //////////////////////////////////////////////////////////////////////////////////////////////////////
        private async void imgClickArea_ImageOpened(object sender, RoutedEventArgs e)
        {
            // Create and initiate the controls object:
            Controls = new Controls(Window.Current.Bounds, imgClickArea);
            Controls.Init(gridControls);

            // Create all controls:
            Controls.AddRotator((int)ControlId.SELECTOR_MIDI_IN, gridControls, new Image[] { imgButtonBackground }, new Point(5, 5),
                "", 24, TextAlignment.Center, ControlBase.ControlTextWeight.BOLD);

            Controls.AddRotator((int)ControlId.SELECTOR_MIDI_OUT, gridControls, new Image[] { imgButtonBackground }, new Point(342, 5),
                "", 24, TextAlignment.Center, ControlBase.ControlTextWeight.BOLD);

            Controls.AddKeyBoard((int)ControlId.KEYBOARD, gridKeyboard, new Image[] { imgWhiteKey, imgBlackKey, imgWhiteKeyDown, imgBlackKeyDown },
                new Point(4, 40), 48, 72);
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

            if (midi.MidiInPortNames.Count > 0)
            {
                foreach (string midiInPortName in midi.MidiInPortNames)
                {
                    ((Rotator)Controls.ControlsList[(int)ControlId.SELECTOR_MIDI_IN]).Texts.Add(midiInPortName);
                }
                ((Rotator)Controls.ControlsList[(int)ControlId.SELECTOR_MIDI_IN]).Selection = 0;
                ((Rotator)Controls.ControlsList[(int)ControlId.SELECTOR_MIDI_IN]).ShowSelection();
            }

            if (midi.MidiOutPortNames.Count > 0)
            {
                foreach (string midiOutPortName in midi.MidiOutPortNames)
                {
                    ((Rotator)Controls.ControlsList[(int)ControlId.SELECTOR_MIDI_OUT]).Texts.Add(midiOutPortName);
                }
                ((Rotator)Controls.ControlsList[(int)ControlId.SELECTOR_MIDI_OUT]).Selection = 0;
                ((Rotator)Controls.ControlsList[(int)ControlId.SELECTOR_MIDI_OUT]).ShowSelection();
            }

            // Make sure all controls has the correct size and position:
            Controls.ResizeControls(gridControls, Window.Current.Bounds);
            Controls.SetControlsUniform(gridControls);
            UpdateLayout();
            initDone = true;
        }

        //////////////////////////////////////////////////////////////////////////////////////////////////////
        // Events from imgClickArea
        //////////////////////////////////////////////////////////////////////////////////////////////////////
        
        // When the pointer is moved over the click-area, ask the Controls
        // object if, and if so which control the pointer is over:
        private void imgClickArea_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            if (initDone && Controls != null)
            {
                Controls.PointerMoved(sender, e);
            }
        }

        private void imgClickArea_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            if (initDone && Controls != null)
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
        }

        // Tapped event, handlers for controls that are supposed to be tapped:
        private void imgClickArea_Tapped(object sender, TappedRoutedEventArgs e)
        {
        }

        // Right tapped event, handlers for controls that are supposed to be tapped:
        private void imgClickArea_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
        }

        // When app size is changed, all controls must also be resized,
        // ask the Controls object to do it:
        private void gridControls_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (initDone && Controls != null)
            {
                Controls.ResizeControls(gridMain, Window.Current.Bounds);
            }
        }

        //////////////////////////////////////////////////////////////////////////////////////////////////////
        // Keyboard events
        //////////////////////////////////////////////////////////////////////////////////////////////////////
        private void Keyboard_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            Controls.PointerMoved(sender, e);
        }

        private void Keyboard_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            Controls.PointerPressed(sender, e);

            int key = ((Keyboard)Controls.ControlsList[(int)ControlId.KEYBOARD]).Key.KeyNumber;
            int velocity = ((Keyboard)Controls.ControlsList[(int)ControlId.KEYBOARD]).Key.Velocity;

            int portIndex = ((Rotator)Controls.ControlsList[(int)ControlId.SELECTOR_MIDI_OUT]).Selection;
            if (portIndex > -1)
            {
                midi.SendNoteOn(midi.MidiOutPorts[portIndex], key, 0, velocity);
            }
        }

        private void Keyboard_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            Controls.PointerReleased(sender, e);

            int key = ((Keyboard)Controls.ControlsList[(int)ControlId.KEYBOARD]).Key.KeyNumber;
            int velocity = ((Keyboard)Controls.ControlsList[(int)ControlId.KEYBOARD]).Key.Velocity;

            int portIndex = ((Rotator)Controls.ControlsList[(int)ControlId.SELECTOR_MIDI_OUT]).Selection;
            if (portIndex > -1)
            {
                midi.SendNoteOff(midi.MidiOutPorts[portIndex], key, 0, velocity);
            }
        }

        //////////////////////////////////////////////////////////////////////////////////////////////////////
        // MIDI in events
        //////////////////////////////////////////////////////////////////////////////////////////////////////
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
                int portIndex = ((Rotator)Controls.ControlsList[(int)ControlId.SELECTOR_MIDI_IN]).Selection;
                if (portIndex > -1)
                {
                    byte[] rawData = args.Message.RawData.ToArray();
                    IBuffer buffer = CryptographicBuffer.CreateFromByteArray(rawData);
                    midi.MidiOutPorts[portIndex].SendBuffer(buffer);
                }
            }
            else if (args.Message.Type == MidiMessageType.NoteOn)
            {
                int portIndex = ((Rotator)Controls.ControlsList[(int)ControlId.SELECTOR_MIDI_OUT]).Selection;
                if (portIndex > -1)
                {
                    byte[] rawData = args.Message.RawData.ToArray();
                    IBuffer buffer = CryptographicBuffer.CreateFromByteArray(rawData);
                    midi.MidiOutPorts[portIndex].SendBuffer(buffer);
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
    }
}
