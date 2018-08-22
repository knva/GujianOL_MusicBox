namespace GujianOL_MusicBox
{
    using GujianOL_MusicBox_Resources;
    using NLua;
    using SharpDX;
    using SharpDX.Direct2D1;
    using SharpDX.DirectWrite;
    using SharpDX.DXGI;
    using SharpDX.Mathematics.Interop;
    using SharpDX.Multimedia;
    using SharpDX.WIC;
    using SharpDX.Windows;
    using SharpDX.XAudio2;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Drawing;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Threading;
    using System.Windows.Forms;
    using Telerik.WinControls;

    public class MusicCanvasControl : UserControl
    {
        private string _commonMessage = "";
        private int _commonMessageAlpha = 0;
        private List<McMeasure>[] _drawingMeasures = new List<McMeasure>[3];
        private static bool _isCompressEnabled = false;
        private bool _isLoading = false;
        private bool _isMusicPlaying = false;
        private Dictionary<Keys, int> _keysPressedRepeatEventRecords = new Dictionary<Keys, int>();
        private string _lastAccessedFilename = "";
        private int _lastValidDurationStampPtr = 0;
        private string _loadingFilename = "";
        private int _loadingFilenameAlpha = 0;
        private MasteringVoice _masteringVoice;
        private OpenFileDialog _openFileDialog = new OpenFileDialog();
        private float _playingAutoScrollXSmooth = 0f;
        private int _playingDeltaDurationStampPtr = 0;
        private int _playingHemidemisemiquaverPtr = -1;
        private int _playingHemidemisemiquaverTimeMs = 8;
        private double _playingLoopingAccumulatedTimeMs = 0.0;
        private int _playingMeasureIndex = 0;
        private float _playingNextValidStampAbsX = 0f;
        private float _playingNextValidStampAbsXSmooth = 0f;
        private float _playingSpeed = 1f;
        private SaveFileDialog _saveFileDialog = new SaveFileDialog();
        private int _scrollX = 0;
        private float _scrollXSmooth = 0f;
        private Dictionary<string, AudioSource> _sourceVoices;
        private List<int> _updateElapsedMsList = new List<int>();
        private Stopwatch _updateStopwatch;
        private long _updateStopwatchElapsedMs;
        private long _updateStopwatchLastMs;
        private System.Windows.Forms.Timer _updateTimer;
        private SharpDX.XAudio2.XAudio2 _xaudio2;
        private readonly RawColor4 BackgroundColor = Color.FromArgb(40, 40, 40).ToRawColor4(1f);
        private IContainer components = null;
        public static bool EnableAutoScrollWhenPlayMusic = true;
        public static bool EnableControlTip = true;
        public static bool EnableDelaylessCommand = false;
        public static bool EnableFps = false;
        public static bool EnableFreePlayMode = false;
        public static bool EnableNumberedSignTip = true;
        public static bool EnablePlaySoundWhenInsert = true;
        public static bool EnableSnaplineWhenInsertNotePack = false;
        public static bool EnableStemAnimation = true;

        public event EventHandler<MusicCanvasIoEventArgs> OnMusicCanvasAccessedFilenameChanged;

        public event EventHandler<MusicCanvasIoEventArgs> OnMusicCanvasFileLoaded;

        public event EventHandler<MusicCanvasIoEventArgs> OnMusicCanvasFileSaved;

        public MusicCanvasControl(GujianOL_MusicBox.GujianOL_MusicBox musicbox)
        {
            this.InitializeComponent();
            this.MusicBox = musicbox;
            this.PropertyPanel = new GujianOL_MusicBox.PropertyPanel();
            this.InitializeAudioPlayer();
            this.InitializeRenderControl();
            this.InitializeMusicalNotation();
            this.InitializeFileSystem();
            RadMessageBox.ThemeName = "VisualStudio2012Dark";
            Canvas = this;
        }

        public void AudioPlayerMainLoop(long updateStopwatchElapsedMs)
        {
            this.CheckAndClearExpiredAudioSource(updateStopwatchElapsedMs);
            if (this.IsMusicPlaying)
            {
                Arpeggio.EffectArpeggios((int) updateStopwatchElapsedMs);
                McRegularTrack track = this.Notation.RegularTracks.First<McRegularTrack>();
                McMeasure measure = track.GetMeasure(this._playingMeasureIndex);
                if (measure == null)
                {
                    this.StopMusic();
                }
                else
                {
                    if (EnableAutoScrollWhenPlayMusic)
                    {
                        this._scrollX = (int) ((float) (this._playingAutoScrollXSmooth - (((float) base.Width) / 2f))).Clamp(((float) 0f), ((float) 1E+08f));
                    }
                    this._playingHemidemisemiquaverTimeMs = Math.Max(8, (int) (1000f / (((float) (this.Notation.BeatDuration * measure.BeatsPerMinute)) / 60f)));
                    this._playingLoopingAccumulatedTimeMs += updateStopwatchElapsedMs * this._playingSpeed;
                    if (this._playingLoopingAccumulatedTimeMs >= this._playingHemidemisemiquaverTimeMs)
                    {
                        McNotation.DurationStamp stamp2;
                        McNotePack anyValidNotePack;
                        float num4;
                        this._playingLoopingAccumulatedTimeMs = this._playingLoopingAccumulatedTimeMs % ((double) this._playingHemidemisemiquaverTimeMs);
                        this._playingHemidemisemiquaverPtr++;
                        int durationStampMax = measure.TemporaryInfo.DurationStampMax;
                        if (this._playingHemidemisemiquaverPtr > durationStampMax)
                        {
                            this._playingMeasureIndex++;
                            this._playingHemidemisemiquaverPtr = 0;
                            measure = track.GetMeasure(this._playingMeasureIndex);
                            if (measure == null)
                            {
                                this.StopMusic();
                                return;
                            }
                        }
                        SortedDictionary<int, McNotation.DurationStamp> reorganizedDurationStamps = measure.TemporaryInfo.ReorganizedDurationStamps;
                        int stampIndex = -1;
                        McNotation.DurationStamp stamp = null;
                        foreach (int num3 in reorganizedDurationStamps.Keys.ToArray<int>())
                        {
                            if (num3 >= this._playingHemidemisemiquaverPtr)
                            {
                                stampIndex = num3;
                                stamp2 = reorganizedDurationStamps[stampIndex];
                                anyValidNotePack = stamp2.GetAnyValidNotePack();
                                num4 = (measure.TemporaryInfo.RelativeX + McNotation.Margin.Left) + McRegularTrack.Margin.Left;
                                this._playingNextValidStampAbsX = Math.Max(this._playingNextValidStampAbsX, ((num4 + anyValidNotePack.TemporaryInfo.RelativeX) + 8f) + 1f);
                                this._playingDeltaDurationStampPtr = num3 - this._lastValidDurationStampPtr;
                                if (num3 == this._playingHemidemisemiquaverPtr)
                                {
                                    stamp = stamp2;
                                    this._lastValidDurationStampPtr = this._playingHemidemisemiquaverPtr;
                                }
                                break;
                            }
                        }
                        if (stampIndex < 0)
                        {
                            McMeasure measure2 = track.GetMeasure(this._playingMeasureIndex + 1);
                            if ((measure2 != null) && (measure2.TemporaryInfo.ReorganizedDurationStamps.Count > 0))
                            {
                                stamp2 = measure2.TemporaryInfo.ReorganizedDurationStamps.Values.First<McNotation.DurationStamp>();
                                stampIndex = stamp2.StampIndex;
                                anyValidNotePack = stamp2.GetAnyValidNotePack();
                                num4 = (measure2.TemporaryInfo.RelativeX + McNotation.Margin.Left) + McRegularTrack.Margin.Left;
                                this._playingNextValidStampAbsX = Math.Max(this._playingNextValidStampAbsX, ((num4 + anyValidNotePack.TemporaryInfo.RelativeX) + 8f) + 1f);
                                this._playingDeltaDurationStampPtr = ((durationStampMax - this._lastValidDurationStampPtr) + stampIndex) + 1;
                                if (this._playingHemidemisemiquaverPtr == durationStampMax)
                                {
                                    this._lastValidDurationStampPtr = (this._lastValidDurationStampPtr - durationStampMax) - 1;
                                }
                            }
                            else
                            {
                                stampIndex = durationStampMax;
                                num4 = (measure.TemporaryInfo.RelativeX + McNotation.Margin.Left) + McRegularTrack.Margin.Left;
                                this._playingNextValidStampAbsX = Math.Max(this._playingNextValidStampAbsX, num4 + measure.TemporaryInfo.Width);
                                this._playingDeltaDurationStampPtr = durationStampMax - this._lastValidDurationStampPtr;
                            }
                        }
                        if ((stampIndex >= 0) && (stamp != null))
                        {
                            McMeasure[] measuresAligned = measure.MeasuresAligned;
                            for (num3 = 0; num3 < measuresAligned.Length; num3++)
                            {
                                McMeasure measure3 = measuresAligned[num3];
                                anyValidNotePack = stamp.GetNotePack(num3);
                                if (((anyValidNotePack != null) && (measure3 != null)) && !anyValidNotePack.IsRest)
                                {
                                    Arpeggio.AppendArpeggio(anyValidNotePack, 2f);
                                }
                            }
                        }
                    }
                }
            }
        }

        public void CheckAndClearExpiredAudioSource(long updateStopwatchElapsedMs)
        {
            List<string> list = new List<string>();
            foreach (string str in this._sourceVoices.Keys)
            {
                AudioSource audioSource = this._sourceVoices[str];
                audioSource.DurationLeftMs -= updateStopwatchElapsedMs;
                if ((audioSource.Voice.Volume < 0.02f) || (audioSource.Voice.State.BuffersQueued <= 0))
                {
                    this.DisposeAudioSource(audioSource);
                    list.Add(str);
                }
                else if (audioSource.DurationLeftMs < 0L)
                {
                    audioSource.Voice.SetVolume(((float) (audioSource.Voice.Volume - audioSource.DecayFactor)).Clamp((float) 0f, (float) 1f), 0);
                }
            }
            foreach (string str in list)
            {
                this._sourceVoices.Remove(str);
            }
        }

        private void DeleteSelectionNotePacks()
        {
            Dictionary<McMeasure, bool> dictionary = new Dictionary<McMeasure, bool>();
            Dictionary<int, List<Dictionary<McNotePack, bool>>> lastPickedNotePacks = MouseDragareaManager.LastPickedNotePacks;
            if (lastPickedNotePacks != null)
            {
                bool flag = false;
                foreach (List<Dictionary<McNotePack, bool>> list in lastPickedNotePacks.Values)
                {
                    foreach (Dictionary<McNotePack, bool> dictionary3 in list)
                    {
                        foreach (McNotePack pack in dictionary3.Keys)
                        {
                            if (dictionary3[pack])
                            {
                                McMeasure parentMeasure = pack.ParentMeasure;
                                if (!dictionary.ContainsKey(parentMeasure))
                                {
                                    dictionary.Add(parentMeasure, true);
                                }
                                parentMeasure.RemoveNotePack(pack);
                                flag = true;
                            }
                        }
                    }
                }
                if (flag)
                {
                    foreach (McMeasure measure2 in dictionary.Keys)
                    {
                        measure2.ReorganizeDurationStamps();
                        foreach (McMeasure measure3 in measure2.MeasuresAligned)
                        {
                            if (measure3 == measure2)
                            {
                                McUtility.MarkModified(measure3);
                            }
                            else
                            {
                                McUtility.AppendRedrawingMeasure(measure3);
                            }
                        }
                    }
                }
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && (this.components != null))
            {
                this.components.Dispose();
            }
            base.Dispose(disposing);
        }

        public void DisposeAudioSource(AudioSource audioSource)
        {
            if (audioSource != null)
            {
                if (audioSource.Voice != null)
                {
                    audioSource.Voice.Stop();
                    audioSource.Voice.DestroyVoice();
                    audioSource.Voice.Dispose();
                }
                if (audioSource.Stream != null)
                {
                    audioSource.Stream.Dispose();
                }
            }
        }

        public bool ExportMgnFile()
        {
            Exception exception;
            if (!this.SaveMusicalNotation())
            {
                return false;
            }
            Lua lua = new Lua();
            string str = "";
            Encoding encoding = new UTF8Encoding(false);
            try
            {
                str = File.ReadAllText(this.LastAccessedFilename, encoding);
                string str2 = str.DecompressString(null);
                if (!string.IsNullOrEmpty(str2))
                {
                    str = str2;
                }
                if (string.IsNullOrEmpty(str))
                {
                    return false;
                }
                lua.DoString(str, "chunk");
            }
            catch (Exception exception1)
            {
                exception = exception1;
                RadMessageBox.Show(exception.Message);
                return false;
            }
            lua.Dispose();
            lua = null;
            if (string.IsNullOrEmpty(str))
            {
                return false;
            }
            string str3 = null;
            try
            {
                lua = new Lua();
                lua.DoString("szGjmContents = [=[" + str + "]=]", "chunk");
                lua.DoFile("Utilities/MgnConverter.lua");
                lua.DoString($"szMgnContents = ConvertGjmToMgn(szGjmContents, 'GJM::{this.GetMD5HashString(encoding, str)}')", "chunk");
                str3 = lua.GetString("szMgnContents");
            }
            catch (Exception exception2)
            {
                exception = exception2;
                lua.Dispose();
                lua = null;
                Console.WriteLine(exception.Message);
                return false;
            }
            lua.Dispose();
            lua = null;
            if (string.IsNullOrEmpty(str3))
            {
                return false;
            }
            string path = Path.ChangeExtension(this.LastAccessedFilename, "mgn");
            if (File.Exists(path))
            {
                FileInfo info = new FileInfo(path) {
                    IsReadOnly = false
                };
            }
            File.WriteAllText(path, str3, encoding);
            return true;
        }

        public static object GetDefaultValue(string fieldName) => 
            McUtility.GetDefaultValue(typeof(MusicCanvasControl), fieldName);

        private string GetMD5HashString(Encoding encode, string sourceStr)
        {
            byte[] bytes = encode.GetBytes(sourceStr);
            return this.HashCore(bytes).ToString();
        }

        public int GetNotationTracksMaxStampCount()
        {
            int num = 0;
            foreach (McMeasure measure in this.Notation.RegularTracks.First<McRegularTrack>().MeasureArray)
            {
                num += measure.TemporaryInfo.ReorganizedDurationStamps.Keys.Count;
            }
            return num;
        }

        private List<List<Dictionary<McNotePack, bool>>> GetValidPickedNotePacks()
        {
            if (McHoverInfo.HoveringMeasure == null)
            {
                return null;
            }
            Dictionary<int, List<Dictionary<McNotePack, bool>>> lastPickedNotePacks = MouseDragareaManager.LastPickedNotePacks;
            if (lastPickedNotePacks == null)
            {
                return null;
            }
            List<List<Dictionary<McNotePack, bool>>> list = new List<List<Dictionary<McNotePack, bool>>>();
            foreach (List<Dictionary<McNotePack, bool>> list2 in lastPickedNotePacks.Values)
            {
                bool flag = false;
                foreach (Dictionary<McNotePack, bool> dictionary2 in list2)
                {
                    foreach (bool flag2 in dictionary2.Values)
                    {
                        if (flag2)
                        {
                            flag = flag2;
                            break;
                        }
                    }
                    if (flag)
                    {
                        break;
                    }
                }
                if ((list.Count != 0) || flag)
                {
                    list.Add(list2);
                }
            }
            if (list.Count == 0)
            {
                return null;
            }
            return list;
        }

        private uint HashCore(byte[] source)
        {
            uint num = 0x521;
            uint num2 = 0;
            foreach (byte num3 in source)
            {
                num2 = (num2 * num) + num3;
            }
            return (num2 & 0x7fffffff);
        }

        public void InitializeAudioPlayer()
        {
            this._xaudio2 = new SharpDX.XAudio2.XAudio2();
            this._masteringVoice = new MasteringVoice(this._xaudio2, 2, 0xac44);
            this._sourceVoices = new Dictionary<string, AudioSource>();
        }

        private void InitializeComponent()
        {
            this.components = new Container();
            base.AutoScaleMode = AutoScaleMode.Font;
        }

        private void InitializeDirect2DAndDirectWrite()
        {
            EventHandler handler = null;
            HwndRenderTargetProperties hwndProperties = new HwndRenderTargetProperties {
                Hwnd = this.RenderCanvas.Handle,
                PixelSize = new Size2(this.RenderCanvas.ClientSize.Width, this.RenderCanvas.ClientSize.Height),
                PresentOptions = PresentOptions.Immediately
            };
            if (this.RenderTarget2D != null)
            {
                this.RenderTarget2D.Dispose();
                RenderTargetProperties renderTargetProperties = new RenderTargetProperties(new SharpDX.Direct2D1.PixelFormat(SharpDX.DXGI.Format.Unknown, SharpDX.Direct2D1.AlphaMode.Premultiplied));
                this.RenderTarget2D = new WindowRenderTarget(this.Factory2D, renderTargetProperties, hwndProperties);
            }
            else
            {
                if (this.WicImagingFactory != null)
                {
                    this.WicImagingFactory.Dispose();
                }
                this.WicImagingFactory = new ImagingFactory();
                if (this.FactoryDWrite != null)
                {
                    this.FactoryDWrite.Dispose();
                }
                this.FactoryDWrite = new SharpDX.DirectWrite.Factory();
                if (this.Factory2D != null)
                {
                    this.Factory2D.Dispose();
                }
                this.Factory2D = new SharpDX.Direct2D1.Factory();
                this.RenderTarget2D = new WindowRenderTarget(this.Factory2D, new RenderTargetProperties(new SharpDX.Direct2D1.PixelFormat(SharpDX.DXGI.Format.Unknown, SharpDX.Direct2D1.AlphaMode.Premultiplied)), hwndProperties);
                this.RenderCanvas.Paint += new PaintEventHandler(this.RenderControl_Paint);
                if (handler == null)
                {
                    handler = delegate (object sender, EventArgs args) {
                        try
                        {
                            this.RenderTarget2D.Resize(new Size2(this.RenderCanvas.ClientSize.Width, this.RenderCanvas.ClientSize.Height));
                            this.RefreshCanvas();
                        }
                        catch (Exception exception)
                        {
                            Console.WriteLine(exception.Message);
                        }
                    };
                }
                this.RenderCanvas.Resize += handler;
            }
            this.RenderTarget2D.AntialiasMode = AntialiasMode.PerPrimitive;
            this.RenderTarget2D.TextAntialiasMode = SharpDX.Direct2D1.TextAntialiasMode.Cleartype;
            CanvasRenderCache.InitializeCanvasRenderCache(this.RenderTarget2D);
            if (this.Logo != null)
            {
                this.Logo.Dispose();
            }
            this.Logo = this.RenderTarget2D.LoadBitmapFromResourceName("Images.Logo.png");
        }

        public void InitializeFileSystem()
        {
            this._openFileDialog.DefaultExt = "gjm";
            this._openFileDialog.Filter = "Gujian-Online musical notation file|*.gjm";
            this._openFileDialog.Title = "打开古剑奇谭网络版乐谱文件";
            this._saveFileDialog.DefaultExt = "gjm";
            this._saveFileDialog.Filter = "Gujian-Online musical notation file|*.gjm";
            this._saveFileDialog.Title = "保存古剑奇谭网络版乐谱文件";
            string path = Application.StartupPath + @"\Notations";
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            this._saveFileDialog.InitialDirectory = path;
            this._openFileDialog.InitialDirectory = path;
            this._saveFileDialog.FileOk += (sender, args) => (this._saveFileDialog.InitialDirectory = null);
            this._openFileDialog.FileOk += (sender, args) => (this._openFileDialog.InitialDirectory = null);
        }

        public void InitializeMusicalNotation()
        {
            this.Notation = this.Reset(true, false);
            McUtility.ClearModifiedState();
        }

        public void InitializeRenderControl()
        {
            this.RenderCanvas = new RenderControl();
            this.RenderCanvas.Dock = DockStyle.Fill;
            this.RenderCanvas.Name = "renderControl";
            this.RenderCanvas.AllowDrop = true;
            base.Controls.Add(this.RenderCanvas);
            this.InitializeDirect2DAndDirectWrite();
            this._updateStopwatch = new Stopwatch();
            this._updateStopwatch.Start();
            System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer {
                Interval = 1,
                Enabled = true
            };
            this._updateTimer = timer;
            this._updateTimer.Start();
            this._updateTimer.Tick += delegate (object sender, EventArgs args) {
                foreach (Keys keys in this._keysPressedRepeatEventRecords.Keys.ToArray<Keys>())
                {
                    if (!keys.IsPressed())
                    {
                        this._keysPressedRepeatEventRecords[keys] = 0;
                    }
                }
                this._updateStopwatchElapsedMs = this._updateStopwatch.ElapsedMilliseconds - this._updateStopwatchLastMs;
                this._updateStopwatchLastMs = this._updateStopwatch.ElapsedMilliseconds;
                this._updateElapsedMsList.Add((int) this._updateStopwatchElapsedMs);
                if (this._updateElapsedMsList.Count > 20)
                {
                    this._updateElapsedMsList.RemoveAt(0);
                }
                this.AudioPlayerMainLoop(this._updateStopwatchElapsedMs);
                this.RenderControl_Update(this._updateStopwatchElapsedMs);
                this.RefreshCanvas();
            };
            this.RenderCanvas.MouseWheel += new MouseEventHandler(this.RenderCanvas_MouseWheel);
            this.RenderCanvas.MouseDown += new MouseEventHandler(this.RenderCanvas_MouseDown);
            this.RenderCanvas.PreviewKeyDown += new PreviewKeyDownEventHandler(this.RenderCanvas_PreviewKeyDown);
            this.RenderCanvas.DragEnter += new DragEventHandler(this.RenderCanvas_DragEnter);
            this.RenderCanvas.DragDrop += new DragEventHandler(this.RenderCanvas_DragDrop);
        }

        public bool LoadMusicalNotation()
        {
            this.PauseMusic();
            return ((this._openFileDialog.ShowDialog() == DialogResult.OK) && this.LoadMusicalNotation(this._openFileDialog.FileName));
        }

        public bool LoadMusicalNotation(string filename)
        {
            bool flag;
            this.PauseMusic();
            if ((this.Notation != null) && McUtility.IsModified)
            {
                switch (RadMessageBox.Show(this, "打开新乐谱前前是否保存现有内容？", "打开古剑奇谭网络版乐谱", MessageBoxButtons.YesNoCancel, RadMessageIcon.Exclamation))
                {
                    case DialogResult.Cancel:
                        return false;

                    case DialogResult.Yes:
                        this.SaveMusicalNotation();
                        break;
                }
            }
            this._isLoading = true;
            this.StopMusic();
            Lua lua = new Lua();
            string str = "";
            try
            {
                Encoding encoding = new UTF8Encoding(false);
                str = File.ReadAllText(filename, encoding);
                string str2 = str.DecompressString(null);
                if (!string.IsNullOrEmpty(str2))
                {
                    str = str2;
                }
                if (string.IsNullOrEmpty(str))
                {
                    this._isLoading = false;
                    return false;
                }
                lua.DoString(str, "chunk");
            }
            catch (Exception exception)
            {
                this._isLoading = false;
                RadMessageBox.Show(exception.Message.ToString());
                return false;
            }
            string str3 = null;
            string str4 = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            try
            {
                str3 = lua.GetString("Version");
            }
            catch (Exception)
            {
            }
            if (string.IsNullOrEmpty(str3))
            {
                base.Focus();
                this._loadingFilename = filename;
                this._loadingFilenameAlpha = 0xff;
                this.RenderControl_Paint(null, null);
                Thread.Sleep(10);
                flag = this.LoadMusicalNotation_1_0_0_0(lua, str);
                this._isLoading = false;
                if (flag)
                {
                    this.LastAccessedFilename = filename;
                    if (this.OnMusicCanvasFileLoaded != null)
                    {
                        this.OnMusicCanvasFileLoaded(this, new MusicCanvasIoEventArgs(filename));
                    }
                }
                return flag;
            }
            if (str3 != str4)
            {
                this._isLoading = false;
                RadMessageBox.Show($"乐谱文件的版本 {str3} 跟编辑器版本 {str4} 不匹配，加载失败。");
                return false;
            }
            base.Focus();
            this._loadingFilename = filename;
            this._loadingFilenameAlpha = 0xff;
            this.RenderControl_Paint(null, null);
            Thread.Sleep(10);
            flag = this.LoadMusicalNotation_1_1_0_0(lua, str);
            this._isLoading = false;
            if (flag)
            {
                this.LastAccessedFilename = filename;
                if (this.OnMusicCanvasFileLoaded != null)
                {
                    this.OnMusicCanvasFileLoaded(this, new MusicCanvasIoEventArgs(filename));
                }
            }
            return flag;
        }

        public bool LoadMusicalNotation_1_0_0_0(Lua lua, string contents)
        {
            int beatsPerMeasure;
            McNotePack.DurationTypes beatDurationType;
            LuaTable table = lua.GetTable("tNotation");
            if (table == null)
            {
                return false;
            }
            this.Reset(false, false);
            string input = contents.Substring(0, 0x200);
            Match match = new Regex(@"\s*NotationName\s*=\s*'(.+?)'").Match(input);
            if (match.Groups.Count >= 2)
            {
                this.Notation.Name = match.Groups[1].Value;
            }
            match = new Regex(@"\s*NotationAuthor\s*=\s*'(.+?)'").Match(input);
            if (match.Groups.Count >= 2)
            {
                this.Notation.Author = match.Groups[1].Value;
            }
            match = new Regex(@"\s*NotationTranslater\s*=\s*'(.+?)'").Match(input);
            if (match.Groups.Count >= 2)
            {
                this.Notation.Translater = match.Groups[1].Value;
            }
            match = new Regex(@"\s*NotationCreator\s*=\s*'(.+?)'").Match(input);
            if (match.Groups.Count >= 2)
            {
                this.Notation.Creator = match.Groups[1].Value;
            }
            this.Notation.Volume = 1f;
            if (table["Volume"] != null)
            {
                float volume = this.Notation.Volume;
                float.TryParse(table["Volume"].ToString(), out volume);
                this.Notation.Volume = volume;
            }
            this.Notation.BeatsPerMeasure = 4;
            if (table["BeatsPerMeasure"] != null)
            {
                beatsPerMeasure = this.Notation.BeatsPerMeasure;
                int.TryParse(table["BeatsPerMeasure"].ToString(), out beatsPerMeasure);
                this.Notation.BeatsPerMeasure = beatsPerMeasure;
            }
            this.Notation.BeatDurationType = McNotePack.DurationTypes.Quarter;
            if (table["BeatDurationType"] != null)
            {
                beatDurationType = this.Notation.BeatDurationType;
                Enum.TryParse<McNotePack.DurationTypes>(table["BeatDurationType"].ToString(), out beatDurationType);
                this.Notation.BeatDurationType = beatDurationType;
            }
            this.Notation.NumberedKeySignature = McNotation.NumberedKeySignatureTypes.C;
            if (table["NumberedKeySignature"] != null)
            {
                McNotation.NumberedKeySignatureTypes numberedKeySignature = this.Notation.NumberedKeySignature;
                if (Enum.TryParse<McNotation.NumberedKeySignatureTypes>(table["NumberedKeySignature"].ToString(), out numberedKeySignature))
                {
                    this.Notation.NumberedKeySignature = numberedKeySignature;
                }
            }
            int result = 0;
            if (table["MeasureColumnsCount"] != null)
            {
                int.TryParse(table["MeasureColumnsCount"].ToString(), out result);
            }
            LuaTable table2 = lua.GetTable("tNotation.MeasureColumns");
            if ((result <= 0) || (table2 == null))
            {
                return false;
            }
            for (int i = 0; i < result; i++)
            {
                LuaTable table3 = table2[i] as LuaTable;
                int? measureIndex = null;
                int index = this.Notation.InsertMeasureAligned(measureIndex);
                if (i != index)
                {
                    this.Reset(true, false);
                    return false;
                }
                if (table3 != null)
                {
                    int num6;
                    if (i == 0)
                    {
                        this.Notation.ResetMeasureBeatsPerMinuteToDefault();
                        num6 = 0;
                        while (num6 < 3)
                        {
                            McRegularTrack track = this.Notation.RegularTracks[num6];
                            track.ResetMeasureClefTypeToDefault();
                            track.ResetMeasureInstrumentTypeToDefault();
                            track.ResetMeasureKeySignatureToDefault();
                            track.ResetMeasureVolumeCurveToDefault();
                            track.ResetMeasureVolumeToDefault();
                            num6++;
                        }
                    }
                    McMeasure[] measuresAligned = this.Notation.RegularTracks.First<McRegularTrack>().GetMeasure(index).MeasuresAligned;
                    if (table3["RawBeatsPerMinute"] != null)
                    {
                        beatsPerMeasure = this.Notation.GetMeasureBeatsPerMinute(index);
                        int.TryParse(table3["RawBeatsPerMinute"].ToString(), out beatsPerMeasure);
                        this.Notation.SetMeasureBeatsPerMinute(index, beatsPerMeasure);
                    }
                    LuaTable table4 = table3["Measures"] as LuaTable;
                    if (table4 != null)
                    {
                        bool isRest;
                        McNotePack pack;
                        int num13;
                        int num14;
                        McPitch pitch;
                        SortedDictionary<int, List<LuaTable>> dictionary = new SortedDictionary<int, List<LuaTable>>();
                        for (num6 = 0; num6 < 2; num6++)
                        {
                            LuaTable table5 = table4[num6] as LuaTable;
                            McMeasure measure2 = measuresAligned[num6];
                            if ((table5 != null) && (measure2 != null))
                            {
                                int num9;
                                if (table5["RawClassicClefType"] != null)
                                {
                                    McMeasure.ClefTypes clefType = measure2.ClefType;
                                    Enum.TryParse<McMeasure.ClefTypes>(table5["RawClassicClefType"].ToString(), out clefType);
                                    if (clefType != McMeasure.ClefTypes.Invaild)
                                    {
                                        measure2.ClefType = clefType;
                                    }
                                }
                                if (table5["RawInstrumentType"] != null)
                                {
                                    McMeasure.InstrumentTypes instrumentType = measure2.InstrumentType;
                                    Enum.TryParse<McMeasure.InstrumentTypes>(table5["RawInstrumentType"].ToString(), out instrumentType);
                                    if (instrumentType != McMeasure.InstrumentTypes.Invaild)
                                    {
                                        measure2.InstrumentType = instrumentType;
                                    }
                                }
                                if (table5["RawClassicKeySignatureIndex"] != null)
                                {
                                    beatsPerMeasure = measure2.KeySignature;
                                    int.TryParse(table5["RawClassicKeySignatureIndex"].ToString(), out beatsPerMeasure);
                                    if (beatsPerMeasure != 0)
                                    {
                                        measure2.KeySignature = beatsPerMeasure;
                                    }
                                }
                                int num7 = 0;
                                if (table5["PercussionNotesCount"] != null)
                                {
                                    int.TryParse(table5["PercussionNotesCount"].ToString(), out num7);
                                }
                                LuaTable table6 = table5["PercussionNotes"] as LuaTable;
                                if (table6 != null)
                                {
                                    int key = 0;
                                    num9 = 0;
                                    while (num9 < num7)
                                    {
                                        LuaTable item = table6[num9] as LuaTable;
                                        if (item != null)
                                        {
                                            list = null;
                                            if (dictionary.ContainsKey(key))
                                            {
                                                list = dictionary[key];
                                            }
                                            else
                                            {
                                                list = new List<LuaTable>();
                                                dictionary.Add(key, list);
                                            }
                                            list.Add(item);
                                            float num10 = 1f;
                                            if (item["IsDotted"] != null)
                                            {
                                                isRest = false;
                                                bool.TryParse(item["IsDotted"].ToString(), out isRest);
                                                num10 = isRest ? 1.5f : 1f;
                                            }
                                            int num11 = 0x10;
                                            if (item["DurationType"] != null)
                                            {
                                                Enum.TryParse<McNotePack.DurationTypes>(item["DurationType"].ToString(), out beatDurationType);
                                                num11 = (int) beatDurationType;
                                            }
                                            key += (int) Math.Round((double) (num11 * num10));
                                        }
                                        num9++;
                                    }
                                }
                                int num12 = 0;
                                if (table5["NotesCount"] != null)
                                {
                                    int.TryParse(table5["NotesCount"].ToString(), out num12);
                                }
                                LuaTable table8 = table5["Notes"] as LuaTable;
                                if (table8 != null)
                                {
                                    for (num9 = 0; num9 < num12; num9++)
                                    {
                                        LuaTable table9 = table8[num9] as LuaTable;
                                        if (table9 != null)
                                        {
                                            measureIndex = null;
                                            pack = measure2.InsertNotePack(measureIndex, null);
                                            if (table9["RawPitchSemitoneIndex"] != null)
                                            {
                                                num13 = 0;
                                                int.TryParse(table9["RawPitchSemitoneIndex"].ToString(), out num13);
                                                if (num13 >= McPitch.PitchMin)
                                                {
                                                    num13 += this.Notation.NumberedKeySignature;
                                                    if (num13 <= McPitch.PitchMax)
                                                    {
                                                        num14 = 0;
                                                        num13 = McPitch.ConvertToNaturalPitch(num13, McPitch.AlterantTypes.Sharp, out num14);
                                                        if (pack.MarkPitch(num13, McPitch.PitchTypes.Enabled))
                                                        {
                                                            pitch = pack.GetPitch(num13);
                                                            if ((pitch != null) && (table9["IsIgnoreCksAlter"] != null))
                                                            {
                                                                isRest = false;
                                                                bool.TryParse(table9["IsIgnoreCksAlter"].ToString(), out isRest);
                                                                pitch.AlterantType = isRest ? McPitch.AlterantTypes.Natural : McPitch.AlterantTypes.NoControl;
                                                                if (pitch.AlterantType != McPitch.AlterantTypes.Natural)
                                                                {
                                                                    if (num14 < 0)
                                                                    {
                                                                        pitch.AlterantType = McPitch.AlterantTypes.Sharp;
                                                                    }
                                                                    else if (num14 > 0)
                                                                    {
                                                                        pitch.AlterantType = McPitch.AlterantTypes.Flat;
                                                                    }
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                            if (table9["ExtraNoteRawPitchSemitoneIndex"] != null)
                                            {
                                                num13 = 0;
                                                int.TryParse(table9["ExtraNoteRawPitchSemitoneIndex"].ToString(), out num13);
                                                if (num13 >= McPitch.PitchMin)
                                                {
                                                    num13 += this.Notation.NumberedKeySignature;
                                                    if (num13 <= McPitch.PitchMax)
                                                    {
                                                        num14 = 0;
                                                        num13 = McPitch.ConvertToNaturalPitch(num13, McPitch.AlterantTypes.Sharp, out num14);
                                                        if (pack.MarkPitch(num13, McPitch.PitchTypes.Enabled))
                                                        {
                                                            pitch = pack.GetPitch(num13);
                                                            if ((pitch != null) && (table9["IsIgnoreCksAlter"] != null))
                                                            {
                                                                isRest = false;
                                                                bool.TryParse(table9["IsIgnoreCksAlter"].ToString(), out isRest);
                                                                pitch.AlterantType = isRest ? McPitch.AlterantTypes.Natural : McPitch.AlterantTypes.NoControl;
                                                                if (pitch.AlterantType != McPitch.AlterantTypes.Natural)
                                                                {
                                                                    if (num14 < 0)
                                                                    {
                                                                        pitch.AlterantType = McPitch.AlterantTypes.Sharp;
                                                                    }
                                                                    else if (num14 > 0)
                                                                    {
                                                                        pitch.AlterantType = McPitch.AlterantTypes.Flat;
                                                                    }
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                            pack.IsRest = false;
                                            if (table9["IsRest"] != null)
                                            {
                                                isRest = pack.IsRest;
                                                bool.TryParse(table9["IsRest"].ToString(), out isRest);
                                                pack.IsRest = isRest;
                                            }
                                            pack.IsDotted = false;
                                            if (table9["IsDotted"] != null)
                                            {
                                                isRest = pack.IsDotted;
                                                bool.TryParse(table9["IsDotted"].ToString(), out isRest);
                                                pack.IsDotted = isRest;
                                            }
                                            pack.DurationType = McNotePack.DurationTypes.Quarter;
                                            if (table9["DurationType"] != null)
                                            {
                                                beatDurationType = pack.DurationType;
                                                Enum.TryParse<McNotePack.DurationTypes>(table9["DurationType"].ToString(), out beatDurationType);
                                                pack.DurationType = beatDurationType;
                                            }
                                            pack.TieType = McNotePack.TieTypes.None;
                                            if ((table9["IsTieStart"] != null) || (table9["IsTieStop"] != null))
                                            {
                                                bool flag2 = false;
                                                bool flag3 = false;
                                                if (table9["IsTieStart"] != null)
                                                {
                                                    bool.TryParse(table9["IsTieStart"].ToString(), out flag2);
                                                }
                                                if (table9["IsTieStop"] != null)
                                                {
                                                    bool.TryParse(table9["IsTieStop"].ToString(), out flag3);
                                                }
                                                if (flag2 && flag3)
                                                {
                                                    pack.TieType = McNotePack.TieTypes.Both;
                                                }
                                                else if (flag2)
                                                {
                                                    pack.TieType = McNotePack.TieTypes.Start;
                                                }
                                                else if (flag3)
                                                {
                                                    pack.TieType = McNotePack.TieTypes.End;
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        McMeasure measure3 = measuresAligned[2];
                        if (dictionary.Count > 0)
                        {
                            measure3.InstrumentType = McMeasure.InstrumentTypes.Misc;
                            measure3.ClefType = McMeasure.ClefTypes.L4F;
                            foreach (List<LuaTable> list in dictionary.Values.ToArray<List<LuaTable>>())
                            {
                                if (list.Count > 0)
                                {
                                    measureIndex = null;
                                    pack = measure3.InsertNotePack(measureIndex, null);
                                    foreach (LuaTable table7 in list)
                                    {
                                        if (table7["RawPitchSemitoneIndex"] != null)
                                        {
                                            num13 = 0;
                                            int.TryParse(table7["RawPitchSemitoneIndex"].ToString(), out num13);
                                            if (num13 >= McPitch.PitchMin)
                                            {
                                                num14 = 0;
                                                num13 = McPitch.ConvertToNaturalPitch(num13, McPitch.AlterantTypes.Sharp, out num14);
                                                if (pack.MarkPitch(num13, McPitch.PitchTypes.Enabled))
                                                {
                                                    pitch = pack.GetPitch(num13);
                                                    if (pitch != null)
                                                    {
                                                        pitch.AlterantType = McPitch.AlterantTypes.NoControl;
                                                        if (num14 < 0)
                                                        {
                                                            pitch.AlterantType = McPitch.AlterantTypes.Sharp;
                                                        }
                                                        else if (num14 > 0)
                                                        {
                                                            pitch.AlterantType = McPitch.AlterantTypes.Flat;
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                        if (table7["IsRest"] != null)
                                        {
                                            isRest = pack.IsRest;
                                            bool.TryParse(table7["IsRest"].ToString(), out isRest);
                                            pack.IsRest = pack.IsRest && isRest;
                                        }
                                        if (table7["IsDotted"] != null)
                                        {
                                            isRest = pack.IsDotted;
                                            bool.TryParse(table7["IsDotted"].ToString(), out isRest);
                                            pack.IsDotted = pack.IsDotted || isRest;
                                        }
                                        pack.DurationType = McNotePack.DurationTypes.Quarter;
                                        if (table7["DurationType"] != null)
                                        {
                                            beatDurationType = pack.DurationType;
                                            Enum.TryParse<McNotePack.DurationTypes>(table7["DurationType"].ToString(), out beatDurationType);
                                            pack.DurationType = beatDurationType;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            this.Notation.TemporaryInfo.ReorganizeAllDurationStamps();
            McUtility.ClearModifiedState();
            this._scrollX = 0;
            this._scrollXSmooth = 0f;
            return true;
        }

        public bool LoadMusicalNotation_1_1_0_0(Lua lua, string contents)
        {
            McNotePack.DurationTypes beatDurationType;
            int num5;
            LuaTable table3;
            McRegularTrack track;
            int num6;
            int? nullable;
            LuaTable table = lua.GetTable("Notation");
            if (table == null)
            {
                return false;
            }
            this.Reset(false, false);
            string input = contents.Substring(0, 0x200);
            Match match = new Regex(@"\s*NotationName\s*=\s*'(.+?)'").Match(input);
            if (match.Groups.Count >= 2)
            {
                this.Notation.Name = match.Groups[1].Value;
            }
            match = new Regex(@"\s*NotationAuthor\s*=\s*'(.+?)'").Match(input);
            if (match.Groups.Count >= 2)
            {
                this.Notation.Author = match.Groups[1].Value;
            }
            match = new Regex(@"\s*NotationTranslater\s*=\s*'(.+?)'").Match(input);
            if (match.Groups.Count >= 2)
            {
                this.Notation.Translater = match.Groups[1].Value;
            }
            match = new Regex(@"\s*NotationCreator\s*=\s*'(.+?)'").Match(input);
            if (match.Groups.Count >= 2)
            {
                this.Notation.Creator = match.Groups[1].Value;
            }
            this.Notation.Volume = 1f;
            if (table["Volume"] != null)
            {
                float volume = this.Notation.Volume;
                float.TryParse(table["Volume"].ToString(), out volume);
                this.Notation.Volume = volume;
            }
            this.Notation.BeatsPerMeasure = 4;
            if (table["BeatsPerMeasure"] != null)
            {
                int beatsPerMeasure = this.Notation.BeatsPerMeasure;
                int.TryParse(table["BeatsPerMeasure"].ToString(), out beatsPerMeasure);
                this.Notation.BeatsPerMeasure = beatsPerMeasure;
            }
            this.Notation.BeatDurationType = McNotePack.DurationTypes.Quarter;
            if (table["BeatDurationType"] != null)
            {
                beatDurationType = this.Notation.BeatDurationType;
                Enum.TryParse<McNotePack.DurationTypes>(table["BeatDurationType"].ToString(), out beatDurationType);
                this.Notation.BeatDurationType = beatDurationType;
            }
            this.Notation.NumberedKeySignature = McNotation.NumberedKeySignatureTypes.C;
            if (table["NumberedKeySignature"] != null)
            {
                McNotation.NumberedKeySignatureTypes numberedKeySignature = this.Notation.NumberedKeySignature;
                Enum.TryParse<McNotation.NumberedKeySignatureTypes>(table["NumberedKeySignature"].ToString(), out numberedKeySignature);
                this.Notation.NumberedKeySignature = numberedKeySignature;
            }
            int result = 0;
            if (table["MeasureAlignedCount"] != null)
            {
                int.TryParse(table["MeasureAlignedCount"].ToString(), out result);
            }
            LuaTable table2 = lua.GetTable("Notation.RegularTracks");
            if ((result <= 0) || (table2 == null))
            {
                return false;
            }
            int index = 0;
            while (index < result)
            {
                nullable = null;
                this.Notation.InsertMeasureAligned(nullable);
                index++;
            }
            for (num5 = 0; num5 < 3; num5++)
            {
                table3 = table2[num5] as LuaTable;
                if (table3 != null)
                {
                    track = this.Notation.RegularTracks[num5];
                    num6 = 0;
                    while (num6 < result)
                    {
                        LuaTable table4 = table3[num6] as LuaTable;
                        if (table4 != null)
                        {
                            int notePackMaxAmount = 0;
                            if (table4["NotePackCount"] != null)
                            {
                                int.TryParse(table4["NotePackCount"].ToString(), out notePackMaxAmount);
                            }
                            if (notePackMaxAmount > 0)
                            {
                                if (notePackMaxAmount > McMeasure.NotePackMaxAmount)
                                {
                                    RadMessageBox.Show($"第 {num5 + 1} 轨第 {num6 + 1} 小节的音符数量超出了所允许的上限（{notePackMaxAmount}/{McMeasure.NotePackMaxAmount}），超出部分将自动截去。", "乐谱加载提示");
                                    notePackMaxAmount = McMeasure.NotePackMaxAmount;
                                }
                                McMeasure measure = track.GetMeasure(num6);
                                for (int i = 0; i < notePackMaxAmount; i++)
                                {
                                    LuaTable table5 = table4[i] as LuaTable;
                                    if (table5 != null)
                                    {
                                        bool isRest;
                                        nullable = null;
                                        McNotePack pack = measure.InsertNotePack(nullable, null);
                                        int num9 = 0;
                                        if (table5["ClassicPitchSignCount"] != null)
                                        {
                                            int.TryParse(table5["ClassicPitchSignCount"].ToString(), out num9);
                                        }
                                        if (num9 > 0)
                                        {
                                            LuaTable table6 = table5["ClassicPitchSign"] as LuaTable;
                                            foreach (object obj2 in table6.Keys)
                                            {
                                                bool flag;
                                                int pitch = (int) Math.Round(double.Parse(obj2.ToString()));
                                                int pitchOffset = 0;
                                                pitch = McPitch.ConvertToNaturalPitch(pitch, McPitch.AlterantTypes.Sharp, out pitchOffset);
                                                LuaTable table7 = table6[obj2] as LuaTable;
                                                McPitch.AlterantTypes noControl = McPitch.AlterantTypes.NoControl;
                                                if (table7 != null)
                                                {
                                                    object obj3 = table7["RawAlterantType"];
                                                    object obj4 = table7["AlterantType"];
                                                    if (obj3 != null)
                                                    {
                                                        Enum.TryParse<McPitch.AlterantTypes>(obj3.ToString(), out noControl);
                                                    }
                                                    else if (obj4 != null)
                                                    {
                                                        Enum.TryParse<McPitch.AlterantTypes>(obj4.ToString(), out noControl);
                                                    }
                                                    else
                                                    {
                                                        flag = false;
                                                        bool.TryParse(table7["HasNaturalSign"].ToString(), out flag);
                                                        noControl = flag ? McPitch.AlterantTypes.Natural : McPitch.AlterantTypes.NoControl;
                                                        if (noControl != McPitch.AlterantTypes.Natural)
                                                        {
                                                            if (pitchOffset < 0)
                                                            {
                                                                noControl = McPitch.AlterantTypes.Sharp;
                                                            }
                                                            else if (pitchOffset > 0)
                                                            {
                                                                noControl = McPitch.AlterantTypes.Flat;
                                                            }
                                                        }
                                                    }
                                                }
                                                else
                                                {
                                                    flag = false;
                                                    bool.TryParse(table6[obj2].ToString(), out flag);
                                                    noControl = flag ? McPitch.AlterantTypes.Natural : McPitch.AlterantTypes.NoControl;
                                                    if (noControl != McPitch.AlterantTypes.Natural)
                                                    {
                                                        if (pitchOffset < 0)
                                                        {
                                                            noControl = McPitch.AlterantTypes.Sharp;
                                                        }
                                                        else if (pitchOffset > 0)
                                                        {
                                                            noControl = McPitch.AlterantTypes.Flat;
                                                        }
                                                    }
                                                }
                                                if (pack.MarkPitch(pitch, McPitch.PitchTypes.Enabled))
                                                {
                                                    pack.GetPitch(pitch).AlterantType = noControl;
                                                }
                                            }
                                        }
                                        if (table5["IsRest"] != null)
                                        {
                                            isRest = pack.IsRest;
                                            bool.TryParse(table5["IsRest"].ToString(), out isRest);
                                            pack.IsRest = isRest;
                                        }
                                        if (table5["IsDotted"] != null)
                                        {
                                            isRest = pack.IsDotted;
                                            bool.TryParse(table5["IsDotted"].ToString(), out isRest);
                                            pack.IsDotted = isRest;
                                        }
                                        if (table5["Triplet"] != null)
                                        {
                                            isRest = pack.Triplet;
                                            bool.TryParse(table5["Triplet"].ToString(), out isRest);
                                            pack.Triplet = isRest;
                                        }
                                        if (table5["DurationType"] != null)
                                        {
                                            beatDurationType = pack.DurationType;
                                            Enum.TryParse<McNotePack.DurationTypes>(table5["DurationType"].ToString(), out beatDurationType);
                                            pack.DurationType = beatDurationType;
                                        }
                                        if (table5["ArpeggioMode"] != null)
                                        {
                                            Arpeggio.ArpeggioTypes arpeggioMode = pack.ArpeggioMode;
                                            Enum.TryParse<Arpeggio.ArpeggioTypes>(table5["ArpeggioMode"].ToString(), out arpeggioMode);
                                            pack.ArpeggioMode = arpeggioMode;
                                        }
                                        if (table5["TieType"] != null)
                                        {
                                            McNotePack.TieTypes tieType = pack.TieType;
                                            Enum.TryParse<McNotePack.TieTypes>(table5["TieType"].ToString(), out tieType);
                                            pack.TieType = tieType;
                                        }
                                    }
                                }
                            }
                        }
                        num6++;
                    }
                }
            }
            for (num5 = 0; num5 < 3; num5++)
            {
                table3 = table2[num5] as LuaTable;
                if (table3 != null)
                {
                    double num13;
                    track = this.Notation.RegularTracks[num5];
                    track.ResetMeasureKeySignatureToDefault();
                    LuaTable table8 = table3["MeasureKeySignatureMap"] as LuaTable;
                    if (table8 != null)
                    {
                        foreach (LuaTable table9 in table8.Values)
                        {
                            num6 = (int) Math.Round(double.Parse(table9[1].ToString()));
                            int num12 = 0;
                            if (int.TryParse(table9[2].ToString(), out num12))
                            {
                                track.SetMeasureKeySignature(num6, num12);
                            }
                        }
                    }
                    track.ResetMeasureClefTypeToDefault();
                    LuaTable table10 = table3["MeasureClefTypeMap"] as LuaTable;
                    if (table10 != null)
                    {
                        foreach (LuaTable table9 in table10.Values)
                        {
                            num6 = (int) Math.Round(double.Parse(table9[1].ToString()));
                            McMeasure.ClefTypes types6 = McMeasure.ClefTypes.L2G;
                            if (Enum.TryParse<McMeasure.ClefTypes>(table9[2].ToString(), out types6))
                            {
                                track.SetMeasureClefType(num6, types6);
                            }
                        }
                    }
                    track.ResetMeasureInstrumentTypeToDefault();
                    LuaTable table11 = table3["MeasureInstrumentTypeMap"] as LuaTable;
                    if (table11 != null)
                    {
                        foreach (LuaTable table9 in table11.Values)
                        {
                            num6 = (int) Math.Round(double.Parse(table9[1].ToString()));
                            McMeasure.InstrumentTypes piano = McMeasure.InstrumentTypes.Piano;
                            if (Enum.TryParse<McMeasure.InstrumentTypes>(table9[2].ToString(), out piano))
                            {
                                track.SetMeasureInstrumentType(num6, piano);
                            }
                        }
                    }
                    track.ResetMeasureVolumeCurveToDefault();
                    LuaTable table12 = table3["MeasureVolumeCurveMap"] as LuaTable;
                    if (table12 != null)
                    {
                        foreach (LuaTable table9 in table12.Values)
                        {
                            num6 = (int) Math.Round(double.Parse(table9[1].ToString()));
                            LuaTable table13 = table9[2] as LuaTable;
                            if (table13 != null)
                            {
                                VolumeCurve volumeCurve = new VolumeCurve();
                                for (index = 0; index < VolumeCurve.DefaultVolumePoint.Length; index++)
                                {
                                    num13 = VolumeCurve.DefaultVolumePoint[index];
                                    if (double.TryParse(table13[index + 1].ToString(), out num13))
                                    {
                                        volumeCurve.SetCurvedVolume(index, ((float) num13).Clamp(0f, 1f));
                                    }
                                }
                                track.SetMeasureVolumeCurve(num6, volumeCurve);
                            }
                        }
                    }
                    track.ResetMeasureVolumeToDefault();
                    LuaTable table14 = table3["MeasureVolumeMap"] as LuaTable;
                    if (table14 != null)
                    {
                        foreach (LuaTable table9 in table14.Values)
                        {
                            num6 = (int) Math.Round(double.Parse(table9[1].ToString()));
                            num13 = 1.0;
                            if (double.TryParse(table9[2].ToString(), out num13))
                            {
                                track.SetMeasureVolume(num6, ((float) num13).Clamp(0f, 1f));
                            }
                        }
                    }
                }
            }
            this.Notation.ResetMeasureBeatsPerMinuteToDefault();
            LuaTable table15 = table["MeasureBeatsPerMinuteMap"] as LuaTable;
            if (table15 != null)
            {
                foreach (LuaTable table9 in table15.Values)
                {
                    num6 = (int) Math.Round(double.Parse(table9[1].ToString()));
                    int num14 = 80;
                    if (int.TryParse(table9[2].ToString(), out num14))
                    {
                        this.Notation.SetMeasureBeatsPerMinute(num6, num14);
                    }
                }
            }
            this.Notation.TemporaryInfo.ReorganizeAllDurationStamps();
            McUtility.ClearModifiedState();
            this._scrollX = 0;
            this._scrollXSmooth = 0f;
            return true;
        }

        public void MouseCircleProgress_Callback(MouseButtons? mouseButton, Keys? key, MouseCircleProgressManager.ExitTypes exitType, string hintText)
        {
            if (exitType != MouseCircleProgressManager.ExitTypes.Finished)
            {
                if ((exitType != MouseCircleProgressManager.ExitTypes.CancelOnKeyReleased) && (exitType == MouseCircleProgressManager.ExitTypes.CancelOnMouseMoved))
                {
                }
            }
            else
            {
                Point point;
                if (!EnableDelaylessCommand)
                {
                    MouseDragareaManager.Fade(MouseDragareaManager.ExitTypes.Cancel);
                }
                if (!mouseButton.HasValue)
                {
                    if (key.HasValue)
                    {
                        switch (key.Value)
                        {
                            case Keys.Delete:
                                this.DeleteSelectionNotePacks();
                                break;

                            case Keys.D1:
                            {
                                point = base.PointToClient(Control.MousePosition);
                                bool flag2 = ((this.Notation.DrawingWidth - this._scrollX) - ((point.X - McNotation.Margin.Left) - McRegularTrack.Margin.Left)) < 0;
                                int num7 = -1;
                                if (!flag2)
                                {
                                    if (McHoverInfo.HoveringMeasure != null)
                                    {
                                        num7 = this.Notation.InsertMeasureAligned(new int?(McHoverInfo.HoveringMeasure.Index));
                                    }
                                }
                                else
                                {
                                    num7 = this.Notation.InsertMeasureAligned(null);
                                }
                                if (num7 >= 0)
                                {
                                    McUtility.AppendDeferredRedrawingMeasures(this._drawingMeasures);
                                }
                                return;
                            }
                            case Keys.D2:
                                if (McHoverInfo.HoveringMeasure != null)
                                {
                                    this.Notation.RemoveMeasureAligned(McHoverInfo.HoveringMeasure.Index);
                                    McUtility.AppendDeferredRedrawingMeasures(this._drawingMeasures);
                                }
                                break;

                            case Keys.D3:
                                if (McHoverInfo.HoveringMeasure == null)
                                {
                                    if (this.Notation.MeasureCountAligned > 0)
                                    {
                                        measure2 = this.Notation.RegularTracks.First<McRegularTrack>().GetMeasure(0);
                                        this.RenderControl_Update(0x10L);
                                        this.OpenPropertyPanel(measure2);
                                    }
                                    break;
                                }
                                this.RenderControl_Update(0x10L);
                                this.OpenPropertyPanel(McHoverInfo.HoveringMeasure);
                                break;
                        }
                    }
                }
                else
                {
                    McNotePack pack3;
                    McNotePack[] notePacks;
                    McMeasure hoveringMeasure = McHoverInfo.HoveringMeasure;
                    McNotePack hoveringNotePack = McHoverInfo.HoveringNotePack;
                    int hoveringInsertPitchValue = McHoverInfo.HoveringInsertPitchValue;
                    switch (mouseButton.Value)
                    {
                        case MouseButtons.Left:
                            if (hoveringInsertPitchValue > 0)
                            {
                                if (hoveringNotePack != null)
                                {
                                    if (hoveringNotePack.GetPitchType(hoveringInsertPitchValue) == McPitch.PitchTypes.Enabled)
                                    {
                                        if (hoveringNotePack.ValidPitchCount > 1)
                                        {
                                            hoveringNotePack.MarkPitch(hoveringInsertPitchValue, McPitch.PitchTypes.Disabled);
                                        }
                                    }
                                    else
                                    {
                                        hoveringNotePack.MarkPitch(hoveringInsertPitchValue, McPitch.PitchTypes.Enabled);
                                        if (EnablePlaySoundWhenInsert)
                                        {
                                            this.PlaySound(hoveringMeasure.InstrumentType, hoveringNotePack.GetPitch(hoveringInsertPitchValue).AlterantValue, hoveringNotePack.Volume, 0L, 0.03f);
                                        }
                                    }
                                }
                                else
                                {
                                    McNotePack pack2;
                                    float num5;
                                    int num6;
                                    point = base.PointToClient(Control.MousePosition);
                                    int hoveringInsertNotePackIndex = McHoverInfo.HoveringInsertNotePackIndex;
                                    bool flag = hoveringInsertNotePackIndex == McHoverInfo.HoveringMeasureInsertNotePackIndexMax;
                                    int num3 = 0;
                                    int num4 = 20;
                                    if (flag && (hoveringMeasure.NotePacks.Length > 0))
                                    {
                                        pack2 = hoveringMeasure.NotePacks.Last<McNotePack>();
                                        num5 = ((hoveringMeasure.TemporaryInfo.RelativeX + McNotation.Margin.Left) + McRegularTrack.Margin.Left) - this._scrollX;
                                        num6 = (int) (((num5 + pack2.TemporaryInfo.RelativeX) + num4) + 20f);
                                        num3 = (num6 - point.X).Clamp(-18, 0x12);
                                    }
                                    pack3 = hoveringMeasure.InsertNotePack(new int?(hoveringInsertNotePackIndex), null);
                                    pack3.MarkPitch(hoveringInsertPitchValue, McPitch.PitchTypes.Enabled);
                                    if (hoveringInsertNotePackIndex > 0)
                                    {
                                        notePacks = hoveringMeasure.NotePacks;
                                        if ((notePacks.Length > 0) && (notePacks.Length > (hoveringInsertNotePackIndex - 1)))
                                        {
                                            pack3.DurationType = hoveringMeasure.NotePacks[hoveringInsertNotePackIndex - 1].DurationType;
                                        }
                                    }
                                    hoveringMeasure.ReorganizeDurationStamps();
                                    foreach (McMeasure measure2 in hoveringMeasure.MeasuresAligned)
                                    {
                                        if (measure2 == hoveringMeasure)
                                        {
                                            McUtility.MarkModified(measure2);
                                        }
                                        else
                                        {
                                            McUtility.AppendRedrawingMeasure(measure2);
                                        }
                                    }
                                    if (flag && (hoveringMeasure.NotePacks.Length > 0))
                                    {
                                        hoveringMeasure.RedrawBitmapCache();
                                        pack2 = hoveringMeasure.NotePacks.Last<McNotePack>();
                                        num5 = ((hoveringMeasure.TemporaryInfo.RelativeX + McNotation.Margin.Left) + McRegularTrack.Margin.Left) - this._scrollX;
                                        num6 = (int) (((num5 + pack2.TemporaryInfo.RelativeX) + num4) + 20f);
                                        if (point.X < (base.Width * 0.45))
                                        {
                                            SetCursorPos(Control.MousePosition.X + ((num6 - point.X) - num3), Control.MousePosition.Y);
                                        }
                                        else
                                        {
                                            this._scrollX += (num6 - point.X) - num3;
                                            this._scrollXSmooth = this._scrollX;
                                        }
                                    }
                                    if (EnablePlaySoundWhenInsert)
                                    {
                                        this.PlaySound(hoveringMeasure.InstrumentType, pack3.GetPitch(hoveringInsertPitchValue).AlterantValue, pack3.Volume, 0L, 0.03f);
                                    }
                                }
                            }
                            break;

                        case MouseButtons.Right:
                            if (hoveringNotePack != null)
                            {
                                hoveringMeasure.RemoveNotePack(hoveringNotePack);
                                McHoverInfo.HoveringNotePack = null;
                                hoveringMeasure.ReorganizeDurationStamps();
                                foreach (McMeasure measure2 in hoveringMeasure.MeasuresAligned)
                                {
                                    if (measure2 == hoveringMeasure)
                                    {
                                        McUtility.MarkModified(measure2);
                                    }
                                    else
                                    {
                                        McUtility.AppendRedrawingMeasure(measure2);
                                    }
                                }
                            }
                            break;

                        case MouseButtons.Middle:
                            if (hoveringNotePack != null)
                            {
                                hoveringNotePack.IsRest = !hoveringNotePack.IsRest;
                                if (!hoveringNotePack.IsRest && EnablePlaySoundWhenInsert)
                                {
                                    foreach (McPitch pitch in hoveringNotePack.ValidPitchArray)
                                    {
                                        this.PlaySound(hoveringMeasure.InstrumentType, pitch.AlterantValue, hoveringNotePack.Volume, 0L, 0.03f);
                                    }
                                }
                            }
                            else
                            {
                                pack3 = hoveringMeasure.InsertNotePack(new int?(McHoverInfo.HoveringInsertNotePackIndex), null);
                                pack3.MarkPitch(hoveringInsertPitchValue, McPitch.PitchTypes.Enabled);
                                pack3.IsRest = true;
                                if (McHoverInfo.HoveringInsertNotePackIndex > 0)
                                {
                                    notePacks = hoveringMeasure.NotePacks;
                                    if ((notePacks.Length > 0) && (notePacks.Length > (McHoverInfo.HoveringInsertNotePackIndex - 1)))
                                    {
                                        pack3.DurationType = hoveringMeasure.NotePacks[McHoverInfo.HoveringInsertNotePackIndex - 1].DurationType;
                                    }
                                }
                                hoveringMeasure.ReorganizeDurationStamps();
                                foreach (McMeasure measure2 in hoveringMeasure.MeasuresAligned)
                                {
                                    if (measure2 == hoveringMeasure)
                                    {
                                        McUtility.MarkModified(measure2);
                                    }
                                    else
                                    {
                                        McUtility.AppendRedrawingMeasure(measure2);
                                    }
                                }
                            }
                            break;
                    }
                }
            }
        }

        public void MouseDragarea_Callback(RawRectangleF mouseDragarea, RawRectangleF validCanvasDragarea, Dictionary<int, List<Dictionary<McNotePack, bool>>> pickedNotePacks, MouseDragareaManager.ExitTypes exitType)
        {
        }

        public void NewMusicalNotation()
        {
            this.Reset(true, true);
            base.Focus();
            this._loadingFilename = "";
            this.RenderControl_Paint(null, null);
            Thread.Sleep(10);
            this.LastAccessedFilename = "";
            this.MouseCircleProgress_Callback(null, 0x33, MouseCircleProgressManager.ExitTypes.Finished, "");
        }

        public void OpenPropertyPanel(McMeasure measure)
        {
            if (measure != null)
            {
                this.PropertyPanel.NotationName = this.Notation.Name;
                this.PropertyPanel.NotationAuthor = this.Notation.Author;
                this.PropertyPanel.NotationTranslater = this.Notation.Translater;
                this.PropertyPanel.NotationBoxCreator = this.Notation.Creator;
                this.PropertyPanel.BeatDurationType = this.Notation.BeatDurationType;
                this.PropertyPanel.BeatsPerMeasure = this.Notation.BeatsPerMeasure;
                this.PropertyPanel.NumberedKeySignature = this.Notation.NumberedKeySignature;
                this.PropertyPanel.InstrumentType = measure.InstrumentType;
                this.PropertyPanel.ClefType = measure.ClefType;
                this.PropertyPanel.KeySignature = measure.KeySignature;
                this.PropertyPanel.BeatsPerMinute = measure.BeatsPerMinute;
                this.PropertyPanel.VolumeCurve = measure.VolumeCurve;
                this.PropertyPanel.MeasureVolume = measure.Volume;
                Rectangle workingArea = Screen.GetWorkingArea(this);
                this.PropertyPanel.Location = new Point((Control.MousePosition.X - 0x18).Clamp(workingArea.Left, workingArea.Right - this.PropertyPanel.Width), (Control.MousePosition.Y - 0x18).Clamp(workingArea.Top, workingArea.Bottom - this.PropertyPanel.Height));
                if (this.PropertyPanel.ShowDialog(this) != DialogResult.Cancel)
                {
                    this.Notation.Name = this.PropertyPanel.NotationName;
                    this.Notation.Author = this.PropertyPanel.NotationAuthor;
                    this.Notation.Translater = this.PropertyPanel.NotationTranslater;
                    this.Notation.Creator = this.PropertyPanel.NotationBoxCreator;
                    this.Notation.BeatDurationType = this.PropertyPanel.BeatDurationType;
                    this.Notation.BeatsPerMeasure = this.PropertyPanel.BeatsPerMeasure;
                    this.Notation.NumberedKeySignature = this.PropertyPanel.NumberedKeySignature;
                    measure.InstrumentType = this.PropertyPanel.InstrumentType;
                    measure.ClefType = this.PropertyPanel.ClefType;
                    measure.KeySignature = this.PropertyPanel.KeySignature;
                    measure.BeatsPerMinute = this.PropertyPanel.BeatsPerMinute;
                    measure.VolumeCurve = this.PropertyPanel.VolumeCurve;
                    measure.Volume = this.PropertyPanel.MeasureVolume;
                    measure.ReorganizeDurationStamps();
                    measure.RedrawBitmapCache();
                    McUtility.AppendDeferredRedrawingMeasures(this._drawingMeasures);
                }
            }
        }

        private void PasteSelectionToMeasure()
        {
            if (((McHoverInfo.HoveringMeasure != null) && (McHoverInfo.HoveringInsertNotePackIndex >= 0)) && (McHoverInfo.HoveringNotePack == null))
            {
                List<List<Dictionary<McNotePack, bool>>> validPickedNotePacks = this.GetValidPickedNotePacks();
                if (validPickedNotePacks != null)
                {
                    Dictionary<McNotePack, bool> dictionary = validPickedNotePacks[0][0];
                    if (dictionary != null)
                    {
                        bool flag = false;
                        int hoveringInsertNotePackIndex = McHoverInfo.HoveringInsertNotePackIndex;
                        foreach (McNotePack pack in dictionary.Keys)
                        {
                            if (dictionary[pack])
                            {
                                McNotePack pack2 = McHoverInfo.HoveringMeasure.InsertNotePack(new int?(hoveringInsertNotePackIndex), pack);
                                hoveringInsertNotePackIndex++;
                                flag = true;
                            }
                        }
                        if (flag)
                        {
                            McHoverInfo.HoveringMeasure.ReorganizeDurationStamps();
                            foreach (McMeasure measure in McHoverInfo.HoveringMeasure.MeasuresAligned)
                            {
                                if (measure == McHoverInfo.HoveringMeasure)
                                {
                                    McUtility.MarkModified(measure);
                                }
                                else
                                {
                                    McUtility.AppendRedrawingMeasure(measure);
                                }
                            }
                            this.ShowCommonMessage("粘贴音符到小节完成");
                        }
                    }
                }
            }
        }

        private void PasteSelectionToNotation()
        {
            if (McHoverInfo.HoveringMeasure != null)
            {
                List<List<Dictionary<McNotePack, bool>>> validPickedNotePacks = this.GetValidPickedNotePacks();
                if (validPickedNotePacks != null)
                {
                    int num = (from trackNotePacks in validPickedNotePacks select trackNotePacks.Count).Concat<int>(new int[1]).Max();
                    if (num > 0)
                    {
                        int num3;
                        int index = McHoverInfo.HoveringMeasure.Index;
                        for (num3 = 0; num3 < num; num3++)
                        {
                            this.Notation.InsertMeasureAligned(new int?(index));
                        }
                        int num4 = McHoverInfo.HoveringMeasure.ParentRegularTrack.Index;
                        for (num3 = 0; num3 < 3; num3++)
                        {
                            int num5 = num3 + num4;
                            if ((num5 >= 3) || (num3 >= validPickedNotePacks.Count))
                            {
                                break;
                            }
                            McRegularTrack track = this.Notation.RegularTracks[num5];
                            for (int i = 0; i < num; i++)
                            {
                                bool flag = false;
                                McMeasure measure = track.GetMeasure(index + i);
                                Dictionary<McNotePack, bool> dictionary = validPickedNotePacks[num3][i];
                                foreach (McNotePack pack in dictionary.Keys)
                                {
                                    if (dictionary[pack])
                                    {
                                        measure.InsertNotePack(new int?(measure.NotePacksCount), pack);
                                        flag = true;
                                    }
                                }
                                if (flag)
                                {
                                    measure.ReorganizeDurationStamps();
                                    foreach (McMeasure measure2 in measure.MeasuresAligned)
                                    {
                                        if (measure2 == McHoverInfo.HoveringMeasure)
                                        {
                                            McUtility.MarkModified(measure2);
                                        }
                                        else
                                        {
                                            McUtility.AppendRedrawingMeasure(measure2);
                                        }
                                    }
                                }
                            }
                        }
                        this.ShowCommonMessage("粘贴小节到乐谱完成");
                    }
                }
            }
        }

        public void PauseMusic()
        {
            this.IsMusicPlaying = false;
        }

        public void PlayMusic(bool continuePlaying, int startMeasureIndex = 0, int startNotePackIndex = -1)
        {
            this.IsMusicPlaying = true;
            if (!continuePlaying)
            {
                this._playingHemidemisemiquaverPtr = -1;
                McNotePack pack = null;
                if (((startNotePackIndex >= 0) && (McHoverInfo.HoveringMeasure != null)) && (McHoverInfo.HoveringMeasure.NotePacks.Length > 0))
                {
                    pack = McHoverInfo.HoveringMeasure.NotePacks[startNotePackIndex.Clamp(0, McHoverInfo.HoveringMeasure.NotePacksCount - 1)];
                    this._playingHemidemisemiquaverPtr = pack.TemporaryInfo.StampIndex - 1;
                }
                this._playingMeasureIndex = startMeasureIndex.Clamp(0, this.Notation.MeasureCountAligned - 1);
                this._playingLoopingAccumulatedTimeMs = 0.0;
                this._playingHemidemisemiquaverTimeMs = 8;
                this._lastValidDurationStampPtr = 0;
                float num = (this.Notation.RegularTracks.First<McRegularTrack>().GetMeasure(this._playingMeasureIndex).TemporaryInfo.RelativeX + McNotation.Margin.Left) + McRegularTrack.Margin.Left;
                if (pack != null)
                {
                    num += pack.TemporaryInfo.RelativeXSmooth;
                }
                this._playingNextValidStampAbsX = num;
                this._playingNextValidStampAbsXSmooth = this._playingNextValidStampAbsX;
                this._playingAutoScrollXSmooth = this._playingNextValidStampAbsX;
            }
        }

        public bool PlaySound(McMeasure.InstrumentTypes instrument, int pitchSemitoneIndex, float volume = 1f, long durationLeftMs = 0L, float decayFactor = 0.03f)
        {
            string key = Guid.NewGuid().ToString();
            if (this._sourceVoices.ContainsKey(key))
            {
                AudioSource audioSource = this._sourceVoices[key];
                this.DisposeAudioSource(audioSource);
                this._sourceVoices.Remove(key);
            }
            Stream stream = MusicBoxResources.GetSoundResource(instrument.ToString(), pitchSemitoneIndex, Assembly.GetExecutingAssembly().GetName().Version.ToString());
            if (stream == null)
            {
                return false;
            }
            SoundStream stream2 = new SoundStream(stream);
            WaveFormat sourceFormat = stream2.Format;
            AudioBuffer bufferRef = new AudioBuffer {
                Stream = stream2.ToDataStream(),
                AudioBytes = (int) stream2.Length,
                Flags = BufferFlags.EndOfStream
            };
            stream2.Close();
            stream.Dispose();
            volume = volume.Clamp(0f, 1f);
            volume = (instrument == McMeasure.InstrumentTypes.Tieqin) ? (volume * 0.33f) : volume;
            SourceVoice voice = new SourceVoice(this._xaudio2, sourceFormat, true);
            voice.SetVolume(volume, 0);
            voice.SubmitSourceBuffer(bufferRef, stream2.DecodedPacketsInfo);
            voice.Start();
            this._sourceVoices.Add(key, new AudioSource(bufferRef.Stream, voice, volume, durationLeftMs, decayFactor));
            return true;
        }

        public void RefreshCanvas()
        {
            this.RenderCanvas.Refresh();
        }

        private void RenderCanvas_DragDrop(object sender, DragEventArgs e)
        {
            string[] data = (string[]) e.Data.GetData(DataFormats.FileDrop, false);
            if (data.Length > 0)
            {
                foreach (string str in data)
                {
                    if ((Path.GetExtension(str) ?? "").ToLower().EndsWith("gjm"))
                    {
                        this.LoadMusicalNotation(str);
                        break;
                    }
                }
            }
        }

        private void RenderCanvas_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = e.Data.GetDataPresent(DataFormats.FileDrop) ? (DragDropEffects.Move | DragDropEffects.Copy | DragDropEffects.Scroll) : DragDropEffects.None;
        }

        public void RenderCanvas_MouseDown(object sender, MouseEventArgs e)
        {
            string str;
            MouseButtons button = e.Button;
            if (button == MouseButtons.Left)
            {
                if (this.IsEditMode && (McHoverInfo.HoveringMeasure != null))
                {
                    str = (McHoverInfo.HoveringNotePack == null) ? "音符" : "音高";
                    if (EnableDelaylessCommand)
                    {
                        if (this.IsCtrlKeyPressed())
                        {
                            MouseDragareaManager.Start(this, new MouseDragareaManager.CallbackDelegate(this.MouseDragarea_Callback));
                        }
                        else
                        {
                            this.MouseCircleProgress_Callback(new MouseButtons?(e.Button), null, MouseCircleProgressManager.ExitTypes.Finished, str);
                        }
                    }
                    else
                    {
                        if (!this.IsCtrlKeyPressed())
                        {
                            MouseCircleProgressManager.Start(this, 200, e.Button, new MouseCircleProgressManager.CallbackDelegate(this.MouseCircleProgress_Callback), str);
                        }
                        MouseDragareaManager.Start(this, new MouseDragareaManager.CallbackDelegate(this.MouseDragarea_Callback));
                    }
                }
            }
            else if (button == MouseButtons.Right)
            {
                if (((McHoverInfo.HoveringNotePack != null) && (McHoverInfo.HoveringMeasure != null)) && this.IsEditMode)
                {
                    str = "删除音符组";
                    if (EnableDelaylessCommand)
                    {
                        this.MouseCircleProgress_Callback(new MouseButtons?(e.Button), null, MouseCircleProgressManager.ExitTypes.Finished, str);
                    }
                    else
                    {
                        MouseCircleProgressManager.Start(this, 400, e.Button, new MouseCircleProgressManager.CallbackDelegate(this.MouseCircleProgress_Callback), str);
                    }
                }
            }
            else if ((button == MouseButtons.Middle) && ((McHoverInfo.HoveringMeasure != null) && this.IsEditMode))
            {
                str = "休止符";
                if (EnableDelaylessCommand)
                {
                    this.MouseCircleProgress_Callback(new MouseButtons?(e.Button), null, MouseCircleProgressManager.ExitTypes.Finished, str);
                }
                else
                {
                    MouseCircleProgressManager.Start(this, 200, e.Button, new MouseCircleProgressManager.CallbackDelegate(this.MouseCircleProgress_Callback), str);
                }
            }
        }

        public void RenderCanvas_MouseWheel(object sender, MouseEventArgs e)
        {
            if ((this.RenderCanvas.Focused && !MouseCircleProgressManager.IsStarted) && (!this.IsMusicPlaying || !EnableAutoScrollWhenPlayMusic))
            {
                float num = 3f;
                if (Canvas.IsCtrlKeyPressed())
                {
                    num = 0.5f;
                }
                else if (Canvas.IsShiftKeyPressed())
                {
                    num = 8f;
                }
                this._scrollX -= (int) (e.Delta * num);
            }
        }

        private void RenderCanvas_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            string str;
            if (this._keysPressedRepeatEventRecords.ContainsKey(e.KeyCode))
            {
                Dictionary<Keys, int> dictionary5;
                Keys keys;
                (dictionary5 = this._keysPressedRepeatEventRecords)[keys = e.KeyCode] = dictionary5[keys] + 1;
            }
            else
            {
                this._keysPressedRepeatEventRecords.Add(e.KeyCode, 1);
            }
            if (this._keysPressedRepeatEventRecords[e.KeyCode] != 1)
            {
                return;
            }
            if (e.KeyCode == Keys.F9)
            {
                Canvas.MusicBox.radMenuItemEnableFreePlayMode_Click(Canvas.MusicBox, null);
                e.IsInputKey = false;
                return;
            }
            if (EnableFreePlayMode && !this.IsMusicPlaying)
            {
                Dictionary<Keys, int> dictionary = new Dictionary<Keys, int> {
                    { 
                        Keys.F1,
                        0x40
                    },
                    { 
                        Keys.F2,
                        0x42
                    },
                    { 
                        Keys.F3,
                        0x44
                    },
                    { 
                        Keys.F4,
                        0x45
                    },
                    { 
                        Keys.F5,
                        0x47
                    },
                    { 
                        Keys.F6,
                        0x49
                    },
                    { 
                        Keys.F7,
                        0x4b
                    },
                    { 
                        Keys.D3,
                        0x34
                    },
                    { 
                        Keys.D4,
                        0x36
                    },
                    { 
                        Keys.D5,
                        0x38
                    },
                    { 
                        Keys.D6,
                        0x39
                    },
                    { 
                        Keys.D7,
                        0x3b
                    },
                    { 
                        Keys.D8,
                        0x3d
                    },
                    { 
                        Keys.D9,
                        0x3f
                    },
                    { 
                        Keys.E,
                        40
                    },
                    { 
                        Keys.R,
                        0x2a
                    },
                    { 
                        Keys.T,
                        0x2c
                    },
                    { 
                        Keys.Y,
                        0x2d
                    },
                    { 
                        Keys.U,
                        0x2f
                    },
                    { 
                        Keys.I,
                        0x31
                    },
                    { 
                        Keys.O,
                        0x33
                    },
                    { 
                        Keys.D,
                        0x1c
                    },
                    { 
                        Keys.F,
                        30
                    },
                    { 
                        Keys.G,
                        0x20
                    },
                    { 
                        Keys.H,
                        0x21
                    },
                    { 
                        Keys.J,
                        0x23
                    },
                    { 
                        Keys.K,
                        0x25
                    },
                    { 
                        Keys.L,
                        0x27
                    },
                    { 
                        Keys.C,
                        0x10
                    },
                    { 
                        Keys.V,
                        0x12
                    },
                    { 
                        Keys.B,
                        20
                    },
                    { 
                        Keys.N,
                        0x15
                    },
                    { 
                        Keys.M,
                        0x17
                    },
                    { 
                        Keys.Oemcomma,
                        0x19
                    },
                    { 
                        Keys.OemPeriod,
                        0x1b
                    }
                };
                Dictionary<Keys, int> dictionary2 = new Dictionary<Keys, int> {
                    { 
                        Keys.F1,
                        0x41
                    },
                    { 
                        Keys.F2,
                        0x43
                    },
                    { 
                        Keys.F3,
                        0x44
                    },
                    { 
                        Keys.F4,
                        70
                    },
                    { 
                        Keys.F5,
                        0x48
                    },
                    { 
                        Keys.F6,
                        0x4a
                    },
                    { 
                        Keys.F7,
                        0x4b
                    },
                    { 
                        Keys.D3,
                        0x35
                    },
                    { 
                        Keys.D4,
                        0x37
                    },
                    { 
                        Keys.D5,
                        0x38
                    },
                    { 
                        Keys.D6,
                        0x3a
                    },
                    { 
                        Keys.D7,
                        60
                    },
                    { 
                        Keys.D8,
                        0x3e
                    },
                    { 
                        Keys.D9,
                        0x3f
                    },
                    { 
                        Keys.E,
                        0x29
                    },
                    { 
                        Keys.R,
                        0x2b
                    },
                    { 
                        Keys.T,
                        0x2c
                    },
                    { 
                        Keys.Y,
                        0x2e
                    },
                    { 
                        Keys.U,
                        0x30
                    },
                    { 
                        Keys.I,
                        50
                    },
                    { 
                        Keys.O,
                        0x33
                    },
                    { 
                        Keys.D,
                        0x1d
                    },
                    { 
                        Keys.F,
                        0x1f
                    },
                    { 
                        Keys.G,
                        0x20
                    },
                    { 
                        Keys.H,
                        0x22
                    },
                    { 
                        Keys.J,
                        0x24
                    },
                    { 
                        Keys.K,
                        0x26
                    },
                    { 
                        Keys.L,
                        0x27
                    },
                    { 
                        Keys.C,
                        0x11
                    },
                    { 
                        Keys.V,
                        0x13
                    },
                    { 
                        Keys.B,
                        20
                    },
                    { 
                        Keys.N,
                        0x16
                    },
                    { 
                        Keys.M,
                        0x18
                    },
                    { 
                        Keys.Oemcomma,
                        0x1a
                    },
                    { 
                        Keys.OemPeriod,
                        0x1b
                    }
                };
                if (this.IsShiftKeyPressed())
                {
                    if (dictionary2.ContainsKey(e.KeyCode))
                    {
                        this.PlaySound((McHoverInfo.HoveringMeasure != null) ? McHoverInfo.HoveringMeasure.InstrumentType : McMeasure.InstrumentTypes.Piano, dictionary2[e.KeyCode] + this.Notation.NumberedKeySignature, 1f, 0L, 0.03f);
                    }
                }
                else if (dictionary.ContainsKey(e.KeyCode))
                {
                    this.PlaySound((McHoverInfo.HoveringMeasure != null) ? McHoverInfo.HoveringMeasure.InstrumentType : McMeasure.InstrumentTypes.Piano, dictionary[e.KeyCode] + this.Notation.NumberedKeySignature, 1f, 0L, 0.03f);
                }
                e.IsInputKey = false;
                return;
            }
            bool flag = false;
            McNotePack hoveringNotePack = McHoverInfo.HoveringNotePack;
            McMeasure hoveringMeasure = McHoverInfo.HoveringMeasure;
            switch (e.KeyCode)
            {
                case Keys.Enter:
                    if (this.IsMusicPlaying)
                    {
                        this.PauseMusic();
                    }
                    else if (this.IsMusicPaused)
                    {
                        this.PlayMusic(true, 0, -1);
                    }
                    else
                    {
                        this.PlayMusic(false, 0, -1);
                    }
                    goto Label_0E86;

                case Keys.Escape:
                    if (this.IsMusicPlaying)
                    {
                        this.StopMusic();
                    }
                    else if (MouseDragareaManager.IsStarted)
                    {
                        MouseDragareaManager.Fade(MouseDragareaManager.ExitTypes.CancelOnEsc);
                    }
                    else if (MouseDragareaManager.HasValidCanvasDragarea)
                    {
                        MouseDragareaManager.ClearValidCanvasDragarea();
                    }
                    else
                    {
                        this.StopMusic();
                        this.MouseCircleProgress_Callback(null, 0x33, MouseCircleProgressManager.ExitTypes.Finished, "");
                    }
                    goto Label_0E86;

                case Keys.Space:
                    if (!this.IsMusicPlaying)
                    {
                        if (McHoverInfo.HoveringMeasure != null)
                        {
                            this.PlayMusic(false, McHoverInfo.HoveringMeasure.Index, McHoverInfo.HoveringInsertNotePackIndex);
                        }
                        else if (this.IsMusicPaused)
                        {
                            this.PlayMusic(true, 0, -1);
                        }
                        else
                        {
                            this.PlayMusic(false, 0, -1);
                        }
                    }
                    else
                    {
                        this.PauseMusic();
                    }
                    goto Label_0E86;

                case Keys.PageUp:
                    this._playingSpeed = ((float) (this._playingSpeed + 0.1f)).Clamp((float) 0.5f, (float) 3f);
                    goto Label_0E86;

                case Keys.Next:
                    this._playingSpeed = ((float) (this._playingSpeed - 0.1f)).Clamp((float) 0.5f, (float) 3f);
                    goto Label_0E86;

                case Keys.Up:
                    if (this.IsEditMode && (hoveringMeasure != null))
                    {
                        hoveringMeasure.Shift(12);
                        flag = true;
                    }
                    goto Label_0E86;

                case Keys.Down:
                    if (this.IsEditMode && (hoveringMeasure != null))
                    {
                        hoveringMeasure.Shift(-12);
                        flag = true;
                    }
                    goto Label_0E86;

                case Keys.Delete:
                    if (!this.IsMusicPlaying && this.IsEditMode)
                    {
                        str = "删除选中音符";
                        if (!EnableDelaylessCommand)
                        {
                            MouseCircleProgressManager.Start(this, 300, e.KeyCode, new MouseCircleProgressManager.CallbackDelegate(this.MouseCircleProgress_Callback), str);
                        }
                        else
                        {
                            this.MouseCircleProgress_Callback(null, new Keys?(e.KeyCode), MouseCircleProgressManager.ExitTypes.Finished, str);
                        }
                    }
                    goto Label_0E86;

                case Keys.D1:
                    if (this.IsEditMode)
                    {
                        str = "插入小节";
                        if (!EnableDelaylessCommand)
                        {
                            MouseCircleProgressManager.Start(this, 300, e.KeyCode, new MouseCircleProgressManager.CallbackDelegate(this.MouseCircleProgress_Callback), str);
                        }
                        else
                        {
                            this.MouseCircleProgress_Callback(null, new Keys?(e.KeyCode), MouseCircleProgressManager.ExitTypes.Finished, str);
                        }
                    }
                    goto Label_0E86;

                case Keys.D2:
                    if (this.IsEditMode && (hoveringMeasure != null))
                    {
                        str = "删除小节";
                        if (!EnableDelaylessCommand)
                        {
                            MouseCircleProgressManager.Start(this, 300, e.KeyCode, new MouseCircleProgressManager.CallbackDelegate(this.MouseCircleProgress_Callback), str);
                        }
                        else
                        {
                            this.MouseCircleProgress_Callback(null, new Keys?(e.KeyCode), MouseCircleProgressManager.ExitTypes.Finished, str);
                        }
                    }
                    goto Label_0E86;

                case Keys.D3:
                    if (this.IsEditMode)
                    {
                        str = "乐谱/小节属性";
                        if (!EnableDelaylessCommand)
                        {
                            this.MouseCircleProgress_Callback(null, new Keys?(e.KeyCode), MouseCircleProgressManager.ExitTypes.Finished, str);
                        }
                        else
                        {
                            this.MouseCircleProgress_Callback(null, new Keys?(e.KeyCode), MouseCircleProgressManager.ExitTypes.Finished, str);
                        }
                    }
                    goto Label_0E86;

                case Keys.A:
                    if (this.IsEditMode && (hoveringNotePack != null))
                    {
                        switch (hoveringNotePack.TieType)
                        {
                            case McNotePack.TieTypes.None:
                                hoveringNotePack.TieType = McNotePack.TieTypes.Start;
                                break;

                            case McNotePack.TieTypes.Start:
                                hoveringNotePack.TieType = McNotePack.TieTypes.None;
                                break;

                            case McNotePack.TieTypes.End:
                                hoveringNotePack.TieType = McNotePack.TieTypes.Both;
                                break;

                            case McNotePack.TieTypes.Both:
                                hoveringNotePack.TieType = McNotePack.TieTypes.End;
                                break;
                        }
                        flag = true;
                    }
                    goto Label_0E86;

                case Keys.C:
                    if (!(((!this.IsEditMode || (hoveringNotePack == null)) || (hoveringMeasure == null)) || hoveringNotePack.IsRest))
                    {
                        hoveringNotePack.Staccato = !hoveringNotePack.Staccato;
                        flag = true;
                    }
                    goto Label_0E86;

                case Keys.D:
                    if ((this.IsEditMode && (hoveringNotePack != null)) && (hoveringMeasure != null))
                    {
                        hoveringNotePack.Triplet = !hoveringNotePack.Triplet;
                        McHoverInfo.HoveringMeasure.ReorganizeDurationStamps();
                        flag = true;
                    }
                    goto Label_0E86;

                case Keys.L:
                    if (e.Control)
                    {
                        this.LoadMusicalNotation();
                        e.IsInputKey = false;
                    }
                    goto Label_0E86;

                case Keys.N:
                    if (e.Control)
                    {
                        this.NewMusicalNotation();
                        e.IsInputKey = false;
                    }
                    goto Label_0E86;

                case Keys.Q:
                case Keys.W:
                    if ((this.IsEditMode && (hoveringMeasure != null)) && (hoveringNotePack != null))
                    {
                        int num = (e.KeyCode == Keys.Q) ? 1 : -1;
                        List<McNotePack.DurationTypes> list = new List<McNotePack.DurationTypes>(new McNotePack.DurationTypes[] { McNotePack.DurationTypes.The32nd, McNotePack.DurationTypes.The16th, McNotePack.DurationTypes.Eighth, McNotePack.DurationTypes.Quarter, McNotePack.DurationTypes.Half, McNotePack.DurationTypes.Whole });
                        int num2 = (list.IndexOf(McHoverInfo.HoveringNotePack.DurationType) + num).Clamp(0, list.Count - 1);
                        if (McHoverInfo.HoveringNotePack.DurationType != ((McNotePack.DurationTypes) list[num2]))
                        {
                            McHoverInfo.HoveringNotePack.DurationType = list[num2];
                            McHoverInfo.HoveringMeasure.ReorganizeDurationStamps();
                            flag = true;
                        }
                    }
                    goto Label_0E86;

                case Keys.S:
                    if (!e.Control)
                    {
                        if (this.IsEditMode && (hoveringNotePack != null))
                        {
                            switch (hoveringNotePack.TieType)
                            {
                                case McNotePack.TieTypes.None:
                                    hoveringNotePack.TieType = McNotePack.TieTypes.End;
                                    break;

                                case McNotePack.TieTypes.Start:
                                    hoveringNotePack.TieType = McNotePack.TieTypes.Both;
                                    break;

                                case McNotePack.TieTypes.End:
                                    hoveringNotePack.TieType = McNotePack.TieTypes.None;
                                    break;

                                case McNotePack.TieTypes.Both:
                                    hoveringNotePack.TieType = McNotePack.TieTypes.Start;
                                    break;
                            }
                            flag = true;
                        }
                    }
                    else
                    {
                        this.SaveMusicalNotation(e.Shift ? "" : null);
                        e.IsInputKey = false;
                    }
                    goto Label_0E86;

                case Keys.V:
                    if (!this.IsMusicPlaying && this.IsEditMode)
                    {
                        Point point = base.PointToClient(Control.MousePosition);
                        RawRectangleF ef = new RawRectangleF(0f, 0f, (float) base.Width, (float) base.Height);
                        if ((((point.X >= ef.Left) && (point.X <= ef.Right)) && (point.Y >= ef.Top)) && (point.Y <= ef.Bottom))
                        {
                            if (!e.Control || !e.Shift)
                            {
                                if (e.Control)
                                {
                                    this.PasteSelectionToMeasure();
                                    goto Label_0E86;
                                }
                                if ((hoveringNotePack == null) || (hoveringMeasure == null))
                                {
                                    goto Label_0E86;
                                }
                                switch (hoveringNotePack.ArpeggioModeRaw)
                                {
                                    case Arpeggio.ArpeggioTypes.Upward:
                                        hoveringNotePack.ArpeggioMode = Arpeggio.ArpeggioTypes.Downward;
                                        goto Label_0831;

                                    case Arpeggio.ArpeggioTypes.Downward:
                                        hoveringNotePack.ArpeggioMode = Arpeggio.ArpeggioTypes.None;
                                        goto Label_0831;

                                    case Arpeggio.ArpeggioTypes.None:
                                        hoveringNotePack.ArpeggioMode = Arpeggio.ArpeggioTypes.Upward;
                                        goto Label_0831;
                                }
                                break;
                            }
                            this.PasteSelectionToNotation();
                        }
                    }
                    goto Label_0E86;

                case Keys.X:
                    if ((this.IsEditMode && (hoveringNotePack != null)) && (hoveringMeasure != null))
                    {
                        hoveringNotePack.IsDotted = !hoveringNotePack.IsDotted;
                        McHoverInfo.HoveringMeasure.ReorganizeDurationStamps();
                        flag = true;
                    }
                    goto Label_0E86;

                case Keys.Z:
                    if (((this.IsEditMode && (hoveringNotePack != null)) && ((hoveringMeasure != null) && !hoveringNotePack.IsRest)) && (McHoverInfo.HoveringInsertPitchValue > 0))
                    {
                        McPitch pitch = hoveringNotePack.GetPitch(McHoverInfo.HoveringInsertPitchValue);
                        if ((pitch != null) && (pitch.PitchType == McPitch.PitchTypes.Enabled))
                        {
                            switch (pitch.RawAlterantType)
                            {
                                case McPitch.AlterantTypes.NoControl:
                                    pitch.AlterantType = McPitch.AlterantTypes.Natural;
                                    break;

                                case McPitch.AlterantTypes.Natural:
                                    pitch.AlterantType = McPitch.AlterantTypes.Sharp;
                                    break;

                                case McPitch.AlterantTypes.Sharp:
                                    pitch.AlterantType = McPitch.AlterantTypes.Flat;
                                    break;

                                case McPitch.AlterantTypes.Flat:
                                    pitch.AlterantType = McPitch.AlterantTypes.NoControl;
                                    break;
                            }
                            flag = true;
                        }
                    }
                    goto Label_0E86;

                case Keys.F1:
                    Canvas.MusicBox.radMenuItemUploadNotation_Click(Canvas.MusicBox, null);
                    goto Label_0E86;

                case Keys.F10:
                    Canvas.MusicBox.radMenuItemEnableDelaylessCommand_Click(Canvas.MusicBox, null);
                    goto Label_0E86;

                default:
                    goto Label_0E86;
            }
        Label_0831:
            flag = true;
        Label_0E86:
            if (flag)
            {
                foreach (McMeasure measure2 in hoveringMeasure.MeasuresAligned)
                {
                    if (measure2 == hoveringMeasure)
                    {
                        McUtility.MarkModified(measure2);
                    }
                    else
                    {
                        McUtility.AppendRedrawingMeasure(measure2);
                    }
                }
            }
        }

        private void RenderControl_Paint(object sender, PaintEventArgs e)
        {
            long num = this._updateStopwatchElapsedMs;
            try
            {
                McMeasure measure;
                McNotePack hoveringNotePack;
                int num22;
                int num25;
                RawRectangleF ef4;
                float num37;
                float num38;
                float num39;
                float num40;
                this.RenderTarget2D.BeginDraw();
                this.RenderTarget2D.Clear(new RawColor4?(this.BackgroundColor));
                foreach (List<McMeasure> list in this._drawingMeasures)
                {
                    if (list != null)
                    {
                        foreach (McMeasure measure in list)
                        {
                            if (measure.BitmapCache != null)
                            {
                                this.DrawBitmap(measure.BitmapCache, new RawVector2(((measure.TemporaryInfo.RelativeX + McNotation.Margin.Left) + McRegularTrack.Margin.Left) - this._scrollXSmooth, (float) measure.Top));
                            }
                        }
                    }
                }
                RawRectangleF ef = new RawRectangleF();
                McMeasure hoveringMeasure = McHoverInfo.HoveringMeasure;
                if (hoveringMeasure != null)
                {
                    float left = ((hoveringMeasure.TemporaryInfo.RelativeX + McNotation.Margin.Left) + McRegularTrack.Margin.Left) - this._scrollXSmooth;
                    ef = new RawRectangleF(left, (float) hoveringMeasure.Top, left + hoveringMeasure.TemporaryInfo.Width, (float) (hoveringMeasure.Top + McMeasure.Height));
                    RawRectangleF rectF = new RawRectangleF(ef.Left + 2f, ef.Top + 2f, ef.Right - 2f, ef.Bottom - 2f);
                    this.FillRoundedRectangle(rectF, 2f, Color.FromArgb(8, Color.WhiteSmoke));
                }
                if ((McHoverInfo.HoveringMeasure != null) && this.IsEditMode)
                {
                    measure = McHoverInfo.HoveringMeasure;
                    hoveringNotePack = McHoverInfo.HoveringNotePack;
                    int hoveringInsertPitchValue = McHoverInfo.HoveringInsertPitchValue;
                    if ((hoveringInsertPitchValue > 0) && (!EnableDelaylessCommand || !Canvas.IsCtrlKeyPressed()))
                    {
                        int num11;
                        RawVector2 vector = new RawVector2();
                        McHoverInfo.HoveringInsertLineAlphaSmooth = McHoverInfo.HoveringInsertLineAlphaSmooth.Lerp((McHoverInfo.HoveringNotePack == null) ? ((float) 1) : ((float) 0), 0.1f);
                        RawVector2 vector2 = new RawVector2();
                        int hoveringInsertNotePackIndex = McHoverInfo.HoveringInsertNotePackIndex;
                        McNotePack[] notePacks = measure.NotePacks;
                        int num5 = McMeasure.MeasureHeadWidth + measure.KeySignatureZoneWidth;
                        int notePacksCount = measure.NotePacksCount;
                        if ((notePacksCount == 0) || (hoveringInsertNotePackIndex <= 0))
                        {
                            vector.X = ((ef.Left + num5) - 20f) - 10f;
                            vector.Y = ef.Top + 2f;
                        }
                        else if (McHoverInfo.HoveringInsertNotePackIndex >= notePacksCount)
                        {
                            McNotePack pack2 = notePacks.Last<McNotePack>();
                            vector.X = ((ef.Left + pack2.TemporaryInfo.RelativeX) + 20f) + 20f;
                            vector.Y = ef.Top + 2f;
                        }
                        else
                        {
                            McNotePack pack3 = notePacks[McHoverInfo.HoveringInsertNotePackIndex];
                            vector.X = (ef.Left + pack3.TemporaryInfo.RelativeX) - 20f;
                            vector.Y = ef.Top + 2f;
                        }
                        vector2.X = vector.X;
                        vector2.Y = (ef.Top + McMeasure.Height) - 2f;
                        if (EnableSnaplineWhenInsertNotePack)
                        {
                            this.DrawLine(vector, vector2, Color.FromArgb((int) (128f * McHoverInfo.HoveringInsertLineAlphaSmooth), 0x80, 0xff, 0), 1f);
                        }
                        if (hoveringNotePack == null)
                        {
                            float num12;
                            int notePackWidth = McNotePack.NotePackWidth;
                            Color baseColor = Color.FromArgb(0x80, 0x80, 0x80);
                            float num8 = (((float) notePackWidth) / 5f) - 2f;
                            int x = (int) vector.X;
                            int measureLineValue = McPitch.GetMeasureLineValue(measure.ClefType, hoveringInsertPitchValue);
                            if (measureLineValue > 40)
                            {
                                for (num11 = 50; num11 <= measureLineValue; num11 += 5)
                                {
                                    if ((num11 % 10) == 0)
                                    {
                                        num12 = measure.Top + McPitch.GetMeasureLineRelativeYByLineValue(measure.ClefType, num11);
                                        this.DrawLine(new RawVector2(x - num8, num12), new RawVector2(x + num8, num12), Color.FromArgb(200, baseColor));
                                    }
                                }
                            }
                            else if (measureLineValue < 0)
                            {
                                for (num11 = measureLineValue; num11 <= -10; num11 += 5)
                                {
                                    if ((num11 % 10) == 0)
                                    {
                                        num12 = measure.Top + McPitch.GetMeasureLineRelativeYByLineValue(measure.ClefType, num11);
                                        this.DrawLine(new RawVector2(x - num8, num12), new RawVector2(x + num8, num12), Color.FromArgb(200, baseColor));
                                    }
                                }
                            }
                        }
                        else
                        {
                            float num13 = hoveringNotePack.TemporaryInfo.RelativeXSmooth + 8f;
                            vector.X = ef.Left + num13;
                        }
                        float measureLineRelativeY = McPitch.GetMeasureLineRelativeY(measure.ClefType, hoveringInsertPitchValue);
                        if (hoveringInsertPitchValue > 0)
                        {
                            SharpDX.Direct2D1.Bitmap bitmap;
                            if ((hoveringNotePack != null) && hoveringNotePack.IsRest)
                            {
                                Color orange = Color.Orange;
                                bitmap = CanvasRenderCache.GetNoteBitmapCache(CanvasRenderCache.BitmapNoteTypes.Rest, orange, hoveringNotePack.DurationType);
                                this.DrawBitmap(bitmap, new RawVector2(vector.X - 8f, (ef.Top + McPitch.GetMeasureFirstLineRelativeY(measure.ClefType)) - 44f), 0.9f);
                            }
                            else
                            {
                                Color color = ((hoveringNotePack != null) && (hoveringNotePack.GetPitchType(hoveringInsertPitchValue) == McPitch.PitchTypes.Enabled)) ? ((hoveringNotePack.ValidPitchCount > 1) ? Color.OrangeRed : McNotePack.NoteColorWhite) : Color.LawnGreen;
                                bitmap = (McHoverInfo.HoveringMeasure.InstrumentType == McMeasure.InstrumentTypes.Misc) ? CanvasRenderCache.GetNoteBitmapCache(CanvasRenderCache.BitmapNoteTypes.Misc, color, (hoveringNotePack != null) ? hoveringNotePack.DurationType : McNotePack.DurationTypes.Quarter) : CanvasRenderCache.GetNoteBitmapCache(CanvasRenderCache.BitmapNoteTypes.Common, color, (hoveringNotePack != null) ? hoveringNotePack.DurationType : McNotePack.DurationTypes.Quarter);
                                this.DrawBitmap(bitmap, new RawVector2(vector.X - 8f, (ef.Top + measureLineRelativeY) - 8f), 0.75f);
                            }
                        }
                        if (EnableNumberedSignTip && ((hoveringNotePack == null) || !hoveringNotePack.IsRest))
                        {
                            RawVector2 vector3 = new RawVector2(vector.X + 36f, (ef.Top + measureLineRelativeY) - 20f);
                            int pitch = (hoveringInsertPitchValue - this.Notation.NumberedKeySignature).Clamp(McPitch.PitchMin, McPitch.PitchMax);
                            int toneValue = McPitch.GetToneValue(pitch);
                            int scaleLevel = McPitch.GetScaleLevel(pitch);
                            bool flag = !McPitch.IsNatural(pitch);
                            RawRectangleF ef3 = new RawRectangleF(vector3.X - 26f, vector3.Y - 10f, vector3.X - 14f, vector3.Y + 9f);
                            ef3.Left -= flag ? ((float) 6) : ((float) 0);
                            ef3.Top = (scaleLevel > 3) ? (ef3.Top - (Math.Abs((int) (scaleLevel - 3)) * 6)) : ef3.Top;
                            ef3.Bottom = (scaleLevel < 3) ? (ef3.Bottom + (Math.Abs((int) (scaleLevel - 3)) * 6)) : ef3.Bottom;
                            this.FillRectangle(ef3, Color.FromArgb(0x1f, 0x1f, 0x23));
                            this.DrawRoundedRectangle(ef3, 0f, Color.DimGray, 2f);
                            this.DrawTextLayout($"{flag ? "#" : " "}{toneValue}", new RawVector2(vector3.X - 30f, vector3.Y - 1f), "Consolas", 12f, Color.Yellow, TextAlignment.Leading, ParagraphAlignment.Center, SharpDX.DirectWrite.FontStyle.Normal, FontWeight.Bold);
                            if (scaleLevel > 3)
                            {
                                for (num11 = 0; num11 <= (scaleLevel - 4); num11++)
                                {
                                    this.FillCircle(new RawVector2(vector3.X - 20f, (vector3.Y - 10f) - (num11 * 6)), 2f, Color.DodgerBlue);
                                }
                            }
                            else if (scaleLevel < 3)
                            {
                                for (num11 = 0; num11 >= (scaleLevel - 2); num11--)
                                {
                                    this.FillCircle(new RawVector2(vector3.X - 20f, (vector3.Y + 9f) - (num11 * 6)), 2f, Color.DodgerBlue);
                                }
                            }
                        }
                    }
                }
                Color color4 = Color.FromArgb(150, Color.WhiteSmoke);
                Color color5 = Color.FromArgb(200, Color.Aqua);
                for (int i = 0; i < this._drawingMeasures.Length; i++)
                {
                    list = this._drawingMeasures[i];
                    if (list != null)
                    {
                        int num19 = 0;
                        for (int j = 0; j < list.Count; j++)
                        {
                            measure = list[j];
                            if (measure.BitmapCache != null)
                            {
                                for (int k = 0; k < measure.NotePacks.Length; k++)
                                {
                                    hoveringNotePack = measure.NotePacks[k];
                                    McNotePack.NpTemporaryInfo temporaryInfo = hoveringNotePack.TemporaryInfo;
                                    if ((hoveringNotePack.TieType == McNotePack.TieTypes.End) || (hoveringNotePack.TieType == McNotePack.TieTypes.Both))
                                    {
                                        num19++;
                                        if (num19 == 1)
                                        {
                                            this.RenderTarget2D.DrawTie(this.FactoryDWrite, temporaryInfo.LinkedInTieNote, hoveringNotePack, color4, false);
                                        }
                                    }
                                    if ((hoveringNotePack.TieType == McNotePack.TieTypes.Start) || (hoveringNotePack.TieType == McNotePack.TieTypes.Both))
                                    {
                                        num19++;
                                        this.RenderTarget2D.DrawTie(this.FactoryDWrite, hoveringNotePack, temporaryInfo.LinkedOutTieNote, color4, false);
                                    }
                                    if (hoveringNotePack.Triplet)
                                    {
                                        McNotePack endNotePack = (hoveringNotePack.TemporaryInfo.TripletNotePacks != null) ? hoveringNotePack.TemporaryInfo.TripletNotePacks[2] : null;
                                        this.RenderTarget2D.DrawTie(this.FactoryDWrite, hoveringNotePack, endNotePack, color5, true);
                                    }
                                }
                            }
                        }
                    }
                }
                if (this.Notation.DrawingWidth > base.Width)
                {
                    num22 = (((Math.Max(this.Notation.DrawingWidth, base.Width) + McNotation.Margin.Left) + McRegularTrack.Margin.Left) + McNotation.Margin.Right) + McRegularTrack.Margin.Right;
                    int num23 = Math.Max(0x18, (int) (base.Width * (((float) base.Width) / ((float) num22))));
                    float num24 = this._scrollXSmooth / ((num22 - base.Width) + 1f);
                    num25 = (int) ((base.Width - num23) * num24);
                    ef4 = new RawRectangleF((float) num25, (float) (base.Height - 9), (float) (num25 + num23), (float) (base.Height - 2));
                    this.FillRoundedRectangle(ef4, 0f, Color.DodgerBlue);
                }
                if (this.IsMusicPlaying || this.IsMusicPaused)
                {
                    McMeasure measure3 = this.Notation.RegularTracks.First<McRegularTrack>().GetMeasure(this._playingMeasureIndex);
                    float num26 = (((measure3 == null) ? 1f : ((((float) measure3.BeatsPerMinute) / 100f) + 0.2f)) * num) / 1000f;
                    this._playingAutoScrollXSmooth += (this._playingNextValidStampAbsXSmooth - this._playingAutoScrollXSmooth) * num26;
                    float num27 = ((float) (8f / ((this._playingDeltaDurationStampPtr < 1) ? ((float) 0x20) : ((float) this._playingDeltaDurationStampPtr)))).Clamp((float) 0.1f, (float) 1f);
                    this._playingNextValidStampAbsXSmooth = this._playingNextValidStampAbsXSmooth.Lerp(this._playingNextValidStampAbsX, 0.2f * num27);
                    float num28 = this._playingNextValidStampAbsXSmooth - this._scrollXSmooth;
                    float y = McNotation.Margin.Top + McRegularTrack.Margin.Top;
                    float num30 = ((((McRegularTrack.Margin.Bottom + McRegularTrack.Margin.Top) + McMeasure.Height) * 2) + y) + McMeasure.Height;
                    this.DrawLine(new RawVector2(num28, y), new RawVector2(num28, num30), this.IsMusicPaused ? Color.FromArgb(90, Color.GhostWhite) : Color.FromArgb(150, Color.Yellow));
                    if (!this.IsMusicPaused)
                    {
                        PlayingStickAniManager.Append((int) Math.Round((double) this._playingNextValidStampAbsXSmooth), Color.LawnGreen, -1);
                    }
                    num22 = (((Math.Max(this.Notation.DrawingWidth, base.Width) + McNotation.Margin.Left) + McRegularTrack.Margin.Left) + McNotation.Margin.Right) + McRegularTrack.Margin.Right;
                    int num31 = 4;
                    float num32 = this._playingAutoScrollXSmooth / ((float) num22);
                    num25 = (int) ((base.Width - num31) * num32);
                    ef4 = new RawRectangleF((float) num25, (float) (base.Height - 10), (float) (num25 + num31), (float) (base.Height - 1));
                    this.FillRoundedRectangle(ef4, 0f, Color.Yellow);
                }
                if (EnableFreePlayMode)
                {
                    RawRectangleF ef5 = new RawRectangleF(0f, 0f, (float) base.Width, (float) base.Height);
                    this.FillRoundedRectangle(ef5, 0f, Color.FromArgb(0x20, Color.DodgerBlue));
                }
                if (this.Logo != null)
                {
                    this.DrawBitmap(this.Logo, new RawVector2((float) (base.Width - 200), (float) (base.Height - 0x5d)));
                }
                float num33 = 56f;
                float num34 = 20f;
                float num35 = 28f;
                float num36 = -8f;
                if (McUtility.IsModified)
                {
                    ef4 = new RawRectangleF((base.Width - num33) + num36, num35 - (num34 / 2f), base.Width + num36, num35 + (num34 / 2f));
                    this.FillRoundedRectangle(ef4, 4f, Color.FromArgb(0x80, Color.Yellow));
                    this.DrawRoundedRectangle(ef4, 4f, Color.FromArgb(0x80, Color.LightGray), 2f);
                    this.DrawTextLayout("存在改动", new RawVector2(((base.Width - num33) + num36) + (num33 / 2f), num35 - 1f), "微软雅黑", 12f, Color.LightYellow, TextAlignment.Center, ParagraphAlignment.Center, SharpDX.DirectWrite.FontStyle.Normal, FontWeight.ExtraBlack);
                    num36 -= num33 + 4f;
                }
                if (!this.RenderCanvas.Focused)
                {
                    ef4 = new RawRectangleF((base.Width - num33) + num36, num35 - (num34 / 2f), base.Width + num36, num35 + (num34 / 2f));
                    this.FillRoundedRectangle(ef4, 4f, Color.FromArgb(80, Color.Black));
                    this.DrawRoundedRectangle(ef4, 4f, Color.FromArgb(0x80, Color.LightGray), 2f);
                    this.DrawTextLayout("丢失焦点", new RawVector2(((base.Width - num33) + num36) + (num33 / 2f), num35 - 1f), "微软雅黑", 12f, Color.LightYellow, TextAlignment.Center, ParagraphAlignment.Center, SharpDX.DirectWrite.FontStyle.Normal, FontWeight.ExtraBlack);
                    num36 -= num33 + 4f;
                    if (!this.IsMusicPlaying)
                    {
                    }
                }
                if (this.IsEditMode)
                {
                    ef4 = new RawRectangleF((base.Width - num33) + num36, num35 - (num34 / 2f), base.Width + num36, num35 + (num34 / 2f));
                    this.FillRoundedRectangle(ef4, 4f, Color.FromArgb(80, Color.ForestGreen));
                    this.DrawRoundedRectangle(ef4, 4f, Color.FromArgb(0x80, Color.LightGray), 2f);
                    this.DrawTextLayout("编辑状态", new RawVector2(((base.Width - num33) + num36) + (num33 / 2f), num35 - 1f), "微软雅黑", 12f, Color.LightYellow, TextAlignment.Center, ParagraphAlignment.Center, SharpDX.DirectWrite.FontStyle.Normal, FontWeight.ExtraBlack);
                    num36 -= num33 + 4f;
                }
                if (this.Notation.NumberedKeySignature != McNotation.NumberedKeySignatureTypes.C)
                {
                    num37 = num33 - 12f;
                    ef4 = new RawRectangleF((base.Width - num37) + num36, num35 - (num34 / 2f), base.Width + num36, num35 + (num34 / 2f));
                    this.FillRoundedRectangle(ef4, 4f, Color.FromArgb(80, Color.RoyalBlue));
                    this.DrawRoundedRectangle(ef4, 4f, Color.FromArgb(0x80, Color.LightGray), 2f);
                    this.DrawTextLayout($"1={this.Notation.NumberedKeySignature.ToString()}", new RawVector2(((base.Width - num37) + num36) + (num37 / 2f), num35 - 1f), "微软雅黑", 12f, Color.LightYellow, TextAlignment.Center, ParagraphAlignment.Center, SharpDX.DirectWrite.FontStyle.Normal, FontWeight.ExtraBlack);
                    num36 -= num37 + 4f;
                }
                if (Math.Abs((float) (this._playingSpeed - 1f)) > 0.01f)
                {
                    num37 = num33 + 32f;
                    ef4 = new RawRectangleF((base.Width - num37) + num36, num35 - (num34 / 2f), base.Width + num36, num35 + (num34 / 2f));
                    this.FillRoundedRectangle(ef4, 4f, Color.FromArgb(80, Color.MediumSlateBlue));
                    this.DrawRoundedRectangle(ef4, 4f, Color.FromArgb(0x80, Color.LightGray), 2f);
                    this.DrawTextLayout($"演奏速度 x{this._playingSpeed:F1}", new RawVector2(((base.Width - num37) + num36) + (num37 / 2f), num35 - 1f), "微软雅黑", 12f, Color.LightYellow, TextAlignment.Center, ParagraphAlignment.Center, SharpDX.DirectWrite.FontStyle.Normal, FontWeight.ExtraBlack);
                    num36 -= num37 + 4f;
                }
                if (this.IsMusicPlaying)
                {
                    ef4 = new RawRectangleF((base.Width - num33) + num36, num35 - (num34 / 2f), base.Width + num36, num35 + (num34 / 2f));
                    this.FillRoundedRectangle(ef4, 4f, Color.FromArgb(80, Color.LawnGreen));
                    this.DrawRoundedRectangle(ef4, 4f, Color.FromArgb(0x80, Color.LightGray), 2f);
                    this.DrawTextLayout("演奏中…", new RawVector2(((base.Width - num33) + num36) + (num33 / 2f), num35 - 1f), "微软雅黑", 12f, Color.LightYellow, TextAlignment.Center, ParagraphAlignment.Center, SharpDX.DirectWrite.FontStyle.Normal, FontWeight.ExtraBlack);
                    num36 -= num33 + 4f;
                }
                else if (EnableFreePlayMode)
                {
                    num37 = base.Width - 4;
                    num38 = 64f;
                    num39 = 300f;
                    num40 = -2f;
                    ef4 = new RawRectangleF((base.Width - num37) + num40, num39 - (num38 / 2f), base.Width + num40, num39 + (num38 / 2f));
                    this.FillRoundedRectangle(ef4, 4f, Color.FromArgb(200, Color.DimGray));
                    this.DrawRoundedRectangle(ef4, 4f, Color.FromArgb(0xff, Color.LightGray), 2f);
                    this.DrawTextLayout("自由弹奏模式", new RawVector2(((base.Width - num37) + num40) + (num37 / 2f), (num39 - 1f) - 10f), "微软雅黑", 24f, Color.FromArgb(0xff, Color.FloralWhite), TextAlignment.Center, ParagraphAlignment.Center, SharpDX.DirectWrite.FontStyle.Normal, FontWeight.ExtraBlack);
                    this.DrawTextLayout("( 升调: Shift, 乐器: 鼠标所在小节, 弹奏: F1/3/E/D/C -> F7/9/O/L/. )", new RawVector2(((base.Width - num37) + num40) + (num37 / 2f), (num39 - 1f) + 16f), "微软雅黑", 12f, Color.FromArgb(0xff, Color.FloralWhite), TextAlignment.Center, ParagraphAlignment.Center, SharpDX.DirectWrite.FontStyle.Normal, FontWeight.ExtraBlack);
                }
                else if (this.IsMusicPaused)
                {
                    ef4 = new RawRectangleF((base.Width - num33) + num36, num35 - (num34 / 2f), base.Width + num36, num35 + (num34 / 2f));
                    this.FillRoundedRectangle(ef4, 4f, Color.FromArgb(80, Color.DimGray));
                    this.DrawRoundedRectangle(ef4, 4f, Color.FromArgb(0x80, Color.LightGray), 2f);
                    this.DrawTextLayout("演奏暂停", new RawVector2(((base.Width - num33) + num36) + (num33 / 2f), num35 - 1f), "微软雅黑", 12f, Color.LightYellow, TextAlignment.Center, ParagraphAlignment.Center, SharpDX.DirectWrite.FontStyle.Normal, FontWeight.ExtraBlack);
                    num36 -= num33 + 4f;
                }
                int alpha = this.IsLoading ? 0xff : this.LoadingFilenameAlpha;
                if (alpha > 0)
                {
                    num37 = base.Width - 4;
                    num38 = 64f;
                    num39 = 200f;
                    num40 = -2f;
                    ef4 = new RawRectangleF((base.Width - num37) + num40, num39 - (num38 / 2f), base.Width + num40, num39 + (num38 / 2f));
                    this.FillRoundedRectangle(ef4, 4f, Color.FromArgb((220 * alpha) / 0xff, Color.DimGray));
                    this.DrawRoundedRectangle(ef4, 4f, Color.FromArgb((0xe4 * alpha) / 0xff, Color.LightGray), 2f);
                    this.DrawTextLayout("Loading " + this.LoadingFilename, new RawVector2(((base.Width - num37) + num40) + (num37 / 2f), num39 - 1f), "微软雅黑", 24f, Color.FromArgb(alpha, Color.FloralWhite), TextAlignment.Center, ParagraphAlignment.Center, SharpDX.DirectWrite.FontStyle.Normal, FontWeight.ExtraBlack);
                }
                int commonMessageAlpha = this.CommonMessageAlpha;
                if (!((commonMessageAlpha <= 0) || string.IsNullOrEmpty(this.CommonMessage)))
                {
                    num37 = base.Width - 4;
                    num38 = 64f;
                    num39 = 280f;
                    num40 = -2f;
                    ef4 = new RawRectangleF((base.Width - num37) + num40, num39 - (num38 / 2f), base.Width + num40, num39 + (num38 / 2f));
                    this.FillRoundedRectangle(ef4, 4f, Color.FromArgb((220 * commonMessageAlpha) / 0xff, Color.DimGray));
                    this.DrawRoundedRectangle(ef4, 4f, Color.FromArgb((0xe4 * commonMessageAlpha) / 0xff, Color.LightGray), 2f);
                    this.DrawTextLayout(this.CommonMessage, new RawVector2(((base.Width - num37) + num40) + (num37 / 2f), num39 - 1f), "微软雅黑", 24f, Color.FromArgb(commonMessageAlpha, Color.FloralWhite), TextAlignment.Center, ParagraphAlignment.Center, SharpDX.DirectWrite.FontStyle.Normal, FontWeight.ExtraBlack);
                }
                RippleAniManager.Draw(this);
                PlayingStickAniManager.Draw(this);
                MouseCircleProgressManager.Draw(this);
                MouseDragareaManager.Draw(this);
                if (EnableFps && (this._updateElapsedMsList.Count >= 10))
                {
                    float num44 = ((float) this._updateElapsedMsList.ToArray().Sum()) / 1000f;
                    float num45 = (((float) this._updateElapsedMsList.Count) / num44) + 4f;
                    this.DrawTextLayout($"Fps:{num45.Clamp(0f, 60f):F1}", new RawVector2((float) (base.Width - 8), -1f), "Consolas", 10f, Color.Yellow, TextAlignment.Trailing, ParagraphAlignment.Near, SharpDX.DirectWrite.FontStyle.Normal, FontWeight.Bold);
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
            }
            try
            {
                this.RenderTarget2D.EndDraw();
            }
            catch (SharpDXException exception2)
            {
                if (exception2.Descriptor.NativeApiCode == "D2DERR_RECREATE_TARGET")
                {
                    this.InitializeDirect2DAndDirectWrite();
                }
            }
        }

        public void RenderControl_Update(long elapsedMs)
        {
            int num;
            float num4;
            int top;
            RawRectangleF ef3;
            Point point = base.PointToClient(Control.MousePosition);
            RawRectangleF ef = new RawRectangleF(0f, 0f, (float) base.Width, (float) base.Height);
            this._scrollX = this._scrollX.Clamp(0, this.Notation.DrawingWidth - McMeasure.MeasureHeadWidth);
            this._scrollXSmooth = this._scrollXSmooth.Lerp((float) this._scrollX, (7f * elapsedMs) / 1000f);
            McUtility.RedrawMeasureInRedrawingList();
            McUtility.RedrawMeasureInDeferredRedrawingList(true);
            for (num = 0; num < this._drawingMeasures.Length; num++)
            {
                if (this._drawingMeasures[num] == null)
                {
                    this._drawingMeasures[num] = new List<McMeasure>();
                }
                else
                {
                    this._drawingMeasures[num].Clear();
                }
            }
            McRegularTrack[] regularTracks = this.Notation.RegularTracks;
            McMeasure measure = null;
            RawRectangleF ef2 = new RawRectangleF();
            for (int i = 0; i < regularTracks.Length; i++)
            {
                McNotePack pack = null;
                McRegularTrack track = regularTracks[i];
                foreach (McMeasure measure2 in track.MeasureArray)
                {
                    bool isDisplay = measure2.IsDisplay;
                    measure2.IsDisplay = false;
                    foreach (McNotePack pack2 in measure2.NotePacks)
                    {
                        McNotePack.NpTemporaryInfo temporaryInfo = pack2.TemporaryInfo;
                        temporaryInfo.LinkedInTieNote = null;
                        temporaryInfo.LinkedOutTieNote = null;
                        switch (pack2.TieType)
                        {
                            case McNotePack.TieTypes.Start:
                                pack = pack2;
                                break;

                            case McNotePack.TieTypes.End:
                                if (pack != null)
                                {
                                    pack.TemporaryInfo.LinkedOutTieNote = pack2;
                                    temporaryInfo.LinkedInTieNote = pack;
                                }
                                pack = null;
                                break;

                            case McNotePack.TieTypes.Both:
                                if (pack != null)
                                {
                                    pack.TemporaryInfo.LinkedOutTieNote = pack2;
                                    temporaryInfo.LinkedInTieNote = pack;
                                }
                                pack = pack2;
                                break;
                        }
                        pack2.TemporaryInfo.UpdateTieTemporaryInfoData();
                    }
                    if (measure2.BitmapCache != null)
                    {
                        int width = measure2.TemporaryInfo.Width;
                        num4 = ((measure2.TemporaryInfo.RelativeX + McNotation.Margin.Left) + McRegularTrack.Margin.Left) - this._scrollXSmooth;
                        if ((((num4 >= ef.Left) && ((num4 + width) <= ef.Right)) || ((num4 < ef.Left) && ((num4 + width) > ef.Left))) || (((num4 + width) > ef.Right) && (num4 < ef.Right)))
                        {
                            foreach (McNotePack pack2 in measure2.NotePacks)
                            {
                                if (((pack2.TieType != McNotePack.TieTypes.None) || (pack2.TemporaryInfo.TripletNotePacks != null)) || pack2.Triplet)
                                {
                                    float measureLineRelativeYByLineValue = 0f;
                                    if (pack2.TemporaryInfo.IsFlipVerticalStemVoted)
                                    {
                                        if (pack2.IsRest)
                                        {
                                            measureLineRelativeYByLineValue = McPitch.GetMeasureLineRelativeYByLineValue(measure2.ClefType, 0x2d);
                                        }
                                        else
                                        {
                                            measureLineRelativeYByLineValue = pack2.GetPitch(pack2.HighestPitchValue).MeasureLineRelativeY - 12f;
                                        }
                                    }
                                    else if (pack2.IsRest)
                                    {
                                        measureLineRelativeYByLineValue = McPitch.GetMeasureFirstLineRelativeY(measure2.ClefType);
                                    }
                                    else
                                    {
                                        measureLineRelativeYByLineValue = pack2.GetPitch(pack2.LowestPitchValue).MeasureLineRelativeY + 10f;
                                    }
                                    pack2.TemporaryInfo.TieMarkerRelativeY = measureLineRelativeYByLineValue;
                                }
                            }
                            measure2.IsDisplay = true;
                            if (!isDisplay)
                            {
                                McUtility.AppendDeferredRedrawingMeasure(measure2);
                            }
                            this._drawingMeasures[i].Add(measure2);
                            measure2.ClearTemporaryNotePacks();
                            top = ((((McRegularTrack.Margin.Bottom + McRegularTrack.Margin.Top) + McMeasure.Height) * i) + McNotation.Margin.Top) + McRegularTrack.Margin.Top;
                            ef3 = new RawRectangleF(num4, (float) top, num4 + measure2.TemporaryInfo.Width, (float) (top + McMeasure.Height));
                            if ((((point.X > ef3.Left) && (point.X < ef3.Right)) && (point.Y > ef3.Top)) && (point.Y < ef3.Bottom))
                            {
                                measure = measure2;
                                ef2 = ef3;
                            }
                        }
                    }
                }
            }
            if (measure != null)
            {
                num4 = ((measure.TemporaryInfo.RelativeX + McNotation.Margin.Left) + McRegularTrack.Margin.Left) - this._scrollXSmooth;
                top = measure.Top;
                ef3 = new RawRectangleF(num4, (float) top, num4 + measure.TemporaryInfo.Width, (float) (top + McMeasure.Height));
                if ((((point.X <= ef3.Left) || (point.X >= ef3.Right)) || (point.Y <= ef3.Top)) || (point.Y >= ef3.Bottom))
                {
                    measure = null;
                    McHoverInfo.Clear();
                }
                else
                {
                    McHoverInfo.HoveringMeasure = measure;
                }
            }
            else
            {
                McHoverInfo.Clear();
            }
            McHoverInfo.HoveringNotePack = null;
            if (measure != null)
            {
                float num10;
                float num11;
                Padding padding = new Padding(0x12, 10, 0x12, 11);
                Point point2 = new Point(point.X - ((int) ef2.Left), point.Y - ((int) ef2.Top));
                float num7 = 999999f;
                float num8 = -1f;
                num7 = 99999f;
                for (num = McPitch.PitchMin; num <= McPitch.PitchMax; num++)
                {
                    if (McPitch.FromNaturalPitchValue(num, McPitch.PitchTypes.Temporary) != null)
                    {
                        float measureLineRelativeY = McPitch.GetMeasureLineRelativeY(measure.ClefType, num);
                        num10 = Math.Abs((float) (point2.Y - measureLineRelativeY));
                        if (num10 <= num7)
                        {
                            McHoverInfo.HoveringInsertPitchValue = num;
                            num7 = num10;
                            num8 = measureLineRelativeY;
                        }
                    }
                }
                bool flag3 = true;
                McNotePack pack3 = null;
                num7 = 999999f;
                foreach (McNotePack pack2 in measure.NotePacks)
                {
                    pack2.TemporaryInfo.RelativeXSmooth = (pack2.TemporaryInfo.RelativeXSmooth < 1f) ? (pack2.TemporaryInfo.RelativeX - 28f) : pack2.TemporaryInfo.RelativeXSmooth.Lerp(pack2.TemporaryInfo.RelativeX, 0.2f);
                    num11 = pack2.TemporaryInfo.RelativeX + 8f;
                    num10 = Math.Abs((float) (point2.X - num11));
                    if (num10 < num7)
                    {
                        num7 = num10;
                        pack3 = pack2;
                        flag3 = point2.X < num11;
                    }
                }
                if (pack3 == null)
                {
                    McHoverInfo.HoveringInsertNotePackIndex = -1;
                }
                else
                {
                    int index = pack3.Index;
                    num11 = pack3.TemporaryInfo.RelativeX + 8f;
                    if (((point2.X <= num11) && ((num11 - point2.X) < padding.Left)) || ((point2.X > num11) && ((point2.X - num11) < padding.Right)))
                    {
                        if (num8 > 0f)
                        {
                            McHoverInfo.HoveringNotePack = pack3;
                            int validHoveringInsertPitchIndex = McHoverInfo.ValidHoveringInsertPitchIndex;
                            if (!((((validHoveringInsertPitchIndex <= 0) || this.IsMusicPlaying) || EnableFreePlayMode) || MouseDragareaManager.IsStarted))
                            {
                                pack3.MarkPitch(validHoveringInsertPitchIndex, McPitch.PitchTypes.Temporary);
                            }
                        }
                    }
                    else
                    {
                        McHoverInfo.HoveringInsertNotePackIndex = flag3 ? index : (index + 1);
                    }
                }
            }
            if (measure != null)
            {
                McUtility.AppendRedrawingMeasure(measure);
            }
            MouseCircleProgressManager.Update(elapsedMs);
            MouseDragareaManager.Update(elapsedMs);
            if (!(this.RenderCanvas.Focused || this.IsMusicPlaying))
            {
                McHoverInfo.Clear();
            }
        }

        public McNotation Reset(bool forceCtrateFirstCol, bool saveNotice)
        {
            if ((saveNotice && (this.Notation != null)) && McUtility.IsModified)
            {
                switch (RadMessageBox.Show(this, "还有未保存的内容，是否保存？", "重置古剑奇谭网络版乐谱", MessageBoxButtons.YesNoCancel, RadMessageIcon.Exclamation))
                {
                    case DialogResult.Cancel:
                        return this.Notation;

                    case DialogResult.Yes:
                        this.SaveMusicalNotation();
                        break;
                }
            }
            if (this.Notation != null)
            {
                McUtility.ClearModifiedState();
                this.Notation.ClearBitmapCache();
            }
            this.StopMusic();
            this.Notation = new McNotation(this);
            if (forceCtrateFirstCol)
            {
                this.Notation.InsertMeasureAligned(null);
                foreach (McRegularTrack track in this.Notation.RegularTracks)
                {
                    track.ResetMeasureClefTypeToDefault();
                    track.ResetMeasureInstrumentTypeToDefault();
                    track.ResetMeasureKeySignatureToDefault();
                    track.ResetMeasureVolumeCurveToDefault();
                    track.ResetMeasureVolumeToDefault();
                }
                this.Notation.BeatDurationType = McNotePack.DurationTypes.Quarter;
                this.Notation.BeatsPerMeasure = 4;
                this.Notation.ResetMeasureBeatsPerMinuteToDefault();
            }
            this.Notation.TemporaryInfo.ReorganizeAllDurationStamps();
            McUtility.ClearModifiedState();
            McUtility.ClearDeferredRedrawingMeasure();
            McUtility.ClearRedrawingMeasure();
            return this.Notation;
        }

        public bool SaveMusicalNotation() => 
            this.SaveMusicalNotation(null);

        public bool SaveMusicalNotation(string filename)
        {
            int current;
            IEnumerator<int> enumerator;
            this.PauseMusic();
            if (filename == null)
            {
                filename = this.LastAccessedFilename ?? "";
            }
            if (string.IsNullOrEmpty(filename) || !File.Exists(filename))
            {
                if (this._saveFileDialog.ShowDialog() != DialogResult.OK)
                {
                    return false;
                }
                filename = this._saveFileDialog.FileName;
            }
            int measuresCount = this.Notation.RegularTracks.First<McRegularTrack>().MeasuresCount;
            StringBuilder builder = new StringBuilder();
            string str = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            builder.AppendLine($"Version ='{str}'");
            builder.AppendLine("Notation = {");
            builder.AppendLine($"	Version ='{str}',");
            builder.AppendLine($"	NotationName = '{this.Notation.Name}',");
            builder.AppendLine($"	NotationAuthor = '{this.Notation.Author}',");
            builder.AppendLine($"	NotationTranslater = '{this.Notation.Translater}',");
            builder.AppendLine($"	NotationCreator = '{this.Notation.Creator}',");
            builder.AppendLine($"	Volume = {this.Notation.Volume},");
            builder.AppendLine($"	BeatsPerMeasure = {this.Notation.BeatsPerMeasure} ,");
            builder.AppendLine($"	BeatDurationType = '{this.Notation.BeatDurationType}',");
            builder.AppendLine($"	NumberedKeySignature = '{this.Notation.NumberedKeySignature}',");
            builder.AppendLine("\tMeasureBeatsPerMinuteMap = {");
            using (enumerator = this.Notation.MeasureBeatsPerMinuteMap.Keys.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    current = enumerator.Current;
                    int num3 = this.Notation.MeasureBeatsPerMinuteMap[current];
                    builder.AppendLine($"		{{ {current}, {num3} }},");
                }
            }
            builder.AppendLine("\t},");
            builder.AppendLine($"	MeasureAlignedCount = {measuresCount},");
            builder.AppendLine("}");
            builder.AppendLine("Notation.RegularTracks = {");
            for (int i = 0; i < 3; i++)
            {
                McRegularTrack track = this.Notation.RegularTracks[i];
                builder.AppendLine($"	[{i}] = {{");
                builder.AppendLine("\t\tMeasureKeySignatureMap = {");
                using (enumerator = track.MeasureKeySignatureMap.Keys.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        current = enumerator.Current;
                        int num5 = track.MeasureKeySignatureMap[current];
                        builder.AppendLine($"			{{ {current}, {num5} }},");
                    }
                }
                builder.AppendLine("\t\t},");
                builder.AppendLine("\t\tMeasureClefTypeMap = {");
                using (enumerator = track.MeasureClefTypeMap.Keys.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        current = enumerator.Current;
                        McMeasure.ClefTypes types = track.MeasureClefTypeMap[current];
                        builder.AppendLine($"			{{ {current}, '{types}' }},");
                    }
                }
                builder.AppendLine("\t\t},");
                builder.AppendLine("\t\tMeasureInstrumentTypeMap = {");
                using (enumerator = track.MeasureInstrumentTypeMap.Keys.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        current = enumerator.Current;
                        McMeasure.InstrumentTypes types2 = track.MeasureInstrumentTypeMap[current];
                        builder.AppendLine($"			{{ {current}, '{types2}' }},");
                    }
                }
                builder.AppendLine("\t\t},");
                builder.AppendLine("\t\tMeasureVolumeCurveMap = {");
                using (enumerator = track.MeasureVolumeCurveMap.Keys.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        current = enumerator.Current;
                        VolumeCurve curve = track.MeasureVolumeCurveMap[current];
                        string str2 = "";
                        for (int j = 0; j < curve.VolumePoint.Length; j++)
                        {
                            str2 = str2 + curve.VolumePoint[j] + ", ";
                        }
                        builder.AppendLine($"			{{ {current}, {{{str2}}} }},");
                    }
                }
                builder.AppendLine("\t\t},");
                builder.AppendLine("\t\tMeasureVolumeMap = {");
                using (enumerator = track.MeasureVolumeMap.Keys.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        current = enumerator.Current;
                        float num7 = track.MeasureVolumeMap[current];
                        builder.AppendLine($"			{{ {current}, {num7} }},");
                    }
                }
                builder.AppendLine("\t\t},");
                McMeasure[] measureArray = track.MeasureArray;
                for (current = 0; current < measureArray.Length; current++)
                {
                    McMeasure measure = measureArray[current];
                    McNotePack[] notePacks = measure.NotePacks;
                    int length = notePacks.Length;
                    builder.AppendLine($"		[{current}] = {{");
                    if (i == 0)
                    {
                        builder.AppendLine($"			DurationStampMax = {measure.TemporaryInfo.DurationStampMax},");
                    }
                    builder.AppendLine($"			NotePackCount = {length},");
                    for (int k = 0; k < length; k++)
                    {
                        McNotePack notePack = notePacks[k];
                        int num10 = notePack.ValidPitchArray.Length;
                        builder.AppendLine($"			[{k}] = {{");
                        if (notePack.IsRestRaw)
                        {
                            builder.AppendLine($"				IsRest = {notePack.IsRestRaw.ToString().ToLower()},");
                        }
                        if (notePack.IsDotted)
                        {
                            builder.AppendLine($"				IsDotted = {notePack.IsDotted.ToString().ToLower()},");
                        }
                        if (notePack.TieType != McNotePack.TieTypes.None)
                        {
                            builder.AppendLine($"				TieType ='{notePack.TieType}',");
                        }
                        if (notePack.Triplet)
                        {
                            builder.AppendLine($"				Triplet = {notePack.Triplet.ToString().ToLower()},");
                        }
                        if (notePack.DurationTypeRaw != McNotePack.DurationTypes.Quarter)
                        {
                            builder.AppendLine($"				DurationType ='{notePack.DurationTypeRaw}',");
                        }
                        if (notePack.ArpeggioModeRaw != Arpeggio.ArpeggioTypes.None)
                        {
                            builder.AppendLine($"				ArpeggioMode ='{notePack.ArpeggioModeRaw}',");
                        }
                        builder.AppendLine($"				StampIndex = {notePack.TemporaryInfo.StampIndex},");
                        builder.AppendLine($"				PlayingDurationTimeMs = {notePack.TemporaryInfo.PlayingDurationTimeMs},");
                        builder.AppendLine($"				ClassicPitchSignCount = {num10},");
                        if (num10 > 0)
                        {
                            int num11 = Arpeggio.CalculateInterval(notePack, 2f);
                            Arpeggio.ArpeggioTypes arpeggioMode = notePack.ArpeggioMode;
                            int num12 = 0;
                            int num13 = notePack.ValidPitchCount.Clamp(4, 8);
                            builder.AppendLine("\t\t\t\tClassicPitchSign = {");
                            foreach (McPitch pitch in notePack.ValidPitchArray)
                            {
                                if (pitch.PitchType != McPitch.PitchTypes.Enabled)
                                {
                                    continue;
                                }
                                int num14 = 0;
                                if ((num11 > 0x10) && (arpeggioMode != Arpeggio.ArpeggioTypes.None))
                                {
                                    switch (arpeggioMode)
                                    {
                                        case Arpeggio.ArpeggioTypes.Upward:
                                            num14 = num12 * num11;
                                            break;

                                        case Arpeggio.ArpeggioTypes.Downward:
                                            num14 = ((num13 - num12) - 1) * num11;
                                            break;
                                    }
                                    num12++;
                                }
                                if (num14 > 0x10)
                                {
                                    builder.AppendLine($"					[{pitch.Value}] = {{ NumberedSign = {pitch.ToneValue}, PlayingPitchIndex = {pitch.AlterantValue}, AlterantType = '{pitch.AlterantType}', RawAlterantType = '{pitch.RawAlterantType}', Volume = {pitch.Volume:F2}, ArpeggioOffsetTimeMs = {num14}, }},");
                                }
                                else
                                {
                                    builder.AppendLine($"					[{pitch.Value}] = {{ NumberedSign = {pitch.ToneValue}, PlayingPitchIndex = {pitch.AlterantValue}, AlterantType = '{pitch.AlterantType}', RawAlterantType = '{pitch.RawAlterantType}', Volume = {pitch.Volume:F2}, }},");
                                }
                            }
                            builder.AppendLine("\t\t\t\t},");
                        }
                        builder.AppendLine("\t\t\t},");
                    }
                    builder.AppendLine("\t\t},");
                }
                builder.AppendLine("\t},");
            }
            builder.AppendLine("}");
            Encoding encoding = new UTF8Encoding(false);
            string str3 = builder.ToString();
            if (_isCompressEnabled)
            {
                str3 = str3.CompressString(encoding);
            }
            if (string.IsNullOrEmpty(str3))
            {
                return false;
            }
            if (File.Exists(filename))
            {
                FileInfo info = new FileInfo(filename) {
                    IsReadOnly = false
                };
            }
            File.WriteAllText(filename, str3, encoding);
            this.LastAccessedFilename = filename;
            McUtility.ClearModifiedState();
            if (this.OnMusicCanvasFileSaved != null)
            {
                this.OnMusicCanvasFileSaved(this, new MusicCanvasIoEventArgs(filename));
            }
            if ((this.Notation.Name == "？？？？") && (RadMessageBox.Show("乐谱名称没有设置，请在编辑状态按下 1 或者 ESC 进入设置界面填写乐谱名称等信息。\n◇ 未设置乐谱名称会导致上传此乐谱到服务器后，显示为《？？？？》，且会覆盖已有的名为《？？？？》的乐谱。\n◇ 有效乐谱可通过快捷键 F1 上传到服务器并使用（上传时需要同时开启游戏客户端并登陆进入游戏场景）。\n◇ 只有上传到服务器的乐谱才能被游戏中的琴言系统使用。\n\n是否现在打开设置界面以填写乐谱名称等信息（修改后需要再次保存曲谱）。", "乐谱名缺失警告", MessageBoxButtons.YesNo) == DialogResult.Yes))
            {
                this.MouseCircleProgress_Callback(null, 0x33, MouseCircleProgressManager.ExitTypes.Finished, "");
            }
            this.ShowCommonMessage("保存乐谱完成");
            return true;
        }

        [DllImport("user32.dll")]
        private static extern int SetCursorPos(int x, int y);
        public void ShowCommonMessage(string message)
        {
            this._commonMessageAlpha = 0xff;
            this._commonMessage = message;
        }

        public void StopAudioPlayer()
        {
            List<string> list = new List<string>();
            foreach (string str in this._sourceVoices.Keys)
            {
                AudioSource audioSource = this._sourceVoices[str];
                this.DisposeAudioSource(audioSource);
                list.Add(str);
            }
            foreach (string str in list)
            {
                this._sourceVoices.Remove(str);
            }
            this._masteringVoice.Dispose();
            this._xaudio2.Dispose();
        }

        public void StopMusic()
        {
            this._playingMeasureIndex = 0;
            this._playingHemidemisemiquaverPtr = -1;
            this._playingLoopingAccumulatedTimeMs = 0.0;
            this._playingHemidemisemiquaverTimeMs = 8;
            this._playingNextValidStampAbsX = 0f;
            this._playingNextValidStampAbsXSmooth = 0f;
            this._lastValidDurationStampPtr = 0;
            this.IsMusicPlaying = false;
        }

        protected override void WndProc(ref Message m)
        {
            if ((m.Msg != 0x105) || (m.WParam.ToInt32() != 0x12))
            {
                base.WndProc(ref m);
            }
        }

        public static MusicCanvasControl Canvas
        {
            [CompilerGenerated]
            get => 
                <Canvas>k__BackingField;
            [CompilerGenerated]
            private set
            {
                <Canvas>k__BackingField = value;
            }
        }

        public string CommonMessage =>
            this._commonMessage;

        public int CommonMessageAlpha
        {
            get
            {
                this._commonMessageAlpha = (this._commonMessageAlpha > 0xff) ? (this._commonMessageAlpha - 1).Clamp(0, 0xff) : (this._commonMessageAlpha - 2).Clamp(0, 0xff);
                return this._commonMessageAlpha;
            }
        }

        public SharpDX.Direct2D1.Factory Factory2D { get; private set; }

        public SharpDX.DirectWrite.Factory FactoryDWrite { get; private set; }

        public bool IsEditMode =>
            ((!this._isMusicPlaying && !EnableFreePlayMode) && !MouseDragareaManager.IsStarted);

        public bool IsLoading =>
            this._isLoading;

        public bool IsMusicPaused =>
            (!this.IsMusicPlaying && ((this._playingMeasureIndex != 0) || (this._playingHemidemisemiquaverPtr >= 0)));

        public bool IsMusicPlaying
        {
            get => 
                this._isMusicPlaying;
            set
            {
                this._isMusicPlaying = value;
            }
        }

        public string LastAccessedFilename
        {
            get => 
                this._lastAccessedFilename;
            set
            {
                if (this._lastAccessedFilename != value)
                {
                    this._lastAccessedFilename = value;
                    if (this.OnMusicCanvasAccessedFilenameChanged != null)
                    {
                        this.OnMusicCanvasAccessedFilenameChanged(this, new MusicCanvasIoEventArgs(this._lastAccessedFilename));
                    }
                }
            }
        }

        public string LoadingFilename =>
            this._loadingFilename;

        public int LoadingFilenameAlpha
        {
            get
            {
                this._loadingFilenameAlpha = (this._loadingFilenameAlpha - 8).Clamp(0, 0xff);
                return this._loadingFilenameAlpha;
            }
        }

        public SharpDX.Direct2D1.Bitmap Logo { get; private set; }

        public GujianOL_MusicBox.GujianOL_MusicBox MusicBox { get; private set; }

        public McNotation Notation { get; private set; }

        public GujianOL_MusicBox.PropertyPanel PropertyPanel { get; private set; }

        public RenderControl RenderCanvas { get; private set; }

        public WindowRenderTarget RenderTarget2D { get; private set; }

        public int ScrollX =>
            this._scrollX;

        public float ScrollXSmooth =>
            this._scrollXSmooth;

        public ImagingFactory WicImagingFactory { get; private set; }

        public static class CanvasRenderCache
        {
            private static Dictionary<int, Dictionary<McNotePack.DurationTypes, SharpDX.Direct2D1.Bitmap>> _bitmapCommonNoteCaches = new Dictionary<int, Dictionary<McNotePack.DurationTypes, SharpDX.Direct2D1.Bitmap>>();
            private static Dictionary<int, Dictionary<McNotePack.DurationTypes, SharpDX.Direct2D1.Bitmap>> _bitmapMiscNoteCaches = new Dictionary<int, Dictionary<McNotePack.DurationTypes, SharpDX.Direct2D1.Bitmap>>();
            private static Dictionary<int, Dictionary<McNotePack.DurationTypes, SharpDX.Direct2D1.Bitmap>> _bitmapRestNoteCaches = new Dictionary<int, Dictionary<McNotePack.DurationTypes, SharpDX.Direct2D1.Bitmap>>();
            private static SharpDX.Direct2D1.Factory _factory = new SharpDX.Direct2D1.Factory();
            private static RenderTargetProperties _renderTargetProperties = new RenderTargetProperties(RenderTargetType.Default, new SharpDX.Direct2D1.PixelFormat(SharpDX.DXGI.Format.Unknown, SharpDX.Direct2D1.AlphaMode.Unknown), 0f, 0f, RenderTargetUsage.None, FeatureLevel.Level_DEFAULT);
            private static SharpDX.WIC.Bitmap _wicBitmap = null;
            private static ImagingFactory _wicFactory = new ImagingFactory();
            private static WicRenderTarget _wicRenderTarget = null;
            private static SharpDX.DirectWrite.Factory _writeFactory = new SharpDX.DirectWrite.Factory();

            public static SharpDX.Direct2D1.Bitmap GetNoteBitmapCache(BitmapNoteTypes noteType, Color color, McNotePack.DurationTypes durType = 0x10)
            {
                Dictionary<int, Dictionary<McNotePack.DurationTypes, SharpDX.Direct2D1.Bitmap>> dictionary = null;
                switch (noteType)
                {
                    case BitmapNoteTypes.Common:
                        dictionary = _bitmapCommonNoteCaches;
                        break;

                    case BitmapNoteTypes.Rest:
                        dictionary = _bitmapRestNoteCaches;
                        break;

                    case BitmapNoteTypes.Misc:
                        dictionary = _bitmapMiscNoteCaches;
                        break;
                }
                if (dictionary == null)
                {
                    return null;
                }
                int key = color.ToArgb();
                Dictionary<McNotePack.DurationTypes, SharpDX.Direct2D1.Bitmap> dictionary2 = null;
                if (dictionary.ContainsKey(key))
                {
                    dictionary2 = dictionary[key];
                }
                else
                {
                    dictionary2 = new Dictionary<McNotePack.DurationTypes, SharpDX.Direct2D1.Bitmap>();
                    dictionary.Add(key, dictionary2);
                }
                if (dictionary2.ContainsKey(durType))
                {
                    return dictionary2[durType];
                }
                SharpDX.Direct2D1.Bitmap bitmap = null;
                switch (noteType)
                {
                    case BitmapNoteTypes.Common:
                    {
                        McNotePack.DurationTypes types2 = durType;
                        if (types2 > McNotePack.DurationTypes.Eighth)
                        {
                            if (types2 != McNotePack.DurationTypes.Quarter)
                            {
                                if (types2 == McNotePack.DurationTypes.Half)
                                {
                                    bitmap = McNotePack.RedrawCommonNoteBitmapCache(RenderTargetSource, color, false, true);
                                }
                                else if (types2 == McNotePack.DurationTypes.Whole)
                                {
                                    bitmap = McNotePack.RedrawCommonNoteBitmapCache(RenderTargetSource, color, true, true);
                                }
                                goto Label_0144;
                            }
                            break;
                        }
                        goto Label_0144;
                    }
                    case BitmapNoteTypes.Rest:
                        bitmap = McNotePack.RedrawRestNoteBitmapCache(RenderTargetSource, color, durType);
                        goto Label_0144;

                    case BitmapNoteTypes.Misc:
                        bitmap = McNotePack.RedrawMiscNoteBitmapCache(RenderTargetSource, color);
                        goto Label_0144;

                    default:
                        goto Label_0144;
                }
                bitmap = McNotePack.RedrawCommonNoteBitmapCache(RenderTargetSource, color, false, false);
            Label_0144:
                dictionary2.Add(durType, bitmap);
                return bitmap;
            }

            public static void InitializeCanvasRenderCache(RenderTarget renderTargetSource)
            {
                RenderTargetSource = renderTargetSource;
                _wicBitmap = new SharpDX.WIC.Bitmap(_wicFactory, 0x40, 0x40, SharpDX.WIC.PixelFormat.Format32bppPBGRA, BitmapCreateCacheOption.CacheOnDemand);
                _wicRenderTarget = new WicRenderTarget(_factory, _wicBitmap, _renderTargetProperties);
            }

            public static RenderTarget RenderTargetSource
            {
                [CompilerGenerated]
                get => 
                    <RenderTargetSource>k__BackingField;
                [CompilerGenerated]
                private set
                {
                    <RenderTargetSource>k__BackingField = value;
                }
            }

            public enum BitmapNoteTypes
            {
                Common,
                Rest,
                Misc
            }
        }

        public static class MouseCircleProgressManager
        {
            public static readonly Color DefaultFadingBgColor = Color.FromArgb(0x80, Color.Firebrick);
            public static readonly Color DefaultFadingBorderColor = Color.FromArgb(0xff, Color.DeepPink);
            public const int DisplayStartTimeMs = 100;
            public static readonly Point DrawingOffset = new Point(20, 20);
            public const int MaxFadingTimeMs = 400;
            public const int MinLifeTimeMs = 200;

            public static void Draw(MusicCanvasControl canvas)
            {
                if (((GoalLifeTimeMs > 0) && (ElapsedLifeTimeMs <= (GoalLifeTimeMs + 400))) && (ElapsedLifeTimeMs >= 100))
                {
                    float num3;
                    RawVector2 pos = new RawVector2(MousePos.X + DrawingOffset.X, MousePos.Y + DrawingOffset.Y);
                    if (IsFading)
                    {
                        int num = (ElapsedLifeTimeMs - GoalLifeTimeMs).Clamp(0, 400);
                        float num2 = ((float) (1f - ((float) (((float) num) / 400f)).Clamp(((float) 0f), ((float) 1f)))).Clamp((float) 0f, (float) 1f);
                        if (num2 > 0.001)
                        {
                            Color color = !FadingBgColor.HasValue ? Color.FromArgb((int) (num2 * DefaultFadingBgColor.A), DefaultFadingBgColor) : Color.FromArgb((int) (num2 * FadingBgColor.Value.A), FadingBgColor.Value);
                            Color color2 = !FadingBorderColor.HasValue ? Color.FromArgb((int) (num2 * DefaultFadingBorderColor.A), DefaultFadingBorderColor) : Color.FromArgb((int) (num2 * FadingBorderColor.Value.A), FadingBorderColor.Value);
                            canvas.FillCircle(pos, 8f * num2, color);
                            canvas.DrawCircle(pos, 8f * num2, color2, 2f);
                            if (!string.IsNullOrEmpty(HintText))
                            {
                                num3 = ((float) (1f - ((float) (((float) num) / 160f)).Clamp(((float) 0f), ((float) 1f)))).Clamp((float) 0f, (float) 1f);
                                canvas.DrawTextLayout(HintText, new RawVector2(pos.X + 13f, pos.Y + 3f), "微软雅黑", 8f, Color.FromArgb((int) (num3 * 255f), Color.Gainsboro), TextAlignment.Leading, ParagraphAlignment.Center, SharpDX.DirectWrite.FontStyle.Normal, FontWeight.Bold);
                            }
                        }
                    }
                    else
                    {
                        float num4 = ((float) ((ElapsedLifeTimeMs - 100f) / ((float) (GoalLifeTimeMs - 100)))).Clamp((float) 0f, (float) 1f);
                        canvas.FillCircle(pos, 8f, Color.FromArgb(0xff, 0x98, 0x98, 0x98));
                        canvas.DrawCircle(pos, 8f, Color.FromArgb(0xff, 0x40, 0x40, 0x40), 2f);
                        canvas.FillCircle(pos, 8f * num4, FillColor);
                        canvas.DrawSector(pos, 10f, FillColor, 0f, 360f * num4, 2f, true);
                        if (!string.IsNullOrEmpty(HintText))
                        {
                            num3 = ((float) (((float) (ElapsedLifeTimeMs - 100f)).Clamp(((float) 0f), ((float) 10000f)) / ((float) 300.Clamp(10, 0x2710)))).Clamp((float) 0f, (float) 1f);
                            canvas.DrawTextLayout(HintText, new RawVector2(pos.X + 13f, pos.Y + 3f), "微软雅黑", 8f, Color.FromArgb((int) (num3 * 255f), Color.Gainsboro), TextAlignment.Leading, ParagraphAlignment.Center, SharpDX.DirectWrite.FontStyle.Normal, FontWeight.Bold);
                        }
                    }
                }
            }

            public static bool Fade(Color? fadingBgColor = new Color?(), Color? fadingBorderColor = new Color?())
            {
                if (IsFading)
                {
                    return false;
                }
                if (ElapsedLifeTimeMs < 100)
                {
                    ElapsedLifeTimeMs = 0;
                    GoalLifeTimeMs = 0;
                    FadingBgColor = null;
                    FadingBorderColor = null;
                    _handled = false;
                    return true;
                }
                FadingBgColor = fadingBgColor;
                FadingBorderColor = fadingBorderColor;
                ElapsedLifeTimeMs = GoalLifeTimeMs + 1;
                _handled = true;
                return true;
            }

            public static void Start(MusicCanvasControl canvas, int goalLifeTimeMs, Keys key, CallbackDelegate callback, string hint = null)
            {
                if (!IsStarted)
                {
                    MouseButton = null;
                    KeyButton = null;
                    FillColor = Color.HotPink;
                    KeyButton = new Keys?(key);
                    Canvas = canvas;
                    StartMousePos = MousePos;
                    ElapsedLifeTimeMs = 0;
                    GoalLifeTimeMs = goalLifeTimeMs.Clamp(200, 0x2710);
                    FadingBgColor = null;
                    FadingBorderColor = null;
                    Callback = callback;
                    HintText = hint;
                    _handled = false;
                }
            }

            public static void Start(MusicCanvasControl canvas, int goalLifeTimeMs, MouseButtons mouseButton, CallbackDelegate callback, string hint = null)
            {
                if (!IsStarted && (callback != null))
                {
                    MouseButton = null;
                    KeyButton = null;
                    MouseButtons buttons = mouseButton;
                    if (buttons != MouseButtons.Left)
                    {
                        if (buttons != MouseButtons.Right)
                        {
                            if (buttons != MouseButtons.Middle)
                            {
                                return;
                            }
                            FillColor = Color.Gainsboro;
                        }
                        else
                        {
                            FillColor = Color.Yellow;
                        }
                    }
                    else
                    {
                        FillColor = Color.Turquoise;
                    }
                    MouseButton = new MouseButtons?(mouseButton);
                    Canvas = canvas;
                    Point point = canvas.PointToClient(Control.MousePosition);
                    StartMousePos = new RawVector2((float) point.X, (float) point.Y);
                    ElapsedLifeTimeMs = 0;
                    GoalLifeTimeMs = goalLifeTimeMs.Clamp(200, 0x2710);
                    FadingBgColor = null;
                    FadingBorderColor = null;
                    Callback = callback;
                    HintText = hint;
                    _handled = false;
                }
            }

            public static void Update(long elapsedMs)
            {
                if ((GoalLifeTimeMs > 0) && (ElapsedLifeTimeMs <= (GoalLifeTimeMs + 400)))
                {
                    Color? nullable;
                    ElapsedLifeTimeMs = (ElapsedLifeTimeMs + ((int) elapsedMs)).Clamp(0, 0xf4240);
                    if (MousePos.GetDistanceByRawVector2(StartMousePos) > 1f)
                    {
                        nullable = null;
                        if (Fade(nullable, null))
                        {
                            Callback(MouseButton, KeyButton, ExitTypes.CancelOnMouseMoved, HintText);
                        }
                    }
                    else
                    {
                        MouseButtons? nullable2;
                        MouseButtons? nullable3;
                        if (MouseButton.HasValue && ((((nullable2 = MouseButton) = nullable2.HasValue ? new MouseButtons?(Control.MouseButtons & ((MouseButtons) nullable2.GetValueOrDefault())) : null).GetValueOrDefault() != (nullable3 = MouseButton).GetValueOrDefault()) || (nullable2.HasValue != nullable3.HasValue)))
                        {
                            nullable = null;
                            if (Fade(nullable, null))
                            {
                                Callback(MouseButton, KeyButton, ExitTypes.CancelOnKeyReleased, HintText);
                            }
                        }
                        else if (!(!KeyButton.HasValue || KeyButton.Value.IsPressed()))
                        {
                            nullable = null;
                            if (Fade(nullable, null))
                            {
                                Callback(MouseButton, KeyButton, ExitTypes.CancelOnKeyReleased, HintText);
                            }
                        }
                        else if (!(_handled || (ElapsedLifeTimeMs < GoalLifeTimeMs)))
                        {
                            ElapsedLifeTimeMs = GoalLifeTimeMs;
                            Fade(new Color?(Color.FromArgb(0x80, Color.ForestGreen)), new Color?(Color.Green));
                            MusicCanvasControl.RippleAniManager.Append(MousePos.X + DrawingOffset.X, MousePos.Y + DrawingOffset.Y, Color.LawnGreen, 20, 0.66f, null);
                            Callback(MouseButton, KeyButton, ExitTypes.Finished, HintText);
                        }
                    }
                }
            }

            private static bool _handled
            {
                [CompilerGenerated]
                get => 
                    <_handled>k__BackingField;
                [CompilerGenerated]
                set
                {
                    <_handled>k__BackingField = value;
                }
            }

            public static CallbackDelegate Callback
            {
                [CompilerGenerated]
                get => 
                    <Callback>k__BackingField;
                [CompilerGenerated]
                private set
                {
                    <Callback>k__BackingField = value;
                }
            }

            public static MusicCanvasControl Canvas
            {
                [CompilerGenerated]
                get => 
                    <Canvas>k__BackingField;
                [CompilerGenerated]
                private set
                {
                    <Canvas>k__BackingField = value;
                }
            }

            public static int ElapsedLifeTimeMs
            {
                [CompilerGenerated]
                get => 
                    <ElapsedLifeTimeMs>k__BackingField;
                [CompilerGenerated]
                private set
                {
                    <ElapsedLifeTimeMs>k__BackingField = value;
                }
            }

            public static Color? FadingBgColor
            {
                [CompilerGenerated]
                get => 
                    <FadingBgColor>k__BackingField;
                [CompilerGenerated]
                private set
                {
                    <FadingBgColor>k__BackingField = value;
                }
            }

            public static Color? FadingBorderColor
            {
                [CompilerGenerated]
                get => 
                    <FadingBorderColor>k__BackingField;
                [CompilerGenerated]
                private set
                {
                    <FadingBorderColor>k__BackingField = value;
                }
            }

            public static Color FillColor
            {
                [CompilerGenerated]
                get => 
                    <FillColor>k__BackingField;
                [CompilerGenerated]
                private set
                {
                    <FillColor>k__BackingField = value;
                }
            }

            public static int GoalLifeTimeMs
            {
                [CompilerGenerated]
                get => 
                    <GoalLifeTimeMs>k__BackingField;
                [CompilerGenerated]
                private set
                {
                    <GoalLifeTimeMs>k__BackingField = value;
                }
            }

            public static string HintText
            {
                [CompilerGenerated]
                get => 
                    <HintText>k__BackingField;
                [CompilerGenerated]
                private set
                {
                    <HintText>k__BackingField = value;
                }
            }

            public static bool IsFading =>
                ((GoalLifeTimeMs > 0) && (ElapsedLifeTimeMs > GoalLifeTimeMs));

            public static bool IsStarted =>
                ((GoalLifeTimeMs > 0) && (ElapsedLifeTimeMs <= GoalLifeTimeMs));

            public static Keys? KeyButton
            {
                [CompilerGenerated]
                get => 
                    <KeyButton>k__BackingField;
                [CompilerGenerated]
                private set
                {
                    <KeyButton>k__BackingField = value;
                }
            }

            public static MouseButtons? MouseButton
            {
                [CompilerGenerated]
                get => 
                    <MouseButton>k__BackingField;
                [CompilerGenerated]
                private set
                {
                    <MouseButton>k__BackingField = value;
                }
            }

            public static RawVector2 MousePos
            {
                get
                {
                    Point point = Canvas.PointToClient(Control.MousePosition);
                    return new RawVector2((float) point.X, (float) point.Y);
                }
            }

            public static RawVector2 StartMousePos
            {
                [CompilerGenerated]
                get => 
                    <StartMousePos>k__BackingField;
                [CompilerGenerated]
                private set
                {
                    <StartMousePos>k__BackingField = value;
                }
            }

            public delegate void CallbackDelegate(MouseButtons? mouseButton, Keys? key, MusicCanvasControl.MouseCircleProgressManager.ExitTypes exitType, string hintText);

            public enum ExitTypes
            {
                CancelOnMouseMoved,
                CancelOnKeyReleased,
                Finished
            }
        }

        public static class MouseDragareaManager
        {
            public static float DashAniOffset = 0f;
            public static readonly Color DefaultCanvasRectColor = Color.FromArgb(220, Color.Yellow);
            public static readonly Color DefaultMouseRectColor = Color.FromArgb(0xff, Color.DeepSkyBlue);

            public static void ClearValidCanvasDragarea()
            {
                FadingLastValidCanvasDragareaAlpha = 1f;
                FadingLastValidCanvasDragarea = new RawRectangleF(LastValidCanvasDragarea.Left, LastValidCanvasDragarea.Top, LastValidCanvasDragarea.Right, LastValidCanvasDragarea.Bottom);
                LastValidCanvasDragarea = new RawRectangleF();
                LastPickedNotePacks = null;
                LastPickedRelativeMeasures = null;
            }

            public static void Draw(MusicCanvasControl canvas)
            {
                RawRectangleF ef;
                if (HasValidCanvasDragarea)
                {
                    ef = new RawRectangleF(LastValidCanvasDragarea.Left - canvas.ScrollXSmooth, LastValidCanvasDragarea.Top, LastValidCanvasDragarea.Right - canvas.ScrollXSmooth, LastValidCanvasDragarea.Bottom);
                    canvas.DrawRoundedRectangle(ef, 4f, Color.FromArgb(200, DefaultCanvasRectColor), 1f);
                    int num = (((Math.Max(canvas.Notation.DrawingWidth, canvas.Width) + McNotation.Margin.Left) + McRegularTrack.Margin.Left) + McNotation.Margin.Right) + McRegularTrack.Margin.Right;
                    int num2 = (int) ((canvas.Width * ef.GetWidth()) / ((float) num));
                    float num3 = LastValidCanvasDragarea.Left / ((float) num);
                    int num4 = (int) ((canvas.Width - num2) * num3);
                    RawRectangleF rectF = new RawRectangleF((float) num4, (float) (canvas.Height - 10), (float) (num4 + num2), (float) (canvas.Height - 1));
                    canvas.DrawRoundedRectangle(rectF, 0f, DefaultCanvasRectColor);
                }
                if (HasValidFadingCanvasDragarea)
                {
                    FadingLastValidCanvasDragareaAlpha = FadingLastValidCanvasDragareaAlpha.Lerp(0f, 0.15f);
                    ef = new RawRectangleF(FadingLastValidCanvasDragarea.Left - canvas.ScrollXSmooth, FadingLastValidCanvasDragarea.Top, FadingLastValidCanvasDragarea.Right - canvas.ScrollXSmooth, FadingLastValidCanvasDragarea.Bottom);
                    canvas.DrawRoundedRectangle(ef, 4f, Color.FromArgb((int) (FadingLastValidCanvasDragareaAlpha * 255f), DefaultCanvasRectColor), 1f);
                }
                if (IsStarted || IsFading)
                {
                    DashAniOffset = (DashAniOffset >= 1E+08f) ? 0f : (DashAniOffset + 0.1f);
                    StrokeStyleProperties properties = new StrokeStyleProperties {
                        DashStyle = DashStyle.DashDot,
                        DashOffset = DashAniOffset
                    };
                    StrokeStyle strokeStyle = new StrokeStyle(canvas.Factory2D, properties);
                    canvas.DrawRoundedRectangle(new RawRectangleF(MouseDragarea.Left - canvas.ScrollXSmooth, MouseDragarea.Top, MouseDragarea.Right - canvas.ScrollXSmooth, MouseDragarea.Bottom), 4f, Color.FromArgb((int) ((IsFading ? FadingAlpha : 1f) * 255f), DefaultMouseRectColor), 1f, strokeStyle, true);
                }
            }

            public static void Fade(ExitTypes exitTypes)
            {
                StartMousePos = null;
                IsFading = true;
                FadingAlpha = 1f;
                switch (exitTypes)
                {
                    case ExitTypes.CancelOnEsc:
                        if (Callback != null)
                        {
                            Callback(MouseDragarea, new RawRectangleF(), null, exitTypes);
                        }
                        break;

                    case ExitTypes.Cancel:
                        if (Callback != null)
                        {
                            Callback(MouseDragarea, new RawRectangleF(), null, exitTypes);
                        }
                        break;

                    case ExitTypes.Finished:
                    {
                        RawRectangleF ef;
                        Dictionary<int, List<Dictionary<McNotePack, bool>>> dictionary;
                        Dictionary<McMeasure, bool> dictionary2;
                        FadingLastValidCanvasDragareaAlpha = 1f;
                        FadingLastValidCanvasDragarea = new RawRectangleF(LastValidCanvasDragarea.Left, LastValidCanvasDragarea.Top, LastValidCanvasDragarea.Right, LastValidCanvasDragarea.Bottom);
                        RawRectangleF pickRect = new RawRectangleF(MouseDragarea.Left, MouseDragarea.Top, MouseDragarea.Right, MouseDragarea.Bottom);
                        if (!PickNotePacks(pickRect, out ef, out dictionary, out dictionary2))
                        {
                            RawRectangleF ef3 = new RawRectangleF();
                            LastValidCanvasDragarea = ef3;
                            if (Callback != null)
                            {
                                Callback(MouseDragarea, new RawRectangleF(), null, ExitTypes.Cancel);
                            }
                            break;
                        }
                        if (PickNotePacks(ef, out ef, out dictionary, out dictionary2))
                        {
                            LastValidCanvasDragarea = ef;
                            LastPickedNotePacks = dictionary;
                            LastPickedRelativeMeasures = dictionary2;
                            if (Callback != null)
                            {
                                Callback(MouseDragarea, ef, dictionary, exitTypes);
                            }
                        }
                        break;
                    }
                }
            }

            private static bool PickNotePacks(RawRectangleF pickRect, out RawRectangleF validCanvasDragarea, out Dictionary<int, List<Dictionary<McNotePack, bool>>> pickedNotePacks, out Dictionary<McMeasure, bool> pickedRelativeMeasures)
            {
                IsInPickingProgress = true;
                int measureCountAligned = Canvas.Notation.MeasureCountAligned;
                if (measureCountAligned <= 0)
                {
                    validCanvasDragarea = new RawRectangleF();
                    pickedNotePacks = null;
                    pickedRelativeMeasures = null;
                    IsInPickingProgress = false;
                    return false;
                }
                bool flag = false;
                pickedRelativeMeasures = new Dictionary<McMeasure, bool>();
                pickedNotePacks = new Dictionary<int, List<Dictionary<McNotePack, bool>>>();
                validCanvasDragarea = new RawRectangleF(1E+08f, 1E+08f, -1E+08f, -1E+08f);
                McRegularTrack track = Canvas.Notation.RegularTracks.First<McRegularTrack>();
                for (int i = 0; i < measureCountAligned; i++)
                {
                    McMeasure measure = track.GetMeasure(i);
                    float relativeX = measure.TemporaryInfo.RelativeX;
                    float num4 = relativeX + measure.TemporaryInfo.Width;
                    if ((relativeX >= pickRect.Left) || (num4 >= pickRect.Left))
                    {
                        if ((relativeX > pickRect.Right) && (num4 > pickRect.Right))
                        {
                            break;
                        }
                        McMeasure[] measuresAligned = measure.MeasuresAligned;
                        for (int j = 0; j < measuresAligned.Length; j++)
                        {
                            List<Dictionary<McNotePack, bool>> list = null;
                            if (pickedNotePacks.ContainsKey(j))
                            {
                                list = pickedNotePacks[j];
                            }
                            else
                            {
                                list = new List<Dictionary<McNotePack, bool>>();
                                pickedNotePacks.Add(j, list);
                            }
                            Dictionary<McNotePack, bool> item = new Dictionary<McNotePack, bool>();
                            list.Add(item);
                            McMeasure measure2 = measuresAligned[j];
                            float measureFirstLineRelativeY = McPitch.GetMeasureFirstLineRelativeY(measure2.ClefType);
                            float top = measure2.Top;
                            foreach (McNotePack pack in measure2.NotePacks)
                            {
                                float num8 = (pack.TemporaryInfo.RelativeX + measure2.TemporaryInfo.RelativeX) + 16f;
                                float num9 = pack.TemporaryInfo.RelativeHighestPitchY + top;
                                float num10 = pack.TemporaryInfo.RelativeLowestPitchY + top;
                                if (pack.IsRest)
                                {
                                    num9 = (measureFirstLineRelativeY - (McMeasure.StaveSpacing * 2)) + top;
                                    num10 = num9;
                                }
                                bool flag2 = (((num8 >= pickRect.Left) && (num8 <= pickRect.Right)) && ((pickRect.Top >= num9) || (pickRect.Bottom >= num9))) && ((pickRect.Top <= num10) || (pickRect.Bottom <= num10));
                                item.Add(pack, flag2);
                                if (flag2)
                                {
                                    flag = true;
                                    McMeasure parentMeasure = pack.ParentMeasure;
                                    if (!pickedRelativeMeasures.ContainsKey(parentMeasure))
                                    {
                                        pickedRelativeMeasures.Add(parentMeasure, true);
                                    }
                                    float num11 = ((pack.TemporaryInfo.StemEndY < 0.2f) ? 1E+08f : pack.TemporaryInfo.StemEndY) + top;
                                    float num12 = ((pack.TemporaryInfo.StemEndY < 0.2f) ? -1E+08f : pack.TemporaryInfo.StemEndY) + top;
                                    if (pack.IsRest)
                                    {
                                        num11 = (measureFirstLineRelativeY - (McMeasure.StaveSpacing * 4)) + top;
                                        num12 = measureFirstLineRelativeY + top;
                                    }
                                    validCanvasDragarea.Left = Math.Min(validCanvasDragarea.Left, num8);
                                    validCanvasDragarea.Top = Math.Min(Math.Min(validCanvasDragarea.Top, num9), num11);
                                    validCanvasDragarea.Right = Math.Max(validCanvasDragarea.Right, num8);
                                    validCanvasDragarea.Bottom = Math.Max(Math.Max(validCanvasDragarea.Bottom, num10), num12);
                                }
                            }
                        }
                    }
                }
                validCanvasDragarea.Left += -8f;
                validCanvasDragarea.Top += -16f;
                validCanvasDragarea.Right += 24f;
                validCanvasDragarea.Bottom += 16f;
                IsInPickingProgress = false;
                return flag;
            }

            public static void Start(MusicCanvasControl canvas, CallbackDelegate callback)
            {
                Canvas = canvas;
                RawVector2 mousePos = MousePos;
                Callback = callback;
                StartMousePos = new RawVector2(mousePos.X + canvas.ScrollXSmooth, mousePos.Y);
                MouseDragarea = new RawRectangleF();
                IsFading = false;
            }

            public static void Update(long elapsedMs)
            {
                if (IsStarted || IsFading)
                {
                    if (!Canvas.IsMouseLeftKeyPressed() && !IsFading)
                    {
                        if ((MouseDragarea.GetHeight() < 0.1f) || (MouseDragarea.GetWidth() < 0.1f))
                        {
                            Fade(ExitTypes.Abort);
                        }
                        else
                        {
                            Fade(IsStarted ? ExitTypes.Finished : ExitTypes.Abort);
                        }
                    }
                    else if (IsFading)
                    {
                        FadingAlpha -= (5f * elapsedMs) / 1000f;
                        if (FadingAlpha < 0.001f)
                        {
                            IsFading = false;
                        }
                    }
                    else if (IsStarted && StartMousePos.HasValue)
                    {
                        IsFading = false;
                        FadingAlpha = 1f;
                        RawVector2 mousePos = MousePos;
                        MouseDragarea = new RawRectangleF(Math.Min(StartMousePos.Value.X, mousePos.X + Canvas.ScrollXSmooth), Math.Min(StartMousePos.Value.Y, mousePos.Y), Math.Max(StartMousePos.Value.X, mousePos.X + Canvas.ScrollXSmooth), Math.Max(StartMousePos.Value.Y, mousePos.Y));
                    }
                }
            }

            public static CallbackDelegate Callback
            {
                [CompilerGenerated]
                get => 
                    <Callback>k__BackingField;
                [CompilerGenerated]
                private set
                {
                    <Callback>k__BackingField = value;
                }
            }

            public static MusicCanvasControl Canvas
            {
                [CompilerGenerated]
                get => 
                    <Canvas>k__BackingField;
                [CompilerGenerated]
                private set
                {
                    <Canvas>k__BackingField = value;
                }
            }

            public static float FadingAlpha
            {
                [CompilerGenerated]
                get => 
                    <FadingAlpha>k__BackingField;
                [CompilerGenerated]
                private set
                {
                    <FadingAlpha>k__BackingField = value;
                }
            }

            private static RawRectangleF FadingLastValidCanvasDragarea
            {
                [CompilerGenerated]
                get => 
                    <FadingLastValidCanvasDragarea>k__BackingField;
                [CompilerGenerated]
                set
                {
                    <FadingLastValidCanvasDragarea>k__BackingField = value;
                }
            }

            private static float FadingLastValidCanvasDragareaAlpha
            {
                [CompilerGenerated]
                get => 
                    <FadingLastValidCanvasDragareaAlpha>k__BackingField;
                [CompilerGenerated]
                set
                {
                    <FadingLastValidCanvasDragareaAlpha>k__BackingField = value;
                }
            }

            public static bool HasValidCanvasDragarea =>
                (LastValidCanvasDragarea.GetWidth() > 0.01f);

            private static bool HasValidFadingCanvasDragarea =>
                (FadingLastValidCanvasDragarea.GetWidth() > 0.01f);

            public static bool IsFading
            {
                [CompilerGenerated]
                get => 
                    <IsFading>k__BackingField;
                [CompilerGenerated]
                private set
                {
                    <IsFading>k__BackingField = value;
                }
            }

            public static bool IsInPickingProgress
            {
                [CompilerGenerated]
                get => 
                    <IsInPickingProgress>k__BackingField;
                [CompilerGenerated]
                private set
                {
                    <IsInPickingProgress>k__BackingField = value;
                }
            }

            public static bool IsStarted
            {
                get
                {
                    if (!StartMousePos.HasValue)
                    {
                        return false;
                    }
                    RawVector2 mousePos = MousePos;
                    if ((Math.Abs((float) ((mousePos.X + Canvas.ScrollXSmooth) - StartMousePos.Value.X)) < 0.01f) && (Math.Abs((float) (mousePos.Y - StartMousePos.Value.Y)) < 0.01f))
                    {
                        return false;
                    }
                    return true;
                }
            }

            public static Dictionary<int, List<Dictionary<McNotePack, bool>>> LastPickedNotePacks
            {
                [CompilerGenerated]
                get => 
                    <LastPickedNotePacks>k__BackingField;
                [CompilerGenerated]
                private set
                {
                    <LastPickedNotePacks>k__BackingField = value;
                }
            }

            public static Dictionary<McMeasure, bool> LastPickedRelativeMeasures
            {
                [CompilerGenerated]
                get => 
                    <LastPickedRelativeMeasures>k__BackingField;
                [CompilerGenerated]
                private set
                {
                    <LastPickedRelativeMeasures>k__BackingField = value;
                }
            }

            public static RawRectangleF LastValidCanvasDragarea
            {
                [CompilerGenerated]
                get => 
                    <LastValidCanvasDragarea>k__BackingField;
                [CompilerGenerated]
                private set
                {
                    <LastValidCanvasDragarea>k__BackingField = value;
                }
            }

            public static RawRectangleF MouseDragarea
            {
                [CompilerGenerated]
                get => 
                    <MouseDragarea>k__BackingField;
                [CompilerGenerated]
                private set
                {
                    <MouseDragarea>k__BackingField = value;
                }
            }

            public static RawVector2 MousePos
            {
                get
                {
                    Point point = Canvas.PointToClient(Control.MousePosition);
                    return new RawVector2((float) point.X, (float) point.Y);
                }
            }

            public static RawVector2? StartMousePos
            {
                [CompilerGenerated]
                get => 
                    <StartMousePos>k__BackingField;
                [CompilerGenerated]
                private set
                {
                    <StartMousePos>k__BackingField = value;
                }
            }

            public delegate void CallbackDelegate(RawRectangleF mouseDragarea, RawRectangleF validCanvasDragarea, Dictionary<int, List<Dictionary<McNotePack, bool>>> pickedNotePacks, MusicCanvasControl.MouseDragareaManager.ExitTypes exitType);

            public enum ExitTypes
            {
                Abort,
                CancelOnEsc,
                Cancel,
                Finished
            }
        }

        public static class PlayingStickAniManager
        {
            private static Dictionary<int, PlayingStickAni> PlayingStickAniList = new Dictionary<int, PlayingStickAni>();

            public static void Append(int x, Color color, int life = -1)
            {
                life = (life <= 0) ? 60 : life;
                if (!PlayingStickAniList.ContainsKey(x))
                {
                    PlayingStickAniList.Add(x, new PlayingStickAni(x, color, life));
                }
            }

            public static void Draw(MusicCanvasControl canvas)
            {
                foreach (int num in PlayingStickAniList.Keys.ToArray<int>())
                {
                    PlayingStickAni ani = PlayingStickAniList[num];
                    int num2 = ani.Decrease();
                    if (num2 >= 0)
                    {
                        float num3 = ((float) num2) / ((float) ani.OrgLife);
                        int num4 = (int) (32f * num3);
                        int num5 = ani.Color.R + ((int) ((0xff - ani.Color.R) * (1f - num3)));
                        int num6 = ani.Color.G + ((int) ((0xff - ani.Color.G) * (1f - num3)));
                        int num7 = ani.Color.B + ((int) ((0xff - ani.Color.B) * (1f - num3)));
                        Color color = Color.FromArgb(num4.Clamp(0, 0xff), num5.Clamp(0, 0xff), num6.Clamp(0, 0xff), num7.Clamp(0, 0xff));
                        float x = ani.X - MusicCanvasControl.Canvas.ScrollXSmooth;
                        float y = McNotation.Margin.Top + McRegularTrack.Margin.Top;
                        float num10 = ((((McRegularTrack.Margin.Bottom + McRegularTrack.Margin.Top) + McMeasure.Height) * 2) + y) + McMeasure.Height;
                        canvas.DrawLine(new RawVector2(x, y), new RawVector2(x, num10), color);
                    }
                    else
                    {
                        PlayingStickAniList.Remove(num);
                    }
                }
            }

            private class PlayingStickAni
            {
                private System.Drawing.Color _color = System.Drawing.Color.Transparent;
                private int _life;
                private int _orgLife;
                private int _x;
                public static int MaxLife = 100;

                public PlayingStickAni(int x, System.Drawing.Color color, int life)
                {
                    this._x = x;
                    this._color = color;
                    this._life = life.Clamp(10, MaxLife);
                    this._orgLife = this._life;
                }

                public int Decrease()
                {
                    this._life--;
                    return this._life;
                }

                public System.Drawing.Color Color =>
                    this._color;

                public int OrgLife =>
                    this._orgLife;

                public int X =>
                    this._x;
            }
        }

        public static class RippleAniManager
        {
            private static List<RippleAni> RippleAniList = new List<RippleAni>();

            public static void Append(float x, float y, Color color, int life = -1, float scale = 1f, McNotePack relatedNotaPack = null)
            {
                life = (life <= 0) ? 30 : life;
                RippleAniList.Add(new RippleAni(x, y, color, MusicCanvasControl.Canvas.ScrollXSmooth, life, scale, relatedNotaPack));
            }

            public static void Draw(MusicCanvasControl canvas)
            {
                foreach (RippleAni ani in RippleAniList.ToArray())
                {
                    int num = ani.Decrease();
                    if (num >= 0)
                    {
                        int num2 = ani.OrgLife - num;
                        float radius = ((float) num2) / ani.Scale;
                        Color color = Color.FromArgb((int) (255f * (((float) num) / ((float) ani.OrgLife))), ani.Color);
                        RawVector2 pos = new RawVector2(ani.Pos.X - (MusicCanvasControl.Canvas.ScrollXSmooth - ani.OrgScrollX), ani.Pos.Y);
                        canvas.DrawCircle(pos, radius, color, 1f);
                        if (ani.RelatedNotaPack != null)
                        {
                            McNotePack relatedNotaPack = ani.RelatedNotaPack;
                            SharpDX.Direct2D1.Bitmap bitmap = (relatedNotaPack.ParentMeasure.InstrumentType == McMeasure.InstrumentTypes.Misc) ? MusicCanvasControl.CanvasRenderCache.GetNoteBitmapCache(MusicCanvasControl.CanvasRenderCache.BitmapNoteTypes.Misc, Color.LawnGreen, relatedNotaPack.DurationType) : MusicCanvasControl.CanvasRenderCache.GetNoteBitmapCache(MusicCanvasControl.CanvasRenderCache.BitmapNoteTypes.Common, Color.LawnGreen, relatedNotaPack.DurationType);
                            float num4 = ((float) ((num - (((float) ani.OrgLife) / 2f)) / (((float) ani.OrgLife) / 2f))).Clamp((float) 0f, (float) 1f);
                            float num5 = ((float) (((float) num) / ((float) ani.OrgLife))).Clamp((float) 0f, (float) 1f);
                            canvas.DrawBitmap(bitmap, new RawVector2(pos.X - 8f, (pos.Y - 9f) + ((relatedNotaPack.TemporaryInfo.IsFlipVerticalStemVoted ? ((float) (-16)) : ((float) 0x10)) * num4)), 1f * num5);
                        }
                    }
                    else
                    {
                        RippleAniList.Remove(ani);
                    }
                }
            }

            private class RippleAni
            {
                private System.Drawing.Color _color = System.Drawing.Color.Transparent;
                private int _life;
                private int _orgLife;
                private float _orgScrollX;
                private RawVector2 _pos = new RawVector2();
                private McNotePack _relatedNotaPack = null;
                private float _scale;
                public static int MaxLife = 40;

                public RippleAni(float x, float y, System.Drawing.Color color, float orgScrollX, int life, float scale, McNotePack relatedNotaPack = null)
                {
                    this._pos.X = x;
                    this._pos.Y = y;
                    this._color = color;
                    this._life = life.Clamp(10, MaxLife);
                    this._orgLife = this._life;
                    this._orgScrollX = orgScrollX;
                    this._scale = scale.Clamp(0.25f, 2f);
                    this._relatedNotaPack = relatedNotaPack;
                }

                public int Decrease()
                {
                    this._life--;
                    return this._life;
                }

                public System.Drawing.Color Color =>
                    this._color;

                public int OrgLife =>
                    this._orgLife;

                public float OrgScrollX =>
                    this._orgScrollX;

                public RawVector2 Pos =>
                    this._pos;

                public McNotePack RelatedNotaPack =>
                    this._relatedNotaPack;

                public float Scale =>
                    this._scale;
            }
        }
    }
}

