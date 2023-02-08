using System;
using System.Collections.Generic;
using System.ServiceModel.Channels;
using UwpControlsLibrary;
using Windows.Foundation;
using Windows.Media.Core;

namespace UwpControlsLibrary
{
    /// <summary>
    /// All oscillators on poly = 0 has an instance of WaveShape containing a class Filter
    /// to hold filter settings for the oscillator on all poly.
    /// It is the oscillator that implements the filter functionality,
    /// which is why no such functionality is implemented here.
    /// </summary>
    public enum FilterFunction
    {
        OFF,
        FIXED,
        ADSR_POSITIVE,
        ADSR_NEGATIVE,
        AM_MODULATOR,
        FM_MODULATOR,
        XM_MODULATOR
    }

    public class WaveShape
    {
        public enum Usage
        {
            NONE,
            CREATE_ONCE,
            CREATE_ALWAYS,
        }

        #region attributes

        /// <summary>
        /// Filters internal float[SampleRate / 100] holding filtered data. This data is to be
        /// read using a value for Angle between 0 and 4 PI. It delivers wave data
        /// with a frequency that defined the step length. 
        /// </summary>

        public double Phase;
        public double Angle;

        public double[] WaveData;
        public bool Buzy;

        #endregion attributes

        #region locals

        private Random random = new Random();
        private Synthesis synthesis;
        private Oscillator oscillator;
        double stepSize;
        public Usage WaveShapeUsage;

        #endregion locals

        #region contstruction

        public WaveShape(Oscillator oscillator)
        {
            this.oscillator = oscillator;
            this.synthesis = oscillator.synthesis;
            //WaveData = new double[requestedNumberOfSamples];
            Phase = Math.PI;
            Angle = 0;
            Buzy = false;
        }

        //public WaveShape(WaveShape waveShape)
        //{
        //}

        //public WaveShape(Oscillator oscillator, WaveShape waveShape)
        //{
        //    this.oscillator = oscillator;
        //    synthesis = oscillator.synthesis;
        //    //WaveData = new double[requestedNumberOfSamples];
        //    Phase = waveShape.Phase;
        //    Angle = waveShape.Angle;
        //    Buzy = false;
        //}

        #endregion construction

        #region wavecreation

        /// <summary>
        /// Makes a standard waveform.
        /// The resulting waveform is of double frequency in order to
        /// accomodate a square sub tone. When using the result,
        /// read at half speed.
        /// </summary>
        /// <param name="waveForm"></param>
        public void MakeWave(uint requestedNumberOfSamples)
        {
            WaveData = new double[requestedNumberOfSamples];
            stepSize = Math.PI * 2 / requestedNumberOfSamples;
            Angle = 0;
            Phase = oscillator.Phase;
            Buzy = true;
            for (int i = 0; i < requestedNumberOfSamples; i++)
            {
                oscillator.MarkModulators(oscillator);
                WaveData[i] = MakeWaveSample(oscillator);
                AdvanceAngle(requestedNumberOfSamples);
            }
            Buzy = false;
        }

        #endregion wavecreation

        ///////////////////////////////////////////////////////////////////////////////////////////////////////
        // Basic wave generating functions
        ///////////////////////////////////////////////////////////////////////////////////////////////////////
        #region basicWaveGenerating

        /// <summary>
        /// Calls the proper wave generation algorithm, depending on oscillator's waveform
        /// to generate one sample of wave data.
        /// </summary>
        /// <param name="oscillator"></param>
        /// <returns></returns>
        public double MakeWaveSample(Oscillator oscillator)
        {
            switch (oscillator.WaveForm)
            {
                case WAVEFORM.SQUARE:
                    return MakeSquareWave();
                case WAVEFORM.SAW_UP:
                    return MakeSawUpWave();
                case WAVEFORM.SAW_DOWN:
                    return MakeSawDownWave();
                case WAVEFORM.TRIANGLE:
                    return MakeTriangleWave();
                case WAVEFORM.SINE:
                    return MakeSineWave(oscillator);
                case WAVEFORM.NOISE:
                    return MakeNoiseWave();
                default:
                    return 0;
            }
        }

