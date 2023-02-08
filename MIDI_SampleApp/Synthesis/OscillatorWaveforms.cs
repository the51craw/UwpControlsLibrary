using System;
using System.Reflection.Metadata;
using UwpControlsLibrary;

namespace UwpControlsLibrary
{
    public partial class Oscillator : ControlBase
    {
        /// <summary>
        /// Calls the proper wave generation algorithm, depending on oscillator's waveform
        /// to generate one sample of wave data.
        /// </summary>
        /// <param name="oscillator"></param>
        /// <returns></returns>
        public double MakeWave(Channel channel, OscillatorUsage usage)
        {
            switch (WaveForm)
            {
                case WAVEFORM.SQUARE:
                    return MakeSquareWave(channel, usage);
                case WAVEFORM.SAW_UP:
                    return MakeSawUpWave(channel, usage);
                case WAVEFORM.SAW_DOWN:
                    return MakeSawDownWave(channel, usage);
                case WAVEFORM.TRIANGLE:
                    return MakeTriangleWave(channel, usage);
                case WAVEFORM.SINE:
                    return MakeSineWave(channel, usage);
                case WAVEFORM.RANDOM:
                    return MakeRandomWave();
                case WAVEFORM.NOISE:
                    return MakeNoiseWave();
                default:
                    return 0.0f;
            }
        }

        /// <summary>
        /// Algorithm for generating a squarewave sample depending on the angle in Radians.
        /// </summary>
        /// <param name="oscillator"></param>
        /// <returns>One sample of wave data</returns>
        public double MakeSquareWave(Channel channel, OscillatorUsage usage)
        {
            double angle = channel == Channel.LEFT ? AngleLeft : AngleRight;
            double sample;
            if (usage == OscillatorUsage.OUTPUT)
            {
                if (angle > Phase)
                {
                    sample = -1.0f;
                }
                else
                {
                    sample = 1.0f;
                }
            }
            else if (usage == OscillatorUsage.FM_PLUS_MINUS)
            {
                if (angle > Phase)
                {
                    sample = -0.5f;
                }
                else
                {
                    sample = 1.0f;
                }
            }
            else if (usage == OscillatorUsage.FM)
            {
                if (angle > Phase)
                {
                    sample = 0.0f;
                }
                else
                {
                    sample = 1.0f;
                }
            }
            else
            {
                if (angle > Phase) // Usage.MODULATION
                {
                    sample = 1.0f;
                }
                else
                {
                    sample = 0.0f;
                }
            }
            return sample;
        }

        /// <summary>
        /// Algorithm for generating a sawtooth down wave sample depending on the angle in Radians.
        /// </summary>
        /// <param name="oscillator"></param>
        /// <returns>One sample of wave data</returns>
        public double MakeSawDownWave(Channel channel, OscillatorUsage usage)
        {
            double angle = channel == Channel.LEFT ? AngleLeft : AngleRight;
            if (usage == OscillatorUsage.OUTPUT)
            {
                //angle += Phase;
                if (angle > 2 * Math.PI)
                {
                    angle -= 2 * Math.PI;
                }
                return (double)((1.0 - angle / Math.PI));
            }
            else if (usage == OscillatorUsage.FM_PLUS_MINUS)
            {
                return (double)(1.0 - 1.5 * (angle / Math.PI / 2));
                //return (double)((1.0 - (angle / Math.PI)) + 1) / 2;
            }
            else if (usage == OscillatorUsage.FM)
            {
                //return (double)(1.0 - 1.5 * (angle / Math.PI / 2));
                return (double)((1.0 - (angle / Math.PI)) + 1) / 2;
            }
            else // OscillatorUsage.AM:
            {
                return (double)(angle / Math.PI / 2);
            }
        }


