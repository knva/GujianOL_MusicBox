namespace GujianOL_MusicBox
{
    using SharpDX;
    using SharpDX.Direct2D1;
    using SharpDX.DirectWrite;
    using SharpDX.DXGI;
    using SharpDX.Mathematics.Interop;
    using System;
    using System.Drawing;
    using System.Drawing.Drawing2D;
    using System.Drawing.Imaging;
    using System.IO;
    using System.IO.Compression;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Security.Cryptography;
    using System.Text;
    using System.Windows.Forms;

    public static class MusicCanvasControlUtility
    {
        private static string encryptIv = "SzAa";
        private static string encryptKey = "hKwY";

        public static int Clamp(this int value, int min, int max) => 
            Math.Min(max, Math.Max(min, value));

        public static float Clamp(this float value, float min, float max) => 
            Math.Min(max, Math.Max(min, value));

        private static byte[] Compress(this byte[] data)
        {
            byte[] buffer2;
            try
            {
                MemoryStream stream = new MemoryStream();
                GZipStream stream2 = new GZipStream(stream, CompressionMode.Compress, true);
                stream2.Write(data, 0, data.Length);
                stream2.Close();
                byte[] buffer = new byte[stream.Length];
                stream.Position = 0L;
                stream.Read(buffer, 0, buffer.Length);
                stream.Close();
                buffer2 = buffer;
            }
            catch (Exception exception)
            {
                throw new Exception(exception.Message);
            }
            return buffer2;
        }

        public static string CompressString(this string str, Encoding encoding = null)
        {
            string s = "";
            encoding = encoding ?? new UTF8Encoding(false);
            s = Convert.ToBase64String(encoding.GetBytes(str).Compress());
            try
            {
                DESCryptoServiceProvider provider = new DESCryptoServiceProvider();
                byte[] bytes = Encoding.Unicode.GetBytes(encryptKey);
                byte[] rgbIV = Encoding.Unicode.GetBytes(encryptIv);
                byte[] buffer = Encoding.Unicode.GetBytes(s);
                MemoryStream stream = new MemoryStream();
                CryptoStream stream2 = new CryptoStream(stream, provider.CreateEncryptor(bytes, rgbIV), CryptoStreamMode.Write);
                stream2.Write(buffer, 0, buffer.Length);
                stream2.FlushFinalBlock();
                s = Convert.ToBase64String(stream.ToArray());
                stream.Close();
                stream.Dispose();
                stream2.Close();
                stream2.Dispose();
                provider.Dispose();
            }
            catch (Exception)
            {
                s = "";
            }
            return s;
        }

        private static byte[] Decompress(this byte[] data)
        {
            byte[] buffer2;
            try
            {
                int num;
                bool flag;
                MemoryStream stream = new MemoryStream(data);
                GZipStream stream2 = new GZipStream(stream, CompressionMode.Decompress, true);
                MemoryStream stream3 = new MemoryStream();
                byte[] buffer = new byte[0x1000];
                goto Label_004D;
            Label_0025:
                num = stream2.Read(buffer, 0, buffer.Length);
                if (num <= 0)
                {
                    goto Label_0052;
                }
                stream3.Write(buffer, 0, num);
            Label_004D:
                flag = true;
                goto Label_0025;
            Label_0052:
                stream2.Close();
                stream.Close();
                stream3.Position = 0L;
                buffer = stream3.ToArray();
                stream3.Close();
                buffer2 = buffer;
            }
            catch (Exception exception)
            {
                throw new Exception(exception.Message);
            }
            return buffer2;
        }

        public static string DecompressString(this string str, Encoding encoding = null)
        {
            try
            {
                DESCryptoServiceProvider provider = new DESCryptoServiceProvider();
                byte[] rgbKey = Encoding.Unicode.GetBytes(encryptKey);
                byte[] rgbIV = Encoding.Unicode.GetBytes(encryptIv);
                byte[] buffer = Convert.FromBase64String(str);
                MemoryStream stream = new MemoryStream();
                CryptoStream stream2 = new CryptoStream(stream, provider.CreateDecryptor(rgbKey, rgbIV), CryptoStreamMode.Write);
                stream2.Write(buffer, 0, buffer.Length);
                stream2.FlushFinalBlock();
                str = Encoding.Unicode.GetString(stream.ToArray());
                stream.Close();
                stream.Dispose();
                stream2.Close();
                stream2.Dispose();
                provider.Dispose();
            }
            catch (Exception)
            {
                return "";
            }
            encoding = encoding ?? new UTF8Encoding(false);
            byte[] bytes = Convert.FromBase64String(str).Decompress();
            return encoding.GetString(bytes);
        }

        public static void DrawBezierLine(this MusicCanvasControl canvas, RawVector2 pStart, RawVector2 pEnd, float heightDiff, float mpRatio, Color color)
        {
            canvas.DrawBezierLine(pStart, pEnd, heightDiff, mpRatio, color, 1f, null, false);
        }

        public static void DrawBezierLine(this MusicCanvasControl canvas, PointF[] points, Color color, float strokeWidth, StrokeStyle strokeStyle, bool disposeStrokeStyle = true)
        {
            PointF[] pathPoints;
            RenderTarget renderTarget = canvas.RenderTarget2D;
            using (GraphicsPath path = new GraphicsPath())
            {
                path.AddBeziers(points);
                path.Flatten();
                pathPoints = path.PathPoints;
                path.Dispose();
            }
            PathGeometry geometry = new PathGeometry(renderTarget.Factory);
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
            if (disposeStrokeStyle && (strokeStyle != null))
            {
                strokeStyle.Dispose();
            }
        }

        public static void DrawBezierLine(this MusicCanvasControl canvas, RawVector2 pStart, RawVector2 pM1, RawVector2 pM2, RawVector2 pEnd, Color color, float strokeWidth, StrokeStyle strokeStyle, bool disposeStrokeStyle = true)
        {
            PointF[] pathPoints;
            RenderTarget renderTarget = canvas.RenderTarget2D;
            using (GraphicsPath path = new GraphicsPath())
            {
                path.AddBezier(pStart.X, pStart.Y, pM1.X, pM1.Y, pM2.X, pM2.Y, pEnd.X, pEnd.Y);
                path.Flatten();
                pathPoints = path.PathPoints;
                path.Dispose();
            }
            PathGeometry geometry = new PathGeometry(renderTarget.Factory);
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
            if (disposeStrokeStyle && (strokeStyle != null))
            {
                strokeStyle.Dispose();
            }
        }

        public static void DrawBezierLine(this MusicCanvasControl canvas, RawVector2 pStart, RawVector2 pEnd, float heightDiff, float mpRatio, Color color, float strokeWidth, StrokeStyle strokeStyle, bool disposeStrokeStyle = true)
        {
            mpRatio = mpRatio.Clamp(0f, 1f);
            float num = pEnd.X - pStart.X;
            float num2 = pEnd.Y - pStart.Y;
            RawVector2 vector = new RawVector2(pStart.X + (num * mpRatio), (pStart.Y + (num2 * mpRatio)) + heightDiff);
            RawVector2 vector2 = new RawVector2(pStart.X + (num * (1f - mpRatio)), (pStart.Y + (num2 * (1f - mpRatio))) + heightDiff);
            canvas.DrawBezierLine(pStart, vector, vector2, pEnd, color, strokeWidth, strokeStyle, disposeStrokeStyle);
        }

        public static void DrawBitmap(this MusicCanvasControl canvas, SharpDX.Direct2D1.Bitmap bitmap, RawVector2 pos)
        {
            canvas.DrawBitmap(bitmap, pos, null, 1f, BitmapInterpolationMode.NearestNeighbor);
        }

        public static void DrawBitmap(this MusicCanvasControl canvas, SharpDX.Direct2D1.Bitmap bitmap, RawVector2 pos, float opacity)
        {
            canvas.DrawBitmap(bitmap, pos, null, opacity, BitmapInterpolationMode.NearestNeighbor);
        }

        public static void DrawBitmap(this MusicCanvasControl canvas, SharpDX.Direct2D1.Bitmap bitmap, RawVector2 pos, Size2F? size, float opacity = 1f, BitmapInterpolationMode interpolationMode = 0)
        {
            if (!size.HasValue)
            {
                size = new Size2F?(bitmap.Size);
            }
            canvas.RenderTarget2D.DrawBitmap(bitmap, new RawRectangleF(pos.X, pos.Y, pos.X + size.Value.Width, pos.Y + size.Value.Height), opacity, interpolationMode);
        }

        public static void DrawCircle(this MusicCanvasControl canvas, RawVector2 pos, float radius, Color color, float strokeWidth = 1f)
        {
            RenderTarget renderTarget = canvas.RenderTarget2D;
            SolidColorBrush brush = new SolidColorBrush(renderTarget, color.ToRawColor4(1f));
            Ellipse ellipse = new Ellipse(new RawVector2(pos.X, pos.Y), radius, radius);
            renderTarget.DrawEllipse(ellipse, brush, strokeWidth);
            brush.Dispose();
        }

        public static void DrawEllipse(this MusicCanvasControl canvas, Ellipse ellipse, Color color, float strokeWidth)
        {
            RenderTarget renderTarget = canvas.RenderTarget2D;
            SolidColorBrush brush = new SolidColorBrush(renderTarget, color.ToRawColor4(1f));
            renderTarget.DrawEllipse(ellipse, brush, strokeWidth);
            brush.Dispose();
        }

        public static void DrawLine(this MusicCanvasControl canvas, RawVector2 p1, RawVector2 p2, Color color)
        {
            RenderTarget renderTarget = canvas.RenderTarget2D;
            SolidColorBrush brush = new SolidColorBrush(renderTarget, color.ToRawColor4(1f));
            renderTarget.DrawLine(p1, p2, brush);
            brush.Dispose();
        }

        public static void DrawLine(this MusicCanvasControl canvas, RawVector2 p1, RawVector2 p2, Color color, float strokeWidth)
        {
            RenderTarget renderTarget = canvas.RenderTarget2D;
            SolidColorBrush brush = new SolidColorBrush(renderTarget, color.ToRawColor4(1f));
            renderTarget.DrawLine(p1, p2, brush, strokeWidth);
            brush.Dispose();
        }

        public static void DrawLine(this MusicCanvasControl canvas, RawVector2 p1, RawVector2 p2, Color color, float strokeWidth, StrokeStyle strokeStyle, bool disposeStrokeStyle = true)
        {
            RenderTarget renderTarget = canvas.RenderTarget2D;
            SolidColorBrush brush = new SolidColorBrush(renderTarget, color.ToRawColor4(1f));
            renderTarget.DrawLine(p1, p2, brush, strokeWidth, strokeStyle);
            brush.Dispose();
            if (disposeStrokeStyle && (strokeStyle != null))
            {
                strokeStyle.Dispose();
            }
        }

        public static void DrawRoundedRectangle(this MusicCanvasControl canvas, RawRectangleF rectF, float radius, Color color)
        {
            RenderTarget renderTarget = canvas.RenderTarget2D;
            SolidColorBrush brush = new SolidColorBrush(renderTarget, color.ToRawColor4(1f));
            RoundedRectangle roundedRect = new RoundedRectangle {
                Rect = rectF,
                RadiusX = radius,
                RadiusY = radius
            };
            renderTarget.DrawRoundedRectangle(roundedRect, brush);
            brush.Dispose();
        }

        public static void DrawRoundedRectangle(this MusicCanvasControl canvas, RawRectangleF rectF, float radius, Color color, float strokeWidth)
        {
            RenderTarget renderTarget = canvas.RenderTarget2D;
            SolidColorBrush brush = new SolidColorBrush(renderTarget, color.ToRawColor4(1f));
            RoundedRectangle roundedRect = new RoundedRectangle {
                Rect = rectF,
                RadiusX = radius,
                RadiusY = radius
            };
            renderTarget.DrawRoundedRectangle(roundedRect, brush, strokeWidth);
            brush.Dispose();
        }

        public static void DrawRoundedRectangle(this MusicCanvasControl canvas, RawRectangleF rectF, float radius, Color color, float strokeWidth, StrokeStyle strokeStyle, bool disposeStrokeStyle = true)
        {
            RenderTarget renderTarget = canvas.RenderTarget2D;
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

        public static void DrawSector(this MusicCanvasControl canvas, RawVector2 pos, float radius, Color color, float angleStart, float angleEnd, float strokeWidth = 1f, bool arc = false)
        {
            canvas.RenderTarget2D.DrawSector(pos, radius, color, angleStart, angleEnd, strokeWidth, arc);
        }

        public static void DrawSimpleTip(this MusicCanvasControl canvas, string text, RawVector2? pos = new RawVector2?())
        {
            if (MusicCanvasControl.EnableControlTip && canvas.IsAltKeyPressed())
            {
                Trimming trimming;
                InlineObject obj2;
                if (!pos.HasValue)
                {
                    Point point = canvas.PointToClient(Control.MousePosition);
                    pos = new RawVector2((float) (point.X + 0x18), (float) (point.Y - 0x18));
                }
                RenderTarget renderTarget = canvas.RenderTarget2D;
                SharpDX.DirectWrite.Factory factoryDWrite = canvas.FactoryDWrite;
                TextFormat textFormat = new TextFormat(factoryDWrite, "微软雅黑", FontWeight.Bold, SharpDX.DirectWrite.FontStyle.Normal, FontStretch.Normal, 12f);
                textFormat.GetTrimming(out trimming, out obj2);
                trimming.Granularity = TrimmingGranularity.Word;
                textFormat.SetTrimming(trimming, obj2);
                textFormat.TextAlignment = TextAlignment.Center;
                textFormat.ParagraphAlignment = ParagraphAlignment.Center;
                TextLayout layout = new TextLayout(factoryDWrite, text, textFormat, 1920f, 1080f);
                textFormat.TextAlignment = TextAlignment.Leading;
                textFormat.ParagraphAlignment = ParagraphAlignment.Far;
                TextLayout textLayout = new TextLayout(factoryDWrite, text, textFormat, layout.Metrics.Width, layout.Metrics.Height);
                layout.Dispose();
                RawVector2 origin = new RawVector2(pos.Value.X, pos.Value.Y);
                switch (textFormat.TextAlignment)
                {
                    case TextAlignment.Trailing:
                        origin.X = pos.Value.X - textLayout.Metrics.Width;
                        break;

                    case TextAlignment.Center:
                    case TextAlignment.Justified:
                        origin.X = pos.Value.X - (textLayout.Metrics.Width / 2f);
                        break;
                }
                switch (textFormat.ParagraphAlignment)
                {
                    case ParagraphAlignment.Far:
                        origin.Y = pos.Value.Y - textLayout.Metrics.Height;
                        break;

                    case ParagraphAlignment.Center:
                        origin.Y = pos.Value.Y - (textLayout.Metrics.Height / 2f);
                        break;
                }
                origin.X += 4f;
                origin.Y += 4f;
                textFormat.Dispose();
                RawRectangleF rectF = new RawRectangleF(pos.Value.X, pos.Value.Y - textLayout.Metrics.Height, (pos.Value.X + textLayout.Metrics.Width) + 8f, pos.Value.Y + 8f);
                canvas.FillRoundedRectangle(rectF, 4f, Color.FromArgb(150, 0x1f, 0x1f, 0x22));
                canvas.DrawRoundedRectangle(rectF, 4f, Color.FromArgb(150, 0xe9, 0xe9, 0xe9), 2f);
                SolidColorBrush defaultForegroundBrush = new SolidColorBrush(renderTarget, Color.FromArgb(250, Color.DarkGray).ToRawColor4(1f));
                renderTarget.DrawTextLayout(origin, textLayout, defaultForegroundBrush, DrawTextOptions.Clip);
                defaultForegroundBrush.Dispose();
                textLayout.Dispose();
            }
        }

        public static void DrawTextLayout(this MusicCanvasControl canvas, string text, RawVector2 pos, string fontFamilyName, float fontSize, Color color, SharpDX.DirectWrite.FontStyle fontStyle = 0)
        {
            canvas.DrawTextLayout(text, pos, fontFamilyName, fontSize, color, TextAlignment.Leading, ParagraphAlignment.Near, fontStyle, FontWeight.Bold);
        }

        public static void DrawTextLayout(this MusicCanvasControl canvas, string text, RawVector2 pos, string fontFamilyName, float fontSize, Color color, TextAlignment textAlignment, SharpDX.DirectWrite.FontStyle fontStyle = 0)
        {
            canvas.DrawTextLayout(text, pos, fontFamilyName, fontSize, color, textAlignment, ParagraphAlignment.Near, fontStyle, FontWeight.Bold);
        }

        public static void DrawTextLayout(this MusicCanvasControl canvas, string text, RawVector2 pos, string fontFamilyName, float fontSize, Color color, TextAlignment textAlignment, ParagraphAlignment paragraphAlignment, SharpDX.DirectWrite.FontStyle fontStyle = 0, FontWeight fontWeight = 700)
        {
            Trimming trimming;
            InlineObject obj2;
            RenderTarget renderTarget = canvas.RenderTarget2D;
            SharpDX.DirectWrite.Factory factoryDWrite = canvas.FactoryDWrite;
            TextFormat textFormat = new TextFormat(factoryDWrite, fontFamilyName, fontWeight, fontStyle, FontStretch.Normal, fontSize);
            textFormat.GetTrimming(out trimming, out obj2);
            trimming.Granularity = TrimmingGranularity.Word;
            textFormat.SetTrimming(trimming, obj2);
            textFormat.TextAlignment = TextAlignment.Center;
            textFormat.ParagraphAlignment = ParagraphAlignment.Center;
            TextLayout layout = new TextLayout(factoryDWrite, text, textFormat, 1920f, 1080f);
            textFormat.TextAlignment = textAlignment;
            textFormat.ParagraphAlignment = paragraphAlignment;
            TextLayout textLayout = new TextLayout(factoryDWrite, text, textFormat, layout.Metrics.Width, layout.Metrics.Height);
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

        public static void FillCircle(this MusicCanvasControl canvas, RawVector2 pos, float radius, Color color)
        {
            RenderTarget renderTarget = canvas.RenderTarget2D;
            SolidColorBrush brush = new SolidColorBrush(renderTarget, color.ToRawColor4(1f));
            Ellipse ellipse = new Ellipse(new RawVector2(pos.X, pos.Y), radius, radius);
            renderTarget.FillEllipse(ellipse, brush);
            brush.Dispose();
        }

        public static void FillEllipse(this MusicCanvasControl canvas, Ellipse ellipse, Color color)
        {
            RenderTarget renderTarget = canvas.RenderTarget2D;
            SolidColorBrush brush = new SolidColorBrush(renderTarget, color.ToRawColor4(1f));
            renderTarget.FillEllipse(ellipse, brush);
            brush.Dispose();
        }

        public static void FillRectangle(this MusicCanvasControl canvas, RawRectangleF rectF, Color color)
        {
            RenderTarget renderTarget = canvas.RenderTarget2D;
            SolidColorBrush brush = new SolidColorBrush(renderTarget, color.ToRawColor4(1f));
            renderTarget.FillRectangle(rectF, brush);
            brush.Dispose();
        }

        public static void FillRoundedRectangle(this MusicCanvasControl canvas, RawRectangleF rectF, float radius, Color color)
        {
            RenderTarget renderTarget = canvas.RenderTarget2D;
            SolidColorBrush brush = new SolidColorBrush(renderTarget, color.ToRawColor4(1f));
            RoundedRectangle roundedRect = new RoundedRectangle {
                Rect = rectF,
                RadiusX = radius,
                RadiusY = radius
            };
            renderTarget.FillRoundedRectangle(roundedRect, brush);
            brush.Dispose();
        }

        public static void FillSector(this MusicCanvasControl canvas, RawVector2 pos, float radius, Color color, float angleStart, float angleEnd)
        {
            canvas.RenderTarget2D.FillSector(pos, radius, color, angleStart, angleEnd);
        }

        public static float GetHeight(this RawRectangleF rect) => 
            (rect.Bottom - rect.Top);

        [DllImport("user32.dll")]
        private static extern int GetKeyState(int nVirtKey);
        public static float GetWidth(this RawRectangleF rect) => 
            (rect.Right - rect.Left);

        public static bool Intersects(this RawRectangleF rect, RawRectangleF rectTest)
        {
            RectangleF ef = new RectangleF(rect.Left, rect.Top, rect.GetWidth(), rect.GetHeight());
            RectangleF ef2 = new RectangleF(rectTest.Left, rectTest.Top, rectTest.GetWidth(), rectTest.GetHeight());
            return ef.IntersectsWith(ef2);
        }

        public static bool IsAltKeyPressed(this MusicCanvasControl canvas) => 
            ((Control.ModifierKeys & Keys.Alt) == Keys.Alt);

        public static bool IsCtrlKeyPressed(this MusicCanvasControl canvas) => 
            ((Control.ModifierKeys & Keys.Control) == Keys.Control);

        public static bool IsMouseLeftKeyPressed(this MusicCanvasControl canvas) => 
            ((Control.MouseButtons & MouseButtons.Left) == MouseButtons.Left);

        public static bool IsMouseMiddleKeyPressed(this MusicCanvasControl canvas) => 
            ((Control.MouseButtons & MouseButtons.Middle) == MouseButtons.Middle);

        public static bool IsMouseRightKeyPressed(this MusicCanvasControl canvas) => 
            ((Control.MouseButtons & MouseButtons.Right) == MouseButtons.Right);

        public static bool IsPressed(this Keys key) => 
            ((GetKeyState((int) key) & 0x8000) != 0);

        public static bool IsShiftKeyPressed(this MusicCanvasControl canvas) => 
            ((Control.ModifierKeys & Keys.Shift) == Keys.Shift);

        [DllImport("user32.dll")]
        private static extern void keybd_event(byte bVk, byte bScan, int dwFlags, int dwExtraInfo);
        public static float Lerp(this int valueA, float valueB, float percent)
        {
            if (Math.Abs((float) (valueA - valueB)) < 1E-06)
            {
                return (float) valueA;
            }
            percent = percent.Clamp(0f, 1f);
            return (valueA + ((valueB - valueA) * percent));
        }

        public static float Lerp(this float valueA, float valueB, float percent)
        {
            if (Math.Abs((float) (valueA - valueB)) < 1E-06)
            {
                return valueA;
            }
            percent = percent.Clamp(0f, 1f);
            return (valueA + ((valueB - valueA) * percent));
        }

        public static SharpDX.Direct2D1.Bitmap LoadBitmapFromFileName(this MusicCanvasControl canvas, string filename)
        {
            if (File.Exists(filename))
            {
                try
                {
                    System.Drawing.Bitmap bitmap2;
                    RenderTarget renderTarget = canvas.RenderTarget2D;
                    System.Drawing.Bitmap image = new System.Drawing.Bitmap(filename);
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
                    SharpDX.Direct2D1.PixelFormat pixelFormat = new SharpDX.Direct2D1.PixelFormat(SharpDX.DXGI.Format.B8G8R8A8_UNorm, SharpDX.Direct2D1.AlphaMode.Premultiplied);
                    BitmapProperties bitmapProperties = new BitmapProperties(pixelFormat, bitmap2.HorizontalResolution, bitmap2.VerticalResolution);
                    SharpDX.Direct2D1.Bitmap bitmap3 = new SharpDX.Direct2D1.Bitmap(renderTarget, new Size2(bitmap2.Width, bitmap2.Height), bitmapProperties);
                    bitmap3.CopyFromMemory(destination, bitmapdata.Stride);
                    bitmap2.Dispose();
                    image.Dispose();
                    return bitmap3;
                }
                catch (Exception exception)
                {
                    Console.WriteLine(exception.Message);
                }
            }
            return null;
        }

        public static RawColor4 RawColor4FromArgb(byte r, byte g, byte b) => 
            new RawColor4(((float) (((float) r) / 255f)).Clamp((float) 0f, (float) 1f), ((float) (((float) g) / 255f)).Clamp((float) 0f, (float) 1f), ((float) (((float) b) / 255f)).Clamp((float) 0f, (float) 1f), 1f);

        public static RawColor4 RawColor4FromArgb(byte a, byte r, byte g, byte b) => 
            new RawColor4(((float) (((float) r) / 255f)).Clamp((float) 0f, (float) 1f), ((float) (((float) g) / 255f)).Clamp((float) 0f, (float) 1f), ((float) (((float) b) / 255f)).Clamp((float) 0f, (float) 1f), ((float) (((float) a) / 255f)).Clamp((float) 0f, (float) 1f));

        public static RawColor4 ToRawColor4(this Color color, float opacity = 1f) => 
            RawColor4FromArgb((byte) ((float) (color.A * opacity)).Clamp(((float) 0f), ((float) 255f)), color.R, color.G, color.B);
    }
}

