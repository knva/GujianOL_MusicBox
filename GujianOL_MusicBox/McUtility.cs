namespace GujianOL_MusicBox
{
    using SharpDX.Mathematics.Interop;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    public static class McUtility
    {
        private static List<McMeasure> _deferredRedrawingMeasureList = new List<McMeasure>();
        private static List<McMeasure> _redrawingMeasureList = new List<McMeasure>();

        public static void AppendDeferredRedrawingMeasure(McMeasure measure)
        {
            if (!((measure == null) || _deferredRedrawingMeasureList.Contains(measure)))
            {
                _deferredRedrawingMeasureList.Add(measure);
            }
        }

        public static void AppendDeferredRedrawingMeasures(List<McMeasure>[] drawingMeasures)
        {
            foreach (List<McMeasure> list in drawingMeasures)
            {
                if (list != null)
                {
                    foreach (McMeasure measure in list)
                    {
                        AppendDeferredRedrawingMeasure(measure);
                    }
                }
            }
        }

        public static void AppendRedrawingMeasure(McMeasure measure)
        {
            if (!((measure == null) || _redrawingMeasureList.Contains(measure)))
            {
                _redrawingMeasureList.Add(measure);
            }
        }

        public static void ClearDeferredRedrawingMeasure()
        {
            _deferredRedrawingMeasureList.Clear();
        }

        public static void ClearModifiedState()
        {
            RedrawMeasureInRedrawingList();
            IsModified = false;
        }

        public static void ClearRedrawingMeasure()
        {
            _redrawingMeasureList.Clear();
        }

        public static string FormatNotationText(this string str) => 
            str.Replace(@"\", "/").Replace("'", "").Replace("\r", "").Replace("\n", "").Replace("\t", " ");

        public static float GetAngleByRawVector2(this RawVector2 posStart, RawVector2 posEnd)
        {
            double num = posEnd.Y - posStart.Y;
            double num2 = posEnd.X - posStart.X;
            float num3 = ((float) Math.Atan(num / num2)).ToDegrees() % 360f;
            if (posEnd.X >= posStart.X)
            {
                if (posEnd.Y < posStart.Y)
                {
                    return Math.Abs(num3);
                }
                return (360f - num3);
            }
            if (posEnd.Y < posStart.Y)
            {
                return (180f - num3);
            }
            return (180f + Math.Abs(num3));
        }

        public static object GetDefaultValue(Type type, string fieldName)
        {
            BindingFlags bindingAttr = BindingFlags.NonPublic | BindingFlags.Instance;
            FieldInfo[] fields = type.GetFields(bindingAttr);
            foreach (FieldInfo info in fields)
            {
                if (info.Name == fieldName)
                {
                    foreach (Attribute attribute in Attribute.GetCustomAttributes(info))
                    {
                        if (attribute is DefaultValueAttribute)
                        {
                            DefaultValueAttribute attribute2 = attribute as DefaultValueAttribute;
                            return attribute2.Value;
                        }
                    }
                }
            }
            return null;
        }

        public static float GetDistanceByRawVector2(this RawVector2 posStart, RawVector2 posEnd) => 
            ((float) Math.Sqrt((double) (((posStart.X - posEnd.X) * (posStart.X - posEnd.X)) + ((posStart.Y - posEnd.Y) * (posStart.Y - posEnd.Y)))));

        public static RawVector2 GetRawVector2ByPolarCoordinates(this RawVector2 posStart, float angle, float dist)
        {
            float num = ((float) Math.Cos((double) angle.ToRadians())) * dist;
            float num2 = ((float) Math.Sin((double) angle.ToRadians())) * dist;
            return new RawVector2(posStart.X + num, posStart.Y - num2);
        }

        public static void MarkModified(McMeasure measure)
        {
            IsModified = true;
            AppendRedrawingMeasure(measure);
            if (((measure != null) && !MusicCanvasControl.MouseDragareaManager.IsInPickingProgress) && (MusicCanvasControl.MouseDragareaManager.LastPickedRelativeMeasures != null))
            {
                MusicCanvasControl.MouseDragareaManager.ClearValidCanvasDragarea();
            }
        }

        public static void RedrawMeasureInDeferredRedrawingList(bool redrawDisplayed = true)
        {
            foreach (McMeasure measure in _deferredRedrawingMeasureList.ToArray())
            {
                if (!(redrawDisplayed && !measure.IsDisplay))
                {
                    measure.RedrawBitmapCache();
                    _deferredRedrawingMeasureList.Remove(measure);
                    RemoveRedrawingMeasure(measure);
                    break;
                }
            }
        }

        public static void RedrawMeasureInRedrawingList()
        {
            foreach (McMeasure measure in _redrawingMeasureList.ToArray())
            {
                measure.RedrawBitmapCache();
                _redrawingMeasureList.Remove(measure);
                RemoveDeferredRedrawingMeasure(measure);
            }
        }

        public static void RemoveDeferredRedrawingMeasure(McMeasure measure)
        {
            if ((measure != null) && _deferredRedrawingMeasureList.Contains(measure))
            {
                _deferredRedrawingMeasureList.Remove(measure);
            }
        }

        public static void RemoveRedrawingMeasure(McMeasure measure)
        {
            if ((measure != null) && _redrawingMeasureList.Contains(measure))
            {
                _redrawingMeasureList.Remove(measure);
            }
        }

        public static float ToDegrees(this float radians) => 
            (57.29578f * radians);

        public static float ToRadians(this float degrees) => 
            (0.01745329f * degrees);

        public static List<McMeasure> DeferredRedrawingMeasureList =>
            _deferredRedrawingMeasureList;

        public static bool IsModified
        {
            [CompilerGenerated]
            get => 
                <IsModified>k__BackingField;
            [CompilerGenerated]
            private set
            {
                <IsModified>k__BackingField = value;
            }
        }

        public static List<McMeasure> RedrawingMeasureList =>
            _redrawingMeasureList;
    }
}