        /// <summary>
        /// Algorithm for generating a squarewave sample depending on the Angle in Radians.
        /// </summary>
        /// <param name="oscillator"></param>
        /// <returns>One sample of wave data</returns>
        private float MakeSquareWave()
        {
            if (Angle > Phase)
            {
                return -1.0f;
            }
            else
            {
                return 1.0f;
            }
        }

        /// <summary>
        /// Algorithm for generating a sawtooth up wave sample depending on the Angle in Radians.
        /// </summary>
        /// <param name="oscillator"></param>
        /// <returns>One sample of wave data</returns>
        private double MakeSawUpWave()
        {
            double value = Angle / Math.PI - 1.0f;
            return value;
        }

        /// <summary>
        /// Algorithm for generating a sawtooth down wave sample depending on the Angle in Radians.
        /// </summary>
        /// <param name="oscillator"></param>
        /// <returns>One sample of wave data</returns>
        private double MakeSawDownWave()
        {
            double value = 1.0f - Angle / Math.PI;
            return value;
        }

        /// <summary>
        /// Algorithm for generating a triangle wave sample depending on the Angle in Radians.
        /// </summary>
        /// <param name="oscillator"></param>
        /// <returns>One sample of wave data</returns>
        private double MakeTriangleWave()
        {
            //                              In order to sync to the other waveforms, triangle
            //   /\            /\           needs to start at zero. Drawing up during first 
            //  /  \          /  \          half period, and down during second half will off-
            // /    \        /    \         set 1/2 PI. Instead, we use the schema below:
            //------------------------- ------------------------------------------------------
            //        \    /        \     / 0 - PI/2 -> from 0 to 1
            //         \  /          \   /  PI/2 - PI -> from 1 to 0
            //          \/            \ /   PI - 3PI/2 -> from 0 to -1
            //                              3PI/2 - 2PI -> from -1 to 0

            double sample = 0;

            if (Angle < Math.PI / 2)
            {
                sample = 2 * Angle / Math.PI;
            }
            else if (Angle < Math.PI)
            {
                sample = 1 - 2 * (Angle - Math.PI / 2) / Math.PI;
            }
            else if (Angle < 3 * Math.PI / 2)
            {
                sample = 0f - 2 * (Angle - Math.PI) / Math.PI;
            }
            else
            {
                sample = 2 * (Angle - 3 * Math.PI / 2) / Math.PI - 1;
            }

            return sample;
        }

        /// <summary>
        /// Algorithm for generating a sine wave sample depending on the Angle in Radians.
        /// </summary>
        /// <param name="oscillator"></param>
        /// <returns>One sample of wave data</returns>
        private double MakeSineWave(Oscillator oscillator)
        {
            if (oscillator.XM_Modulator != null && oscillator.XM_Modulator.WaveForm == WAVEFORM.SINE && oscillator.XM_Modulator.Keyboard)
            {
                return Math.Sin(Angle
                    + oscillator.Get_XM_Sensitivity(oscillator) * oscillator.ModulationVelocitySensitivity / 64f * MakeSineWave(oscillator.XM_Modulator));
            }
            else
            {
                return Math.Sin(Angle);
            }
        }
        //private double MakeSineWave(Oscillator oscillator)
        //{
        //    if (oscillator.XM_Modulator != null && oscillator.XM_Modulator.WaveForm == WAVEFORM.SINE && oscillator.XM_Modulator.Keyboard)
        //    {
        //        return Math.Sin(Angle
        //            + oscillator.Get_XM_Sensitivity(oscillator) * oscillator.ModulationVelocitySensitivity / 64f * MakeSineWave(oscillator.XM_Modulator));
        //    }
        //    else
        //    {
        //        return Math.Sin(Angle);
        //    }
        //}

        /// <summary>
        /// Algorithm for generating a random wave sample for each step.
        /// </summary>
        /// <param name="oscillator"></param>
        /// <returns>One sample of wave data</returns>
        private float MakeNoiseWave()
        {
            return 0.002f * (random.Next(1000) - 500);
        }

