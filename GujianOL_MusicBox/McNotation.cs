namespace GujianOL_MusicBox
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Windows.Forms;

    public class McNotation
    {
        private string _author = "？？？？";
        private McNotePack.DurationTypes _beatDurationType = McNotePack.DurationTypes.Quarter;
        private int _beatsPerMeasure = 4;
        private readonly MusicCanvasControl _canvas;
        private string _creator = "？？？？";
        private readonly SortedList<int, int> _measureBeatsPerMinuteMap = new SortedList<int, int>();
        private string _name = "？？？？";
        private NumberedKeySignatureTypes _numberedKeySignature = NumberedKeySignatureTypes.C;
        private readonly List<McRegularTrack> _regularTrackList = new List<McRegularTrack>();
        private bool _regularTracksMLock = true;
        private string _translater = "？？？？";
        private float _volume = 1f;
        public static readonly int BeatsPerMeasureMax = 8;
        public static readonly int BeatsPerMeasureMin = 2;
        public static readonly int BeatsPerMinuteMax = 180;
        public static readonly int BeatsPerMinuteMin = 20;
        public static readonly Padding Margin = new Padding(8, 8, 0, 0);
        public const int RegularTracksMax = 3;

        public McNotation(MusicCanvasControl canvas)
        {
            this._canvas = canvas;
            this.TemporaryInfo = new NtTemporaryInfo(this);
            for (int i = 0; i < 3; i++)
            {
                this._regularTrackList.Add(new McRegularTrack(this));
            }
        }

        public void ClearBitmapCache()
        {
            for (int i = 0; i < 3; i++)
            {
                this._regularTrackList[i].ClearBitmapCache();
            }
        }

        public void ClearMeasureBeatsPerMinute(int measureIndex)
        {
            if ((measureIndex >= 0) && this._measureBeatsPerMinuteMap.ContainsKey(measureIndex))
            {
                this._measureBeatsPerMinuteMap.Remove(measureIndex);
                McUtility.MarkModified(null);
            }
        }

        public int GetMeasureBeatsPerMinute(int measureIndex)
        {
            int[] numArray = this._measureBeatsPerMinuteMap.Keys.ToArray<int>();
            if ((measureIndex < 0) || (numArray.Length <= 0))
            {
                return 80;
            }
            int num = -1;
            for (int i = numArray.Length - 1; i >= 0; i--)
            {
                int num3 = numArray[i];
                if (measureIndex >= num3)
                {
                    num = num3;
                    break;
                }
            }
            return ((num < 0) ? 80 : this._measureBeatsPerMinuteMap[num]);
        }

        public int GetMeasureDurationAligned(int measureIndex)
        {
            int num = 0;
            McMeasure measure = this._regularTrackList.First<McRegularTrack>().GetMeasure(measureIndex);
            if (measure != null)
            {
                foreach (McMeasure measure2 in measure.MeasuresAligned)
                {
                    if (measure2 != null)
                    {
                        int measureDuration = measure2.MeasureDuration;
                        if (measureDuration > num)
                        {
                            num = measureDuration;
                        }
                    }
                }
            }
            return num;
        }

        public int IndexOf(McRegularTrack track) => 
            this._regularTrackList.IndexOf(track);

        public int InsertMeasureAligned(int? measureIndex = new int?())
        {
            int num = -1;
            McMeasure[] measuresAligned = new McMeasure[3];
            this._regularTracksMLock = false;
            for (int i = 0; i < 3; i++)
            {
                num = this._regularTrackList[i]._InsertMeasure(measureIndex, out measuresAligned[i]);
            }
            foreach (McMeasure measure in measuresAligned)
            {
                measure._ResetMeasuresAligned(measuresAligned);
                McMeasure preMeasure = measure.PreMeasure;
                if (preMeasure != null)
                {
                    measure.TemporaryInfo.RelativeX = (preMeasure.TemporaryInfo.RelativeX + preMeasure.TemporaryInfo.Width) + Math.Max(McMeasure.Margin.Right, McMeasure.Margin.Left);
                }
                measure.ReorganizeDurationStamps();
            }
            int[] numArray = this._measureBeatsPerMinuteMap.Keys.ToArray<int>();
            for (int j = numArray.Length - 1; j >= 0; j--)
            {
                int key = numArray[j];
                if (key >= measureIndex)
                {
                    this._measureBeatsPerMinuteMap.Add(key + 1, this._measureBeatsPerMinuteMap[key]);
                    this._measureBeatsPerMinuteMap.Remove(key);
                }
            }
            this._regularTracksMLock = true;
            return num;
        }

        public int? RawGetMeasureBeatsPerMinute(int measureIndex)
        {
            if (this._measureBeatsPerMinuteMap.ContainsKey(measureIndex))
            {
                return new int?(this._measureBeatsPerMinuteMap[measureIndex]);
            }
            return null;
        }

        public bool RemoveMeasureAligned(int measureIndex)
        {
            bool flag = true;
            this._regularTracksMLock = false;
            foreach (McRegularTrack track in this._regularTrackList)
            {
                flag = track._RemoveMeasure(measureIndex) && flag;
            }
            if (flag)
            {
                foreach (int num in this._measureBeatsPerMinuteMap.Keys.ToArray<int>())
                {
                    if (num == measureIndex)
                    {
                        this._measureBeatsPerMinuteMap.Remove(num);
                    }
                    else if (num > measureIndex)
                    {
                        this._measureBeatsPerMinuteMap.Add(num - 1, this._measureBeatsPerMinuteMap[num]);
                        this._measureBeatsPerMinuteMap.Remove(num);
                    }
                }
            }
            this._regularTracksMLock = true;
            return flag;
        }

        public void ReorganizeAlignedMesaureurationStamps(int measureIndex, bool includeContext)
        {
            if ((measureIndex >= 0) && (measureIndex < this.MeasureCountAligned))
            {
                McMeasure measure = this.RegularTracks.First<McRegularTrack>().GetMeasure(measureIndex);
                if (measure != null)
                {
                    foreach (McMeasure measure2 in measure.MeasuresAligned)
                    {
                        if (includeContext && (measure2.PreMeasure != null))
                        {
                            measure2.PreMeasure.ReorganizeDurationStamps();
                        }
                        measure2.ReorganizeDurationStamps();
                        if (includeContext && (measure2.NextMeasure != null))
                        {
                            measure2.NextMeasure.ReorganizeDurationStamps();
                        }
                        McUtility.MarkModified(measure2);
                    }
                }
            }
        }

        public void ResetMeasureBeatsPerMinuteToDefault()
        {
            this.SetMeasureBeatsPerMinute(0, 80);
        }

        public void SetMeasureBeatsPerMinute(int measureIndex, int bpm)
        {
            if (measureIndex >= 0)
            {
                bpm = bpm.Clamp(BeatsPerMinuteMin, BeatsPerMinuteMax);
                if ((measureIndex > 0) && (bpm == this.GetMeasureBeatsPerMinute(measureIndex - 1)))
                {
                    this.ClearMeasureBeatsPerMinute(measureIndex);
                }
                else
                {
                    if (this._measureBeatsPerMinuteMap.ContainsKey(measureIndex))
                    {
                        this._measureBeatsPerMinuteMap[measureIndex] = bpm;
                    }
                    else
                    {
                        this._measureBeatsPerMinuteMap.Add(measureIndex, bpm);
                    }
                    McUtility.MarkModified(null);
                }
            }
        }

        public string Author
        {
            get => 
                this._author;
            set
            {
                this._author = value.FormatNotationText();
            }
        }

        public int BeatDuration =>
            ((int) this.BeatDurationType);

        public McNotePack.DurationTypes BeatDurationType
        {
            get => 
                this._beatDurationType;
            set
            {
                switch (value)
                {
                    case McNotePack.DurationTypes.Eighth:
                    case McNotePack.DurationTypes.Quarter:
                    case McNotePack.DurationTypes.Half:
                        this._beatDurationType = value;
                        McUtility.MarkModified(null);
                        break;
                }
            }
        }

        public int BeatsPerMeasure
        {
            get => 
                this._beatsPerMeasure;
            set
            {
                this._beatsPerMeasure = value.Clamp(BeatsPerMeasureMin, BeatsPerMeasureMax);
                McUtility.MarkModified(null);
            }
        }

        public MusicCanvasControl Canvas =>
            this._canvas;

        public string Creator
        {
            get => 
                this._creator;
            set
            {
                this._creator = value.FormatNotationText();
            }
        }

        public int DrawingWidth
        {
            get
            {
                McMeasure last = this._regularTrackList[0].Last;
                return (last?.TemporaryInfo.RelativeX + last?.TemporaryInfo.Width);
            }
        }

        public SortedList<int, int> MeasureBeatsPerMinuteMap =>
            this._measureBeatsPerMinuteMap;

        public int MeasureCountAligned
        {
            get
            {
                McRegularTrack track = this._regularTrackList.First<McRegularTrack>();
                return ((track == null) ? 0 : track.MeasuresCount);
            }
        }

        public string Name
        {
            get => 
                this._name;
            set
            {
                this._name = value.FormatNotationText();
            }
        }

        public NumberedKeySignatureTypes NumberedKeySignature
        {
            get => 
                this._numberedKeySignature;
            set
            {
                this._numberedKeySignature = value;
                McUtility.MarkModified(null);
            }
        }

        public McRegularTrack[] RegularTracks =>
            this._regularTrackList.ToArray();

        public int RegularTracksCount =>
            this._regularTrackList.Count;

        public bool RegularTracksMLock =>
            this._regularTracksMLock;

        public NtTemporaryInfo TemporaryInfo { get; private set; }

        public string Translater
        {
            get => 
                this._translater;
            set
            {
                this._translater = value.FormatNotationText();
            }
        }

        public float Volume
        {
            get => 
                this._volume;
            set
            {
                this._volume = value.Clamp(0f, 1f);
                McUtility.MarkModified(null);
            }
        }

        public class DurationStamp
        {
            private readonly McNotePack[] _notePackInTracks;

            public DurationStamp(McMeasure measure, int stampIndex, McNotePack[] notePackInTracks = null)
            {
                this.Measure = measure;
                this.StampIndex = stampIndex;
                this._notePackInTracks = new McNotePack[3];
                for (int i = 0; i < this._notePackInTracks.Length; i++)
                {
                    this._notePackInTracks[i] = ((notePackInTracks == null) || (i >= notePackInTracks.Length)) ? null : notePackInTracks[i];
                }
            }

            public McNotePack GetAnyValidNotePack() => 
                this._notePackInTracks.FirstOrDefault<McNotePack>(t => (t != null));

            public McNotePack GetNotePack(int trackIndex)
            {
                if ((trackIndex < 0) || (trackIndex >= this._notePackInTracks.Length))
                {
                    return null;
                }
                return this._notePackInTracks[trackIndex];
            }

            public bool RemoveNotePack(int trackIndex)
            {
                if ((trackIndex < 0) || (trackIndex >= this._notePackInTracks.Length))
                {
                    return false;
                }
                this._notePackInTracks[trackIndex] = null;
                SortedDictionary<int, McNotation.DurationStamp> reorganizedDurationStamps = this.Measure.TemporaryInfo.ReorganizedDurationStamps;
                if (this.IsEmpty && reorganizedDurationStamps.ContainsKey(this.StampIndex))
                {
                    reorganizedDurationStamps.Remove(this.StampIndex);
                }
                return true;
            }

            public bool SetNotePack(int trackIndex, McNotePack note)
            {
                if ((trackIndex < 0) || (trackIndex >= this._notePackInTracks.Length))
                {
                    return false;
                }
                this._notePackInTracks[trackIndex] = note;
                foreach (McNotePack pack in this._notePackInTracks)
                {
                    if (pack != null)
                    {
                        pack.TemporaryInfo.StampIndex = this.StampIndex;
                    }
                }
                return true;
            }

            public bool HasAnyVaildNotePack =>
                (this.VaildNotePackCount > 0);

            public bool IsEmpty =>
                this._notePackInTracks.All<McNotePack>(note => (note == null));

            public McMeasure Measure { get; private set; }

            public int StampIndex { get; private set; }

            public int VaildNotePackCount =>
                this._notePackInTracks.Count<McNotePack>(note => (note != null));
        }

        public class NtTemporaryInfo
        {
            public NtTemporaryInfo(McNotation parentNotation)
            {
                this.ParentNotation = parentNotation;
            }

            public void ReorganizeAllDurationStamps()
            {
                McNotation parentNotation = this.ParentNotation;
                int regularTracksCount = parentNotation.RegularTracksCount;
                McRegularTrack track = parentNotation.RegularTracks.First<McRegularTrack>();
                int num2 = 0;
                for (int i = 0; i < parentNotation.MeasureCountAligned; i++)
                {
                    McMeasure measure = track.GetMeasure(i);
                    McMeasure[] measuresAligned = measure.MeasuresAligned;
                    for (int j = 0; j < regularTracksCount; j++)
                    {
                        measuresAligned[j].ReorganizeDurationStamps();
                    }
                    McMeasure.MsAlignedTemporaryInfo temporaryInfo = measure.TemporaryInfo;
                    temporaryInfo.RelativeX = num2;
                    num2 += temporaryInfo.Width + Math.Max(McMeasure.Margin.Right, McMeasure.Margin.Left);
                }
            }

            public MusicCanvasControl Canvas =>
                this.ParentNotation.Canvas;

            public McNotation ParentNotation { get; private set; }
        }

        public enum NumberedKeySignatureTypes
        {
            C,
            bD,
            D,
            bE,
            E,
            F,
            bG,
            G,
            bA,
            A,
            bB,
            B
        }
    }
}

