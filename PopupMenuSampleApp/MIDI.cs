using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Devices.Midi;
using Windows.UI.Xaml.Controls;
using Windows.UI.Core;
using Windows.Storage.Streams;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.UI.Popups;
using Windows.Foundation;
using Windows.Security.Cryptography;

namespace MIDI_to_tablature
{
    public class MIDI
    {
        //public CoreDispatcher coreDispatcher;
        //public IMidiOutPort midiOutPort;
        //public MidiInPort midiInPort;
        //public byte MidiOutPortChannel { get; set; }
        //public byte MidiInPortChannel { get; set; }
        //public Int32 MidiOutPortSelectedIndex { get; set; }
        //public Int32 MidiInPortSelectedIndex { get; set; }
        //public Boolean VenderDriverPresent = false;
        public bool IsInitiated;
        public List<MidiInPort> MidiInPorts { get; set; }
        public List<string> MidiInPortNames { get; set; }
        public List<IMidiOutPort> MidiOutPorts { get; set; }
        public List<string> MidiOutPortNames { get; set; }

        public bool IsConnected { get; set; }

        public MIDI(TypedEventHandler<MidiInPort, MidiMessageReceivedEventArgs> inPort_MessageReceived)
        {
            IsInitiated = false;
            IsConnected = true;
            Init(inPort_MessageReceived);
        }

        public async void Init(TypedEventHandler<MidiInPort, MidiMessageReceivedEventArgs> inPort_MessageReceived)
        {
            await InitMidi(inPort_MessageReceived);
            IsInitiated = true;
        }

        public async Task InitMidi(TypedEventHandler<MidiInPort, MidiMessageReceivedEventArgs> inPort_MessageReceived)
        {
            IsConnected = false;
            try
            {
                MidiInPorts = new List<MidiInPort>();
                MidiOutPorts = new List<IMidiOutPort>();
                string outPortSelection = MidiOutPort.GetDeviceSelector();
                string inPortSelection = MidiInPort.GetDeviceSelector();
                DeviceInformationCollection midiOutputDevices = await DeviceInformation.FindAllAsync(outPortSelection);
                DeviceInformationCollection midiInputDevices = await DeviceInformation.FindAllAsync(inPortSelection);

                MidiInPortNames = new List<string>();
                MidiOutPortNames = new List<string>();

                foreach (DeviceInformation device in midiOutputDevices)
                {
                    if (device != null)
                    {
                        try
                        {
                            IMidiOutPort midiOutPort = await MidiOutPort.FromIdAsync(device.Id);
                            if (midiOutPort != null)
                            {
                                MidiOutPorts.Add(midiOutPort);
                                MidiOutPortNames.Add(device.Name);
                            }
                        }
                        catch (Exception ex)
                        {
                            throw (ex);
                        }
                    }
                }

                foreach (DeviceInformation device in midiInputDevices)
                {
                    if (device != null)
                    {
                        try
                        {
                            MidiInPort midiInPort = await MidiInPort.FromIdAsync(device.Id);
                            if (midiInPort != null)
                            {
                                midiInPort.MessageReceived += inPort_MessageReceived;
                                MidiInPorts.Add(midiInPort);
                                MidiInPortNames.Add(device.Name);
                            }
                        }
                        catch (Exception ex)
                        {
                            throw (ex);
                        }
                    }
                }

            } catch { }
            
        }

        //private void MidiInPort_MessageReceived(MidiInPort sender, MidiMessageReceivedEventArgs args)
        //{
        //    throw new NotImplementedException();
        //}

        public void SendNoteOn(IMidiOutPort midiOutPort, int key, int channel, int velocity)
        {
            try
            {
                byte[] data = new byte[3];
                data[0] = (byte)(0x90 | channel);
                data[1] = (byte)(key);
                data[2] = (byte)(velocity);
                IBuffer buffer = CryptographicBuffer.CreateFromByteArray(data);
                midiOutPort.SendBuffer(buffer);
            }
            catch (Exception ex) { throw (ex); }
        }

        public void SendNoteOff(IMidiOutPort midiOutPort, int key, int channel, int velocity)
        {
            try { 
                byte[] data = new byte[3];
                data[0] = (byte)(0x80 | channel);
                data[1] = (byte)(key);
                data[2] = (byte)(velocity);
                IBuffer buffer = CryptographicBuffer.CreateFromByteArray(data);
                midiOutPort.SendBuffer(buffer);
            }
            catch (Exception ex) { throw (ex); }
        }

