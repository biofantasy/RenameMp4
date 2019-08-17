using CsAtomReader;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace RenameMp4
{
    class Program
    {
        public static Config Config = null;

        public static void Main(string[] args)
        {
            if (Config == null)
                Config = new Config();

            try
            {
                string DirPath = Config.ReConfig.ReNameDirPath;

                DirectoryInfo directory = new DirectoryInfo(DirPath);

                FileInfo[] files = directory.GetFiles();

                List<string> oriNames = new List<string>();
                Dictionary<string, string> newNames = new Dictionary<string, string>();

                foreach (FileInfo info in files)
                {
                    using (FileStream stream = info.Open(FileMode.Open))
                    {
                        bool IsArt = false;
                        bool IsName = false;

                        #region Get Mp4 Info
                        var mp4Reader = new AtomReader(stream);
                        string value = mp4Reader.GetMetaAtomValue(AtomReader.TitleTypeName);
                        string value2 = mp4Reader.GetMetaAtomValue(AtomReader.IlstTypeName);
                        string value3 = mp4Reader.GetMetaAtomValue(AtomReader.MetaTypeName);
                        #endregion

                        string songName = "";
                        string AuthorName = "";

                        value3 = value3.Replace("\0", "");
                        value3 = value3.Replace("\u0001", "");
                        string[] list = value3.Split("data");

                        if (list != null)
                        {
                            foreach (string s in list)
                            {
                                string x = s;

                                #region Art Tag
                                if (IsArt)
                                {
                                    IsArt = !IsArt;

                                    if (hasQuesMarkTag(s))
                                    {
                                        if (hasTooTag(s))
                                        {
                                            int too = s.ToLower().IndexOf("too");
                                            AuthorName = CheckVaildFileName(s.Substring(0, too - 2));
                                        }
                                        else
                                        {
                                            x = s.Replace("�", "?");

                                            int test = x.IndexOf("?");

                                            AuthorName = CheckVaildFileName(s.Substring(0, test - 1));
                                        }
                                    }
                                    else
                                    {
                                        AuthorName = CheckVaildFileName(s);
                                    }

                                    Console.WriteLine("歌手名 : " + AuthorName);
                                }

                                if (hasArtTag(s)) { IsArt = true; }
                                #endregion

                                #region Nam Tag
                                if (IsName)
                                {
                                    IsName = !IsName;

                                    if (hasQuesMarkTag(s))
                                    {
                                        if (hasArtTag(s))
                                        {
                                            int art = s.ToLower().IndexOf("art");
                                            songName = CheckVaildFileName(s.Substring(0, art - 2));
                                        }
                                        else
                                        {
                                            x = s.Replace("�", "?");

                                            int test = x.IndexOf("?");

                                            songName = CheckVaildFileName(s.Substring(0, test - 1));
                                        }
                                    }
                                    else
                                    {
                                        songName = CheckVaildFileName(s);
                                    }
                                    Console.WriteLine("歌曲名 : " + songName);
                                }

                                if (hasNamTag(s)) { IsName = true; }
                                #endregion
                            }

                            Console.WriteLine("=========================");
                        }

                        oriNames.Add(info.FullName);
                        newNames.Add(info.FullName, AuthorName + "-" + songName);
                    }
                }

                foreach (string oriName in oriNames)
                {
                    try
                    {
                        if (File.Exists(oriName))
                        {
                            string strResult = DirPath + newNames[oriName] + ".mp4";
                            if (!File.Exists(DirPath + strResult))
                                File.Move(oriName, strResult);
                        }
                        else
                            Console.WriteLine("oriName ...");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("===================================");
                        Console.WriteLine(
                             "舊檔案檔案名稱 : " + oriName + "\r\n" +
                             "新檔案檔案名稱 : " + newNames[oriName] + "\r\n" +
                             "錯誤訊息 : "   +ex.Message);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("檔名轉換出錯：" + ex.Message);
            }

            Console.WriteLine("=========================");
            Console.WriteLine("全部執行結束 請按任意鍵退出視窗");
            Console.ReadKey();
        }

        private static bool hasArtTag(string strInput)
        {
            return Regex.IsMatch(strInput.ToLower(), @"[^A-z]art");
        }

        private static bool hasQuesMarkTag(string strInput)
        {
            return Regex.IsMatch(strInput.ToLower(), "�");
        }

        private static bool hasNamTag(string strInput)
        {
            return Regex.IsMatch(strInput.ToLower(), @"[^A-z]nam");
        }

        private static bool hasTooTag(string strInput)
        {
            return Regex.IsMatch(strInput.ToLower(), @"[^A-z]too");
        }

        private static string CheckVaildFileName(string strInput)
        {
            return strInput.Replace("/", " ")
                 .Replace("<", " ")
                 .Replace(">", " ")
                 .Replace("?", " ")
                 .Replace(":", " ")
                 .Replace("\"", " ")
                 .Replace("/", " ")
                 .Replace("\\", " ")
                 .Replace("|", " ")
                 .Replace("*", " ")
                 .Replace(";", " ");
        }
    }

    internal class Config
    {
        public ReNameDirConfig ReConfig;

        public Config()
        {
            string downloadConfigPath = Directory.GetCurrentDirectory() + "\\RenameMp4_config.json";
            if (File.Exists(downloadConfigPath))
            {
                using (var reader = new StreamReader(downloadConfigPath))
                {
                    string txt = reader.ReadToEnd();
                    ReConfig = JsonConvert.DeserializeObject<ReNameDirConfig>(txt);
                }
            }
            else
            {
                Console.WriteLine(string.Format("{0} not found", downloadConfigPath));
                ReConfig = new ReNameDirConfig()
                {
                    ReNameDirPath = string.Empty
                };
            }
        }
    }

    public class ReNameDirConfig
    {
        public string ReNameDirPath = string.Empty;
    }
}
