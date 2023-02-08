using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;
using UwpControlsLibrary;

namespace UwpControlsLibrary
{
    public partial class Oscillator : ControlBase
    {
        private double MakeFrequencyModulatedWave(Oscillator oscillator, int channel)
        {
            double angle = channel % 2 == 0 ? oscillator.AngleLeft : oscillator.AngleRight;
            double result = 0;

            //if (oscillator.XM_Modulator != null && oscillator.XM_Modulator.WaveForm == WAVEFORM.SINE && oscillator.XM_Modulator.Keyboard)
            //{
            //    result += Math.Sin(angle
            //        + Get_XM_Sensitivity() * ModulationVelocitySensitivity / 64f * MakeFrequencyModulatedWave(oscillator.XM_Modulator, channel));
            //}
            //else
            //{
            //    result += Math.Sin(angle);
            //}
            return result;
        }
    }
}