        public void SendControlChange(IMidiOutPort midiOutPort, byte channel, byte controller, byte value)
        {
            if (IsConnected)
            {
                try
                {
                    if (midiOutPort != null)
                    {
                        IMidiMessage midiMessageToSend = new MidiControlChangeMessage(channel, controller, value);
                        midiOutPort.SendMessage(midiMessageToSend);
                    }
                }
                catch
                {
                    IsConnected = false;
                    //MessageDialog warning = new MessageDialog("Communication with your VT-4 has been lost. Please verify connection and restart the app.");
                    //warning.Title = "Warning!";
                    //warning.Commands.Add(new UICommand { Label = "Ok", Id = 0 });
                    //var response = await warning.ShowAsync();
                }
            }
        }

        public void SendProgramChange(IMidiOutPort midiOutPort, byte channel, byte value)
        {
            //if (IsConnected)
            {
                try
                {
                    if (midiOutPort != null)
                    {
                        IMidiMessage midiMessageToSend = new MidiProgramChangeMessage(channel, value);
                        midiOutPort.SendMessage(midiMessageToSend);
                    }
                }
                catch
                {
                    IsConnected = false;
                    //MessageDialog warning = new MessageDialog("Communication with your VT-4 has been lost. Please verify connection and restart the app.");
                    //warning.Title = "Warning!";
                    //warning.Commands.Add(new UICommand { Label = "Ok", Id = 0 });
                    //var response = await warning.ShowAsync();
                }
            }
        }

        public async void SendPitchBender(IMidiOutPort midiOutPort, byte channel, Int32 value)
        {
            try
            {
                if (midiOutPort != null)
                {
                    //IMidiMessage midiMessageToSend = new MidiPitchBendChangeMessage(channel, (UInt16)value);
                    byte[] msg = new byte[] { (byte)(0xe0 + channel), (byte)(value % 128), (byte)(value / 128) };
                    midiOutPort.SendBuffer(msg.AsBuffer());
                }
            }
            catch
            {
                MessageDialog warning = new MessageDialog("Communication with your VT-4 has been lost. Please verify connection and restart the app.");
                warning.Title = "Warning!";
                warning.Commands.Add(new UICommand { Label = "Ok", Id = 0 });
                var response = await warning.ShowAsync();
            }
        }

        //public async void SetVolume(IMidiOutPort midiOutPort, byte currentChannel, byte volume)
        //{
        //    try
        //    {
        //        if (midiOutPort != null)
        //        {
        //            IMidiMessage midiMessageToSend = new MidiControlChangeMessage(channel, 0x07, volume);
        //            midiOutPort.SendMessage(midiMessageToSend);
        //        }
        //    }
        //    catch
        //    {
        //        MessageDialog warning = new MessageDialog("Communication with your VT-4 has been lost. Please verify connection and restart the app.");
        //        warning.Title = "Warning!";
        //        warning.Commands.Add(new UICommand { Label = "Ok", Id = 0 });
        //        var response = await warning.ShowAsync();
        //    }
        //}

        //public async void AllNotesOff(IMidiOutPort midiOutPort, byte currentChannel)
        //{
        //    try
        //    {
        //        if (midiOutPort != null)
        //        {
        //            IMidiMessage midiMessageToSend = new MidiControlChangeMessage(channel, 0x78, 0);
        //            midiOutPort.SendMessage(midiMessageToSend);
        //        }
        //    }
        //    catch
        //    {
        //        MessageDialog warning = new MessageDialog("Communication with your VT-4 has been lost. Please verify connection and restart the app.");
        //        warning.Title = "Warning!";
        //        warning.Commands.Add(new UICommand { Label = "Ok", Id = 0 });
        //        var response = await warning.ShowAsync();
        //    }
        //}

