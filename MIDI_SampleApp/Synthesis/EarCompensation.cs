﻿namespace UwpControlsLibrary
{
    /// <summary>
    /// Creates values to compensates for the unlinear ear response
    /// which is most notabliced with soft waves like sine or heavy
    /// filtered wave forms.
    /// Uses lines to approximate the ear response.
    /// Gain = k * frequency + base
    /// </summary>

    public class EarCompensation
    {
        Synthesis synthesis
            ;
        public EarCompensation(Synthesis synthesis)
        {
            this.synthesis = synthesis;
        }

        public double KeyToGain(int Key)
        {
            double gain = 1.0;
            double frequency = synthesis.NoteKeys.NoteFrequency[Key];
            if (frequency < 200)
            {
                //gain += mainPage.Settings.LowSineWaveFactor * (200 - frequency) / 200;
            }
            return gain;
        }
    }
}
