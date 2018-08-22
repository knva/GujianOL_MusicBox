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

    public class McNotePack
    {
        private Arpeggio.ArpeggioTypes _arpeggioMode = Arpeggio.ArpeggioTypes.None;
        private DurationTypes _durationType = DurationTypes.Quarter;
        private bool _isDotted = false;
        private bool _isRest = false;
        private readonly McPitch[] _pitches = new McPitch[McPitch.PitchMax + 1];
        private bool _staccato = false;
        private TieTypes _tieType = TieTypes.None;
        private bool _triplet = false;
        private int? _tripletDuration = null;
        private DurationTypes? _tripletDurationType = null;
        private float _volume = 1f;
        public static readonly int MaxPitchAllowed = 4;
        public static readonly Color NoteColorWhite = Color.FromArgb(210, Color.GhostWhite);
        public static readonly int NotePackWidth = 50;

        public McNotePack(McMeasure parentMeasure)
        {
            this.ParentMeasure = parentMeasure;
            this.TemporaryInfo = new NpTemporaryInfo(this);
            if (parentMeasure != null)
            {
                McUtility.MarkModified(parentMeasure);
            }
        }

        public void _SetVolumeRaw(float volume)
        {
            this._volume = volume.Clamp(0f, 1f);
            McUtility.MarkModified(this.ParentMeasure);
        }

        public void ClearTemporaryPitch()
        {
            for (int i = McPitch.PitchMin; i <= McPitch.PitchMax; i++)
            {
                McPitch pitch = this._pitches[i];
                if ((pitch != null) && (pitch.PitchType == McPitch.PitchTypes.Temporary))
                {
                    this.MarkPitch(i, McPitch.PitchTypes.Disabled);
                }
            }
        }

        public McNotePack Clone(McMeasure measure)
        {
            McNotePack pack = new McNotePack(measure ?? this.ParentMeasure) {
                _isRest = this._isRest,
                _isDotted = this._isDotted,
                _tieType = this._tieType,
                _triplet = this._triplet,
                _durationType = this._durationType,
                _staccato = this._staccato,
                _arpeggioMode = this._arpeggioMode
            };
            foreach (int num in this.ValidPitchValueArray)
            {
                McPitch pitch = this._pitches[num];
                if (((pitch != null) && (pitch.PitchType == McPitch.PitchTypes.Enabled)) && pack.MarkPitch(pitch.Value, McPitch.PitchTypes.Enabled))
                {
                    pack.GetPitch(pitch.Value).AlterantType = pitch.RawAlterantType;
                }
            }
            return pack;
        }

        private static void DrawRestDotPart(RenderTarget renderTarget, RawVector2 pos, Color color)
        {
            PointF[] pathPoints;
            using (GraphicsPath path = new GraphicsPath())
            {
                path.AddBezier(pos.X, pos.Y + 3f, pos.X - 2f, pos.Y + 6f, pos.X - 8f, pos.Y + 8f, pos.X - 12f, pos.Y + 2f);
                path.AddBezier((float) (pos.X - 12f), (float) (pos.Y + 2f), (float) (pos.X - 8f), (float) (pos.Y + 6.5f), (float) (pos.X - 2f), (float) (pos.Y + 5f), (float) (pos.X + 1f), (float) (pos.Y - 1f));
                path.Flatten();
                pathPoints = path.PathPoints;
                path.Dispose();
            }
            using (SharpDX.Direct2D1.Factory factory = renderTarget.Factory)
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
                SolidColorBrush brush = new SolidColorBrush(renderTarget, color.ToRawColor4(1f));
                renderTarget.FillGeometry(geometry, brush);
                brush.Dispose();
                geometry.Dispose();
            }
            renderTarget.FillCircle(new RawVector2(pos.X - 8f, pos.Y + 2f), 3f, color);
        }

        public McPitch GetPitch(int pitchValue)
        {
            if ((pitchValue < McPitch.PitchMin) || (pitchValue > McPitch.PitchMax))
            {
                return null;
            }
            return this._pitches[pitchValue];
        }

        public McPitch.PitchTypes GetPitchType(int pitchValue)
        {
            if ((pitchValue < McPitch.PitchMin) || (pitchValue > McPitch.PitchMax))
            {
                return McPitch.PitchTypes.Disabled;
            }
            McPitch pitch = this._pitches[pitchValue];
            return ((pitch == null) ? McPitch.PitchTypes.Disabled : pitch.PitchType);
        }

        public bool IsPitchEnabled(int pitchValue)
        {
            if ((pitchValue < McPitch.PitchMin) || (pitchValue > McPitch.PitchMax))
            {
                return false;
            }
            McPitch pitch = this._pitches[pitchValue];
            return ((pitch != null) && (pitch.PitchType != McPitch.PitchTypes.Disabled));
        }

        public bool MarkPitch(int pitchValue, McPitch.PitchTypes pitchType)
        {
            if ((pitchValue < McPitch.PitchMin) || (pitchValue > McPitch.PitchMax))
            {
                return false;
            }
            if (!((pitchType != McPitch.PitchTypes.Enabled) || this.CanAppendNewPitch))
            {
                return false;
            }
            McPitch pitch = this._pitches[pitchValue];
            McPitch.PitchTypes disabled = McPitch.PitchTypes.Disabled;
            if (pitch != null)
            {
                disabled = pitch.PitchType;
            }
            if ((disabled != pitchType) && this.IsViald)
            {
                this._pitches[pitchValue] = (pitchType != McPitch.PitchTypes.Disabled) ? McPitch.FromNaturalPitchValue(this, pitchValue, pitchType) : null;
                int num = (this.ParentMeasure.ClefType == McMeasure.ClefTypes.L2G) ? 0x2c : 0x17;
                int? nullable = null;
                int? nullable2 = null;
                for (int i = McPitch.PitchMin; i <= McPitch.PitchMax; i++)
                {
                    if (this._pitches[i] != null)
                    {
                        if (!nullable.HasValue)
                        {
                            nullable = new int?(i);
                        }
                        nullable2 = new int?(i);
                    }
                }
                int? nullable3 = nullable2;
                this.HighestPitchValue = nullable3.HasValue ? nullable3.GetValueOrDefault() : num;
                nullable3 = nullable;
                this.LowestPitchValue = nullable3.HasValue ? nullable3.GetValueOrDefault() : num;
                if (this.IsViald)
                {
                    if (((disabled == McPitch.PitchTypes.Temporary) && (pitchType == McPitch.PitchTypes.Disabled)) || ((disabled == McPitch.PitchTypes.Disabled) && (pitchType == McPitch.PitchTypes.Temporary)))
                    {
                        McUtility.AppendRedrawingMeasure(this.ParentMeasure);
                    }
                    else
                    {
                        McUtility.MarkModified(this.ParentMeasure);
                    }
                }
            }
            return true;
        }

        public static SharpDX.Direct2D1.Bitmap RedrawCommonNoteBitmapCache(RenderTarget renderTargetSource, Color color, bool flipVertical, bool hollow)
        {
            ImagingFactory factory = new ImagingFactory();
            SharpDX.Direct2D1.Factory factory2 = new SharpDX.Direct2D1.Factory();
            SharpDX.DirectWrite.Factory factory3 = new SharpDX.DirectWrite.Factory();
            SharpDX.WIC.Bitmap wicBitmap = new SharpDX.WIC.Bitmap(factory, 0x10, 0x10, SharpDX.WIC.PixelFormat.Format32bppPBGRA, BitmapCreateCacheOption.CacheOnDemand);
            RenderTargetProperties renderTargetProperties = new RenderTargetProperties(RenderTargetType.Default, new SharpDX.Direct2D1.PixelFormat(Format.Unknown, SharpDX.Direct2D1.AlphaMode.Unknown), 0f, 0f, RenderTargetUsage.None, FeatureLevel.Level_DEFAULT);
            WicRenderTarget renderTarget = new WicRenderTarget(factory2, wicBitmap, renderTargetProperties);
            try
            {
                PointF[] pathPoints;
                renderTarget.BeginDraw();
                renderTarget.Clear(new RawColor4?(Color.Transparent.ToRawColor4(1f)));
                RawVector2 vector = new RawVector2(8f, 8f);
                using (GraphicsPath path = new GraphicsPath())
                {
                    int num = flipVertical ? -1 : 1;
                    float num2 = vector.X - 6f;
                    float y = vector.Y;
                    path.AddBezier(num2, y, num2 + 2f, y - (6 * num), (num2 + 12f) + 2f, y - (6 * num), num2 + 12f, y);
                    path.AddBezier(num2 + 12f, y, (num2 + 12f) - 2f, y + (6 * num), num2 - 2f, y + (6 * num), num2, y);
                    if (hollow)
                    {
                        path.AddLine(num2, y, num2 + 4f, y);
                        path.AddBezier(num2 + 4f, y, num2 + 5f, y - (2 * num), (num2 + 12f) + 0f, y - (6 * num), (num2 + 12f) - 4f, y);
                        path.AddBezier((num2 + 12f) - 4f, y, (num2 + 12f) - 5f, y + (2 * num), num2 - 0f, y + (6 * num), num2 + 4f, y);
                        path.AddLine(num2 + 4f, y, num2, y);
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

        public static SharpDX.Direct2D1.Bitmap RedrawMiscNoteBitmapCache(RenderTarget renderTargetSource, Color color)
        {
            ImagingFactory factory = new ImagingFactory();
            SharpDX.Direct2D1.Factory factory2 = new SharpDX.Direct2D1.Factory();
            SharpDX.DirectWrite.Factory factory3 = new SharpDX.DirectWrite.Factory();
            SharpDX.WIC.Bitmap wicBitmap = new SharpDX.WIC.Bitmap(factory, 0x10, 0x10, SharpDX.WIC.PixelFormat.Format32bppPBGRA, BitmapCreateCacheOption.CacheOnDemand);
            RenderTargetProperties renderTargetProperties = new RenderTargetProperties(RenderTargetType.Default, new SharpDX.Direct2D1.PixelFormat(Format.Unknown, SharpDX.Direct2D1.AlphaMode.Unknown), 0f, 0f, RenderTargetUsage.None, FeatureLevel.Level_DEFAULT);
            WicRenderTarget renderTarget = new WicRenderTarget(factory2, wicBitmap, renderTargetProperties);
            try
            {
                PointF[] pathPoints;
                renderTarget.BeginDraw();
                renderTarget.Clear(new RawColor4?(Color.Transparent.ToRawColor4(1f)));
                RawVector2 vector = new RawVector2(8f, 8f);
                using (GraphicsPath path = new GraphicsPath())
                {
                    float num = vector.X - 6f;
                    float num2 = vector.Y - 3f;
                    path.AddLine((float) (num + 0f), (float) (num2 + 0f), (float) (num + 12f), (float) (num2 + 0f));
                    path.AddLine((float) (num + 12f), (float) (num2 + 0f), (float) (num + 12f), (float) (num2 + 6f));
                    path.AddLine((float) (num + 12f), (float) (num2 + 6f), (float) (num + 0f), (float) (num2 + 6f));
                    path.AddLine((float) (num + 0f), (float) (num2 + 6f), (float) (num + 0f), (float) (num2 + 0f));
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

        public static SharpDX.Direct2D1.Bitmap RedrawRestNoteBitmapCache(RenderTarget renderTargetSource, Color color, DurationTypes durationType)
        {
            ImagingFactory factory = new ImagingFactory();
            SharpDX.Direct2D1.Factory factory2 = new SharpDX.Direct2D1.Factory();
            SharpDX.DirectWrite.Factory factory3 = new SharpDX.DirectWrite.Factory();
            SharpDX.WIC.Bitmap wicBitmap = new SharpDX.WIC.Bitmap(factory, 0x20, 0x30, SharpDX.WIC.PixelFormat.Format32bppPBGRA, BitmapCreateCacheOption.CacheOnDemand);
            RenderTargetProperties renderTargetProperties = new RenderTargetProperties(RenderTargetType.Default, new SharpDX.Direct2D1.PixelFormat(Format.Unknown, SharpDX.Direct2D1.AlphaMode.Unknown), 0f, 0f, RenderTargetUsage.None, FeatureLevel.Level_DEFAULT);
            WicRenderTarget renderTarget = new WicRenderTarget(factory2, wicBitmap, renderTargetProperties);
            try
            {
                PointF[] pathPoints;
                renderTarget.BeginDraw();
                renderTarget.Clear(new RawColor4?(Color.Transparent.ToRawColor4(1f)));
                RawVector2 pos = new RawVector2(18f, 44f);
                using (GraphicsPath path = new GraphicsPath())
                {
                    DurationTypes types = durationType;
                    if (types <= DurationTypes.Eighth)
                    {
                        switch (types)
                        {
                            case DurationTypes.The32nd:
                                pos = new RawVector2(pos.X, (pos.Y - (8f * (((float) McMeasure.StaveSpacing) / 2f))) + 3f);
                                path.AddLine(pos.X, pos.Y, pos.X + 1.6f, pos.Y);
                                path.AddLine(pos.X + 1.6f, pos.Y, (pos.X - 8f) + 2.4f, pos.Y + 41f);
                                path.AddLine((float) ((pos.X - 8f) + 2.4f), (float) (pos.Y + 41f), (float) ((pos.X - 8f) - 0.8f), (float) (pos.Y + 41f));
                                path.AddLine((pos.X - 8f) - 0.8f, pos.Y + 41f, pos.X, pos.Y);
                                DrawRestDotPart(renderTarget, pos, color);
                                DrawRestDotPart(renderTarget, new RawVector2(pos.X - 2.2f, pos.Y + 11f), color);
                                DrawRestDotPart(renderTarget, new RawVector2(pos.X - 4.4f, pos.Y + 22f), color);
                                break;

                            case DurationTypes.The16th:
                                pos = new RawVector2(pos.X, (pos.Y - (6f * (((float) McMeasure.StaveSpacing) / 2f))) + 3f);
                                path.AddLine(pos.X, pos.Y, pos.X + 1.6f, pos.Y);
                                path.AddLine(pos.X + 1.6f, pos.Y, (pos.X - 8f) + 2.4f, pos.Y + 30f);
                                path.AddLine((float) ((pos.X - 8f) + 2.4f), (float) (pos.Y + 30f), (float) ((pos.X - 8f) - 0.8f), (float) (pos.Y + 30f));
                                path.AddLine((pos.X - 8f) - 0.8f, pos.Y + 30f, pos.X, pos.Y);
                                DrawRestDotPart(renderTarget, pos, color);
                                DrawRestDotPart(renderTarget, new RawVector2(pos.X - 2.4f, pos.Y + 11f), color);
                                break;

                            case DurationTypes.Eighth:
                                goto Label_06F6;
                        }
                    }
                    else
                    {
                        switch (types)
                        {
                            case DurationTypes.Quarter:
                                pos = new RawVector2(pos.X - 11f, pos.Y - (7f * (((float) McMeasure.StaveSpacing) / 2f)));
                                path.AddLine(pos.X, pos.Y, pos.X + 7f, pos.Y + 9.5f);
                                path.AddBezier((float) (pos.X + 7f), (float) (pos.Y + 9.5f), (float) (pos.X + 3f), (float) (pos.Y + 16f), (float) (pos.X + 3f), (float) (pos.Y + 16f), (float) (pos.X + 8f), (float) (pos.Y + 24f));
                                path.AddBezier((float) (pos.X + 8f), (float) (pos.Y + 24f), (float) (pos.X - 0f), (float) (pos.Y + 21f), (float) (pos.X - 1f), (float) (pos.Y + 26f), (float) (pos.X + 4f), (float) (pos.Y + 34f));
                                path.AddBezier((float) (pos.X + 4f), (float) (pos.Y + 34f), (float) (pos.X - 8f), (float) (pos.Y + 28f), (float) (pos.X - 4f), (float) (pos.Y + 17f), (float) (pos.X + 4.5f), (float) (pos.Y + 21f));
                                path.AddLine((float) (pos.X + 4.5f), (float) (pos.Y + 21f), (float) (pos.X - 1f), (float) (pos.Y + 14f));
                                path.AddBezier((float) (pos.X - 1f), (float) (pos.Y + 14f), (float) (pos.X + 3.5f), (float) (pos.Y + 11f), (float) (pos.X + 4f), (float) (pos.Y + 7f), (float) (pos.X - 1f), (float) (pos.Y + 2f));
                                goto Label_0AD7;

                            case DurationTypes.Half:
                                pos = new RawVector2(pos.X - 8f, pos.Y - (4f * (((float) McMeasure.StaveSpacing) / 2f)));
                                path.AddLine(pos.X - 7f, pos.Y, pos.X + 7f, pos.Y);
                                path.AddLine(pos.X + 7f, pos.Y, pos.X + 7f, pos.Y - 5f);
                                path.AddLine((float) (pos.X + 7f), (float) (pos.Y - 5f), (float) (pos.X - 7f), (float) (pos.Y - 5f));
                                path.AddLine(pos.X - 7f, pos.Y - 5f, pos.X - 7f, pos.Y);
                                path.AddLine(pos.X - 7f, pos.Y, pos.X - 10f, pos.Y);
                                path.AddLine(pos.X - 10f, pos.Y, pos.X - 10f, pos.Y + 1f);
                                path.AddLine((float) (pos.X - 10f), (float) (pos.Y + 1f), (float) (pos.X + 10f), (float) (pos.Y + 1f));
                                path.AddLine(pos.X + 10f, pos.Y + 1f, pos.X + 10f, pos.Y);
                                goto Label_0AD7;
                        }
                        if (types == DurationTypes.Whole)
                        {
                            pos = new RawVector2(pos.X - 8f, pos.Y - (6f * (((float) McMeasure.StaveSpacing) / 2f)));
                            path.AddLine(pos.X - 7f, pos.Y, pos.X + 7f, pos.Y);
                            path.AddLine(pos.X + 7f, pos.Y, pos.X + 7f, pos.Y + 5f);
                            path.AddLine((float) (pos.X + 7f), (float) (pos.Y + 5f), (float) (pos.X - 7f), (float) (pos.Y + 5f));
                            path.AddLine(pos.X - 7f, pos.Y + 5f, pos.X - 7f, pos.Y);
                            path.AddLine(pos.X - 7f, pos.Y, pos.X - 10f, pos.Y);
                            path.AddLine(pos.X - 10f, pos.Y, pos.X - 10f, pos.Y - 1f);
                            path.AddLine((float) (pos.X - 10f), (float) (pos.Y - 1f), (float) (pos.X + 10f), (float) (pos.Y - 1f));
                            path.AddLine(pos.X + 10f, pos.Y - 1f, pos.X + 10f, pos.Y);
                        }
                    }
                    goto Label_0AD7;
                Label_06F6:
                    pos = new RawVector2(pos.X - 2f, (pos.Y - (6f * (((float) McMeasure.StaveSpacing) / 2f))) + 3f);
                    path.AddLine(pos.X, pos.Y, pos.X + 1.6f, pos.Y);
                    path.AddLine(pos.X + 1.6f, pos.Y, (pos.X - 8f) + 2.4f, pos.Y + 19f);
                    path.AddLine((float) ((pos.X - 8f) + 2.4f), (float) (pos.Y + 19f), (float) ((pos.X - 8f) - 0.8f), (float) (pos.Y + 19f));
                    path.AddLine((pos.X - 8f) - 0.8f, pos.Y + 19f, pos.X, pos.Y);
                    DrawRestDotPart(renderTarget, pos, color);
                Label_0AD7:
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

        public Arpeggio.ArpeggioTypes ArpeggioMode
        {
            get => 
                ((this.IsRest || (this.ValidPitchCount < 3)) ? Arpeggio.ArpeggioTypes.None : this._arpeggioMode);
            set
            {
                this._arpeggioMode = value;
                McUtility.MarkModified(this.ParentMeasure);
            }
        }

        public Arpeggio.ArpeggioTypes ArpeggioModeRaw =>
            this._arpeggioMode;

        public bool CanAppendNewPitch =>
            (!this._isRest && (this.ValidPitchCount < MaxPitchAllowed));

        public MusicCanvasControl Canvas =>
            this.ParentMeasure.Canvas;

        public int Duration
        {
            get
            {
                int? nullable = this._tripletDuration;
                return (nullable.HasValue ? nullable.GetValueOrDefault() : this.DurationRaw);
            }
        }

        public int DurationRaw =>
            ((int) Math.Round((double) (((float) this.DurationTypeRaw) * (this.IsDotted ? 1.5f : 1f))));

        public DurationTypes DurationType
        {
            get
            {
                DurationTypes? nullable = this._tripletDurationType;
                return (nullable.HasValue ? nullable.GetValueOrDefault() : this._durationType);
            }
            set
            {
                this._durationType = value;
                McUtility.MarkModified(this.ParentMeasure);
            }
        }

        public DurationTypes DurationTypeRaw =>
            this._durationType;

        public int EnabledPitchCount
        {
            get
            {
                int num = 0;
                for (int i = McPitch.PitchMin; i <= McPitch.PitchMax; i++)
                {
                    McPitch pitch = this._pitches[i];
                    if ((pitch != null) && (pitch.PitchType != McPitch.PitchTypes.Disabled))
                    {
                        num++;
                    }
                }
                return num;
            }
        }

        public int[] EnabledPitchValueArray
        {
            get
            {
                List<int> list = new List<int>();
                for (int i = McPitch.PitchMin; i <= McPitch.PitchMax; i++)
                {
                    McPitch pitch = this._pitches[i];
                    if ((pitch != null) && (pitch.PitchType != McPitch.PitchTypes.Disabled))
                    {
                        list.Add(i);
                    }
                }
                return list.ToArray();
            }
        }

        public bool HasStem =>
            (!this.IsRest && (this.DurationType != DurationTypes.Whole));

        public int HighestPitchValue { get; private set; }

        public int Index =>
            ((this.ParentMeasure == null) ? -1 : this.ParentMeasure.IndexOfNotePack(this));

        public bool IsDotted
        {
            get => 
                this._isDotted;
            set
            {
                this._isDotted = value;
                McUtility.MarkModified(this.ParentMeasure);
            }
        }

        public bool IsFlipVertical =>
            (this.DurationType == DurationTypes.Whole);

        public bool IsFlipVerticalStem
        {
            get
            {
                McPitch pitch = this.GetPitch(this.HighestPitchValue);
                if ((pitch != null) && (pitch.MeasureLineValue < 20))
                {
                    return false;
                }
                McPitch pitch2 = this.GetPitch(this.LowestPitchValue);
                if ((pitch2 != null) && (pitch2.MeasureLineValue >= 20))
                {
                    return true;
                }
                if ((pitch == null) || (pitch2 == null))
                {
                    return false;
                }
                return ((pitch.MeasureLineValue - 20) >= (20 - pitch2.MeasureLineValue));
            }
        }

        public bool IsHollow
        {
            get
            {
                switch (this.DurationType)
                {
                    case DurationTypes.Half:
                    case DurationTypes.Whole:
                        return true;
                }
                return false;
            }
        }

        public bool IsHovering { get; set; }

        public bool IsRest
        {
            get => 
                (this._isRest || (this.EnabledPitchCount == 0));
            set
            {
                this._isRest = value;
                McUtility.MarkModified(this.ParentMeasure);
            }
        }

        public bool IsRestRaw =>
            this._isRest;

        public bool IsViald =>
            (this.ParentMeasure != null);

        public int LowestPitchValue { get; private set; }

        public McMeasure ParentMeasure { get; private set; }

        public bool Staccato
        {
            get => 
                this._staccato;
            set
            {
                this._staccato = value;
                McUtility.MarkModified(this.ParentMeasure);
            }
        }

        public int TailCount
        {
            get
            {
                switch (this.DurationType)
                {
                    case DurationTypes.The32nd:
                        return 3;

                    case DurationTypes.The16th:
                        return 2;

                    case DurationTypes.Eighth:
                        return 1;
                }
                return 0;
            }
        }

        public NpTemporaryInfo TemporaryInfo { get; private set; }

        public TieTypes TieType
        {
            get => 
                this._tieType;
            set
            {
                this._tieType = value;
                McUtility.MarkModified(this.ParentMeasure);
            }
        }

        public bool Triplet
        {
            get => 
                this._triplet;
            set
            {
                this._triplet = value;
                McUtility.MarkModified(this.ParentMeasure);
            }
        }

        public int? TripletDuration
        {
            get => 
                this._tripletDuration;
            set
            {
                this._tripletDuration = value;
            }
        }

        public DurationTypes? TripletDurationType
        {
            get => 
                this._tripletDurationType;
            set
            {
                this._tripletDurationType = value;
            }
        }

        public McPitch[] ValidPitchArray
        {
            get
            {
                List<McPitch> list = new List<McPitch>();
                for (int i = McPitch.PitchMin; i <= McPitch.PitchMax; i++)
                {
                    McPitch item = this._pitches[i];
                    if ((item != null) && (item.PitchType == McPitch.PitchTypes.Enabled))
                    {
                        list.Add(item);
                    }
                }
                return list.ToArray();
            }
        }

        public int ValidPitchCount
        {
            get
            {
                int num = 0;
                for (int i = McPitch.PitchMin; i <= McPitch.PitchMax; i++)
                {
                    McPitch pitch = this._pitches[i];
                    if ((pitch != null) && (pitch.PitchType == McPitch.PitchTypes.Enabled))
                    {
                        num++;
                    }
                }
                return num;
            }
        }

        public int[] ValidPitchValueArray
        {
            get
            {
                List<int> list = new List<int>();
                for (int i = McPitch.PitchMin; i <= McPitch.PitchMax; i++)
                {
                    McPitch pitch = this._pitches[i];
                    if ((pitch != null) && (pitch.PitchType == McPitch.PitchTypes.Enabled))
                    {
                        list.Add(i);
                    }
                }
                return list.ToArray();
            }
        }

        public float Volume =>
            ((float) (this._volume * this.ParentMeasure.Volume)).Clamp(((float) 0f), ((float) 1f));

        public float VolumeRaw =>
            this._volume;

        public enum DurationTypes
        {
            Eighth = 8,
            Half = 0x20,
            Quarter = 0x10,
            The16th = 4,
            The32nd = 2,
            Whole = 0x40
        }

        public class NpTemporaryInfo
        {
            private bool? _isFlipVerticalStemVoted = null;
            private int _playingDurationTimeMs = 0;
            private float _relativeX = -1f;
            private float _relativeXSmooth = -1f;
            private int _stampIndex = -1;
            private float _stemEndY = 0f;
            private float _stemEndYSmooth = 0f;
            private float _stemStartY = 0f;
            private float _stemStartYSmooth = 0f;
            private float _tieMarkerRelativeY = -1f;
            private float _tieMarkerRelativeYSmooth = 0f;
            private McNotePack[] _tripletNotePacks = null;
            public Dictionary<int, RawVector2> PitchIndexRelativePosMap = new Dictionary<int, RawVector2>();

            public NpTemporaryInfo(McNotePack parentNotePack)
            {
                this.ParentNotePack = parentNotePack;
                this.LinkedInTieNote = null;
                this.LinkedOutTieNote = null;
            }

            public void ClearVotedFlipVerticalStemState()
            {
                this._isFlipVerticalStemVoted = null;
            }

            public void UpdateTieTemporaryInfoData()
            {
                McNotePack parentNotePack = this.ParentNotePack;
                if (!parentNotePack.IsRest)
                {
                    int num = Math.Max(8, (int) (1000f / (((float) (parentNotePack.ParentMeasure.ParentRegularTrack.ParentNotation.BeatDuration * parentNotePack.ParentMeasure.BeatsPerMinute)) / 60f)));
                    parentNotePack.TemporaryInfo.PlayingDurationTimeMs = (int) ((float) ((parentNotePack.Duration * num) * 2f)).Clamp(((float) 240f), ((float) 999999f));
                    switch (parentNotePack.TieType)
                    {
                        case McNotePack.TieTypes.End:
                        case McNotePack.TieTypes.Both:
                        {
                            McNotePack linkedInTieNote = parentNotePack.TemporaryInfo.LinkedInTieNote;
                            if (linkedInTieNote != null)
                            {
                                bool flag = false;
                                int[] validPitchValueArray = linkedInTieNote.ValidPitchValueArray;
                                foreach (McPitch pitch in parentNotePack.ValidPitchArray)
                                {
                                    flag = validPitchValueArray.Contains<int>(pitch.Value);
                                    pitch.TieVolumeAlter = flag ? 0f : 0.6f;
                                }
                                if (flag)
                                {
                                    linkedInTieNote.TemporaryInfo.PlayingDurationTimeMs = (int) ((float) (((linkedInTieNote.Duration + parentNotePack.Duration) * num) * 2f)).Clamp(((float) 240f), ((float) 999999f));
                                }
                            }
                            break;
                        }
                    }
                }
            }

            public bool IsFlipVerticalStemVoted
            {
                get
                {
                    bool? nullable = this._isFlipVerticalStemVoted;
                    return (nullable.HasValue ? nullable.GetValueOrDefault() : this.ParentNotePack.IsFlipVerticalStem);
                }
                set
                {
                    this._isFlipVerticalStemVoted = new bool?(value);
                }
            }

            public McNotePack LinkedInTieNote { get; set; }

            public McNotePack LinkedOutTieNote { get; set; }

            public McMeasure ParentMeasure =>
                this.ParentNotePack?.ParentMeasure;

            public McNotePack ParentNotePack { get; private set; }

            public int PlayingDurationTimeMs
            {
                get => 
                    this._playingDurationTimeMs;
                set
                {
                    this._playingDurationTimeMs = value;
                }
            }

            public float RelativeHighestPitchY { get; set; }

            public float RelativeLowestPitchY { get; set; }

            public float RelativeX
            {
                get => 
                    this._relativeX;
                set
                {
                    this._relativeX = value;
                }
            }

            public float RelativeXSmooth
            {
                get => 
                    (this.SmoothEffectEnabled ? this._relativeXSmooth : this._relativeX);
                set
                {
                    this._relativeXSmooth = value;
                }
            }

            public bool SmoothEffectEnabled =>
                (((this.ParentNotePack.Canvas.IsEditMode && MusicCanvasControl.EnableStemAnimation) && (this.ParentMeasure != null)) && this.ParentMeasure.IsHovering);

            public int StampIndex
            {
                get => 
                    this._stampIndex;
                set
                {
                    this._stampIndex = value;
                }
            }

            public float StemEndY
            {
                get => 
                    this._stemEndY;
                set
                {
                    this._stemEndY = value;
                }
            }

            public float StemEndYSmooth
            {
                get => 
                    (this.SmoothEffectEnabled ? this._stemEndYSmooth : this._stemEndY);
                set
                {
                    this._stemEndYSmooth = value;
                }
            }

            public float StemEndYSmoothAccSpeed { get; set; }

            public float StemStartY
            {
                get => 
                    this._stemStartY;
                set
                {
                    this._stemStartY = value;
                }
            }

            public float StemStartYSmooth
            {
                get => 
                    (this.SmoothEffectEnabled ? this._stemStartYSmooth : this._stemStartY);
                set
                {
                    this._stemStartYSmooth = value;
                }
            }

            public float TieMarkerRelativeY
            {
                get => 
                    this._tieMarkerRelativeY;
                set
                {
                    this._tieMarkerRelativeY = value;
                }
            }

            public float TieMarkerRelativeYSmooth
            {
                get => 
                    (this.SmoothEffectEnabled ? this._tieMarkerRelativeYSmooth : this._tieMarkerRelativeY);
                set
                {
                    this._tieMarkerRelativeYSmooth = value;
                }
            }

            public McNotePack[] TripletNotePacks
            {
                get => 
                    this._tripletNotePacks;
                set
                {
                    this._tripletNotePacks = value;
                }
            }
        }

        public enum TieTypes
        {
            None,
            Start,
            End,
            Both
        }
    }
}

