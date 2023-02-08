﻿using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.Timers;
using UwpControlsLibrary;
using Windows.Foundation;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;

namespace UwpControlsLibrary
{
    public class ADSR
    {
        ////////////////////////////////////////////////////////////////////////////////////////////////
        // ADSR
        ////////////////////////////////////////////////////////////////////////////////////////////////

        public enum AdsrStates
        {
            NONE,
            ATTACK,
            DECAY,
            SUSTAIN,
            RELEASE,
            RELEASE_END
        }

        //-----------------------------------------------------------------------------------------------------
        // ADSR properties
        //-----------------------------------------------------------------------------------------------------

        public AdsrStates AdsrState;
        public byte AdsrAttackTime;   // 0 - 127 = 0 - 3 seconds
        public byte AdsrDecayTime;    // 0 - 127 = 0 - 12 seconds
        public byte AdsrSustainLevel { get { return (byte)(sustainLevel * 128); } set { sustainLevel = (float)value / (float)128; } } // between 0  and 1
        public byte AdsrReleaseTime;  // 0 - 127 = 0 - 12 seconds
        public bool AdsrAmSensitive;
        public bool AdsrFmSensitive;
        public bool AdsrXmSensitive;

        public double AdsrLevel; // 0 - 1 Compensated for ears being non-linear in audio level experience.
        public double AdsrTime;
        public Boolean PedalDown;
        public Knob knobA;
        public Knob knobD;
        public Knob knobS;
        public Knob knobR;
        public Rotator pedal;
        public int AdsrModulationWheelTarget;
        public Rect AdsrBounds;
        private byte key;
        private double adsrTimeStep = 0.167F;
        public int channel;
        private Synthesis synthesis;
        private int Id;

        public ADSR(Synthesis synthesis)
        {
            this.synthesis = synthesis;
            if (synthesis.Oscillators != null )
            {
                Id = synthesis.Oscillators[synthesis.Oscillators.Count - 1].Count;
            }
            else
            {
                Id = -1;
            }
            AdsrAttackTime = 0;
            AdsrDecayTime = 0;
            AdsrSustainLevel = 127;
        }

        //-----------------------------------------------------------------------------------------------------
        // ADSR private
        //-----------------------------------------------------------------------------------------------------
        private double sustainLevel;
        double adsrReleasedAtLevel = 1f;    // If key is down after full attack, this value is 1, 
                                     // but if the key is released during attack or decay, 
                                     // it is lower. This is the level to start release from.

        ////////////////////////////////////////////////////////////////////////////////////////////////
        // ADSR funtions
        ////////////////////////////////////////////////////////////////////////////////////////////////

        public void Advance()
        {
            double progress;

            switch (AdsrState)
            {
                case AdsrStates.ATTACK:
                    if (AdsrTime <= AdsrAttackTime)
                    {
                        // During attack time, increment and update peak value:
                        AdsrTime += adsrTimeStep; // 0.167F;
                        AdsrLevel = (float)(AdsrTime / AdsrAttackTime);
                    }
                    else
                    {
                        // Attack time is over:
                        if (AdsrDecayTime > 0)
                        {
                            // Decay time > 0 => go to decay:
                            AdsrTime = AdsrDecayTime;
                            AdsrState = AdsrStates.DECAY;
                        }
                        else
                        {
                            AdsrState = AdsrStates.SUSTAIN;
                            AdsrTime = 0;
                        }
                    }
                    // Keep track of level in case key is released during a slope (attack or decay):
                    adsrReleasedAtLevel = AdsrLevel;
                    break;
                case AdsrStates.DECAY:
                    if (AdsrTime > 0)
                    {
                        // Decay in progress:
                        // AdsrTime goes from AdsrDecayTime to 0.
                        AdsrLevel = AdsrTime / AdsrDecayTime;
                        AdsrTime -= adsrTimeStep;
                        if (AdsrLevel < AdsrSustainLevel / 128.0)
                        {
                            AdsrLevel = AdsrSustainLevel / 128.0;
                        }
                    }
                    else
                    {
                        // Decay time ended:
                        // Jump down to sustain level without delay:
                        AdsrLevel = AdsrSustainLevel / 127f;
                        AdsrState = AdsrStates.SUSTAIN;
                        adsrReleasedAtLevel = AdsrLevel;
                    }
                    break;
                case AdsrStates.SUSTAIN:
                    AdsrLevel = AdsrSustainLevel / 128f;
                    AdsrTime = 0;
                    break;
                case AdsrStates.RELEASE:
                    progress = AdsrTime / (double)AdsrReleaseTime;
                    AdsrTime += adsrTimeStep; // 0.5167F;
                    AdsrLevel = adsrReleasedAtLevel - adsrReleasedAtLevel * progress;
                    if (AdsrLevel < 0.00001) //(1f / 128f))
                    {
                        AdsrState = AdsrStates.RELEASE_END;
                        AdsrLevel = 0;
                    }
                    break;
                case AdsrStates.RELEASE_END:
                    AdsrState = AdsrStates.NONE;
                    synthesis.Dispatcher[Id].ReleaseOscillator(key);
                    break;
            }
        }

        public void AdsrStart(byte key, bool useAdsr)
        {
            this.key = key;
            AdsrTime = 0;

            if (useAdsr)
            {
                if (AdsrAttackTime > 0)
                {
                    AdsrState = AdsrStates.ATTACK;
                    AdsrLevel = 0;
                }
                else
                {
                    adsrReleasedAtLevel = 1;
                    if (AdsrDecayTime > 0)
                    {
                        AdsrState = AdsrStates.DECAY;
                        AdsrTime = AdsrDecayTime;
                    }
                    else
                    {
                        AdsrLevel = AdsrSustainLevel / 127f;
                        AdsrState = AdsrStates.SUSTAIN;
                    }
                }
            }
            else
            {
                adsrReleasedAtLevel = 1;
                AdsrLevel = AdsrSustainLevel / 127f;
                AdsrState = AdsrStates.SUSTAIN;
            }

        }

        public void AdsrRestart(bool useAdsr)
        {
            AdsrTime = 0;
            AdsrStart(key, useAdsr);
        }

        public void AdsrRelease(bool useAdsr)
        {
            if (AdsrState == ADSR.AdsrStates.NONE)
            {
                AdsrStop();
            }
            else if (useAdsr)
            {
                adsrReleasedAtLevel = AdsrLevel;
                AdsrReleaseTime = (byte)(AdsrReleaseTime < 2 ? 2 : AdsrReleaseTime);
                AdsrState = AdsrStates.RELEASE;
            }
            else
            {
                adsrReleasedAtLevel = 1;
                AdsrState = AdsrStates.RELEASE;
            }
            AdsrTime = 0;
        }

        public void AdsrStop()
        {
            AdsrState = AdsrStates.RELEASE_END;
            AdsrLevel = 0;
            synthesis.Dispatcher[Id].ReleaseOscillator(key);
        }
    }
}
