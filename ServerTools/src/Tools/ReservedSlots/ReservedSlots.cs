﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;

namespace ServerTools
{
    public class ReservedSlots
    {
        public static bool IsEnabled = false, IsRunning = false, Operating = false, Reduced_Delay = false, Admin_Slot = false, Bonus_Exp = false;
        public static int Session_Time = 30, Admin_Level = 0;
        public static string Command_reserved = "reserved";

        public static Dictionary<string, DateTime> Dict = new Dictionary<string, DateTime>();
        public static Dictionary<string, string> Dict1 = new Dictionary<string, string>();
        public static Dictionary<string, DateTime> Kicked = new Dictionary<string, DateTime>();

        private static string file = "ReservedSlots.xml";
        private static string FilePath = string.Format("{0}/{1}", API.ConfigPath, file);
        private static FileSystemWatcher FileWatcher = new FileSystemWatcher(API.ConfigPath, file);

        private static XmlNodeList OldNodeList;

        public static void Load()
        {
            LoadXml();
            InitFileWatcher();
        }

        public static void Unload()
        {
            Dict.Clear();
            Dict1.Clear();
            FileWatcher.Dispose();
            IsRunning = false;
        }

        private static void LoadXml()
        {
            try
            {
                if (!Utils.FileExists(FilePath))
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
                bool upgrade = true;
                XmlNodeList childNodes = xmlDoc.DocumentElement.ChildNodes;
                if (childNodes != null)
                {
                    Dict.Clear();
                    Dict1.Clear();
                    for (int i = 0; i < childNodes.Count; i++)
                    {
                        if (childNodes[i].NodeType != XmlNodeType.Comment)
                        {
                            XmlElement line = (XmlElement)childNodes[i];
                            if (line.HasAttributes)
                            {
                                if (line.HasAttribute("Version") && line.GetAttribute("Version") == Config.Version)
                                {
                                    upgrade = false;
                                    continue;
                                }
                                else if (line.HasAttribute("SteamId") && line.HasAttribute("Name") && line.HasAttribute("Expires"))
                                {
                                    if (!DateTime.TryParse(line.GetAttribute("Expires"), out DateTime dt))
                                    {
                                        Log.Warning(string.Format("[SERVERTOOLS] Ignoring ReservedSlots.xml entry. Invalid (date) value for 'Expires' attribute: {0}", line.OuterXml));
                                        continue;
                                    }
                                    if (!Dict.ContainsKey(line.GetAttribute("SteamId")))
                                    {
                                        Dict.Add(line.GetAttribute("SteamId"), dt);
                                    }
                                    if (!Dict1.ContainsKey(line.GetAttribute("SteamId")))
                                    {
                                        Dict1.Add(line.GetAttribute("SteamId"), line.GetAttribute("Name"));
                                    }
                                }
                            }
                        }
                    }
                }
                if (upgrade)
                {
                    XmlNodeList nodeList = xmlDoc.DocumentElement.ChildNodes;
                    XmlNode node = nodeList[0];
                    XmlElement line = (XmlElement)nodeList[0];
                    if (line != null)
                    {
                        if (line.HasAttributes)
                        {
                            OldNodeList = nodeList;
                            Utils.FileDelete(FilePath);
                            UpgradeXml();
                            return;
                        }
                        else
                        {
                            nodeList = node.ChildNodes;
                            line = (XmlElement)nodeList[0];
                            if (line != null)
                            {
                                if (line.HasAttributes)
                                {
                                    OldNodeList = nodeList;
                                    Utils.FileDelete(FilePath);
                                    UpgradeXml();
                                    return;
                                }
                            }
                            Utils.FileDelete(FilePath);
                            UpdateXml();
                            Log.Out(string.Format("[SERVERTOOLS] The existing ReservedSlots.xml was too old or misconfigured. File deleted and rebuilt for version {0}", Config.Version));
                        }
                    }
                }
            }
            catch (Exception e)
            {
                if (e.Message == "Specified cast is not valid.")
                {
                    Utils.FileDelete(FilePath);
                    UpdateXml();
                }
                else
                {
                    Log.Out(string.Format("[SERVERTOOLS] Error in ReservedSlots.LoadXml: {0}", e.Message));
                }
            }
        }

