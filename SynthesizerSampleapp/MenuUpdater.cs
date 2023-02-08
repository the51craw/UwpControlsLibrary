using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UwpControlsLibrary;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace SynthesizerSampleapp
{
    public sealed partial class MainPage : Page
    {
        public DispatcherTimer menuUpdater;
        public enum Update
        {
            NONE,
            READ_PATCH,
            WRITE_PATCH,
        }
        public Update update;
        public Patch patch;

        public void InitMenuUpdater()
        {
            menuUpdater = new DispatcherTimer();
            menuUpdater.Interval = new TimeSpan(0, 0, 0, 0, 100);
            menuUpdater.Tick += MenuUpdater_Tick;
            menuUpdater.Start();
        }

        private void MenuUpdater_Tick(object sender, object e)
        {
            if(update == Update.READ_PATCH)
            {
                update = Update.NONE;
                synthesis.SetVibrato(patch.Vibrato);
                slVibrato.Value = patch.Vibrato;
                synthesis.SetTremolo(patch.Tremolo);
                slTremolo.Value = patch.Tremolo;
                synthesis.SetLfoFrequency(patch.Speed);
                slLFOFrequency.Value = patch.Speed;
                synthesis.SetPhase(patch.Phase, 0);
                slPhase.Value = patch.Phase;
                synthesis.SetLfoPhase(patch.LfoPhase, 0);
                slLfoPhase.Value = patch.LfoPhase;
                synthesis.SetSub(patch.Sub);
                slSub.Value = patch.Sub;
                synthesis.SetFilterQ(patch.Q);
                slFilterQ.Value = patch.Q;
                synthesis.SetFilterFreq(patch.Frequency);
                slFilterFreq.Value = patch.Frequency;
                synthesis.SetFilterKeyFollow(patch.KeyFollow);
                slFilterKeyFollow.Value = patch.KeyFollow;
                synthesis.SetFilterGain(patch.Gain);
                slFilterGain.Value = patch.Gain;
                synthesis.SetAdsr_A(patch.Attack);
                slADSR_A.Value = patch.Attack;
                synthesis.SetAdsr_D(patch.Decay);
                slADSR_D.Value = patch.Decay;
                synthesis.SetAdsr_S(patch.Sustain);
                slADSR_S.Value = patch.Sustain;
                synthesis.SetAdsr_R(patch.Release);
                slADSR_R.Value = patch.Release;
                UpdateAdsrGraph();
                synthesis.SetReverb(patch.Reverb);
                slReverb.Value = patch.Reverb;
                synthesis.SetWaveform(patch.Waveform);
                waveform.Selection = patch.Waveform;
                SetFilter(patch.Filter);
                filter.Selection = patch.Filter;
                synthesis.SetAdsrPulse(patch.UseAdsr > 0);
                if (patch.UseAdsr == 0)
                {
                    useAdsr.Selection = 0;
                }
                else
                {
                    useAdsr.Selection = 1;
                }
                synthesis.SetChorus(patch.Chorus > 0);
                ibChorus.IsOn = patch.Chorus > 0;
            }
            else if (update == Update.WRITE_PATCH)
            {
                patch.Vibrato = slVibrato.Value;
                patch.Tremolo = slTremolo.Value;
                patch.Speed = slLFOFrequency.Value;
                patch.Phase = slPhase.Value;
                patch.LfoPhase = slLfoPhase.Value;
                patch.Sub = slSub.Value;
                patch.Q = slFilterQ.Value;
                patch.Frequency = slFilterFreq.Value;
                patch.KeyFollow = slFilterKeyFollow.Value;
                patch.Gain = slFilterGain.Value;
                patch.Attack = slADSR_A.Value;
                patch.Decay = slADSR_D.Value;
                patch.Sustain = slADSR_S.Value;
                patch.Release = slADSR_R.Value;
                patch.Reverb = slReverb.Value;
                patch.Waveform = waveform.Selection;
                patch.Filter = filter.Selection;
                //patch.UseAdsr = useAdsr.Selection;
                //patch.Chorus = ibChorus.IsOn ? 1 : 0;
                //ibChorus.IsOn = patch.Chorus > 0;
                update = Update.NONE;
            }
        }
    }
}
