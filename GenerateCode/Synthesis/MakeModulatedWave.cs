using System;
using System.Reflection.Metadata;
using UwpControlsLibrary;

namespace UwpControlsLibrary
{

    /// <summary>
    /// MakeModulatedWave() is called to make one sample for the oscillator to hand over
    /// to the AudioGraph system. 
    /// At a sampling rate of 48 kHz MakeModulatedWave() is called 480 times for each
    /// frame of samples for the AuriodGraph system.
    /// One frame containing 480 samples is handed over each 10 millisecond.
    /// Thus, MakeModulatedWave() is called 48000 times per second.
    /// When more than one oscillator is producing sound (volume is turned up) this
    /// number is multiplied by the number of sounding oscillators.
    /// 
    /// When an oscillator is modulated by another oscillator, MakeModulatedWave() is called
    /// recursively to obtain modulation data before making the sample to deliver. Modulated
    /// modulators also make recursive calls the same way enabling multiple levels of modu-
    /// lation. Each oscillator originally produces a sample with a value within the range
    /// of -1 to +1.
    /// 
    /// There are four types of modulations:
    /// 
    /// Amplitude Modulation, AM, where an oscillator is modulated to follow the modulators
    /// sample value to limit the volume the sample the oscillator is producing.
    /// 
    /// Frequency Modulation, FM, where an oscillator is modulated to change its frequency
    /// by a factor proportional to the modulators sample value. FM is obtained by adding
    /// to the value of StepSize.
    /// Note: do not confuse FM with the DX style frequency modulation which uses a specific
    /// operation used in FM synthesizers like the Yamaha DX series!
    /// 
    /// Extra Modulation, XM, comes in two flavours depending on the wave form:
    /// 1) Square waves has the time duration ratio between high value (+1) and low value (-1) 
    /// changed by the modulator, which changes the overtone spectra following the modulator.
    /// 2) DX style FM synthesis occurs only when the oscillator and the modulator both has
    /// the waveform Sine, and both are following the keyboare, i.e. are not set to be LFO's.
    /// XM has no effect for waveform combinations other than the two mentioned above.
    /// 
    /// Since modulators are used by MakeModulatedWave(), other inputs must be taken in account
    /// already here. Only exception is the Filter which is always only possible to use on
    /// the oscillators that actually produces sound, and that occurs after MakeModulatedWave()
    /// has returned all 480 samples of one frame.
    /// </summary>
    public partial class Oscillator : ControlBase
    {
        public double MakeModulatedWaveFromWaveShape(int i, Channel channel, uint requestedNumberOfSamples)
        {
            double am = 0;
            double angle = channel == Channel.LEFT ? AngleLeft : AngleRight;
            double offset = channel == Channel.LEFT ? chorusOffset : -chorusOffset;
            StepSizeOffset = 0;

            if (AM_Modulator != null)
            {
                am = (1 - Get_AM_Sensitivity(this) * AM_Modulator.MakeModulatedWave(channel, OscillatorUsage.MODULATION));
                //WaveShape.SetNeedsToBeCreated();
            }
            if (FM_Modulator != null)
            {
                OscillatorUsage usage = ModulationKnobTarget == 1 ? OscillatorUsage.FM : OscillatorUsage.FM_PLUS_MINUS;
                StepSizeOffset = Get_FM_Sensitivity(this) * FM_Modulator.MakeModulatedWave(channel, usage) * StepSize;
                //WaveShape.SetNeedsToBeCreated();
            }
            if (XM_Modulator != null && WaveForm != WAVEFORM.SINE) // XM modulation for sine waves are handled in WaveShape
            {
                Phase = (1 - Get_XM_Sensitivity(this) * XM_Modulator.MakeModulatedWave(channel, OscillatorUsage.MODULATION) / 256) * Math.PI;
                WaveShape.WaveShapeUsage = WaveShape.Usage.CREATE_ALWAYS;
            }
            if (WaveForm == WAVEFORM.NOISE)
            {
                // Do not apply frequency to noise, just copy left or right
                // value, but also adjust for chorus:
                int pos = i / 2;
                if (i % 2 == 0)
                {
                    pos += (int)(chorusOffset * 100000.0);
                }
                else
                {
                    pos -= (int)(chorusOffset * 100000.0);
                }
                pos = (int)(pos >= requestedNumberOfSamples ? pos - requestedNumberOfSamples : pos);
                pos = (int)(pos < 0 ? pos + requestedNumberOfSamples : pos);
                //return am + WaveShape.WaveData[((int)(angle * requestedNumberOfSamples / Math.PI / 2)) % requestedNumberOfSamples];
                return (am + 1) * WaveShape.WaveData[pos];
            }
            else
            {
                return (1 - am) * WaveShape.WaveData[((int)(angle * requestedNumberOfSamples / Math.PI / 2)) % requestedNumberOfSamples];
            }
        }

