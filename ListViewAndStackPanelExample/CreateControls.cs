using System;
using UwpControlsLibrary;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace ListViewAndStackPanelExample
{
    public sealed partial class MainPage : Page
    {
        // The Controls object:
        Controls Controls;
        Controls FixedControls;

        // This enum must contain one entry for each control!
        // You can use it in handlers to identify what action to take when control is used.
        public enum OscillatorControls
        {
        MODULATION,
        FREQUENCY,
        FINE_TUNE,
        VOLUME,
        WAVE,
        KEYBOARD_OR_FIXED,
        ADSR_PULSE_OR_FM,
        VIEW_ME,
        LEDSOUNDING,
        MODULATION_KNOB_TARGET,
        AM_SOCKET,
        FM_SOCKET,
        PM_SOCKET,
        OUT_SOCKET,
        WIRE,
        NONE
        }

    public enum FilterControls
    {
        Q,
        FREQUENCY_CENTER,
        KEYBOARD_FOLLOW,
        DEPTH,
        FILTER_MIX,
        FILTER_FUNCTION,
        FILTER_SELECTOR,
    }

    public enum AdsrControls
    {
        ADSR_A,
        ADSR_D,
        ADSR_S,
        ADSR_R,
        PEDAL_HOLD,
        ADSR_GRAPH,
    }

    public enum DisplayControls
    {
        FREQUENCY,
        DIGITS,
        OSCILLOGRAPH,
    }
    public enum type
        {
            OSCILLATOR,
            FILTER,
            ADSR,
            DISPLAY,
        }

        public CompoundControl[] oscillators;
        public CompoundControl[] filters;
        public CompoundControl[] displays;
        public CompoundControl[] adsrs;

    public enum WAVEFORM
    {
        SQUARE,
        SAW_UP,
        SAW_DOWN,
        TRIANGLE,
        SINE,
        RANDOM,
        NOISE,
        NONE
    }

    public enum MODULATION
    {
        NONE,
        AM,
        FM_SINE,
        FM,
        PM,
    }
        Brush color;

        // PointerMoved will update this in order to let other handlers know which control is handled.
        private Int32 currentControlItem;

        // Main grid layout:

        /// <summary>
        /// Horizontal start point of grid for layout of controls
        /// </summary>
        Int32 gridStartX = 12;

        /// <summary>
        /// Vertical start point of grid for layout of controls
        /// </summary>
        Int32 gridStartY = 41;

        /// <summary>
        /// Horizontal spacing in grid for layout of controls
        /// </summary>
        Int32 gridSpacingX = 475;

        /// <summary>
        /// Vertical spacing in grid for layout of controls
        /// </summary>
        Int32 gridSpacingY = 207;

        // Oscillators layout:

        /// <summary>
        /// Number of oscillators:
        /// </summary>
        Int32 oscillatorCount = 4;

        /// <summary>
        /// Column of leftmost oscillator(s) in grid for layout of controls
        /// </summary>
        Int32 oscillatorsX = 0;

        /// <summary>
        /// Row of topmost oscillator(s) in grid for layout of controls
        /// </summary>
        Int32 oscillatorsY = 0;

        /// <summary>
        /// Number of columns to skip for oscillators in layout of controls
        /// </summary>
        Int32 oscillatorsSkipX = 1;

        /// <summary>
        /// Number of rows to skip for oscillators in layout of controls
        /// </summary>
        Int32 oscillatorsSkipY = 0;

        // Filters layout:

        /// <summary>
        /// Number of filters:
        /// </summary>
        Int32 filterCount = 2;

        /// <summary>
        /// Column of leftmost filter(s) in grid for layout of controls
        /// </summary>
        Int32 filtersX = 2;

        /// <summary>
        /// Row of topmost filter(s) in grid for layout of controls
        /// </summary>
        Int32 filtersY = 2;

        /// <summary>
        /// Number of columns to skip for filters in layout of controls
        /// </summary>
        Int32 filtersSkipX = 0;

        /// <summary>
        /// Number of rows to skip for filters in layout of controls
        /// </summary>
        Int32 filtersSkipY = 0;

        // Displays layout:

        /// <summary>
        /// Number of displays:
        /// </summary>
        Int32 displayCount = 2;

        /// <summary>
        /// Column of leftmost display(s) in grid for layout of controls
        /// </summary>
        Int32 displaysX = 0;

        /// <summary>
        /// Row of topmost display(s) in grid for layout of controls
        /// </summary>
        Int32 displaysY = 2;

        /// <summary>
        /// Number of columns to skip for displays in layout of controls
        /// </summary>
        Int32 displaysSkipX = 0;

        /// <summary>
        /// Number of rows to skip for displays in layout of controls
        /// </summary>
        Int32 displaysSkipY = 0;

        // Adsrs layout:

        /// <summary>
        /// Number of adsrs:
        /// </summary>
        Int32 adsrCount = 4;

        /// <summary>
        /// Column of leftmost adsr(s) in grid for layout of controls
        /// </summary>
        Int32 adsrsX = 1;

        /// <summary>
        /// Row of topmost adsr(s) in grid for layout of controls
        /// </summary>
        Int32 adsrsY = 0;

        /// <summary>
        /// Number of columns to skip for adsrs in layout of controls
        /// </summary>
        Int32 adsrsSkipX = 1;

        /// <summary>
        /// Number of rows to skip for adsrs in layout of controls
        /// </summary>
        Int32 adsrsSkipY = 0;

        private void SetupGrid()
        {
            switch (layout)
            {
                case 0:
                    oscillatorCount = 4;
                    oscillatorsX = 0;
                    oscillatorsY = 0;
                    oscillatorsSkipX = 3;
                    oscillatorsSkipY = 1;
                    filterCount = 4;
                    filtersX = 1;
                    filtersY = 0;
                    filtersSkipX = 3;
                    filtersSkipY = 1;
                    displayCount = 4;
                    adsrCount = 4;
                    adsrsX = 2;
                    adsrsY = 0;
                    adsrsSkipX = 3;
                    adsrsSkipY = 1;
                    displaysX = 3;
                    displaysY = 0;
                    displaysSkipX = 3;
                    displaysSkipY = 1;
                    break;
                case 1:
                    oscillatorCount = 6;
                    oscillatorsX = 0;
                    oscillatorsY = 0;
                    oscillatorsSkipX = 1;
                    oscillatorsSkipY = 0;
                    filterCount = 6;
                    filtersX = 1;
                    filtersY = 0;
                    filtersSkipX = 1;
                    filtersSkipY = 0;
                    adsrCount = 2;
                    adsrsX = 0;
                    adsrsY = 3;
                    adsrsSkipX = 1;
                    adsrsSkipY = 0;
                    displayCount = 2;
                    displaysX = 1;
                    displaysY = 3;
                    displaysSkipX = 1;
                    displaysSkipY = 0;
                    break;
                case 2:
                    oscillatorCount = 8;
                    oscillatorsX = 0;
                    oscillatorsY = 0;
                    oscillatorsSkipX = 0;
                    oscillatorsSkipY = 1;
                    filterCount = 4;
                    filtersX = 0;
                    filtersY = 2;
                    filtersSkipX = 0;
                    filtersSkipY = 0;
                    adsrCount = 2;
                    adsrsX = 0;
                    adsrsY = 3;
                    adsrsSkipX = 1;
                    adsrsSkipY = 0;
                    displayCount = 2;
                    displaysX = 1;
                    displaysY = 3;
                    displaysSkipX = 1;
                    displaysSkipY = 0;
                    break;
                case 3:
                    oscillatorCount = 12;
                    oscillatorsX = 0;
                    oscillatorsY = 0;
                    oscillatorsSkipX = 0;
                    oscillatorsSkipY = 0;
                    filterCount = 1;
                    filtersX = 2;
                    filtersY = 3;
                    filtersSkipX = 0;
                    filtersSkipY = 0;
                    adsrCount = 1;
                    adsrsX = 0;
                    adsrsY = 3;
                    adsrsSkipX = 0;
                    adsrsSkipY = 0;
                    displayCount = 1;
                    displaysX = 1;
                    displaysY = 3;
                    displaysSkipX = 0;
                    displaysSkipY = 0;
                    break;
            }
        }

        private void CreateControls()
        {
            Controls.Init(gridControls);
            SetupGrid();
            oscillators = new CompoundControl[oscillatorCount];
            displays = new CompoundControl[displayCount];
            filters = new CompoundControl[filterCount];
            adsrs = new CompoundControl[adsrCount];

            Int32 knobOscillatorX = 47;
            Int32 knobOscillatorY = 120;
            Int32 knobOscillatorSpacingX = 81;
            Int32 buttonOscillatorX = 333;
            Int32 buttonOscillatorY = 12;
            Int32 buttonOscillatorSpacingY = 31;
            Int32 modButtonOscillatorX = 10;
            Int32 indicatorOscillatorX = 431;
            Int32 indicatorOscillatorY = 137;

            // Input buttons offsets and spacing:
            Int32 inputOffsetX = 71;
            Int32 inputOffsetY = 11;
            Int32 inputSpacing = 102;

            // Output button offset:
            Int32 outputOffsetX = 426;
            Int32 outputOffsetY = 160;

            Int32 knobFilterX = 51;
            Int32 knobFilterY = 121;
            Int32 knobFiltersSpacingX = 84;
            Int32 buttonFunctioFilterX = 10;
            Int32 buttonModFilterX = 206;
            Int32 buttonFilterY = 12;

            Int32 knobAdsrX = 47;
            Int32 knobAdsrY = 132;
            Int32 knobAdsrSpacingX = 81;
            Int32 buttonAdsrX = 333;
            Int32 buttonAdsrY = 12;
            Int32 buttonAdsrSpacingY = 31;
            Int32 modButtonAdsrX = 10;

            Int32 displayDigitsBackgroundX = 13;
            Int32 displayDigitsBackgroundY = 13;
            Int32 displayDigitsX = 4;
            Int32 displayDigitsY = 80;
            Int32 displayOscilloscopeBackgroundX = 238;
            Int32 displayOscilloscopeBackgroundY = 13;

            // Create all controls:
            Int32 i = 0;
            for (Int32 y = 0; y < LimitY(oscillatorCount, oscillatorsSkipX, oscillatorsSkipY); y++)
            {
                for (Int32 x = 0; x < LimitX(oscillatorCount, oscillatorsSkipX, oscillatorsSkipY); x++)
                {
                    oscillators[i] = Controls.AddCompoundControl(
                        new Rect(0, 0, imgBackgroundUpper.ActualWidth, imgBackgroundUpper.ActualHeight),
                        imgClickArea, i, (Int32)type.OSCILLATOR, gridControls, new Image[] { imgOscillatorBackground },
                        AssembleHitarea(type.OSCILLATOR, x, y, 0, 0));
                    oscillators[i].AddKnob((Int32)OscillatorControls.MODULATION, gridControls, new Image[] { imgArrowknob_75 },
                        new Point(knobOscillatorX, knobOscillatorY), 0, 127, 30, 330, 2);
                    oscillators[i].AddKnob((Int32)OscillatorControls.FREQUENCY, gridControls, new Image[] { imgArrowknob_75 },
                        new Point(knobOscillatorX + knobOscillatorSpacingX, knobOscillatorY), 0, 127, 30, 330, 2);
                    oscillators[i].AddKnob((Int32)OscillatorControls.FINE_TUNE, gridControls, new Image[] { imgArrowknob_75 },
                        new Point(knobOscillatorX + knobOscillatorSpacingX * 2, knobOscillatorY), 0, 127, 30, 330, 8);
                    oscillators[i].AddKnob((Int32)OscillatorControls.VOLUME, gridControls, new Image[] { imgArrowknob_75 },
                        new Point(knobOscillatorX + knobOscillatorSpacingX * 3, knobOscillatorY), 0, 127, 30, 330, 2);
                    oscillators[i].AddRotator((Int32)OscillatorControls.WAVE, gridControls,
                        new Image[] { imgSelectSquare, imgSelectSawDwn, imgSelectSawUp, imgSelectTriangle, imgSelectSine, imgSelectRandom, imgSelectNoise },
                        new Point(buttonOscillatorX, buttonOscillatorY));
                    oscillators[i].AddRotator((Int32)OscillatorControls.KEYBOARD_OR_FIXED, gridControls,
                        new Image[] { imgSelectKeyboard, imgSelectFixFreq },
                        new Point(buttonOscillatorX, buttonOscillatorY + buttonOscillatorSpacingY));
                    oscillators[i].AddRotator((Int32)OscillatorControls.ADSR_PULSE_OR_FM, gridControls,
                        new Image[] { imgSelectADSR, imgSelectPulse, imgSelectorAdsrFm },
                        new Point(buttonOscillatorX, buttonOscillatorY + buttonOscillatorSpacingY * 2));
                    oscillators[i].AddRotator((Int32)OscillatorControls.VIEW_ME, gridControls,
                        new Image[] { imgSelectorViewOff, imgSelectorViewOn },
                        new Point(buttonOscillatorX, buttonOscillatorY + buttonOscillatorSpacingY * 3));
                    oscillators[i].AddIndicator((Int32)OscillatorControls.LEDSOUNDING, gridControls, new Image[] { imgLedOn },
                        new Point(indicatorOscillatorX, indicatorOscillatorY));
                    oscillators[i].AddRotator((Int32)OscillatorControls.MODULATION_KNOB_TARGET, gridControls,
                        new Image[] { imgModAM, imgModFM, imgModPM, imgModAll },
                        new Point(modButtonOscillatorX, buttonOscillatorY + buttonOscillatorSpacingY));
                    //oscillators[i].AddAreaButton(Int32)OscillatorControls.AM_SOCKET, gridControls,
                    //    new Rect(inputOffsetX, inputOffsetY, 30, 30));
                    //oscillators[i].AddAreaButton((Int32)OscillatorControls.FM_SOCKET, gridControls,
                    //    new Rect(inputOffsetX + inputSpacing, inputOffsetY, 30, 30));
                    //oscillators[i].AddAreaButton((Int32)OscillatorControls.PM_SOCKET, gridControls,
                    //    new Rect(inputOffsetX + inputSpacing * 2, inputOffsetY, 30, 30));
                    //oscillators[i].AddAreaButton((Int32)OscillatorControls.OUT_SOCKET, gridControls,
                    //    new Rect(outputOffsetX, outputOffsetY, 30, 30));
                    i++;
                }
            }

            i = 0;
            for (Int32 y = 0; y < LimitY(filterCount, filtersSkipX, filtersSkipY); y++)
            {
                for (Int32 x = 0; x < LimitX(filterCount, filtersSkipX, filtersSkipY); x++)
                {
                    filters[i] = Controls.AddCompoundControl(
                        new Rect(0, 0, imgBackgroundUpper.ActualWidth, imgBackgroundUpper.ActualHeight), imgClickArea,
                        i, (Int32)type.FILTER,
                        gridControls, new Image[] { imgFilterBackground },
                        AssembleHitarea(type.FILTER, x, y, 0, 0));
                    filters[i].AddKnob((Int32)FilterControls.Q, gridControls, new Image[] { imgArrowknob_75 },
                        new Point(knobFilterX, knobFilterY), 0, 127, 30, 330, 2);
                    filters[i].AddKnob((Int32)FilterControls.FREQUENCY_CENTER, gridControls, new Image[] { imgArrowknob_75 },
                        new Point(knobFilterX + knobFiltersSpacingX, knobFilterY), 0, 127, 30, 330, 2);
                    filters[i].AddKnob((Int32)FilterControls.KEYBOARD_FOLLOW, gridControls, new Image[] { imgArrowknob_75 },
                        new Point(knobFilterX + knobFiltersSpacingX * 2, knobFilterY), 0, 127, 30, 330, 2);
                    filters[i].AddKnob((Int32)FilterControls.DEPTH, gridControls, new Image[] { imgArrowknob_75 },
                        new Point(knobFilterX + knobFiltersSpacingX * 3, knobFilterY), 0, 127, 30, 330, 2);
                    filters[i].AddKnob((Int32)FilterControls.FILTER_MIX, gridControls, new Image[] { imgArrowknob_75 },
                        new Point(knobFilterX + knobFiltersSpacingX * 4, knobFilterY), 0, 127, 30, 330, 2);
                    filters[i].AddRotator((Int32)FilterControls.FILTER_FUNCTION, gridControls,
                        new Image[] { imgFilterOff, imgFilterFixed, imgFilterAdsrPositive, imgFilterAdsrNegative,
                            imgFilterAmModulator, imgFilterFmModulator, imgFilterPmModulator },
                        new Point(buttonFunctioFilterX, buttonFilterY));
                    filters[i].AddRotator((Int32)FilterControls.FILTER_SELECTOR, gridControls,
                        new Image[] { imgModFilterOff, imgModFilterQ, imgModFilterFreq, imgModFilterKeyFollow, imgModFilterDepth },
                        new Point(buttonModFilterX, buttonFilterY));
                    i++;
                }
            }

            i = 0;
            for (Int32 y = 0; y < LimitY(adsrCount, adsrsSkipX, adsrsSkipY); y++)
            {
                for (Int32 x = 0; x < LimitX(adsrCount, adsrsSkipX, adsrsSkipY); x++)
                {
                    adsrs[i] = Controls.AddCompoundControl(
                        new Rect(0, 0, imgBackgroundUpper.ActualWidth, imgBackgroundUpper.ActualHeight),
                        imgClickArea, i, (Int32)type.ADSR, gridControls, new Image[] { imgAdsrBackground },
                        AssembleHitarea(type.ADSR, x, y, 0, 0));
                    adsrs[i].AddKnob((Int32)AdsrControls.ADSR_A, gridControls, new Image[] { imgArrowknob_75 },
                        new Point(knobAdsrX, knobAdsrY), 0, 127, 30, 330, 2);
                    adsrs[i].AddKnob((Int32)AdsrControls.ADSR_D, gridControls, new Image[] { imgArrowknob_75 },
                        new Point(knobAdsrX + knobAdsrSpacingX, knobAdsrY), 0, 127, 30, 330, 2);
                    adsrs[i].AddKnob((Int32)AdsrControls.ADSR_S, gridControls, new Image[] { imgArrowknob_75 },
                        new Point(knobAdsrX + knobAdsrSpacingX * 2, knobAdsrY), 0, 127, 30, 330, 2);
                    adsrs[i].AddKnob((Int32)AdsrControls.ADSR_R, gridControls, new Image[] { imgArrowknob_75 },
                        new Point(knobAdsrX + knobAdsrSpacingX * 3, knobAdsrY), 0, 127, 30, 330, 2);
                    adsrs[i].AddRotator((Int32)AdsrControls.PEDAL_HOLD, gridControls,
                        new Image[] { imgPedalUp, imgPedalDown }, new Point(342, 145));
                    Graph graph = adsrs[i].AddGraph((int)AdsrControls.ADSR_GRAPH, gridControls, new Image[] { imgADSRGraphBackground },
                        new Point(14, 14), new SolidColorBrush(Windows.UI.Colors.Chartreuse), 1);
                    graph.AddPoint(new Point(1, 1));
                    graph.AddPoint(new Point(imgADSRGraphBackground.ActualWidth - 5, 1));
                    graph.AddPoint(new Point(imgADSRGraphBackground.ActualWidth - 5, imgADSRGraphBackground.ActualHeight - 5));
                    graph.AddPoint(new Point(1, imgADSRGraphBackground.ActualHeight - 5));
                    graph.AddPoint(new Point(1, 1));
                    i++;
                }
            }

            i = 0;
            for (Int32 y = 0; y < LimitY(displayCount, displaysSkipX, displaysSkipY); y++)
            {
                for (Int32 x = 0; x < LimitX(displayCount, displaysSkipX, displaysSkipY); x++)
                {
                    displays[i] = Controls.AddCompoundControl(
                        new Rect(0, 0, imgBackgroundUpper.ActualWidth, imgBackgroundUpper.ActualHeight),
                        imgClickArea, i, (Int32)type.DISPLAY, gridControls, new Image[] { imgControlPanelBackground },
                        AssembleHitarea(type.DISPLAY, x, y, 0, 0));
                    displays[i].AddGraph((Int32)DisplayControls.FREQUENCY, gridControls, new Image[] { imgGraphDisplayBackground },
                        new Point(displayDigitsBackgroundX, displayDigitsBackgroundY), color);
                    displays[i].AddGraph((Int32)DisplayControls.OSCILLOGRAPH, gridControls, new Image[] { imgGraphDisplayBackground },
                        new Point(displayOscilloscopeBackgroundX, displayOscilloscopeBackgroundY), color);
                    displays[i].AddDigitalDisplay((Int32)DisplayControls.DIGITS, gridControls,
                        new Image[] { img0, img1, img2, img3, img4, img5, img6, img7, img8, img9, img0Dot,
                            img1Dot, img2Dot, img3Dot, img4Dot, img5Dot, img6Dot, img7Dot, img8Dot, img9Dot },
                        new Point(displayDigitsX, displayDigitsY), 7, 2);
                    i++;
                }
            }

            // Make sure all controls has the correct size and position:
            Controls.ResizeControls(gridControls, Window.Current.Bounds);
            Controls.SetControlsUniform(gridControls);
            UpdateLayout();
        }

        private Int32 LimitY(Int32 count, Int32 skipX, Int32 skipY)
        {
            if (count == 1)
            {
                return 1;
            }
            else if (count == 2)
            {
                if (skipY > 0)
                {
                    return 2;
                }
                else
                {
                    return 1;
                }
            }
            else
            {
                return count / (4 - (skipX > 2 ? 3 : skipX > 0 ? 2 : 0));
            }
        }

        private Int32 LimitX(Int32 count, Int32 skipX, Int32 skipY)
        {
            if (count == 1)
            {
                return 1;
            }
            else if (count == 2)
            {
                if (skipY > 0)
                {
                    return 1;
                }
                else
                {
                    return 2;
                }
            }
            else
            {
                return 4 - (skipX > 2 ? 3 : skipX > 0 ? 2 : 0);
            }
        }

        private Rect AssembleHitarea(type type, Int32 x, Int32 y, Int32 width, Int32 height)
        {
            Rect rect = new Rect(0, 0, 0, 0);
            switch (type)
            {
                case type.OSCILLATOR:
                    rect = new Rect(
                        gridStartX + gridSpacingX * (x + oscillatorsX + oscillatorsSkipX * x),
                        gridStartY + gridSpacingY * (y + oscillatorsY),
                        width, height);
                    break;
                case type.FILTER:
                    rect = new Rect(
                        gridStartX + gridSpacingX * (x + filtersX + filtersSkipX * x),
                        gridStartY + gridSpacingY * y + filtersY * gridSpacingY,
                        width, height);
                    break;
                case type.ADSR:
                    rect = new Rect(
                        gridStartX + gridSpacingX * (x + adsrsX + adsrsSkipX * x),
                        gridStartY + gridSpacingY * y + adsrsY * gridSpacingY,
                        width, height);
                    break;
                case type.DISPLAY:
                    rect = new Rect(
                        gridStartX + gridSpacingX * (x + displaysX + displaysSkipX * x),
                        gridStartY + gridSpacingY * y + displaysY * gridSpacingY,
                        width, height);
                    break;
            }
            return rect;
        }
    }
}
