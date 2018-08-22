namespace GujianOL_MusicBox
{
    using SharpDX.Direct2D1;
    using SharpDX.DirectWrite;
    using SharpDX.DXGI;
    using SharpDX.Mathematics.Interop;
    using SharpDX.WIC;
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Drawing.Drawing2D;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Windows.Forms;
    using Telerik.WinControls;

    public class McMeasure
    {
        private McMeasure[] _measuresAligned = new McMeasure[3];
        private readonly List<McNotePack> _notePackList = new List<McNotePack>();
        public static readonly int Height = 280;
        public static readonly Color KeySignatureColor = Color.FromArgb(0xff, Color.MediumPurple);
        public static readonly Color KeySignatureColorWhite = Color.FromArgb(0xff, Color.WhiteSmoke);
        public static readonly int KeySignatureMax = 7;
        public static readonly int KeySignatureMin = -7;
        public static readonly int KeySignatureSingleWidth = 5;
        public static readonly int KeySignatureZoneHeadWidth = 0x38;
        public static readonly Padding Margin = new Padding(-1, -1, -1, -1);
        public static readonly int MeasureHeadWidth = 0x2e;
        public static readonly int MeasureTailWidth = 0x10;
        public static readonly Color NaturalSignColor = Color.FromArgb(0xff, Color.DodgerBlue);
        public static readonly int NotePackMaxAmount = 0x40;
        public static readonly int StaveSpacing = 11;

        public McMeasure(McRegularTrack parentRegularTrack)
        {
            this.ParentRegularTrack = parentRegularTrack;
            this.TemporaryInfo = null;
            if (parentRegularTrack != null)
            {
                McUtility.MarkModified(this);
            }
        }

        public void _ResetMeasuresAligned(McMeasure[] measuresAligned)
        {
            if ((measuresAligned.Length == this._measuresAligned.Length) && !this.ParentRegularTrack.ParentNotation.RegularTracksMLock)
            {
                this._measuresAligned = measuresAligned;
                McMeasure measure = measuresAligned[0];
                this.TemporaryInfo = (this == measure) ? new MsAlignedTemporaryInfo(measuresAligned) : measure.TemporaryInfo;
            }
        }

        public void _SetMeasureAligned(int trackIndex, McMeasure measure)
        {
            if (((trackIndex >= 0) && (trackIndex < this._measuresAligned.Length)) && !this.ParentRegularTrack.ParentNotation.RegularTracksMLock)
            {
                this._measuresAligned[trackIndex] = measure;
            }
        }

        public void ClearBitmapCache()
        {
            if (!((this.BitmapCache == null) || this.BitmapCache.IsDisposed))
            {
                this.BitmapCache.Dispose();
            }
            this.BitmapCache = null;
        }

        public void ClearTemporaryNotePacks()
        {
            foreach (McNotePack pack in this._notePackList)
            {
                pack.ClearTemporaryPitch();
            }
        }

        public int IndexOfNotePack(McNotePack note) => 
            this._notePackList.IndexOf(note);

        public McNotePack InsertNotePack(int? noteIndex = new int?(), McNotePack noteCloneSource = null)
        {
            int? nullable = noteIndex;
            noteIndex = new int?(nullable.HasValue ? nullable.GetValueOrDefault() : 0x5f5e0ff);
            if (noteIndex.Value < 0)
            {
                return null;
            }
            noteIndex = new int?(Math.Min(this._notePackList.Count, noteIndex.Value));
            McNotePack item = (noteCloneSource != null) ? noteCloneSource.Clone(this) : new McNotePack(this);
            this._notePackList.Insert(noteIndex.Value, item);
            if (this.NotePacksCount > NotePackMaxAmount)
            {
                RadMessageBox.Show($"第 {this.ParentRegularTrack.Index + 1} 轨第 {this.Index + 1} 小节的音符数量超出了所允许的上限（{this.NotePacksCount}/{NotePackMaxAmount}），超出部分将不会被保存。", "音符插入提示");
            }
            return item;
        }

        public void RedrawBitmapCache()
        {
            if (this.TemporaryInfo.Width > 0)
            {
                this.ClearBitmapCache();
                this.BitmapCache = MeasureRenderCache.DrawBitmapCache(this);
                McUtility.RemoveRedrawingMeasure(this);
            }
        }

        public static SharpDX.Direct2D1.Bitmap RedrawKeySignatureBitmapCache(RenderTarget renderTargetSource, Color color, bool isRaising, bool isLite)
        {
            ImagingFactory factory = new ImagingFactory();
            SharpDX.Direct2D1.Factory factory2 = new SharpDX.Direct2D1.Factory();
            SharpDX.DirectWrite.Factory factory3 = new SharpDX.DirectWrite.Factory();
            SharpDX.WIC.Bitmap wicBitmap = new SharpDX.WIC.Bitmap(factory, 0x18, 40, SharpDX.WIC.PixelFormat.Format32bppPBGRA, BitmapCreateCacheOption.CacheOnDemand);
            RenderTargetProperties renderTargetProperties = new RenderTargetProperties(RenderTargetType.Default, new SharpDX.Direct2D1.PixelFormat(SharpDX.DXGI.Format.Unknown, SharpDX.Direct2D1.AlphaMode.Unknown), 0f, 0f, RenderTargetUsage.None, FeatureLevel.Level_DEFAULT);
            WicRenderTarget renderTarget = new WicRenderTarget(factory2, wicBitmap, renderTargetProperties);
            try
            {
                RawVector2 vector;
                renderTarget.BeginDraw();
                renderTarget.Clear(new RawColor4?(Color.Transparent.ToRawColor4(1f)));
                if (isRaising)
                {
                    if (!isLite)
                    {
                        vector = new RawVector2(8f, 30f);
                        renderTarget.FillClassicMusicalVerticalParallelRect(new RawVector2(vector.X - 7f, vector.Y - 2f), new RawVector2(vector.X + 1f, vector.Y - 5f), color, 1.5f, 1f, false);
                        renderTarget.FillClassicMusicalVerticalParallelRect(new RawVector2(vector.X - 7f, vector.Y + 4f), new RawVector2(vector.X + 1f, vector.Y + 1f), color, 1.5f, 1f, false);
                        renderTarget.FillClassicMusicalVerticalParallelRect(new RawVector2(vector.X - 5f, vector.Y + 3f), new RawVector2(vector.X - 4.8f, vector.Y - 2f), color, 18f, 1f, false);
                        renderTarget.FillClassicMusicalVerticalParallelRect(new RawVector2(vector.X - 1.2f, vector.Y), new RawVector2(vector.X - 1f, vector.Y - 4.5f), color, 18f, 1f, false);
                    }
                    else
                    {
                        vector = new RawVector2(10f, 32f);
                        renderTarget.FillClassicMusicalVerticalParallelRect(new RawVector2(vector.X - 7f, vector.Y - 2f), new RawVector2(vector.X - 1f, vector.Y - 5f), color, 1.5f, 1f, false);
                        renderTarget.FillClassicMusicalVerticalParallelRect(new RawVector2(vector.X - 7f, vector.Y + 1f), new RawVector2(vector.X - 1f, vector.Y - 2f), color, 1.5f, 1f, false);
                        renderTarget.FillClassicMusicalVerticalParallelRect(new RawVector2(vector.X - 5f, vector.Y + 1f), new RawVector2(vector.X - 4.8f, vector.Y - 3f), color, 10f, 1f, false);
                        renderTarget.FillClassicMusicalVerticalParallelRect(new RawVector2(vector.X - 3.2f, vector.Y), new RawVector2(vector.X - 3f, vector.Y - 4.5f), color, 10f, 1f, false);
                    }
                }
                else
                {
                    PointF[] pathPoints;
                    using (GraphicsPath path = new GraphicsPath())
                    {
                        if (!isLite)
                        {
                            vector = new RawVector2(6f, 30f);
                            path.AddLine((float) (vector.X - 3f), (float) (vector.Y - 26f), (float) (vector.X - 3f), (float) (vector.Y + 7f));
                            path.AddBezier(vector.X - 3f, vector.Y + 7f, vector.X + 7f, vector.Y, vector.X + 4f, vector.Y - 10f, vector.X - 3f, vector.Y - 3f);
                            path.AddBezier((float) (vector.X - 3f), (float) (vector.Y - 3f), (float) (vector.X + 3f), (float) (vector.Y - 5f), (float) (vector.X + 3f), (float) (vector.Y - 2f), (float) (vector.X - 2f), (float) (vector.Y + 6f));
                        }
                        else
                        {
                            vector = new RawVector2(6f, 28f);
                            path.AddLine((float) (vector.X - 3f), (float) (vector.Y - 11f), (float) (vector.X - 3f), (float) (vector.Y + 7f));
                            path.AddBezier((float) (vector.X - 3f), (float) (vector.Y + 7f), (float) (vector.X + 5f), (float) (vector.Y + 4f), (float) (vector.X + 2f), (float) (vector.Y - 6f), (float) (vector.X - 3f), (float) (vector.Y + 2f));
                            path.AddBezier((float) (vector.X - 3f), (float) (vector.Y + 2f), (float) (vector.X + 2f), (float) (vector.Y - 1f), (float) (vector.X + 2f), (float) (vector.Y + 2f), (float) (vector.X - 2f), (float) (vector.Y + 6f));
                        }
                        path.Flatten();
                        pathPoints = path.PathPoints;
                        path.Dispose();
                    }
                    PathGeometry geometry = new PathGeometry(factory2);
                    if (pathPoints.Length > 1)
                    {
                        GeometrySink sink = geometry.Open();
                        sink.SetSegmentFlags(PathSegment.ForceRoundLineJoin);
                        sink.BeginFigure(new RawVector2(pathPoints[0].X, pathPoints[0].Y), FigureBegin.Filled);
                        for (int i = 1; i < pathPoints.Length; i++)
                        {
                            sink.AddLine(new RawVector2(pathPoints[i].X, pathPoints[i].Y));
                        }
                        sink.EndFigure(FigureEnd.Closed);
                        sink.Close();
                        sink.Dispose();
                    }
                    SolidColorBrush brush = new SolidColorBrush(renderTarget, color.ToRawColor4(1f));
                    renderTarget.FillGeometry(geometry, brush);
                    brush.Dispose();
                    geometry.Dispose();
                }
                renderTarget.EndDraw();
            }
            catch (Exception)
            {
                factory.Dispose();
                factory2.Dispose();
                factory3.Dispose();
                wicBitmap.Dispose();
                renderTarget.Dispose();
                return null;
            }
            SharpDX.Direct2D1.Bitmap bitmap2 = SharpDX.Direct2D1.Bitmap.FromWicBitmap(renderTargetSource, wicBitmap);
            factory.Dispose();
            factory2.Dispose();
            factory3.Dispose();
            wicBitmap.Dispose();
            renderTarget.Dispose();
            return bitmap2;
        }

        public static SharpDX.Direct2D1.Bitmap RedrawNaturalSignBitmapCache(RenderTarget renderTargetSource, Color color)
        {
            ImagingFactory factory = new ImagingFactory();
            SharpDX.Direct2D1.Factory factory2 = new SharpDX.Direct2D1.Factory();
            SharpDX.DirectWrite.Factory factory3 = new SharpDX.DirectWrite.Factory();
            SharpDX.WIC.Bitmap wicBitmap = new SharpDX.WIC.Bitmap(factory, 0x18, 40, SharpDX.WIC.PixelFormat.Format32bppPBGRA, BitmapCreateCacheOption.CacheOnDemand);
            RenderTargetProperties renderTargetProperties = new RenderTargetProperties(RenderTargetType.Default, new SharpDX.Direct2D1.PixelFormat(SharpDX.DXGI.Format.Unknown, SharpDX.Direct2D1.AlphaMode.Unknown), 0f, 0f, RenderTargetUsage.None, FeatureLevel.Level_DEFAULT);
            WicRenderTarget renderTarget = new WicRenderTarget(factory2, wicBitmap, renderTargetProperties);
            try
            {
                renderTarget.BeginDraw();
                renderTarget.Clear(new RawColor4?(Color.Transparent.ToRawColor4(1f)));
                RawVector2 vector = new RawVector2(9f, 31f);
                renderTarget.FillClassicMusicalVerticalParallelRect(new RawVector2(vector.X - 5f, vector.Y - 2f), new RawVector2(vector.X - 1f, vector.Y - 5f), color, 1.5f, 1f, false);
                renderTarget.FillClassicMusicalVerticalParallelRect(new RawVector2(vector.X - 5f, vector.Y + 4f), new RawVector2(vector.X - 1f, vector.Y + 1f), color, 1.5f, 1f, false);
                renderTarget.FillClassicMusicalVerticalParallelRect(new RawVector2(vector.X - 5f, (vector.Y + 3f) - 4f), new RawVector2(vector.X - 4.8f, (vector.Y - 2f) - 4f), color, 12f, 1f, false);
                renderTarget.FillClassicMusicalVerticalParallelRect(new RawVector2(vector.X - 1.2f, vector.Y + 4f), new RawVector2(vector.X - 1f, (vector.Y - 4.5f) + 4f), color, 12f, 1f, false);
                renderTarget.EndDraw();
            }
            catch (Exception)
            {
                factory.Dispose();
                factory2.Dispose();
                factory3.Dispose();
                wicBitmap.Dispose();
                renderTarget.Dispose();
                return null;
            }
            SharpDX.Direct2D1.Bitmap bitmap2 = SharpDX.Direct2D1.Bitmap.FromWicBitmap(renderTargetSource, wicBitmap);
            factory.Dispose();
            factory2.Dispose();
            factory3.Dispose();
            wicBitmap.Dispose();
            renderTarget.Dispose();
            return bitmap2;
        }

        public bool RemoveNotePack(McNotePack note)
        {
            if (note == null)
            {
                return false;
            }
            bool flag = this._notePackList.Remove(note);
            if (this.ParentRegularTrack != null)
            {
                McUtility.MarkModified(this);
            }
            return flag;
        }

        public bool RemoveNotePack(int noteIndex)
        {
            if ((noteIndex < 0) || (noteIndex >= this._notePackList.Count))
            {
                return false;
            }
            this._notePackList.RemoveAt(noteIndex);
            if (this.ParentRegularTrack != null)
            {
                McUtility.MarkModified(this);
            }
            return true;
        }

        public void ReorganizeDurationStamps()
        {
            this.TemporaryInfo.ReorganizeDurationStamps(this);
        }

        public void Shift(int offset)
        {
            if (offset != 0)
            {
                int pitchMin = McPitch.PitchMin;
                int pitchMax = McPitch.PitchMax;
                foreach (McNotePack pack in this._notePackList)
                {
                    foreach (McPitch pitch in pack.ValidPitchArray)
                    {
                        if (pitch.PitchType == McPitch.PitchTypes.Enabled)
                        {
                            pitchMin = Math.Max(pitchMin, pitch.Value);
                            pitchMax = Math.Min(pitchMax, pitch.Value);
                        }
                    }
                }
                if (((offset <= 0) || ((pitchMin + offset) <= McPitch.PitchMax)) && ((offset >= 0) || ((pitchMax + offset) >= McPitch.PitchMin)))
                {
                    foreach (McNotePack pack in this._notePackList)
                    {
                        if (!pack.IsRest)
                        {
                            McPitch[] validPitchArray = pack.ValidPitchArray;
                            for (int i = 0; i < pack.ValidPitchCount; i++)
                            {
                                pitch = (offset > 0) ? validPitchArray[(pack.ValidPitchCount - 1) - i] : validPitchArray[i];
                                if (pitch.PitchType == McPitch.PitchTypes.Enabled)
                                {
                                    int pitchValue = pitch.Value;
                                    int num5 = (pitchValue + offset).Clamp(McPitch.PitchMin, McPitch.PitchMax);
                                    McPitch.AlterantTypes rawAlterantType = pitch.RawAlterantType;
                                    pack.MarkPitch(pitchValue, McPitch.PitchTypes.Disabled);
                                    pack.MarkPitch(num5, McPitch.PitchTypes.Enabled);
                                    McPitch pitch2 = pack.GetPitch(num5);
                                    if (pitch2 != null)
                                    {
                                        pitch2.AlterantType = rawAlterantType;
                                    }
                                }
                            }
                        }
                    }
                    this.ReorganizeDurationStamps();
                }
            }
        }

        public int BeatsPerMeasure =>
            this.ParentRegularTrack.BeatsPerMeasure;

        public int BeatsPerMinute
        {
            get => 
                this.ParentRegularTrack.ParentNotation.GetMeasureBeatsPerMinute(this.Index);
            set
            {
                this.ParentRegularTrack.ParentNotation.SetMeasureBeatsPerMinute(this.Index, value);
            }
        }

        public int? BeatsPerMinuteRaw =>
            this.ParentRegularTrack.ParentNotation.RawGetMeasureBeatsPerMinute(this.Index);

        public SharpDX.Direct2D1.Bitmap BitmapCache { get; private set; }

        public MusicCanvasControl Canvas =>
            this.ParentRegularTrack.Canvas;

        public ClefTypes ClefType
        {
            get => 
                this.ParentRegularTrack.GetMeasureClefType(this.Index);
            set
            {
                this.ParentRegularTrack.SetMeasureClefType(this.Index, value);
            }
        }

        public ClefTypes ClefTypeRaw =>
            this.ParentRegularTrack.RawGetMeasureClefType(this.Index);

        public int Index =>
            this.ParentRegularTrack.IndexOf(this);

        public InstrumentTypes InstrumentType
        {
            get => 
                this.ParentRegularTrack.GetMeasureInstrumentType(this.Index);
            set
            {
                this.ParentRegularTrack.SetMeasureInstrumentType(this.Index, value);
            }
        }

        public InstrumentTypes InstrumentTypeRaw =>
            this.ParentRegularTrack.RawGetMeasureInstrumentType(this.Index);

        public bool IsDisplay { get; set; }

        public bool IsHovering { get; set; }

        public int KeySignature
        {
            get => 
                this.ParentRegularTrack.GetMeasureKeySignature(this.Index);
            set
            {
                this.ParentRegularTrack.SetMeasureKeySignature(this.Index, value);
            }
        }

        public int? KeySignatureRaw =>
            this.ParentRegularTrack.RawGetMeasureKeySignature(this.Index);

        public int KeySignatureZoneWidth =>
            KeySignatureZoneHeadWidth;

        public int MeasureDuration =>
            this._notePackList.Sum<McNotePack>(((Func<McNotePack, int>) (note => note.Duration)));

        public int MeasureDurationAligned =>
            this.ParentRegularTrack.ParentNotation.GetMeasureDurationAligned(this.Index);

        public McMeasure[] MeasuresAligned =>
            this._measuresAligned.ToArray<McMeasure>();

        public McMeasure NextMeasure =>
            this.ParentRegularTrack.GetMeasure(this.Index + 1);

        public McNotePack[] NotePacks =>
            this._notePackList.ToArray();

        public int NotePacksCount =>
            this._notePackList.Count;

        public McRegularTrack ParentRegularTrack { get; private set; }

        public McMeasure PreMeasure =>
            this.ParentRegularTrack.GetMeasure(this.Index - 1);

        public MsAlignedTemporaryInfo TemporaryInfo { get; private set; }

        public int Top =>
            (((((McRegularTrack.Margin.Bottom + McRegularTrack.Margin.Top) + Height) * this.ParentRegularTrack.Index) + McNotation.Margin.Top) + McRegularTrack.Margin.Top);

        public float Volume
        {
            get => 
                this.ParentRegularTrack.GetMeasureVolume(this.Index).Clamp(0f, 1f);
            set
            {
                this.ParentRegularTrack.SetMeasureVolume(this.Index, value.Clamp(0f, 1f));
            }
        }

        public GujianOL_MusicBox.VolumeCurve VolumeCurve
        {
            get => 
                this.ParentRegularTrack.GetMeasureVolumeCurve(this.Index);
            set
            {
                this.ParentRegularTrack.SetMeasureVolumeCurve(this.Index, value);
            }
        }

        public GujianOL_MusicBox.VolumeCurve VolumeCurveRaw =>
            this.ParentRegularTrack.RawGetMeasureVolumeCurve(this.Index);

        public float VolumeRaw =>
            this.ParentRegularTrack.RawGetMeasureVolume(this.Index).Clamp(-1f, 1f);

        public enum ClefTypes
        {
            L2G,
            L4F,
            Invaild
        }

        public enum InstrumentTypes
        {
            Zheng,
            Tieqin,
            Piano,
            Misc,
            Invaild
        }

        public static class MeasureRenderCache
        {
            private static Dictionary<McNotePack.DurationTypes, SharpDX.Direct2D1.Bitmap> _bitmapCommonNoteCaches = new Dictionary<McNotePack.DurationTypes, SharpDX.Direct2D1.Bitmap>();
            private static SharpDX.Direct2D1.Bitmap _bitmapKeySignatureFlatCache = null;
            private static SharpDX.Direct2D1.Bitmap _bitmapKeySignatureFlatLiteCache = null;
            private static SharpDX.Direct2D1.Bitmap _bitmapKeySignatureFlatLiteWhiteCache = null;
            private static SharpDX.Direct2D1.Bitmap _bitmapKeySignatureSharpCache = null;
            private static SharpDX.Direct2D1.Bitmap _bitmapKeySignatureSharpLiteCache = null;
            private static SharpDX.Direct2D1.Bitmap _bitmapKeySignatureSharpLiteWhiteCache = null;
            private static SharpDX.Direct2D1.Bitmap _bitmapL2GCache = null;
            private static SharpDX.Direct2D1.Bitmap _bitmapL4FCache = null;
            private static SharpDX.Direct2D1.Bitmap _bitmapMiscNoteCache = null;
            private static SharpDX.Direct2D1.Bitmap _bitmapNaturalSignCache = null;
            private static Dictionary<McNotePack.DurationTypes, SharpDX.Direct2D1.Bitmap> _bitmapRestNoteCaches = new Dictionary<McNotePack.DurationTypes, SharpDX.Direct2D1.Bitmap>();
            private static SharpDX.Direct2D1.Factory _factory = new SharpDX.Direct2D1.Factory();
            private static int _lastWidth = -1;
            private static RenderTargetProperties _renderTargetProperties = new RenderTargetProperties(RenderTargetType.Default, new SharpDX.Direct2D1.PixelFormat(SharpDX.DXGI.Format.Unknown, SharpDX.Direct2D1.AlphaMode.Unknown), 0f, 0f, RenderTargetUsage.None, FeatureLevel.Level_DEFAULT);
            private static SharpDX.WIC.Bitmap _wicBitmap = null;
            private static ImagingFactory _wicFactory = new ImagingFactory();
            private static WicRenderTarget _wicRenderTarget = null;
            private static SharpDX.DirectWrite.Factory _writeFactory = new SharpDX.DirectWrite.Factory();

            private static void DrawArpeggio(McMeasure measure, RawVector2 pos, Color color)
            {
                PointF[] pathPoints;
                float x = pos.X;
                float y = pos.Y;
                using (GraphicsPath path = new GraphicsPath())
                {
                    PointF[] points = new PointF[] { new PointF(x - 1f, y - 2f), new PointF(x + 2f, y + 3f), new PointF(x - 0.5f, y + 8.5f), new PointF(x + 1f, y + 11f), new PointF((x + 1f) - 1f, y + 12f), new PointF((x - 1f) - 2f, y + 8f), new PointF((x - 1f) + 0.5f, y + 2.5f), new PointF((x - 1f) - 1f, y - 1f) };
                    path.AddClosedCurve(points);
                    path.Flatten();
                    pathPoints = path.PathPoints;
                    path.Dispose();
                }
                using (SharpDX.Direct2D1.Factory factory = _wicRenderTarget.Factory)
                {
                    PathGeometry geometry = new PathGeometry(factory);
                    if (pathPoints.Length > 1)
                    {
                        GeometrySink sink = geometry.Open();
                        sink.SetSegmentFlags(PathSegment.ForceRoundLineJoin);
                        sink.BeginFigure(new RawVector2(pathPoints[0].X, pathPoints[0].Y), FigureBegin.Filled);
                        for (int i = 1; i < pathPoints.Length; i++)
                        {
                            sink.AddLine(new RawVector2(pathPoints[i].X, pathPoints[i].Y));
                        }
                        sink.EndFigure(FigureEnd.Closed);
                        sink.Close();
                        sink.Dispose();
                    }
                    SolidColorBrush brush = new SolidColorBrush(_wicRenderTarget, color.ToRawColor4(1f));
                    _wicRenderTarget.FillGeometry(geometry, brush);
                    brush.Dispose();
                    geometry.Dispose();
                }
            }

            private static void DrawBeatGroupStemAndTailSmoothly(McMeasure measure, Dictionary<int, McNotePack> lastBeatGroup, Color color)
            {
                if ((lastBeatGroup != null) && (lastBeatGroup.Count > 0))
                {
                    McPitch pitch;
                    McPitch pitch2;
                    int num = lastBeatGroup.Keys.ToArray<int>().Last<int>();
                    McNotePack pack = lastBeatGroup[num];
                    float num2 = ((pack.Index + 1) > McMeasure.NotePackMaxAmount) ? 0.25f : 1f;
                    Color color2 = Color.FromArgb((int) (color.A * num2), color);
                    int stampNum = lastBeatGroup.Keys.ToArray<int>().First<int>();
                    McNotePack notePack = lastBeatGroup[stampNum];
                    bool isFlipVerticalStem = notePack.IsFlipVerticalStem;
                    if (lastBeatGroup.Count == 1)
                    {
                        if ((notePack.TemporaryInfo.LinkedInTieNote != null) && ((notePack.TieType == McNotePack.TieTypes.End) || (notePack.TieType == McNotePack.TieTypes.Both)))
                        {
                            isFlipVerticalStem = notePack.TemporaryInfo.LinkedInTieNote.TemporaryInfo.IsFlipVerticalStemVoted;
                        }
                        notePack.TemporaryInfo.IsFlipVerticalStemVoted = isFlipVerticalStem;
                        pitch = notePack.GetPitch(notePack.HighestPitchValue);
                        pitch2 = notePack.GetPitch(notePack.LowestPitchValue);
                        notePack.TemporaryInfo.RelativeHighestPitchY = pitch.MeasureLineRelativeY;
                        notePack.TemporaryInfo.RelativeLowestPitchY = pitch2.MeasureLineRelativeY;
                        notePack.TemporaryInfo.StemStartY = isFlipVerticalStem ? notePack.TemporaryInfo.RelativeHighestPitchY : notePack.TemporaryInfo.RelativeLowestPitchY;
                        notePack.TemporaryInfo.StemStartYSmooth = measure.IsHovering ? notePack.TemporaryInfo.StemStartYSmooth.Lerp(notePack.TemporaryInfo.StemStartY, 0.2f) : notePack.TemporaryInfo.StemStartY;
                        notePack.TemporaryInfo.StemEndY = isFlipVerticalStem ? ((notePack.TemporaryInfo.RelativeLowestPitchY + 40f) + 10f) : ((notePack.TemporaryInfo.RelativeHighestPitchY - 40f) - 10f);
                        notePack.TemporaryInfo.StemEndYSmooth = measure.IsHovering ? notePack.TemporaryInfo.StemEndYSmooth.Lerp(notePack.TemporaryInfo.StemEndY, 0.2f) : notePack.TemporaryInfo.StemEndY;
                        DrawNotePackStem(measure, notePack, stampNum, color2, notePack.TemporaryInfo.StemStartYSmooth, notePack.TemporaryInfo.StemEndYSmooth, true);
                    }
                    else
                    {
                        McNotePack pack3;
                        int num4 = 0;
                        int num5 = 0;
                        foreach (int num6 in lastBeatGroup.Keys)
                        {
                            pack3 = lastBeatGroup[num6];
                            if ((pack3.TemporaryInfo.LinkedInTieNote != null) && ((pack3.TieType == McNotePack.TieTypes.End) || (pack3.TieType == McNotePack.TieTypes.Both)))
                            {
                                if (pack3.TemporaryInfo.LinkedInTieNote.TemporaryInfo.IsFlipVerticalStemVoted)
                                {
                                    num5 += 100;
                                }
                                else
                                {
                                    num4 += 100;
                                }
                            }
                            else if (pack3.IsFlipVerticalStem)
                            {
                                num5++;
                            }
                            else
                            {
                                num4++;
                            }
                        }
                        isFlipVerticalStem = num5 >= num4;
                        float num7 = 0f;
                        float num8 = isFlipVerticalStem ? ((float) 3) : ((float) 14);
                        float measureLineRelativeYByLineValue = McPitch.GetMeasureLineRelativeYByLineValue(measure.ClefType, 0x2d);
                        float measureFirstLineRelativeY = McPitch.GetMeasureFirstLineRelativeY(measure.ClefType);
                        float num11 = pack.TemporaryInfo.RelativeXSmooth - notePack.TemporaryInfo.RelativeXSmooth;
                        McPitch pitch3 = notePack.GetPitch(isFlipVerticalStem ? notePack.LowestPitchValue : notePack.HighestPitchValue);
                        McPitch pitch4 = pack.GetPitch(isFlipVerticalStem ? pack.LowestPitchValue : pack.HighestPitchValue);
                        float num12 = (pitch3 != null) ? pitch3.MeasureLineRelativeY : (isFlipVerticalStem ? measureFirstLineRelativeY : measureLineRelativeYByLineValue);
                        float num13 = (pitch4 != null) ? pitch4.MeasureLineRelativeY : (isFlipVerticalStem ? measureFirstLineRelativeY : measureLineRelativeYByLineValue);
                        if (isFlipVerticalStem)
                        {
                            if (num13 > num12)
                            {
                                num12 = Math.Max(num12, num13 - 24f);
                            }
                            else
                            {
                                num13 = Math.Max(num13, num12 - 24f);
                            }
                        }
                        else if (num13 < num12)
                        {
                            num12 = Math.Min(num12, num13 + 24f);
                        }
                        else
                        {
                            num13 = Math.Min(num13, num12 + 24f);
                        }
                        float num14 = num13 - num12;
                        foreach (int num6 in lastBeatGroup.Keys)
                        {
                            pack3 = lastBeatGroup[num6];
                            pack3.TemporaryInfo.IsFlipVerticalStemVoted = isFlipVerticalStem;
                            pitch = pack3.GetPitch(pack3.HighestPitchValue);
                            pitch2 = pack3.GetPitch(pack3.LowestPitchValue);
                            pack3.TemporaryInfo.RelativeHighestPitchY = (pitch != null) ? pitch.MeasureLineRelativeY : measureLineRelativeYByLineValue;
                            pack3.TemporaryInfo.RelativeLowestPitchY = (pitch2 != null) ? pitch2.MeasureLineRelativeY : measureFirstLineRelativeY;
                            float num15 = isFlipVerticalStem ? (pack3.TemporaryInfo.RelativeLowestPitchY + 40f) : (pack3.TemporaryInfo.RelativeHighestPitchY - 40f);
                            float num16 = (num11 < 1E-06f) ? 0f : (((pack3.TemporaryInfo.RelativeXSmooth - notePack.TemporaryInfo.RelativeXSmooth) * num14) / num11);
                            float num17 = num12 + num16;
                            if ((isFlipVerticalStem && (num15 > num17)) || (!isFlipVerticalStem && (num15 < num17)))
                            {
                                num7 = Math.Max(num7, Math.Abs((float) (num15 - num17)));
                            }
                            pack3.TemporaryInfo.StemEndY = num17;
                        }
                        foreach (int num6 in lastBeatGroup.Keys)
                        {
                            pack3 = lastBeatGroup[num6];
                            bool flag2 = (pack3.TemporaryInfo.StemStartYSmooth < 1f) && (pack3.TemporaryInfo.StemEndYSmooth < 1f);
                            bool flag3 = (pack3.TemporaryInfo.StemStartYSmooth > pack3.TemporaryInfo.StemEndYSmooth) && isFlipVerticalStem;
                            bool flag4 = (pack3.TemporaryInfo.StemStartYSmooth < pack3.TemporaryInfo.StemEndYSmooth) && !isFlipVerticalStem;
                            pack3.TemporaryInfo.StemStartY = isFlipVerticalStem ? pack3.TemporaryInfo.RelativeHighestPitchY : pack3.TemporaryInfo.RelativeLowestPitchY;
                            pack3.TemporaryInfo.StemStartYSmooth = measure.IsHovering ? pack3.TemporaryInfo.StemStartYSmooth.Lerp(pack3.TemporaryInfo.StemStartY, 0.2f) : pack3.TemporaryInfo.StemStartY;
                            McNotePack.NpTemporaryInfo temporaryInfo = pack3.TemporaryInfo;
                            temporaryInfo.StemEndY += num7 * (isFlipVerticalStem ? ((float) 1) : ((float) (-1)));
                            float num18 = 0.25f * ((float) (Math.Abs((float) (pack3.TemporaryInfo.StemEndY - pack3.TemporaryInfo.StemEndYSmooth)) / 10f)).Clamp(((float) 0f), ((float) 1f));
                            pack3.TemporaryInfo.StemEndYSmoothAccSpeed = ((float) (pack3.TemporaryInfo.StemEndYSmoothAccSpeed + ((pack3.TemporaryInfo.StemEndY > pack3.TemporaryInfo.StemEndYSmooth) ? num18 : -num18))).Clamp((float) -8f, (float) 8f);
                            if (measure.IsHovering)
                            {
                                int num19 = (pack3.TemporaryInfo.StemEndY > pack3.TemporaryInfo.StemEndYSmooth) ? 2 : 8;
                                if (((pack3.TemporaryInfo.StemEndYSmoothAccSpeed > 0f) && (num19 == 8)) || ((pack3.TemporaryInfo.StemEndYSmoothAccSpeed < 0f) && (num19 == 2)))
                                {
                                    pack3.TemporaryInfo.StemEndYSmoothAccSpeed = pack3.TemporaryInfo.StemEndYSmoothAccSpeed.Lerp(0f, 0.5f);
                                }
                                McNotePack.NpTemporaryInfo info2 = pack3.TemporaryInfo;
                                info2.StemEndYSmooth += pack3.TemporaryInfo.StemEndYSmoothAccSpeed;
                            }
                            else
                            {
                                pack3.TemporaryInfo.StemEndYSmooth = pack3.TemporaryInfo.StemEndY;
                            }
                            float num20 = 8f;
                            if (flag2)
                            {
                                pack3.TemporaryInfo.StemStartYSmooth = isFlipVerticalStem ? pack3.TemporaryInfo.RelativeHighestPitchY : pack3.TemporaryInfo.RelativeLowestPitchY;
                                pack3.TemporaryInfo.StemEndYSmooth = pack3.TemporaryInfo.StemEndY + (isFlipVerticalStem ? -num20 : num20);
                            }
                            else if (flag3)
                            {
                                pack3.TemporaryInfo.StemEndYSmooth = pack3.TemporaryInfo.StemEndY - num20;
                            }
                            else if (flag4)
                            {
                                pack3.TemporaryInfo.StemEndYSmooth = pack3.TemporaryInfo.StemEndY + num20;
                            }
                            DrawNotePackStem(measure, pack3, num6, color2, pack3.TemporaryInfo.StemStartYSmooth, pack3.TemporaryInfo.StemEndYSmooth, false);
                            if (measure.IsHovering)
                            {
                                _wicRenderTarget.FillCircle(new RawVector2(pack3.TemporaryInfo.RelativeXSmooth + num8, pack3.TemporaryInfo.StemEndYSmooth + (isFlipVerticalStem ? ((float) 0) : ((float) (-2)))), 1f, Color.FromArgb(0xff, Color.DodgerBlue));
                            }
                        }
                        float num21 = notePack.TemporaryInfo.RelativeXSmooth + num8;
                        float num22 = notePack.TemporaryInfo.StemEndYSmooth + (isFlipVerticalStem ? ((float) (-2)) : ((float) 0));
                        float num23 = pack.TemporaryInfo.StemEndYSmooth + (isFlipVerticalStem ? ((float) (-2)) : ((float) 0));
                        float num24 = num23 - num22;
                        int[] numArray = lastBeatGroup.Keys.ToArray<int>();
                        float num25 = 12f;
                        float?[] nullableArray3 = new float?[4];
                        float? nullable = null;
                        nullableArray3[0] = nullable;
                        nullable = null;
                        nullableArray3[1] = nullable;
                        nullable = null;
                        nullableArray3[2] = nullable;
                        nullable = null;
                        nullableArray3[3] = nullable;
                        float?[] nullableArray = nullableArray3;
                        nullableArray3 = new float?[4];
                        nullable = null;
                        nullableArray3[0] = nullable;
                        nullable = null;
                        nullableArray3[1] = nullable;
                        nullable = null;
                        nullableArray3[2] = nullable;
                        nullableArray3[3] = null;
                        float?[] nullableArray2 = nullableArray3;
                        for (int i = 0; i < numArray.Length; i++)
                        {
                            bool flag5 = i == (numArray.Length - 1);
                            pack3 = lastBeatGroup[numArray[i]];
                            int tailCount = pack3.TailCount;
                            for (int j = 1; j <= 3; j++)
                            {
                                if (tailCount >= j)
                                {
                                    float num29 = pack3.TemporaryInfo.RelativeXSmooth + num8;
                                    if (!nullableArray[j].HasValue)
                                    {
                                        nullableArray[j] = new float?(flag5 ? (num29 - num25) : num29);
                                    }
                                    nullableArray2[j] = new float?(flag5 ? num29 : (num29 + num25));
                                }
                                if (((tailCount < j) || flag5) && nullableArray[j].HasValue)
                                {
                                    float num30 = ((j - 1) * 6) * (isFlipVerticalStem ? -1 : 1);
                                    float x = nullableArray[j].Value;
                                    float num32 = num22 + ((num11 < 1E-06f) ? 0f : (((x - num21) * num24) / num11));
                                    float num33 = nullableArray2[j].Value;
                                    float num34 = num22 + ((num11 < 1E-06f) ? 0f : (((num33 - num21) * num24) / num11));
                                    _wicRenderTarget.DrawLine(new RawVector2(x, num32 + num30), new RawVector2(num33, num34 + num30), color2, 4f);
                                    nullableArray[j] = null;
                                    nullableArray2[j] = null;
                                }
                            }
                        }
                    }
                }
            }

            public static SharpDX.Direct2D1.Bitmap DrawBitmapCache(McMeasure measure)
            {
                int width = measure.TemporaryInfo.Width;
                int height = McMeasure.Height;
                if (width != _lastWidth)
                {
                    _lastWidth = width;
                    ResetBitmapCaches(width);
                }
                try
                {
                    float num4;
                    int num6;
                    Size2F? nullable5;
                    _wicRenderTarget.BeginDraw();
                    _wicRenderTarget.Clear(new RawColor4?(Color.Transparent.ToRawColor4(1f)));
                    RawRectangleF rectF = new RawRectangleF(0f, 0f, (float) width, (float) McMeasure.Height);
                    _wicRenderTarget.FillRectangle(rectF, Color.FromArgb(30, 30, 30));
                    if (measure.ParentRegularTrack.Index == 0)
                    {
                        _wicRenderTarget.DrawTextLayout(_writeFactory, $"{measure.Index + 1:D2}", new RawVector2(12f, 4f), "Arial", 14f, Color.FromArgb(220, Color.WhiteSmoke), TextAlignment.Leading, ParagraphAlignment.Near, SharpDX.DirectWrite.FontStyle.Italic, FontWeight.Bold);
                        _wicRenderTarget.DrawTextLayout(_writeFactory, "Measure", new RawVector2(6f, 16f), "Arial", 9f, Color.FromArgb(0xcd, Color.LightGreen), TextAlignment.Leading, ParagraphAlignment.Near, SharpDX.DirectWrite.FontStyle.Italic, FontWeight.Bold);
                    }
                    int index = measure.Index;
                    if ((measure.VolumeRaw > 0f) || (index == 0))
                    {
                        _wicRenderTarget.DrawTextLayout(_writeFactory, $"{measure.Volume * 100f:F0}%", new RawVector2(12f, 40f), "Arial", 14f, Color.FromArgb(220, Color.WhiteSmoke), TextAlignment.Leading, ParagraphAlignment.Near, SharpDX.DirectWrite.FontStyle.Italic, FontWeight.Bold);
                        _wicRenderTarget.DrawTextLayout(_writeFactory, "Volume", new RawVector2(6f, 52f), "Arial", 9f, Color.FromArgb(0xcd, Color.Yellow), TextAlignment.Leading, ParagraphAlignment.Near, SharpDX.DirectWrite.FontStyle.Italic, FontWeight.Bold);
                    }
                    McMeasure.ClefTypes clefType = measure.ClefType;
                    McMeasure.ClefTypes clefTypeRaw = measure.ClefTypeRaw;
                    bool flag = (index == 0) || (clefTypeRaw != McMeasure.ClefTypes.Invaild);
                    if ((measure.InstrumentTypeRaw != McMeasure.InstrumentTypes.Invaild) || (index == 0))
                    {
                        num4 = flag ? ((clefType == McMeasure.ClefTypes.L2G) ? ((float) 0x58) : ((float) 0x81)) : ((clefType == McMeasure.ClefTypes.L2G) ? ((float) 0x65) : ((float) 0x81));
                        _wicRenderTarget.DrawTextLayout(_writeFactory, $"{measure.InstrumentType}", new RawVector2(6f, num4), "Arial", 14f, Color.FromArgb(220, Color.DeepSkyBlue), TextAlignment.Leading, ParagraphAlignment.Near, SharpDX.DirectWrite.FontStyle.Italic, FontWeight.Bold);
                    }
                    if (measure.BeatsPerMinuteRaw.HasValue)
                    {
                        num4 = flag ? ((clefType == McMeasure.ClefTypes.L2G) ? ((float) 0xbd) : ((float) 0xc5)) : ((clefType == McMeasure.ClefTypes.L2G) ? ((float) 0xa9) : ((float) 0xc5));
                        _wicRenderTarget.DrawTextLayout(_writeFactory, $"{measure.BeatsPerMinuteRaw.Value}", new RawVector2(12f, num4), "Arial", 14f, Color.FromArgb(220, Color.WhiteSmoke), TextAlignment.Leading, ParagraphAlignment.Near, SharpDX.DirectWrite.FontStyle.Italic, FontWeight.Bold);
                        _wicRenderTarget.DrawTextLayout(_writeFactory, "Beat pm.", new RawVector2(6f, num4 + 12f), "Arial", 9f, Color.FromArgb(0xcd, Color.Gold), TextAlignment.Leading, ParagraphAlignment.Near, SharpDX.DirectWrite.FontStyle.Italic, FontWeight.Bold);
                    }
                    int measureFirstLineRelativeY = McPitch.GetMeasureFirstLineRelativeY(measure.ClefType);
                    Color color = Color.FromArgb(0x80, 0x80, 0x80);
                    Color color2 = Color.FromArgb(0xa8, 0xa8, 0xbc);
                    for (num6 = 0; num6 < 5; num6++)
                    {
                        int num7 = measureFirstLineRelativeY - (num6 * McMeasure.StaveSpacing);
                        if ((clefType == McMeasure.ClefTypes.L2G) && (num6 == 1))
                        {
                            _wicRenderTarget.DrawLine(new RawVector2(0f, (float) num7), new RawVector2((float) width, (float) num7), color2, 1f);
                        }
                        else if ((clefType == McMeasure.ClefTypes.L4F) && (num6 == 3))
                        {
                            _wicRenderTarget.DrawLine(new RawVector2(0f, (float) num7), new RawVector2((float) width, (float) num7), color2, 1f);
                        }
                        else
                        {
                            _wicRenderTarget.DrawLine(new RawVector2(0f, (float) num7), new RawVector2((float) width, (float) num7), color, 1f);
                        }
                    }
                    if (flag)
                    {
                        switch (clefType)
                        {
                            case McMeasure.ClefTypes.L2G:
                                nullable5 = null;
                                _wicRenderTarget.DrawBitmap(BitmapL2GCache, new RawVector2(2f, (float) (measureFirstLineRelativeY - 0x3e)), nullable5, 0.9f, SharpDX.Direct2D1.BitmapInterpolationMode.NearestNeighbor);
                                break;

                            case McMeasure.ClefTypes.L4F:
                                nullable5 = null;
                                _wicRenderTarget.DrawBitmap(BitmapL4FCache, new RawVector2(2f, (float) (measureFirstLineRelativeY - 0x3e)), nullable5, 0.9f, SharpDX.Direct2D1.BitmapInterpolationMode.NearestNeighbor);
                                break;
                        }
                    }
                    int? keySignatureRaw = measure.KeySignatureRaw;
                    if (keySignatureRaw.HasValue && (keySignatureRaw.Value != 0))
                    {
                        int[] numArray;
                        int num8;
                        float num9;
                        if (keySignatureRaw.Value > 0)
                        {
                            numArray = new int[] { 8, 5, 9, 6, 3, 7, 4 };
                            for (num6 = 0; num6 < keySignatureRaw.Value.Clamp(1, McMeasure.KeySignatureMax); num6++)
                            {
                                num8 = (clefType == McMeasure.ClefTypes.L4F) ? (numArray[num6] - 2) : numArray[num6];
                                num9 = (measureFirstLineRelativeY - (num8 * (((float) McMeasure.StaveSpacing) / 2f))) - 30f;
                                nullable5 = null;
                                _wicRenderTarget.DrawBitmap(BitmapKeySignatureSharpCache, new RawVector2((float) (McMeasure.MeasureHeadWidth + (McMeasure.KeySignatureSingleWidth * num6)), num9), nullable5, 1f, SharpDX.Direct2D1.BitmapInterpolationMode.NearestNeighbor);
                            }
                        }
                        else if (keySignatureRaw.Value < 0)
                        {
                            numArray = new int[] { 4, 7, 3, 6, 2, 5, 1 };
                            for (num6 = 0; num6 < Math.Abs(keySignatureRaw.Value.Clamp(McMeasure.KeySignatureMin, -1)); num6++)
                            {
                                num8 = (clefType == McMeasure.ClefTypes.L4F) ? (numArray[num6] - 2) : numArray[num6];
                                num9 = (measureFirstLineRelativeY - (num8 * (((float) McMeasure.StaveSpacing) / 2f))) - 30f;
                                nullable5 = null;
                                _wicRenderTarget.DrawBitmap(BitmapKeySignatureFlatCache, new RawVector2((float) ((McMeasure.MeasureHeadWidth - 4) + (McMeasure.KeySignatureSingleWidth * num6)), num9), nullable5, 1f, SharpDX.Direct2D1.BitmapInterpolationMode.NearestNeighbor);
                            }
                        }
                    }
                    DrawNotePacks(measure, Color.FromArgb(0xff, (int) (McNotePack.NoteColorWhite.R * 0.7f), (int) (McNotePack.NoteColorWhite.G * 0.7f), (int) (McNotePack.NoteColorWhite.B * 0.7f)));
                    _wicRenderTarget.DrawRoundedRectangle(rectF, 0f, Color.FromArgb(0x22, 0x22, 0x24), 2.5f);
                    McNotation parentNotation = measure.ParentRegularTrack.ParentNotation;
                    int num10 = parentNotation.BeatDuration * parentNotation.BeatsPerMeasure;
                    bool flag2 = (measure.VolumeCurveRaw != null) || (index == 0);
                    float? nullable2 = null;
                    float? nullable3 = null;
                    Color baseColor = flag2 ? Color.LawnGreen : Color.DeepSkyBlue;
                    List<PointF> list = new List<PointF>();
                    for (num6 = 0; num6 < measure.NotePacks.Length; num6++)
                    {
                        McNotePack pack = measure.NotePacks[num6];
                        nullable2 = new float?(pack.TemporaryInfo.RelativeX + 8f);
                        nullable3 = new float?((McMeasure.Height - 8) - (56f * pack.VolumeRaw));
                        list.Add(new PointF(nullable2.Value, nullable3.Value));
                        if ((num6 == 0) || (num6 == (measure.NotePacks.Length - 1)))
                        {
                            _wicRenderTarget.FillCircle(new RawVector2(nullable2.Value, nullable3.Value), 2f, Color.FromArgb(0x80, baseColor));
                        }
                    }
                    if (list.Count > 1)
                    {
                        StrokeStyleProperties properties = new StrokeStyleProperties {
                            DashStyle = SharpDX.Direct2D1.DashStyle.Dash
                        };
                        StrokeStyle strokeStyle = new StrokeStyle(_factory, properties);
                        _wicRenderTarget.DrawBezierLine(list.ToArray(), Color.FromArgb(0x18, baseColor), 2f, strokeStyle, true);
                    }
                    if ((flag2 && nullable3.HasValue) && nullable2.HasValue)
                    {
                        _wicRenderTarget.DrawTextLayout(_writeFactory, "new Vc.", new RawVector2(nullable2.Value + 14f, nullable3.Value - 5f), "Arial", 9f, Color.FromArgb(0x80, baseColor), TextAlignment.Leading, ParagraphAlignment.Near, SharpDX.DirectWrite.FontStyle.Italic, FontWeight.Bold);
                    }
                    if (((measure.NotePacks.Length > 0) && (measure.MeasureDuration != num10)) && (measure.InstrumentType != McMeasure.InstrumentTypes.Misc))
                    {
                        _wicRenderTarget.DrawTextLayout(_writeFactory, $"{measure.MeasureDuration}/{num10}", new RawVector2(12f, rectF.Bottom - 28f), "Arial", 14f, Color.FromArgb(220, Color.IndianRed), TextAlignment.Leading, ParagraphAlignment.Near, SharpDX.DirectWrite.FontStyle.Italic, FontWeight.Bold);
                        _wicRenderTarget.DrawTextLayout(_writeFactory, "Beat im.", new RawVector2(6f, (rectF.Bottom - 28f) + 12f), "Arial", 9f, Color.FromArgb(0xcd, Color.Red), TextAlignment.Leading, ParagraphAlignment.Near, SharpDX.DirectWrite.FontStyle.Italic, FontWeight.Bold);
                    }
                    _wicRenderTarget.EndDraw();
                }
                catch (Exception)
                {
                    return null;
                }
                return SharpDX.Direct2D1.Bitmap.FromWicBitmap(measure.Canvas.RenderTarget2D, _wicBitmap);
            }

            private static void DrawNotePacks(McMeasure measure, Color color)
            {
                int num6;
                McNotePack notePack;
                int num = McMeasure.MeasureHeadWidth + measure.KeySignatureZoneWidth;
                int notePackWidth = McNotePack.NotePackWidth;
                int index = measure.ParentRegularTrack.Index;
                McNotation.DurationStamp[] stampArray = measure.TemporaryInfo.ReorganizedDurationStamps.Values.ToArray<McNotation.DurationStamp>();
                int duration = 0;
                int num5 = measure.ParentRegularTrack.ParentNotation.BeatDuration * 2;
                Dictionary<int, McNotePack> lastBeatGroup = new Dictionary<int, McNotePack>();
                for (num6 = 0; num6 < stampArray.Length; num6++)
                {
                    McNotation.DurationStamp stamp = stampArray[num6];
                    notePack = stamp.GetNotePack(index);
                    if (notePack != null)
                    {
                        float measureFirstLineRelativeY = McPitch.GetMeasureFirstLineRelativeY(measure.ClefType);
                        if (notePack.IsRest)
                        {
                            int num8 = (num + (num6 * notePackWidth)) - 8;
                            notePack.TemporaryInfo.RelativeX = num8;
                            _wicRenderTarget.DrawBitmap(GetBitmapRestNoteCache(notePack.DurationType), new RawVector2(notePack.TemporaryInfo.RelativeXSmooth, measureFirstLineRelativeY - 44f), ((notePack.Index + 1) > McMeasure.NotePackMaxAmount) ? 0.25f : 1f);
                        }
                        else
                        {
                            int num12;
                            float measureLineRelativeYByLineValue;
                            Color baseColor = Color.FromArgb(0x80, 0x80, 0x80);
                            float num9 = (((float) notePackWidth) / 5f) + 2f;
                            int num10 = num + (num6 * notePackWidth);
                            McPitch pitch = notePack.GetPitch(notePack.HighestPitchValue);
                            int measureLineValue = pitch.MeasureLineValue;
                            if (measureLineValue > 40)
                            {
                                num12 = 50;
                                while (num12 <= measureLineValue)
                                {
                                    if ((num12 % 10) == 0)
                                    {
                                        measureLineRelativeYByLineValue = McPitch.GetMeasureLineRelativeYByLineValue(measure.ClefType, num12);
                                        _wicRenderTarget.DrawLine(new RawVector2(num10 - num9, measureLineRelativeYByLineValue), new RawVector2(num10 + num9, measureLineRelativeYByLineValue), Color.FromArgb(200, baseColor));
                                    }
                                    num12 += 5;
                                }
                            }
                            McPitch pitch2 = notePack.GetPitch(notePack.LowestPitchValue);
                            int num14 = pitch2.MeasureLineValue;
                            if (num14 < 0)
                            {
                                num12 = num14;
                                while (num12 <= -10)
                                {
                                    if ((num12 % 10) == 0)
                                    {
                                        measureLineRelativeYByLineValue = McPitch.GetMeasureLineRelativeYByLineValue(measure.ClefType, num12);
                                        _wicRenderTarget.DrawLine(new RawVector2(num10 - num9, measureLineRelativeYByLineValue), new RawVector2(num10 + num9, measureLineRelativeYByLineValue), Color.FromArgb(200, baseColor));
                                    }
                                    num12 += 5;
                                }
                            }
                            notePack.TemporaryInfo.PitchIndexRelativePosMap.Clear();
                            foreach (int num15 in notePack.EnabledPitchValueArray)
                            {
                                DrawPitch(measure, notePack, num15, num6, color);
                            }
                            if (notePack.Staccato && !notePack.IsRest)
                            {
                                if (notePack.TemporaryInfo.IsFlipVerticalStemVoted)
                                {
                                    measureLineRelativeYByLineValue = McPitch.GetMeasureLineRelativeY(measure.ClefType, pitch.Value) - 11;
                                    _wicRenderTarget.FillCircle(new RawVector2((float) num10, measureLineRelativeYByLineValue), 2f, Color.Yellow);
                                }
                                else
                                {
                                    measureLineRelativeYByLineValue = McPitch.GetMeasureLineRelativeY(measure.ClefType, pitch2.Value) + 11;
                                    _wicRenderTarget.FillCircle(new RawVector2((float) num10, measureLineRelativeYByLineValue), 2f, Color.Yellow);
                                }
                            }
                            if ((notePack.ArpeggioModeRaw != Arpeggio.ArpeggioTypes.None) && !notePack.IsRest)
                            {
                                Color color3 = (notePack.ArpeggioMode == Arpeggio.ArpeggioTypes.None) ? Color.FromArgb(0x40, color) : color;
                                bool flag = notePack.ArpeggioModeRaw == Arpeggio.ArpeggioTypes.Upward;
                                float x = notePack.TemporaryInfo.RelativeXSmooth - 10f;
                                float num17 = -99999f;
                                for (num12 = num14; num12 <= measureLineValue; num12 += 5)
                                {
                                    if ((notePack.ValidPitchCount <= 1) || ((num12 % 10) == 0))
                                    {
                                        measureLineRelativeYByLineValue = McPitch.GetMeasureLineRelativeYByLineValue(measure.ClefType, num12);
                                        DrawArpeggio(measure, new RawVector2(x, measureLineRelativeYByLineValue - 7f), color3);
                                        if (flag || (num17 < -99990f))
                                        {
                                            num17 = measureLineRelativeYByLineValue;
                                        }
                                    }
                                }
                                string text = flag ? "↑" : "↓";
                                _wicRenderTarget.DrawTextLayout(_writeFactory, text, new RawVector2(x - 7f, num17 - 4f), "Arial", 14f, color3, TextAlignment.Center, ParagraphAlignment.Center, SharpDX.DirectWrite.FontStyle.Normal, FontWeight.ExtraBlack);
                            }
                        }
                        if (notePack.TemporaryInfo.TripletNotePacks != null)
                        {
                            if (notePack == notePack.TemporaryInfo.TripletNotePacks[0])
                            {
                                DrawBeatGroupStemAndTailSmoothly(measure, lastBeatGroup, color);
                                lastBeatGroup.Clear();
                                duration = 0;
                                lastBeatGroup.Add(num6, notePack);
                            }
                            else if (notePack == notePack.TemporaryInfo.TripletNotePacks[1])
                            {
                                lastBeatGroup.Add(num6, notePack);
                            }
                            else if (notePack == notePack.TemporaryInfo.TripletNotePacks[2])
                            {
                                lastBeatGroup.Add(num6, notePack);
                                DrawBeatGroupStemAndTailSmoothly(measure, lastBeatGroup, color);
                                lastBeatGroup.Clear();
                                duration = 0;
                            }
                        }
                        else if (notePack.IsRest)
                        {
                            DrawBeatGroupStemAndTailSmoothly(measure, lastBeatGroup, color);
                            lastBeatGroup.Clear();
                            duration = 0;
                        }
                        else if (measure.InstrumentType == McMeasure.InstrumentTypes.Misc)
                        {
                            lastBeatGroup.Add(num6, notePack);
                            DrawBeatGroupStemAndTailSmoothly(measure, lastBeatGroup, Color.FromArgb(0x40, Color.Goldenrod));
                            lastBeatGroup.Clear();
                            duration = 0;
                        }
                        else if (notePack.Duration >= 0x10)
                        {
                            DrawBeatGroupStemAndTailSmoothly(measure, lastBeatGroup, color);
                            lastBeatGroup.Clear();
                            duration = 0;
                            lastBeatGroup.Add(num6, notePack);
                            DrawBeatGroupStemAndTailSmoothly(measure, lastBeatGroup, color);
                            lastBeatGroup.Clear();
                            duration = 0;
                        }
                        else
                        {
                            duration += notePack.Duration;
                            if (duration > num5)
                            {
                                DrawBeatGroupStemAndTailSmoothly(measure, lastBeatGroup, color);
                                lastBeatGroup.Clear();
                                lastBeatGroup.Add(num6, notePack);
                                duration = notePack.Duration;
                            }
                            else if ((duration == num5) || (num6 >= (stampArray.Length - 1)))
                            {
                                lastBeatGroup.Add(num6, notePack);
                                DrawBeatGroupStemAndTailSmoothly(measure, lastBeatGroup, color);
                                lastBeatGroup.Clear();
                                duration = 0;
                            }
                            else
                            {
                                lastBeatGroup.Add(num6, notePack);
                            }
                        }
                    }
                }
                if (lastBeatGroup.Count > 0)
                {
                    DrawBeatGroupStemAndTailSmoothly(measure, lastBeatGroup, color);
                }
                lastBeatGroup.Clear();
                for (num6 = 0; num6 < stampArray.Length; num6++)
                {
                    notePack = stampArray[num6].GetNotePack(index);
                    if ((notePack != null) && notePack.IsDotted)
                    {
                        float num18 = (num + (num6 * notePackWidth)) + 10;
                        float measureLineRelativeY = 0f;
                        if (notePack.IsRest)
                        {
                            switch (notePack.DurationType)
                            {
                                case McNotePack.DurationTypes.The32nd:
                                    num18 += 6f;
                                    break;

                                case McNotePack.DurationTypes.The16th:
                                    num18 += 6f;
                                    break;

                                case McNotePack.DurationTypes.Eighth:
                                    num18 += 4f;
                                    break;

                                case McNotePack.DurationTypes.Quarter:
                                    num18++;
                                    break;

                                case McNotePack.DurationTypes.Half:
                                    num18 += 6f;
                                    break;

                                case McNotePack.DurationTypes.Whole:
                                    num18 += 6f;
                                    break;
                            }
                            measureLineRelativeY = McPitch.GetMeasureFirstLineRelativeY(measure.ClefType) - (((float) (5 * McMeasure.StaveSpacing)) / 2f);
                        }
                        else if (notePack.TemporaryInfo.IsFlipVerticalStemVoted)
                        {
                            measureLineRelativeY = notePack.GetPitch(notePack.HighestPitchValue).MeasureLineRelativeY;
                        }
                        else
                        {
                            measureLineRelativeY = notePack.GetPitch(notePack.LowestPitchValue).MeasureLineRelativeY;
                        }
                        _wicRenderTarget.FillCircle(new RawVector2(num18, measureLineRelativeY - 1f), 2f, Color.LightPink);
                    }
                }
            }

            private static void DrawNotePackStem(McMeasure measure, McNotePack notePack, int stampNum, Color color, float startY, float endY, bool drawTail = true)
            {
                if ((notePack.IsViald && notePack.HasStem) && (Math.Abs((float) (endY - startY)) >= 4f))
                {
                    PointF[] pathPoints;
                    int num7;
                    int tailCount = notePack.TailCount;
                    float x = notePack.TemporaryInfo.RelativeXSmooth + 8f;
                    int num3 = ((endY - startY) > 0f) ? -1 : 1;
                    float num4 = Math.Abs((float) (startY - endY));
                    float num5 = (num3 < 0) ? (x - 5f) : (x + 6f);
                    float num6 = startY - 1f;
                    using (GraphicsPath path = new GraphicsPath())
                    {
                        path.AddLine(num5, num6, num5 + 1f, num6 - (num4 * num3));
                        path.AddLine((float) (num5 + 1f), (float) (num6 - (num4 * num3)), (float) (num5 - 1f), (float) (num6 - (num4 * num3)));
                        path.AddLine(num5 - 1f, num6 - (num4 * num3), num5 - 1f, num6);
                        path.AddLine(num5 - 1f, num6, num5, num6);
                        path.Flatten();
                        pathPoints = path.PathPoints;
                        path.Dispose();
                    }
                    using (SharpDX.Direct2D1.Factory factory = _wicRenderTarget.Factory)
                    {
                        PathGeometry geometry = new PathGeometry(factory);
                        if (pathPoints.Length > 1)
                        {
                            GeometrySink sink = geometry.Open();
                            sink.SetSegmentFlags(PathSegment.ForceRoundLineJoin);
                            sink.BeginFigure(new RawVector2(pathPoints[0].X, pathPoints[0].Y), FigureBegin.Filled);
                            num7 = 1;
                            while (num7 < pathPoints.Length)
                            {
                                sink.AddLine(new RawVector2(pathPoints[num7].X, pathPoints[num7].Y));
                                num7++;
                            }
                            sink.EndFigure(FigureEnd.Closed);
                            sink.Close();
                            sink.Dispose();
                        }
                        SolidColorBrush brush = new SolidColorBrush(_wicRenderTarget, color.ToRawColor4(1f));
                        _wicRenderTarget.FillGeometry(geometry, brush);
                        brush.Dispose();
                        geometry.Dispose();
                    }
                    if (drawTail)
                    {
                        for (num7 = 0; num7 < tailCount; num7++)
                        {
                            DrawNotePackTail(new RawVector2(x, (num6 - ((num4 - 10f) * num3)) - ((num7 * num3) * 6)), color, num3 < 0);
                        }
                    }
                }
            }

            private static void DrawNotePackTail(RawVector2 pos, Color color, bool flipVertical = false)
            {
                PointF[] pathPoints;
                using (GraphicsPath path = new GraphicsPath())
                {
                    int num = flipVertical ? -1 : 1;
                    float num2 = flipVertical ? (pos.X + (5 * num)) : (pos.X + (6 * num));
                    float y = pos.Y;
                    path.AddBezier(num2, y, num2, y + (7 * num), num2 + 15f, y + (8 * num), num2 + 8f, y + (20 * num));
                    path.AddBezier(num2 + 8f, y + (20 * num), num2 + 13f, y + (8 * num), num2, y + (7 * num), num2, y + (6 * num));
                    path.Flatten();
                    pathPoints = path.PathPoints;
                    path.Dispose();
                }
                using (SharpDX.Direct2D1.Factory factory = _wicRenderTarget.Factory)
                {
                    PathGeometry geometry = new PathGeometry(factory);
                    if (pathPoints.Length > 1)
                    {
                        GeometrySink sink = geometry.Open();
                        sink.SetSegmentFlags(PathSegment.ForceRoundLineJoin);
                        sink.BeginFigure(new RawVector2(pathPoints[0].X, pathPoints[0].Y), FigureBegin.Filled);
                        for (int i = 1; i < pathPoints.Length; i++)
                        {
                            sink.AddLine(new RawVector2(pathPoints[i].X, pathPoints[i].Y));
                        }
                        sink.EndFigure(FigureEnd.Closed);
                        sink.Close();
                        sink.Dispose();
                    }
                    SolidColorBrush brush = new SolidColorBrush(_wicRenderTarget, color.ToRawColor4(1f));
                    _wicRenderTarget.FillGeometry(geometry, brush);
                    brush.Dispose();
                    geometry.Dispose();
                }
            }

            private static void DrawPitch(McMeasure measure, McNotePack notePack, int pitchValue, int stampNum, Color color)
            {
                int num = McMeasure.MeasureHeadWidth + measure.KeySignatureZoneWidth;
                int notePackWidth = McNotePack.NotePackWidth;
                McPitch pitch = notePack.GetPitch(pitchValue);
                float num3 = num + (stampNum * notePackWidth);
                float measureLineRelativeY = pitch.MeasureLineRelativeY;
                float opacity = ((notePack.Index + 1) > McMeasure.NotePackMaxAmount) ? 0.25f : 1f;
                RawVector2 vector = new RawVector2(num3 - 8f, measureLineRelativeY - 8f);
                bool flag = notePack.GetPitchType(pitchValue) == McPitch.PitchTypes.Temporary;
                SharpDX.Direct2D1.Bitmap bitmap = (measure.InstrumentType == McMeasure.InstrumentTypes.Misc) ? BitmapMiscNoteCache : GetBitmapCommonNoteCache(notePack.DurationType);
                float num6 = flag ? (!notePack.CanAppendNewPitch ? 0.1f : 0.5f) : (((notePack == McHoverInfo.HoveringNotePack) && (pitchValue == McHoverInfo.HoveringInsertPitchValue)) ? 0.5f : 1f);
                notePack.TemporaryInfo.PitchIndexRelativePosMap.Add(pitchValue, vector);
                notePack.TemporaryInfo.RelativeX = vector.X;
                _wicRenderTarget.DrawBitmap(bitmap, new RawVector2(notePack.TemporaryInfo.RelativeXSmooth, vector.Y), num6 * opacity);
                RawVector2 pos = new RawVector2(num3 - 16f, measureLineRelativeY - 31f);
                bool flag2 = pitch.AlterantType == pitch.RawAlterantType;
                switch (pitch.AlterantType)
                {
                    case McPitch.AlterantTypes.NoControl:
                        switch (pitch.AlterantState)
                        {
                            case McPitch.AlterantStates.UnalteredSharp:
                                _wicRenderTarget.DrawBitmap(BitmapKeySignatureSharpLiteWhiteCache, pos, null, 0.2f * opacity, SharpDX.Direct2D1.BitmapInterpolationMode.NearestNeighbor);
                                return;

                            case McPitch.AlterantStates.UnalteredFlat:
                                _wicRenderTarget.DrawBitmap(BitmapKeySignatureFlatLiteWhiteCache, pos, null, 0.2f * opacity, SharpDX.Direct2D1.BitmapInterpolationMode.NearestNeighbor);
                                return;

                            case McPitch.AlterantStates.AlteredSharp:
                                _wicRenderTarget.DrawBitmap(BitmapKeySignatureSharpLiteWhiteCache, pos, null, opacity, SharpDX.Direct2D1.BitmapInterpolationMode.NearestNeighbor);
                                return;

                            case McPitch.AlterantStates.AlteredFlat:
                                _wicRenderTarget.DrawBitmap(BitmapKeySignatureFlatLiteWhiteCache, pos, null, opacity, SharpDX.Direct2D1.BitmapInterpolationMode.NearestNeighbor);
                                return;
                        }
                        break;

                    case McPitch.AlterantTypes.Natural:
                        _wicRenderTarget.DrawBitmap(BitmapNaturalSignCache, pos, null, flag2 ? opacity : (0.2f * opacity), SharpDX.Direct2D1.BitmapInterpolationMode.NearestNeighbor);
                        break;

                    case McPitch.AlterantTypes.Sharp:
                        _wicRenderTarget.DrawBitmap(BitmapKeySignatureSharpLiteCache, pos, null, flag2 ? opacity : (0.2f * opacity), SharpDX.Direct2D1.BitmapInterpolationMode.NearestNeighbor);
                        break;

                    case McPitch.AlterantTypes.Flat:
                        _wicRenderTarget.DrawBitmap(BitmapKeySignatureFlatLiteCache, pos, null, flag2 ? opacity : (0.2f * opacity), SharpDX.Direct2D1.BitmapInterpolationMode.NearestNeighbor);
                        break;
                }
            }

            private static SharpDX.Direct2D1.Bitmap GetBitmapCommonNoteCache(McNotePack.DurationTypes durType)
            {
                if (_bitmapCommonNoteCaches.ContainsKey(durType))
                {
                    return _bitmapCommonNoteCaches[durType];
                }
                SharpDX.Direct2D1.Bitmap bitmap = null;
                switch (durType)
                {
                    case McNotePack.DurationTypes.The32nd:
                    case McNotePack.DurationTypes.The16th:
                    case McNotePack.DurationTypes.Eighth:
                    case McNotePack.DurationTypes.Quarter:
                        bitmap = McNotePack.RedrawCommonNoteBitmapCache(_wicRenderTarget, McNotePack.NoteColorWhite, false, false);
                        break;

                    case McNotePack.DurationTypes.Half:
                        bitmap = McNotePack.RedrawCommonNoteBitmapCache(_wicRenderTarget, McNotePack.NoteColorWhite, false, true);
                        break;

                    case McNotePack.DurationTypes.Whole:
                        bitmap = McNotePack.RedrawCommonNoteBitmapCache(_wicRenderTarget, McNotePack.NoteColorWhite, true, true);
                        break;
                }
                if (bitmap != null)
                {
                    _bitmapCommonNoteCaches.Add(durType, bitmap);
                }
                return bitmap;
            }

            private static SharpDX.Direct2D1.Bitmap GetBitmapRestNoteCache(McNotePack.DurationTypes durType)
            {
                if (_bitmapRestNoteCaches.ContainsKey(durType))
                {
                    return _bitmapRestNoteCaches[durType];
                }
                SharpDX.Direct2D1.Bitmap bitmap = McNotePack.RedrawRestNoteBitmapCache(_wicRenderTarget, McNotePack.NoteColorWhite, durType);
                if (bitmap != null)
                {
                    _bitmapRestNoteCaches.Add(durType, bitmap);
                }
                return bitmap;
            }

            private static void ResetBitmapCaches(int width)
            {
                if (!((_wicBitmap == null) || _wicBitmap.IsDisposed))
                {
                    _wicBitmap.Dispose();
                }
                _wicBitmap = new SharpDX.WIC.Bitmap(_wicFactory, width, McMeasure.Height, SharpDX.WIC.PixelFormat.Format32bppPBGRA, BitmapCreateCacheOption.CacheOnDemand);
                if (!((_wicRenderTarget == null) || _wicRenderTarget.IsDisposed))
                {
                    _wicRenderTarget.Dispose();
                }
                _wicRenderTarget = new WicRenderTarget(_factory, _wicBitmap, _renderTargetProperties);
                if (!((_bitmapMiscNoteCache == null) || _bitmapMiscNoteCache.IsDisposed))
                {
                    _bitmapMiscNoteCache.Dispose();
                }
                _bitmapMiscNoteCache = null;
                foreach (SharpDX.Direct2D1.Bitmap bitmap in from bitmap in _bitmapCommonNoteCaches.Values.ToArray<SharpDX.Direct2D1.Bitmap>()
                    where (bitmap != null) && !bitmap.IsDisposed
                    select bitmap)
                {
                    bitmap.Dispose();
                }
                _bitmapCommonNoteCaches.Clear();
                foreach (SharpDX.Direct2D1.Bitmap bitmap in from bitmap in _bitmapRestNoteCaches.Values.ToArray<SharpDX.Direct2D1.Bitmap>()
                    where (bitmap != null) && !bitmap.IsDisposed
                    select bitmap)
                {
                    bitmap.Dispose();
                }
                _bitmapRestNoteCaches.Clear();
                if (!((_bitmapL2GCache == null) || _bitmapL2GCache.IsDisposed))
                {
                    _bitmapL2GCache.Dispose();
                }
                _bitmapL2GCache = null;
                if (!((_bitmapL4FCache == null) || _bitmapL4FCache.IsDisposed))
                {
                    _bitmapL4FCache.Dispose();
                }
                _bitmapL4FCache = null;
                if (!((_bitmapKeySignatureSharpCache == null) || _bitmapKeySignatureSharpCache.IsDisposed))
                {
                    _bitmapKeySignatureSharpCache.Dispose();
                }
                _bitmapKeySignatureSharpCache = null;
                if (!((_bitmapKeySignatureFlatCache == null) || _bitmapKeySignatureFlatCache.IsDisposed))
                {
                    _bitmapKeySignatureFlatCache.Dispose();
                }
                _bitmapKeySignatureFlatCache = null;
                if (!((_bitmapKeySignatureSharpLiteCache == null) || _bitmapKeySignatureSharpLiteCache.IsDisposed))
                {
                    _bitmapKeySignatureSharpLiteCache.Dispose();
                }
                _bitmapKeySignatureSharpLiteCache = null;
                if (!((_bitmapKeySignatureFlatLiteCache == null) || _bitmapKeySignatureFlatLiteCache.IsDisposed))
                {
                    _bitmapKeySignatureFlatLiteCache.Dispose();
                }
                _bitmapKeySignatureFlatLiteCache = null;
                if (!((_bitmapNaturalSignCache == null) || _bitmapNaturalSignCache.IsDisposed))
                {
                    _bitmapNaturalSignCache.Dispose();
                }
                _bitmapNaturalSignCache = null;
            }

            private static SharpDX.Direct2D1.Bitmap BitmapKeySignatureFlatCache
            {
                get
                {
                    if (_bitmapKeySignatureFlatCache == null)
                    {
                        _bitmapKeySignatureFlatCache = McMeasure.RedrawKeySignatureBitmapCache(_wicRenderTarget, McMeasure.KeySignatureColor, false, false);
                    }
                    return _bitmapKeySignatureFlatCache;
                }
            }

            private static SharpDX.Direct2D1.Bitmap BitmapKeySignatureFlatLiteCache
            {
                get
                {
                    if (_bitmapKeySignatureFlatLiteCache == null)
                    {
                        _bitmapKeySignatureFlatLiteCache = McMeasure.RedrawKeySignatureBitmapCache(_wicRenderTarget, McMeasure.KeySignatureColor, false, true);
                    }
                    return _bitmapKeySignatureFlatLiteCache;
                }
            }

            private static SharpDX.Direct2D1.Bitmap BitmapKeySignatureFlatLiteWhiteCache
            {
                get
                {
                    if (_bitmapKeySignatureFlatLiteWhiteCache == null)
                    {
                        _bitmapKeySignatureFlatLiteWhiteCache = McMeasure.RedrawKeySignatureBitmapCache(_wicRenderTarget, McMeasure.KeySignatureColorWhite, false, true);
                    }
                    return _bitmapKeySignatureFlatLiteWhiteCache;
                }
            }

            private static SharpDX.Direct2D1.Bitmap BitmapKeySignatureSharpCache
            {
                get
                {
                    if (_bitmapKeySignatureSharpCache == null)
                    {
                        _bitmapKeySignatureSharpCache = McMeasure.RedrawKeySignatureBitmapCache(_wicRenderTarget, McMeasure.KeySignatureColor, true, false);
                    }
                    return _bitmapKeySignatureSharpCache;
                }
            }

            private static SharpDX.Direct2D1.Bitmap BitmapKeySignatureSharpLiteCache
            {
                get
                {
                    if (_bitmapKeySignatureSharpLiteCache == null)
                    {
                        _bitmapKeySignatureSharpLiteCache = McMeasure.RedrawKeySignatureBitmapCache(_wicRenderTarget, McMeasure.KeySignatureColor, true, true);
                    }
                    return _bitmapKeySignatureSharpLiteCache;
                }
            }

            private static SharpDX.Direct2D1.Bitmap BitmapKeySignatureSharpLiteWhiteCache
            {
                get
                {
                    if (_bitmapKeySignatureSharpLiteWhiteCache == null)
                    {
                        _bitmapKeySignatureSharpLiteWhiteCache = McMeasure.RedrawKeySignatureBitmapCache(_wicRenderTarget, McMeasure.KeySignatureColorWhite, true, true);
                    }
                    return _bitmapKeySignatureSharpLiteWhiteCache;
                }
            }

            private static SharpDX.Direct2D1.Bitmap BitmapL2GCache
            {
                get
                {
                    if (_bitmapL2GCache == null)
                    {
                        _bitmapL2GCache = _wicRenderTarget.LoadBitmapFromResourceName("Images.ClefL2G.png");
                    }
                    return _bitmapL2GCache;
                }
            }

            private static SharpDX.Direct2D1.Bitmap BitmapL4FCache
            {
                get
                {
                    if (_bitmapL4FCache == null)
                    {
                        _bitmapL4FCache = _wicRenderTarget.LoadBitmapFromResourceName("Images.ClefL4F.png");
                    }
                    return _bitmapL4FCache;
                }
            }

            private static SharpDX.Direct2D1.Bitmap BitmapMiscNoteCache
            {
                get
                {
                    if (_bitmapMiscNoteCache == null)
                    {
                        _bitmapMiscNoteCache = McNotePack.RedrawMiscNoteBitmapCache(_wicRenderTarget, Color.Goldenrod);
                    }
                    return _bitmapMiscNoteCache;
                }
            }

            private static SharpDX.Direct2D1.Bitmap BitmapNaturalSignCache
            {
                get
                {
                    if (_bitmapNaturalSignCache == null)
                    {
                        _bitmapNaturalSignCache = McMeasure.RedrawNaturalSignBitmapCache(_wicRenderTarget, McMeasure.NaturalSignColor);
                    }
                    return _bitmapNaturalSignCache;
                }
            }
        }

        public class MsAlignedTemporaryInfo
        {
            private int[] _durationStampMaxAligned = new int[3];
            private int _width = 0;

            public MsAlignedTemporaryInfo(McMeasure[] measuresAligned)
            {
                this.ParentMeasuresAligned = measuresAligned;
                this.ReorganizedDurationStamps = new SortedDictionary<int, McNotation.DurationStamp>();
            }

            public void ReorganizeDurationStamps(McMeasure measure)
            {
                int index = measure.ParentRegularTrack.Index;
                foreach (McNotation.DurationStamp stamp in this.ReorganizedDurationStamps.Values.ToArray<McNotation.DurationStamp>())
                {
                    stamp.RemoveNotePack(index);
                }
                int key = 0;
                McNotation parentNotation = measure.ParentRegularTrack.ParentNotation;
                int num3 = parentNotation.BeatDuration * parentNotation.BeatsPerMeasure;
                int num4 = num3 / 8;
                VolumeCurve volumeCurve = measure.VolumeCurve;
                foreach (McNotePack pack in measure.NotePacks)
                {
                    pack.TripletDurationType = null;
                    pack.TripletDuration = null;
                    pack.TemporaryInfo.TripletNotePacks = null;
                }
                for (int i = 0; i < measure.NotePacks.Length; i++)
                {
                    pack = measure.NotePacks[i];
                    if (pack.Triplet && (i <= (measure.NotePacks.Length - 3)))
                    {
                        bool flag = true;
                        for (int j = i + 1; j < (i + 3); j++)
                        {
                            McNotePack pack2 = measure.NotePacks[j];
                            if (pack2.Triplet)
                            {
                                flag = false;
                                break;
                            }
                        }
                        if (flag)
                        {
                            McNotePack pack3 = measure.NotePacks[i + 1];
                            McNotePack pack4 = measure.NotePacks[i + 2];
                            McNotePack[] packArray = new McNotePack[] { pack, pack3, pack4 };
                            pack.TemporaryInfo.TripletNotePacks = packArray;
                            pack3.TemporaryInfo.TripletNotePacks = packArray;
                            pack4.TemporaryInfo.TripletNotePacks = packArray;
                            int num7 = (int) Math.Round((double) (((float) (pack.DurationRaw * 2)) / 3f));
                            pack.TripletDuration = new int?(num7);
                            pack3.TripletDuration = new int?(num7);
                            pack4.TripletDuration = new int?((pack.DurationRaw * 2) - (num7 * 2));
                            pack.TripletDurationType = new McNotePack.DurationTypes?(pack.DurationTypeRaw);
                            pack3.TripletDurationType = new McNotePack.DurationTypes?(pack.DurationTypeRaw);
                            pack4.TripletDurationType = new McNotePack.DurationTypes?(pack.DurationTypeRaw);
                        }
                    }
                    McNotation.DurationStamp stamp2 = null;
                    if (!this.ReorganizedDurationStamps.ContainsKey(key))
                    {
                        stamp2 = new McNotation.DurationStamp(measure, key, null);
                        this.ReorganizedDurationStamps.Add(key, stamp2);
                    }
                    else
                    {
                        stamp2 = this.ReorganizedDurationStamps[key];
                    }
                    stamp2.SetNotePack(index, pack);
                    int num8 = key / num4;
                    int num9 = key % num4;
                    float curvedVolume = volumeCurve.GetCurvedVolume(num8);
                    float num11 = volumeCurve.GetCurvedVolume(num8 + 1);
                    pack._SetVolumeRaw(((float) (curvedVolume + ((num11 - curvedVolume) * (((float) num9) / ((float) num4))))).Clamp((float) 0f, (float) 1f));
                    key += pack.Duration;
                }
                this._durationStampMaxAligned[index] = key - 1;
                this.Width = ((McMeasure.MeasureHeadWidth + measure.KeySignatureZoneWidth) + (McNotePack.NotePackWidth * this.ReorganizedDurationStamps.Count)) + McMeasure.MeasureTailWidth;
            }

            public int DurationStampMax =>
                this._durationStampMaxAligned.Max();

            public int[] DurationStampMaxAligned =>
                this._durationStampMaxAligned;

            public McMeasure FirstMeasure =>
                this.ParentMeasuresAligned.FirstOrDefault<McMeasure>(measure => (measure != null));

            public McMeasure[] ParentMeasuresAligned { get; private set; }

            public int RelativeX { get; set; }

            public SortedDictionary<int, McNotation.DurationStamp> ReorganizedDurationStamps { get; set; }

            public int Width
            {
                get => 
                    this._width;
                set
                {
                    int num = value - this._width;
                    this._width = value;
                    if (num != 0)
                    {
                        McMeasure firstMeasure = this.FirstMeasure;
                        int index = firstMeasure.Index;
                        int num3 = firstMeasure.ParentRegularTrack.MeasuresCount - 1;
                        for (int i = index + 1; i <= num3; i++)
                        {
                            McMeasure.MsAlignedTemporaryInfo temporaryInfo = firstMeasure.ParentRegularTrack.GetMeasure(i).TemporaryInfo;
                            temporaryInfo.RelativeX += num;
                        }
                    }
                }
            }
        }
    }
}