        public double MakeModulatedWave(Channel channel, OscillatorUsage usage)
        {
            double sample = 1;
            double am = 0;
            bool makeWave = true;
            double offset = channel == Channel.LEFT ? chorusOffset : -chorusOffset;
            StepSizeOffset = 0;

            if (WaveForm == WAVEFORM.SQUARE)
            {
                if (AM_Modulator != null)
                {
                    am = Get_AM_Sensitivity(this) * AM_Modulator.MakeModulatedWave(channel, OscillatorUsage.MODULATION);
                }
                if (FM_Modulator != null)
                {
                    OscillatorUsage modulatorUsage = ModulationKnobTarget == 1 ? OscillatorUsage.FM : OscillatorUsage.FM_PLUS_MINUS;
                    StepSizeOffset = Get_FM_Sensitivity(this) * FM_Modulator.MakeModulatedWave(channel, modulatorUsage) * StepSize;
                }
                if (XM_Modulator != null)
                {
                    Phase = Get_XM_Sensitivity(this) / 265.0 * XM_Modulator.MakeModulatedWave(channel, OscillatorUsage.MODULATION) * Math.PI;
                }
                // oscillator.AM_Sensitivity = 0 - 1, 0 for no modulation, just wave as is.
                // am = 0 - 1, 0 is no modulation, 1 is full modulation, on each sample
                //sample = MakeSquareWave(channel, usage) - this.AM_Sensitivity * am * MakeSquareWave(channel, usage);
                //sample = MakeSquareWave(channel, usage) * (1 - this.AM_Sensitivity * am);
                sample = MakeSquareWave(channel, usage) * (1 - am);
            }
            else if (WaveForm == WAVEFORM.SAW_DOWN)
            {
                if (AM_Modulator != null)
                {
                    am = Get_AM_Sensitivity(this) * AM_Modulator.MakeModulatedWave(channel, OscillatorUsage.MODULATION);
                }
                if (FM_Modulator != null)
                {
                    //StepSizeOffset = Get_FM_Sensitivity(this) * FM_Modulator.MakeModulatedWave(channel, Usage) / 2048 * Math.PI;
                    OscillatorUsage modulatorUsage = ModulationKnobTarget == 1 ? OscillatorUsage.FM : OscillatorUsage.FM_PLUS_MINUS;
                    StepSizeOffset = Get_FM_Sensitivity(this) * FM_Modulator.MakeModulatedWave(channel, modulatorUsage) * StepSize;
                }
                sample = (1 - am) * MakeSawDownWave(channel, usage);
            }
            else if (WaveForm == WAVEFORM.SAW_UP)
            {
                if (AM_Modulator != null)
                {
                    am = Get_AM_Sensitivity(this) * AM_Modulator.MakeModulatedWave(channel, OscillatorUsage.MODULATION);
                }
                if (FM_Modulator != null)
                {
                    //StepSizeOffset = (Get_FM_Sensitivity(this) * (1 + FM_Modulator.MakeModulatedWave(channel, OscillatorUsage.MODULATION)) / 8) * Math.PI;
                    OscillatorUsage modulatorUsage = ModulationKnobTarget == 1 ? OscillatorUsage.FM : OscillatorUsage.FM_PLUS_MINUS;
                    StepSizeOffset = Get_FM_Sensitivity(this) * FM_Modulator.MakeModulatedWave(channel, modulatorUsage) * StepSize;
                }
                sample = (1 - am) * MakeSawUpWave(channel, usage);
            }
            else if (WaveForm == WAVEFORM.TRIANGLE)
            {
                if (AM_Modulator != null)
                {
                    am = Get_AM_Sensitivity(this) * AM_Modulator.MakeModulatedWave(channel, OscillatorUsage.MODULATION);
                }
                if (FM_Modulator != null)
                {
                    //StepSizeOffset = (Get_FM_Sensitivity(this) * (1 + FM_Modulator.MakeModulatedWave(channel, OscillatorUsage.MODULATION)) / 8) * Math.PI;
                    OscillatorUsage modulatorUsage = ModulationKnobTarget == 1 ? OscillatorUsage.FM : OscillatorUsage.FM_PLUS_MINUS;
                    StepSizeOffset = Get_FM_Sensitivity(this) * FM_Modulator.MakeModulatedWave(channel, modulatorUsage) * StepSize;
                }
                sample = (1 - am) * MakeTriangleWave(channel, usage);
            }
            else if (WaveForm == WAVEFORM.SINE)
            {
                if (AM_Modulator != null)
                {
                    //am = MakeSineWave(channel, usage) * (0.5 + Get_AM_Sensitivity(this) * AM_Modulator.MakeModulatedWave(channel, Usage.MODULATION) / 2) * Math.PI;
                    am = 0.5 + Get_AM_Sensitivity(this) * AM_Modulator.MakeModulatedWave(channel, OscillatorUsage.MODULATION) / 2 * Math.PI;
                }
                if (FM_Modulator != null)
                {
                    //StepSizeOffset = Get_FM_Sensitivity(this) * (1 + FM_Modulator.MakeModulatedWave(channel, OscillatorUsage.MODULATION)) / 8 * Math.PI;
                    OscillatorUsage modulatorUsage = ModulationKnobTarget == 1 ? OscillatorUsage.FM : OscillatorUsage.FM_PLUS_MINUS;
                    StepSizeOffset = Get_FM_Sensitivity(this) * FM_Modulator.MakeModulatedWave(channel, modulatorUsage) * StepSize;
                }
                // This is now always handled by oscillator via MakeDXStyleWave() below. Maybe we should
                // use this again, via WaveShape, when all modulators are a multiple of the base frequency?
                //if (XM_Modulator != null && Keyboard && XM_Modulator.WaveForm == WAVEFORM.SINE && XM_Modulator.Keyboard)
                //{
                //    sample = am + Math.Sin(AngleLeft
                //        + Get_XM_Sensitivity(this) * ModulationVelocitySensitivity * XM_Modulator.MakeModulatedWave(channel, Usage.MODULATION));
                //}
                sample = (1 - am) * MakeSineWave(channel, usage);
            }
            else if (WaveForm == WAVEFORM.RANDOM)
            {
                if (AM_Modulator != null)
                {
                    //am = MakeRandomWave() * (0.5 + Get_AM_Sensitivity(this) * AM_Modulator.MakeModulatedWave(channel, Usage.MODULATION) / 2);
                    am = 0.5 + Get_AM_Sensitivity(this) * AM_Modulator.MakeModulatedWave(channel, OscillatorUsage.MODULATION) / 2;
                }
                //if (FM_Modulator != null)
                //{
                //    StepSizeOffset = (Get_FM_Sensitivity(this) * (FM_Modulator.MakeModulatedWave(channel, OscillatorUsage.MODULATION)) / 80) * Math.PI;
                //}
                //else
                //{
                //    StepSizeOffset = 0;
                //}
                if (XM_Modulator != null)
                {
                    Phase = (0.5 + Get_XM_Sensitivity(this) * XM_Modulator.MakeModulatedWave(channel, OscillatorUsage.MODULATION) / 2) * Math.PI;
                }
                sample = (1 - am) * MakeRandomWave(); // WaveShape.WaveData[((int)(Angle * requestedNumberOfSamples / Math.PI / 2)) % requestedNumberOfSamples];
            }
            else if (WaveForm == WAVEFORM.NOISE)
            {
                if (AM_Modulator != null)
                {
                    //sample = MakeNoiseWave() * (0.5 + Get_AM_Sensitivity(this) * AM_Modulator.MakeModulatedWave(channel, Usage.MODULATION) / 2) * Math.PI;
                    sample = 0.5 + Get_AM_Sensitivity(this) * AM_Modulator.MakeModulatedWave(channel, OscillatorUsage.MODULATION) / 2 * Math.PI;
                    makeWave = false;
                }
                if (makeWave)
                {
                    sample = MakeNoiseWave();
                }
            }
            else
            {
                sample = MakeWave(channel, OscillatorUsage.OUTPUT);
            }
            return sample;
        }