        /// <summary>
        /// Algorithm for generating a sawtooth up wave sample depending on the angle in Radians.
        /// </summary>
        /// <param name="oscillator"></param>
        /// <returns>One sample of wave data</returns>
        public double MakeSawUpWave(Channel channel, OscillatorUsage usage)
        {
            double angle = channel == Channel.LEFT ? AngleLeft : AngleRight;
            if (usage == OscillatorUsage.OUTPUT)
            {
                //angle += Phase;
                if (angle > 2 * Math.PI)
                {
                    angle -= 2 * Math.PI;
                }
                return (double)((angle / Math.PI) - 1.0);
            }
            else if (usage == OscillatorUsage.FM_PLUS_MINUS)
            {
                return (double)(1.5 * (angle / Math.PI / 2) - 0.5);
                //return (double)((angle / Math.PI)) / 2;
            }
            else if (usage == OscillatorUsage.FM)
            {
                //return (double)(1.5 * (angle / Math.PI / 2) - 0.5);
                return (double)((angle / Math.PI)) / 2;
            }
            else // OscillatorUsage.AM:
            {
                return (double)(1.0 - angle / Math.PI / 2);
            }


            //else
            //{
            //    if (angle > 2 * Math.PI)
            //    {
            //        angle -= 2 * Math.PI;
            //    }
            //    return (double)((1 - (angle / Math.PI)) + 1) / 2;
            //}
        }
        /// <summary>
        /// Algorithm for generating a triangle wave sample depending on the angle in Radians.
        /// </summary>
        /// <param name="oscillator"></param>
        /// <returns>One sample of wave data</returns>
        public double MakeTriangleWave(Channel channel, OscillatorUsage usage)
        {
            // +1__________________________ In order to sync to the other waveforms, triangle
            //   /\            /\           needs to start at zero. Drawing up during first 
            //  /  \          /  \          half period, and down during second half will off-
            // /    \        /    \         set 1/2 PI. Instead, we use the schema below:
            // 0 --------------------------
            //        \    /        \     / 0 - PI/2 -> from zero to Amplitude
            //         \  /          \   /  PI/2 - PI -> from Amplitude to zero
            //   _______\/____________\ /__ PI - 3PI/2 -> from zero to -Amplitude
            // -1                           3PI/2 - 2PI -> from -Amplitude to zero

            double angle = channel == Channel.LEFT ? AngleLeft + Phase : AngleRight + Phase;
            double sample;
            if (usage == OscillatorUsage.FM)
            {
                angle += Math.PI / 2;
                if (angle > Math.PI * 2)
                {
                    angle -= Math.PI * 2;
                }
            }
            else if (usage == OscillatorUsage.FM_PLUS_MINUS)
            {
                angle += Math.PI;
                if (angle > Math.PI * 2)
                {
                    angle -= Math.PI * 2;
                }
            }
            if (angle > Math.PI * 2)
            {
                angle -= Math.PI * 2;
            }

            if (angle < Math.PI / 2)
            {
                sample = (double)(angle / (Math.PI / 2));
            }
            else if (angle < Math.PI)
            {
                sample = (double)(1 - ((angle / (Math.PI / 2)) - 1));
            }
            else if (angle < 3 * Math.PI / 2)
            {
                sample = (double)(0 - (4 * (angle - Math.PI) / Math.PI / 2));
            }
            else
            {
                sample = (double)((4 * (angle - 3 * Math.PI / 2) / Math.PI / 2) - 1);
            }
            sample = AdjustRangeForUsage(sample, usage);
            //if (usage == Usage.MODULATION)
            //{
            //    sample = (1 + sample) / 2;
            //}
            //else if (usage == Usage.FM)
            //{
            //    sample = (1 + sample) / 2;
            //}
            //else if (usage == Usage.FM_PLUS_MINUS)
            //{
            //    // Change range -1 -- +1 to -.5 -- +1:
            //    sample = (3.0 * sample / 4.0) + 1.0 / 4.0;
            //}
            return sample;
        }

        /// <summary>
        /// Algorithm for generating a sine wave sample depending on the angle in Radians.
        /// </summary>
        /// <param name="oscillator"></param>
        /// <returns>One sample of wave data</returns>
        public double MakeSineWave(Channel channel, OscillatorUsage usage)
        {
            double angle = channel == Channel.LEFT ? AngleLeft + Phase : AngleRight + Phase;
            double sample;
            if (usage == OscillatorUsage.FM)
            {
                angle += Math.PI / 2;
                if (angle > Math.PI * 2)
                {
                    angle -= Math.PI * 2;
                }
            }
            else if (usage == OscillatorUsage.FM_PLUS_MINUS)
            {
                angle += Math.PI;
                if (angle > Math.PI * 2)
                {
                    angle -= Math.PI * 2;
                }
            }
            sample = (double)Math.Sin(angle);
            sample = AdjustRangeForUsage(sample, usage);
            //if (usage == Usage.FM)
            //{
            //    sample = (1 + sample) / 2;
            //}
            //else if (usage == Usage.FM_PLUS_MINUS)
            //{
            //    // Change range -1 -- +1 to -.5 -- +1:
            //    sample = (3.0 * sample / 4.0) + 1.0 / 4.0;
            //}
            ////if (usage == Usage.MODULATION)
            ////{
            ////    angle -= Math.PI / 2;
            ////    if (angle < 0)
            ////    {
            ////        angle += Math.PI * 2;
            ////    }
            ////}
            ////sample = (double)Math.Sin(angle);
            ////if (usage == Usage.MODULATION)
            ////{
            ////    sample = 0.5 + sample / 2;
            ////}
            return sample;
        }

        public double MakeRandomWave()
        {
            return randomValue;
        }

        public double MakeNoiseWave()
        {
            // Noise is not to be translated to a certain frequency!
            return 0.001 * (random.Next(1000) - 500);
            //mean += CurrentSample;
        }

        private double AdjustRangeForUsage(double sample, OscillatorUsage usage)
        {
            if (usage == OscillatorUsage.MODULATION)
            {
                sample = (1 + sample) / 2;
            }
            else if (usage == OscillatorUsage.FM)
            {
                sample = (1 + sample) / 2;
            }
            else if (usage == OscillatorUsage.FM_PLUS_MINUS)
            {
                // Change range -1 -- +1 to -.5 -- +1:
                sample = (3.0 * sample / 4.0) + 1.0 / 4.0;
            }
            return sample;
        }

        ///// <summary>
        ///// Moves the angle in Radians one StepSize forward. Backs up 2 * PI when
        ///// Radians exeeds 2 * PI. 
        ///// </summary>
        ///// <param name="oscillator"></param>
        ///// <returns>true if Radians backed up, else false (used when generating random waveform to detect when to generate a new sample</returns>
        //public void AdvanceAngle(int i)
        //{
        //    bool left = i % 2 == 0;
        //    if (left)
        //    {
        //        AngleLeft += StepSize + chorusOffset;
        //        if (AngleLeft >= Math.PI * 2)
        //        {
        //            AngleLeft -= Math.PI * 2;
        //        }
        //        if (AngleLeft < 0)
        //        {
        //            AngleLeft += Math.PI * 2;
        //        }
        //    }
        //    else
        //    {
        //        AngleRight += StepSize - chorusOffset;
        //        if (AngleRight >= Math.PI * 2)
        //        {
        //            AngleRight -= Math.PI * 2;
        //        }
        //        if (AngleRight < 0)
        //        {
        //            AngleRight += Math.PI * 2;
        //        }
        //    }
        //}
    }
}