        /// <summary>
        /// Moves the Angle in Radians one StepSize forward. Backs up 2 * PI when
        /// Radians exeeds 2 * PI. 
        /// </summary>
        /// <param name="oscillator"></param>
        /// <returns>true if Radians backed up, else false (used when generating random waveform to detect when to generate a new sample</returns>
        private Boolean AdvanceAngle(uint requestedNumberOfSamples)
        {
            Angle += Math.PI * 2 / requestedNumberOfSamples;
            if (Angle > Math.PI * 2)
            {
                Angle -= Math.PI * 2;
            }
            if (Angle < 0)
            {
                Angle += Math.PI * 2;
            }
            return true;
        }

        //private void AdvanceAngle(Oscillator oscillator, uint requestedNumberOfSamples)
        //{
        //    oscillator.AngleLeft += Math.PI * 2 / (float)requestedNumberOfSamples;
        //    if (oscillator.AngleLeft > Math.PI * 2)
        //    {
        //        oscillator.AngleLeft -= Math.PI * 2;
        //    }
        //}

        public void AdvanceModulatorsAngles(Oscillator oscillator)
        {
            if (oscillator.AM_Modulator != null)
            {
                AdvanceModulatorsAngles(oscillator.AM_Modulator);
                if (oscillator.AM_Modulator.Advance)
                {
                    oscillator.AdvanceAngle(oscillator.AM_Modulator, Channel.LEFT);
                    oscillator.AdvanceAngle(oscillator.AM_Modulator, Channel.RIGHT);
                    oscillator.AM_Modulator.Advance = false;
                }
            }
            if (oscillator.FM_Modulator != null)
            {
                AdvanceModulatorsAngles(oscillator.FM_Modulator);
                if (oscillator.FM_Modulator.Advance)
                {
                    oscillator.AdvanceAngle(oscillator.FM_Modulator, Channel.LEFT);
                    oscillator.AdvanceAngle(oscillator.FM_Modulator, Channel.RIGHT);
                    oscillator.FM_Modulator.Advance = false;
                }
            }
            if (oscillator.XM_Modulator != null)
            {
                //if (oscillator.WaveForm == WAVEFORM.SINE
                //&& oscillator.XM_Modulator.WaveForm == WAVEFORM.SINE && oscillator.XM_Modulator.Keyboard == true)
                //{
                //    oscillator.AdvanceAngle(oscillator.XM_Modulator, true);
                //    oscillator.AdvanceAngle(oscillator.XM_Modulator, false);
                //}


                AdvanceModulatorsAngles(oscillator.XM_Modulator);
                if (oscillator.XM_Modulator.Advance
                    || (oscillator.WaveForm == WAVEFORM.SINE 
                    && oscillator.XM_Modulator.WaveForm == WAVEFORM.SINE 
                    && oscillator.XM_Modulator.Keyboard == true))
                {
                    oscillator.AdvanceAngle(oscillator.XM_Modulator, Channel.LEFT);
                    oscillator.AdvanceAngle(oscillator.XM_Modulator, Channel.RIGHT);
                    oscillator.XM_Modulator.Advance = false;
                }
            }
        }

        //public void AdvanceFrequencyModulationSynthesisAngles(Oscillator oscillator, uint requestedNumberOfSamples)
        //{
        //    if (oscillator.XM_Modulator != null)
        //    {
        //        AdvanceModulatorsAngles(oscillator.XM_Modulator);
        //    }
        //    AdvanceAngle(requestedNumberOfSamples);
        //    //oscillator.XM_Modulator.Advance = false;
        //}

        #endregion basicWaveGenerating

