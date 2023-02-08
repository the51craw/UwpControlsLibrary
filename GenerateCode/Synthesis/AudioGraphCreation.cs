using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Windows.Media.Audio;
using Windows.Media.MediaProperties;
using Windows.Media.Render;
using Windows.UI.Xaml.Controls;

namespace SynthLab
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
        public AudioGraph theAudioGraph;
        public AudioDeviceOutputNode DeviceOutputNode;
        public AudioSubmixNode Mixer;
        public ReverbEffectDefinition reverbEffectDefinition;
        //public EchoEffectDefinition echoEffectDefinition;

        private async Task<bool> CreateAudioGraph()
        {
            // Create an AudioGraph with default settings
            AudioGraphSettings settings = new AudioGraphSettings(AudioRenderCategory.Media);
            CreateAudioGraphResult result = await AudioGraph.CreateAsync(settings);

            if (result.Status != AudioGraphCreationStatus.Success)
            {
                // Cannot create AudioGraph
                ContentDialog error = new Message("Failed to create connection with audio hardware!");
                await error.ShowAsync();
                return false;
            }
            theAudioGraph = result.Graph;

            // Create a device output node
            CreateAudioDeviceOutputNodeResult deviceOutputNodeResult = await theAudioGraph.CreateDeviceOutputNodeAsync();
            if (deviceOutputNodeResult.Status != AudioDeviceNodeCreationStatus.Success)
            {
                // Cannot create device output node
                ContentDialog error = new Message("Failed to create connection with audio hardware!");
                await error.ShowAsync();
                return false;
            }
            DeviceOutputNode = deviceOutputNodeResult.DeviceOutputNode;

            // Create mixer:
            Mixer = theAudioGraph.CreateSubmixNode();
            Mixer.OutgoingGain = 1;
            Mixer.AddOutgoingConnection(DeviceOutputNode);

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
            mainPage.SampleRate = nodeEncodingProperties.SampleRate;
            //mainPage.SampleCount = nodeEncodingProperties.SampleRate / 100;

            // Create the FrameInputNode:
            FrameInputNode = theAudioGraph.CreateFrameInputNode(nodeEncodingProperties);
            FrameInputNode.AddOutgoingConnection(Mixer);

            // Create reverb:
            reverbEffectDefinition = new ReverbEffectDefinition(theAudioGraph);
            //reverbEffectDefinition.PositionLeft = 0;
            //reverbEffectDefinition.PositionRight = 0;
            //reverbEffectDefinition.PositionMatrixLeft = 0;
            //reverbEffectDefinition.PositionMatrixRight = 0;
            reverbEffectDefinition.ReverbGain = 1;
            reverbEffectDefinition.WetDryMix = 50;
            reverbEffectDefinition.ReflectionsDelay = 120;
            reverbEffectDefinition.ReverbDelay = 0;// 30;
            reverbEffectDefinition.RearDelay = 3;
            reverbEffectDefinition.DecayTime = 2;
            FrameInputNode.EffectDefinitions.Add(reverbEffectDefinition);
            TurnOffReverb();
        }

        public void StartAudioGraph()
        {
            FrameInputNode.QuantumStarted += FrameInputNode_QuantumStarted;
            theAudioGraph.Start();
        }

        public void TurnOnReverb()
        {
            FrameInputNode.EnableEffectsByDefinition(reverbEffectDefinition);
        }

        public void TurnOffReverb()
        {
            FrameInputNode.DisableEffectsByDefinition(reverbEffectDefinition);
        }
    }
}
