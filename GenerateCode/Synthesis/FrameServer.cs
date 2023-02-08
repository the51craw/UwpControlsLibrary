using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Media;
using Windows.Media.Audio;
using Windows.UI.Xaml.Controls;

namespace SynthLab
{
    public enum CURRENTACTIVITY
    {
        NONE,
        GENERATING_WAVE_SHAPE,
        CHANGING_PARAMETERS,
    }

    public sealed partial class MainPage : Page
    {
        //public int adjust = 0;
        public double[] PitchBend; // -1 -- +1 => -1 octave -- +1 octave
        public Boolean allowGuiUpdates = false;
        public Boolean allowUpdateOscilloscope = false;
        public FrameServer FrameServer;
        public CURRENTACTIVITY CurrentActivity = CURRENTACTIVITY.NONE;
    }
    public partial class FrameServer
    {
        Synthesis synthesis;
        float[] oscilloscopeData;

        /// <summary>
        /// The frame server's FrameInputNode delivers the audio data to the AudioGraph system.
        /// </summary>
        public AudioFrameInputNode FrameInputNode;

        private AudioFrame frame;

        public FrameServer(Synthesis synthesis)
        {
            this.synthesis = synthesis;

            // Create pitch bend values for all MIDI channels:
            this.synthesis.PitchBend = new double[16];
            for (int i = 0; i < 16; i++)
            {
                this.synthesis.PitchBend[i] = 1;
            }
        }

        public async Task<bool> InitAudio()
        {
            if (await CreateAudioGraph())
            {
                CreateFrameInputNode();
                return true;
            }
            return false;
        }

        public void StartAudio()
        {
            StartAudioGraph();
        }

        /// <summary>
        /// Assembles a frame from all actively sounding oscillators
        /// in all used polyphony layer (for all pressed keys).
        /// Rather than waiting for a request form AudioGraph we create a frame while waiting.
        /// After creating the new frame we need to know weather to send it or to wait for
        /// next request. 
        /// </summary>
        public unsafe void FrameInputNode_QuantumStarted(AudioFrameInputNode sender, FrameInputNodeQuantumStartedEventArgs args)
        {
            synthesis.CurrentActivity = CURRENTACTIVITY.GENERATING_WAVE_SHAPE;
            uint requestedNumberOfSamples = (uint)args.RequiredSamples;
            synthesis.SampleCount = requestedNumberOfSamples;

            if (synthesis.initDone && requestedNumberOfSamples != 0)
            {
                // If there is already a pre-created frame, hand it over to AudioGraph:
                if (frame != null)
                {
                    FrameInputNode.AddFrame(frame);
                    synthesis.allowUpdateOscilloscope = true;
                    synthesis.oscilloscope.AddWaveData(oscilloscopeData, requestedNumberOfSamples);
                }

                // Then create next frame rather than waiting for AudioGraph to request it:
                frame = new AudioFrame(2 * requestedNumberOfSamples * sizeof(float)); // Times 2 because it is stereo.
                oscilloscopeData = new float[2 * requestedNumberOfSamples];

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
                    for (int i = 0; i < requestedNumberOfSamples * 2; i++)
                    {
                        bufferPointer[i] = 0;
                        oscilloscopeData[i] = 0;
                    }

                    // Generate and add together audio data for all channels and pressed keys:
                    for (int osc = 0; osc < synthesis.Oscillators[0].Count; osc++)
                    {
                        // Check if this oscillator needs to use WaveShape:
                        //mainPage.Oscillators[0][osc].WaveShape.SetNeedsToBeUsed(mainPage.Oscillators[0][osc]);
                        for (int poly = 0; poly < 6; poly++)
                        {
                            if (synthesis.dispatcher[osc].PolyIsPlaying(poly)) // IsPressed[poly])
                            {
                                //mainPage.Oscillators[poly][osc].WaveShape.SetNeedsToBeUsed(mainPage.Oscillators[0][osc].WaveShape.NeedsToBeUsed);
                                //PolyServers[poly].GenerateAudioData(poly, osc, requestedNumberOfSamples);
                                synthesis.Oscillators[poly][osc].GenerateAudioData(requestedNumberOfSamples);
                                for (int i = 0; i < requestedNumberOfSamples * 2; i++)
                                {
                                    bufferPointer[i] += (float)synthesis.Oscillators[poly][osc].WaveData[i];
                                    oscilloscopeData[i] += (float)synthesis.Oscillators[poly][osc].WaveData[i];
                                }
                            }
                        }
                    }
                }
                synthesis.CurrentActivity = CURRENTACTIVITY.NONE;
            }
        }
    }
}