        public Usage SetWaveShapeUsage(Oscillator oscillator)
        {
            // Assume no usage:
            WaveShapeUsage = Usage.NONE;

            if (oscillator.WaveForm == WAVEFORM.NOISE && oscillator.Filter.FilterFunction > 0)
            {
                WaveShapeUsage =  Usage.CREATE_ALWAYS;
            }
            //if (oscillator.WaveForm == WAVEFORM.RANDOM)
            //{
            //    WaveShapeUsage =  Usage.CREATE_ALWAYS;
            //}

            //if (oscillator.WaveForm == WAVEFORM.SINE && oscillator.XM_Modulator != null
            //        && oscillator.XM_Modulator.WaveForm == WAVEFORM.SINE && oscillator.XM_Modulator.Keyboard)
            //{
            //    if (AnyDXStyleModulationUsesOddFrequency(oscillator))
            //    {
            //        WaveShapeUsage =  Usage.CREATE_ALWAYS;
            //    }
            //    else
            //    {
            //        WaveShapeUsage =  Usage.NONE;
            //    }
            //}

            if (oscillator.WaveForm == WAVEFORM.SINE && oscillator.XM_Modulator != null
                    && oscillator.XM_Modulator.WaveForm == WAVEFORM.SINE && oscillator.XM_Modulator.Keyboard)
            {
                //((Rotator)mainPage.FilterGUIs[mainPage.OscillatorToFilter(oscillator.Id)].SubControls.
                //    ControlsList[(int)FilterControls.FILTER_FUNCTION]).Selection = 0;
                oscillator.Filter.FilterFunction = 0;
            }

            if (oscillator.WaveForm != WAVEFORM.NOISE && oscillator.Filter.FilterFunction == 1)
            {
                WaveShapeUsage =  Usage.CREATE_ONCE;
            }

            if (oscillator.WaveForm != WAVEFORM.NOISE && oscillator.Filter.FilterFunction > 1)
            {
                WaveShapeUsage =  Usage.CREATE_ALWAYS;
            }

            //// (Note that LFO's does not use the filter, but the filter type selector can still be on.)
            //if (oscillator.Keyboard && oscillator.Filter.FilterFunction > 1)
            //{
            //    WaveShapeUsage =  Usage.;
            //}

            // If a squarewave is beeing XM modulated:
            if (oscillator.WaveForm == WAVEFORM.SQUARE && oscillator.XM_Modulator != null && oscillator.Filter.FilterFunction > 0)
            {
                WaveShapeUsage =  Usage.CREATE_ALWAYS;
            }

            //
            //if ()
            //{
            //    this.needsToBeCreated = true;
            //}
            return WaveShapeUsage;
        }

        ///// <summary>
        ///// When WaveShape is in use, there are situations when the WaveShape needs
        ///// to be re-created between frames, such as when a filter is changing over
        ///// time due to modulation from some source.
        ///// </summary>
        //public void ResetNeedsToBeCreated()
        //{
        //    SetNeedsToBeCreated(null, false);
        //}

        ///// <summary>
        ///// When WaveShape is in use, there are situations when the WaveShape needs
        ///// to be re-created between frames, such as when a filter is changing over
        ///// time due to modulation from some source.
        ///// </summary>
        //public void SetNeedsToBeCreated()
        //{
        //    SetNeedsToBeCreated(null, true);
        //}

        ///// <summary>
        ///// When WaveShape is in use, there are situations when the WaveShape needs
        ///// to be re-created between frames, such as when a filter is changing over
        ///// time due to modulation from some source.
        ///// </summary>
        //public void SetNeedsToBeCreated(Oscillator oscillator, bool needsToBeCreated = true)
        //{
        //    if (oscillator != null)
        //    {
        //        // Oscillator supplied:
        //        this.NeedsToBeCreated = false;

        //        // Waveforms that always change between frames:
        //        if (oscillator.WaveForm == WAVEFORM.NOISE)
        //        {
        //            this.NeedsToBeCreated = true;
        //        }
        //        if (oscillator.WaveForm == WAVEFORM.RANDOM)
        //        {
        //            this.NeedsToBeCreated = true;
        //        }

        //        // DX style synthesis needs to be re-generated between frames only when
        //        // modulator(s) do not use frequencies that are multiples of the base frequency:
        //        if (oscillator.WaveForm == WAVEFORM.SINE && oscillator.XM_Modulator != null
        //                && oscillator.XM_Modulator.WaveForm == WAVEFORM.SINE && oscillator.XM_Modulator.Keyboard)
        //        {
        //            this.NeedsToBeCreated = AnyDXStyleModulationUsesOddFrequency(oscillator);
        //        }

