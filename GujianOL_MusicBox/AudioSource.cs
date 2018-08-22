namespace GujianOL_MusicBox
{
    using SharpDX;
    using SharpDX.XAudio2;
    using System;
    using System.Runtime.CompilerServices;

    public class AudioSource
    {
        public AudioSource(DataStream stream, SourceVoice voice, float volume, long durationLeftMs, float decayFactor)
        {
            this.Stream = stream;
            this.Voice = voice;
            this.DurationLeftMs = durationLeftMs;
            if (this.DurationLeftMs == 0L)
            {
                this.DurationLeftMs = 0x174876e7ffL;
            }
            this.DecayFactor = decayFactor * volume;
            this.Volume = volume;
        }

        public float DecayFactor { get; set; }

        public long DurationLeftMs { get; set; }

        public DataStream Stream { get; set; }

        public SourceVoice Voice { get; set; }

        public float Volume { get; set; }
    }
}

