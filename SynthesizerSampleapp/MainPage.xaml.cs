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
using System.Threading.Tasks;
using Windows.Devices.Midi;
using SynthesizerSampleapp;
using Windows.ApplicationModel.Core;
using Windows.UI.ViewManagement;
using Windows.UI;
using Windows.Storage.Streams;
using System.Diagnostics;
using System.ServiceModel.Channels;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace SynthesizerSampleapp
{
    public sealed partial class MainPage : Page
    {
        // The Controls object:
        Controls Controls;

        // This enum must contain one entry for each control!
        // You can use it in handlers to identify what action to take when control is used.
        public enum ControlId
        {
            KEYBOARD,
            MIDI_IN_SELECTOR,
            WAVEFORM,
            ADSR_PULSE,
            CHORUS,
            SYNTHESIS,
            VOLUME,
            VIBRATO,
            TREMOLO,
            LFOFREQ,
            PHASE,
            LFOPHASE,
            SUB,
            FILTER,
            FILTER_Q,
            FILTER_FREQ,
            FILTER_KEYFOLLOW,
            FILTER_GAIN,
            ADSR_A,
            ADSR_D,
            ADSR_S,
            ADSR_R,
            REVERB,
            ADSR_GRAPH,
            MENU,
        }

        public Double WidthSizeRatio = 1;
        public Double HeightSizeRatio = 1;
        private bool initDone = false;

        // Control objects:
        public Keyboard keyboard;
        public Rotator midiInSelector;
        public Rotator waveform;
        public ImageButton ibChorus;
        public Rotator filter;
        public Rotator useAdsr;
        public PopupMenuButton menu;
        public Graph graphADSR;

        // Synthesizer objects:
        public Synthesis synthesis;
        public VerticalSlider slVolume;
        public VerticalSlider slVibrato;
        public VerticalSlider slTremolo;
        public VerticalSlider slLFOFrequency;
        public VerticalSlider slPhase;
        public VerticalSlider slLfoPhase;
        public VerticalSlider slSub;
        public VerticalSlider slFilterQ;
        public VerticalSlider slFilterFreq;
        public VerticalSlider slFilterKeyFollow;
        public VerticalSlider slFilterGain;
        public VerticalSlider slADSR_A;
        public VerticalSlider slADSR_D;
        public VerticalSlider slADSR_S;
        public VerticalSlider slADSR_R;
        public VerticalSlider slReverb;
        MIDI midi;
        private bool shift = false;
        private bool ctrl = false;

        private byte[] MidiInBuffer;
        public MainPage()
        {
            this.InitializeComponent();
            Window.Current.CoreWindow.KeyDown += CoreWindow_KeyDown;
            Window.Current.CoreWindow.KeyUp += CoreWindow_KeyUp;
        }

        private void CoreWindow_KeyDown(Windows.UI.Core.CoreWindow sender, Windows.UI.Core.KeyEventArgs args)
        {
            switch (args.VirtualKey)
            {
                case Windows.System.VirtualKey.Shift: shift = true; break;
                case Windows.System.VirtualKey.Control: ctrl = true; break;
            }
        }

        private void CoreWindow_KeyUp(Windows.UI.Core.CoreWindow sender, Windows.UI.Core.KeyEventArgs args)
        {
            switch (args.VirtualKey)
            {
                case Windows.System.VirtualKey.Shift: shift = false; break;
                case Windows.System.VirtualKey.Control: ctrl = false; break;
            }
        }

        // When imgClickArea is opened it has also got its size, so now
        // we can create the controls object:
        private async void imgClickArea_ImageOpened(object sender, RoutedEventArgs e)
        {
            // Fix the title bar:
            CoreApplicationViewTitleBar coreTitleBar = CoreApplication.GetCurrentView().TitleBar;
            coreTitleBar.ExtendViewIntoTitleBar = true;
            ApplicationViewTitleBar titleBar = ApplicationView.GetForCurrentView().TitleBar;
            titleBar.ButtonBackgroundColor = Color.FromArgb(0, 128, 128, 128);
            titleBar.ButtonForegroundColor = Color.FromArgb(0, 128, 128, 128);

            // Create the LFO:


            // Create and initiate the controls object:
            Controls = new Controls(Window.Current.Bounds, imgClickArea);
            Controls.Init(gridControls);

            // Create all controls:
            keyboard = Controls.AddKeyBoard((int)ControlId.KEYBOARD, gridKeyboard,
                new Image[] { imgWhiteKey, imgBlackKey, imgWhiteKeyDown, imgBlackKeyDown},
                new Point (5, 260), 36, 96);
            foreach (Octave octave in ((Keyboard)Controls.GetControl((Int32)ControlId.KEYBOARD)).Octaves)
            {
                foreach (Key key in octave.Keys)
                {
                    key.Images[key.Images.Length - 1].PointerMoved += Keyboard_PointerMoved;
                    key.Images[key.Images.Length - 1].PointerPressed += Keyboard_PointerPressed;
                    key.Images[key.Images.Length - 1].PointerReleased += Keyboard_PointerReleased;
                }
            }

            int buttonsPos = 0;
            int buttonStart = 8;
            int buttonsSpacing = 254;
            midiInSelector = Controls.AddRotator((int)ControlId.MIDI_IN_SELECTOR, gridControls,
                new Image[] { imgButtonUp }, new Point(buttonStart + buttonsSpacing * buttonsPos++, 35), "", 14,
                TextAlignment.Center, ControlBase.ControlTextWeight.BOLD);

            waveform = Controls.AddRotator((int)ControlId.WAVEFORM, gridControls,
                new Image[] { imgButtonUp }, new Point(buttonStart + buttonsSpacing * buttonsPos++, 35), "SQUARE", 14,
                TextAlignment.Center, ControlBase.ControlTextWeight.BOLD);
            waveform.Texts.Add("SAWTOOTH");
            waveform.Texts.Add("TRIANGLE");
            waveform.Texts.Add("SINE");
            waveform.Texts.Add("NOISE");

            filter = Controls.AddRotator((int)ControlId.FILTER, gridControls,
                new Image[] { imgButtonUp }, new Point(buttonStart + buttonsSpacing * buttonsPos++, 35), "FILTER OFF", 14,
                TextAlignment.Center, ControlBase.ControlTextWeight.BOLD);
            filter.Texts.Add("FILTER FIXED");
            filter.Texts.Add("FILTER ADSR POS");
            filter.Texts.Add("FILTER ADSR NEG");
            filter.Texts.Add("FILTER LFO");

            useAdsr = Controls.AddRotator((int)ControlId.ADSR_PULSE, gridControls,
                new Image[] { imgButtonUp }, new Point(buttonStart + buttonsSpacing * buttonsPos++, 35), "ADSR", 14,
                TextAlignment.Center, ControlBase.ControlTextWeight.BOLD);
            useAdsr.Texts.Add("PULSE");

            ibChorus = Controls.AddImageButton((int)ControlId.CHORUS, gridControls, new Image[] { imgButtonUp, imgButtonDown },
                new Point(buttonStart + buttonsSpacing * buttonsPos++, 35), ControlBase.ImageButtonFunction.TOGGLE, false, "CHORUS", 14,
                TextAlignment.Center, ControlBase.ControlTextWeight.BOLD);

            int spacing = 80;
            int left = 10;
            int pos = 0; // For horizontal position of labels and sliders.
            Controls.AddLabel(-1, gridControls, new Rect(left + spacing * pos, 68, 70, 20), "VOLUME", 14, TextAlignment.Center, ControlBase.ControlTextWeight.BOLD);
            slVolume = Controls.AddVerticalSlider((int)ControlId.VOLUME, gridControls,
                new Image[] { imgSliderBackground, imgSliderHandle }, new Rect(left + spacing * pos++, 88, 0, 0));

            Controls.AddLabel(-1, gridControls, new Rect(left + spacing * pos, 68, 70, 20), "VIBRATO", 14, TextAlignment.Center, ControlBase.ControlTextWeight.BOLD);
            slVibrato = Controls.AddVerticalSlider((int)ControlId.VIBRATO, gridControls,
                new Image[] { imgSliderBackground, imgSliderHandle }, new Rect(left + spacing * pos++, 88, 0, 0));

            Controls.AddLabel(-1, gridControls, new Rect(left + spacing * pos, 68, 70, 20), "TREMOLO", 14, TextAlignment.Center, ControlBase.ControlTextWeight.BOLD);
            slTremolo = Controls.AddVerticalSlider((int)ControlId.TREMOLO, gridControls,
                new Image[] { imgSliderBackground, imgSliderHandle }, new Rect(left + spacing * pos++, 88, 0, 0));

            Controls.AddLabel(-1, gridControls, new Rect(left + spacing * pos, 68, 70, 20), "SPEED", 14, TextAlignment.Center, ControlBase.ControlTextWeight.BOLD);
            slLFOFrequency = Controls.AddVerticalSlider((int)ControlId.LFOFREQ, gridControls,
                new Image[] { imgSliderBackground, imgSliderHandle }, new Rect(left + spacing * pos++, 88, 0, 0), 1, 300);

            Controls.AddLabel(-1, gridControls, new Rect(left + spacing * pos, 68, 70, 20), "PHASE", 14, TextAlignment.Center, ControlBase.ControlTextWeight.BOLD);
            slPhase = Controls.AddVerticalSlider((int)ControlId.PHASE, gridControls,
                new Image[] { imgSliderBackground, imgSliderHandle }, new Rect(left + spacing * pos++, 88, 0, 0));

            Controls.AddLabel(-1, gridControls, new Rect(left + spacing * pos, 68, 70, 20), "LFO PH", 14, TextAlignment.Center, ControlBase.ControlTextWeight.BOLD);
            slLfoPhase = Controls.AddVerticalSlider((int)ControlId.LFOPHASE, gridControls,
                new Image[] { imgSliderBackground, imgSliderHandle }, new Rect(left + spacing * pos++, 88, 0, 0));

            Controls.AddLabel(-1, gridControls, new Rect(left + spacing * pos, 68, 70, 20), "SUB", 14, TextAlignment.Center, ControlBase.ControlTextWeight.BOLD);
            slSub = Controls.AddVerticalSlider((int)ControlId.SUB, gridControls,
                new Image[] { imgSliderBackground, imgSliderHandle }, new Rect(left + spacing * pos++, 88, 0, 0));

            Controls.AddLabel(-1, gridControls, new Rect(left + spacing * pos, 68, 70, 20), "Q", 14, TextAlignment.Center, ControlBase.ControlTextWeight.BOLD);
            slFilterQ = Controls.AddVerticalSlider((int)ControlId.FILTER_Q, gridControls,
                new Image[] { imgSliderBackground, imgSliderHandle }, new Rect(left + spacing * pos++, 88, 0, 0));

            Controls.AddLabel(-1, gridControls, new Rect(left + spacing * pos, 68, 70, 20), "FREQ.", 14, TextAlignment.Center, ControlBase.ControlTextWeight.BOLD);
            slFilterFreq = Controls.AddVerticalSlider((int)ControlId.FILTER_FREQ, gridControls,
                new Image[] { imgSliderBackground, imgSliderHandle }, new Rect(left + spacing * pos++, 88, 0, 0));

            Controls.AddLabel(-1, gridControls, new Rect(left + spacing * pos, 68, 70, 20), "KEY FOL.", 14, TextAlignment.Center, ControlBase.ControlTextWeight.BOLD);
            slFilterKeyFollow = Controls.AddVerticalSlider((int)ControlId.FILTER_KEYFOLLOW, gridControls,
                new Image[] { imgSliderBackground, imgSliderHandle }, new Rect(left + spacing * pos++, 88, 0, 0));
            slFilterKeyFollow.Value = 127;

            Controls.AddLabel(-1, gridControls, new Rect(left + spacing * pos, 68, 70, 20), "GAIN", 14, TextAlignment.Center, ControlBase.ControlTextWeight.BOLD);
            slFilterGain = Controls.AddVerticalSlider((int)ControlId.FILTER_GAIN, gridControls,
                new Image[] { imgSliderBackground, imgSliderHandle }, new Rect(left + spacing * pos++, 88, 0, 0));
            slFilterGain.Value = 63;

            Controls.AddLabel(-1, gridControls, new Rect(left + spacing * pos, 68, 70, 20), "ATTACK", 14, TextAlignment.Center, ControlBase.ControlTextWeight.BOLD);
            slADSR_A = Controls.AddVerticalSlider((int)ControlId.ADSR_A, gridControls,
                new Image[] { imgSliderBackground, imgSliderHandle }, new Rect(left + spacing * pos++, 88, 0, 0));

            Controls.AddLabel(-1, gridControls, new Rect(left + spacing * pos, 68, 70, 20), "DECAY", 14, TextAlignment.Center, ControlBase.ControlTextWeight.BOLD);
            slADSR_D = Controls.AddVerticalSlider((int)ControlId.ADSR_D, gridControls,
                new Image[] { imgSliderBackground, imgSliderHandle }, new Rect(left + spacing * pos++, 88, 0, 0));

            Controls.AddLabel(-1, gridControls, new Rect(left + spacing * pos, 68, 70, 20), "SUSTAIN", 14, TextAlignment.Center, ControlBase.ControlTextWeight.BOLD);
            slADSR_S = Controls.AddVerticalSlider((int)ControlId.ADSR_S, gridControls,
                new Image[] { imgSliderBackground, imgSliderHandle }, new Rect(left + spacing * pos++, 88, 0, 0));
            slADSR_S.Value = 127;

            Controls.AddLabel(-1, gridControls, new Rect(left + spacing * pos, 68, 70, 20), "RELEASE", 14, TextAlignment.Center, ControlBase.ControlTextWeight.BOLD);
            slADSR_R = Controls.AddVerticalSlider((int)ControlId.ADSR_R, gridControls,
                new Image[] { imgSliderBackground, imgSliderHandle }, new Rect(left + spacing * pos++, 88, 0, 0));

            Controls.AddLabel(-1, gridControls, new Rect(left + spacing * pos, 68, 70, 20), "REVERB", 14, TextAlignment.Center, ControlBase.ControlTextWeight.BOLD);
            slReverb = Controls.AddVerticalSlider((int)ControlId.REVERB, gridControls,
                new Image[] { imgSliderBackground, imgSliderHandle }, new Rect(left + spacing * pos++, 88, 0, 0));

            graphADSR = Controls.AddGraph((int)ControlId.ADSR_GRAPH, gridControls, new Image[] { imgGraph },
                new Point(5 + spacing * pos, 72), new SolidColorBrush(Color.FromArgb(255, 162, 197, 249)), 2);

            menu = Controls.AddPopupMenuButton((int)ControlId.MENU, gridControls, new Image[] { imgMenuButton },
                new Point(5 + spacing * pos, 35), ControlBase.PopupMenuButtonStyle.MENU,
                new ControlBase.PointerButton[] { ControlBase.PointerButton.LEFT, ControlBase.PointerButton.RIGHT, ControlBase.PointerButton.OTHER },
                "PATCHES", 14, false, ControlBase.ControlTextWeight.BOLD, ControlBase.ControlTextAlignment.CENTER);
            menu.TextBlock.Foreground = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255));

            Controls.AddLabel(-1, gridControls, new Rect(left, 5, 700, 30), 
                "This is a sample application demonstrating sound synthesis support in UwpControlsLibrary", 
                14, TextAlignment.Left, ControlBase.ControlTextWeight.NORMAL);

            // Load patches:
            await LoadPatches();

            synthesis = Controls.AddSynthesis((int)ControlId.SYNTHESIS, 6, 2, true);
            SetSubOscillator(1);

            midi = new MIDI(MidiInPort_MessageReceived);

            while (!midi.IsInitiated)
            {
                await Task.Delay(1);
            }

            if (midi.MidiInPortNames.Count > 0)
            {
                foreach (string midiInPortName in midi.MidiInPortNames)
                {
                    // Fill out some way of selecting midi in port or find a specific port:
                    if (midiInPortName.Contains(""))
                    {
                        midiInSelector.Texts.Add(midiInPortName);
                    }
                }
                if (midiInSelector.Texts.Count > 0)
                {
                    midiInSelector.Selection = 0;
                }
            }

            slVolume.Value = 20;
            InitMenuUpdater();
            SetAllValues();

            // Make sure all controls has the correct size and position:
            Controls.ResizeControls(gridControls, Window.Current.Bounds);
            Controls.SetControlsUniform(gridControls);
            UpdateLayout();
            UpdateAdsrGraph();
            imgReadingPatches.Visibility= Visibility.Collapsed;
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
                if (Controls.PointerButtonStates.Count > 0)
                {
                    HandleEvent(Controls.PointerMoved(sender, e));
                }
            }
        }

        private void imgClickArea_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            if (initDone && Controls != null)
            {
                HandleEvent(Controls.PointerPressed(sender, e));
            }
        }

        private void imgClickArea_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            //if (initDone && Controls != null)
            //{
            //    Controls.PointerReleased(sender, e);
            //}
        }

        private void imgClickArea_PointerWheelChanged(object sender, PointerRoutedEventArgs e)
        {
            if (initDone && Controls != null)
            {
                HandleEvent(Controls.PointerWheelChanged(sender, e));
            }
        }

        private async void HandleEvent(object control)
        {
            if (control != null)
            {
                if (control.GetType() != typeof(PopupMenuButton))
                {
                    switch (Controls.Current)
                    {
                        case (int)ControlId.VOLUME:
                            synthesis.SetVolume(slVolume.Value);
                            synthesis.SetSub((int)(slSub.Value * (slVolume.Value / 127.0)));
                            break;
                        case (int)ControlId.VIBRATO:
                            synthesis.SetVibrato(slVibrato.Value);
                            break;
                        case (int)ControlId.TREMOLO:
                            synthesis.SetTremolo(slTremolo.Value);
                            break;
                        case (int)ControlId.LFOFREQ:
                            synthesis.SetLfoFrequency(slLFOFrequency.Value);
                            //synthesis.LFO.StepSize = FrequencyInUse * Math.PI * 2 / synthesis.SampleRate;
                            break;
                        case (int)ControlId.PHASE:
                            synthesis.SetPhase(slPhase.Value, 0);
                            break;
                        case (int)ControlId.LFOPHASE:
                            synthesis.SetLfoPhase(slLfoPhase.Value, 0);
                            break;
                        case (int)ControlId.SUB:
                            synthesis.SetSub((int)(slSub.Value * (slVolume.Value / 127.0)));
                            break;
                        case (int)ControlId.FILTER_Q:
                            synthesis.SetFilterQ(slFilterQ.Value);
                            break;
                        case (int)ControlId.FILTER_FREQ:
                            synthesis.SetFilterFreq(slFilterFreq.Value);
                            break;
                        case (int)ControlId.FILTER_KEYFOLLOW:
                            synthesis.SetFilterKeyFollow(slFilterKeyFollow.Value);
                            break;
                        case (int)ControlId.FILTER_GAIN:
                            synthesis.SetFilterGain(slFilterGain.Value);
                            break;
                        case (int)ControlId.ADSR_A:
                            synthesis.SetAdsr_A(slADSR_A.Value);
                            UpdateAdsrGraph();
                            break;
                        case (int)ControlId.ADSR_D:
                            synthesis.SetAdsr_D(slADSR_D.Value);
                            UpdateAdsrGraph();
                            break;
                        case (int)ControlId.ADSR_S:
                            synthesis.SetAdsr_S(slADSR_S.Value);
                            UpdateAdsrGraph();
                            break;
                        case (int)ControlId.ADSR_R:
                            synthesis.SetAdsr_R(slADSR_R.Value);
                            UpdateAdsrGraph();
                            break;
                        case (int)ControlId.REVERB:
                            synthesis.SetReverb(slReverb.Value);
                            UpdateAdsrGraph();
                            break;

                        case (int)ControlId.MIDI_IN_SELECTOR:
                            //SetMidiIn(midiInSelector.Selection);
                            break;
                        case (int)ControlId.WAVEFORM:
                            SetWaveform(waveform.Selection);
                            break;
                        case (int)ControlId.CHORUS:
                            synthesis.SetChorus(ibChorus.IsOn);
                            break;
                        case (int)ControlId.FILTER:
                            SetFilter(filter.Selection);
                            break;
                        case (int)ControlId.ADSR_PULSE:
                            Debug.WriteLine("case (int)ControlId.ADSR_PULSE:");
                            bool value = useAdsr.Selection == 0;
                            synthesis.SetAdsrPulse(value);
                            break;
                    }
                }
                else
                {
                    // Handles all PopupMenuButton objects.
                    object obj = ((PopupMenuButton)control).Tag;

                    // Does this PopupMenuItem represent a Folder object or a Patch object?
                    if (obj != null && obj.GetType() == typeof(Patch))
                    {
                        // Handle Patch (green) buttons:
                        if (Controls.PointerButtonStates.Contains(ControlBase.PointerButton.LEFT))
                        {
                            // Select this patch:
                            ((Patch)obj).Read(this);
                            menu.HideAllMenus();
                        }
                        else if (Controls.PointerButtonStates.Contains(ControlBase.PointerButton.RIGHT))
                        {
                            // Save current settings to this path:
                            Confirm confirm = new Confirm("Are you sure you want to overwrite this patch?");
                            await confirm.ShowAsync();
                            if (confirm.Ok)
                            {
                                UpdatePatch((Patch)obj);
                                await StorePatches();
                            }
                            //SavePatch((Patch)obj);
                            //menu.HideAllMenus();
                        }
                        else if (Controls.PointerButtonStates.Contains(ControlBase.PointerButton.OTHER))
                        {
                            if (shift)
                            {

                            }
                            else
                            {
                                while (((PopupMenuButton)control).IsEditing)
                                {
                                    await Task.Delay(100);
                                }
                                ((Patch)((PopupMenuButton)control).Tag).Name = ((PopupMenuButton)control).TextBlock.Text;
                                await StorePatches();
                            }
                        }
                        else if (Controls.PointerButtonStates.Contains(ControlBase.PointerButton.RIGHT))
                        {
                            if (shift)
                            {
                                // Delete this patch:
                                //await DeletePatch((Patch)((PopupMenuButton)control).Tag);
                            }
                            else
                            {
                                AddPatch((PopupMenuButton)control);
                            }
                            //await CoreApplication.RequestRestartAsync("Application Restart Programmatically ");
                        }
                    }
                    else if (obj != null && obj.GetType() == typeof(Folder))
                    {
                        // Handle Folder (violet) buttons:
                        if (Controls.PointerButtonStates.Contains(ControlBase.PointerButton.LEFT))
                        {
                            //Left is already handled by the menu system!
                        }
                        else if (Controls.PointerButtonStates.Contains(ControlBase.PointerButton.RIGHT))
                        {
                            AddPatch((PopupMenuButton)control);
                            //await CoreApplication.RequestRestartAsync("Application Restart Programmatically ");
                        }
                        else if (Controls.PointerButtonStates.Contains(ControlBase.PointerButton.OTHER))
                        {
                            if (shift)
                            {

                            }
                            else
                            {
                                while (((PopupMenuButton)control).IsEditing)
                                {
                                    await Task.Delay(100);
                                }
                                ((Folder)((PopupMenuButton)control).Tag).Name = ((PopupMenuButton)control).TextBlock.Text;
                                await StorePatches();
                            }
                        }
                        //else if (Controls.PointerButtonStates.Contains(ControlBase.PointerButton.OTHER))
                        //{
                        //    if (shift)
                        //    {
                        //        DeleteFolder((Folder)obj);
                        //        await CoreApplication.RequestRestartAsync("Application Restart Programmatically ");
                        //    }
                        //    else
                        //    {
                        //        await AddFolder((PopupMenuButton)control);
                        //        await CoreApplication.RequestRestartAsync("Application Restart Programmatically ");
                        //    }
                        //}
                    }
                }
            }
        }

        private void SetAllValues()
        {
            synthesis.SetVolume(slVolume.Value);
            synthesis.SetVibrato(slVibrato.Value);
            synthesis.SetTremolo(slTremolo.Value);
            synthesis.SetLfoFrequency(slLFOFrequency.Value);
            synthesis.SetPhase(slPhase.Value, 0);
            synthesis.SetLfoPhase(slLfoPhase.Value, 0);
            synthesis.SetSub(slSub.Value);
            synthesis.SetFilterQ(slFilterQ.Value);
            synthesis.SetFilterFreq(slFilterFreq.Value);
            synthesis.SetFilterKeyFollow(slFilterKeyFollow.Value);
            synthesis.SetFilterGain(slFilterGain.Value);
            synthesis.SetAdsr_A(slADSR_A.Value);
            synthesis.SetAdsr_D(slADSR_D.Value);
            synthesis.SetAdsr_S(slADSR_S.Value);
            synthesis.SetAdsr_R(slADSR_R.Value);
            synthesis.SetReverb(slReverb.Value);
            UpdateAdsrGraph();
            SetWaveform(waveform.Selection);
            synthesis.SetChorus(ibChorus.IsOn);
            SetFilter(filter.Selection);
            bool value = useAdsr.Selection == 0;
            synthesis.SetAdsrPulse(value);
        }

        private void SetMidiIn(MidiInPort midiInPort)
        {
            
        }

        private void SetWaveform(int waveform)
        {
            // There are sawtooth down and sawtooth up, skip the latter.
            if (waveform > 1)
            {
                waveform++;
            }

            // There is also a waveform called 'Random' we skip it here.
            if (waveform > 4)
            {
                waveform++;
            }

            synthesis.SetWaveform(waveform, 0);
        }

        public void SetFilter(int value)
        {
            if (value > 3)
            {
                // Skipping pitch envelope and XM modulations:
                value += 2;
            }
            foreach (List<Oscillator> oscillators in synthesis.Oscillators)
            {
                foreach (Oscillator oscillator in oscillators)
                {
                    oscillator.Filter.FilterFunction = value;
                }
            }
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

        public void UpdateAdsrGraph()
        {
            graphADSR.Points.Clear();
            graphADSR.AddPoint(new Point(10, 152));
            graphADSR.AddPoint(new Point(10 + slADSR_A.Value, 10));
            graphADSR.AddPoint(new Point(10 + slADSR_A.Value + slADSR_D.Value, 152 - slADSR_S.Value * 1.118));
            graphADSR.AddPoint(new Point(300 - slADSR_R.Value - 15, 152 - slADSR_S.Value * 1.118));
            graphADSR.AddPoint(new Point(300 - 15, 152));
            graphADSR.Draw();
        }

        private async void SetSubOscillator(int osc)
        {
            while (synthesis == null || !synthesis.FrameServer.IsInitiated || synthesis.Oscillators == null)
            {
                await Task.Delay(1);
            }
            foreach(List<Oscillator> oscillators in synthesis.Oscillators)
            {
                oscillators[osc].IsSub = true;
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
            int portIndex = ((Rotator)Controls.ControlsList[(int)ControlId.MIDI_IN_SELECTOR]).Selection;
            if (portIndex > -1)
            {
                // For some strange reason setting UseAdsr from Patch.Read upsets the AudioGraph, probably
                // due to some wrong thread issue or something. This is a problem only when using Microsoft
                // Reverb and/or Equalizer. I have not been able to find the reason, but changing it here,
                // from the same thread that works fine when using the ADSR button, seems to be a reasonable
                // work-around. Therefore I always set/reset it here, disregarding its current state:
                synthesis.SetAdsrPulse(useAdsr.Selection > 0);
                synthesis.NoteKeys.KeyOn((byte)key, 0, (byte)velocity);
            }
        }

        private void Keyboard_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            Controls.PointerReleased(sender, e);

            int key = ((Keyboard)Controls.ControlsList[(int)ControlId.KEYBOARD]).Key.KeyNumber;
            int velocity = ((Keyboard)Controls.ControlsList[(int)ControlId.KEYBOARD]).Key.Velocity;

            int portIndex = ((Rotator)Controls.ControlsList[(int)ControlId.MIDI_IN_SELECTOR]).Selection;
            if (portIndex > -1)
            {
                synthesis.NoteKeys.KeyOff((byte)key, 0);
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
                byte[] rawdata = args.Message.RawData.ToArray();
                synthesis.NoteKeys.KeyOff(rawdata[1], 0);
            }
            else if (args.Message.Type == MidiMessageType.NoteOn)
            {
                byte[] rawdata = args.Message.RawData.ToArray();
                synthesis.NoteKeys.KeyOn(rawdata[1], 0, rawdata[2]);
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