        //        // If the filter is changing over time, frames needs to be re-created between frames:
        //        // (Note that LFO's does not use the filter, but the filter type selector can still be on.)
        //        if (oscillator.Keyboard && oscillator.Filter.FilterFunction > 0)
        //        {
        //            this.NeedsToBeCreated = true;
        //        }

        //        // If a squarewave is beeing XM modulated:
        //        if (oscillator.WaveForm == WAVEFORM.SQUARE && oscillator.XM_Modulator != null)
        //        {
        //            this.NeedsToBeCreated = true;
        //        }

        //        //
        //        //if ()
        //        //{
        //        //    this.needsToBeCreated = true;
        //        //}
        //    }
        //    else
        //    {
        //        // Default argument supplied:
        //        this.NeedsToBeCreated = needsToBeCreated;
        //    }
        //}

        ///// <summary>
        ///// WaveShape needs to be used in order to speed up complex modulation situations but
        ///// is also used for simple waveforms, thus enabling low-end computers to do simple synthesis.
        ///// One frame containing exactly one period of the synthesized wave is then re-used for
        ///// many subsequent frames.
        ///// Note that the need to re-create WaveShape implies that it also needs to be used.
        ///// Therefore SetNeedsToBeCreated() automatically sets SetNeedToBeUsed.
        ///// </summary>
        //public void SetCanBeUsed(bool canBeUsed)
        //{
        //    SetCanBeUsed(null, canBeUsed);
        //}

        ///// <summary>
        ///// WaveShape needs to be used in order to speed up complex modulation situations but
        ///// is also used for simple waveforms, thus enabling low-end computers to do simple synthesis.
        ///// One frame containing exactly one period of the synthesized wave is then re-used for
        ///// many subsequent frames.
        ///// Note that the need to re-create WaveShape implies that it also needs to be used.
        ///// Therefore SetNeedsToBeCreated() automatically sets SetNeedToBeUsed.
        ///// </summary>
        //public void SetCanBeUsed()
        //{
        //    SetCanBeUsed(null, true);
        //}

        ///// <summary>
        ///// WaveShape needs to be used in order to speed up complex modulation situations but
        ///// is also used for simple waveforms, thus enabling low-end computers to do simple synthesis.
        ///// One frame containing exactly one period of the synthesized wave is then re-used for
        ///// many subsequent frames.
        ///// Note that the need to re-create WaveShape implies that it also needs to be used.
        ///// Therefore SetNeedsToBeCreated() automatically sets SetNeedToBeUsed.
        ///// </summary>
        //public void ResetCanBeUsed()
        //{
        //    SetCanBeUsed(null, false);
        //}

        ///// <summary>
        ///// Tests oscillator and all XM_Modulator objects in chain abov the oscillator
        ///// for frequencies that could cause a need for re-generating WaveShape for each frame.
        ///// </summary>
        //public void SetCanBeUsed(Oscillator oscillator, bool canBeUsed = true)
        //{
        //    this.CanBeUsed = false;
        //    if (oscillator != null)
        //    {
        //        if (oscillator.XM_Modulator != null)
        //        {
        //            if (IsMultippleFrequency(oscillator.FrequencyInUse, oscillator.XM_Modulator.FrequencyInUse))
        //            {
        //                CanBeUsed = false; //NoDXStyleModulationUsesOddFrequency(oscillator.XM_Modulator);
        //            }
        //            else
        //            {
        //                CanBeUsed = true;
        //            }
        //        }
        //        else
        //        {
        //            CanBeUsed = DescideNeed(oscillator);
        //        }
        //    }
        //    else
        //    {
        //        CanBeUsed = canBeUsed;
        //    }

        //    NeedsToBeCreated = CanBeUsed;
        //    if (CanBeUsed)
        //    {
        //        ((Indicator)mainPage.OscillatorGUIs[oscillator.Id].SubControls.
        //            ControlsList[(int)OscillatorControls.LEDSOUNDING]).IsOn = true;
        //        ((Indicator)mainPage.OscillatorGUIs[oscillator.Id].SubControls.
        //            ControlsList[(int)OscillatorControls.LEDSOUNDING_ADVANCED]).IsOn = false;
        //    }
        //    else
        //    {
        //        ((Indicator)mainPage.OscillatorGUIs[oscillator.Id].SubControls.
        //            ControlsList[(int)OscillatorControls.LEDSOUNDING]).IsOn = true;
        //        ((Indicator)mainPage.OscillatorGUIs[oscillator.Id].SubControls.
        //            ControlsList[(int)OscillatorControls.LEDSOUNDING_ADVANCED]).IsOn = false;
        //    }
        //}

