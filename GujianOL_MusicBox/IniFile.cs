namespace GujianOL_MusicBox
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;

    public class IniFile
    {
        public IniFile(string fileName)
        {
            this.IniFileName = fileName;
        }

        [DllImport("kernel32")]
        private static extern int GetPrivateProfileString(string section, string key, string defaultVal, StringBuilder retVal, int size, string filePath);
        [DllImport("kernel32")]
        private static extern int GetPrivateProfileString(string section, string key, string defaultVal, byte[] retReturnedString, uint size, string filePath);
        public string IniReadValue(string section, string key) => 
            IniReadValue(this.IniFileName, section, key);

        public static string IniReadValue(string filename, string section, string key)
        {
            StringBuilder retVal = new StringBuilder(0xff);
            GetPrivateProfileString(section, key, "", retVal, 0xff, filename);
            return retVal.ToString();
        }

        public void IniWriteValue(string section, string key, string value)
        {
            IniWriteValue(this.IniFileName, section, key, value);
        }

        public static void IniWriteValue(string filename, string section, string key, string value)
        {
            WritePrivateProfileString(section, key, value, filename);
        }

        public Dictionary<string, Dictionary<string, string>> ReadFullIniDatas()
        {
            Dictionary<string, Dictionary<string, string>> dictionary = new Dictionary<string, Dictionary<string, string>>();
            foreach (string str in this.ReadSections())
            {
                Dictionary<string, string> dictionary2;
                if (!dictionary.TryGetValue(str, out dictionary2))
                {
                    dictionary.Add(str, new Dictionary<string, string>());
                    dictionary2 = dictionary[str];
                }
                foreach (string str2 in this.ReadSingleSectionKeys(str))
                {
                    string str3 = this.IniReadValue(str, str2);
                    if (!dictionary2.ContainsKey(str2))
                    {
                        dictionary2.Add(str2, str3);
                    }
                    else
                    {
                        dictionary2[str2] = str3;
                    }
                }
            }
            return dictionary;
        }

        private List<string> ReadSections()
        {
            List<string> list = new List<string>();
            byte[] retReturnedString = new byte[0x10000];
            int num = GetPrivateProfileString(null, null, null, retReturnedString, (uint) retReturnedString.Length, this.IniFileName);
            int index = 0;
            for (int i = 0; i < num; i++)
            {
                if (retReturnedString[i] == 0)
                {
                    list.Add(Encoding.Default.GetString(retReturnedString, index, i - index));
                    index = i + 1;
                }
            }
            return list;
        }

        private List<string> ReadSingleSectionKeys(string section)
        {
            List<string> list = new List<string>();
            byte[] retReturnedString = new byte[0x10000];
            int num = GetPrivateProfileString(section, null, null, retReturnedString, (uint) retReturnedString.Length, this.IniFileName);
            int index = 0;
            for (int i = 0; i < num; i++)
            {
                if (retReturnedString[i] == 0)
                {
                    list.Add(Encoding.Default.GetString(retReturnedString, index, i - index));
                    index = i + 1;
                }
            }
            return list;
        }

        public void SaveFullIniDatas(Dictionary<string, Dictionary<string, string>> dicIniDatas)
        {
            foreach (string str in dicIniDatas.Keys)
            {
                Dictionary<string, string> dictionary = dicIniDatas[str];
                foreach (string str2 in dictionary.Keys)
                {
                    string str3 = dictionary[str2];
                    this.IniWriteValue(str, str2, str3);
                }
            }
        }

        [DllImport("kernel32")]
        private static extern long WritePrivateProfileString(string section, string key, string val, string filePath);

        public string IniFileName { get; set; }
    }
}

