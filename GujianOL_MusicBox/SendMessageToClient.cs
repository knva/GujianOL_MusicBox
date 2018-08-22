namespace GujianOL_MusicBox
{
    using System;
    using System.Runtime.InteropServices;
    using System.Text;

    public static class SendMessageToClient
    {
        private const int WM_COPYDATA = 0x4a;

        [DllImport("User32.dll")]
        public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);
        public static bool Send(string msg)
        {
            int hWnd = (int) FindWindow("VVideoClass", null);
            if (hWnd != 0)
            {
                COPYDATASTRUCT copydatastruct;
                copydatastruct.dwData = new IntPtr(0x6d832950);
                copydatastruct.cbData = Encoding.Default.GetByteCount(msg);
                copydatastruct.lpData = msg;
                SendMessage(hWnd, 0x4a, 0, ref copydatastruct);
                return true;
            }
            return false;
        }

        [DllImport("User32.dll")]
        private static extern int SendMessage(int hWnd, int Msg, int wParam, ref COPYDATASTRUCT lParam);

        [StructLayout(LayoutKind.Sequential)]
        public struct COPYDATASTRUCT
        {
            public IntPtr dwData;
            public int cbData;
            [MarshalAs(UnmanagedType.LPStr)]
            public string lpData;
        }
    }
}

