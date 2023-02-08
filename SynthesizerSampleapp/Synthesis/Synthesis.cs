using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Media.Audio;

namespace UwpControlsLibrary
{
    public partial class Synthesis : ControlBase
    {
        public int Volume { set { SetVolume(value); } } 
        public List<List<Oscillator>> Oscillators;
        public Oscillator LFO;
        public KeyDispatcher[] Dispatcher;
        public FrameServer FrameServer;
        public NoteKeys NoteKeys;
        public double[] PitchBend; // -1 -- +1 => -1 octave -- +1 octave
        public Boolean allowGuiUpdates = false;
        public Boolean allowUpdateOscilloscope = false;
        public CurrentActivity CurrentActivity = CurrentActivity.NONE;
        public uint SampleRate;
        public EarCompensation EarCompensation;
        public int SampleCount;

        public Synthesis(Controls controls, int Id, int polyphony, int oscillatorCount, bool addCommonLFO = false)
        {
        //    FrameServer = new FrameServer(this);
        //    CreateObjects(controls, Id, polyphony, oscillatorCount, addCommonLFO);
        //}
        //private async void CreateObjects(Controls controls, int Id, int polyphony, int oscillatorCount, bool addCommonLFO = false)
        //{ 
        //    while (FrameServer.SampleCount == 0)
        //    {
        //        await Task.Delay(1);
        //    }
            if (addCommonLFO)
            {
                LFO = new Oscillator(this, 1);
                LFO.Usage = OscillatorUsage.MODULATION;
                LFO.Keyboard = false;
                LFO.WaveForm = WAVEFORM.TRIANGLE;
                LFO.SetFrequency();
            }
            else
            {
                LFO = null;
            }
            Oscillators = new List<List<Oscillator>>();
            for (int poly = 0; poly < polyphony; poly++)
            {
                Oscillators.Add(new List<Oscillator>());
                for (int osc = 0; osc < oscillatorCount; osc++)
                {
                    Oscillators[poly].Add(new Oscillator(this, poly));
                    if (addCommonLFO)
                    {
                        Oscillators[poly][Oscillators[poly].Count - 1].AM_Modulator = LFO;
                        Oscillators[poly][Oscillators[poly].Count - 1].FM_Modulator = LFO;
                        Oscillators[poly][Oscillators[poly].Count - 1].XM_Modulator = LFO;
                        Oscillators[poly][Oscillators[poly].Count - 1].AM_Sensitivity = 0.0;
                        Oscillators[poly][Oscillators[poly].Count - 1].FM_Sensitivity = 0.0;
                        Oscillators[poly][Oscillators[poly].Count - 1].XM_Sensitivity = 0.0;
                    }
                }
            }
            Dispatcher = new KeyDispatcher[oscillatorCount];
            for (int i = 0; i < oscillatorCount; i++)
            {
                Dispatcher[i] = new KeyDispatcher(this, polyphony);
            }
            NoteKeys = new NoteKeys(this);
            PitchBend = new double[1]; // -1 -- +1 => -1 octave -- +1 octave
            FrameServer = new FrameServer(this, addCommonLFO);
            EarCompensation = new EarCompensation(this);
        }

        public void Add(int poly, Oscillator oscillator)
        {
            while (poly > Oscillators.Count)
            {
                Oscillators.Add(new List<Oscillator>());
            }
            Oscillators[poly].Add(oscillator);
        }

        public void SetVolume(int volume)
        {
            foreach (List<Oscillator> oscillators in Oscillators)
            {
                oscillators[0].Volume = (byte)volume;
            }
        }

        public void SetSub(int subVolume)
        {
            foreach (List<Oscillator> oscillators in Oscillators)
            {
                oscillators[1].Volume = (byte)subVolume;
            }
        }

        public void SetWaveform(int form, int oscNumber = -1)
        {
            if (oscNumber == -1)
            {
                foreach (List<Oscillator> oscillators in Oscillators)
                {
                    foreach (Oscillator oscillator in oscillators)
                    {
                        oscillator.WaveForm = (WAVEFORM)form;
                    }
                }
            }
            else
            {
                foreach (List<Oscillator> oscillators in Oscillators)
                {
                    oscillators[oscNumber].WaveForm = (WAVEFORM)form;
                }
            }
        }