        //public async void ProgramChange(IMidiOutPort midiOutPort, byte currentChannel, String smsb, String slsb, String spc)
        //{
        //    try
        //    {
        //        MidiControlChangeMessage controlChangeMsb = new MidiControlChangeMessage(channel, 0x00, (byte)(UInt16.Parse(smsb)));
        //        MidiControlChangeMessage controlChangeLsb = new MidiControlChangeMessage(channel, 0x20, (byte)(UInt16.Parse(slsb)));
        //        MidiProgramChangeMessage programChange = new MidiProgramChangeMessage(channel, (byte)(UInt16.Parse(spc) - 1));
        //        midiOutPort.SendMessage(controlChangeMsb);
        //        midiOutPort.SendMessage(controlChangeLsb);
        //        midiOutPort.SendMessage(programChange);
        //    }
        //    catch
        //    {
        //        MessageDialog warning = new MessageDialog("Communication with your VT-4 has been lost. Please verify connection and restart the app.");
        //        warning.Title = "Warning!";
        //        warning.Commands.Add(new UICommand { Label = "Ok", Id = 0 });
        //        var response = await warning.ShowAsync();
        //    }
        //}

        //public async void ProgramChange(IMidiOutPort midiOutPort, byte currentChannel, byte msb, byte lsb, byte pc)
        //{
        //    try
        //    {
        //        MidiControlChangeMessage controlChangeMsb = new MidiControlChangeMessage(channel, 0x00, msb);
        //        MidiControlChangeMessage controlChangeLsb = new MidiControlChangeMessage(channel, 0x20, lsb);
        //        MidiProgramChangeMessage programChange = new MidiProgramChangeMessage(currentChannel, (byte)(pc - 1));
        //        midiOutPort.SendMessage(controlChangeMsb);
        //        midiOutPort.SendMessage(controlChangeLsb);
        //        midiOutPort.SendMessage(programChange);
        //    }
        //    catch
        //    {
        //        MessageDialog warning = new MessageDialog("Communication with your VT-4 has been lost. Please verify connection and restart the app.");
        //        warning.Title = "Warning!";
        //        warning.Commands.Add(new UICommand { Label = "Ok", Id = 0 });
        //        var response = await warning.ShowAsync();
        //    }
        //}

        //public async void SetInControl(IMidiOutPort midiOutPort)
        //{
        //    try
        //    {
        //        //MidiNoteOnMessage InControlMesssage = new MidiNoteOnMessage(0x0f, 0x00, 0x7f);
        //        //midiOutPort.SendMessage(InControlMesssage);
        //        byte[] bytes = { 0x9f, (byte)(channel + 0x0c), 0x7f };
        //        IBuffer buffer = bytes.AsBuffer();
        //        midiOutPort.SendBuffer(buffer);
        //    }
        //    catch (Exception e)
        //    {
        //        MessageDialog warning = new MessageDialog("Communication with your VT-4 has been lost. Please verify connection and restart the app.");
        //        warning.Title = "Warning!";
        //        warning.Commands.Add(new UICommand { Label = "Ok", Id = 0 });
        //        var response = await warning.ShowAsync();
        //    }
        //}

        //public async void ResetInControl(IMidiOutPort midiOutPort)
        //{
        //    try
        //    {
        //        MidiControlChangeMessage InControlMesssage = new MidiControlChangeMessage(0x9f, 0x00, 0x00);
        //        midiOutPort.SendMessage(InControlMesssage);
        //    }
        //    catch
        //    {
        //        MessageDialog warning = new MessageDialog("Communication with your VT-4 has been lost. Please verify connection and restart the app.");
        //        warning.Title = "Warning!";
        //        warning.Commands.Add(new UICommand { Label = "Ok", Id = 0 });
        //        var response = await warning.ShowAsync();
        //    }
        //}

        public void SendSystemExclusive(IMidiOutPort midiOutPort, byte[] bytes)
        {
            if (IsConnected)
            {
                try
                {
                    IBuffer buffer = bytes.AsBuffer();
                    midiOutPort.SendBuffer(buffer);
                }
                catch
                {
                    IsConnected = false;
                    //MessageDialog warning = new MessageDialog("Communication with your VT-4 has been lost. Please verify connection and restart the app.");
                    //warning.Title = "Warning!";
                    //warning.Commands.Add(new UICommand { Label = "Ok", Id = 0 });
                    //var response = await warning.ShowAsync();
                }
            }
        }