        public static void UpdateXml()
        {
            FileWatcher.EnableRaisingEvents = false;
            using (StreamWriter sw = new StreamWriter(FilePath, false, Encoding.UTF8))
            {
                sw.WriteLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
                sw.WriteLine("<ReservedSlots>");
                sw.WriteLine(string.Format("<ST Version=\"{0}\" />", Config.Version));
                sw.WriteLine("    <!-- <Player SteamId=\"76561191234567891\" Name=\"Tron\" Expires=\"10/29/2050 7:30:00 AM\" /> -->");
                sw.WriteLine();
                sw.WriteLine();
                if (Dict.Count > 0)
                {
                    foreach (KeyValuePair<string, DateTime> kvp in Dict)
                    {
                        Dict1.TryGetValue(kvp.Key, out string _name);
                        sw.WriteLine(string.Format("    <Player SteamId=\"{0}\" Name=\"{1}\" Expires=\"{2}\" />", kvp.Key, _name, kvp.Value.ToString()));
                    }
                }
                else
                {
                    sw.WriteLine(string.Format("    <!-- <Player SteamId=\"\" Name=\"\" Expires=\"\" /> -->"));
                }
                sw.WriteLine("</ReservedSlots>");
                sw.Flush();
                sw.Close();
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
            if (!Utils.FileExists(FilePath))
            {
                UpdateXml();
            }
            LoadXml();
        }

        public static bool ReservedCheck(string _id)
        {
            if (Dict.ContainsKey(_id))
            {
                Dict.TryGetValue(_id, out DateTime _dt);
                if (DateTime.Now < _dt)
                {
                    return true;
                }
            }
            return false;
        }

        public static void ReservedStatus(ClientInfo _cInfo)
        {
            if (Dict.ContainsKey(_cInfo.playerId))
            {
                if (Dict.TryGetValue(_cInfo.playerId, out DateTime _dt))
                {
                    if (DateTime.Now < _dt)
                    {
                        Phrases.Dict.TryGetValue("Reserved4", out string _phrase4);
                        _phrase4 = _phrase4.Replace("{DateTime}", _dt.ToString());
                        ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase4 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    }
                    else
                    {
                        Phrases.Dict.TryGetValue("Reserved5", out string _phrase5);
                        _phrase5 = _phrase5.Replace("{DateTime}", _dt.ToString());
                        ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase5 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    }
                }
            }
            else
            {
                Phrases.Dict.TryGetValue("Reserved6", out string _phrase6);
                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase6 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
            }
        }

        public static bool AdminCheck(string _steamId)
        {
            if (GameManager.Instance.adminTools.GetUserPermissionLevel(_steamId) <= Admin_Level)
            {
                return true;
            }
            return false;
        }

        public static bool FullServer(string _playerId)
        {
            try
            {
                List<string> reservedKicks = new List<string>();
                List<string> normalKicks = new List<string>();
                string clientToKick = null;
                List<ClientInfo> clientList = PersistentOperations.ClientList();
                if (clientList != null)
                {
                    if (AdminCheck(_playerId))//admin is joining
                    {
                        for (int i = 0; i < clientList.Count; i++)
                        {
                            ClientInfo cInfo2 = clientList[i];
                            if (cInfo2 != null && !string.IsNullOrEmpty(cInfo2.playerId) && cInfo2.playerId != _playerId)
                            {
                                if (!AdminCheck(cInfo2.playerId))//not admin
                                {
                                    if (ReservedCheck(cInfo2.playerId))//reserved player
                                    {
                                        reservedKicks.Add(cInfo2.playerId);
                                    }
                                    else
                                    {
                                        normalKicks.Add(cInfo2.playerId);
                                    }
                                }
                            }
                        }
                    }
                    else if (ReservedCheck(_playerId))//reserved player is joining
                    {
                        for (int i = 0; i < clientList.Count; i++)
                        {
                            ClientInfo cInfo2 = clientList[i];
                            if (cInfo2 != null && !string.IsNullOrEmpty(cInfo2.playerId) && cInfo2.playerId != _playerId)
                            {
                                if (!AdminCheck(cInfo2.playerId) && !ReservedCheck(cInfo2.playerId))
                                {
                                    normalKicks.Add(cInfo2.playerId);
                                }
                            }
                        }
                    }
                    else//regular player is joining
                    {
                        for (int i = 0; i < clientList.Count; i++)
                        {
                            ClientInfo cInfo2 = clientList[i];
                            if (cInfo2 != null && !string.IsNullOrEmpty(cInfo2.playerId) && cInfo2.playerId != _playerId)
                            {
                                if (!AdminCheck(cInfo2.playerId) && !ReservedCheck(cInfo2.playerId))
                                {
                                    if (Session_Time > 0)
                                    {
                                        if (PersistentOperations.Session.TryGetValue(cInfo2.playerId, out DateTime dateTime))
                                        {
                                            TimeSpan varTime = DateTime.Now - dateTime;
                                            double fractionalMinutes = varTime.TotalMinutes;
                                            int timepassed = (int)fractionalMinutes;
                                            if (timepassed >= Session_Time)
                                            {
                                                normalKicks.Add(cInfo2.playerId);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    if (normalKicks.Count > 0)
                    {
                        normalKicks.RandomizeList();
                        clientToKick = normalKicks[0];
                        if (Session_Time > 0)
                        {
                            Kicked.Add(clientToKick, DateTime.Now);
                        }
                        Phrases.Dict.TryGetValue("Reserved1", out string phrase1);
                        SdtdConsole.Instance.ExecuteSync(string.Format("kick {0} \"{1}\"", clientToKick, phrase1), null);
                        return true;
                    }
                    else if (reservedKicks.Count > 0)
                    {
                        reservedKicks.RandomizeList();
                        clientToKick = reservedKicks[0];
                        Phrases.Dict.TryGetValue("Reserved1", out string phrase1);
                        SdtdConsole.Instance.ExecuteSync(string.Format("kick {0} \"{1}\"", clientToKick, phrase1), null);
                        return true;
                    }
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in ReservedSlots.FullServer: {0}", e.Message));
            }
            return false;
        }

        private static void UpgradeXml()
        {
            try
            {
                FileWatcher.EnableRaisingEvents = false;
                using (StreamWriter sw = new StreamWriter(FilePath, false, Encoding.UTF8))
                {
                    sw.WriteLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
                    sw.WriteLine("<ReservedSlots>");
                    sw.WriteLine(string.Format("<ST Version=\"{0}\" />", Config.Version));
                    sw.WriteLine("    <!-- <Player SteamId=\"76561191234567891\" Name=\"Tron\" Expires=\"10/29/2050 7:30:00 AM\" /> -->");
                    for (int i = 0; i < OldNodeList.Count; i++)
                    {
                        if (OldNodeList[i].NodeType == XmlNodeType.Comment && !OldNodeList[i].OuterXml.Contains("<!-- <Player SteamId=\"\"") &&
                            !OldNodeList[i].OuterXml.Contains("<!-- <Player SteamId=\"76561191234567891\""))
                        {
                            sw.WriteLine(OldNodeList[i].OuterXml);
                        }
                    }
                    sw.WriteLine();
                    sw.WriteLine();
                    for (int i = 0; i < OldNodeList.Count; i++)
                    {
                        if (OldNodeList[i].NodeType != XmlNodeType.Comment)
                        {
                            XmlElement line = (XmlElement)OldNodeList[i];
                            if (line.HasAttributes && line.Name == "Player")
                            {
                                string steamId = "", name = "", expires = "";
                                if (line.HasAttribute("SteamId"))
                                {
                                    steamId = line.GetAttribute("SteamId");
                                }
                                if (line.HasAttribute("Name"))
                                {
                                    name = line.GetAttribute("Name");
                                }
                                if (line.HasAttribute("Expires"))
                                {
                                    expires = line.GetAttribute("Expires");
                                }
                                sw.WriteLine(string.Format("    <Player SteamId=\"{0}\" Name=\"{1}\" Expires=\"{2}\" />", steamId, name, expires));
                            }
                        }
                    }
                    sw.WriteLine("</ReservedSlots>");
                    sw.Flush();
                    sw.Close();
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in ReservedSlots.UpgradeXml: {0}", e.Message));
            }
            FileWatcher.EnableRaisingEvents = true;
            LoadXml();
        }
    }
}