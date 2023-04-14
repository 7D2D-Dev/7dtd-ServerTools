﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;

namespace ServerTools
{
    public class WatchList
    {
        public static bool IsEnabled = false, IsRunning = false;
        public static int Admin_Level = 0;
        public static string Delay = "5";

        public static SortedDictionary<string, string> Dict = new SortedDictionary<string, string>();

        private static string EventDelay = "";
        private static string file = "WatchList.xml";
        private static string FilePath = string.Format("{0}/{1}", API.ConfigPath, file);
        private static FileSystemWatcher FileWatcher = new FileSystemWatcher(API.ConfigPath, file);

        public static void Load()
        {
            LoadXml();
            InitFileWatcher();
        }

        public static void Unload()
        {
            Dict.Clear();
            FileWatcher.Dispose();
            IsRunning = false;
        }

        private static void LoadXml()
        {
            try
            {
                if (!File.Exists(FilePath))
                {
                    UpdateXml();
                }
                XmlDocument xmlDoc = new XmlDocument();
                try
                {
                    xmlDoc.Load(FilePath);
                }
                catch (XmlException e)
                {
                    Log.Error(string.Format("[SERVERTOOLS] Failed loading {0}: {1}", file, e.Message));
                    return;
                }
                XmlNodeList childNodes = xmlDoc.DocumentElement.ChildNodes;
                Dict.Clear();
                if (childNodes != null && childNodes[0] != null && childNodes[0].OuterXml.Contains("Version") && childNodes[0].OuterXml.Contains(Config.Version))
                {
                    for (int i = 0; i < childNodes.Count; i++)
                    {
                        if (childNodes[i].NodeType != XmlNodeType.Comment)
                        {
                            XmlElement line = (XmlElement)childNodes[i];
                            if (!line.HasAttributes)
                            {
                                continue;
                            }
                            if (line.HasAttribute("Id") && line.HasAttribute("Reason"))
                            {
                                string id = line.GetAttribute("Id");
                                if (id == "")
                                {
                                    continue;
                                }
                                string reason = line.GetAttribute("Reason");
                                if (!Dict.ContainsKey(line.GetAttribute("Id")))
                                {
                                    Dict.Add(id, reason);
                                }
                            }
                        }
                    }
                }
                else
                {
                    XmlNodeList nodeList = xmlDoc.DocumentElement.ChildNodes;
                    if (nodeList != null)
                    {
                        File.Delete(FilePath);
                        UpgradeXml(nodeList);
                        return;
                    }
                    File.Delete(FilePath);
                    UpdateXml();
                    return;
                }
            }
            catch (Exception e)
            {
                if (e.Message == "Specified cast is not valid.")
                {
                    File.Delete(FilePath);
                    UpdateXml();
                }
                else
                {
                    Log.Out(string.Format("[SERVERTOOLS] Error in WatchList.LoadXml: {0}", e.Message));
                }
            }
        }

