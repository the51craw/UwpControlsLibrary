using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UwpControlsLibrary;

namespace SynthesizerSampleapp
{
    public class Folder
    {
        [JsonIgnore]
        public Folder Parent { get; set; }
        public int Id;
        public string Name;
        public List<Folder> FolderList;
        public List<Patch> PatchList;

        public Folder(string name, int id = -1)
        {
            FolderList = new List<Folder>();
            PatchList = new List<Patch>();
            Name = name;
            Id = id;
        }

        public Patch FindFolder(int Id)
        {
            foreach (Folder folder in FolderList)
            {

            }
            return null;
        }
    }

    public class Patch
    {
        [JsonIgnore]
        public Folder Parent { get; set; }
        public PopupMenuButton PopupMenuButton;
        public int Id { get; set; }
        public string Name { get; set; }
        public int Vibrato { get; set; }
        public int Tremolo { get; set; }
        public int Speed { get; set; }
        public int Phase { get; set; }
        public int LfoPhase { get; set; }
        public int Sub { get; set; }
        public int Q { get; set; }
        public int Frequency { get; set; }
        public int KeyFollow { get; set; }
        public int Gain { get; set; }
        public int Attack { get; set; }
        public int Decay { get; set; }
        public int Sustain { get; set; }
        public int Release { get; set; }
        public int Reverb { get; set; }
        public int Waveform { get; set; }
        public int Filter { get; set; }
        public int UseAdsr { get; set; }
        public int Chorus { get; set; }

        public Patch() { }

        public void Read(/*Synthesis synthesis, */MainPage mainPage)
        {
            mainPage.patch = this;
            mainPage.update = MainPage.Update.READ_PATCH;
            //synthesis.SetVibrato(Vibrato);
            //mainPage.slVibrato.Value = Vibrato;
            //synthesis.SetTremolo(Tremolo);
            //mainPage.slTremolo.Value = Tremolo;
            //synthesis.SetLfoFrequency(Speed);
            //mainPage.slLFOFrequency.Value = Speed;
            //synthesis.SetPhase(Phase, 0);
            //mainPage.slPhase.Value = Phase;
            //synthesis.SetLfoPhase(LfoPhase, 0);
            //mainPage.slLfoPhase.Value = LfoPhase;
            //synthesis.SetSub(Sub);
            //mainPage.slSub.Value = Sub;
            //synthesis.SetFilterQ(Q);
            //mainPage.slFilterQ.Value = Q;
            //synthesis.SetFilterFreq(Frequency);
            //mainPage.slFilterFreq.Value = Frequency;
            //synthesis.SetFilterKeyFollow(KeyFollow);
            //mainPage.slFilterKeyFollow.Value = KeyFollow;
            //synthesis.SetFilterGain(Gain);
            //mainPage.slFilterGain.Value = Gain;
            //synthesis.SetAdsr_A(Attack);
            //mainPage.slADSR_A.Value = Attack;
            //synthesis.SetAdsr_D(Decay);
            //mainPage.slADSR_D.Value = Decay;
            //synthesis.SetAdsr_S(Sustain);
            //mainPage.slADSR_S.Value = Sustain;
            //synthesis.SetAdsr_R(Release);
            //mainPage.slADSR_R.Value = Release;
            //mainPage.UpdateAdsrGraph();
            //synthesis.SetReverb(Reverb);
            //mainPage.slReverb.Value = Reverb;
            //synthesis.SetWaveform(Waveform);
            //mainPage.waveform.Selection = Waveform;
            //mainPage.SetFilter(Filter);
            //mainPage.filter.Selection = Filter;
            //synthesis.SetAdsrPulse(AdsrPulse);
            //if (AdsrPulse)
            //{
            //    mainPage.adsrPulse.Selection = 0;
            //}
            //else
            //{
            //    mainPage.adsrPulse.Selection = 1;
            //}
            //synthesis.SetChorus(Chorus);
            //mainPage.ibChorus.IsOn = Chorus;
        }

        public void Save(Patch patch)
        {
            //patch.
        }
    }
}
