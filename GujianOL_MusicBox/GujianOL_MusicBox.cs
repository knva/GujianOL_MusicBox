namespace GujianOL_MusicBox
{
    using GujianOL_MusicBox_Resources;
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Drawing;
    using System.IO;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using System.Windows.Forms;
    using Telerik.WinControls;
    using Telerik.WinControls.Themes;
    using Telerik.WinControls.UI;

    public class GujianOL_MusicBox : RadForm
    {
        private MusicBoxHelpWindow _helpWindow = null;
        private IniFile _iniFileManager = null;
        private IContainer components = null;
        private Panel panelMusicCanvas;
        private RadMenu radMenu;
        private RadMenuItem radMenuItemAbout;
        private RadMenuItem radMenuItemAddOrRemoveNotePitch;
        private RadMenuItem radMenuItemAddOrRemoveRest;
        private RadMenuItem radMenuItemArpeggio;
        private RadMenuItem radMenuItemChangePitchLevel;
        private RadMenuItem radMenuItemDecreaseDuration;
        private RadMenuItem radMenuItemEdit;
        private RadMenuItem radMenuItemEnableAutoScrollWhenPlayMusic;
        private RadMenuItem radMenuItemEnableDelaylessCommand;
        private RadMenuItem radMenuItemEnableFreePlayMode;
        private RadMenuItem radMenuItemEnableNumberedSignTip;
        private RadMenuItem radMenuItemEnablePlaySoundWhenInsert;
        private RadMenuItem radMenuItemEnableSnaplineWhenInsertNotePack;
        private RadMenuItem radMenuItemEnableStemAnimation;
        private RadMenuItem radMenuItemEsc;
        private RadMenuItem radMenuItemExit;
        private RadMenuItem radMenuItemFile;
        private RadMenuItem radMenuItemHelp;
        private RadMenuItem radMenuItemIncreaseDuration;
        private RadMenuItem radMenuItemInsertMeasure;
        private RadMenuItem radMenuItemLoad;
        private RadMenuItem radMenuItemMouseDragarea;
        private RadMenuItem radMenuItemNaturalSign;
        private RadMenuItem radMenuItemNew;
        private RadMenuItem radMenuItemNotationMenu;
        private RadMenuItem radMenuItemPasteSelection;
        private RadMenuItem radMenuItemPasteSelectionToNotation;
        private RadMenuItem radMenuItemRemoveMeasure;
        private RadMenuItem radMenuItemRemoveNotePack;
        private RadMenuItem radMenuItemRemoveSelection;
        private RadMenuItem radMenuItemSave;
        private RadMenuItem radMenuItemSaveAs;
        private RadMenuItem radMenuItemTieStart;
        private RadMenuItem radMenuItemTieStop;
        private RadMenuItem radMenuItemTieTriplet;
        private RadMenuItem radMenuItemToggleDelaylessCommandMode;
        private RadMenuItem radMenuItemToggleDotted;
        private RadMenuItem radMenuItemToggleFreePlayMode;
        private RadMenuItem radMenuItemTogglePlayMusic;
        private RadMenuItem radMenuItemTogglePlayMusicFull;
        private RadMenuItem radMenuItemTogglePlayMusicSpeedDown;
        private RadMenuItem radMenuItemTogglePlayMusicSpeedUp;
        private RadMenuItem radMenuItemToggleStaccato;
        private RadMenuItem radMenuItemTool;
        private RadMenuItem radMenuItemUploadNotation;
        private RadMenuSeparatorItem radMenuSeparatorItem10;
        private RadMenuSeparatorItem radMenuSeparatorItem11;
        private RadMenuSeparatorItem radMenuSeparatorItem3;
        private RadMenuSeparatorItem radMenuSeparatorItem4;
        private RadMenuSeparatorItem radMenuSeparatorItem5;
        private RadMenuSeparatorItem radMenuSeparatorItem8;
        private RadMenuSeparatorItem radMenuSeparatorItem9;
        private RadOffice2007ScreenTipElement radOffice2007ScreenTipElement1;
        private VisualStudio2012DarkTheme visualStudio2012DarkTheme;

        public GujianOL_MusicBox()
        {
            this.InitializeComponent();
            if (!Path.GetFileNameWithoutExtension(Process.GetCurrentProcess().MainModule.FileName).StartsWith("GujianOL_MusicBox"))
            {
                Environment.Exit(-1);
            }
            this.InitializeConfiguration();
            this.InitializeMusicCanvasControl();
            this._helpWindow = new MusicBoxHelpWindow();
            MusicBoxResources.ShowAboutWindow(Assembly.GetExecutingAssembly().GetName().Version.ToString());
            RadMessageBox.ThemeName = "VisualStudio2012Dark";
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && (this.components != null))
            {
                this.components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void GujianOL_MusicBox_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (this.CanvasControl != null)
            {
                if ((this.CanvasControl.Notation == null) || !McUtility.IsModified)
                {
                    goto Label_0069;
                }
                DialogResult result2 = RadMessageBox.Show(this, "还有未保存的内容，是否保存？", "关闭古剑奇谭网络版乐谱编辑工具", MessageBoxButtons.YesNoCancel, RadMessageIcon.Exclamation);
                if (result2 != DialogResult.Cancel)
                {
                    if (result2 == DialogResult.Yes)
                    {
                        this.CanvasControl.SaveMusicalNotation();
                    }
                    goto Label_0069;
                }
                e.Cancel = true;
            }
            return;
        Label_0069:
            this.CanvasControl.StopAudioPlayer();
            this.SaveConfiguration();
            Thread.Sleep(100);
        }

        private void InitializeComponent()
        {
            ComponentResourceManager manager = new ComponentResourceManager(typeof(GujianOL_MusicBox.GujianOL_MusicBox));
            this.visualStudio2012DarkTheme = new VisualStudio2012DarkTheme();
            this.panelMusicCanvas = new Panel();
            this.radOffice2007ScreenTipElement1 = new RadOffice2007ScreenTipElement();
            this.radMenuItemEdit = new RadMenuItem();
            this.radMenuItemMouseDragarea = new RadMenuItem();
            this.radMenuItemPasteSelection = new RadMenuItem();
            this.radMenuItemRemoveSelection = new RadMenuItem();
            this.radMenuSeparatorItem4 = new RadMenuSeparatorItem();
            this.radMenuItemInsertMeasure = new RadMenuItem();
            this.radMenuItemRemoveMeasure = new RadMenuItem();
            this.radMenuItemAddOrRemoveNotePitch = new RadMenuItem();
            this.radMenuItemRemoveNotePack = new RadMenuItem();
            this.radMenuItemAddOrRemoveRest = new RadMenuItem();
            this.radMenuSeparatorItem5 = new RadMenuSeparatorItem();
            this.radMenuSeparatorItem8 = new RadMenuSeparatorItem();
            this.radMenuItemIncreaseDuration = new RadMenuItem();
            this.radMenuItemDecreaseDuration = new RadMenuItem();
            this.radMenuSeparatorItem9 = new RadMenuSeparatorItem();
            this.radMenuItemEsc = new RadMenuItem();
            this.radMenuItemTogglePlayMusic = new RadMenuItem();
            this.radMenuItemTogglePlayMusicFull = new RadMenuItem();
            this.radMenuItemTogglePlayMusicSpeedUp = new RadMenuItem();
            this.radMenuItemTogglePlayMusicSpeedDown = new RadMenuItem();
            this.radMenuItemToggleFreePlayMode = new RadMenuItem();
            this.radMenuItemToggleDelaylessCommandMode = new RadMenuItem();
            this.radMenuItemNotationMenu = new RadMenuItem();
            this.radMenuItemToggleDotted = new RadMenuItem();
            this.radMenuItemToggleStaccato = new RadMenuItem();
            this.radMenuItemNaturalSign = new RadMenuItem();
            this.radMenuItemArpeggio = new RadMenuItem();
            this.radMenuItemPasteSelectionToNotation = new RadMenuItem();
            this.radMenuItemChangePitchLevel = new RadMenuItem();
            this.radMenuItemTieStart = new RadMenuItem();
            this.radMenuItemTieStop = new RadMenuItem();
            this.radMenuItemTieTriplet = new RadMenuItem();
            this.radMenuSeparatorItem10 = new RadMenuSeparatorItem();
            this.radMenuSeparatorItem11 = new RadMenuSeparatorItem();
            this.radMenuItemTool = new RadMenuItem();
            this.radMenuItemEnableFreePlayMode = new RadMenuItem();
            this.radMenuItemEnableNumberedSignTip = new RadMenuItem();
            this.radMenuItemEnablePlaySoundWhenInsert = new RadMenuItem();
            this.radMenuItemEnableAutoScrollWhenPlayMusic = new RadMenuItem();
            this.radMenuItemEnableSnaplineWhenInsertNotePack = new RadMenuItem();
            this.radMenuItemEnableDelaylessCommand = new RadMenuItem();
            this.radMenuItemEnableStemAnimation = new RadMenuItem();
            this.radMenuItemHelp = new RadMenuItem();
            this.radMenuItemAbout = new RadMenuItem();
            this.radMenuItemFile = new RadMenuItem();
            this.radMenuItemNew = new RadMenuItem();
            this.radMenuItemSave = new RadMenuItem();
            this.radMenuItemSaveAs = new RadMenuItem();
            this.radMenuItemUploadNotation = new RadMenuItem();
            this.radMenuItemLoad = new RadMenuItem();
            this.radMenuSeparatorItem3 = new RadMenuSeparatorItem();
            this.radMenuItemExit = new RadMenuItem();
            this.radMenu = new RadMenu();
            this.radMenu.BeginInit();
            this.BeginInit();
            base.SuspendLayout();
            this.panelMusicCanvas.Dock = DockStyle.Fill;
            this.panelMusicCanvas.Location = new Point(0, 0x16);
            this.panelMusicCanvas.Margin = new Padding(3, 0, 3, 3);
            this.panelMusicCanvas.Name = "panelMusicCanvas";
            this.panelMusicCanvas.Padding = new Padding(1, 0, 1, 0);
            this.panelMusicCanvas.Size = new Size(0x638, 0x362);
            this.panelMusicCanvas.TabIndex = 0;
            this.radOffice2007ScreenTipElement1.set_Description("Override this property and provide custom screentip template description in DesignTime.");
            this.radOffice2007ScreenTipElement1.set_Name("radOffice2007ScreenTipElement1");
            this.radOffice2007ScreenTipElement1.set_TemplateType(null);
            this.radOffice2007ScreenTipElement1.set_TipSize(new Size(0, 0));
            this.radMenuItemEdit.set_Alignment(ContentAlignment.MiddleCenter);
            this.radMenuItemEdit.set_AutoSize(false);
            this.radMenuItemEdit.set_Bounds(new Rectangle(0, 0, 0x38, 20));
            this.radMenuItemEdit.set_Font(new Font("微软雅黑", 11f));
            this.radMenuItemEdit.Items.AddRange(new RadItem[] { 
                this.radMenuSeparatorItem4, this.radMenuItemInsertMeasure, this.radMenuItemRemoveMeasure, this.radMenuItemAddOrRemoveNotePitch, this.radMenuItemAddOrRemoveRest, this.radMenuItemRemoveNotePack, this.radMenuSeparatorItem5, this.radMenuItemMouseDragarea, this.radMenuItemPasteSelection, this.radMenuItemPasteSelectionToNotation, this.radMenuItemRemoveSelection, this.radMenuSeparatorItem8, this.radMenuItemIncreaseDuration, this.radMenuItemDecreaseDuration, this.radMenuItemTieStart, this.radMenuItemTieStop,
                this.radMenuItemTieTriplet, this.radMenuItemToggleDotted, this.radMenuItemToggleStaccato, this.radMenuItemNaturalSign, this.radMenuItemArpeggio, this.radMenuItemChangePitchLevel, this.radMenuSeparatorItem9, this.radMenuItemEsc, this.radMenuItemTogglePlayMusic, this.radMenuItemTogglePlayMusicFull, this.radMenuItemTogglePlayMusicSpeedUp, this.radMenuItemTogglePlayMusicSpeedDown, this.radMenuSeparatorItem10, this.radMenuItemToggleFreePlayMode, this.radMenuItemToggleDelaylessCommandMode, this.radMenuItemNotationMenu,
                this.radMenuSeparatorItem11
            });
            this.radMenuItemEdit.set_Name("radMenuItemEdit");
            this.radMenuItemEdit.ShowArrow = false;
            this.radMenuItemEdit.set_Text("说明");
            this.radMenuItemMouseDragarea.set_Font(new Font("微软雅黑", 10f));
            this.radMenuItemMouseDragarea.set_ForeColor(Color.FromArgb(200, 200, 200));
            this.radMenuItemMouseDragarea.set_Name("radMenuItemMouseDragarea");
            this.radMenuItemMouseDragarea.set_Text("鼠标框选音符　　　　　　　　　　　　　鼠标左键（按住拖动）");
            this.radMenuItemPasteSelection.set_Font(new Font("微软雅黑", 10f));
            this.radMenuItemPasteSelection.set_ForeColor(Color.FromArgb(200, 200, 200));
            this.radMenuItemPasteSelection.set_Name("radMenuItemPasteSelection");
            this.radMenuItemPasteSelection.set_Text("粘贴以鼠标框选的音符到小节　　　　　　Ctrl+V");
            this.radMenuItemRemoveSelection.set_Font(new Font("微软雅黑", 10f));
            this.radMenuItemRemoveSelection.set_ForeColor(Color.FromArgb(200, 200, 200));
            this.radMenuItemRemoveSelection.set_Name("radMenuItemRemoveSelection");
            this.radMenuItemRemoveSelection.set_Text("删除以鼠标框选的音符　　　　　　　　　Del（按住）");
            this.radMenuSeparatorItem4.set_Name("radMenuSeparatorItem4");
            this.radMenuSeparatorItem4.set_Text("radMenuSeparatorItem4");
            this.radMenuSeparatorItem4.TextAlignment = ContentAlignment.MiddleLeft;
            this.radMenuItemInsertMeasure.set_Font(new Font("微软雅黑", 10f));
            this.radMenuItemInsertMeasure.set_ForeColor(Color.FromArgb(200, 200, 200));
            this.radMenuItemInsertMeasure.set_Name("radMenuItemInsertMeasure");
            this.radMenuItemInsertMeasure.set_Text("插入新小节（鼠标位置）　　　　　　　　1（按住）");
            this.radMenuItemRemoveMeasure.set_Font(new Font("微软雅黑", 10f));
            this.radMenuItemRemoveMeasure.set_ForeColor(Color.FromArgb(200, 200, 200));
            this.radMenuItemRemoveMeasure.set_Name("radMenuItemRemoveMeasure");
            this.radMenuItemRemoveMeasure.set_Text("删除新小节（鼠标位置）　　　　　　　　2（按住）");
            this.radMenuItemAddOrRemoveNotePitch.set_Font(new Font("微软雅黑", 10f));
            this.radMenuItemAddOrRemoveNotePitch.set_ForeColor(Color.FromArgb(200, 200, 200));
            this.radMenuItemAddOrRemoveNotePitch.set_Name("radMenuItemAddOrRemoveNotePitch");
            this.radMenuItemAddOrRemoveNotePitch.set_Text("添加／删除单个音符（鼠标位置）　　　　鼠标左键（按住）");
            this.radMenuItemRemoveNotePack.set_Font(new Font("微软雅黑", 10f));
            this.radMenuItemRemoveNotePack.set_ForeColor(Color.FromArgb(200, 200, 200));
            this.radMenuItemRemoveNotePack.set_Name("radMenuItemRemoveNotePack");
            this.radMenuItemRemoveNotePack.set_Text("删除整组音符（鼠标位置）　　　　　　　鼠标右键（按住）");
            this.radMenuItemAddOrRemoveRest.set_Font(new Font("微软雅黑", 10f));
            this.radMenuItemAddOrRemoveRest.set_ForeColor(Color.FromArgb(200, 200, 200));
            this.radMenuItemAddOrRemoveRest.set_Name("radMenuItemAddOrRemoveRest");
            this.radMenuItemAddOrRemoveRest.set_Text("添加／删除／切换休止符（鼠标位置）　　鼠标中键（按住）");
            this.radMenuSeparatorItem5.set_Name("radMenuSeparatorItem5");
            this.radMenuSeparatorItem5.set_Text("radMenuSeparatorItem5");
            this.radMenuSeparatorItem5.TextAlignment = ContentAlignment.MiddleLeft;
            this.radMenuSeparatorItem8.set_Name("radMenuSeparatorItem8");
            this.radMenuSeparatorItem8.set_Text("radMenuSeparatorItem8");
            this.radMenuSeparatorItem8.TextAlignment = ContentAlignment.MiddleLeft;
            this.radMenuItemIncreaseDuration.set_Font(new Font("微软雅黑", 10f));
            this.radMenuItemIncreaseDuration.set_ForeColor(Color.FromArgb(200, 200, 200));
            this.radMenuItemIncreaseDuration.set_Name("radMenuItemIncreaseDuration");
            this.radMenuItemIncreaseDuration.set_Text("音符增加时值（鼠标位置）　　　　　　　Q");
            this.radMenuItemDecreaseDuration.set_Font(new Font("微软雅黑", 10f));
            this.radMenuItemDecreaseDuration.set_ForeColor(Color.FromArgb(200, 200, 200));
            this.radMenuItemDecreaseDuration.set_Name("radMenuItemDecreaseDuration");
            this.radMenuItemDecreaseDuration.set_Text("音符减少时值（鼠标位置）　　　　　　　W");
            this.radMenuSeparatorItem9.set_Name("radMenuSeparatorItem9");
            this.radMenuSeparatorItem9.set_Text("radMenuSeparatorItem9");
            this.radMenuSeparatorItem9.TextAlignment = ContentAlignment.MiddleLeft;
            this.radMenuItemEsc.set_Font(new Font("微软雅黑", 10f));
            this.radMenuItemEsc.set_ForeColor(Color.FromArgb(250, 200, 200));
            this.radMenuItemEsc.set_Name("radMenuItemEsc");
            this.radMenuItemEsc.set_Text("终止播放／取消框选／其他关闭操作　　　Esc");
            this.radMenuItemTogglePlayMusic.set_Font(new Font("微软雅黑", 10f));
            this.radMenuItemTogglePlayMusic.set_ForeColor(Color.FromArgb(200, 200, 200));
            this.radMenuItemTogglePlayMusic.set_Name("radMenuItemTogglePlayMusic");
            this.radMenuItemTogglePlayMusic.set_Text("从指定位置播放／暂停（鼠标位置）　　　Space");
            this.radMenuItemTogglePlayMusicFull.set_Font(new Font("微软雅黑", 10f));
            this.radMenuItemTogglePlayMusicFull.set_ForeColor(Color.FromArgb(200, 200, 200));
            this.radMenuItemTogglePlayMusicFull.set_Name("radMenuItemTogglePlayMusicFull");
            this.radMenuItemTogglePlayMusicFull.set_Text("从开始位置播放／暂停　　　　　　　　　Enter");
            this.radMenuItemTogglePlayMusicSpeedUp.set_Font(new Font("微软雅黑", 10f));
            this.radMenuItemTogglePlayMusicSpeedUp.set_ForeColor(Color.FromArgb(200, 200, 200));
            this.radMenuItemTogglePlayMusicSpeedUp.set_Name("radMenuItemTogglePlayMusicSpeedUp");
            this.radMenuItemTogglePlayMusicSpeedUp.set_Text("增加播放速度　　　　　　　　　　　　　PageUp");
            this.radMenuItemTogglePlayMusicSpeedDown.set_Font(new Font("微软雅黑", 10f));
            this.radMenuItemTogglePlayMusicSpeedDown.set_ForeColor(Color.FromArgb(200, 200, 200));
            this.radMenuItemTogglePlayMusicSpeedDown.set_Name("radMenuItemTogglePlayMusicSpeedDown");
            this.radMenuItemTogglePlayMusicSpeedDown.set_Text("降低播放速度　　　　　　　　　　　　　PageDown");
            this.radMenuItemToggleFreePlayMode.set_Font(new Font("微软雅黑", 10f));
            this.radMenuItemToggleFreePlayMode.set_ForeColor(Color.FromArgb(200, 200, 200));
            this.radMenuItemToggleFreePlayMode.set_Name("radMenuItemToggleFreePlayMode");
            this.radMenuItemToggleFreePlayMode.set_Text("开启／关闭自由演奏模式　　　　　　　　F9");
            this.radMenuItemToggleDelaylessCommandMode.set_Font(new Font("微软雅黑", 10f));
            this.radMenuItemToggleDelaylessCommandMode.set_ForeColor(Color.FromArgb(200, 200, 200));
            this.radMenuItemToggleDelaylessCommandMode.set_Name("radMenuItemToggleDelaylessCommandMode");
            this.radMenuItemToggleDelaylessCommandMode.set_Text("按住／点击操作模式切换　　　　　　　　F10");
            this.radMenuItemNotationMenu.set_Font(new Font("微软雅黑", 10f));
            this.radMenuItemNotationMenu.set_ForeColor(Color.FromArgb(200, 250, 200));
            this.radMenuItemNotationMenu.set_Name("radMenuItemTogglePlayMusic");
            this.radMenuItemNotationMenu.set_Text("乐谱／小节配置菜单（鼠标位置）　　　　3");
            this.radMenuItemToggleDotted.set_Font(new Font("微软雅黑", 10f));
            this.radMenuItemToggleDotted.set_ForeColor(Color.FromArgb(200, 200, 200));
            this.radMenuItemToggleDotted.set_Name("radMenuItemToggleDotted");
            this.radMenuItemToggleDotted.set_Text("音符附点开关　　　　　　　　　　　　　X");
            this.radMenuItemToggleStaccato.set_Font(new Font("微软雅黑", 10f));
            this.radMenuItemToggleStaccato.set_ForeColor(Color.FromArgb(200, 200, 200));
            this.radMenuItemToggleStaccato.set_Name("radMenuItemToggleStaccato");
            this.radMenuItemToggleStaccato.set_Text("音符跳音开关（此功能暂未开放）　　　　C");
            this.radMenuItemNaturalSign.set_Font(new Font("微软雅黑", 10f));
            this.radMenuItemNaturalSign.set_ForeColor(Color.FromArgb(200, 200, 200));
            this.radMenuItemNaturalSign.set_Name("radMenuItemNaturalSign");
            this.radMenuItemNaturalSign.set_Text("变音记号（仅单音符，升、降、还原号）　Z");
            this.radMenuItemArpeggio.set_Font(new Font("微软雅黑", 10f));
            this.radMenuItemArpeggio.set_ForeColor(Color.FromArgb(200, 200, 200));
            this.radMenuItemArpeggio.set_Name("radMenuItemArpeggio");
            this.radMenuItemArpeggio.set_Text("琶音记号切换（上行、下行、无）　　　　V");
            this.radMenuItemPasteSelectionToNotation.set_Font(new Font("微软雅黑", 10f));
            this.radMenuItemPasteSelectionToNotation.set_ForeColor(Color.FromArgb(200, 200, 200));
            this.radMenuItemPasteSelectionToNotation.set_Name("radMenuItemPasteSelectionToNotation");
            this.radMenuItemPasteSelectionToNotation.set_Text("粘贴以鼠标框选的所有音符到乐谱新小节　Ctrl+Shift+V");
            this.radMenuItemChangePitchLevel.set_Font(new Font("微软雅黑", 10f));
            this.radMenuItemChangePitchLevel.set_ForeColor(Color.FromArgb(200, 200, 200));
            this.radMenuItemChangePitchLevel.set_Name("radMenuItemChangePitchLevel");
            this.radMenuItemChangePitchLevel.set_Text("小节增加／减少整八度　　　　　　　　　UpArrow、DownArrow");
            this.radMenuItemTieStart.set_Font(new Font("微软雅黑", 10f));
            this.radMenuItemTieStart.set_ForeColor(Color.FromArgb(200, 200, 200));
            this.radMenuItemTieStart.set_Name("radMenuItemTieStart");
            this.radMenuItemTieStart.set_Text("音符延音线（圆滑线）开始　　　　　　　A");
            this.radMenuItemTieStop.set_Font(new Font("微软雅黑", 10f));
            this.radMenuItemTieStop.set_ForeColor(Color.FromArgb(200, 200, 200));
            this.radMenuItemTieStop.set_Name("radMenuItemTieStop");
            this.radMenuItemTieStop.set_Text("音符延音线（圆滑线）结束　　　　　　　S");
            this.radMenuItemTieTriplet.set_Font(new Font("微软雅黑", 10f));
            this.radMenuItemTieTriplet.set_ForeColor(Color.FromArgb(200, 200, 200));
            this.radMenuItemTieTriplet.set_Name("radMenuItemTieTriplet");
            this.radMenuItemTieTriplet.set_Text("音符三连音开始　　　　　　　　　　　　D");
            this.radMenuSeparatorItem10.set_Name("radMenuSeparatorItem10");
            this.radMenuSeparatorItem10.set_Text("radMenuSeparatorItem10");
            this.radMenuSeparatorItem10.TextAlignment = ContentAlignment.MiddleLeft;
            this.radMenuSeparatorItem11.set_Name("radMenuSeparatorItem11");
            this.radMenuSeparatorItem11.set_Text("radMenuSeparatorItem11");
            this.radMenuSeparatorItem11.TextAlignment = ContentAlignment.MiddleLeft;
            this.radMenuItemTool.set_Alignment(ContentAlignment.MiddleCenter);
            this.radMenuItemTool.set_AutoSize(false);
            this.radMenuItemTool.set_Bounds(new Rectangle(0, 0, 0x38, 20));
            this.radMenuItemTool.set_Font(new Font("微软雅黑", 11f));
            this.radMenuItemTool.Items.AddRange(new RadItem[] { this.radMenuItemEnableFreePlayMode, this.radMenuItemEnableNumberedSignTip, this.radMenuItemEnablePlaySoundWhenInsert, this.radMenuItemEnableAutoScrollWhenPlayMusic, this.radMenuItemEnableSnaplineWhenInsertNotePack, this.radMenuItemEnableStemAnimation, this.radMenuItemEnableDelaylessCommand });
            this.radMenuItemTool.set_Name("radMenuItemTool");
            this.radMenuItemTool.ShowArrow = false;
            this.radMenuItemTool.set_Text("设置");
            this.radMenuItemEnableFreePlayMode.set_Font(new Font("微软雅黑", 10f));
            this.radMenuItemEnableFreePlayMode.set_ForeColor(Color.FromArgb(200, 200, 200));
            this.radMenuItemEnableFreePlayMode.set_Name("radMenuItemEnableFreePlayMode");
            this.radMenuItemEnableFreePlayMode.set_Text("自由演奏模式　　　　　　　　　　　　　F9");
            this.radMenuItemEnableFreePlayMode.IsChecked = false;
            this.radMenuItemEnableFreePlayMode.add_Click(new EventHandler(this.radMenuItemEnableFreePlayMode_Click));
            this.radMenuItemEnableStemAnimation.set_Font(new Font("微软雅黑", 10f));
            this.radMenuItemEnableStemAnimation.set_ForeColor(Color.FromArgb(200, 200, 200));
            this.radMenuItemEnableStemAnimation.set_Name("radMenuItemEnableStemAnimation");
            this.radMenuItemEnableStemAnimation.set_Text("符尾动画表现");
            this.radMenuItemEnableStemAnimation.IsChecked = true;
            this.radMenuItemEnableStemAnimation.add_Click(new EventHandler(this.radMenuItemEnableStemAnimation_Click));
            this.radMenuItemEnableNumberedSignTip.set_Font(new Font("微软雅黑", 10f));
            this.radMenuItemEnableNumberedSignTip.set_ForeColor(Color.FromArgb(200, 200, 200));
            this.radMenuItemEnableNumberedSignTip.set_Name("radMenuItemEnableNumberedSignTip");
            this.radMenuItemEnableNumberedSignTip.set_Text("音符添加时的简谱提示");
            this.radMenuItemEnableNumberedSignTip.IsChecked = true;
            this.radMenuItemEnableNumberedSignTip.add_Click(new EventHandler(this.radMenuItemEnableNumberedSignTip_Click));
            this.radMenuItemEnablePlaySoundWhenInsert.set_Font(new Font("微软雅黑", 10f));
            this.radMenuItemEnablePlaySoundWhenInsert.set_ForeColor(Color.FromArgb(200, 200, 200));
            this.radMenuItemEnablePlaySoundWhenInsert.set_Name("radMenuItemEnablePlaySoundWhenInsert");
            this.radMenuItemEnablePlaySoundWhenInsert.set_Text("添加音符时的效果音");
            this.radMenuItemEnablePlaySoundWhenInsert.IsChecked = true;
            this.radMenuItemEnablePlaySoundWhenInsert.add_Click(new EventHandler(this.radMenuItemEnablePlaySoundWhenInsert_Click));
            this.radMenuItemEnableAutoScrollWhenPlayMusic.set_Font(new Font("微软雅黑", 10f));
            this.radMenuItemEnableAutoScrollWhenPlayMusic.set_ForeColor(Color.FromArgb(200, 200, 200));
            this.radMenuItemEnableAutoScrollWhenPlayMusic.set_Name("radMenuItemEnableAutoScrollWhenPlayMusic");
            this.radMenuItemEnableAutoScrollWhenPlayMusic.set_Text("播放时的乐谱自动滚动功能");
            this.radMenuItemEnableAutoScrollWhenPlayMusic.IsChecked = true;
            this.radMenuItemEnableAutoScrollWhenPlayMusic.add_Click(new EventHandler(this.radMenuItemEnableAutoScrollWhenPlayMusic_Click));
            this.radMenuItemEnableSnaplineWhenInsertNotePack.set_Font(new Font("微软雅黑", 10f));
            this.radMenuItemEnableSnaplineWhenInsertNotePack.set_ForeColor(Color.FromArgb(200, 200, 200));
            this.radMenuItemEnableSnaplineWhenInsertNotePack.set_Name("radMenuItemEnableSnaplineWhenInsertNotePack");
            this.radMenuItemEnableSnaplineWhenInsertNotePack.set_Text("插入音符时的对齐线");
            this.radMenuItemEnableSnaplineWhenInsertNotePack.IsChecked = false;
            this.radMenuItemEnableSnaplineWhenInsertNotePack.add_Click(new EventHandler(this.radMenuItemEnableSnaplineWhenInsertNotePack_Click));
            this.radMenuItemEnableDelaylessCommand.set_Font(new Font("微软雅黑", 10f));
            this.radMenuItemEnableDelaylessCommand.set_ForeColor(Color.FromArgb(200, 200, 200));
            this.radMenuItemEnableDelaylessCommand.set_Name("radMenuItemEnableDelaylessCommand");
            this.radMenuItemEnableDelaylessCommand.set_Text("按住操作→点击操作（框选需按住Ctrl）　F10");
            this.radMenuItemEnableDelaylessCommand.IsChecked = false;
            this.radMenuItemEnableDelaylessCommand.add_Click(new EventHandler(this.radMenuItemEnableDelaylessCommand_Click));
            this.radMenuItemHelp.set_Alignment(ContentAlignment.MiddleCenter);
            this.radMenuItemHelp.set_AutoSize(false);
            this.radMenuItemHelp.set_Bounds(new Rectangle(0, 0, 0x38, 20));
            this.radMenuItemHelp.set_Font(new Font("微软雅黑", 11f));
            this.radMenuItemHelp.Items.AddRange(new RadItem[] { this.radMenuItemAbout });
            this.radMenuItemHelp.set_Name("radMenuItemHelp");
            this.radMenuItemHelp.set_PositionOffset(new SizeF(0f, 0f));
            this.radMenuItemHelp.ShowArrow = false;
            this.radMenuItemHelp.set_Text("关于");
            this.radMenuItemAbout.set_Font(new Font("微软雅黑", 10f));
            this.radMenuItemAbout.set_ForeColor(Color.FromArgb(200, 240, 200));
            this.radMenuItemAbout.set_Name("radMenuItemAbout");
            this.radMenuItemAbout.set_Text("关于...");
            this.radMenuItemAbout.add_Click(new EventHandler(this.radMenuItemAbout_Click));
            this.radMenuItemFile.set_Alignment(ContentAlignment.MiddleCenter);
            this.radMenuItemFile.set_AutoSize(false);
            this.radMenuItemFile.set_Bounds(new Rectangle(0, 0, 0x38, 20));
            this.radMenuItemFile.set_Font(new Font("微软雅黑", 11f));
            this.radMenuItemFile.Items.AddRange(new RadItem[] { this.radMenuItemNew, this.radMenuItemSave, this.radMenuItemSaveAs, this.radMenuItemLoad, this.radMenuItemUploadNotation, this.radMenuSeparatorItem3, this.radMenuItemExit });
            this.radMenuItemFile.set_Name("radMenuItemFile");
            this.radMenuItemFile.ShowArrow = false;
            this.radMenuItemFile.set_Text("文件");
            this.radMenuItemNew.set_Font(new Font("微软雅黑", 10f));
            this.radMenuItemNew.set_ForeColor(Color.FromArgb(200, 200, 200));
            this.radMenuItemNew.set_Name("radMenuItemNew");
            this.radMenuItemNew.set_Text("新建乐谱　　　　　Ctrl+N");
            this.radMenuItemNew.add_Click(new EventHandler(this.radMenuItemNew_Click));
            this.radMenuItemSave.set_Font(new Font("微软雅黑", 10f));
            this.radMenuItemSave.set_ForeColor(Color.FromArgb(200, 200, 200));
            this.radMenuItemSave.set_Name("radMenuItemSave");
            this.radMenuItemSave.set_Text("保存乐谱　　　　　Ctrl+S");
            this.radMenuItemSave.add_Click(new EventHandler(this.radMenuItemSave_Click));
            this.radMenuItemSaveAs.set_Font(new Font("微软雅黑", 10f));
            this.radMenuItemSaveAs.set_ForeColor(Color.FromArgb(200, 200, 200));
            this.radMenuItemSaveAs.set_Name("radMenuItemSaveAs");
            this.radMenuItemSaveAs.set_Text("乐谱另存为　　　　Ctrl+Shift+S");
            this.radMenuItemSaveAs.add_Click(new EventHandler(this.radMenuItemSaveAs_Click));
            this.radMenuItemUploadNotation.set_Font(new Font("微软雅黑", 10f));
            this.radMenuItemUploadNotation.set_ForeColor(Color.FromArgb(200, 200, 200));
            this.radMenuItemUploadNotation.set_Name("radMenuItemUploadNotation");
            this.radMenuItemUploadNotation.set_Text("上传乐谱至服务器　F1");
            this.radMenuItemUploadNotation.add_Click(new EventHandler(this.radMenuItemUploadNotation_Click));
            this.radMenuItemLoad.set_Font(new Font("微软雅黑", 10f));
            this.radMenuItemLoad.set_ForeColor(Color.FromArgb(200, 200, 200));
            this.radMenuItemLoad.set_Name("radMenuItemLoad");
            this.radMenuItemLoad.set_Text("打开乐谱　　　　　Ctrl+L");
            this.radMenuItemLoad.add_Click(new EventHandler(this.radMenuItemLoad_Click));
            this.radMenuSeparatorItem3.set_Name("radMenuSeparatorItem3");
            this.radMenuSeparatorItem3.set_Text("radMenuSeparatorItem3");
            this.radMenuSeparatorItem3.TextAlignment = ContentAlignment.MiddleLeft;
            this.radMenuItemExit.set_Font(new Font("微软雅黑", 10f));
            this.radMenuItemExit.set_ForeColor(Color.FromArgb(220, 200, 200));
            this.radMenuItemExit.set_Name("radMenuItemExit");
            this.radMenuItemExit.set_Text("退出");
            this.radMenuItemExit.add_Click(new EventHandler(this.radMenuItemExit_Click));
            this.radMenu.AllItemsEqualHeight = true;
            this.radMenu.BackColor = Color.FromArgb(0x2d, 0x2d, 0x30);
            this.radMenu.Font = new Font("Consolas", 10.25f);
            this.radMenu.get_Items().AddRange(new RadItem[] { this.radMenuItemFile, this.radMenuItemEdit, this.radMenuItemTool, this.radMenuItemHelp });
            this.radMenu.Location = new Point(0, 0);
            this.radMenu.Margin = new Padding(3, 3, 3, 0);
            this.radMenu.Name = "radMenu";
            this.radMenu.Size = new Size(0x638, 0x16);
            this.radMenu.SystemKeyHighlight = false;
            this.radMenu.TabIndex = 2;
            this.radMenu.set_ThemeName("VisualStudio2012Dark");
            base.AutoScaleDimensions = new SizeF(6f, 12f);
            base.AutoScaleMode = AutoScaleMode.Font;
            this.BackColor = Color.FromArgb(0x2d, 0x2d, 0x30);
            base.ClientSize = new Size(0x638, 0x395);
            base.Controls.Add(this.panelMusicCanvas);
            base.Controls.Add(this.radMenu);
            base.Icon = (Icon) manager.GetObject("$this.Icon");
            base.KeyPreview = true;
            this.MinimumSize = new Size(0x400, 0x300);
            base.Name = "GujianOL_MusicBox";
            base.RootElement.set_ApplyShapeToControl(true);
            base.StartPosition = FormStartPosition.CenterScreen;
            this.Text = "Gujian-Online MusicBox Editor";
            base.ThemeName = "VisualStudio2012Dark";
            base.FormClosing += new FormClosingEventHandler(this.GujianOL_MusicBox_FormClosing);
            this.radMenu.EndInit();
            this.EndInit();
            base.ResumeLayout(false);
            base.PerformLayout();
        }

        public void InitializeConfiguration()
        {
            string fileName = "./Configuration.ini";
            this._iniFileManager = new IniFile(fileName);
            if (!File.Exists(fileName))
            {
                this.SaveConfiguration();
            }
            MusicCanvasControl.EnableStemAnimation = this._iniFileManager.IniReadValue("Common", "EnableStemAnimation") == "1";
            MusicCanvasControl.EnableControlTip = this._iniFileManager.IniReadValue("Common", "EnableControlTip") == "1";
            MusicCanvasControl.EnableNumberedSignTip = this._iniFileManager.IniReadValue("Common", "EnableNumberedSignTip") == "1";
            MusicCanvasControl.EnablePlaySoundWhenInsert = this._iniFileManager.IniReadValue("Common", "EnablePlaySoundWhenInsert") == "1";
            MusicCanvasControl.EnableAutoScrollWhenPlayMusic = this._iniFileManager.IniReadValue("Common", "EnableAutoScrollWhenPlayMusic") == "1";
            MusicCanvasControl.EnableSnaplineWhenInsertNotePack = this._iniFileManager.IniReadValue("Common", "EnableSnaplineWhenInsertNotePack") == "1";
            MusicCanvasControl.EnableDelaylessCommand = this._iniFileManager.IniReadValue("Common", "EnableDelaylessCommand") == "1";
            this.radMenuItemEnableFreePlayMode.IsChecked = MusicCanvasControl.EnableFreePlayMode;
            this.radMenuItemEnableStemAnimation.IsChecked = MusicCanvasControl.EnableStemAnimation;
            this.radMenuItemEnableNumberedSignTip.IsChecked = MusicCanvasControl.EnableNumberedSignTip;
            this.radMenuItemEnablePlaySoundWhenInsert.IsChecked = MusicCanvasControl.EnablePlaySoundWhenInsert;
            this.radMenuItemEnableAutoScrollWhenPlayMusic.IsChecked = MusicCanvasControl.EnableAutoScrollWhenPlayMusic;
            this.radMenuItemEnableSnaplineWhenInsertNotePack.IsChecked = MusicCanvasControl.EnableSnaplineWhenInsertNotePack;
            this.radMenuItemEnableDelaylessCommand.IsChecked = MusicCanvasControl.EnableDelaylessCommand;
        }

        public void InitializeMusicCanvasControl()
        {
            this.CanvasControl = new MusicCanvasControl(this);
            this.CanvasControl.Dock = DockStyle.Fill;
            this.CanvasControl.Name = "musicCanvas";
            this.panelMusicCanvas.Controls.Add(this.CanvasControl);
            this.CanvasControl.RefreshCanvas();
            this.CanvasControl.OnMusicCanvasAccessedFilenameChanged += delegate (object sender, MusicCanvasIoEventArgs args) {
                if (string.IsNullOrEmpty(args.LastAccessedFilename))
                {
                    this.Text = "Gujian-Online MusicBox Editor";
                }
                else
                {
                    this.Text = "Gujian-Online MusicBox Editor :: " + args.LastAccessedFilename;
                }
            };
        }

        private void radMenuItemAbout_Click(object sender, EventArgs e)
        {
            MusicBoxResources.ShowAboutWindow(Assembly.GetExecutingAssembly().GetName().Version.ToString());
        }

        private void radMenuItemEnableAutoScrollWhenPlayMusic_Click(object sender, EventArgs e)
        {
            MusicCanvasControl.EnableAutoScrollWhenPlayMusic = !MusicCanvasControl.EnableAutoScrollWhenPlayMusic;
            this.radMenuItemEnableAutoScrollWhenPlayMusic.IsChecked = MusicCanvasControl.EnableAutoScrollWhenPlayMusic;
        }

        public void radMenuItemEnableDelaylessCommand_Click(object sender, EventArgs e)
        {
            MusicCanvasControl.EnableDelaylessCommand = !MusicCanvasControl.EnableDelaylessCommand;
            this.radMenuItemEnableDelaylessCommand.IsChecked = MusicCanvasControl.EnableDelaylessCommand;
        }

        public void radMenuItemEnableFreePlayMode_Click(object sender, EventArgs e)
        {
            MusicCanvasControl.EnableFreePlayMode = !MusicCanvasControl.EnableFreePlayMode;
            this.radMenuItemEnableFreePlayMode.IsChecked = MusicCanvasControl.EnableFreePlayMode;
        }

        private void radMenuItemEnableNumberedSignTip_Click(object sender, EventArgs e)
        {
            MusicCanvasControl.EnableNumberedSignTip = !MusicCanvasControl.EnableNumberedSignTip;
            this.radMenuItemEnableNumberedSignTip.IsChecked = MusicCanvasControl.EnableNumberedSignTip;
        }

        private void radMenuItemEnablePlaySoundWhenInsert_Click(object sender, EventArgs e)
        {
            MusicCanvasControl.EnablePlaySoundWhenInsert = !MusicCanvasControl.EnablePlaySoundWhenInsert;
            this.radMenuItemEnablePlaySoundWhenInsert.IsChecked = MusicCanvasControl.EnablePlaySoundWhenInsert;
        }

        private void radMenuItemEnableSnaplineWhenInsertNotePack_Click(object sender, EventArgs e)
        {
            MusicCanvasControl.EnableSnaplineWhenInsertNotePack = !MusicCanvasControl.EnableSnaplineWhenInsertNotePack;
            this.radMenuItemEnableSnaplineWhenInsertNotePack.IsChecked = MusicCanvasControl.EnableSnaplineWhenInsertNotePack;
        }

        private void radMenuItemEnableStemAnimation_Click(object sender, EventArgs e)
        {
            MusicCanvasControl.EnableStemAnimation = !MusicCanvasControl.EnableStemAnimation;
            this.radMenuItemEnableStemAnimation.IsChecked = MusicCanvasControl.EnableStemAnimation;
        }

        private void radMenuItemExit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void radMenuItemLoad_Click(object sender, EventArgs e)
        {
            this.CanvasControl.LoadMusicalNotation();
        }

        private void radMenuItemNew_Click(object sender, EventArgs e)
        {
            this.CanvasControl.NewMusicalNotation();
        }

        private void radMenuItemSave_Click(object sender, EventArgs e)
        {
            this.CanvasControl.SaveMusicalNotation();
        }

        private void radMenuItemSaveAs_Click(object sender, EventArgs e)
        {
            this.CanvasControl.SaveMusicalNotation("");
        }

        public void radMenuItemUploadNotation_Click(object sender, EventArgs e)
        {
            if ((this.CanvasControl.SaveMusicalNotation() && (RadMessageBox.Show($"是否将以下乐谱文件上传至《古剑奇谭网络版》服务器？

{this.CanvasControl.LastAccessedFilename}", "乐谱上传", MessageBoxButtons.OKCancel) == DialogResult.OK)) && !SendMessageToClient.Send(this.CanvasControl.LastAccessedFilename))
            {
                RadMessageBox.Show("上传乐谱失败！\n\n请确认已经运行《古剑奇谭网络版》并有角色登录游戏。\n\n上传的乐谱会以在当前登录的游戏角色为上传者身份进行保存。", "乐谱上传", MessageBoxButtons.OK);
            }
        }

        public void SaveConfiguration()
        {
            this._iniFileManager.IniWriteValue("Common", "EnableStemAnimation", MusicCanvasControl.EnableStemAnimation ? "1" : "0");
            this._iniFileManager.IniWriteValue("Common", "EnableControlTip", MusicCanvasControl.EnableControlTip ? "1" : "0");
            this._iniFileManager.IniWriteValue("Common", "EnableNumberedSignTip", MusicCanvasControl.EnableNumberedSignTip ? "1" : "0");
            this._iniFileManager.IniWriteValue("Common", "EnablePlaySoundWhenInsert", MusicCanvasControl.EnablePlaySoundWhenInsert ? "1" : "0");
            this._iniFileManager.IniWriteValue("Common", "EnableAutoScrollWhenPlayMusic", MusicCanvasControl.EnableAutoScrollWhenPlayMusic ? "1" : "0");
            this._iniFileManager.IniWriteValue("Common", "EnableSnaplineWhenInsertNotePack", MusicCanvasControl.EnableSnaplineWhenInsertNotePack ? "1" : "0");
            this._iniFileManager.IniWriteValue("Common", "EnableDelaylessCommand", MusicCanvasControl.EnableDelaylessCommand ? "1" : "0");
        }

        public MusicCanvasControl CanvasControl { get; private set; }

        public RadButton LastPercussionsButton { get; private set; }
    }
}