        public static void UpdateXml()
        {
            try
            {
                FileWatcher.EnableRaisingEvents = false;
                using (StreamWriter sw = new StreamWriter(FilePath, false, Encoding.UTF8))
                {
                    sw.WriteLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
                    sw.WriteLine("<Watchlist>");
                    sw.WriteLine(string.Format("    <!-- <Version=\"{0}\" /> -->", Config.Version));
                    sw.WriteLine("    <!-- <Player Id=\"Steam_12345678909876543\" Reason=\"Suspected cheating\" /> -->");
                    sw.WriteLine("    <!-- <Player Id=\"EOS_1a3b5c7a9b1c3a5b7c9a1b3c5a7b9c1a3\" Reason=\"Cheaters R Assho\" /> -->");
                    sw.WriteLine();
                    sw.WriteLine();
                    sw.WriteLine("    <Player Id=\"\" Reason=\"\" />");
                    if (Dict.Count > 0)
                    {
                        foreach (KeyValuePair<string, string> kvp in Dict)
                        {
                            sw.WriteLine(string.Format("    <Player Id=\"{0}\" Reason=\"{1}\" />", kvp.Key, kvp.Value));
                        }
                    }
                    sw.WriteLine("</Watchlist>");
                    sw.Flush();
                    sw.Close();
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in WatchList.UpdateXml: {0}", e.Message));
            }
            FileWatcher.EnableRaisingEvents = true;
        }

        private static void InitFileWatcher()
        {
            FileWatcher.Changed += new FileSystemEventHandler(OnFileChanged);
            FileWatcher.Created += new FileSystemEventHandler(OnFileChanged);
            FileWatcher.Deleted += new FileSystemEventHandler(OnFileChanged);
            FileWatcher.EnableRaisingEvents = true;
            IsRunning = true;
        }

        private static void OnFileChanged(object source, FileSystemEventArgs e)
        {
            if (!File.Exists(FilePath))
            {
                UpdateXml();
            }
            LoadXml();
        }

        public static void SetDelay(bool _reset)
        {
            if (EventDelay != Delay || _reset)
            {
                EventDelay = Delay;
                EventSchedule.Clear("WatchList_");
                if (Delay.Contains(",") && Delay.Contains(":"))
                {
                    string[] times = Delay.Split(',');
                    for (int i = 0; i < times.Length; i++)
                    {
                        string[] timeSplit = times[i].Split(':');
                        int.TryParse(timeSplit[0], out int hours);
                        int.TryParse(timeSplit[1], out int minutes);
                        DateTime time = DateTime.Today.AddHours(hours).AddMinutes(minutes);
                        EventSchedule.Schedule.Add("WatchList_" + time, time);
                    }
                }
                else if (Delay.Contains(":"))
                {
                    string[] timeSplit = Delay.Split(':');
                    int.TryParse(timeSplit[0], out int hours);
                    int.TryParse(timeSplit[1], out int minutes);
                    DateTime time = DateTime.Today.AddHours(hours).AddMinutes(minutes);
                    EventSchedule.Schedule.Add("WatchList_" + time, time);
                }
                else
                {
                    if (int.TryParse(Delay, out int delay))
                    {
                        DateTime time = DateTime.Now.AddMinutes(delay);
                        EventSchedule.Schedule.Add("WatchList_" + time, time);
                    }
                    else
                    {
                        Log.Out(string.Format("[SERVERTOOLS] Invalid Shutdown Time detected. Use a single integer, 24h time or multiple 24h time entries"));
                        Log.Out(string.Format("[SERVERTOOLS] Example: 120 or 03:00 or 03:00, 06:00, 09:00"));
                    }
                }
            }
        }

        public static void Exec()
        {
            try
            {
                List<ClientInfo> clients = GeneralOperations.ClientList();
                if (clients != null && clients.Count > 0)
                {
                    List<ClientInfo> admin = GeneralOperations.ClientList();
                    List<ClientInfo> player = GeneralOperations.ClientList();
                    for (int i = 0; i < clients.Count; i++)
                    {
                        ClientInfo cInfo = clients[i];
                        if (cInfo != null && cInfo.PlatformId != null && cInfo.CrossplatformId != null)
                        {
                            if (GameManager.Instance.adminTools.GetUserPermissionLevel(cInfo.PlatformId) > Admin_Level &&
                                GameManager.Instance.adminTools.GetUserPermissionLevel(cInfo.CrossplatformId) > Admin_Level)
                            {
                                if (Dict.ContainsKey(cInfo.PlatformId.CombinedString) ||
                                Dict.ContainsKey(cInfo.CrossplatformId.CombinedString))
                                {
                                    player.Add(cInfo);
                                }
                            }
                            else
                            {
                                admin.Add(cInfo);
                            }
                        }
                    }
                    if (admin.Count > 0 && player.Count > 0)
                    {
                        for (int i = 0; i < player.Count; i++)
                        {
                            Phrases.Dict.TryGetValue("Watchlist1", out string phrase);
                            if (Dict.TryGetValue(player[i].PlatformId.CombinedString, out string reason))
                            {
                                phrase = phrase.Replace("{PlayerName}", player[i].playerName);
                                phrase = phrase.Replace("{Reason}", reason);
                                for (int j = 0; j < admin.Count; j++)
                                {
                                    ChatHook.ChatMessage(admin[j], Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                                }
                            }
                            else if (Dict.TryGetValue(player[i].CrossplatformId.CombinedString, out reason))
                            {
                                phrase = phrase.Replace("{PlayerName}", player[i].playerName);
                                phrase = phrase.Replace("{Reason}", reason);
                                for (int j = 0; j < admin.Count; j++)
                                {
                                    ChatHook.ChatMessage(admin[j], Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in WatchList.List: {0}", e.Message));
            }
        }

        private static void UpgradeXml(XmlNodeList nodeList)
        {
            try
            {
                FileWatcher.EnableRaisingEvents = false;
                File.Delete(FilePath);
                using (StreamWriter sw = new StreamWriter(FilePath, false, Encoding.UTF8))
                {
                    sw.WriteLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
                    sw.WriteLine("<Watchlist>");
                    sw.WriteLine("    <!-- <Version=\"{0}\" /> -->", Config.Version);
                    sw.WriteLine("    <!-- <Player Id=\"Steam_12345678909876543\" Reason=\"Suspected cheating.\" /> -->");
                    sw.WriteLine("    <!-- <Player Id=\"EOS_1a3b5c7a9b1c3a5b7c9a1b3c5a7b9c1a3\" Reason=\"Cheaters R Assho\" /> -->");
                    sw.WriteLine("    <Player Id=\"\" Reason=\"\" />");
                    for (int i = 0; i < nodeList.Count; i++)
                    {
                        if (nodeList[i].NodeType == XmlNodeType.Comment && !nodeList[i].OuterXml.Contains("<!-- <Player Id=\"Steam_12345678909876543") &&
                            !nodeList[i].OuterXml.Contains("<!-- <Player Id=\"EOS_1a3b5c7a9b1c3a5b7c9a1b3c5a7b9c1a3") && !nodeList[i].OuterXml.Contains("<Player Id=\"") &&
                            !nodeList[i].OuterXml.Contains("<!-- <Version"))
                        {
                            sw.WriteLine(nodeList[i].OuterXml);
                        }
                    }
                    for (int i = 0; i < nodeList.Count; i++)
                    {
                        if (nodeList[i].NodeType != XmlNodeType.Comment)
                        {
                            XmlElement line = (XmlElement)nodeList[i];
                            if (line.HasAttributes && line.Name == "Player")
                            {
                                string id = "", reason = "";
                                if (line.HasAttribute("Id"))
                                {
                                    id = line.GetAttribute("Id");
                                }
                                if (line.HasAttribute("Reason"))
                                {
                                    reason = line.GetAttribute("Reason");
                                }
                                sw.WriteLine(string.Format("    <Player Id=\"{0}\" Reason=\"{1}\" />", id, reason));
                            }
                        }
                    }
                    sw.WriteLine("</Watchlist>");
                    sw.Flush();
                    sw.Close();
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in WatchList.UpgradeXml: {0}", e.Message));
            }
            FileWatcher.EnableRaisingEvents = true;
            LoadXml();
        }
    }
}