        //private bool DescideNeed(Oscillator oscillator)
        //{
        //    // Assume need to use WaveShape:
        //    bool result = true;
        //    if (oscillator.WaveForm == WAVEFORM.NOISE)
        //    {
        //        result = false;
        //    }
        //    if (oscillator.WaveForm == WAVEFORM.RANDOM)
        //    {
        //        result = false;
        //    }

        //    if (oscillator.WaveForm == WAVEFORM.SINE && oscillator.XM_Modulator != null
        //            && oscillator.XM_Modulator.WaveForm == WAVEFORM.SINE && oscillator.XM_Modulator.Keyboard)
        //    {
        //        result = AnyDXStyleModulationUsesOddFrequency(oscillator);
        //    }
			
        //    if (oscillator.Filter.FilterFunction > 1)
        //    {
        //        result = false;
        //    }
			
        //    // (Note that LFO's does not use the filter, but the filter type selector can still be on.)
        //    if (oscillator.Keyboard && oscillator.Filter.FilterFunction > 1)
        //    {
        //        result = false;
        //    }

        //    // If a squarewave is beeing XM modulated:
        //    if (oscillator.WaveForm == WAVEFORM.SQUARE && oscillator.XM_Modulator != null)
        //    {
        //        return false;
        //    }

        //    //
        //    //if ()
        //    //{
        //    //    this.needsToBeCreated = true;
        //    //}
        //    return result;
        //}

        private bool AnyDXStyleModulationUsesOddFrequency(Oscillator oscillator)
        {
            if (oscillator.XM_Modulator != null)
            {
                if (!IsMultippleFrequency(oscillator.FrequencyInUse, oscillator.XM_Modulator.FrequencyInUse))
                {
                    return true;
                }
                else
                {
                    return AnyDXStyleModulationUsesOddFrequency(oscillator.XM_Modulator);
                }
            }
            else
            {
                return false;
            }
        }

        private bool IsMultippleFrequency(double freqBase, double freq)
        {
            double multi = freq / freqBase;
            multi -= Math.Truncate(multi);
            return multi < 0.0001;
        }
    }

    public partial class Oscillator : ControlBase
    {

        /////////////////////////////////////////////////////////////////////////////////////////////////////////
        //// Oscillograph methods
        /////////////////////////////////////////////////////////////////////////////////////////////////////////
        //#region oscillograph

        ///// <summary>
        ///// Uses the wave-generating functions to produce two cycles of wave data
        ///// as an array of 200 integers to use by the oscilloscope as a 200 pixel wide graph.
        ///// </summary>
        ///// <param name="oscillator" the selected oscillator for which to show waveform></param>
        ///// <returns></returns>
        //public Point[] MakeGraphData(double height)
        //{
        //    double OriginalPhase = Phase;
        //    double OriginalAngle = Angle;
        //    double OriginalStepSize = StepSize;
        //    BackupOriginalModulators(this);
        //    ResetModulators(this, FrequencyInUse);

        //    double[] localBuffer = new double[200];
        //    StepSize = Math.PI / 50;
        //    Angle = 0;
        //    Phase = 0;
        //    int graphRandomValue = random.Next(20) - 10;

