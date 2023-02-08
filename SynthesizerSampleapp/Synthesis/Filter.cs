using MathNet.Numerics.IntegralTransforms;
using System;
using System.Net.NetworkInformation;
using System.Numerics;
using Windows.UI.Xaml.Controls;

namespace UwpControlsLibrary
{
    public class Filter
    {
        public double Q;
        public double FrequencyCenter;
        public double KeyboardFollow;
        public double Gain;
        public double Mix;
        public int FilterFunction = 0;
        public int ModulationWheelTarget = 0;

        public Synthesis synthesis;

        /// <summary>
        /// The filter applies to fftData.
        /// For a smoother sound fftData is mixed with three previous fftData.
        /// Those are ramped down with age, so that the oldest sample is 0.
        /// 
        /// </summary>
        public Complex[] fftData;

        public Oscillator oscillator;

        //public Filter() { }

        public Filter(Synthesis synthesis)
        {
            this.synthesis = synthesis;
            this.FilterFunction = 0;
            this.Mix = 0;
            this.FrequencyCenter = 0;
            this.Gain = 0;
            this.KeyboardFollow = 127;
            this.ModulationWheelTarget = 0;
            this.Q = 0;
            //filterCurve = new FilterCurve(synthesis.FrameServer.SampleCount, 5.0, 10.0, 15.0, 35.0, 90.0, 95.0, 3.0);
        }

        //public Filter(Oscillator oscillator)
        //{
        //    this.mainPage = oscillator.synthesis;
        //    this.oscillator = oscillator;
        //    this.FilterFunction = oscillator.Filter.FilterFunction;
        //    this.Mix = oscillator.Filter.Mix;
        //    this.FrequencyCenter = oscillator.Filter.FrequencyCenter;
        //    this.Gain = oscillator.Filter.Gain;
        //    this.KeyboardFollow = oscillator.Filter.KeyboardFollow;
        //    this.ModulationWheelTarget = oscillator.Filter.ModulationWheelTarget;
        //    this.Q = oscillator.Filter.Q;
        //}

        public void PostCreationInit(uint requestedNumberOfSamples)
        {
            fftData = new Complex[requestedNumberOfSamples];
            for (int i = 0; i < requestedNumberOfSamples; i++)
            {
                fftData[i] = new Complex(0, 0);
            }
        }

        public double[] Apply(double[] waveData, int Key)
        {
            double[] result = new double[waveData.Length];

            if (fftData != null && fftData.Length > 0)
            {
                for (int i = 0; i < waveData.Length; i++)
                {
                    fftData[i] = waveData[i];
                }

                // Forward Fourier transformation:
                Fourier.Forward(fftData, FourierOptions.AsymmetricScaling);

                // Then filter the data:
                double q;
                double fc = 440;
                double y;

                // MIDI key = 0 - 127 => fcKey = 8.176 - 12543.850 Hz
                // Fc knob = 0 - 127 => fc = 440 - fcKey
                // = 440 + fcKnob * (fcKey - 440)
                double keyFrequency = 440 + KeyboardFollow * (synthesis.NoteKeys.NoteFrequency[Key] - 440) / 127.0;
                q = Q / 2.0;
                q = Math.Pow(q * q / fftData.Length, 2f) / 100.0;
                switch (FilterFunction)
                {
                    case 0:
                    case 1:
                        fc = keyFrequency / 25.4 + (FrequencyCenter - 63) / 4.0;
                        break;
                    case 2:
                        fc = oscillator.Adsr.AdsrLevel * keyFrequency / 25.4 + (FrequencyCenter - 63) / 4.0;
                        break;
                    case 3:
                        fc = (1 - oscillator.Adsr.AdsrLevel) * keyFrequency / 25.4 + (FrequencyCenter - 63) / 4.0;
                        break;
                    case 4:
                        fc = (oscillator.PitchEnvelope.Value + 1.0) * keyFrequency / 50.8 + (FrequencyCenter - 63) / 4.0;
                        break;
                    case 5:
                        if (oscillator.XM_Modulator != null)
                        {
                            // Use any channel to create filter.
                            fc = (1.0 - oscillator.XM_Modulator.MakeWave(Channel.LEFT, OscillatorUsage.MODULATION)) * oscillator.Get_XM_Sensitivity(oscillator) * keyFrequency / 6502.4 + (FrequencyCenter - 63) / 4.0;
                        }
                        else
                        {
                            fc = keyFrequency / 25.4 + (FrequencyCenter - 63) / 4.0;
                        }
                        break;
                    case 6:
                        fc = keyFrequency / 25.4 + (FrequencyCenter - 63) / 4.0;
                        fc *= synthesis.LFO.MakeTriangleWave(0, OscillatorUsage.MODULATION);
                        break;
                }
                fftData[0] = new Complex(0, 0);
                fftData[fftData.Length / 2] = new Complex(0, 0);
                for (int i = 0; i < (fftData.Length / 2) - 1; i++)
                {
                    y = 1 - (q * (i - fc) * (i - fc));
                    y = y < 0 ? 0 : y;
                    fftData[i] = new Complex(fftData[i].Real * y, fftData[i].Imaginary * y);
                    fftData[fftData.Length - i - 1] = new Complex(fftData[fftData.Length - i - 1].
                        Real * y, fftData[fftData.Length - i - 1].Imaginary * y);
                }

                // Return to time domain:
                Fourier.Inverse(fftData, FourierOptions.AsymmetricScaling);

                for (int i = 0; i < fftData.Length; i++)
                {
                    result[i] = fftData[i].Real * (1 + Gain / 32) + waveData[i] * Mix / 127.0;
                }
            }
            return result;
        }
    }
}