        public void SetChorus(bool on, int oscNumber = -1)
        {
            if (oscNumber == -1)
            {
                foreach (List<Oscillator> oscillators in Oscillators)
                {
                    foreach (Oscillator oscillator in oscillators)
                    {
                        if (on)
                        {
                            oscillator.SetChorus(0.7, 0.3);
                        }
                        else
                        {
                            oscillator.SetChorus(0.0, 0.0);
                        }
                    }
                }
            }
            else
            {
                foreach (List<Oscillator> oscillators in Oscillators)
                {
                    if (on)
                    {
                        oscillators[oscNumber].SetChorus(0.5, 0.5);
                    }
                    else
                    {
                        oscillators[oscNumber].SetChorus(0.0, 0.0);
                    }
                }
            }
        }

        public void SetVibrato(int vibrato, int oscNumber = -1)
        {
            if (oscNumber == -1)
            {
                foreach (List<Oscillator> oscillators in Oscillators)
                {
                    foreach (Oscillator oscillator in oscillators)
                    {
                        oscillator.FM_Sensitivity = (byte)vibrato / 127.0;
                    }
                }
            }
            else
            {
                foreach (List<Oscillator> oscillators in Oscillators)
                {
                    oscillators[oscNumber].FM_Sensitivity = (byte)vibrato / 127.0;
                }
            }
        }

        public void SetTremolo(int tremolo, int oscNumber = -1)
        {
            if (oscNumber == -1)
            {
                foreach (List<Oscillator> oscillators in Oscillators)
                {
                    foreach (Oscillator oscillator in oscillators)
                    {
                        oscillator.AM_Sensitivity = (byte)tremolo / 127.0;
                    }
                }
            }
            else
            {
                foreach (List<Oscillator> oscillators in Oscillators)
                {
                    oscillators[oscNumber].AM_Sensitivity = (byte)tremolo / 127.0;
                }
            }
        }

        public void SetLfoFrequency(int frequency, int oscNumber = -1)
        {
            if (LFO != null)
            {
                LFO.FrequencyLfo = frequency / 10.0;
                LFO.StepSize = LFO.FrequencyLfo * Math.PI * 2 / SampleRate;
                //if (oscNumber == -1)
                //{
                //    foreach (List<Oscillator> oscillators in Oscillators)
                //    {
                //        foreach (Oscillator oscillator in oscillators)
                //        {
                //            LFO.FrequencyLfo = frequency / 8.0;
                //        }
                //    }
                //}
                //else
                //{
                //    foreach (List<Oscillator> oscillators in Oscillators)
                //    {
                //        LFO.FrequencyLfo = frequency / 8.0;
                //    }
                //}
            }
        }

        public void SetPhase(int phase, int oscNumber = -1)
        {
            if (oscNumber == -1)
            {
                foreach (List<Oscillator> oscillators in Oscillators)
                {
                    foreach (Oscillator oscillator in oscillators)
                    {
                        oscillator.Phase = 3.16 + 3.14 * phase / 129.0;
                    }
                }
            }
            else
            {
                foreach (List<Oscillator> oscillators in Oscillators)
                {
                    oscillators[oscNumber].Phase = 3.16 + 3.14 * phase / 129.0;
                }
            }
        }

        public void SetLfoPhase(int phase, int oscNumber = -1)
        {
            if (oscNumber == -1)
            {
                foreach (List<Oscillator> oscillators in Oscillators)
                {
                    foreach (Oscillator oscillator in oscillators)
                    {
                        oscillator.XM_Sensitivity = phase;
                    }
                }
            }
            else
            {
                foreach (List<Oscillator> oscillators in Oscillators)
                {
                    oscillators[oscNumber].XM_Sensitivity = phase;
                }
            }
        }

        public void SetFilterQ(int value, int oscNumber = -1)
        {
            if (oscNumber == -1)
            {
                foreach (List<Oscillator> oscillators in Oscillators)
                {
                    foreach (Oscillator oscillator in oscillators)
                    {
                        oscillator.Filter.Q = value;
                    }
                }
            }
            else
            {
                foreach (List<Oscillator> oscillators in Oscillators)
                {
                    oscillators[oscNumber].Filter.Q = value;
                }
            }
        }

        public void SetFilterFreq(int value, int oscNumber = -1)
        {
            if (oscNumber == -1)
            {
                foreach (List<Oscillator> oscillators in Oscillators)
                {
                    foreach (Oscillator oscillator in oscillators)
                    {
                        oscillator.Filter.FrequencyCenter = value;
                    }
                }
            }
            else
            {
                foreach (List<Oscillator> oscillators in Oscillators)
                {
                    oscillators[oscNumber].Filter.FrequencyCenter = value;
                }
            }
        }