        public double MakeDXStyleWave(Channel channel, OscillatorUsage usage)
        {
            double angle = channel == Channel.LEFT ? AngleLeft : AngleRight;
            double sample;
            double am = 0;

            if (AM_Modulator != null)
            {
                am = Get_AM_Sensitivity(this) * AM_Modulator.MakeModulatedWave(channel, OscillatorUsage.MODULATION);
            }
            if (FM_Modulator != null)
            {
                StepSizeOffset = Get_FM_Sensitivity(this) * FM_Modulator.MakeModulatedWave(channel, OscillatorUsage.FM) * StepSize;
            }
            if (XM_Modulator != null && XM_Modulator.WaveForm == WAVEFORM.SINE && XM_Modulator.Keyboard)
            {
                sample = Math.Sin(angle
                    + 2 * Get_XM_Sensitivity(this) / 256.0 * ModulationVelocitySensitivity * XM_Modulator.MakeDXStyleWave(channel, usage));
            }
            else
            {
                sample = Math.Sin(angle);
            }
            return sample * (1 - am);
        }


        public double Get_AM_Sensitivity(Oscillator oscillator)
        {
            double sensitivity = oscillator.AM_Sensitivity;
            if (((Rotator)mainPage.PitchEnvelopeGUIs[mainPage.OscillatorToPitchEnvelope(oscillator.Id)]
                .SubControls.ControlsList[(int)PitchEnvelopeControls.MOD_AM]).Selection == 1)
            {
                sensitivity *= oscillator.PitchEnvelope.Value;
            }
            if (((Rotator)mainPage.AdsrGUIs[mainPage.OscillatorToPitchEnvelope(oscillator.Id)]
                .SubControls.ControlsList[(int)AdsrControls.ADSR_AM_SENS]).Selection == 1)
            {
                sensitivity *= oscillator.Adsr.AdsrLevel;
            }
            return sensitivity * oscillator.ModulationVelocitySensitivity;
        }

