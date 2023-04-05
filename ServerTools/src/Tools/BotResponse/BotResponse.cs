﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;

namespace ServerTools
{
    class BotResponse
    {
        public static bool IsEnabled = false, IsRunning = false, Whisper = false;

        public static Dictionary<string, string[]> Dict1 = new Dictionary<string, string[]>();
        public static Dictionary<string, string[]> Dict = new Dictionary<string, string[]>();

        private const string file = "BotResponse.xml";
        private static readonly string FilePath = string.Format("{0}/{1}", API.ConfigPath, file);
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
            FileWatcher.Dispose();
            IsRunning = false;
        }

        public static void LoadXml()
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
                if (childNodes != null)
                {
                    Dict.Clear();
                    if (childNodes[0] != null && childNodes[0].OuterXml.Contains("Version") && childNodes[0].OuterXml.Contains(Config.Version))
                    {
                        for (int i = 0; i < childNodes.Count; i++)
                        {
                            if (childNodes[i].NodeType == XmlNodeType.Comment)
                            {
                                continue;
                            }
                            XmlElement line = (XmlElement)childNodes[i];
                            if (!line.HasAttributes)
                            {
                                continue;
                            }
                            if (line.HasAttribute("Message") && line.HasAttribute("Response") && line.HasAttribute("Exact") && line.HasAttribute("Whisper"))
                            {
                                string message = line.GetAttribute("Message").ToLower();
                                if (message == "")
                                {
                                    continue;
                                }
                                string response = line.GetAttribute("Response");
                                if (bool.TryParse(line.GetAttribute("Exact"), out bool exact))
                                {
                                    if (bool.TryParse(line.GetAttribute("Whisper"), out bool whisper))
                                    {
                                        string[] values = { response, exact.ToString().ToLower(), whisper.ToString().ToLower() };
                                        if (!Dict.ContainsKey(message))
                                        {
                                            Dict.Add(message, values);
                                        }
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        XmlNodeList nodeList = xmlDoc.DocumentElement.ChildNodes;
                        XmlNode node = nodeList[0];
                        XmlElement line = (XmlElement)nodeList[0];
                        if (line != null)
                        {
                            if (line.HasAttributes)
                            {
                                OldNodeList = nodeList;
                                File.Delete(FilePath);
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
                                        File.Delete(FilePath);
                                        UpgradeXml();
                                        return;
                                    }
                                }
                                File.Delete(FilePath);
                                UpdateXml();
                                Log.Out(string.Format("[SERVERTOOLS] The existing BotResponse.xml was too old or misconfigured. File deleted and rebuilt for version {0}", Config.Version));
                            }
                        }
                    }
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
                    Log.Out(string.Format("[SERVERTOOLS] Error in BotResponse.LoadXml: {0}", e.Message));
                }
            }
        }

        private static void UpdateXml()
        {
            try
            {
                FileWatcher.EnableRaisingEvents = false;
                using (StreamWriter sw = new StreamWriter(FilePath, false, Encoding.UTF8))
                {
                    sw.WriteLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
                    sw.WriteLine("<BotResponse>");
                    sw.WriteLine("    <!-- <Version=\"{0}\" /> -->", Config.Version);
                    sw.WriteLine("    <!-- <Chat Message=\"Any admin on\" Response=\"From the skies comes a bolt of lightning\" /> -->");
                    sw.WriteLine("    <Chat Message=\"\" Response=\"\" />");
                    if (Dict.Count > 0)
                    {
                        foreach (KeyValuePair<string, string[]> kvp in Dict)
                        {
                            sw.WriteLine(string.Format("    <Chat Message=\"{0}\" Response=\"{1}\" Exact=\"{2}\" Whisper=\"{3}\" />", kvp.Key, kvp.Value[0], kvp.Value[1], kvp.Value[2]));
                        }
                    }
                    sw.WriteLine("</BotResponse>");
                    sw.Flush();
                    sw.Close();
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in BotResponse.UpdateXml: {0}", e.Message));
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

        private static void UpgradeXml()
        {
            try
            {
                FileWatcher.EnableRaisingEvents = false;
                using (StreamWriter sw = new StreamWriter(FilePath, false, Encoding.UTF8))
                {
                    sw.WriteLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
                    sw.WriteLine("<BotResponse>");
                    sw.WriteLine("    <!-- <Version=\"{0}\" /> -->", Config.Version);
                    sw.WriteLine("    <!-- <Chat Message=\"Any admin on\" Response=\"From the skies comes a bolt of lightning\" /> -->");
                    for (int i = 0; i < OldNodeList.Count; i++)
                    {
                        if (OldNodeList[i].NodeType == XmlNodeType.Comment && !OldNodeList[i].OuterXml.Contains("<!-- <Chat Message=\"Any admin on\"") &&
                            !OldNodeList[i].OuterXml.Contains("<!-- <Version"))
                        {
                            sw.WriteLine(OldNodeList[i].OuterXml);
                        }
                    }
                    sw.WriteLine("    <Chat Message=\"\" Response=\"\" />");
                    for (int i = 0; i < OldNodeList.Count; i++)
                    {
                        if (OldNodeList[i].NodeType != XmlNodeType.Comment)
                        {
                            XmlElement line = (XmlElement)OldNodeList[i];
                            if (line.HasAttributes && line.Name == "Chat")
                            {
                                string message = "", response = "", exact = "", whisper = "";
                                if (line.HasAttribute("Message"))
                                {
                                    message = line.GetAttribute("Message");
                                }
                                if (line.HasAttribute("Response"))
                                {
                                    response = line.GetAttribute("Response");
                                }
                                if (line.HasAttribute("Exact"))
                                {
                                    exact = line.GetAttribute("Exact");
                                }
                                if (line.HasAttribute("Whisper"))
                                {
                                    whisper = line.GetAttribute("Whisper");
                                }
                                sw.WriteLine(string.Format("    <Chat Message=\"{0}\" Response=\"{1}\" Exact=\"{2}\" Whisper=\"{3}\" />", message, response, exact, whisper));
                            }
                        }
                    }
                    sw.WriteLine("</BotResponse>");
                    sw.Flush();
                    sw.Close();
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in BotResponse.UpgradeXml: {0}", e.Message));
            }
            FileWatcher.EnableRaisingEvents = true;
            LoadXml();
        }
    }
}
