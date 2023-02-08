using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using Windows.Foundation;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace SynthLab
{
    ////////////////////////////////////////////////////////////////////////////////////////////////
    // PitchEnvelope class
    ////////////////////////////////////////////////////////////////////////////////////////////////

    public class PitchEnvelope
    {
        /// <summary>
        /// Sets/gets how much pitch is changed.
        /// Note: gets/sets pot value, but internally uses a double value = pot value / 127.
        /// </summary>
        public int Depth { get { return intDepth; } set { intDepth = value; depth = value / 127f; } }

        /// <summary>
        /// Pot value of depth
        /// </summary>
        private int intDepth;

        /// <summary>
        /// Depth used in caldulations. Is pot value / 127.
        /// </summary>
        private double depth;

        /// <summary>
        ///  The speed of progress to change pitch
        /// </summary>
        public int Speed { get { return intSpeed; } set { intSpeed = value; speed = (double)((value + 1) / 64f); } }

        /// <summary>
        /// Pot value of speed
        /// </summary>
        private int intSpeed;

        /// <summary>
        /// Speed used in caldulations. Is pot (value + 1) / 16.
        /// </summary>
        private double speed;

        /// <summary>
        /// Value from graph at current time varying from -1 to +1.
        /// </summary>
        [JsonIgnore]
        public float Value = 0.0f;

        /// <summary>
        /// List of points defining the graph.
        /// </summary>
        public List<Point> Points;

        public int PitchEnvModulationWheelTarget = 0;
        public int PitchEnvPitch = 1;
        public int PitchEnvAm = 0;
        public int PitchEnvFm = 0;
        public int PitchEnvXm = 0;


        private Brush brush = new SolidColorBrush(Windows.UI.Colors.Chartreuse);
        [JsonIgnore]
        public MainPage mainPage;
        [JsonIgnore]
        public Oscillator oscillator;
        private Rect pitchEnvelopeBounds;
        [JsonIgnore]
        public int X;
        [JsonIgnore]
        public int Y;
        private double xPosition;

        //-----------------------------------------------------------------------------------------------------
        // PitchEnvelope private
        //-----------------------------------------------------------------------------------------------------

        [JsonConstructor]
        public PitchEnvelope() { }

        public PitchEnvelope(MainPage mainPage, Oscillator oscillator)
        {
            this.mainPage = mainPage;
            this.oscillator = oscillator;
            Points = new List<Point>();
            Value = 0;
        }

        public PitchEnvelope(Oscillator oscillator, PitchEnvelope pitchEnvelope)
        {
            this.mainPage = oscillator.mainPage;
            this.oscillator = oscillator;
            Points = new List<Point>();
            foreach (Point point in pitchEnvelope.Points)
            {
                Points.Add(new Point(point.X, point.Y));
            }
            Value = 0;
            PitchEnvModulationWheelTarget = pitchEnvelope.PitchEnvModulationWheelTarget;
            Depth = pitchEnvelope.Depth;
            Speed = pitchEnvelope.Speed;
            PitchEnvPitch = pitchEnvelope.PitchEnvPitch;
            PitchEnvAm = pitchEnvelope.PitchEnvAm;
            PitchEnvFm = pitchEnvelope.PitchEnvFm;
            PitchEnvXm = pitchEnvelope.PitchEnvXm;
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////
        // PitchEnvelope funtions
        ////////////////////////////////////////////////////////////////////////////////////////////////

        public void Start()
        {
            if (Points.Count > 1)
            {
                xPosition = 0;
                Value = 0;
            }
        }

        public void Advance()
        {
            if (Points.Count > 1)
            {
                xPosition++;
                CalculateValue();
            }
        }

        public void CalculateValue()
        {
            if (Points.Count > 1)
            {
                try
                { 
                    int beginning = (int)Points[0].X;
                    int end = (int)Points[Points.Count - 1].X;
                    int length = end - beginning;
                    xPosition += Math.Round(speed);
                    xPosition = xPosition > length ? length : xPosition;

                    int p1 = 0;
                    int p2 = Points.Count - 1;
                    try
                    {
                        while (p1 < Points.Count && Points[p1].X <= xPosition)
                        {
                            p1++;
                        }
                    }
                    catch (Exception exception)
                    {
                        ContentDialog error = new Message(exception.Message);
                        _ = error.ShowAsync();
                    }
                    p1--;
                    try
                    {
                        while (p2 > p1 + 1)
                        {
                            p2--;
                        }
                    }
                    catch (Exception exception)
                    {
                        ContentDialog error = new Message(exception.Message);
                        _ = error.ShowAsync();
                    }
                    if (p2 < Points.Count && p1 == p2 - 1 && Points[p2].X - Points[p1].X > 0)
                    {
                        double xFraction = (xPosition - Points[p1].X) / (Points[p2].X - Points[p1].X);
                        double y1 = pitchEnvelopeBounds.Height / 2 - Points[p1].Y;
                        double y2 = pitchEnvelopeBounds.Height / 2 - Points[p2].Y;
                        Value = (float)((y1 + (y2 - y1) * xFraction) * depth / pitchEnvelopeBounds.Height * 2);
                    }
                    else if (Math.Abs(Value) < 0.05)
                    {
                        Value = 0;
                    }
                }
                catch (Exception exception)
                {
                    ContentDialog error = new Message(exception.Message);
                    Value = 0;
                    _ = error.ShowAsync();
                }
            }
            else
            {
                Value = 0;
                return;
            }
        }

        public void SetBounds(Rect rect)
        {
            pitchEnvelopeBounds = new Rect(rect.X, rect.Y, rect.Width, rect.Height);
        }

        public void CopyPoints(List<Point> PointsToCopy)
        {
            Points.Clear();
            foreach (Point point in PointsToCopy)
            {
                Points.Add(new Point(point.X, point.Y));
            }
        }
    }
}
