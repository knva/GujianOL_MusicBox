namespace GujianOL_MusicBox
{
    using System;
    using System.Runtime.CompilerServices;

    public class MusicCanvasIoEventArgs : EventArgs
    {
        public MusicCanvasIoEventArgs(string accessedFilename)
        {
            this.LastAccessedFilename = accessedFilename;
        }

        public string LastAccessedFilename { get; set; }
    }
}

