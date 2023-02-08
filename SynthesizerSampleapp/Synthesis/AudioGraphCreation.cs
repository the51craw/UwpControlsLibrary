using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Windows.Media.Audio;
using Windows.Media.MediaProperties;
using Windows.Media.Render;
using Windows.UI.Xaml.Controls;

namespace UwpControlsLibrary
{
    [ComImport]
    [Guid("5B0D3235-4DBA-4D44-865E-8F1D0E4FD04D")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    unsafe interface IMemoryBufferByteAccess
    {
        void GetBuffer(out byte* buffer, out uint capacity);
    }

    public partial class FrameServer
    {
        public bool IsInitiated = false;
        public AudioGraph theAudioGraph;
        public AudioDeviceOutputNode DeviceOutputNode;
        public AudioSubmixNode Mixer;
        public ReverbEffectDefinition reverbEffectDefinition;
        public EqualizerEffectDefinition eqEffectDefinition;
        public uint SampleCount;

        private async Task<bool> CreateAudioGraph()
        {
            // Create an AudioGraph with default settings
            AudioGraphSettings settings = new AudioGraphSettings(AudioRenderCategory.Media);
            CreateAudioGraphResult result = await AudioGraph.CreateAsync(settings);

            if (result.Status != AudioGraphCreationStatus.Success)
            {
                // Cannot create AudioGraph
                //ContentDialog error = new Message("Failed to create connection with audio hardware!");
                //await error.ShowAsync();
                return false;
            }
            theAudioGraph = result.Graph;
            synthesis.SampleCount = theAudioGraph.SamplesPerQuantum;

            // Create a device output node
            CreateAudioDeviceOutputNodeResult deviceOutputNodeResult = await theAudioGraph.CreateDeviceOutputNodeAsync();
            if (deviceOutputNodeResult.Status != AudioDeviceNodeCreationStatus.Success)
            {
                // Cannot create device output node
                //ContentDialog error = new Message("Failed to create connection with audio hardware!");
                //await error.ShowAsync();
                return false;
            }
            DeviceOutputNode = deviceOutputNodeResult.DeviceOutputNode;
            synthesis.SampleRate = DeviceOutputNode.EncodingProperties.SampleRate;

            // Create mixer:
            Mixer = theAudioGraph.CreateSubmixNode();
            Mixer.OutgoingGain = 1;
            Mixer.AddOutgoingConnection(DeviceOutputNode);
            IsInitiated = true;
            return true;
        }

        public void CreateFrameInputNode()
        {
            // Set the NodeEncodingPorperties as the same format as the graph, except explicitly set mono:
            AudioEncodingProperties nodeEncodingProperties = theAudioGraph.EncodingProperties;
            nodeEncodingProperties.ChannelCount = 2;
            //nodeEncodingProperties.SampleRate = 48000;
            nodeEncodingProperties.Bitrate = nodeEncodingProperties.SampleRate * nodeEncodingProperties.BitsPerSample;
            //nodeEncodingProperties.BitsPerSample = 32;
            //nodeEncodingProperties.Subtype = "Float";
            //mainPage.SampleRate = nodeEncodingProperties.SampleRate;
            SampleCount = nodeEncodingProperties.SampleRate / 100;

            // Create the FrameInputNode:
            FrameInputNode = theAudioGraph.CreateFrameInputNode(nodeEncodingProperties);
            FrameInputNode.AddOutgoingConnection(Mixer);

            // Create reverb:
            CreateReverb();

            // Create equalizer:
            CreateEqualizer();
        }

        private void CreateReverb()
        {
            reverbEffectDefinition = new ReverbEffectDefinition(theAudioGraph);
            //reverbEffectDefinition.PositionLeft = 0;
            //reverbEffectDefinition.PositionRight = 0;
            //reverbEffectDefinition.PositionMatrixLeft = 0;
            //reverbEffectDefinition.PositionMatrixRight = 0;
            reverbEffectDefinition.ReverbGain = 10;
            reverbEffectDefinition.WetDryMix = 0;
            reverbEffectDefinition.ReflectionsDelay = 0;
            reverbEffectDefinition.ReverbDelay = 0;
            reverbEffectDefinition.RearDelay = 0;
            reverbEffectDefinition.DecayTime = 1;
            FrameInputNode.EffectDefinitions.Add(reverbEffectDefinition);
            //TurnOnReverb();
            //TurnOffReverb();
        }

        private void CreateEqualizer()
        {
            // Max Gain = 7.94
            // No Gain = 1.0
            // Min Gain = 0.126
            // Min FrequencyCenter = 20
            // See the MSDN page for parameter explanations
            // https://msdn.microsoft.com/en-us/library/windows/desktop/microsoft.directx_sdk.xapofx.fxeq_parameters(v=vs.85).aspx
            eqEffectDefinition = new EqualizerEffectDefinition(theAudioGraph);
            //eqEffectDefinition.Bands[0].FrequencyCenter = 20f;        // 100.0f
            //eqEffectDefinition.Bands[0].Gain = 0.126;                 // 4.033f
            //eqEffectDefinition.Bands[0].Bandwidth = 1.5f;             // 1.5f

            //eqEffectDefinition.Bands[1].FrequencyCenter = 100.0f;     // 900.0f
            //eqEffectDefinition.Bands[1].Gain = 5.0f;                  // 1.6888f
            //eqEffectDefinition.Bands[1].Bandwidth = 1.5f;             // 1.5f

            //eqEffectDefinition.Bands[2].FrequencyCenter = 3000.0f;    // 5000.0f
            //eqEffectDefinition.Bands[2].Gain = 1.0f;                  // 2.4702f
            //eqEffectDefinition.Bands[2].Bandwidth = 1.5f;             // 1.5f

            //eqEffectDefinition.Bands[3].FrequencyCenter = 9000.0f;    // 12000.0f
            //eqEffectDefinition.Bands[3].Gain = 1.0;                   // 5.5958f;
            //eqEffectDefinition.Bands[3].Bandwidth = 2.0f;             // 2.0f

            eqEffectDefinition.Bands[0].FrequencyCenter = 100.0f;
            eqEffectDefinition.Bands[0].Gain = 4.033f;
            eqEffectDefinition.Bands[0].Bandwidth = 1.5f;

            eqEffectDefinition.Bands[1].FrequencyCenter = 900.0f;
            eqEffectDefinition.Bands[1].Gain = 1.6888f;
            eqEffectDefinition.Bands[1].Bandwidth = 1.5f;

            eqEffectDefinition.Bands[2].FrequencyCenter = 5000.0f;
            eqEffectDefinition.Bands[2].Gain = 2.4702f;
            eqEffectDefinition.Bands[2].Bandwidth = 1.5f;

            eqEffectDefinition.Bands[3].FrequencyCenter = 12000.0f;
            eqEffectDefinition.Bands[3].Gain = 5.5958f;
            eqEffectDefinition.Bands[3].Bandwidth = 2.0f;

            FrameInputNode.EffectDefinitions.Add(eqEffectDefinition);
            FrameInputNode.EnableEffectsByDefinition(eqEffectDefinition);
        }


        public void StartAudioGraph()
        {
            FrameInputNode.QuantumStarted += FrameInputNode_QuantumStarted;
            theAudioGraph.Start();
        }
    }
}
