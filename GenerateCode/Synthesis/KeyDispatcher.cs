using System;

namespace SynthLab
{
    /**
     * Polyphony:
     * Synthlab can play up to 6 tones at the same time, including tones in release!
     * Prioritizer keeps track of which oscillators are available
     * for playing next key when multiple keys are pressed.
     * 
     * KeyOn:
     *  Key[poly] = key#
     *  IsPressed[poly] = true
     *  
     * KeyOff:
     *  if !PedalHold:
     *      poly 0 -> 9
     *          adsr release
     *  else
     *      adsr release
     *      
     * PedalHold off:
     *      poly 0 -> 9
     *          if not IsPressed[poly]
     *              adsr release
     */
    public class KeyDispatcher
    {
        public Boolean PedalHold { get { return pedalHold; } set { SetPedalHold(value); } }
        public int KeyPriority = 0;
        //public int Polyphony = 6;
        private MainPage mainPage;

        /// <summary>
        /// Index = poly-number, values are the keys assinged to the polys/oscillators.
        /// </summary>
        public int[] Key;

        /// <summary>
        /// Keys that were not released before pedalHold goes off are marked here to stay alive.
        /// </summary>
        public Boolean[] IsPressed;

        /// <summary>
        /// Since key-off only hands the responsibility over to ADSR that knows not of
        /// channels, the dispatcher keeps track of channel.
        /// </summary>
        //public int[] Channel;

        public int[] KeyOrder;

        private Boolean pedalHold = false;

        public KeyDispatcher(MainPage mainPage)
        {
            this.mainPage = mainPage;
            Key = new int[6];
            IsPressed = new bool[6];
            pedalHold = false;
            Clear();
            KeyOrder = new int[6];
            for (int i = 0; i < 6; i++)
            {
                KeyOrder[i] = -1;
            }
        }

        public bool AnyKeyInUse(MainPage mainPage)
        {
            int count = 0;
            for (int poly = 0; poly < 6; poly++)
            {
                count += NumberOfOscillatorsInUse();
            }
            return count > 0;
        }

        public static bool AnyOscillatorInUse(MainPage mainPage)
        {
            for (int osc = 0; osc < synthesis.Oscillators[0].Count; osc++)
            {
                if (mainPage.Oscillators[0][osc].IsOn)
                {
                    return true;
                }
            }
            return false;
        }

        public void SetPedalHold(Boolean value)
        {
            pedalHold = value;
            if (!pedalHold)
            {
                for (int poly = 0; poly < 6; poly++)
                {
                    if (!IsPressed[poly])
                    {
                        for (int osc = 0; osc < synthesis.Oscillators[0].Count; osc++)
                        {
                            mainPage.KeyOff((byte)Key[poly], mainPage.Oscillators[poly][osc].MidiChannel);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// If there is a free poly it will be returned, else -1.
        /// Also sets Key, Channel and IsPressed for the poly.
        /// </summary>
        /// <param name="Key"></param>
        /// <param name="Channel"></param>
        /// <returns></returns>
        public int TryAssignPoly(int Key)
        {
            if (NumberOfOscillatorsInUse() >= 6)
            {
                if (KeyPriority > 0)
                {
                    int freePoly = -1;

                    // All polys are taken. Find the oldest one:
                    for (int i = 0; i < 6; i++)
                    {
                        if (KeyOrder[i] == 0)
                        {
                            // Mark it as the newest one:
                            KeyOrder[i] = 6;
                            // Remember which poly to use:
                            freePoly = i;
                        }
                        // Re-number the order to still be 0 - 6 - 1:
                        KeyOrder[i]--;
                    }

                    if (pedalHold)
                    {
                        // Remember that this key was pressed while pedal was down:
                        IsPressed[freePoly] = true;
                    }
                    if (freePoly > -1)
                    {
                        this.Key[freePoly] = Key;
                    }
                    return freePoly;
                }
                else
                {
                    return -1;
                }
            }
            else
            {
                // Since not all polys are in use, there must be one free:
                for (int poly = 0; poly < 6; poly++)
                {
                    if (this.Key[poly] == -1)
                    {
                        // Find the KeyOrder that is newest and make this one newer:
                        int newest = -1;
                        for (int i = 0; i < 6; i++)
                        {
                            if (KeyOrder[i] > newest)
                            {
                                newest = KeyOrder[i];
                            }
                        }
                        KeyOrder[poly] = newest + 1;

                        this.Key[poly] = Key;
                        if (pedalHold)
                        {
                            // Remember that this key was pressed while pedal was down:
                            IsPressed[poly] = true;
                        }
                        return poly;
                    }
                }
                return -1;
            }
        }

        public int TryGetPolyFromKey(int Key)
        {
            for (int poly = 0; poly < 6; poly++)
            {
                if (this.Key[poly] == Key)
                {
                    return poly;
                }
            }
            return -1;
        }

        public Boolean PolyIsPlaying(int poly)
        {
            return Key[poly] > -1;
        }

        public void ReleaseOscillator(int Key)
        {
            for (int poly = 0; poly < 6; poly++)
            {
                if (this.Key[poly] == Key)
                {
                    KeyOrder[poly] = -1;
                    this.Key[poly] = -1;
                    if (pedalHold)
                    {
                        IsPressed[poly] = false;
                    }
                }
            }
        }

        public void ReleasePoly(int poly)
        {
            KeyOrder[poly] = -1;
            Key[poly] = -1;
            if (pedalHold)
            {
                IsPressed[poly] = false;
            }
        }

        public int RePress(int Key)
        {
            int foundPoly = -1;
            for (int poly = 0; poly < 6; poly++)
            {
                if (this.Key[poly] == Key)
                {
                    foundPoly = poly;
                    break;
                }
            }

            return foundPoly;
        }

        public Boolean KeyIsPlaying(int Key)
        {
            for (int poly = 0; poly < 6; poly++)
            {
                if (this.Key[poly] == Key)
                {
                    return true;
                }
            }
            return false;
        }


        public void Clear()
        {
            for (int poly = 0; poly < 6; poly++)
            {
                Key[poly] = -1;
                IsPressed[poly] = false;
            }
        }

        public int NumberOfOscillatorsInUse()
        {
            int count = 0;
            for (int poly = 0; poly < 6; poly++)
            {
                if (Key[poly] > -1)
                {
                    count++;
                }
            }
            return count;
        }
    }
}