        //public byte[] SystemExclusiveRQ1Message(DeviceInfo device, byte[] Address, byte[] Length)
        //{
        //    byte[] result = new byte[17];
        //    result[0] = 0xf0; // Start of exclusive message
        //    result[1] = device.CompanyNumber;
        //    result[2] = device.UnitNumber;
        //    result[3] = device.SysExId[0];
        //    result[4] = device.SysExId[1];
        //    result[5] = device.SysExId[2];
        //    result[6] = 0x11; // Command (RQ1)
        //    result[7] = Address[0];
        //    result[8] = Address[1];
        //    result[9] = Address[2];
        //    result[10] = Address[3];
        //    result[11] = Length[0];
        //    result[12] = Length[1];
        //    result[13] = Length[2];
        //    result[14] = Length[3];
        //    result[15] = 0x00; // Filled out by CheckSum but present here to avoid confusion about index 15 missing.
        //    result[16] = 0xf7; // End of sysex
        //    CheckSum(ref result);
        //    return (result);
        //}

        //public byte[] SystemExclusiveRQ1Message(byte[] Address, byte[] Length)
        //{
        //    byte[] result = new byte[18];
        //    result[0] = 0xf0; // Start of exclusive message
        //    result[1] = 0x41;
        //    result[2] = 0x10;
        //    result[3] = 0x00;
        //    result[4] = 0x00;
        //    result[5] = 0x00;
        //    result[6] = 0x51;
        //    result[7] = 0x11; // Command (RQ1)
        //    result[8] = Address[0];
        //    result[9] = Address[1];
        //    result[10] = Address[2];
        //    result[11] = Address[3];
        //    result[12] = Length[0];
        //    result[13] = Length[1];
        //    result[14] = Length[2];
        //    result[15] = Length[3];
        //    result[16] = 0x00; // Filled out by CheckSum but present here to avoid confusion about index 15 missing.
        //    result[17] = 0xf7; // End of sysex
        //    CheckSum(ref result);
        //    return (result);
        //}

        public byte[] SystemExclusiveDT1Message(byte[] DeviceInfo, byte[] Address, byte[] DataToTransmit)
        {
            Int32 length = DeviceInfo.Length + DataToTransmit.Length + 7; // Start of sysEx + 4 bytes address + end of sysEx + checksum = 7.
            byte[] result = new byte[length];
            int i = 0;
            result[i++] = 0xf0; // Start of exclusive message
            for (int j = 0; j < DeviceInfo.Length; j++)
            {
                result[i++] = DeviceInfo[1 + j];
            }
            result[i++] = 0x12; // Command (DT1)
            result[i++] = Address[0];
            result[i++] = Address[1];
            result[i++] = Address[2];
            result[i++] = Address[3];
            for (Int32 j = 0; j < DataToTransmit.Length; j++)
            {
                result[j + i] = DataToTransmit[j];
            }
            result[i++] = 0xf7; // End of sysex
            result[i++] = CheckSum(result);
            return (result);
        }

        public byte[] SystemExclusiveDT1Message(byte[] Address, byte[] DataToTransmit)
        {
            Int32 length = 14 + DataToTransmit.Length;
            byte[] result = new byte[length];
            result[0] = 0xf0; // Start of exclusive message
            result[1] = 0x41;
            result[2] = 0x10;
            result[3] = 0x00;
            result[4] = 0x00;
            result[5] = 0x00;
            result[6] = 0x51;
            result[7] = 0x12; // Command (DT1)
            result[8] = Address[0];
            result[9] = Address[1];
            result[10] = Address[2];
            result[11] = Address[3];
            for (Int32 i = 0; i < DataToTransmit.Length; i++)
            {
                result[i + 12] = DataToTransmit[i];
            }
            result[13 + DataToTransmit.Length] = 0xf7; // End of sysex
            CheckSum(result);
            return (result);
        }

        public byte CheckSum(byte[] bytes)
        {
            byte chksum = 0;
            for (Int32 i = 8; i < bytes.Length - 2; i++)
            {
                chksum += bytes[i];
            }
            bytes[bytes.Length - 2] = (byte)((0x80 - (chksum & 0x7f)) & 0x7f);
            return chksum;
        }
    }

    public class PortPair
    {
        public Int32 ID { get;set; }
        public String Name { get; set; }
        public MidiInPort InPort;
        //public byte DeviceID = 0x10;
        public byte InChannel { get; set; }
        //public Int32 MidiInPortSelectedIndex { get; set; }
        public IMidiOutPort OutPort;
        //public byte DeviceID = 0x10;
        public byte OutChannel { get; set; }
        //public Int32 MidiOutPortSelectedIndex { get; set; }
    }
}