        public void SetFilterKeyFollow(int value, int oscNumber = -1)
        {
            if (oscNumber == -1)
            {
                foreach (List<Oscillator> oscillators in Oscillators)
                {
                    foreach (Oscillator oscillator in oscillators)
                    {
                        oscillator.Filter.KeyboardFollow = value;
                    }
                }
            }
            else
            {
                foreach (List<Oscillator> oscillators in Oscillators)
                {
                    oscillators[oscNumber].Filter.KeyboardFollow = value;
                }
            }
        }

        public void SetFilterGain(int value, int oscNumber = -1)
        {
            if (oscNumber == -1)
            {
                foreach (List<Oscillator> oscillators in Oscillators)
                {
                    foreach (Oscillator oscillator in oscillators)
                    {
                        oscillator.Filter.Gain = (value - 31);
                    }
                }
            }
            else
            {
                foreach (List<Oscillator> oscillators in Oscillators)
                {
                    oscillators[oscNumber].Filter.Gain = (value - 63);
                }
            }
        }

        public void SetAdsr_A(int value, int oscNumber = -1)
        {
            if (oscNumber == -1)
            {
                foreach (List<Oscillator> oscillators in Oscillators)
                {
                    foreach (Oscillator oscillator in oscillators)
                    {
                        oscillator.Adsr.AdsrAttackTime = (byte)value;
                    }
                }
            }
            else
            {
                foreach (List<Oscillator> oscillators in Oscillators)
                {
                    oscillators[oscNumber].Adsr.AdsrAttackTime = (byte)value;
                }
            }
        }

        public void SetAdsr_D(int value, int oscNumber = -1)
        {
            if (oscNumber == -1)
            {
                foreach (List<Oscillator> oscillators in Oscillators)
                {
                    foreach (Oscillator oscillator in oscillators)
                    {
                        oscillator.Adsr.AdsrDecayTime = (byte)value;
                    }
                }
            }
            else
            {
                foreach (List<Oscillator> oscillators in Oscillators)
                {
                    oscillators[oscNumber].Adsr.AdsrDecayTime = (byte)value;
                }
            }
        }

        public void SetAdsr_S(int value, int oscNumber = -1)
        {
            if (oscNumber == -1)
            {
                foreach (List<Oscillator> oscillators in Oscillators)
                {
                    foreach (Oscillator oscillator in oscillators)
                    {
                        oscillator.Adsr.AdsrSustainLevel = (byte)value;
                    }
                }
            }
            else
            {
                foreach (List<Oscillator> oscillators in Oscillators)
                {
                    oscillators[oscNumber].Adsr.AdsrSustainLevel = (byte)value;
                }
            }
        }

        public void SetAdsr_R(int value, int oscNumber = -1)
        {
            if (oscNumber == -1)
            {
                foreach (List<Oscillator> oscillators in Oscillators)
                {
                    foreach (Oscillator oscillator in oscillators)
                    {
                        oscillator.Adsr.AdsrReleaseTime = (byte)value;
                    }
                }
            }
            else
            {
                foreach (List<Oscillator> oscillators in Oscillators)
                {
                    oscillators[oscNumber].Adsr.AdsrReleaseTime = (byte)value;
                }
            }
        }

        public void SetReverb(int value)
        {
            SetReverbParametersFromSingleSource(value);
        }

        private void SetReverbParametersFromSingleSource(int value)
        {
            FrameServer.reverbEffectDefinition.WetDryMix = value / 4;
            FrameServer.reverbEffectDefinition.ReflectionsDelay = 0;
            FrameServer.reverbEffectDefinition.ReverbDelay = (byte)(value / 64);
            FrameServer.reverbEffectDefinition.RearDelay = (byte)(value / 32);
            FrameServer.reverbEffectDefinition.DecayTime = 1 + value / 4;
        }

        public void SetAdsrPulse(bool on, int oscNumber = -1)
        {
            if (oscNumber == -1)
            {
                foreach (List<Oscillator> oscillators in Oscillators)
                {
                    foreach (Oscillator oscillator in oscillators)
                    {
                        if (on)
                        {
                            oscillator.UseAdsr = true;
                        }
                        else
                        {
                            oscillator.UseAdsr = false;
                        }
                    }
                }
            }
            else
            {
                foreach (List<Oscillator> oscillators in Oscillators)
                {
                    if (on)
                    {
                        oscillators[oscNumber].UseAdsr = true;
                    }
                    else
                    {
                        oscillators[oscNumber].UseAdsr = false;
                    }
                }
            }
        }
    }
}
