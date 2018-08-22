namespace GujianOL_MusicBox
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    public class Arpeggio
    {
        public static List<Arpeggio> ActivatedArpeggios = new List<Arpeggio>();

        private Arpeggio(McNotePack notePack, float intervalSpeedScale = 2f)
        {
            this.NotePack = notePack;
            this.ElapsedTimeMs = 0;
            this.PlayedPitchCount = 1;
            this.IntervalSpeedScale = intervalSpeedScale.Clamp(0.5f, 4f);
        }

        public static Arpeggio AppendArpeggio(McNotePack notePack, float intervalSpeedScale = 2f)
        {
            McPitch[] validPitchArray = notePack.ValidPitchArray;
            switch (notePack.ArpeggioMode)
            {
                case ArpeggioTypes.Upward:
                    PlayPitch(validPitchArray.First<McPitch>());
                    break;

                case ArpeggioTypes.Downward:
                    PlayPitch(validPitchArray.Last<McPitch>());
                    break;

                case ArpeggioTypes.None:
                    foreach (McPitch pitch in validPitchArray)
                    {
                        PlayPitch(pitch);
                    }
                    return null;
            }
            Arpeggio item = new Arpeggio(notePack, intervalSpeedScale);
            ActivatedArpeggios.Add(item);
            return item;
        }

        public static int CalculateInterval(McNotePack notePack, float intervalSpeedScale = 2f)
        {
            if ((notePack == null) || (notePack.ArpeggioMode == ArpeggioTypes.None))
            {
                return 0;
            }
            int num = (int) (((1000f / intervalSpeedScale) * 60f) / ((float) notePack.ParentMeasure.BeatsPerMinute));
            return (num / notePack.ValidPitchCount.Clamp(4, 8));
        }

        public static void EffectArpeggios(int elapsedTimeMs)
        {
            foreach (Arpeggio arpeggio in ActivatedArpeggios.ToArray())
            {
                arpeggio.ElapsedTimeMs += elapsedTimeMs;
                if (arpeggio.ElapsedTimeMs >= (arpeggio.PlayedPitchCount * arpeggio.Interval))
                {
                    int num = arpeggio.PlayedPitchCount + 1;
                    McPitch[] validPitchArray = arpeggio.NotePack.ValidPitchArray;
                    McPitch pitch = null;
                    switch (arpeggio.NotePack.ArpeggioMode)
                    {
                        case ArpeggioTypes.Upward:
                            if ((num > 1) && (num <= validPitchArray.Length))
                            {
                                pitch = validPitchArray[num - 1];
                            }
                            break;

                        case ArpeggioTypes.Downward:
                            if ((num > 1) && (num <= validPitchArray.Length))
                            {
                                pitch = validPitchArray[validPitchArray.Length - num];
                            }
                            break;
                    }
                    if (pitch != null)
                    {
                        PlayPitch(pitch);
                        arpeggio.PlayedPitchCount = num;
                    }
                    if (num > validPitchArray.Length)
                    {
                        ActivatedArpeggios.Remove(arpeggio);
                    }
                }
            }
        }

        public static void PlayPitch(McPitch pitch)
        {
            if (pitch.Volume >= 0.001f)
            {
                McNotePack parentNotePack = pitch.ParentNotePack;
                MusicCanvasControl canvas = parentNotePack.Canvas;
                McMeasure parentMeasure = parentNotePack.ParentMeasure;
                float num = ((parentMeasure.TemporaryInfo.RelativeX + McNotation.Margin.Left) + McRegularTrack.Margin.Left) - canvas.ScrollXSmooth;
                float num2 = ((((McRegularTrack.Margin.Bottom + McRegularTrack.Margin.Top) + McMeasure.Height) * parentMeasure.ParentRegularTrack.Index) + McNotation.Margin.Top) + McRegularTrack.Margin.Top;
                float x = (num + parentNotePack.TemporaryInfo.RelativeXSmooth) + 8f;
                if (parentNotePack.Staccato)
                {
                    canvas.PlaySound(parentMeasure.InstrumentType, pitch.AlterantValue, pitch.Volume, (long) parentNotePack.TemporaryInfo.PlayingDurationTimeMs, 0.03f);
                }
                else
                {
                    canvas.PlaySound(parentMeasure.InstrumentType, pitch.AlterantValue, pitch.Volume, (long) parentNotePack.TemporaryInfo.PlayingDurationTimeMs, 0.03f);
                }
                int num4 = parentNotePack.IsRest ? (((parentMeasure.ClefType == McMeasure.ClefTypes.L2G) ? 0x2c : 0x17) + 5) : pitch.Value;
                float measureLineRelativeY = pitch.MeasureLineRelativeY;
                float num6 = 3.141593f * (((float) num4) / 88f);
                Color color = parentNotePack.IsRest ? Color.FromArgb(0x80, Color.White) : Color.FromArgb(0xff, (int) (((Math.Cos((double) num6) + 1.0) / 2.0) * 255.0), (int) (((Math.Sin((double) num6) + 1.0) / 2.0) * 255.0), (int) (((Math.Cos((double) num6) + 1.0) / 2.0) * 255.0));
                MusicCanvasControl.RippleAniManager.Append(x, num2 + measureLineRelativeY, color, 10 + ((int) (30f * parentNotePack.Volume)), 0.5f, parentNotePack);
            }
        }

        public int ArpeggioPeriodTimeMs =>
            ((int) (((1000f / this.IntervalSpeedScale) * 60f) / ((float) this.NotePack.ParentMeasure.BeatsPerMinute)));

        public MusicCanvasControl Canvas =>
            this.NotePack.Canvas;

        public int ElapsedTimeMs { get; private set; }

        public int Interval =>
            (((this.NotePack == null) || (this.NotePack.ArpeggioMode == ArpeggioTypes.None)) ? 0 : (this.ArpeggioPeriodTimeMs / this.NotePack.ValidPitchCount.Clamp(4, 8)));

        public float IntervalSpeedScale { get; private set; }

        public McNotePack NotePack { get; private set; }

        public int PlayedPitchCount { get; private set; }

        public enum ArpeggioTypes
        {
            Upward,
            Downward,
            None
        }
    }
}

