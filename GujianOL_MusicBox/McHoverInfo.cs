namespace GujianOL_MusicBox
{
    using System;

    public static class McHoverInfo
    {
        private static int _hoveringInsertNotePackIndex = -1;
        private static int _hoveringInsertPitchValue = 0;
        private static McMeasure _hoveringMeasure = null;
        private static McNotePack _hoveringNotePack = null;
        public static float HoveringDashAniOffset = 0f;
        public static float HoveringInsertLineAlphaSmooth = 0f;

        public static void Clear()
        {
            if (_hoveringNotePack != null)
            {
                _hoveringNotePack.IsHovering = false;
            }
            if (_hoveringMeasure != null)
            {
                _hoveringMeasure.IsHovering = false;
                McUtility.AppendRedrawingMeasure(_hoveringMeasure);
            }
            _hoveringInsertNotePackIndex = -1;
            _hoveringInsertPitchValue = 0;
            _hoveringNotePack = null;
            _hoveringMeasure = null;
        }

        public static int HoveringInsertNotePackIndex
        {
            get
            {
                if ((HoveringMeasure == null) || (_hoveringInsertNotePackIndex < 0))
                {
                    return HoveringMeasureInsertNotePackIndexMax;
                }
                _hoveringInsertNotePackIndex = _hoveringInsertNotePackIndex.Clamp(-1, HoveringMeasureInsertNotePackIndexMax);
                return _hoveringInsertNotePackIndex;
            }
            set
            {
                _hoveringInsertNotePackIndex = value.Clamp(-1, HoveringMeasureInsertNotePackIndexMax);
            }
        }

        public static int HoveringInsertPitchValue
        {
            get => 
                _hoveringInsertPitchValue;
            set
            {
                _hoveringInsertPitchValue = ((value < McPitch.PitchMin) || (value > McPitch.PitchMax)) ? 0 : value;
            }
        }

        public static McMeasure HoveringMeasure
        {
            get
            {
                McMeasure parentMeasure;
                if (_hoveringMeasure)
                {
                    parentMeasure = _hoveringMeasure;
                }
                else
                {
                    parentMeasure = _hoveringNotePack?.ParentMeasure;
                }
                return parentMeasure;
            }
            set
            {
                if (_hoveringMeasure != null)
                {
                    _hoveringMeasure.IsHovering = false;
                    if (_hoveringMeasure != value)
                    {
                        McUtility.AppendRedrawingMeasure(_hoveringMeasure);
                    }
                }
                _hoveringMeasure = value;
                if (_hoveringMeasure != null)
                {
                    _hoveringMeasure.IsHovering = true;
                }
            }
        }

        public static int HoveringMeasureInsertNotePackIndexMax =>
            ((HoveringMeasure == null) ? -1 : ((HoveringMeasure.NotePacksCount == 0) ? 0 : HoveringMeasure.NotePacksCount));

        public static McNotePack HoveringNotePack
        {
            get => 
                _hoveringNotePack;
            set
            {
                if (_hoveringNotePack != null)
                {
                    _hoveringNotePack.IsHovering = false;
                }
                _hoveringNotePack = value;
                if (_hoveringNotePack != null)
                {
                    _hoveringNotePack.IsHovering = true;
                }
            }
        }

        public static int ValidHoveringInsertPitchIndex
        {
            get
            {
                if (((HoveringMeasure == null) || (_hoveringInsertPitchValue < McPitch.PitchMin)) || (McPitch.PitchMin > McPitch.PitchMax))
                {
                    return 0;
                }
                if ((HoveringNotePack != null) && HoveringNotePack.IsPitchEnabled(_hoveringInsertPitchValue))
                {
                    return 0;
                }
                return _hoveringInsertPitchValue;
            }
        }
    }
}

