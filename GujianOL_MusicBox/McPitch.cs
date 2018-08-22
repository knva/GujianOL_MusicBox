namespace GujianOL_MusicBox
{
    using System;
    using System.Runtime.InteropServices;

    public class McPitch
    {
        private AlterantTypes _alterantTypes = AlterantTypes.NoControl;
        private readonly McNotePack _parentNotePack;
        private readonly PitchTypes _pitchType;
        private readonly int _scaleLevel;
        private readonly int _semitoneValue;
        private float _tieVolumeAlter = 1f;
        private readonly int _toneValue;
        private readonly int _value;
        public static readonly int PitchMax = 0x57;
        public static readonly int PitchMin = 4;

        private McPitch(McNotePack parent, int naturePitch, PitchTypes pitchType)
        {
            this._parentNotePack = parent;
            this._value = naturePitch;
            this._semitoneValue = GetSemitoneValue(naturePitch);
            this._toneValue = GetToneValue(naturePitch);
            this._scaleLevel = GetScaleLevel(naturePitch);
            this._pitchType = pitchType;
            if (((parent != null) && parent.IsViald) && (this._pitchType != PitchTypes.Temporary))
            {
                McUtility.MarkModified(parent.ParentMeasure);
            }
        }

        public static int ConvertToNaturalPitch(int pitch, AlterantTypes asAlterantType, out int pitchOffset)
        {
            pitch = pitch.Clamp(PitchMin, PitchMax);
            if (IsNatural(pitch))
            {
                pitchOffset = 0;
                return pitch;
            }
            pitchOffset = (asAlterantType == AlterantTypes.Flat) ? 1 : -1;
            return (pitch + pitchOffset);
        }

        public static McPitch FromNaturalPitchValue(int naturaPitch, PitchTypes pitchType = 2) => 
            FromNaturalPitchValue(null, naturaPitch, pitchType);

        public static McPitch FromNaturalPitchValue(McNotePack parent, int naturaPitch, PitchTypes pitchType = 2)
        {
            if (!IsNatural(naturaPitch))
            {
                return null;
            }
            return new McPitch(parent, naturaPitch, pitchType);
        }

        public static int GetMeasureFirstLineRelativeY(McMeasure.ClefTypes clefType) => 
            ((clefType == McMeasure.ClefTypes.L2G) ? 0xa6 : 0xc2);

        public static int GetMeasureLineRelativeY(McMeasure.ClefTypes clefType, int pitch)
        {
            int measureLineValue = GetMeasureLineValue(clefType, pitch);
            return (GetMeasureFirstLineRelativeY(clefType) - ((McMeasure.StaveSpacing * measureLineValue) / 10));
        }

        public static int GetMeasureLineRelativeYByLineValue(McMeasure.ClefTypes clefType, int lineValue) => 
            (GetMeasureFirstLineRelativeY(clefType) - ((McMeasure.StaveSpacing * lineValue) / 10));

        public static int GetMeasureLineValue(McMeasure.ClefTypes clefType, int pitch)
        {
            int toneValue = GetToneValue(pitch);
            int scaleLevel = GetScaleLevel(pitch);
            int num3 = (clefType == McMeasure.ClefTypes.L2G) ? -10 : 50;
            return (num3 + ((((scaleLevel - 3) * 7) * 5) + ((toneValue - 1) * 5)));
        }

        public static int GetScaleLevel(int pitch)
        {
            pitch = pitch.Clamp(PitchMin, PitchMax);
            return ((pitch - PitchMin) / 12).Clamp(0, 6);
        }

        public static int GetSemitoneValue(int pitch)
        {
            pitch = pitch.Clamp(PitchMin, PitchMax);
            return (((pitch - PitchMin) % 12) + 1).Clamp(1, 12);
        }

        public static int GetToneValue(int pitch)
        {
            pitch = pitch.Clamp(PitchMin, PitchMax);
            switch (GetSemitoneValue(pitch))
            {
                case 1:
                case 2:
                    return 1;

                case 3:
                case 4:
                    return 2;

                case 5:
                    return 3;

                case 6:
                case 7:
                    return 4;

                case 8:
                case 9:
                    return 5;

                case 10:
                case 11:
                    return 6;

                case 12:
                    return 7;
            }
            return 0;
        }

        public static bool IsNatural(int pitch)
        {
            pitch = pitch.Clamp(PitchMin, PitchMax);
            switch (GetSemitoneValue(pitch))
            {
                case 2:
                case 4:
                case 7:
                case 9:
                case 11:
                    return false;
            }
            return true;
        }

        public AlterantStates AlterantState
        {
            get
            {
                AlterantStates rawAlterantState = this.RawAlterantState;
                switch (rawAlterantState)
                {
                    case AlterantStates.UnalteredNatural:
                        switch (this.AlterantType)
                        {
                            case AlterantTypes.NoControl:
                                return AlterantStates.UnalteredNatural;

                            case AlterantTypes.Natural:
                                return AlterantStates.UnalteredNatural;

                            case AlterantTypes.Sharp:
                                return AlterantStates.AlteredSharp;

                            case AlterantTypes.Flat:
                                return AlterantStates.AlteredFlat;
                        }
                        return rawAlterantState;

                    case AlterantStates.UnalteredSharp:
                        switch (this.AlterantType)
                        {
                            case AlterantTypes.NoControl:
                                return AlterantStates.UnalteredSharp;

                            case AlterantTypes.Natural:
                                return AlterantStates.AlteredNatural;

                            case AlterantTypes.Sharp:
                                return AlterantStates.UnalteredSharp;

                            case AlterantTypes.Flat:
                                return AlterantStates.AlteredFlat;
                        }
                        return rawAlterantState;

                    case AlterantStates.UnalteredFlat:
                        switch (this.AlterantType)
                        {
                            case AlterantTypes.NoControl:
                                return AlterantStates.UnalteredFlat;

                            case AlterantTypes.Natural:
                                return AlterantStates.AlteredNatural;

                            case AlterantTypes.Sharp:
                                return AlterantStates.AlteredSharp;

                            case AlterantTypes.Flat:
                                return AlterantStates.UnalteredFlat;
                        }
                        return rawAlterantState;

                    case AlterantStates.AlteredNatural:
                    case AlterantStates.AlteredSharp:
                    case AlterantStates.AlteredFlat:
                        throw new Exception("McPitch.AlterantState -> " + rawAlterantState);
                }
                return rawAlterantState;
            }
        }

        public AlterantTypes AlterantType
        {
            get
            {
                if (this._alterantTypes == AlterantTypes.NoControl)
                {
                    McNotePack parentNotePack = this.ParentNotePack;
                    McMeasure parentMeasure = parentNotePack.ParentMeasure;
                    int index = parentNotePack.Index;
                    McNotePack[] notePacks = parentMeasure.NotePacks;
                    for (int i = index - 1; i >= 0; i--)
                    {
                        McNotePack pack2 = notePacks[i];
                        foreach (McPitch pitch in pack2.ValidPitchArray)
                        {
                            if ((pitch.PitchType == PitchTypes.Enabled) && (pitch.Value == this.Value))
                            {
                                return pitch.AlterantType;
                            }
                        }
                    }
                }
                return this._alterantTypes;
            }
            set
            {
                this._alterantTypes = value;
            }
        }

        public int AlterantValue
        {
            get
            {
                switch (this.AlterantState)
                {
                    case AlterantStates.UnalteredNatural:
                    case AlterantStates.AlteredNatural:
                        return this.Value;

                    case AlterantStates.UnalteredSharp:
                    case AlterantStates.AlteredSharp:
                        return (this.Value + 1);

                    case AlterantStates.UnalteredFlat:
                    case AlterantStates.AlteredFlat:
                        return (this.Value - 1);
                }
                return this.Value;
            }
        }

        public bool IsRest =>
            ((this.ParentNotePack != null) && this.ParentNotePack.IsRest);

        public int MeasureKeySignature
        {
            get
            {
                if ((this._parentNotePack == null) || (this._parentNotePack.ParentMeasure == null))
                {
                    return 0;
                }
                return this._parentNotePack.ParentMeasure.KeySignature;
            }
        }

        public float MeasureLineRelativeY
        {
            get
            {
                McMeasure measure = ((this.ParentNotePack != null) && (this.ParentNotePack.ParentMeasure != null)) ? this.ParentNotePack.ParentMeasure : null;
                McMeasure.ClefTypes clefType = (measure != null) ? measure.ClefType : McMeasure.ClefTypes.L2G;
                return (float) GetMeasureLineRelativeY(clefType, this.Value);
            }
        }

        public int MeasureLineValue
        {
            get
            {
                McMeasure measure = ((this.ParentNotePack != null) && (this.ParentNotePack.ParentMeasure != null)) ? this.ParentNotePack.ParentMeasure : null;
                McMeasure.ClefTypes clefType = (measure != null) ? measure.ClefType : McMeasure.ClefTypes.L2G;
                return GetMeasureLineValue(clefType, this.Value);
            }
        }

        public McNotePack ParentNotePack =>
            this._parentNotePack;

        public PitchTypes PitchType =>
            this._pitchType;

        public AlterantStates RawAlterantState
        {
            get
            {
                int semitoneValue;
                switch (this.MeasureKeySignature)
                {
                    case -7:
                        switch (this.SemitoneValue)
                        {
                            case 1:
                            case 3:
                            case 5:
                            case 6:
                            case 8:
                            case 10:
                            case 12:
                                return AlterantStates.UnalteredFlat;
                        }
                        break;

                    case -6:
                        switch (this.SemitoneValue)
                        {
                            case 1:
                            case 3:
                            case 5:
                            case 8:
                            case 10:
                            case 12:
                                return AlterantStates.UnalteredFlat;
                        }
                        break;

                    case -5:
                        switch (this.SemitoneValue)
                        {
                            case 3:
                            case 5:
                            case 8:
                            case 10:
                            case 12:
                                return AlterantStates.UnalteredFlat;
                        }
                        break;

                    case -4:
                        switch (this.SemitoneValue)
                        {
                            case 3:
                            case 5:
                            case 10:
                            case 12:
                                return AlterantStates.UnalteredFlat;
                        }
                        break;

                    case -3:
                        switch (this.SemitoneValue)
                        {
                            case 10:
                            case 12:
                            case 5:
                                return AlterantStates.UnalteredFlat;
                        }
                        break;

                    case -2:
                        semitoneValue = this.SemitoneValue;
                        if ((semitoneValue != 5) && (semitoneValue != 12))
                        {
                            break;
                        }
                        return AlterantStates.UnalteredFlat;

                    case -1:
                        if (this.SemitoneValue != 12)
                        {
                            break;
                        }
                        return AlterantStates.UnalteredFlat;

                    case 1:
                        if (this.SemitoneValue != 6)
                        {
                            break;
                        }
                        return AlterantStates.UnalteredSharp;

                    case 2:
                        semitoneValue = this.SemitoneValue;
                        if ((semitoneValue != 1) && (semitoneValue != 6))
                        {
                            break;
                        }
                        return AlterantStates.UnalteredSharp;

                    case 3:
                        switch (this.SemitoneValue)
                        {
                            case 6:
                            case 8:
                            case 1:
                                return AlterantStates.UnalteredSharp;
                        }
                        break;

                    case 4:
                        switch (this.SemitoneValue)
                        {
                            case 1:
                            case 3:
                            case 6:
                            case 8:
                                return AlterantStates.UnalteredSharp;
                        }
                        break;

                    case 5:
                        switch (this.SemitoneValue)
                        {
                            case 1:
                            case 3:
                            case 6:
                            case 8:
                            case 10:
                                return AlterantStates.UnalteredSharp;
                        }
                        break;

                    case 6:
                        switch (this.SemitoneValue)
                        {
                            case 1:
                            case 3:
                            case 5:
                            case 6:
                            case 8:
                            case 10:
                                return AlterantStates.UnalteredSharp;
                        }
                        break;

                    case 7:
                        switch (this.SemitoneValue)
                        {
                            case 1:
                            case 3:
                            case 5:
                            case 6:
                            case 8:
                            case 10:
                            case 12:
                                return AlterantStates.UnalteredSharp;
                        }
                        break;
                }
                return AlterantStates.UnalteredNatural;
            }
        }

        public AlterantTypes RawAlterantType =>
            this._alterantTypes;

        public int ScaleLevel =>
            this._scaleLevel;

        public int SemitoneValue =>
            this._semitoneValue;

        public float TieVolumeAlter
        {
            get
            {
                McNotePack.TieTypes tieType = this.ParentNotePack.TieType;
                return (((tieType == McNotePack.TieTypes.End) || (tieType == McNotePack.TieTypes.Both)) ? this._tieVolumeAlter : 1f);
            }
            set
            {
                this._tieVolumeAlter = value;
            }
        }

        public int ToneValue =>
            this._toneValue;

        public int Value =>
            this._value;

        public float Volume =>
            (((this.ParentNotePack == null) ? 1f : this.ParentNotePack.Volume) * this.TieVolumeAlter);

        public enum AlterantStates
        {
            UnalteredNatural,
            UnalteredSharp,
            UnalteredFlat,
            AlteredNatural,
            AlteredSharp,
            AlteredFlat
        }

        public enum AlterantTypes
        {
            NoControl,
            Natural,
            Sharp,
            Flat
        }

        public enum PitchTypes
        {
            Enabled,
            Disabled,
            Temporary
        }
    }
}

