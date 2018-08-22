namespace GujianOL_MusicBox
{
    using GujianOL_MusicBox.Properties;
    using System;
    using System.ComponentModel;
    using System.Drawing;
    using System.Windows.Forms;
    using Telerik.WinControls.UI;

    public class MusicBoxHelpWindow : RadForm
    {
        private IContainer components = null;

        public MusicBoxHelpWindow()
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
            this.BeginInit();
            base.SuspendLayout();
            base.AutoScaleDimensions = new SizeF(6f, 12f);
            base.AutoScaleMode = AutoScaleMode.Font;
            this.BackgroundImage = Resources.MusicBoxHelp;
            this.BackgroundImageLayout = ImageLayout.Stretch;
            base.ClientSize = new Size(0x570, 0x2e6);
            base.ControlBox = false;
            base.FormBorderStyle = FormBorderStyle.FixedToolWindow;
            base.MaximizeBox = false;
            base.MinimizeBox = false;
            base.Name = "MusicBoxHelpWindow";
            base.RootElement.set_ApplyShapeToControl(true);
            base.ShowIcon = false;
            base.ShowInTaskbar = false;
            this.ShowItemToolTips = false;
            base.StartPosition = FormStartPosition.CenterScreen;
            this.Text = "MusicBox Help Window";
            base.ThemeName = "VisualStudio2012Dark";
            base.TopMost = true;
            base.FormClosing += new FormClosingEventHandler(this.MusicBoxHelpWindow_FormClosing);
            base.MouseClick += new MouseEventHandler(this.MusicBoxHelpWindow_MouseClick);
            base.PreviewKeyDown += new PreviewKeyDownEventHandler(this.MusicBoxHelpWindow_PreviewKeyDown);
            this.EndInit();
            base.ResumeLayout(false);
        }

        private void MusicBoxHelpWindow_FormClosing(object sender, FormClosingEventArgs e)
        {
            base.Hide();
            e.Cancel = true;
        }

        private void MusicBoxHelpWindow_MouseClick(object sender, MouseEventArgs e)
        {
        }

        private void MusicBoxHelpWindow_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                base.Hide();
            }
        }
    }
}

