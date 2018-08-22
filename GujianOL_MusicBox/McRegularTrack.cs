namespace GujianOL_MusicBox
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Windows.Forms;

    public class McRegularTrack
    {
        private readonly SortedList<int, McMeasure.ClefTypes> _measureClefTypeMap = new SortedList<int, McMeasure.ClefTypes>();
        private readonly SortedList<int, McMeasure.InstrumentTypes> _measureInstrumentTypeMap = new SortedList<int, McMeasure.InstrumentTypes>();
        private readonly SortedList<int, int> _measureKeySignatureMap = new SortedList<int, int>();
        private readonly List<McMeasure> _measureList = new List<McMeasure>();
        private readonly SortedList<int, VolumeCurve> _measureVolumeCurveMap = new SortedList<int, VolumeCurve>();
        private readonly SortedList<int, float> _measureVolumeMap = new SortedList<int, float>();
        public static readonly Padding Margin = new Padding(8, 1, 8, 0);

        public McRegularTrack(McNotation parentNotation)
        {
            this.ParentNotation = parentNotation;
            if (parentNotation != null)
            {
                McUtility.MarkModified(null);
            }
        }

        public int _InsertMeasure(int? measureIndex, out McMeasure newMeasure)
        {
            int num;
            int num2;
            int? nullable = measureIndex;
            measureIndex = new int?(nullable.HasValue ? nullable.GetValueOrDefault() : 0x5f5e0ff);
            if (this.ParentNotation.RegularTracksMLock || (((nullable = measureIndex).GetValueOrDefault() < 0) && nullable.HasValue))
            {
                newMeasure = null;
                return -1;
            }
            nullable = measureIndex;
            int measuresCount = this.MeasuresCount;
            measureIndex = ((nullable.GetValueOrDefault() >= measuresCount) && nullable.HasValue) ? new int?(this.MeasuresCount) : measureIndex;
            newMeasure = new McMeasure(this);
            this._measureList.Insert(measureIndex.Value, newMeasure);
            int[] numArray = this._measureClefTypeMap.Keys.ToArray<int>();
            for (num = numArray.Length - 1; num >= 0; num--)
            {
                num2 = numArray[num];
                measuresCount = num2;
                if (measuresCount >= measureIndex)
                {
                    this._measureClefTypeMap.Add(num2 + 1, this._measureClefTypeMap[num2]);
                    this._measureClefTypeMap.Remove(num2);
                }
            }
            numArray = this._measureInstrumentTypeMap.Keys.ToArray<int>();
            for (num = numArray.Length - 1; num >= 0; num--)
            {
                num2 = numArray[num];
                measuresCount = num2;
                if (measuresCount >= measureIndex)
                {
                    this._measureInstrumentTypeMap.Add(num2 + 1, this._measureInstrumentTypeMap[num2]);
                    this._measureInstrumentTypeMap.Remove(num2);
                }
            }
            numArray = this._measureKeySignatureMap.Keys.ToArray<int>();
            for (num = numArray.Length - 1; num >= 0; num--)
            {
                num2 = numArray[num];
                measuresCount = num2;
                if (measuresCount >= measureIndex)
                {
                    this._measureKeySignatureMap.Add(num2 + 1, this._measureKeySignatureMap[num2]);
                    this._measureKeySignatureMap.Remove(num2);
                }
            }
            numArray = this._measureVolumeCurveMap.Keys.ToArray<int>();
            for (num = numArray.Length - 1; num >= 0; num--)
            {
                num2 = numArray[num];
                measuresCount = num2;
                if (measuresCount >= measureIndex)
                {
                    this._measureVolumeCurveMap.Add(num2 + 1, this._measureVolumeCurveMap[num2]);
                    this._measureVolumeCurveMap.Remove(num2);
                }
            }
            numArray = this._measureVolumeMap.Keys.ToArray<int>();
            for (num = numArray.Length - 1; num >= 0; num--)
            {
                num2 = numArray[num];
                measuresCount = num2;
                if (measuresCount >= measureIndex)
                {
                    this._measureVolumeMap.Add(num2 + 1, this._measureVolumeMap[num2]);
                    this._measureVolumeMap.Remove(num2);
                }
            }
            return measureIndex.Value;
        }

        public bool _RemoveMeasure(int measureIndex)
        {
            if (this.ParentNotation.RegularTracksMLock)
            {
                return false;
            }
            if ((measureIndex < 0) || (measureIndex >= this.MeasuresCount))
            {
                return false;
            }
            McMeasure measure = this._measureList[measureIndex];
            measure.TemporaryInfo.Width = -Math.Max(McMeasure.Margin.Right, McMeasure.Margin.Left) - 1;
            measure.ClearBitmapCache();
            this._measureList.RemoveAt(measureIndex);
            foreach (int num in this._measureClefTypeMap.Keys.ToArray<int>())
            {
                if (num == measureIndex)
                {
                    this._measureClefTypeMap.Remove(num);
                }
                else if (num >= measureIndex)
                {
                    this._measureClefTypeMap.Add(num - 1, this._measureClefTypeMap[num]);
                    this._measureClefTypeMap.Remove(num);
                }
            }
            foreach (int num in this._measureInstrumentTypeMap.Keys.ToArray<int>())
            {
                if (num == measureIndex)
                {
                    this._measureInstrumentTypeMap.Remove(num);
                }
                else if (num >= measureIndex)
                {
                    this._measureInstrumentTypeMap.Add(num - 1, this._measureInstrumentTypeMap[num]);
                    this._measureInstrumentTypeMap.Remove(num);
                }
            }
            foreach (int num in this._measureKeySignatureMap.Keys.ToArray<int>())
            {
                if (num == measureIndex)
                {
                    this._measureKeySignatureMap.Remove(num);
                }
                else if (num >= measureIndex)
                {
                    this._measureKeySignatureMap.Add(num - 1, this._measureKeySignatureMap[num]);
                    this._measureKeySignatureMap.Remove(num);
                }
            }
            foreach (int num in this._measureVolumeCurveMap.Keys.ToArray<int>())
            {
                if (num == measureIndex)
                {
                    this._measureVolumeCurveMap.Remove(num);
                }
                else if (num >= measureIndex)
                {
                    this._measureVolumeCurveMap.Add(num - 1, this._measureVolumeCurveMap[num]);
                    this._measureVolumeCurveMap.Remove(num);
                }
            }
            foreach (int num in this._measureVolumeMap.Keys.ToArray<int>())
            {
                if (num == measureIndex)
                {
                    this._measureVolumeMap.Remove(num);
                }
                else if (num >= measureIndex)
                {
                    this._measureVolumeMap.Add(num - 1, this._measureVolumeMap[num]);
                    this._measureVolumeMap.Remove(num);
                }
            }
            return true;
        }

        public void ClearBitmapCache()
        {
            for (int i = 0; i < this._measureList.Count; i++)
            {
                this._measureList[i].ClearBitmapCache();
            }
        }

        public void ClearMeasureClefType(int measureIndex)
        {
            if ((measureIndex > 0) && this._measureClefTypeMap.ContainsKey(measureIndex))
            {
                this._measureClefTypeMap.Remove(measureIndex);
                McUtility.MarkModified(this.GetMeasure(measureIndex));
            }
        }

        public void ClearMeasureInstrumentType(int measureIndex)
        {
            if ((measureIndex > 0) && this._measureInstrumentTypeMap.ContainsKey(measureIndex))
            {
                this._measureInstrumentTypeMap.Remove(measureIndex);
                McUtility.MarkModified(this.GetMeasure(measureIndex));
            }
        }

        public void ClearMeasureKeySignature(int measureIndex)
        {
            if ((measureIndex > 0) && this._measureKeySignatureMap.ContainsKey(measureIndex))
            {
                this._measureKeySignatureMap.Remove(measureIndex);
                foreach (McRegularTrack track in this.ParentNotation.RegularTracks)
                {
                    McUtility.MarkModified(track.GetMeasure(measureIndex));
                }
            }
        }

        public void ClearMeasureVolume(int measureIndex)
        {
            if ((measureIndex > 0) && this._measureVolumeMap.ContainsKey(measureIndex))
            {
                this._measureVolumeMap.Remove(measureIndex);
                McUtility.MarkModified(this.GetMeasure(measureIndex));
            }
        }

        public void ClearMeasureVolumeCurve(int measureIndex)
        {
            if ((measureIndex > 0) && this._measureVolumeCurveMap.ContainsKey(measureIndex))
            {
                this._measureVolumeCurveMap.Remove(measureIndex);
                McUtility.MarkModified(this.GetMeasure(measureIndex));
            }
        }

        public McMeasure GetMeasure(int index)
        {
            if ((index < 0) || (index >= this._measureList.Count))
            {
                return null;
            }
            return this._measureList[index];
        }

        public McMeasure.ClefTypes GetMeasureClefType(int measureIndex)
        {
            int[] numArray = this._measureClefTypeMap.Keys.ToArray<int>();
            if ((measureIndex < 0) || (numArray.Length <= 0))
            {
                return McMeasure.ClefTypes.L2G;
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
            return ((num < 0) ? McMeasure.ClefTypes.L2G : this._measureClefTypeMap[num]);
        }

        public McMeasure.InstrumentTypes GetMeasureInstrumentType(int measureIndex)
        {
            int[] numArray = this._measureInstrumentTypeMap.Keys.ToArray<int>();
            if ((measureIndex < 0) || (numArray.Length <= 0))
            {
                return McMeasure.InstrumentTypes.Piano;
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
            return ((num < 0) ? McMeasure.InstrumentTypes.Piano : this._measureInstrumentTypeMap[num]);
        }

        public int GetMeasureKeySignature(int measureIndex)
        {
            int[] numArray = this._measureKeySignatureMap.Keys.ToArray<int>();
            if ((measureIndex < 0) || (numArray.Length <= 0))
            {
                return 0;
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
            return ((num < 0) ? 0 : this._measureKeySignatureMap[num]);
        }

        public float GetMeasureVolume(int measureIndex)
        {
            int[] numArray = this._measureVolumeMap.Keys.ToArray<int>();
            if ((measureIndex < 0) || (numArray.Length <= 0))
            {
                return 1f;
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
            return ((num < 0) ? 1f : this._measureVolumeMap[num]);
        }

        public VolumeCurve GetMeasureVolumeCurve(int measureIndex)
        {
            int[] numArray = this._measureVolumeCurveMap.Keys.ToArray<int>();
            if ((measureIndex < 0) || (numArray.Length <= 0))
            {
                return VolumeCurve.Default;
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
            return ((num < 0) ? VolumeCurve.Default : this._measureVolumeCurveMap[num]);
        }

        public int IndexOf(McMeasure measure) => 
            this._measureList.IndexOf(measure);

        public McMeasure.ClefTypes RawGetMeasureClefType(int measureIndex) => 
            (this._measureClefTypeMap.ContainsKey(measureIndex) ? this._measureClefTypeMap[measureIndex] : McMeasure.ClefTypes.Invaild);

        public McMeasure.InstrumentTypes RawGetMeasureInstrumentType(int measureIndex) => 
            (this._measureInstrumentTypeMap.ContainsKey(measureIndex) ? this._measureInstrumentTypeMap[measureIndex] : McMeasure.InstrumentTypes.Invaild);

        public int? RawGetMeasureKeySignature(int measureIndex)
        {
            if (this._measureKeySignatureMap.ContainsKey(measureIndex))
            {
                return new int?(this._measureKeySignatureMap[measureIndex]);
            }
            return null;
        }

        public float RawGetMeasureVolume(int measureIndex) => 
            (this._measureVolumeMap.ContainsKey(measureIndex) ? this._measureVolumeMap[measureIndex] : -1f);

        public VolumeCurve RawGetMeasureVolumeCurve(int measureIndex) => 
            (this._measureVolumeCurveMap.ContainsKey(measureIndex) ? this._measureVolumeCurveMap[measureIndex] : null);

        public void ResetMeasureClefTypeToDefault()
        {
            switch (this.Index)
            {
                case 0:
                    this.SetMeasureClefType(0, McMeasure.ClefTypes.L2G);
                    break;

                case 1:
                    this.SetMeasureClefType(0, McMeasure.ClefTypes.L4F);
                    break;

                case 2:
                    this.SetMeasureClefType(0, McMeasure.ClefTypes.L4F);
                    break;
            }
        }

        public void ResetMeasureInstrumentTypeToDefault()
        {
            switch (this.Index)
            {
                case 0:
                    this.SetMeasureInstrumentType(0, McMeasure.InstrumentTypes.Piano);
                    break;

                case 1:
                    this.SetMeasureInstrumentType(0, McMeasure.InstrumentTypes.Piano);
                    break;

                case 2:
                    this.SetMeasureInstrumentType(0, McMeasure.InstrumentTypes.Misc);
                    break;
            }
        }

        public void ResetMeasureKeySignatureToDefault()
        {
            this.SetMeasureKeySignature(0, 0);
        }

        public void ResetMeasureVolumeCurveToDefault()
        {
            VolumeCurve volumeCurve = new VolumeCurve();
            switch (this.Index)
            {
                case 2:
                    volumeCurve.SetCurvedVolume(0, 0.7f);
                    volumeCurve.SetCurvedVolume(0, 0.7f);
                    volumeCurve.SetCurvedVolume(0, 0.6f);
                    volumeCurve.SetCurvedVolume(0, 0.6f);
                    volumeCurve.SetCurvedVolume(0, 0.7f);
                    volumeCurve.SetCurvedVolume(0, 0.7f);
                    volumeCurve.SetCurvedVolume(0, 0.5f);
                    volumeCurve.SetCurvedVolume(0, 0.5f);
                    break;
            }
            this.SetMeasureVolumeCurve(0, volumeCurve);
        }

        public void ResetMeasureVolumeToDefault()
        {
            switch (this.Index)
            {
                case 0:
                    this.SetMeasureVolume(0, 0.9f);
                    break;

                case 1:
                    this.SetMeasureVolume(0, 0.7f);
                    break;

                case 2:
                    this.SetMeasureVolume(0, 0.8f);
                    break;
            }
        }

        public void SetMeasureClefType(int measureIndex, McMeasure.ClefTypes cleftype)
        {
            if (measureIndex >= 0)
            {
                if ((measureIndex > 0) && (cleftype == this.GetMeasureClefType(measureIndex - 1)))
                {
                    this.ClearMeasureClefType(measureIndex);
                }
                else
                {
                    if (this._measureClefTypeMap.ContainsKey(measureIndex))
                    {
                        this._measureClefTypeMap[measureIndex] = cleftype;
                    }
                    else
                    {
                        this._measureClefTypeMap.Add(measureIndex, cleftype);
                    }
                    McUtility.MarkModified(this.GetMeasure(measureIndex));
                }
            }
        }

        public void SetMeasureInstrumentType(int measureIndex, McMeasure.InstrumentTypes instrumentType)
        {
            if (measureIndex >= 0)
            {
                if ((measureIndex > 0) && (instrumentType == this.GetMeasureInstrumentType(measureIndex - 1)))
                {
                    this.ClearMeasureInstrumentType(measureIndex);
                }
                else
                {
                    if (this._measureInstrumentTypeMap.ContainsKey(measureIndex))
                    {
                        this._measureInstrumentTypeMap[measureIndex] = instrumentType;
                    }
                    else
                    {
                        this._measureInstrumentTypeMap.Add(measureIndex, instrumentType);
                    }
                    McUtility.MarkModified(this.GetMeasure(measureIndex));
                }
            }
        }

        public void SetMeasureKeySignature(int measureIndex, int keySignature)
        {
            if (measureIndex >= 0)
            {
                keySignature = keySignature.Clamp(McMeasure.KeySignatureMin, McMeasure.KeySignatureMax);
                if ((measureIndex > 0) && (keySignature == this.GetMeasureKeySignature(measureIndex - 1)))
                {
                    this.ClearMeasureKeySignature(measureIndex);
                }
                else
                {
                    if (this._measureKeySignatureMap.ContainsKey(measureIndex))
                    {
                        this._measureKeySignatureMap[measureIndex] = keySignature;
                    }
                    else
                    {
                        this._measureKeySignatureMap.Add(measureIndex, keySignature);
                    }
                    McUtility.MarkModified(this.GetMeasure(measureIndex));
                }
            }
        }

        public void SetMeasureVolume(int measureIndex, float volume)
        {
            if (measureIndex >= 0)
            {
                volume = volume.Clamp(0f, 1f);
                if ((measureIndex > 0) && (Math.Abs((float) (this.GetMeasureVolume(measureIndex - 1) - volume)) < 0.01))
                {
                    this.ClearMeasureVolume(measureIndex);
                }
                else
                {
                    if (this._measureVolumeMap.ContainsKey(measureIndex))
                    {
                        this._measureVolumeMap[measureIndex] = volume;
                    }
                    else
                    {
                        this._measureVolumeMap.Add(measureIndex, volume);
                    }
                    McUtility.MarkModified(this.GetMeasure(measureIndex));
                }
            }
        }

        public void SetMeasureVolumeCurve(int measureIndex, VolumeCurve volumeCurve)
        {
            if (measureIndex >= 0)
            {
                if ((measureIndex > 0) && volumeCurve.Equals(this.GetMeasureVolumeCurve(measureIndex - 1)))
                {
                    this.ClearMeasureVolumeCurve(measureIndex);
                }
                else
                {
                    if (this._measureVolumeCurveMap.ContainsKey(measureIndex))
                    {
                        this._measureVolumeCurveMap[measureIndex] = volumeCurve;
                    }
                    else
                    {
                        this._measureVolumeCurveMap.Add(measureIndex, volumeCurve);
                    }
                    McUtility.MarkModified(this.GetMeasure(measureIndex));
                }
            }
        }

        public int BeatsPerMeasure =>
            this.ParentNotation.BeatsPerMeasure;

        public MusicCanvasControl Canvas =>
            this.ParentNotation.Canvas;

        public McMeasure First =>
            ((this._measureList.Count == 0) ? null : this._measureList.First<McMeasure>());

        public int Index =>
            this.ParentNotation.IndexOf(this);

        public McMeasure Last =>
            ((this._measureList.Count == 0) ? null : this._measureList.Last<McMeasure>());

        public McMeasure[] MeasureArray =>
            this._measureList.ToArray();

        public SortedList<int, McMeasure.ClefTypes> MeasureClefTypeMap =>
            this._measureClefTypeMap;

        public SortedList<int, McMeasure.InstrumentTypes> MeasureInstrumentTypeMap =>
            this._measureInstrumentTypeMap;

        public SortedList<int, int> MeasureKeySignatureMap =>
            this._measureKeySignatureMap;

        public int MeasuresCount =>
            this._measureList.Count;

        public SortedList<int, VolumeCurve> MeasureVolumeCurveMap =>
            this._measureVolumeCurveMap;

        public SortedList<int, float> MeasureVolumeMap =>
            this._measureVolumeMap;

        public McNotation ParentNotation { get; private set; }
    }
}

