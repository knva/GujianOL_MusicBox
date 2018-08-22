namespace GujianOL_MusicBox
{
    using System;
    using System.Linq;

    public class VolumeCurve
    {
        private static VolumeCurve _default = null;
        private static readonly float[] _defaultVolumePoint = new float[] { 0.8f, 0.7f, 0.5f, 0.5f, 0.7f, 0.6f, 0.5f, 0.4f };
        public float[] VolumePoint = ((float[]) _defaultVolumePoint.Clone());

        public bool Equals(VolumeCurve volumeCurve)
        {
            for (int i = 0; i < _defaultVolumePoint.Length; i++)
            {
                if (Math.Abs((float) (this.VolumePoint[i] - volumeCurve.VolumePoint[i])) >= 0.0001f)
                {
                    return false;
                }
            }
            return true;
        }

        public float GetCurvedVolume(int index)
        {
            index = index.Clamp(0, 7);
            return this.VolumePoint[index];
        }

        public void SetCurvedVolume(int index, float value)
        {
            index = index.Clamp(0, _defaultVolumePoint.Length - 1);
            value = value.Clamp(0f, 1f);
            this.VolumePoint[index] = value;
        }

        public float AverageVolume =>
            this.VolumePoint.Average();

        public static VolumeCurve Default =>
            (_default ?? (_default = new VolumeCurve()));

        public static float[] DefaultVolumePoint =>
            _defaultVolumePoint;

        public float MaxVolume =>
            this.VolumePoint.Max();

        public float MinVolume =>
            this.VolumePoint.Min();
    }
}