        public double Get_FM_Sensitivity(Oscillator oscillator)
        {
            double sensitivity = oscillator.FM_Sensitivity;
            if (((Rotator)mainPage.PitchEnvelopeGUIs[mainPage.OscillatorToPitchEnvelope(oscillator.Id)]
                .SubControls.ControlsList[(int)PitchEnvelopeControls.MOD_FM]).Selection == 1)
            {
                sensitivity *= oscillator.PitchEnvelope.Value;
            }
            if (((Rotator)mainPage.AdsrGUIs[mainPage.OscillatorToPitchEnvelope(oscillator.Id)]
                .SubControls.ControlsList[(int)AdsrControls.ADSR_FM_SENS]).Selection == 1)
            {
                sensitivity *= oscillator.Adsr.AdsrLevel;
            }
            return sensitivity * oscillator.ModulationVelocitySensitivity;
        }

        public double Get_XM_Sensitivity(Oscillator oscillator)
        {
            double sensitivity = oscillator.XM_Sensitivity;
            if (((Rotator)mainPage.PitchEnvelopeGUIs[mainPage.OscillatorToPitchEnvelope(oscillator.Id)]
                .SubControls.ControlsList[(int)PitchEnvelopeControls.MOD_XM]).Selection == 1)
            {
                sensitivity *= oscillator.PitchEnvelope.Value;
            }
            if (((Rotator)mainPage.AdsrGUIs[mainPage.OscillatorToPitchEnvelope(oscillator.Id)]
                .SubControls.ControlsList[(int)AdsrControls.ADSR_XM_SENS]).Selection == 1)
            {
                sensitivity *= oscillator.Adsr.AdsrLevel;
            }
            return sensitivity * oscillator.ModulationVelocitySensitivity;
        }
    }
}
