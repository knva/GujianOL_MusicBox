namespace GujianOL_MusicBox
{
    using SharpDX;
    using SharpDX.Direct2D1;
    using SharpDX.DirectWrite;
    using SharpDX.DXGI;
    using SharpDX.Mathematics.Interop;
    using SharpDX.WIC;
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Drawing.Drawing2D;
    using System.Drawing.Imaging;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    public static class MusicObjectUtility
    {
        public static void DrawBezierLine(this RenderTarget renderTarget, RawVector2 pStart, RawVector2 pEnd, float heightDiff, float mpRatio, Color color)
        {
            renderTarget.DrawBezierLine(pStart, pEnd, heightDiff, mpRatio, color, 1f, null, false);
        }

        public static void DrawBezierLine(this RenderTarget renderTarget, PointF[] points, Color color, float strokeWidth, StrokeStyle strokeStyle, bool disposeStrokeStyle = true)
        {
            if (points.Length >= 2)
            {
                PointF[] pathPoints;
                int num;
                using (GraphicsPath path = new GraphicsPath())
                {
                    List<PointF> list = new List<PointF> {
                        points.First<PointF>()
                    };
                    for (num = 1; num < points.Length; num++)
                    {
                        PointF tf;
                        PointF tf2;
                        GetBezierCtrlPoint(points, num - 1, 0.15f, 0.15f, out tf, out tf2);
                        list.Add(tf);
                        list.Add(tf2);
                        list.Add(points[num]);
                    }
                    path.AddBeziers(list.ToArray());
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
                        for (num = 1; num < pathPoints.Length; num++)
                        {
                            sink.AddLine(new RawVector2(pathPoints[num].X, pathPoints[num].Y));
                        }
                        sink.EndFigure(FigureEnd.Open);
                        sink.Close();
                        sink.Dispose();
                    }
                    SolidColorBrush brush = new SolidColorBrush(renderTarget, color.ToRawColor4(1f));
                    renderTarget.DrawGeometry(geometry, brush, strokeWidth, strokeStyle);
                    brush.Dispose();
                    geometry.Dispose();
                }
                if (disposeStrokeStyle && (strokeStyle != null))
                {
                    strokeStyle.Dispose();
                }
            }
        }

        public static void DrawBezierLine(this RenderTarget renderTarget, RawVector2 pStart, RawVector2 pM1, RawVector2 pM2, RawVector2 pEnd, Color color, float strokeWidth, StrokeStyle strokeStyle, bool disposeStrokeStyle = true)
        {
            PointF[] pathPoints;
            using (GraphicsPath path = new GraphicsPath())
            {
                path.AddBezier(pStart.X, pStart.Y, pM1.X, pM1.Y, pM2.X, pM2.Y, pEnd.X, pEnd.Y);
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
                    sink.EndFigure(FigureEnd.Open);
                    sink.Close();
                    sink.Dispose();
                }
                SolidColorBrush brush = new SolidColorBrush(renderTarget, color.ToRawColor4(1f));
                renderTarget.DrawGeometry(geometry, brush, strokeWidth, strokeStyle);
                brush.Dispose();
                geometry.Dispose();
            }
            if (disposeStrokeStyle && (strokeStyle != null))
            {
                strokeStyle.Dispose();
            }
        }

        public static void DrawBezierLine(this RenderTarget renderTarget, RawVector2 pStart, RawVector2 pEnd, float heightDiff, float mpRatio, Color color, float strokeWidth, StrokeStyle strokeStyle, bool disposeStrokeStyle = true)
        {
            mpRatio = mpRatio.Clamp(0f, 1f);
            float num = pEnd.X - pStart.X;
            float num2 = pEnd.Y - pStart.Y;
            RawVector2 vector = new RawVector2(pStart.X + (num * mpRatio), (pStart.Y + (num2 * mpRatio)) + heightDiff);
            RawVector2 vector2 = new RawVector2(pStart.X + (num * (1f - mpRatio)), (pStart.Y + (num2 * (1f - mpRatio))) + heightDiff);
            renderTarget.DrawBezierLine(pStart, vector, vector2, pEnd, color, strokeWidth, strokeStyle, disposeStrokeStyle);
        }

        public static void DrawBitmap(this RenderTarget renderTarget, SharpDX.Direct2D1.Bitmap bitmap, RawVector2 pos)
        {
            renderTarget.DrawBitmap(bitmap, pos, null, 1f, SharpDX.Direct2D1.BitmapInterpolationMode.NearestNeighbor);
        }

        public static void DrawBitmap(this RenderTarget renderTarget, SharpDX.Direct2D1.Bitmap bitmap, RawVector2 pos, float opacity)
        {
            renderTarget.DrawBitmap(bitmap, pos, null, opacity, SharpDX.Direct2D1.BitmapInterpolationMode.NearestNeighbor);
        }

        public static void DrawBitmap(this RenderTarget renderTarget, SharpDX.WIC.Bitmap bitmap, RawVector2 pos, float opacity = 1f)
        {
            renderTarget.DrawBitmap(SharpDX.Direct2D1.Bitmap.FromWicBitmap(renderTarget, bitmap), pos, null, opacity, SharpDX.Direct2D1.BitmapInterpolationMode.NearestNeighbor);
        }

        public static void DrawBitmap(this RenderTarget renderTarget, SharpDX.Direct2D1.Bitmap bitmap, RawVector2 pos, Size2F? size, float opacity = 1f, SharpDX.Direct2D1.BitmapInterpolationMode interpolationMode = 0)
        {
            if (!size.HasValue)
            {
                size = new Size2F?(bitmap.Size);
            }
            renderTarget.DrawBitmap(bitmap, new RawRectangleF(pos.X, pos.Y, pos.X + size.Value.Width, pos.Y + size.Value.Height), opacity, interpolationMode);
        }

        public static void DrawCircle(this RenderTarget renderTarget, RawVector2 pos, float radius, Color color)
        {
            SolidColorBrush brush = new SolidColorBrush(renderTarget, color.ToRawColor4(1f));
            Ellipse ellipse = new Ellipse(pos, radius, radius);
            renderTarget.DrawEllipse(ellipse, brush);
            brush.Dispose();
        }

        public static void DrawEllipse(this RenderTarget renderTarget, Ellipse ellipse, Color color, float strokeWidth)
        {
            SolidColorBrush brush = new SolidColorBrush(renderTarget, color.ToRawColor4(1f));
            renderTarget.DrawEllipse(ellipse, brush, strokeWidth);
            brush.Dispose();
        }

        public static void DrawLine(this RenderTarget renderTarget, RawVector2 p1, RawVector2 p2, Color color)
        {
            SolidColorBrush brush = new SolidColorBrush(renderTarget, color.ToRawColor4(1f));
            renderTarget.DrawLine(p1, p2, brush);
            brush.Dispose();
        }

        public static void DrawLine(this RenderTarget renderTarget, RawVector2 p1, RawVector2 p2, Color color, float strokeWidth)
        {
            SolidColorBrush brush = new SolidColorBrush(renderTarget, color.ToRawColor4(1f));
            renderTarget.DrawLine(p1, p2, brush, strokeWidth);
            brush.Dispose();
        }

        public static void DrawLine(this RenderTarget renderTarget, RawVector2 p1, RawVector2 p2, Color color, float strokeWidth, StrokeStyle strokeStyle, bool disposeStrokeStyle = true)
        {
            SolidColorBrush brush = new SolidColorBrush(renderTarget, color.ToRawColor4(1f));
            renderTarget.DrawLine(p1, p2, brush, strokeWidth, strokeStyle);
            brush.Dispose();
            if (disposeStrokeStyle && (strokeStyle != null))
            {
                strokeStyle.Dispose();
            }
        }

        public static void DrawRoundedRectangle(this RenderTarget renderTarget, RawRectangleF rectF, float radius, Color color)
        {
            SolidColorBrush brush = new SolidColorBrush(renderTarget, color.ToRawColor4(1f));
            RoundedRectangle roundedRect = new RoundedRectangle {
                Rect = rectF,
                RadiusX = radius,
                RadiusY = radius
            };
            renderTarget.DrawRoundedRectangle(roundedRect, brush);
            brush.Dispose();
        }

        public static void DrawRoundedRectangle(this RenderTarget renderTarget, RawRectangleF rectF, float radius, Color color, float strokeWidth)
        {
            SolidColorBrush brush = new SolidColorBrush(renderTarget, color.ToRawColor4(1f));
            RoundedRectangle roundedRect = new RoundedRectangle {
                Rect = rectF,
                RadiusX = radius,
                RadiusY = radius
            };
            renderTarget.DrawRoundedRectangle(roundedRect, brush, strokeWidth);
            brush.Dispose();
        }

        public static void DrawRoundedRectangle(this RenderTarget renderTarget, RawRectangleF rectF, float radius, Color color, float strokeWidth, StrokeStyle strokeStyle, bool disposeStrokeStyle = true)
        {
            SolidColorBrush brush = new SolidColorBrush(renderTarget, color.ToRawColor4(1f));
            RoundedRectangle roundedRect = new RoundedRectangle {
                Rect = rectF,
                RadiusX = radius,
                RadiusY = radius
            };
            renderTarget.DrawRoundedRectangle(roundedRect, brush, strokeWidth, strokeStyle);
            brush.Dispose();
            if (disposeStrokeStyle && (strokeStyle != null))
            {
                strokeStyle.Dispose();
            }
        }

        public static void DrawSector(this RenderTarget renderTarget, RawVector2 pos, float radius, Color color, float angleStart, float angleEnd, float strokeWidth = 1f, bool arc = false)
        {
            if (Math.Abs((float) (angleStart - angleEnd)) >= 0.0001)
            {
                PointF[] pathPoints;
                using (GraphicsPath path = new GraphicsPath())
                {
                    radius *= 2f;
                    if (arc)
                    {
                        path.AddArc(pos.X - (radius / 2f), pos.Y - (radius / 2f), radius, radius, angleStart, angleEnd);
                    }
                    else
                    {
                        path.AddPie(pos.X - (radius / 2f), pos.Y - (radius / 2f), radius, radius, angleStart, angleEnd);
                    }
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
                        sink.EndFigure(arc ? FigureEnd.Open : FigureEnd.Closed);
                        sink.Close();
                        sink.Dispose();
                    }
                    SolidColorBrush brush = new SolidColorBrush(renderTarget, color.ToRawColor4(1f));
                    renderTarget.DrawGeometry(geometry, brush, strokeWidth);
                    brush.Dispose();
                    geometry.Dispose();
                }
            }
        }

        public static void DrawTextLayout(this RenderTarget renderTarget, SharpDX.DirectWrite.Factory writeFactory, string text, RawVector2 pos, string fontFamilyName, float fontSize, Color color, SharpDX.DirectWrite.FontStyle fontStyle = 0)
        {
            renderTarget.DrawTextLayout(writeFactory, text, pos, fontFamilyName, fontSize, color, TextAlignment.Leading, ParagraphAlignment.Near, fontStyle, FontWeight.Bold);
        }

        public static void DrawTextLayout(this RenderTarget renderTarget, SharpDX.DirectWrite.Factory writeFactory, string text, RawVector2 pos, string fontFamilyName, float fontSize, Color color, TextAlignment textAlignment, SharpDX.DirectWrite.FontStyle fontStyle = 0)
        {
            renderTarget.DrawTextLayout(writeFactory, text, pos, fontFamilyName, fontSize, color, textAlignment, ParagraphAlignment.Near, fontStyle, FontWeight.Bold);
        }

        public static void DrawTextLayout(this RenderTarget renderTarget, SharpDX.DirectWrite.Factory writeFactory, string text, RawVector2 pos, string fontFamilyName, float fontSize, Color color, TextAlignment textAlignment, ParagraphAlignment paragraphAlignment, SharpDX.DirectWrite.FontStyle fontStyle = 0, FontWeight fontWeight = 700)
        {
            Trimming trimming;
            InlineObject obj2;
            TextFormat textFormat = new TextFormat(writeFactory, fontFamilyName, fontWeight, fontStyle, FontStretch.Normal, fontSize);
            textFormat.GetTrimming(out trimming, out obj2);
            trimming.Granularity = TrimmingGranularity.Word;
            textFormat.SetTrimming(trimming, obj2);
            textFormat.TextAlignment = TextAlignment.Center;
            textFormat.ParagraphAlignment = ParagraphAlignment.Center;
            TextLayout layout = new TextLayout(writeFactory, text, textFormat, 1920f, 1080f);
            textFormat.TextAlignment = textAlignment;
            textFormat.ParagraphAlignment = paragraphAlignment;
            TextLayout textLayout = new TextLayout(writeFactory, text, textFormat, layout.Metrics.Width + 2f, layout.Metrics.Height);
            layout.Dispose();
            textFormat.Dispose();
            switch (textAlignment)
            {
                case TextAlignment.Trailing:
                    pos.X -= textLayout.Metrics.Width;
                    break;

                case TextAlignment.Center:
                case TextAlignment.Justified:
                    pos.X -= textLayout.Metrics.Width / 2f;
                    break;
            }
            switch (paragraphAlignment)
            {
                case ParagraphAlignment.Far:
                    pos.Y -= textLayout.Metrics.Height;
                    break;

                case ParagraphAlignment.Center:
                    pos.Y -= textLayout.Metrics.Height / 2f;
                    break;
            }
            SolidColorBrush defaultForegroundBrush = new SolidColorBrush(renderTarget, color.ToRawColor4(1f));
            renderTarget.DrawTextLayout(pos, textLayout, defaultForegroundBrush, DrawTextOptions.Clip);
            defaultForegroundBrush.Dispose();
            textLayout.Dispose();
        }

        public static void DrawTie(this RenderTarget renderTarget, SharpDX.DirectWrite.Factory writeFactory, McNotePack startNotePack, McNotePack endNotePack, Color color, bool isTriplet = false)
        {
            float scrollXSmooth;
            RawVector2 startPos = new RawVector2();
            RawVector2 endPos = new RawVector2();
            bool flipVertical = false;
            if (startNotePack != null)
            {
                McNotePack.NpTemporaryInfo temporaryInfo = startNotePack.TemporaryInfo;
                temporaryInfo.TieMarkerRelativeYSmooth = temporaryInfo.TieMarkerRelativeYSmooth.Lerp(temporaryInfo.TieMarkerRelativeY, 0.2f);
                McMeasure parentMeasure = startNotePack.ParentMeasure;
                scrollXSmooth = parentMeasure.ParentRegularTrack.ParentNotation.Canvas.ScrollXSmooth;
                RawVector2 vector3 = new RawVector2(((parentMeasure.TemporaryInfo.RelativeX + McNotation.Margin.Left) + McRegularTrack.Margin.Left) - scrollXSmooth, (float) parentMeasure.Top);
                startPos = new RawVector2((vector3.X + temporaryInfo.RelativeXSmooth) + 8f, vector3.Y + temporaryInfo.TieMarkerRelativeYSmooth);
                flipVertical = temporaryInfo.IsFlipVerticalStemVoted;
            }
            if (endNotePack != null)
            {
                McNotePack.NpTemporaryInfo info2 = endNotePack.TemporaryInfo;
                info2.TieMarkerRelativeYSmooth = info2.TieMarkerRelativeYSmooth.Lerp(info2.TieMarkerRelativeY, 0.2f);
                McMeasure measure2 = endNotePack.ParentMeasure;
                scrollXSmooth = measure2.ParentRegularTrack.ParentNotation.Canvas.ScrollXSmooth;
                RawVector2 vector4 = new RawVector2(((measure2.TemporaryInfo.RelativeX + McNotation.Margin.Left) + McRegularTrack.Margin.Left) - scrollXSmooth, (float) measure2.Top);
                endPos = new RawVector2((vector4.X + info2.RelativeXSmooth) + 8f, vector4.Y + info2.TieMarkerRelativeYSmooth);
            }
            if (isTriplet)
            {
                if (endNotePack == null)
                {
                    renderTarget.DrawTextLayout(writeFactory, "Tri", new RawVector2(startPos.X - 6f, startPos.Y - 5f), "Consolas", 10f, Color.FromArgb(0x80, Color.Aqua), SharpDX.DirectWrite.FontStyle.Italic);
                }
            }
            else if ((startNotePack != null) && (endNotePack == null))
            {
                renderTarget.DrawTextLayout(writeFactory, startNotePack.TieType.ToString().Substring(0, 1), new RawVector2(startPos.X - 12f, startPos.Y - 4f), "Consolas", 10f, Color.FromArgb(0x40, Color.FloralWhite), SharpDX.DirectWrite.FontStyle.Italic);
            }
            else if ((startNotePack == null) && (endNotePack != null))
            {
                renderTarget.DrawTextLayout(writeFactory, endNotePack.TieType.ToString().Substring(0, 1), new RawVector2(endPos.X - 12f, endPos.Y - 4f), "Consolas", 10f, Color.FromArgb(0x40, Color.FloralWhite), SharpDX.DirectWrite.FontStyle.Italic);
            }
            if ((startNotePack != null) && (endNotePack != null))
            {
                renderTarget.DrawTie(writeFactory, startPos, endPos, color, flipVertical, isTriplet);
            }
        }

        private static void DrawTie(this RenderTarget renderTarget, SharpDX.DirectWrite.Factory writeFactory, RawVector2 startPos, RawVector2 endPos, Color color, bool flipVertical = false, bool isTriplet = false)
        {
            PointF[] pathPoints;
            RawVector2 vector;
            RawVector2 vector2;
            using (GraphicsPath path = new GraphicsPath())
            {
                float num = 45f;
                float num2 = 0.3f;
                float num3 = startPos.GetDistanceByRawVector2(endPos).Clamp(64f, 256f);
                float num4 = startPos.GetAngleByRawVector2(endPos);
                float num5 = endPos.GetAngleByRawVector2(startPos);
                if (flipVertical)
                {
                    vector = startPos.GetRawVector2ByPolarCoordinates(num4 + num, num3 * num2);
                    vector2 = endPos.GetRawVector2ByPolarCoordinates(num5 - num, num3 * num2);
                    path.AddBezier(startPos.X, startPos.Y, vector.X, vector.Y, vector2.X, vector2.Y, endPos.X, endPos.Y);
                    path.AddBezier(endPos.X, endPos.Y + 1f, vector2.X, vector2.Y + 2f, vector.X, vector.Y + 2f, startPos.X, startPos.Y + 1f);
                }
                else
                {
                    vector = startPos.GetRawVector2ByPolarCoordinates(num4 - num, num3 * num2);
                    vector2 = endPos.GetRawVector2ByPolarCoordinates(num5 + num, num3 * num2);
                    path.AddBezier(startPos.X, startPos.Y, vector.X, vector.Y, vector2.X, vector2.Y, endPos.X, endPos.Y);
                    path.AddBezier(endPos.X, endPos.Y - 1f, vector2.X, vector2.Y - 2f, vector.X, vector.Y - 2f, startPos.X, startPos.Y - 1f);
                }
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
                if (isTriplet)
                {
                    RawVector2 pos = new RawVector2(vector.X + ((vector2.X - vector.X) / 2f), vector.Y + ((vector2.Y - vector.Y) / 2f));
                    renderTarget.DrawTextLayout(writeFactory, "3", pos, "Consolas", 16f, Color.FromArgb((int) (color.A * 0.75f), color), TextAlignment.Center, ParagraphAlignment.Center, SharpDX.DirectWrite.FontStyle.Normal, FontWeight.ExtraBlack);
                }
            }
        }

        public static void FillCircle(this RenderTarget renderTarget, RawVector2 pos, float radius, Color color)
        {
            SolidColorBrush brush = new SolidColorBrush(renderTarget, color.ToRawColor4(1f));
            Ellipse ellipse = new Ellipse(pos, radius, radius);
            renderTarget.FillEllipse(ellipse, brush);
            brush.Dispose();
        }

        public static void FillClassicMusicalVerticalParallelRect(this RenderTarget renderTarget, RawVector2 startPos, RawVector2 endPos, Color color, float strokeWidth, float scale = 1f, bool shadow = false)
        {
            PointF[] pathPoints;
            if (startPos.X > endPos.X)
            {
                RawVector2 vector = new RawVector2 {
                    X = startPos.X,
                    Y = startPos.Y
                };
                startPos.X = endPos.X;
                startPos.Y = endPos.Y;
                endPos.X = vector.X;
                endPos.Y = vector.Y;
            }
            if (scale > 0f)
            {
                endPos.X = startPos.X + ((endPos.X - startPos.X) * scale);
                endPos.Y = startPos.Y + ((endPos.Y - startPos.Y) * scale);
            }
            else
            {
                startPos.X += (endPos.X - startPos.X) * (1f + scale);
                startPos.Y += (endPos.Y - startPos.Y) * (1f + scale);
            }
            startPos.X--;
            using (GraphicsPath path = new GraphicsPath())
            {
                float num = strokeWidth / 2f;
                path.AddLine(startPos.X, startPos.Y, startPos.X, startPos.Y - num);
                path.AddLine(startPos.X, startPos.Y - num, endPos.X, endPos.Y - num);
                path.AddLine(endPos.X, endPos.Y - num, endPos.X, endPos.Y + num);
                path.AddLine(endPos.X, endPos.Y + num, startPos.X, startPos.Y + num);
                path.AddLine(startPos.X, startPos.Y + num, startPos.X, startPos.Y);
                path.Flatten();
                pathPoints = path.PathPoints;
                path.Dispose();
            }
            using (SharpDX.Direct2D1.Factory factory = renderTarget.Factory)
            {
                GeometrySink sink;
                int num2;
                PathGeometry geometry = new PathGeometry(factory);
                if (pathPoints.Length > 1)
                {
                    sink = geometry.Open();
                    sink.SetSegmentFlags(PathSegment.ForceRoundLineJoin);
                    sink.BeginFigure(new RawVector2(pathPoints[0].X, pathPoints[0].Y), FigureBegin.Filled);
                    for (num2 = 1; num2 < pathPoints.Length; num2++)
                    {
                        sink.AddLine(new RawVector2(pathPoints[num2].X, pathPoints[num2].Y));
                    }
                    sink.EndFigure(FigureEnd.Closed);
                    sink.Close();
                    sink.Dispose();
                }
                if (shadow)
                {
                    PathGeometry geometry2 = new PathGeometry(factory);
                    if (shadow && (pathPoints.Length > 1))
                    {
                        sink = geometry2.Open();
                        sink.SetSegmentFlags(PathSegment.ForceRoundLineJoin);
                        sink.BeginFigure(new RawVector2(pathPoints[0].X + 2f, pathPoints[0].Y + 2f), FigureBegin.Filled);
                        for (num2 = 1; num2 < pathPoints.Length; num2++)
                        {
                            sink.AddLine(new RawVector2(pathPoints[num2].X + 2f, pathPoints[num2].Y + 2f));
                        }
                        sink.EndFigure(FigureEnd.Closed);
                        sink.Close();
                        sink.Dispose();
                    }
                    SolidColorBrush brush = new SolidColorBrush(renderTarget, Color.FromArgb((int) (color.A * 0.2f), color).ToRawColor4(1f));
                    renderTarget.FillGeometry(geometry2, brush);
                    brush.Dispose();
                    geometry2.Dispose();
                }
                SolidColorBrush brush2 = new SolidColorBrush(renderTarget, color.ToRawColor4(1f));
                renderTarget.FillGeometry(geometry, brush2);
                brush2.Dispose();
                geometry.Dispose();
            }
        }

        public static void FillEllipse(this RenderTarget renderTarget, Ellipse ellipse, Color color)
        {
            SolidColorBrush brush = new SolidColorBrush(renderTarget, color.ToRawColor4(1f));
            renderTarget.FillEllipse(ellipse, brush);
            brush.Dispose();
        }

        public static void FillRectangle(this RenderTarget renderTarget, RawRectangleF rectF, Color color)
        {
            SolidColorBrush brush = new SolidColorBrush(renderTarget, color.ToRawColor4(1f));
            renderTarget.FillRectangle(rectF, brush);
            brush.Dispose();
        }

        public static void FillRoundedRectangle(this RenderTarget renderTarget, RawRectangleF rectF, float radius, Color color)
        {
            SolidColorBrush brush = new SolidColorBrush(renderTarget, color.ToRawColor4(1f));
            RoundedRectangle roundedRect = new RoundedRectangle {
                Rect = rectF,
                RadiusX = radius,
                RadiusY = radius
            };
            renderTarget.FillRoundedRectangle(roundedRect, brush);
            brush.Dispose();
        }

        public static void FillSector(this RenderTarget renderTarget, RawVector2 pos, float radius, Color color, float angleStart, float angleEnd)
        {
            PointF[] pathPoints;
            using (GraphicsPath path = new GraphicsPath())
            {
                radius *= 2f;
                path.AddPie(pos.X - (radius / 2f), pos.Y - (radius / 2f), radius, radius, angleStart, angleEnd);
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
        }

        private static void GetBezierCtrlPoint(PointF[] points, int i, float p1, float p2, out PointF ctrlA, out PointF ctrlB)
        {
            p1 = p1.Clamp(0f, 1f);
            p2 = p2.Clamp(0f, 1f);
            float x = 0f;
            float y = 0f;
            float num3 = 0f;
            float num4 = 0f;
            if (i < 1)
            {
                x = points[0].X + ((points[1].X - points[0].X) * p1);
                y = points[0].Y + ((points[1].Y - points[0].Y) * p1);
            }
            else
            {
                x = points[i].X + ((points[i + 1].X - points[i - 1].X) * p1);
                y = points[i].Y + ((points[i + 1].Y - points[i - 1].Y) * p1);
            }
            if (i > (points.Length - 3))
            {
                int index = points.Length - 1;
                num3 = points[index].X - ((points[index].X - points[index - 1].X) * p2);
                num4 = points[index].Y - ((points[index].Y - points[index - 1].Y) * p2);
            }
            else
            {
                num3 = points[i + 1].X - ((points[i + 2].X - points[i].X) * p2);
                num4 = points[i + 1].Y - ((points[i + 2].Y - points[i].Y) * p2);
            }
            ctrlA = new PointF(x, y);
            ctrlB = new PointF(num3, num4);
        }

        public static SharpDX.Direct2D1.Bitmap LoadBitmapFromResourceName(this RenderTarget renderTarget, string resourceName)
        {
            System.Drawing.Bitmap bitmap2;
            System.Drawing.Bitmap image = new System.Drawing.Bitmap(typeof(MusicCanvasControl), resourceName);
            if (image.PixelFormat != System.Drawing.Imaging.PixelFormat.Format32bppPArgb)
            {
                bitmap2 = new System.Drawing.Bitmap(image.Width, image.Height, System.Drawing.Imaging.PixelFormat.Format32bppPArgb);
                using (Graphics graphics = Graphics.FromImage(bitmap2))
                {
                    graphics.DrawImage(image, 0, 0, image.Width, image.Height);
                }
            }
            else
            {
                bitmap2 = image;
            }
            BitmapData bitmapdata = bitmap2.LockBits(new Rectangle(0, 0, bitmap2.Width, bitmap2.Height), ImageLockMode.ReadOnly, bitmap2.PixelFormat);
            int length = bitmapdata.Stride * bitmap2.Height;
            byte[] destination = new byte[length];
            Marshal.Copy(bitmapdata.Scan0, destination, 0, length);
            bitmap2.UnlockBits(bitmapdata);
            SharpDX.Direct2D1.PixelFormat pixelFormat = new SharpDX.Direct2D1.PixelFormat(Format.B8G8R8A8_UNorm, SharpDX.Direct2D1.AlphaMode.Premultiplied);
            BitmapProperties bitmapProperties = new BitmapProperties(pixelFormat, bitmap2.HorizontalResolution, bitmap2.VerticalResolution);
            SharpDX.Direct2D1.Bitmap bitmap3 = new SharpDX.Direct2D1.Bitmap(renderTarget, new Size2(bitmap2.Width, bitmap2.Height), bitmapProperties);
            bitmap3.CopyFromMemory(destination, bitmapdata.Stride);
            bitmap2.Dispose();
            image.Dispose();
            return bitmap3;
        }
    }
}

