using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Globalization.Fonts;
using Windows.Media;
using Windows.Media.Audio;
using Windows.UI.Xaml.Controls;

namespace UwpControlsLibrary
{
    public enum CurrentActivity
    {
        NONE,
        GENERATING_WAVE_SHAPE,
        CHANGING_PARAMETERS,
    }

    public enum LfoAdvance
    {
        NONE,
        ADVANCE,
        ADVANCED,
    }

    //public sealed partial class Synthesis
    //{
    //    //public int adjust = 0;
    //    public double[] PitchBend; // -1 -- +1 => -1 octave -- +1 octave
    //    public Boolean allowGuiUpdates = false;
    //    public Boolean allowUpdateOscilloscope = false;
    //    public FrameServer FrameServer;
    //    public CURRENTACTIVITY CurrentActivity = CURRENTACTIVITY.NONE;
    //}
    public partial class FrameServer
    {
        Synthesis synthesis;
        float[] oscilloscopeData;
        LfoAdvance LfoAdvance;

        /// <summary>
        /// The frame server's FrameInputNode delivers the audio data to the AudioGraph system.
        /// </summary>
        public AudioFrameInputNode FrameInputNode;

        private AudioFrame frame;

        public FrameServer(Synthesis synthesis, bool addCommonLFO = false)
        {
            this.synthesis = synthesis;
            if (addCommonLFO)
            {
                LfoAdvance = LfoAdvance.ADVANCE;
            }
            else
            {
                LfoAdvance = LfoAdvance.NONE;
            }

            // Create pitch bend values for all MIDI channels:
            this.synthesis.PitchBend = new double[16];
            for (int i = 0; i < 16; i++)
            {
                this.synthesis.PitchBend[i] = 1;
            }
            InitAudio();
        }

        public async void InitAudio()
        {
            await InitAudioAsync();
        }

        private async Task<bool> InitAudioAsync()
        { 
            if (await CreateAudioGraph())
            {
                CreateFrameInputNode();
                StartAudioGraph();
                return true;
            }
            return false;
        }

        /// <summary>
        /// Assembles a frame from all actively sounding oscillators
        /// in all used polyphony layer (for all pressed keys).
        /// </summary>
        public unsafe void FrameInputNode_QuantumStarted(AudioFrameInputNode sender, FrameInputNodeQuantumStartedEventArgs args)
        {
            synthesis.CurrentActivity = CurrentActivity.GENERATING_WAVE_SHAPE;

            if (LfoAdvance != LfoAdvance.NONE)
            {
                if (synthesis.LFO != null && synthesis.LFO.WaveData == null)
                {
                    synthesis.LFO.WaveData = new double[synthesis.SampleCount * 2];
                }
            }

            if (synthesis.SampleCount != 0)
            {
                // Create frame:
                frame = new AudioFrame(2 * (uint)synthesis.SampleCount * sizeof(float)); // Times 2 because it is stereo.
                oscilloscopeData = new float[2 * synthesis.SampleCount];

                using (AudioBuffer buffer = frame.LockBuffer(AudioBufferAccessMode.Write))
                using (IMemoryBufferReference reference = buffer.CreateReference())
                {
                    byte* inBytes;
                    uint capacity;
                    float* bufferPointer;

                    // Get the AudioFrame buffer
                    ((IMemoryBufferByteAccess)reference).GetBuffer(out inBytes, out capacity);

                    // Make a float pointer for inserting wave data into buffer:
                    bufferPointer = (float*)inBytes;

                    // Since FrameSever adds all waveforms from the PolyServer
                    // together, we must first sett all samples to zero:
                    for (int i = 0; i < synthesis.SampleCount * 2; i++)
                    {
                        bufferPointer[i] = 0;
                        oscilloscopeData[i] = 0;
                    }

                    // Generate and add together audio data for all channels and pressed keys:
                    for (int osc = 0; osc < synthesis.Oscillators[0].Count; osc++)
                    {
                        // Check if this oscillator needs to use WaveShape:
                        //synthesis.Oscillators[0][osc].WaveShape.SetNeedsToBeUsed(synthesis.Oscillators[0][osc]);
                        if (LfoAdvance != LfoAdvance.NONE)
                        {
                            LfoAdvance = LfoAdvance.ADVANCE;
                        }
                        for (int poly = 0; poly < synthesis.Oscillators.Count; poly++)
                        {
                            if (synthesis.Dispatcher[osc].PolyIsPlaying(poly)) // IsPressed[poly])
                            {
                                synthesis.Oscillators[poly][osc].GenerateAudioData((uint)synthesis.SampleCount, LfoAdvance);
                                if (LfoAdvance != LfoAdvance.NONE)
                                {
                                    LfoAdvance = LfoAdvance.ADVANCED;
                                }
                                for (int i = 0; i < synthesis.SampleCount * 2; i++)
                                {
                                    bufferPointer[i] += (float)synthesis.Oscillators[poly][osc].WaveData[i];
                                    oscilloscopeData[i] += (float)synthesis.Oscillators[poly][osc].WaveData[i];
                                }
                            }
                        }
                    }
                }
                FrameInputNode.AddFrame(frame);
                synthesis.CurrentActivity = CurrentActivity.NONE;
            }
        }
    }
}