        //    for (int i = 0; i < 200; i++)
        //    {
        //        MarkModulators(this);
        //        //if (XM_Modulator != null)
        //        ////if (ModulationType == ModulationType.DX)
        //        //{
        //        //    localBuffer[i] = 0;// HBE MakeDxWave(this);
        //        //    //Modulate();
        //        //    AdvanceAngle(this);
        //        //    AdvanceModulatorsAngles(this);
        //        //}
        //        //else if (WaveForm == WAVEFORM.SINE)
        //        //{
        //        //    localBuffer[i] = MakeWave();
        //        //    //Modulate();
        //        //    AdvanceAngle(this);
        //        //    AdvanceModulatorsAngles(this);
        //        //}
        //        //else if (WaveForm == WAVEFORM.RANDOM)
        //        //{
        //        //    if (i % 40 == 0)
        //        //    {
        //        //        graphRandomValue = random.Next(20) - 10;
        //        //    }
        //        //    localBuffer[i] = graphRandomValue;
        //        //}
        //        //else
        //        {
        //            localBuffer[i] = mainPage.Patch.Oscillators[0][Id].WaveShape.WaveData[(i * 4) % requestedNumberOfSamples];
        //        }
        //    }

        //    Phase = OriginalPhase;
        //    StepSize = OriginalStepSize;
        //    Angle = OriginalAngle;
        //    return NormalizeAmplitude(localBuffer, height);
        //}

        ///// <summary>
        ///// Used by MakeGraphData to translate the float values generated 
        ///// by the wave-generating functions into integers with an amplitude of 80 pixels.
        ///// </summary>
        ///// <param name="inBuffer"></param>
        ///// <returns></returns>
        //public Point[] NormalizeAmplitude(double[] inBuffer, double height)
        //{
        //    double factor = 0;
        //    Point[] outBuffer = new Point[inBuffer.Length];
        //    double max = 0;
        //    double min = 0;
        //    double offset = 0;

        //    // Measure largest amplitude:
        //    for (int i = 0; i < inBuffer.Length; i++)
        //    {
        //        if (inBuffer[i] > 0 && max < inBuffer[i])
        //        {
        //            max = inBuffer[i];
        //        }
        //        else if (inBuffer[i] < 0 && min > inBuffer[i])
        //        {
        //            min = inBuffer[i];
        //        }
        //    }
        //    factor = max - min;
        //    offset = (max + min) / 2;

        //    // Convert to an amplitude of 80 peak-to-peak and invert signal:
        //    for (int i = 0; i < inBuffer.Length; i++)
        //    {
        //        if (factor > 0)
        //        {
        //            // Graph hight = 92 px. Center is 66 so span is from 20 - 112
        //            outBuffer[i] = new Point(i + 9, 1.5 * (44 + (offset - inBuffer[i]) * height / factor));
        //        }
        //        else
        //        {
        //            outBuffer[i] = new Point(i + 9, 66);
        //        }
        //    }

        //    RestoreOriginalModulators(this);
        //    return outBuffer;
        //}

        //private void BackupOriginalModulators(Oscillator oscillator)
        //{
        //    if (oscillator.XM_Modulator != null)
        //    {
        //        BackupOriginalModulators(oscillator.XM_Modulator);
        //        OriginalModulatorsPhase = oscillator.XM_Modulator.Phase;
        //        OriginalModulatorsAngle = oscillator.XM_Modulator.Angle;
        //        OriginalModulatorsStepSize = oscillator.XM_Modulator.StepSize;
        //    }
        //}

        //private void RestoreOriginalModulators(Oscillator oscillator)
        //{
        //    if (oscillator.XM_Modulator != null)
        //    {
        //        RestoreOriginalModulators(oscillator.XM_Modulator);
        //        oscillator.XM_Modulator.Phase = OriginalModulatorsPhase;
        //        oscillator.XM_Modulator.Angle = OriginalModulatorsAngle;
        //        oscillator.XM_Modulator.StepSize = OriginalModulatorsStepSize;
        //    }
        //}

        //private void ResetModulators(Oscillator oscillator, double frequency)
        //{
        //    if (oscillator.XM_Modulator != null)
        //    {
        //        ResetModulators(oscillator.XM_Modulator, frequency);
        //        oscillator.XM_Modulator.Phase = 0;
        //        oscillator.XM_Modulator.Angle = 0;
        //        oscillator.XM_Modulator.StepSize = oscillator.XM_Modulator.FrequencyInUse / frequency * Math.PI / 50;
        //    }
        //}

        //#endregion oscillograph
        public Oscillator(List<Oscillator> value)
        {
            Value = value;
        }

        public List<Oscillator> Value { get; }
    }
}
