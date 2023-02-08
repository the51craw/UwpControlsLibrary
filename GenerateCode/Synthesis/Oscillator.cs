using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Numerics;
//using System.Reflection.Metadata;
using UwpControlsLibrary;
using Windows.Services.Maps;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace UwpControlsLibrary
{
    #region mainPage
    public sealed partial class MainPage : Page
    {
        public OscillatorGUI[] oscillatorGUI;
    }

    /// The frame server is the one recieving requests from the AudioGraph system.
    /// The reason for this frame server is that the AudioGraph system calls oscillators
    /// that are supposed to produce output sound at different times. Since Addition
    /// synthesis is supposed to create a mix of sound from multiple oscillators, those
    /// oscillators need to run in sync.
    /// The frame server keeps a list of oscillators that are actually supposed
    /// to create output sound. It is the only object registered as an input node to be
    /// called from the AudioGraph system.
    /// When a request comes in it generates the sound by running the logic of the 
    /// oscillators that are set to create the sound, and multiplies all samples
    /// togehter to create the final sound, which is delivered to the AudioGraph system.
    /// It also has the reverb effects.
    /// Note that playing multiple keys constitutes playing different frequencies,
    /// and the buffer size is different fore each frequency. Ergo, we need one frame
    /// server for each played key. Since the polyphony is 6, we need 6 frame servers.

    /// <summary>
    /// Sound is in stereo in order to allow implementation of chorus.
    /// Samples are created in pairs, first one sample for left channel
    /// then one sample for right channel. Methods that needs to use the
    /// AngleLeft and AngleRight depends on Channel to decide on which to use.
    /// </summary>
    public enum Channel
    {
        LEFT,
        RIGHT,
    }

    /// <summary>
    /// Usage determines starting point and value range for a waveform.
    /// If usage is OUTPUT the wave should start at 0 and have a range of -1 to +1.
    /// If usage is MODULATION the wave should also start at 0 but have a range
    /// of 0 to +1 in order to create proper modulation effects.
    /// </summary>
    public enum OscillatorUsage
    {
        OUTPUT,
        MODULATION,
        FM,
        FM_PLUS_MINUS,
    }
    #endregion mainPage
    #region oscillatorGui
    public class OscillatorGUI
    {
        ///////////////////////////////////////////////////////////////////////////////////////////////////////
        // Control references:
        ///////////////////////////////////////////////////////////////////////////////////////////////////////
        #region controlReferences

        public Knob knobModulation;
        public Knob knobFrequency;
        public Knob knobFinetune;
        public Knob knobVolume;
        public AreaButton btnView;
        public Rotator SelectorWave;
        public Rotator SelectorKeyboard;
        public Rotator SelectorModulation;
        public Rotator SelectorView;
        public Indicator View;
        public Indicator Sounding;

        //public Echo Echo;
        //public Reverb Reverb;
        #endregion controlReferences
    }
    #endregion oscillatorGui

    public partial class Oscillator : ControlBase
    {
        ///////////////////////////////////////////////////////////////////////////////////////////////////////
        // Properties
        ///////////////////////////////////////////////////////////////////////////////////////////////////////
        #region properties

        /// <summary>
        /// One-dimensional oscillator id 0 - 11. Does not account for polyphony,
        /// which is a third dimension after row and column.
        /// </summary>
        public int  Id;

        /// <summary>
        /// The oscillator has its own filter, but it is sometimes used from
        /// the WaveShape object.
        /// </summary>
        public Filter Filter;

        /// <summary>
        /// The oscillator has its own ADSR envelope generator.
        /// </summary>
        public ADSR Adsr;

        public OscillatorUsage Usage; 

        /// <summary>
        /// The oscillator has its own pitch envelope.
        /// </summary>
        public PitchEnvelope PitchEnvelope;

        /// <summary>
        /// The oscillator has a waveshape object that is used to pre-create
        /// the shape of a waveform thet the oscillator uses to crete a wave
            /// form of expected frequency. This saves some time since the wave
        /// shape does not always need to be re-created.
        /// </summary>
        public WaveShape WaveShape;

        /// <summary>
        /// Polyphonic Id, the first index of the oscillator.
        /// </summary>
        //public int PolyId;

        /// <summary>
        /// Oscillators normally react on all MIDI channels, but can also be set
        /// to a specific channel. All = -1, channel = 0 - 15;
        /// </summary>
        public int MidiChannel;

        /// <summary>
        /// Oscillators normally does not have velocity sensitivity, but that can be activated.
        /// </summary>
        public byte Velocity;
        public double ModulationVelocitySensitivity;

        /// <summary>
        /// Velocity is used both for output volume and modulation output:
        /// 0: Velocity is not used.
        /// 1: Output volume is affected by velocity.
        /// 2: Modulation output is affected by velocity.
        /// 3: Output volume and modulation output is affected by velocity.
        /// </summary>
        public byte VelocitySensitive;

        /// <summary>
        /// Frequency and FineTune uses SetFrequency() to combine into FrequenceInUse,
        /// the base frequency used either as a fixed LFO frequency or to be translated
        /// to keyboard frequency. Also sets StepSize.
        /// </summary>
        public double Frequency { get { return frequency; } set { frequency = value; SetFrequency(); } }
        public double FrequencyLfo { get { return frequencyLfo; } set { frequencyLfo = value; SetFrequency(); } }

        /// <summary>
        /// Frequency and FineTune uses SetFrequency() to combine into FrequenceInUse,
        /// the base frequency used either as a fixed LFO frequency or to be translated
        /// to keyboard frequency. Also sets StepSize.
        /// </summary>
        public double FineTune { get { return finetune; } set { finetune = value; SetFrequency(); } }
        public double FineTuneLfo { get { return finetuneLfo; } set { finetuneLfo = value; SetFrequency(); } }

        /// <summary>
        /// The combination of Frequency and FineTune frequencies to use as
        /// keyboard frequency after translation to KeyboardAdjustedFrequency.
        /// </summary>
        public double FinetunedFrequency { get { return finetunedFrequency; } set { finetunedFrequency = value; } }

        /// <summary>
        /// The combination of Frequency and FineTune frequencies to use as a fixed (or FM modulated) LFO frequency.
        /// </summary>
        public double LfoFrequency { get { return lfoFrequency; } set { lfoFrequency = value; SetFrequency(); } }

        /// <summary>
        /// The actual frequency used when a keyboard key is pressed.
        /// </summary>
        public double KeyboardAdjustedFrequency { get { return keyboardAdjustedFrequency; } set { keyboardAdjustedFrequency = value; } }

        /// <summary>
        /// The actual frequency in use, LFO or base frequency for translation when a keyboard key is pressed.
        /// </summary>
        public double FrequencyInUse;

        /// <summary>
        /// The keyboard key pressed. Calls SetKeyboardAdjustedFrequency(key) to translate
        /// FrequencyInUse into KeyboardAdjustedFrequency when a key is pressed.
        /// </summary>
        [JsonIgnore]
        public byte Key { get { return key; } set { key = value; SetKeyboardAdjustedFrequency(key); } }

        /// <summary>
        /// The output volume of the oscillator. When set to a value greater than zero, the oscillator
        /// output is sent to the output mixer to directly participate in sound generating rather than
        /// to only modulate another oscillator (which it can do at the same time).
        /// </summary>
        public byte Volume;

        /// <summary>
        /// The waveform generated by the oscillator: SQARE, SAW_UP, SAW_DOWN, TRIANGLE, SINE, RANDOM or NOISE.
        /// </summary>
        public WAVEFORM WaveForm;

        /// <summary>
        /// Keyboard/LFO flag. Denotes whether the oscillator is used as an LFO 
        /// or a tone generator that generates frequencies depending on played key.
        /// True => using Keyboard, false => LFO.
        /// </summary>
        public Boolean Keyboard;

        /// <summary>
        /// UseAdsr: true => use ADSR, false => play pulse
        /// </summary>
        public Boolean UseAdsr;

        /// <summary>
        /// Modulation sensitivity is how much a modulation source modulates an oscillator.
        /// </summary>
        public double AM_Sensitivity;
        /// <summary>
        /// Modulation sensitivity is how much a modulation source modulates an oscillator.
        /// </summary>
        public double FM_Sensitivity;
        /// <summary>
        /// Modulation sensitivity is how much a modulation source modulates an oscillator.
        /// </summary>
        public double XM_Sensitivity;

        /// <summary>
        /// Phase is used to alter high/low time ratio for square waves
        /// and as a phase shift for other wave forms except random and noise.
        /// </summary>
        public double Phase;

        public int ModulationKnobTarget;
        public int ModulationWheelTarget;
        public bool ViewMe;

        /// <summary>
        /// A modulator is an oscillator used to modulate another oscillator
        /// </summary>
        [JsonIgnore]
        public Oscillator AM_Modulator;
        public int AM_ModulatorId = -1;

        /// <summary>
        /// A modulator is an oscillator used to modulate another oscillator
        /// </summary>
        [JsonIgnore]
        public Oscillator FM_Modulator;
        public int FM_ModulatorId = -1;

        /// <summary>
        /// A modulator is an oscillator used to modulate another oscillator
        /// </summary>
        [JsonIgnore]
        public Oscillator XM_Modulator;
        public int XM_ModulatorId = -1;

        /// <summary>
        /// A list containint all oscillators modulated by current (this) oscillator
        /// </summary>
        [JsonIgnore]
        public List<Oscillator> Modulating;

        /// <summary>
        /// The cyclic position, angle, within a waveform. Varies from zero to 2 PI.
        /// Used to calculate sample values for a waveform.
        /// </summary>

        [JsonIgnore]
        public double AngleLeft;
        public double AngleRight;

        /// <summary>
        /// StepSize is used when generating a waveform. Each sample is depending on
        /// how large step within a cycle a momentary sample value in order to create
        /// a certain frequency. Used in GenerateAudioData and MakeGraphData to generate
        /// the waveform. Also altered when FM modulating with non-sine waveforms.
        /// </summary>
        [JsonIgnore]
        public double StepSize;

        /// <summary>
        /// This is an offset from StepSize only used for FM modulation ()NOT DX style!)
        /// in order to retain StepSize. Normally set to 0.
        /// </summary>
        [JsonIgnore]
        public double StepSizeOffset;

        /// <summary>
        /// Wave data for one frame to deliver.
        /// </summary>
        [JsonIgnore]
        public double[] WaveData;

        #endregion properties

        ///////////////////////////////////////////////////////////////////////////////////////////////////////
        // Property actions
        ///////////////////////////////////////////////////////////////////////////////////////////////////////
        #region propertyActions

        /// <summary>
        /// Combines frequency and finetune into finetunedFrequency and lfoFrequency.
        /// Also sets FrequencyInUse to lfoFrequency or finetunedFrequency depending on the Keyboard switch setting.
        /// Also sets StepSize accordingly.
        /// </summary>
        public void SetFrequency()
        {
            // Keyboard frequency range: 10 - 10.000 Hz. Fixed frequency range: 0.1 - 100 Hz.
            // Knobs ranges from 0 to 1000, but for finetunedFrequency we want it to look like
            // coarse ranges from 0 to 10000 in steps of 100 and fine from 0 - 100 in steps of 0.01
            // and for lfoFrequency like coarse ranges from 0 - 100 in steps of 1 and fine in steps of 0.01.
            if (Keyboard)
            {
                finetunedFrequency = frequency + finetune / 100.0f;
                finetunedFrequency = finetunedFrequency > 10000 ? 10000 : finetunedFrequency;
                FrequencyInUse = finetunedFrequency;
            }
            else
            {
                lfoFrequency = frequencyLfo + finetuneLfo / 1000;
                lfoFrequency = lfoFrequency > 100 ? 100 : lfoFrequency;
                lfoFrequency = lfoFrequency < 0.01f ? 0.01f : lfoFrequency;
                FrequencyInUse = lfoFrequency;
            }
            //if (mainPage != null && mainPage.FrameServer != null)
            //{
            //    StepSize = FrequencyInUse * Math.PI * 2 / (double)mainPage.SampleRate;
            //}
        }

        /// <summary>
        /// Translates from key number to frequency, or selects lfo frequency, depending 
        /// on Keyboard flag, and stores the result in FrequencyInUse, but only if key is not 0x80.
        /// </summary>
        /// <param name="key"></param>
        public void SetKeyboardAdjustedFrequency(byte key)
        {
            if (mainPage != null && key != 0xff)
            {
                keyboardAdjustedFrequency = finetunedFrequency * mainPage.NoteFrequency[key] / 440f;
                if (Keyboard)
                {
                    FrequencyInUse = keyboardAdjustedFrequency;
                }
                else
                {
                    FrequencyInUse = lfoFrequency;
                }
            }
        }
        #endregion propertyActions

        ///////////////////////////////////////////////////////////////////////////////////////////////////////
        // Locals
        ///////////////////////////////////////////////////////////////////////////////////////////////////////
        #region locals

        /// <summary>
        /// This is a main switch used to let the oscillator run only when it should.
        /// The AudioGraph and Frameinput node is always running, so oscillator
        /// may create sound during initalizations, which is not prefered because
        /// it makes short sound bursts before all initiation is done. Turn on last
        /// at KeyOn and turn off first at ReleaseOscillator.
        /// </summary>
        [JsonIgnore]
        public bool IsOn;
        [JsonIgnore]
        public bool KeyOn;
        [JsonIgnore]
        public bool KeyOff;

        private Random random = new Random();

        [JsonIgnore]
        public double CurrentSample;

        [JsonIgnore]
        public MainPage mainPage;

        /// <summary>
        /// When MIDI shannel is set to 'All' the oscillator still needs
        /// to know where an incoming CC originates from in order to know
        /// weather to react to an incoming Pitch Bender or not.
        /// </summary>
        [JsonIgnore]
        private int incomingMidiChannel;

        [JsonIgnore]
        private double frequency;
        [JsonIgnore]
        private double finetune;
        [JsonIgnore]
        private double frequencyLfo;
        [JsonIgnore]
        private double finetuneLfo;
        [JsonIgnore]
        private byte key;
        [JsonIgnore]
        private double keyboardAdjustedFrequency;
        [JsonIgnore]
        private double finetunedFrequency;
        [JsonIgnore]
        private double lfoFrequency;
        [JsonIgnore]
        public Rotator selectorWave;
        [JsonIgnore]
        public Rotator selectorKeyboard;
        [JsonIgnore]
        public Rotator selectorModulation;
        [JsonIgnore]
        public Rotator selectorAdsrPulse;

        [JsonIgnore]
        public Complex[] fftData;
        [JsonIgnore]
        public Boolean Advance;

        /// <summary>
        /// chorusOffset is the offset the left and right channel in
        /// frequencies, by adding chorusOffset to the stepSize of
        /// left channel, and subtracting corusPhase to the stepSize
        /// of the rigth channel. chorusOffset is updated for each sample
        /// in SetStepSize, but since SetStepSize is also called when
        /// modulating a wave, there is a bool argument to only use
        /// chorusOffset once.
        /// </summary>
        [JsonIgnore]
        private double chorusOffset;

        /// <summary>
        /// Forward is a switch for 'bouncing' chorus against maximum
        /// frequency deviation. By simply adding/subtracting to/from
        /// chorusOffset beween maximum frequency deviation up and
        /// down, a triangle 'wave' of deviation is created.
        /// </summary>
        [JsonIgnore]
        private bool Forward;

        [JsonIgnore]
        double adsrStart;
        [JsonIgnore]
        double adsrRamp;
        [JsonIgnore]
        public double randomValue;
        [JsonIgnore]
        private double lowSineWaveCompensation;

        [JsonIgnore]
        public bool NeedNewWaveShape;

        [JsonIgnore]
        public bool needsToGenerateWaveShape;

        /// <summary>
        /// Holds the sound generating. Use for short time only in order for oscillator to wait
        /// with generating data because something that could interfere is just going on.
        /// Hold for very short time, max a few milliseconds.
        /// </summary>
        #endregion locals

        ///////////////////////////////////////////////////////////////////////////////////////////////////////
        // Constructor
        ///////////////////////////////////////////////////////////////////////////////////////////////////////
        #region construction

        [JsonConstructor]
        public Oscillator()
        {
        }

        /// <summary>
        /// Copy constructor
        /// NOTE: Sub objects has to be created and added afterwards,
        /// Filter, PitchEnvelope, ADSR and WaveShape.
        /// </summary>
        /// <param name="oscillator"></param>
        public Oscillator(MainPage mainPage, Oscillator oscillator)
        {
            this.mainPage = mainPage;
            Filter = new Filter();
            Filter.FilterFunction = oscillator.Filter.FilterFunction;
            //dispatcher = new KeyDispatcher(mainPage);
            AM_Sensitivity = oscillator.AM_Sensitivity;
            FM_Sensitivity = oscillator.FM_Sensitivity;
            XM_Sensitivity = oscillator.XM_Sensitivity;
            AM_ModulatorId = oscillator.AM_ModulatorId;
            FM_ModulatorId = oscillator.FM_ModulatorId;
            XM_ModulatorId = oscillator.XM_ModulatorId;
            if (oscillator.Modulating == null)
            {
                Modulating = new List<Oscillator>();
            }
            else
            {
                Modulating = oscillator.Modulating;
            }
            FineTune = oscillator.FineTune;
            FinetunedFrequency = oscillator.FinetunedFrequency;
            Frequency = oscillator.Frequency;
            FrequencyInUse = oscillator.FrequencyInUse;
            FrequencyLfo = oscillator.FrequencyLfo;
            FineTune = oscillator.FineTune;
            FineTuneLfo = oscillator.FineTuneLfo;
            Id = oscillator.Id;
            KeyboardAdjustedFrequency = oscillator.KeyboardAdjustedFrequency;
            Keyboard = oscillator.Keyboard;
            LfoFrequency = oscillator.LfoFrequency;
            MidiChannel = oscillator.MidiChannel;
            ModulationKnobTarget = oscillator.ModulationKnobTarget;
            ModulationVelocitySensitivity = oscillator.ModulationVelocitySensitivity;
            ModulationWheelTarget = oscillator.ModulationWheelTarget;
            UseAdsr = oscillator.UseAdsr;
            VelocitySensitive = oscillator.VelocitySensitive;
            Volume = oscillator.Volume;
            WaveForm = oscillator.WaveForm;
            Phase = oscillator.Phase;
            StepSize = oscillator.StepSize;
            StepSizeOffset = oscillator.StepSizeOffset;
            AngleLeft = oscillator.AngleLeft;
            AngleRight = oscillator.AngleRight;
            lowSineWaveCompensation = 1;
            KeyOff = false;
            KeyOn = false;
            //NeedNewWaveShape = true;
        }

        public Oscillator(MainPage mainPage)
        {
            this.mainPage = mainPage;
            //dispatcher = new KeyDispatcher(mainPage);
            frequency = 440;
            finetune = 0;
            frequencyLfo = 5;
            finetuneLfo = 0;
            //WaveData = new double[mainPage.SampleCount * 2];
            //NeedNewWaveShape = true;
            WaveShape = new WaveShape(this);
            Modulating = new List<Oscillator>();
        }

        public void Init(MainPage mainPage)
        {
            PitchEnvelope = new PitchEnvelope(mainPage, this);
            //WaveData = new double[mainPage.SampleCount * 2];
            Adsr = new ADSR(mainPage, this);
            Keyboard = true;
            frequency = 440;
            lfoFrequency = 5;
            FineTune = 0.0f;
            MidiChannel = 0;
            VelocitySensitive = 0;
            Velocity = 0x40;
            Phase = Math.PI;
            AM_Sensitivity = 0;
            FM_Sensitivity = 0;
            XM_Sensitivity = 128;
            UseAdsr = true;
            Filter = new Filter(mainPage);
            Filter.oscillator = this;
            //NeedNewWaveShape = true;
            WaveShape = new WaveShape(this);
        }

        #endregion construction

        ///////////////////////////////////////////////////////////////////////////////////////////////////////
        // Re-initializations on KeyOn()
        ///////////////////////////////////////////////////////////////////////////////////////////////////////
        #region reInitializations
        /// <summary>
        /// Re-initiates the oscillator at key-on and calls InitModulators
        /// that in turn recursively initiates all connected modulators.
        /// </summary>
        /// <param name="key"></param>
        public void InitOscillator(int channel, byte key, byte velocity)
        {
            Key = key;
            KeyOff = false;
            KeyOn = false;
            incomingMidiChannel = channel;
            StepSize = (1 + PitchEnvelope.Value * PitchEnvelope.PitchEnvPitch) * FrequencyInUse * Math.PI * 2 / mainPage.SampleRate;
            StepSizeOffset = 0;
            Velocity = (byte)(VelocitySensitive % 2 > 0 ? velocity : 64);
            ModulationVelocitySensitivity = VelocitySensitive > 1 ? (double)velocity / 128.0 : 1;
            CurrentSample = 1;
            AngleLeft = 0;
            AngleRight = 0;
            adsrStart = UseAdsr ? 0 : 1;
            PitchEnvelope.Value = 0;
            if (AM_Modulator != null)
            {
                AM_Modulator.InitOscillator(channel, key, velocity);
            }
            if (FM_Modulator != null)
            {
                FM_Modulator.InitOscillator(channel, key, velocity);
            }
            if (XM_Modulator != null)
            {
                XM_Modulator.InitOscillator(channel, key, velocity);
            }
        }

        public void CreateWaveData(uint requestedNumberOfSamples)
        {
            WaveData = new double[requestedNumberOfSamples * 2];
        }

        public void SetStepSize()
        {
            StepSize = (double)FrequencyInUse * Math.PI * 2 / mainPage.SampleRate;
        }

        #endregion reInitializations

        ///////////////////////////////////////////////////////////////////////////////////////////////////////
        // Audio generating functions
        ///////////////////////////////////////////////////////////////////////////////////////////////////////
        #region audioGenerating

        /// <summary>
        /// Generates one chunk of audio data.
        /// Also modulates data.
        /// </summary>
        /// <param name="requestedNumberOfSamples"></param>
        /// <returns></returns>
        public void GenerateAudioData(uint requestedNumberOfSamples)
        {
            //randomValue = 0;
            if (WaveShape.WaveData == null)
            {
                WaveShape.WaveData = new double[requestedNumberOfSamples];
            }

            // Make one cycle of wave samples if needed:
            //WaveShape.SetCanBeUsed(this);
            if (needsToGenerateWaveShape)
            {
                WaveShape.MakeWave(requestedNumberOfSamples);
                if (Filter.FilterFunction > 0)
                {
                    Filter.PostCreationInit(requestedNumberOfSamples);
                    WaveShape.WaveData = Filter.Apply(WaveShape.WaveData, key);
                }
                // This will turn off NeedsToBeCreated if the created WaveShape is not going
                // need to be created every frame.
                if (WaveShape.WaveShapeUsage == WaveShape.Usage.CREATE_ONCE)
                {
                    needsToGenerateWaveShape = false;
                }
            }

            // We will modify and use the above paragraph when rules are formed.
            //WaveShape.MakeWave(requestedNumberOfSamples);


            PitchEnvelope.Advance();
            AdvanceModulatorEnvelopes();
            SetStepSize(this);
            //StepSize = mainPage.PitchBend[MidiChannel] * (1 + PitchEnvelope.Value * PitchEnvelope.PitchEnvPitch) * FrequencyInUse * Math.PI * 2 / mainPage.SampleRate;

            WaveData = new double[requestedNumberOfSamples * 2]; // Because here we produce stereo!
            CalculateChorus();

            if (WaveForm == WAVEFORM.SINE)
            {
                lowSineWaveCompensation = mainPage.EarCompensation.KeyToGain(key);
            }

            // At KeyOn the current (first) frame ramps up from 0 to Adsr.AdsrLevel.
            // At KeyOff the current (last) frame ramps down from Adsr.AdsrLevel to 0.
            // Frames in between ramps between previous value of Adsr.AdsrLevel to 
            // current level of Adsr.AdsrLevel.
            // If Adsr is set to Pulse, this works fine. AdsrLevel is set to 127 and
            // AdsrState is set to AdsrStates.SUSTAIN at KeyOn, and Adsr is set to
            // AdsrStates.RELEASE_END _after_ the last frame is generated.
            adsrRamp = adsrStart / requestedNumberOfSamples;

            // Generate requestedNumberOfSamples stereo samples:

            // Temporary!
            //WaveShape.ResetCanBeUsed();

            for (int i = 0; i < requestedNumberOfSamples * 2; i++)
            {
                Channel channel = (Channel)(i % 2);
                MarkModulators(this);

                if (WaveShape.WaveShapeUsage != WaveShape.Usage.NONE)
                {
                    CurrentSample = MakeModulatedWaveFromWaveShape(i, channel, requestedNumberOfSamples);
                }
                else if (WaveForm == WAVEFORM.RANDOM)
                {
                    CurrentSample = randomValue;
                }
                else if (XM_Modulator != null && WaveForm == WAVEFORM.SQUARE
                        && XM_Modulator.Keyboard == false)
                {
                    CurrentSample = MakeModulatedWave(channel, OscillatorUsage.OUTPUT);
                }
                else if (XM_Modulator != null && WaveForm == WAVEFORM.SINE
                        && XM_Modulator.WaveForm == WAVEFORM.SINE && XM_Modulator.Keyboard == true)
                {
                    CurrentSample = MakeDXStyleWave(channel, OscillatorUsage.OUTPUT);
                }
                else
                {
                    CurrentSample = MakeModulatedWave(channel, OscillatorUsage.OUTPUT);
                }
                AdvanceAngle(this, channel);
                AdvanceModulatorsAngles(this, channel);

                if (i % 2 == 0)
                {
                    adsrStart -= adsrRamp;
                }
                //if (WaveForm == WAVEFORM.RANDOM)
                //{
                //    CurrentSample = randomValue;
                //}

                if (WaveForm == WAVEFORM.SINE)
                //if (lowSineWaveCompensation > 0)
                {
                    CurrentSample *= lowSineWaveCompensation;
                }
                CurrentSample *= Velocity / 127f;
                if (UseAdsr && !KeyOff)
                {
                    CurrentSample *= adsrStart;
                }
                
                CurrentSample *= Volume / 128f;
                WaveData[i] = CurrentSample;
                adsrStart = Adsr.AdsrLevel;
            }
            if (KeyOff)
            {
                mainPage.dispatcher[Id].ReleaseOscillator(key);
            }
            else
            {
                Adsr.Advance();
                mainPage.allowGuiUpdates = true;
            }
        }

        /// <summary>
        /// CalculateChorus makes chorusOffset 'bounce' between a maximum
        /// frequency deviation in a triangle wave fashion.
        /// </summary>
        private void CalculateChorus()
        {
            double speed = 0.0; // 1 -> 10 => 0.000001 -> 0.00001
            double depth = 0.0;

            switch (mainPage.Chorus.Selection)
            {
                case 0:
                    chorusOffset = 0.0;
                    break;
                case 1:
                    speed = mainPage.Settings.ChorusSpeed1 * 0.01;
                    depth = mainPage.Settings.ChorusDepth1 * 0.00003;
                    break;
                case 2:
                    speed = mainPage.Settings.ChorusSpeed2 * 0.01;
                    depth = mainPage.Settings.ChorusDepth2 * 0.00003;
                    break;
                case 3:
                    speed = mainPage.Settings.ChorusSpeed3 * 0.01;
                    depth = mainPage.Settings.ChorusDepth3 * 0.00003;
                    break;
            }

            //speed = 0.000001;
            if (Forward)
            {
                chorusOffset += speed * depth;
                if (chorusOffset > depth)
                {
                    Forward = false;
                }
            }
            else
            {
                chorusOffset -= speed * depth;
                if (chorusOffset < -depth)
                {
                    Forward = true;
                }
            }
        }


        private void SetStepSize(Oscillator oscillator, bool chorus = true)
        {
            //if (chorus)
            //{
            //    switch (mainPage.Chorus.Selection)
            //    {
            //        case 0:
            //            chorusOffset = 0;
            //            break;
            //        case 1:
            //            if (Forward)
            //            {
            //                chorusOffset += 0.000001;// 0.0000015;
            //                if (chorusOffset > 0.00006) //0.00005)
            //                {
            //                    Forward = false;
            //                }
            //            }
            //            else
            //            {
            //                chorusOffset -= 0.0000015;
            //                if (chorusOffset < -0.00005)
            //                {
            //                    Forward = true;
            //                }
            //            }
            //            break;
            //        case 2:
            //            if (Forward)
            //            {
            //                chorusOffset += 0.000006;
            //                if (chorusOffset > 0.00015)
            //                {
            //                    Forward = false;
            //                }
            //            }
            //            else
            //            {
            //                chorusOffset -= 0.000006;
            //                if (chorusOffset < -0.00015)
            //                {
            //                    Forward = true;
            //                }
            //            }
            //            break;
            //        case 3:
            //            if (Forward)
            //            {
            //                chorusOffset += 0.000012;
            //                if (chorusOffset > 0.00025)
            //                {
            //                    Forward = false;
            //                }
            //            }
            //            else
            //            {
            //                chorusOffset -= 0.000012;
            //                if (chorusOffset < -0.00025)
            //                {
            //                    Forward = true;
            //                }
            //            }
            //            break;
            //    }
            //}
            //else
            //{
            //    chorusOffset = 0;
            //}

            oscillator.StepSize = mainPage.PitchBend[incomingMidiChannel] * 
                (1 + oscillator.PitchEnvelope.Value * oscillator.PitchEnvelope.PitchEnvPitch) * 
                oscillator.FrequencyInUse * Math.PI * 2 / mainPage.SampleRate;
            if (oscillator.AM_Modulator != null && oscillator.AM_Modulator.Keyboard)
            {
                SetStepSize(oscillator.AM_Modulator, false);
            }
            if (oscillator.FM_Modulator != null && oscillator.FM_Modulator.Keyboard)
            {
                SetStepSize(oscillator.FM_Modulator, false);
            }
            if (oscillator.XM_Modulator != null && oscillator.XM_Modulator.Keyboard)
            {
                SetStepSize(oscillator.XM_Modulator, false);
            }
        }

        private void AdvanceModulatorEnvelopes()
        {
            if (AM_Modulator != null)
            {
                AM_Modulator.PitchEnvelope.Advance();
                AM_Modulator.Adsr.Advance();
                AM_Modulator.AdvanceModulatorEnvelopes();
            }
            if (FM_Modulator != null)
            {
                FM_Modulator.PitchEnvelope.Advance();
                FM_Modulator.Adsr.Advance();
                FM_Modulator.AdvanceModulatorEnvelopes();
            }
            if (XM_Modulator != null)
            {
                XM_Modulator.PitchEnvelope.Advance();
                XM_Modulator.Adsr.Advance();
                XM_Modulator.AdvanceModulatorEnvelopes();
            }
        }

        #endregion audioGenerating

        ///////////////////////////////////////////////////////////////////////////////////////////////////////
        // Basic wave generating functions
        ///////////////////////////////////////////////////////////////////////////////////////////////////////

        #region basicWaveGenerating

        /// <summary>
        /// Moves the angle in Radians one StepSize forward. Backs up 2 * PI when
        /// Radians exeeds 2 * PI. 
        /// </summary>
        /// <param name="oscillator"></param>
        /// <returns>true if Radians backed up, else false (used when generating random waveform to detect when to generate a new sample</returns>
        public void AdvanceAngle(Oscillator oscillator, Channel channel)
        {
            double angle = channel == Channel.LEFT ? oscillator.AngleLeft : oscillator.AngleRight;
            double offsetDirection = channel == Channel.LEFT ? +1 : -1;

            //if (channel == Channel.LEFT)
            //{
            angle += oscillator.StepSize + oscillator.StepSizeOffset + chorusOffset * offsetDirection;
            angle += oscillator.PitchEnvelope.Value * Get_FM_Sensitivity(this) / 20480 * Math.PI;
            // + oscillator.Adsr.AdsrLevel;
            while (angle > Math.PI * 2)
            {
                angle -= Math.PI * 2;
                if (oscillator.WaveForm == WAVEFORM.RANDOM)
                {
                    oscillator.randomValue = random.NextDouble() * 2 - 1;
                }
            }
            while (angle < 0)
            {
                angle += Math.PI * 2;
                if (oscillator.WaveForm == WAVEFORM.RANDOM)
                {
                    oscillator.randomValue = random.NextDouble() * 2 - 1;
                }
            }
            if (channel == Channel.LEFT)
            {
                oscillator.AngleLeft = angle;
            }
            else
            {
                oscillator.AngleRight = angle;
            }

            //}
            //else
            //{
            //    oscillator.AngleRight += oscillator.StepSize + oscillator.StepSizeOffset - chorusOffset;// + oscillator.PitchEnvelope.Value * PitchEnvelope.PitchEnvPitch;
            //    while (oscillator.AngleRight > Math.PI * 2)
            //    {
            //        oscillator.AngleRight -= Math.PI * 2;
            //    }
            //    while (oscillator.AngleRight < 0)
            //    {
            //        oscillator.AngleRight += Math.PI * 2;
            //    }
            //}
            //if (oscillator.WaveForm == WAVEFORM.NOISE)
            //{
            //    oscillator.RandomValue = (random.Next(1000) - 500) / 500.0;
            //}
        }

        public void AdvanceModulatorsAngles(Oscillator oscillator, Channel channel)
        {
            double angle = channel == Channel.LEFT ? oscillator.AngleLeft : oscillator.AngleRight;

            if (oscillator.AM_Modulator != null)
            {
                AdvanceModulatorsAngles(oscillator.AM_Modulator, channel);
                if (oscillator.AM_Modulator.Advance)
                {
                    AdvanceAngle(oscillator.AM_Modulator, channel);
                    oscillator.AM_Modulator.Advance = false;
                }
            }
            if (oscillator.FM_Modulator != null)
            {
                AdvanceModulatorsAngles(oscillator.FM_Modulator, channel);
                if (oscillator.FM_Modulator.Advance)
                {
                    oscillator.AdvanceAngle(oscillator.FM_Modulator, channel);
                    oscillator.FM_Modulator.Advance = false;
                }
            }
            if (oscillator.XM_Modulator != null)
            {
                AdvanceModulatorsAngles(oscillator.XM_Modulator, channel);
                if (oscillator.XM_Modulator.Advance)
                {
                    oscillator.AdvanceAngle(oscillator.XM_Modulator, channel);
                    oscillator.XM_Modulator.Advance = false;
                }
            }
        }

        public void MarkModulators(Oscillator oscillator)
        {
            if (oscillator.AM_Modulator != null)
            {
                MarkModulators(oscillator.AM_Modulator);
                oscillator.AM_Modulator.Advance = true;
            }
            if (oscillator.FM_Modulator != null)
            {
                MarkModulators(oscillator.FM_Modulator);
                oscillator.FM_Modulator.Advance = true;
            }
            if (oscillator.XM_Modulator != null)
            {
                MarkModulators(oscillator.XM_Modulator);
                oscillator.XM_Modulator.Advance = true;
            }
        }
        #endregion basicWaveGenerating
    }
}
