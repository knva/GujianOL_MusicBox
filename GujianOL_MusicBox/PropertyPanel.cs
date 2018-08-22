namespace GujianOL_MusicBox
{
    using System;
    using System.ComponentModel;
    using System.Drawing;
    using System.Windows.Forms;

    public class PropertyPanel : Form
    {
        private Button buttonCancel;
        private Button buttonOK;
        private IContainer components = null;
        private Label label1;
        private Label label11;
        private Label label2;
        private Label label3;
        private Label label4;
        private Label label5;
        private Label label6;
        private Label label7;
        private Label label8;
        private Label label9;
        private Label labelAuthor;
        private Label labelBpm;
        private Label labelClefType;
        private Label labelCreator;
        private Label labelDenominator;
        private Label labelInstrument;
        private Label labelKeySignature;
        private Label labelName;
        private Label labelNumberedKeySignature;
        private Label labelTranslater;
        private Panel panel1;
        private Panel panel2;
        private Panel panel3;
        private Panel panel4;
        private Panel panel5;
        private Panel panelBg;
        private TextBox textBoxAuthor;
        private TextBox textBoxCreator;
        private TextBox textBoxName;
        private TextBox textBoxTranslater;
        private TrackBar trackBarBpm;
        private TrackBar trackBarClefType;
        private TrackBar trackBarDenominator;
        private TrackBar trackBarInstrument;
        private TrackBar trackBarKeySignature;
        private TrackBar trackBarMeasureVolume;
        private TrackBar trackBarNumberedKeySignature;
        private TrackBar trackBarVolumeCurve0;
        private TrackBar trackBarVolumeCurve1;
        private TrackBar trackBarVolumeCurve2;
        private TrackBar trackBarVolumeCurve3;
        private TrackBar trackBarVolumeCurve4;
        private TrackBar trackBarVolumeCurve5;
        private TrackBar trackBarVolumeCurve6;
        private TrackBar trackBarVolumeCurve7;

        public PropertyPanel()
        {
            this.InitializeComponent();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && (this.components != null))
            {
                this.components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.buttonOK = new Button();
            this.buttonCancel = new Button();
            this.panelBg = new Panel();
            this.panel5 = new Panel();
            this.labelNumberedKeySignature = new Label();
            this.trackBarNumberedKeySignature = new TrackBar();
            this.panel3 = new Panel();
            this.panel4 = new Panel();
            this.label11 = new Label();
            this.trackBarMeasureVolume = new TrackBar();
            this.label9 = new Label();
            this.label8 = new Label();
            this.label7 = new Label();
            this.label6 = new Label();
            this.label5 = new Label();
            this.label4 = new Label();
            this.label3 = new Label();
            this.label2 = new Label();
            this.label1 = new Label();
            this.trackBarVolumeCurve7 = new TrackBar();
            this.trackBarVolumeCurve6 = new TrackBar();
            this.trackBarVolumeCurve5 = new TrackBar();
            this.trackBarVolumeCurve4 = new TrackBar();
            this.trackBarVolumeCurve3 = new TrackBar();
            this.trackBarVolumeCurve2 = new TrackBar();
            this.trackBarVolumeCurve1 = new TrackBar();
            this.trackBarVolumeCurve0 = new TrackBar();
            this.panel2 = new Panel();
            this.labelDenominator = new Label();
            this.trackBarDenominator = new TrackBar();
            this.labelBpm = new Label();
            this.labelKeySignature = new Label();
            this.trackBarBpm = new TrackBar();
            this.trackBarKeySignature = new TrackBar();
            this.trackBarClefType = new TrackBar();
            this.labelClefType = new Label();
            this.trackBarInstrument = new TrackBar();
            this.labelInstrument = new Label();
            this.panel1 = new Panel();
            this.labelTranslater = new Label();
            this.textBoxTranslater = new TextBox();
            this.labelCreator = new Label();
            this.textBoxCreator = new TextBox();
            this.labelAuthor = new Label();
            this.textBoxAuthor = new TextBox();
            this.labelName = new Label();
            this.textBoxName = new TextBox();
            this.panelBg.SuspendLayout();
            this.panel5.SuspendLayout();
            this.trackBarNumberedKeySignature.BeginInit();
            this.panel3.SuspendLayout();
            this.trackBarMeasureVolume.BeginInit();
            this.trackBarVolumeCurve7.BeginInit();
            this.trackBarVolumeCurve6.BeginInit();
            this.trackBarVolumeCurve5.BeginInit();
            this.trackBarVolumeCurve4.BeginInit();
            this.trackBarVolumeCurve3.BeginInit();
            this.trackBarVolumeCurve2.BeginInit();
            this.trackBarVolumeCurve1.BeginInit();
            this.trackBarVolumeCurve0.BeginInit();
            this.panel2.SuspendLayout();
            this.trackBarDenominator.BeginInit();
            this.trackBarBpm.BeginInit();
            this.trackBarKeySignature.BeginInit();
            this.trackBarClefType.BeginInit();
            this.trackBarInstrument.BeginInit();
            this.panel1.SuspendLayout();
            base.SuspendLayout();
            this.buttonOK.DialogResult = DialogResult.OK;
            this.buttonOK.FlatStyle = FlatStyle.Flat;
            this.buttonOK.Font = new Font("微软雅黑", 9.75f, FontStyle.Bold);
            this.buttonOK.ForeColor = Color.Silver;
            this.buttonOK.Location = new Point(0x92, 0x238);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new Size(0x4b, 30);
            this.buttonOK.TabIndex = 0x13;
            this.buttonOK.Text = "OK";
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonCancel.DialogResult = DialogResult.Cancel;
            this.buttonCancel.FlatStyle = FlatStyle.Flat;
            this.buttonCancel.Font = new Font("微软雅黑", 9.75f, FontStyle.Bold, GraphicsUnit.Point, 0x86);
            this.buttonCancel.ForeColor = Color.Silver;
            this.buttonCancel.Location = new Point(0xe3, 0x238);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new Size(0x4b, 30);
            this.buttonCancel.TabIndex = 20;
            this.buttonCancel.Text = "Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.panelBg.BackColor = Color.FromArgb(30, 30, 30);
            this.panelBg.BorderStyle = BorderStyle.FixedSingle;
            this.panelBg.Controls.Add(this.panel5);
            this.panelBg.Controls.Add(this.panel3);
            this.panelBg.Controls.Add(this.panel2);
            this.panelBg.Controls.Add(this.panel1);
            this.panelBg.Controls.Add(this.buttonOK);
            this.panelBg.Controls.Add(this.buttonCancel);
            this.panelBg.Dock = DockStyle.Fill;
            this.panelBg.Location = new Point(0, 0);
            this.panelBg.Name = "panelBg";
            this.panelBg.Padding = new Padding(3);
            this.panelBg.Size = new Size(0x13e, 610);
            this.panelBg.TabIndex = 3;
            this.panel5.BorderStyle = BorderStyle.FixedSingle;
            this.panel5.Controls.Add(this.labelNumberedKeySignature);
            this.panel5.Controls.Add(this.trackBarNumberedKeySignature);
            this.panel5.Location = new Point(7, 0x94);
            this.panel5.Name = "panel5";
            this.panel5.Size = new Size(0x12e, 0x2a);
            this.panel5.TabIndex = 8;
            this.labelNumberedKeySignature.AutoSize = true;
            this.labelNumberedKeySignature.Font = new Font("微软雅黑", 10f);
            this.labelNumberedKeySignature.ForeColor = Color.FromArgb(0xe0, 0xe0, 0xe0);
            this.labelNumberedKeySignature.Location = new Point(2, 9);
            this.labelNumberedKeySignature.Name = "labelNumberedKeySignature";
            this.labelNumberedKeySignature.Size = new Size(0x52, 20);
            this.labelNumberedKeySignature.TabIndex = 15;
            this.labelNumberedKeySignature.Text = "简调：C (0)";
            this.trackBarNumberedKeySignature.LargeChange = 1;
            this.trackBarNumberedKeySignature.Location = new Point(0x61, 9);
            this.trackBarNumberedKeySignature.Maximum = 11;
            this.trackBarNumberedKeySignature.Name = "trackBarNumberedKeySignature";
            this.trackBarNumberedKeySignature.Size = new Size(0xc3, 0x2d);
            this.trackBarNumberedKeySignature.TabIndex = 4;
            this.trackBarNumberedKeySignature.ValueChanged += new EventHandler(this.trackBarNumberedKeySignature_ValueChanged);
            this.panel3.BorderStyle = BorderStyle.FixedSingle;
            this.panel3.Controls.Add(this.panel4);
            this.panel3.Controls.Add(this.label11);
            this.panel3.Controls.Add(this.trackBarMeasureVolume);
            this.panel3.Controls.Add(this.label9);
            this.panel3.Controls.Add(this.label8);
            this.panel3.Controls.Add(this.label7);
            this.panel3.Controls.Add(this.label6);
            this.panel3.Controls.Add(this.label5);
            this.panel3.Controls.Add(this.label4);
            this.panel3.Controls.Add(this.label3);
            this.panel3.Controls.Add(this.label2);
            this.panel3.Controls.Add(this.label1);
            this.panel3.Controls.Add(this.trackBarVolumeCurve7);
            this.panel3.Controls.Add(this.trackBarVolumeCurve6);
            this.panel3.Controls.Add(this.trackBarVolumeCurve5);
            this.panel3.Controls.Add(this.trackBarVolumeCurve4);
            this.panel3.Controls.Add(this.trackBarVolumeCurve3);
            this.panel3.Controls.Add(this.trackBarVolumeCurve2);
            this.panel3.Controls.Add(this.trackBarVolumeCurve1);
            this.panel3.Controls.Add(this.trackBarVolumeCurve0);
            this.panel3.Location = new Point(7, 360);
            this.panel3.Name = "panel3";
            this.panel3.Size = new Size(0x12e, 0xc5);
            this.panel3.TabIndex = 7;
            this.panel4.BackColor = Color.FromArgb(100, 100, 100);
            this.panel4.Location = new Point(0xed, -1);
            this.panel4.Name = "panel4";
            this.panel4.Size = new Size(1, 0xca);
            this.panel4.TabIndex = 0x17;
            this.label11.AutoSize = true;
            this.label11.Font = new Font("微软雅黑", 10f);
            this.label11.ForeColor = Color.FromArgb(0xe0, 0xe0, 0xe0);
            this.label11.Location = new Point(0xf6, 7);
            this.label11.Name = "label11";
            this.label11.Size = new Size(0x25, 20);
            this.label11.TabIndex = 0x16;
            this.label11.Text = "音量";
            this.trackBarMeasureVolume.LargeChange = 1;
            this.trackBarMeasureVolume.Location = new Point(0xf7, 0x1a);
            this.trackBarMeasureVolume.Name = "trackBarMeasureVolume";
            this.trackBarMeasureVolume.Orientation = Orientation.Vertical;
            this.trackBarMeasureVolume.Size = new Size(0x2d, 0x94);
            this.trackBarMeasureVolume.TabIndex = 10;
            this.trackBarMeasureVolume.Value = 8;
            this.trackBarMeasureVolume.ValueChanged += new EventHandler(this.trackBarMeasureVolume_ValueChanged);
            this.label9.AutoSize = true;
            this.label9.Font = new Font("微软雅黑", 10f);
            this.label9.ForeColor = Color.FromArgb(0xe0, 0xe0, 0xe0);
            this.label9.Location = new Point(0xc2, 170);
            this.label9.Name = "label9";
            this.label9.Size = new Size(0x11, 20);
            this.label9.TabIndex = 0x13;
            this.label9.Text = "1";
            this.label8.AutoSize = true;
            this.label8.Font = new Font("微软雅黑", 10f);
            this.label8.ForeColor = Color.FromArgb(0x9a, 0x9a, 0xff);
            this.label8.Location = new Point(0xa4, 170);
            this.label8.Name = "label8";
            this.label8.Size = new Size(0x17, 20);
            this.label8.TabIndex = 0x12;
            this.label8.Text = "⅞";
            this.label7.AutoSize = true;
            this.label7.Font = new Font("微软雅黑", 10f);
            this.label7.ForeColor = Color.FromArgb(0xe0, 0xe0, 0xe0);
            this.label7.Location = new Point(0x89, 170);
            this.label7.Name = "label7";
            this.label7.Size = new Size(0x17, 20);
            this.label7.TabIndex = 0x11;
            this.label7.Text = "\x00be";
            this.label6.AutoSize = true;
            this.label6.Font = new Font("微软雅黑", 10f);
            this.label6.ForeColor = Color.FromArgb(0x9a, 0x9a, 0xff);
            this.label6.Location = new Point(110, 170);
            this.label6.Name = "label6";
            this.label6.Size = new Size(0x17, 20);
            this.label6.TabIndex = 0x10;
            this.label6.Text = "⅝";
            this.label5.AutoSize = true;
            this.label5.Font = new Font("微软雅黑", 10f);
            this.label5.ForeColor = Color.FromArgb(0xe0, 0xe0, 0xe0);
            this.label5.Location = new Point(0x53, 170);
            this.label5.Name = "label5";
            this.label5.Size = new Size(0x17, 20);
            this.label5.TabIndex = 15;
            this.label5.Text = "\x00bd";
            this.label4.AutoSize = true;
            this.label4.Font = new Font("微软雅黑", 10f);
            this.label4.ForeColor = Color.FromArgb(0x9a, 0x9a, 0xff);
            this.label4.Location = new Point(0x38, 170);
            this.label4.Name = "label4";
            this.label4.Size = new Size(0x17, 20);
            this.label4.TabIndex = 14;
            this.label4.Text = "⅜";
            this.label3.AutoSize = true;
            this.label3.Font = new Font("微软雅黑", 10f);
            this.label3.ForeColor = Color.FromArgb(0xe0, 0xe0, 0xe0);
            this.label3.Location = new Point(0x1d, 170);
            this.label3.Name = "label3";
            this.label3.Size = new Size(0x17, 20);
            this.label3.TabIndex = 13;
            this.label3.Text = "\x00bc";
            this.label2.AutoSize = true;
            this.label2.Font = new Font("微软雅黑", 10f);
            this.label2.ForeColor = Color.FromArgb(0x9a, 0x9a, 0xff);
            this.label2.Location = new Point(2, 170);
            this.label2.Name = "label2";
            this.label2.Size = new Size(0x17, 20);
            this.label2.TabIndex = 12;
            this.label2.Text = "⅛";
            this.label1.AutoSize = true;
            this.label1.Font = new Font("微软雅黑", 10f);
            this.label1.ForeColor = Color.FromArgb(0xe0, 0xe0, 0xe0);
            this.label1.Location = new Point(2, 7);
            this.label1.Name = "label1";
            this.label1.Size = new Size(0x41, 20);
            this.label1.TabIndex = 11;
            this.label1.Text = "音量曲线";
            this.trackBarVolumeCurve7.LargeChange = 1;
            this.trackBarVolumeCurve7.Location = new Point(0xc0, 0x1a);
            this.trackBarVolumeCurve7.Name = "trackBarVolumeCurve7";
            this.trackBarVolumeCurve7.Orientation = Orientation.Vertical;
            this.trackBarVolumeCurve7.Size = new Size(0x2d, 0x94);
            this.trackBarVolumeCurve7.TabIndex = 0x12;
            this.trackBarVolumeCurve7.Value = 8;
            this.trackBarVolumeCurve6.LargeChange = 1;
            this.trackBarVolumeCurve6.Location = new Point(0xa5, 0x1a);
            this.trackBarVolumeCurve6.Name = "trackBarVolumeCurve6";
            this.trackBarVolumeCurve6.Orientation = Orientation.Vertical;
            this.trackBarVolumeCurve6.Size = new Size(0x2d, 0x94);
            this.trackBarVolumeCurve6.TabIndex = 0x11;
            this.trackBarVolumeCurve6.Value = 8;
            this.trackBarVolumeCurve5.LargeChange = 1;
            this.trackBarVolumeCurve5.Location = new Point(0x8a, 0x1a);
            this.trackBarVolumeCurve5.Name = "trackBarVolumeCurve5";
            this.trackBarVolumeCurve5.Orientation = Orientation.Vertical;
            this.trackBarVolumeCurve5.Size = new Size(0x2d, 0x94);
            this.trackBarVolumeCurve5.TabIndex = 0x10;
            this.trackBarVolumeCurve5.Value = 8;
            this.trackBarVolumeCurve4.LargeChange = 1;
            this.trackBarVolumeCurve4.Location = new Point(0x6f, 0x1a);
            this.trackBarVolumeCurve4.Name = "trackBarVolumeCurve4";
            this.trackBarVolumeCurve4.Orientation = Orientation.Vertical;
            this.trackBarVolumeCurve4.Size = new Size(0x2d, 0x94);
            this.trackBarVolumeCurve4.TabIndex = 15;
            this.trackBarVolumeCurve4.Value = 8;
            this.trackBarVolumeCurve3.LargeChange = 1;
            this.trackBarVolumeCurve3.Location = new Point(0x54, 0x1a);
            this.trackBarVolumeCurve3.Name = "trackBarVolumeCurve3";
            this.trackBarVolumeCurve3.Orientation = Orientation.Vertical;
            this.trackBarVolumeCurve3.Size = new Size(0x2d, 0x94);
            this.trackBarVolumeCurve3.TabIndex = 14;
            this.trackBarVolumeCurve3.Value = 8;
            this.trackBarVolumeCurve2.LargeChange = 1;
            this.trackBarVolumeCurve2.Location = new Point(0x39, 0x1a);
            this.trackBarVolumeCurve2.Name = "trackBarVolumeCurve2";
            this.trackBarVolumeCurve2.Orientation = Orientation.Vertical;
            this.trackBarVolumeCurve2.Size = new Size(0x2d, 0x94);
            this.trackBarVolumeCurve2.TabIndex = 13;
            this.trackBarVolumeCurve2.Value = 8;
            this.trackBarVolumeCurve1.LargeChange = 1;
            this.trackBarVolumeCurve1.Location = new Point(30, 0x1a);
            this.trackBarVolumeCurve1.Name = "trackBarVolumeCurve1";
            this.trackBarVolumeCurve1.Orientation = Orientation.Vertical;
            this.trackBarVolumeCurve1.Size = new Size(0x2d, 0x94);
            this.trackBarVolumeCurve1.TabIndex = 12;
            this.trackBarVolumeCurve1.Value = 8;
            this.trackBarVolumeCurve0.LargeChange = 1;
            this.trackBarVolumeCurve0.Location = new Point(3, 0x1a);
            this.trackBarVolumeCurve0.Name = "trackBarVolumeCurve0";
            this.trackBarVolumeCurve0.Orientation = Orientation.Vertical;
            this.trackBarVolumeCurve0.Size = new Size(0x2d, 0x94);
            this.trackBarVolumeCurve0.TabIndex = 11;
            this.trackBarVolumeCurve0.Value = 8;
            this.panel2.BorderStyle = BorderStyle.FixedSingle;
            this.panel2.Controls.Add(this.labelDenominator);
            this.panel2.Controls.Add(this.trackBarDenominator);
            this.panel2.Controls.Add(this.labelBpm);
            this.panel2.Controls.Add(this.labelKeySignature);
            this.panel2.Controls.Add(this.trackBarBpm);
            this.panel2.Controls.Add(this.trackBarKeySignature);
            this.panel2.Controls.Add(this.trackBarClefType);
            this.panel2.Controls.Add(this.labelClefType);
            this.panel2.Controls.Add(this.trackBarInstrument);
            this.panel2.Controls.Add(this.labelInstrument);
            this.panel2.Location = new Point(7, 0xc4);
            this.panel2.Name = "panel2";
            this.panel2.Size = new Size(0x12e, 0x9e);
            this.panel2.TabIndex = 6;
            this.labelDenominator.AutoSize = true;
            this.labelDenominator.Font = new Font("微软雅黑", 10f);
            this.labelDenominator.ForeColor = Color.FromArgb(0xe0, 0xe0, 0xe0);
            this.labelDenominator.Location = new Point(2, 0x7d);
            this.labelDenominator.Name = "labelDenominator";
            this.labelDenominator.Size = new Size(0x49, 20);
            this.labelDenominator.TabIndex = 0x12;
            this.labelDenominator.Text = "拍号：4/4";
            this.trackBarDenominator.LargeChange = 1;
            this.trackBarDenominator.Location = new Point(0x61, 0x7d);
            this.trackBarDenominator.Maximum = 20;
            this.trackBarDenominator.Name = "trackBarDenominator";
            this.trackBarDenominator.Size = new Size(0xc3, 0x2d);
            this.trackBarDenominator.TabIndex = 9;
            this.trackBarDenominator.TickFrequency = 3;
            this.trackBarDenominator.Value = 7;
            this.trackBarDenominator.ValueChanged += new EventHandler(this.trackBarDenominator_ValueChanged);
            this.labelBpm.AutoSize = true;
            this.labelBpm.Font = new Font("微软雅黑", 10f);
            this.labelBpm.ForeColor = Color.FromArgb(0xe0, 0xe0, 0xe0);
            this.labelBpm.Location = new Point(2, 0x60);
            this.labelBpm.Name = "labelBpm";
            this.labelBpm.Size = new Size(0x59, 20);
            this.labelBpm.TabIndex = 11;
            this.labelBpm.Text = "节拍：80pm";
            this.labelKeySignature.AutoSize = true;
            this.labelKeySignature.Font = new Font("微软雅黑", 10f);
            this.labelKeySignature.ForeColor = Color.FromArgb(0xe0, 0xe0, 0xe0);
            this.labelKeySignature.Location = new Point(2, 0x43);
            this.labelKeySignature.Name = "labelKeySignature";
            this.labelKeySignature.Size = new Size(0x52, 20);
            this.labelKeySignature.TabIndex = 15;
            this.labelKeySignature.Text = "调号：C (0)";
            this.trackBarBpm.LargeChange = 1;
            this.trackBarBpm.Location = new Point(0x61, 0x60);
            this.trackBarBpm.Maximum = 90;
            this.trackBarBpm.Minimum = 10;
            this.trackBarBpm.Name = "trackBarBpm";
            this.trackBarBpm.Size = new Size(0xc3, 0x2d);
            this.trackBarBpm.TabIndex = 8;
            this.trackBarBpm.TickFrequency = 10;
            this.trackBarBpm.Value = 40;
            this.trackBarBpm.ValueChanged += new EventHandler(this.trackBarBpm_ValueChanged);
            this.trackBarKeySignature.LargeChange = 1;
            this.trackBarKeySignature.Location = new Point(0x61, 0x43);
            this.trackBarKeySignature.Maximum = 7;
            this.trackBarKeySignature.Minimum = -7;
            this.trackBarKeySignature.Name = "trackBarKeySignature";
            this.trackBarKeySignature.Size = new Size(0xc3, 0x2d);
            this.trackBarKeySignature.TabIndex = 7;
            this.trackBarKeySignature.ValueChanged += new EventHandler(this.trackBarKeySignature_ValueChanged);
            this.trackBarClefType.LargeChange = 1;
            this.trackBarClefType.Location = new Point(0x61, 0x26);
            this.trackBarClefType.Maximum = 1;
            this.trackBarClefType.Name = "trackBarClefType";
            this.trackBarClefType.Size = new Size(0xc3, 0x2d);
            this.trackBarClefType.TabIndex = 6;
            this.trackBarClefType.Value = 1;
            this.trackBarClefType.ValueChanged += new EventHandler(this.trackBarClefType_ValueChanged);
            this.labelClefType.AutoSize = true;
            this.labelClefType.Font = new Font("微软雅黑", 10f);
            this.labelClefType.ForeColor = Color.FromArgb(0xe0, 0xe0, 0xe0);
            this.labelClefType.Location = new Point(2, 0x26);
            this.labelClefType.Name = "labelClefType";
            this.labelClefType.Size = new Size(0x4f, 20);
            this.labelClefType.TabIndex = 13;
            this.labelClefType.Text = "谱号：高音";
            this.trackBarInstrument.LargeChange = 1;
            this.trackBarInstrument.Location = new Point(0x61, 9);
            this.trackBarInstrument.Maximum = 3;
            this.trackBarInstrument.Name = "trackBarInstrument";
            this.trackBarInstrument.Size = new Size(0xc3, 0x2d);
            this.trackBarInstrument.TabIndex = 5;
            this.trackBarInstrument.ValueChanged += new EventHandler(this.trackBarInstrument_ValueChanged);
            this.labelInstrument.AutoSize = true;
            this.labelInstrument.Font = new Font("微软雅黑", 10f);
            this.labelInstrument.ForeColor = Color.FromArgb(0xe0, 0xe0, 0xe0);
            this.labelInstrument.Location = new Point(2, 9);
            this.labelInstrument.Name = "labelInstrument";
            this.labelInstrument.Size = new Size(0x4f, 20);
            this.labelInstrument.TabIndex = 10;
            this.labelInstrument.Text = "乐器：钢琴";
            this.panel1.BorderStyle = BorderStyle.FixedSingle;
            this.panel1.Controls.Add(this.labelTranslater);
            this.panel1.Controls.Add(this.textBoxTranslater);
            this.panel1.Controls.Add(this.labelCreator);
            this.panel1.Controls.Add(this.textBoxCreator);
            this.panel1.Controls.Add(this.labelAuthor);
            this.panel1.Controls.Add(this.textBoxAuthor);
            this.panel1.Controls.Add(this.labelName);
            this.panel1.Controls.Add(this.textBoxName);
            this.panel1.Location = new Point(7, 7);
            this.panel1.Name = "panel1";
            this.panel1.Size = new Size(0x12e, 0x87);
            this.panel1.TabIndex = 5;
            this.labelTranslater.AutoSize = true;
            this.labelTranslater.Font = new Font("微软雅黑", 10f);
            this.labelTranslater.ForeColor = Color.FromArgb(0xe0, 0xe0, 0xe0);
            this.labelTranslater.Location = new Point(2, 0x47);
            this.labelTranslater.Name = "labelTranslater";
            this.labelTranslater.Size = new Size(0x25, 20);
            this.labelTranslater.TabIndex = 11;
            this.labelTranslater.Text = "转谱";
            this.textBoxTranslater.BackColor = Color.FromArgb(40, 40, 40);
            this.textBoxTranslater.BorderStyle = BorderStyle.FixedSingle;
            this.textBoxTranslater.Cursor = Cursors.IBeam;
            this.textBoxTranslater.Font = new Font("微软雅黑", 10f);
            this.textBoxTranslater.ForeColor = SystemColors.Info;
            this.textBoxTranslater.Location = new Point(0x2b, 0x45);
            this.textBoxTranslater.Name = "textBoxTranslater";
            this.textBoxTranslater.Size = new Size(0xf9, 0x19);
            this.textBoxTranslater.TabIndex = 2;
            this.textBoxTranslater.Text = "？？？？";
            this.textBoxTranslater.TextAlign = HorizontalAlignment.Center;
            this.textBoxTranslater.KeyPress += new KeyPressEventHandler(this.textBoxCommon_KeyPress);
            this.labelCreator.AutoSize = true;
            this.labelCreator.Font = new Font("微软雅黑", 10f);
            this.labelCreator.ForeColor = Color.FromArgb(0xe0, 0xe0, 0xe0);
            this.labelCreator.Location = new Point(2, 0x65);
            this.labelCreator.Name = "labelCreator";
            this.labelCreator.Size = new Size(0x25, 20);
            this.labelCreator.TabIndex = 9;
            this.labelCreator.Text = "录入";
            this.textBoxCreator.BackColor = Color.FromArgb(40, 40, 40);
            this.textBoxCreator.BorderStyle = BorderStyle.FixedSingle;
            this.textBoxCreator.Cursor = Cursors.IBeam;
            this.textBoxCreator.Font = new Font("微软雅黑", 10f);
            this.textBoxCreator.ForeColor = SystemColors.Info;
            this.textBoxCreator.Location = new Point(0x2b, 0x63);
            this.textBoxCreator.Name = "textBoxCreator";
            this.textBoxCreator.Size = new Size(0xf9, 0x19);
            this.textBoxCreator.TabIndex = 3;
            this.textBoxCreator.Text = "？？？？";
            this.textBoxCreator.TextAlign = HorizontalAlignment.Center;
            this.textBoxCreator.KeyPress += new KeyPressEventHandler(this.textBoxCommon_KeyPress);
            this.labelAuthor.AutoSize = true;
            this.labelAuthor.Font = new Font("微软雅黑", 10f);
            this.labelAuthor.ForeColor = Color.FromArgb(0xe0, 0xe0, 0xe0);
            this.labelAuthor.Location = new Point(2, 0x29);
            this.labelAuthor.Name = "labelAuthor";
            this.labelAuthor.Size = new Size(0x25, 20);
            this.labelAuthor.TabIndex = 7;
            this.labelAuthor.Text = "作者";
            this.textBoxAuthor.BackColor = Color.FromArgb(40, 40, 40);
            this.textBoxAuthor.BorderStyle = BorderStyle.FixedSingle;
            this.textBoxAuthor.Cursor = Cursors.IBeam;
            this.textBoxAuthor.Font = new Font("微软雅黑", 10f);
            this.textBoxAuthor.ForeColor = SystemColors.Info;
            this.textBoxAuthor.Location = new Point(0x2b, 0x27);
            this.textBoxAuthor.Name = "textBoxAuthor";
            this.textBoxAuthor.Size = new Size(0xf9, 0x19);
            this.textBoxAuthor.TabIndex = 1;
            this.textBoxAuthor.Text = "？？？？";
            this.textBoxAuthor.TextAlign = HorizontalAlignment.Center;
            this.textBoxAuthor.KeyPress += new KeyPressEventHandler(this.textBoxCommon_KeyPress);
            this.labelName.AutoSize = true;
            this.labelName.Font = new Font("微软雅黑", 10f);
            this.labelName.ForeColor = Color.FromArgb(0xe0, 0xe0, 0xe0);
            this.labelName.Location = new Point(2, 11);
            this.labelName.Name = "labelName";
            this.labelName.Size = new Size(0x25, 20);
            this.labelName.TabIndex = 5;
            this.labelName.Text = "名称";
            this.textBoxName.BackColor = Color.FromArgb(50, 50, 50);
            this.textBoxName.BorderStyle = BorderStyle.FixedSingle;
            this.textBoxName.Cursor = Cursors.IBeam;
            this.textBoxName.Font = new Font("微软雅黑", 10f);
            this.textBoxName.ForeColor = SystemColors.Info;
            this.textBoxName.Location = new Point(0x2b, 9);
            this.textBoxName.Name = "textBoxName";
            this.textBoxName.Size = new Size(0xf9, 0x19);
            this.textBoxName.TabIndex = 0;
            this.textBoxName.Text = "？？？？";
            this.textBoxName.TextAlign = HorizontalAlignment.Center;
            this.textBoxName.KeyPress += new KeyPressEventHandler(this.textBoxCommon_KeyPress);
            base.AutoScaleDimensions = new SizeF(7f, 17f);
            base.AutoScaleMode = AutoScaleMode.Font;
            this.BackColor = Color.FromArgb(130, 130, 130);
            base.CancelButton = this.buttonCancel;
            base.ClientSize = new Size(0x13e, 610);
            base.ControlBox = false;
            base.Controls.Add(this.panelBg);
            this.DoubleBuffered = true;
            this.Font = new Font("微软雅黑", 9f);
            base.FormBorderStyle = FormBorderStyle.None;
            base.Margin = new Padding(3, 4, 3, 4);
            base.Name = "PropertyPanel";
            base.ShowIcon = false;
            base.ShowInTaskbar = false;
            base.SizeGripStyle = SizeGripStyle.Hide;
            base.StartPosition = FormStartPosition.Manual;
            this.Text = "Property";
            base.Shown += new EventHandler(this.PropertyPanel_Shown);
            this.panelBg.ResumeLayout(false);
            this.panel5.ResumeLayout(false);
            this.panel5.PerformLayout();
            this.trackBarNumberedKeySignature.EndInit();
            this.panel3.ResumeLayout(false);
            this.panel3.PerformLayout();
            this.trackBarMeasureVolume.EndInit();
            this.trackBarVolumeCurve7.EndInit();
            this.trackBarVolumeCurve6.EndInit();
            this.trackBarVolumeCurve5.EndInit();
            this.trackBarVolumeCurve4.EndInit();
            this.trackBarVolumeCurve3.EndInit();
            this.trackBarVolumeCurve2.EndInit();
            this.trackBarVolumeCurve1.EndInit();
            this.trackBarVolumeCurve0.EndInit();
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            this.trackBarDenominator.EndInit();
            this.trackBarBpm.EndInit();
            this.trackBarKeySignature.EndInit();
            this.trackBarClefType.EndInit();
            this.trackBarInstrument.EndInit();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            base.ResumeLayout(false);
        }

        private void PropertyPanel_Shown(object sender, EventArgs e)
        {
            this.buttonCancel.Select();
        }

        private void textBoxCommon_KeyPress(object sender, KeyPressEventArgs e)
        {
            switch (e.KeyChar)
            {
                case '\'':
                case '\\':
                case '\t':
                case '\n':
                case '\r':
                case '"':
                    e.Handled = true;
                    break;
            }
        }

        private void trackBarBpm_ValueChanged(object sender, EventArgs e)
        {
            this.labelBpm.Text = "节拍：" + ((this.trackBarBpm.Value * 2)).ToString() + "pm";
        }

        private void trackBarClefType_ValueChanged(object sender, EventArgs e)
        {
            this.labelClefType.Text = (this.trackBarClefType.Value == 1) ? "谱号：高音" : "谱号：低音";
        }

        private void trackBarDenominator_ValueChanged(object sender, EventArgs e)
        {
            McNotePack.DurationTypes[] typesArray = new McNotePack.DurationTypes[] { McNotePack.DurationTypes.Half, McNotePack.DurationTypes.Quarter, McNotePack.DurationTypes.Eighth };
            int num = (int) (McNotePack.DurationTypes.Whole / typesArray[this.trackBarDenominator.Value % 3]);
            this.labelDenominator.Text = string.Concat(new object[] { "拍号：", this.BeatsPerMeasure, "/", num });
        }

        private void trackBarInstrument_ValueChanged(object sender, EventArgs e)
        {
            switch (this.trackBarInstrument.Value)
            {
                case 0:
                    this.labelInstrument.Text = "乐器：钢琴";
                    break;

                case 1:
                    this.labelInstrument.Text = "乐器：筝";
                    break;

                case 2:
                    this.labelInstrument.Text = "乐器：铁琴";
                    break;

                case 3:
                    this.labelInstrument.Text = "乐器：音效";
                    break;
            }
        }

        private void trackBarKeySignature_ValueChanged(object sender, EventArgs e)
        {
            string[] strArray = new string[] { "bC", "bG", "bD", "bA", "bE", "bB", "F", "C", "G", "D", "A", "E", "B", "#F", "#C" };
            this.labelKeySignature.Text = string.Concat(new object[] { "调号：", strArray[this.trackBarKeySignature.Value + 7], " (", this.trackBarKeySignature.Value, ")" });
        }

        private void trackBarMeasureVolume_ValueChanged(object sender, EventArgs e)
        {
        }

        private void trackBarNumberedKeySignature_ValueChanged(object sender, EventArgs e)
        {
            McNotation.NumberedKeySignatureTypes c = McNotation.NumberedKeySignatureTypes.C;
            if (Enum.IsDefined(typeof(McNotation.NumberedKeySignatureTypes), this.trackBarNumberedKeySignature.Value))
            {
                c = (McNotation.NumberedKeySignatureTypes) Enum.ToObject(typeof(McNotation.NumberedKeySignatureTypes), this.trackBarNumberedKeySignature.Value);
            }
            this.labelNumberedKeySignature.Text = "简调：" + c.ToString();
        }

        public McNotePack.DurationTypes BeatDurationType
        {
            get
            {
                McNotePack.DurationTypes[] typesArray = new McNotePack.DurationTypes[] { McNotePack.DurationTypes.Half, McNotePack.DurationTypes.Quarter, McNotePack.DurationTypes.Eighth };
                return typesArray[this.trackBarDenominator.Value % 3];
            }
            set
            {
                switch (value)
                {
                    case McNotePack.DurationTypes.Eighth:
                        this.trackBarDenominator.Value = ((this.BeatsPerMeasure - 2) * 3) + 2;
                        break;

                    case McNotePack.DurationTypes.Quarter:
                        this.trackBarDenominator.Value = ((this.BeatsPerMeasure - 2) * 3) + 1;
                        break;

                    case McNotePack.DurationTypes.Half:
                        this.trackBarDenominator.Value = (this.BeatsPerMeasure - 2) * 3;
                        break;
                }
            }
        }

        public int BeatsPerMeasure
        {
            get => 
                (2 + (this.trackBarDenominator.Value / 3));
            set
            {
                this.trackBarDenominator.Value = ((value - 2) * 3) + (this.trackBarDenominator.Value % 3);
            }
        }

        public int BeatsPerMinute
        {
            get => 
                (this.trackBarBpm.Value * 2);
            set
            {
                this.trackBarBpm.Value = value / 2;
            }
        }

        public McMeasure.ClefTypes ClefType
        {
            get
            {
                switch (this.trackBarClefType.Value)
                {
                    case 0:
                        return McMeasure.ClefTypes.L4F;

                    case 1:
                        return McMeasure.ClefTypes.L2G;
                }
                return McMeasure.ClefTypes.Invaild;
            }
            set
            {
                switch (value)
                {
                    case McMeasure.ClefTypes.L2G:
                        this.trackBarClefType.Value = 1;
                        break;

                    case McMeasure.ClefTypes.L4F:
                        this.trackBarClefType.Value = 0;
                        break;
                }
            }
        }

        public McMeasure.InstrumentTypes InstrumentType
        {
            get
            {
                switch (this.trackBarInstrument.Value)
                {
                    case 0:
                        return McMeasure.InstrumentTypes.Piano;

                    case 1:
                        return McMeasure.InstrumentTypes.Zheng;

                    case 2:
                        return McMeasure.InstrumentTypes.Tieqin;

                    case 3:
                        return McMeasure.InstrumentTypes.Misc;
                }
                return McMeasure.InstrumentTypes.Invaild;
            }
            set
            {
                switch (value)
                {
                    case McMeasure.InstrumentTypes.Zheng:
                        this.trackBarInstrument.Value = 1;
                        break;

                    case McMeasure.InstrumentTypes.Tieqin:
                        this.trackBarInstrument.Value = 2;
                        break;

                    case McMeasure.InstrumentTypes.Piano:
                        this.trackBarInstrument.Value = 0;
                        break;

                    case McMeasure.InstrumentTypes.Misc:
                        this.trackBarInstrument.Value = 3;
                        break;
                }
            }
        }

        public int KeySignature
        {
            get => 
                this.trackBarKeySignature.Value;
            set
            {
                this.trackBarKeySignature.Value = value.Clamp(-7, 7);
            }
        }

        public float MeasureVolume
        {
            get => 
                (((float) this.trackBarMeasureVolume.Value) / 10f);
            set
            {
                this.trackBarMeasureVolume.Value = (int) Math.Round((double) (value.Clamp(0f, 1f) * 10f));
            }
        }

        public string NotationAuthor
        {
            get => 
                this.textBoxAuthor.Text;
            set
            {
                this.textBoxAuthor.Text = value.FormatNotationText();
            }
        }

        public string NotationBoxCreator
        {
            get => 
                this.textBoxCreator.Text;
            set
            {
                this.textBoxCreator.Text = value.FormatNotationText();
            }
        }

        public string NotationName
        {
            get => 
                this.textBoxName.Text;
            set
            {
                this.textBoxName.Text = value.FormatNotationText();
            }
        }

        public string NotationTranslater
        {
            get => 
                this.textBoxTranslater.Text;
            set
            {
                this.textBoxTranslater.Text = value.FormatNotationText();
            }
        }

        public McNotation.NumberedKeySignatureTypes NumberedKeySignature
        {
            get
            {
                McNotation.NumberedKeySignatureTypes c = McNotation.NumberedKeySignatureTypes.C;
                if (Enum.IsDefined(typeof(McNotation.NumberedKeySignatureTypes), this.trackBarNumberedKeySignature.Value))
                {
                    c = (McNotation.NumberedKeySignatureTypes) Enum.ToObject(typeof(McNotation.NumberedKeySignatureTypes), this.trackBarNumberedKeySignature.Value);
                }
                return c;
            }
            set
            {
                this.trackBarNumberedKeySignature.Value = (int) value;
            }
        }

        public GujianOL_MusicBox.VolumeCurve VolumeCurve
        {
            get
            {
                GujianOL_MusicBox.VolumeCurve curve = new GujianOL_MusicBox.VolumeCurve();
                curve.SetCurvedVolume(0, ((float) this.trackBarVolumeCurve0.Value) / 10f);
                curve.SetCurvedVolume(1, ((float) this.trackBarVolumeCurve1.Value) / 10f);
                curve.SetCurvedVolume(2, ((float) this.trackBarVolumeCurve2.Value) / 10f);
                curve.SetCurvedVolume(3, ((float) this.trackBarVolumeCurve3.Value) / 10f);
                curve.SetCurvedVolume(4, ((float) this.trackBarVolumeCurve4.Value) / 10f);
                curve.SetCurvedVolume(5, ((float) this.trackBarVolumeCurve5.Value) / 10f);
                curve.SetCurvedVolume(6, ((float) this.trackBarVolumeCurve6.Value) / 10f);
                curve.SetCurvedVolume(7, ((float) this.trackBarVolumeCurve7.Value) / 10f);
                return curve;
            }
            set
            {
                this.trackBarVolumeCurve0.Value = (int) Math.Round((double) (value.GetCurvedVolume(0) * 10f));
                this.trackBarVolumeCurve1.Value = (int) Math.Round((double) (value.GetCurvedVolume(1) * 10f));
                this.trackBarVolumeCurve2.Value = (int) Math.Round((double) (value.GetCurvedVolume(2) * 10f));
                this.trackBarVolumeCurve3.Value = (int) Math.Round((double) (value.GetCurvedVolume(3) * 10f));
                this.trackBarVolumeCurve4.Value = (int) Math.Round((double) (value.GetCurvedVolume(4) * 10f));
                this.trackBarVolumeCurve5.Value = (int) Math.Round((double) (value.GetCurvedVolume(5) * 10f));
                this.trackBarVolumeCurve6.Value = (int) Math.Round((double) (value.GetCurvedVolume(6) * 10f));
                this.trackBarVolumeCurve7.Value = (int) Math.Round((double) (value.GetCurvedVolume(7) * 10f));
            }
        }
    }
}

