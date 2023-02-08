using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;

namespace UwpControlsLibrary
{
    public enum _compoundType
    {
        OSCILLATOR,
        PITCH_ENVELOPE,
        ADSR,
        DISPLAY,
        WIRE,
        OTHER,
        FILTER,
        CONTROL_PANEL,
        NONE,
    }

    // NOTE! These declarations _must_ be in the same order as they are created!
    public enum OscillatorControls
    {
        MODULATION,
        FREQUENCY,
        FINE_TUNE,
        VOLUME,
        WAVE,
        KEYBOARD_OR_FIXED,
        ADSR_OR_PULSE,
        VIEW_ME,
        LEDSOUNDING,
        LEDSOUNDING_LIGHT,
        LEDSOUNDING_HEAVY,
        MODULATION_KNOB_TARGET,
        MODULATION_WHEEL_TARGET,
        AM_SOCKET,
        FM_SOCKET,
        XM_SOCKET,
        OUT_SOCKET,
        NUMBER,
        WIRE,
        NONE
    }

    public enum FilterControls
    {
        Q,
        FREQUENCY_CENTER,
        KEYBOARD_FOLLOW,
        GAIN,
        FILTER_MIX,
        FILTER_FUNCTION,
        MODULATION_WHEEL_TARGET,
        NONE,
    }

    public enum PitchEnvelopeControls
    {
        PITCH_ENV_MODULATION_WHEEL_USE,
        DEPTH,
        SPEED,
        MOD_PITCH,
        MOD_AM,
        MOD_FM,
        MOD_XM,
        GRAPH,
        NONE,
    }

    public enum AdsrControls
    {
        ADSR_A,
        ADSR_D,
        ADSR_S,
        ADSR_R,
        ADSR_MODULATION_WHEEL_USE,
        ADSR_AM_SENS,
        ADSR_FM_SENS,
        ADSR_XM_SENS,
        PEDAL_HOLD,
        GRAPH,
        NONE,
    }

    public enum DisplayControls
    {
        FREQUENCY,
        OSCILLOGRAPH,
        DIGITS,
        MILLIVOLTS_PER_CM,
        MILLISECONDS_PER_CM,
        DISPLAY_ON_OFF,
    }

    public enum type
    {
        OSCILLATOR,
        PITCH_ENVELOPE,
        ADSR,
        DISPLAY,
        FILTER,
        CONTROL_PANEL,
    }

    public enum ControlPanelControls
    {
        SAVE_TO_FILE,
        SAVE_AS_TO_FILE,
        LOAD_FROM_FILE,
        FACTORY_PATCHES,
        CHORUS,
        LAYOUT,
        REVERB,
        SETTINGS,
        MIDI_SETTINGS,
        MANUAL,
        REVERB_VALUE,
        USING_GRAPHICS_CARD,
        NONE
    }

    public enum OtherControls
    {
        PITCH_BENDER_WHEEL = 1,
        MODULATION_WHEEL,
        DISPLAY,
        NONE
    }

    public enum WAVEFORM
    {
        SQUARE,
        SAW_DOWN,
        SAW_UP,
        TRIANGLE,
        SINE,
        RANDOM,
        NOISE,
        WAVE,
        DRUMSET,
        NONE,
    }

    public enum MODULATION
    {
        NONE,
        AM,
        FM_SINE,
        FM,
        PM,
    }
    //public sealed partial class MainPage : Page
    //{
    //}
}
