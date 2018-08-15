﻿using System;
using System.Collections.Generic;
using System.Data;
using System.IO;

namespace ServerTools
{
    public class ResetPlayer : ConsoleCmdAbstract
    {
        public override string GetDescription()
        {
            return "[ServerTools]-Reset a players profile. Warning, can not be undone without a backup.";
        }
        public override string GetHelp()
        {
            return "Usage: resetplayerprofile <steamId/entityId>";
        }
        public override string[] GetCommands()
        {
            return new string[] { "st-ResetPlayerProfile", "resetplayerprofile", "rpp" };
        }
        public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
        {
            try
            {
                if (_params.Count != 1)
                {
                    SdtdConsole.Instance.Output(string.Format("Wrong number of arguments, expected 1, found {0}", _params.Count));
                    return;
                }
                if (_params[0].Length < 1 || _params[0].Length > 17)
                {
                    SdtdConsole.Instance.Output(string.Format("Can not reset Id: Invalid Id {0}", _params[0]));
                    return;
                }
                ClientInfo _cInfo = ConsoleHelper.ParseParamIdOrName(_params[0]);
                if (_cInfo != null)
                {
                    string _filepath = string.Format("{0}/Player/{1}.map", GameUtils.GetSaveGameDir(), _cInfo.playerId);
                    string _filepath1 = string.Format("{0}/Player/{1}.ttp", GameUtils.GetSaveGameDir(), _cInfo.playerId);
                    Player p = PersistentContainer.Instance.Players[_cInfo.playerId, false];
                    if (p != null)
                    {
                        string _phrase400;
                        if (!Phrases.Dict.TryGetValue(400, out _phrase400))
                        {
                            _phrase400 = "Reseting players profile.";
                        }
                        SdtdConsole.Instance.ExecuteSync(string.Format("kick {0} \"{1}\"", _cInfo.entityId, _phrase400), _cInfo);
                        if (!File.Exists(_filepath))
                        {
                            SdtdConsole.Instance.Output(string.Format("Could not find file {0}.map", _params[0]));
                        }
                        else
                        {
                            File.Delete(_filepath);
                        }
                        if (!File.Exists(_filepath1))
                        {
                            SdtdConsole.Instance.Output(string.Format("Could not find file {0}.ttp", _params[0]));
                        }
                        else
                        {
                            File.Delete(_filepath1);
                        }
                        PersistentContainer.Instance.Players[_cInfo.playerId, true].AuctionData = 0;
                        PersistentContainer.Instance.Players[_cInfo.playerId, true].StartingItems = false;
                        PersistentContainer.Instance.Players[_cInfo.playerId, true].IsClanOwner = false;
                        PersistentContainer.Instance.Players[_cInfo.playerId, true].IsClanOfficer = false;
                        PersistentContainer.Instance.Players[_cInfo.playerId, true].CancelTime = DateTime.Now.AddDays(-5);
                        PersistentContainer.Instance.Players[_cInfo.playerId, true].SellDate = DateTime.Now.AddDays(-5);
                        PersistentContainer.Instance.Players[_cInfo.playerId, true].CustomCommand1 = DateTime.Now.AddDays(-5);
                        PersistentContainer.Instance.Players[_cInfo.playerId, true].CustomCommand2 = DateTime.Now.AddDays(-5);
                        PersistentContainer.Instance.Players[_cInfo.playerId, true].CustomCommand3 = DateTime.Now.AddDays(-5);
                        PersistentContainer.Instance.Players[_cInfo.playerId, true].CustomCommand4 = DateTime.Now.AddDays(-5);
                        PersistentContainer.Instance.Players[_cInfo.playerId, true].CustomCommand5 = DateTime.Now.AddDays(-5);
                        PersistentContainer.Instance.Players[_cInfo.playerId, true].CustomCommand6 = DateTime.Now.AddDays(-5);
                        PersistentContainer.Instance.Players[_cInfo.playerId, true].CustomCommand7 = DateTime.Now.AddDays(-5);
                        PersistentContainer.Instance.Players[_cInfo.playerId, true].CustomCommand8 = DateTime.Now.AddDays(-5);
                        PersistentContainer.Instance.Players[_cInfo.playerId, true].CustomCommand9 = DateTime.Now.AddDays(-5);
                        PersistentContainer.Instance.Players[_cInfo.playerId, true].CustomCommand10 = DateTime.Now.AddDays(-5);
                        PersistentContainer.Instance.Players[_cInfo.playerId, true].ClanName = null;
                        PersistentContainer.Instance.Players[_cInfo.playerId, true].InvitedToClan = null;
                        PersistentContainer.Instance.Save();
                        string _sql = string.Format("SELECT last_gimme FROM Players WHERE steamid = '{0}'", _cInfo.playerId);
                        DataTable _result = SQL.TQuery(_sql);
                        if (_result.Rows.Count != 0)
                        {
                            _sql = string.Format("UPDATE Players SET " +
                                "playername = 'Unknown', " +
                                "last_gimme = '10/29/2000 7:30:00 AM', " +
                                "lastkillme = '10/29/2000 7:30:00 AM', " +
                                "playerSpentCoins = 0, " +
                                "sessionTime = 0, " +
                                "bikeId = 0, " +
                                "lastBike = '10/29/2000 7:30:00 AM', " +
                                "jailName = 'Unknown', " +
                                "jailDate = '10/29/2000 7:30:00 AM', " +
                                "muteName = 'Unknown', " +
                                "muteDate = '10/29/2000 7:30:00 AM', " +
                                "lobbyReturn = 'Unknown', " +
                                "newTeleSpawn = 'Unknown', " +
                                "homeposition = 'Unknown', " +
                                "homeposition2 = 'Unknown', " +
                                "lastsethome = '10/29/2000 7:30:00 AM', " +
                                "lastwhisper = 'Unknown', " +
                                "lastStuck = '10/29/2000 7:30:00 AM', " +
                                "lastLobby = '10/29/2000 7:30:00 AM', " +
                                "lastLog = '10/29/2000 7:30:00 AM', " +
                                "lastBackpack = '10/29/2000 7:30:00 AM', " +
                                "lastFriendTele = '10/29/2000 7:30:00 AM', " +
                                "respawnTime = '10/29/2000 7:30:00 AM', " +
                                "lastTravel = '10/29/2000 7:30:00 AM', " +
                                "lastAnimals = '10/29/2000 7:30:00 AM', " +
                                "lastVoteReward = '10/29/2000 7:30:00 AM', " +
                                "firstClaim = 'false', " +
                                "ismuted = 'false', " +
                                "isjailed = 'false' " +
                                "WHERE steamid = '{0}'", _cInfo.playerId);
                            SQL.FastQuery(_sql);
                        }
                        _result.Dispose();
                        string _phrase401;
                        if (!Phrases.Dict.TryGetValue(401, out _phrase401))
                        {
                            _phrase401 = "You have reset the profile for Player {SteamId}.";
                        }
                        _phrase401 = _phrase401.Replace("{SteamId}", _params[0]);
                        SdtdConsole.Instance.Output(string.Format("{0}", _phrase401));
                    }
                    else
                    {
                        SdtdConsole.Instance.Output(string.Format("Player file {0}.ttp does not exist", _params[0]));
                    }
                }
                else
                {
                    int _value = 0;
                    if (int.TryParse(_params[0], out _value))
                    {
                        string _filepath = string.Format("{0}/Player/{1}.map", GameUtils.GetSaveGameDir(), _value.ToString());
                        string _filepath1 = string.Format("{0}/Player/{1}.ttp", GameUtils.GetSaveGameDir(), _value.ToString());
                        Player p = PersistentContainer.Instance.Players[_value.ToString(), false];
                        if (p != null)
                        {
                            if (!File.Exists(_filepath))
                            {
                                SdtdConsole.Instance.Output(string.Format("Could not find file {0}.map", _params[0]));
                            }
                            else
                            {
                                File.Delete(_filepath);
                            }
                            if (!File.Exists(_filepath1))
                            {
                                SdtdConsole.Instance.Output(string.Format("Could not find file {0}.ttp", _params[0]));
                            }
                            else
                            {
                                File.Delete(_filepath1);
                            }
                            PersistentContainer.Instance.Players[_value.ToString(), true].AuctionData = 0;
                            PersistentContainer.Instance.Players[_value.ToString(), true].StartingItems = false;
                            PersistentContainer.Instance.Players[_value.ToString(), true].IsClanOwner = false;
                            PersistentContainer.Instance.Players[_value.ToString(), true].IsClanOfficer = false;
                            PersistentContainer.Instance.Players[_value.ToString(), true].CancelTime = DateTime.Now.AddDays(-5);
                            PersistentContainer.Instance.Players[_value.ToString(), true].SellDate = DateTime.Now.AddDays(-5);
                            PersistentContainer.Instance.Players[_value.ToString(), true].CustomCommand1 = DateTime.Now.AddDays(-5);
                            PersistentContainer.Instance.Players[_value.ToString(), true].CustomCommand2 = DateTime.Now.AddDays(-5);
                            PersistentContainer.Instance.Players[_value.ToString(), true].CustomCommand3 = DateTime.Now.AddDays(-5);
                            PersistentContainer.Instance.Players[_value.ToString(), true].CustomCommand4 = DateTime.Now.AddDays(-5);
                            PersistentContainer.Instance.Players[_value.ToString(), true].CustomCommand5 = DateTime.Now.AddDays(-5);
                            PersistentContainer.Instance.Players[_value.ToString(), true].CustomCommand6 = DateTime.Now.AddDays(-5);
                            PersistentContainer.Instance.Players[_value.ToString(), true].CustomCommand7 = DateTime.Now.AddDays(-5);
                            PersistentContainer.Instance.Players[_value.ToString(), true].CustomCommand8 = DateTime.Now.AddDays(-5);
                            PersistentContainer.Instance.Players[_value.ToString(), true].CustomCommand9 = DateTime.Now.AddDays(-5);
                            PersistentContainer.Instance.Players[_value.ToString(), true].CustomCommand10 = DateTime.Now.AddDays(-5);
                            PersistentContainer.Instance.Players[_value.ToString(), true].ClanName = null;
                            PersistentContainer.Instance.Players[_value.ToString(), true].InvitedToClan = null;
                            PersistentContainer.Instance.Save();
                            string _sql = string.Format("SELECT last_gimme FROM Players WHERE steamid = '{0}'", _value.ToString());
                            DataTable _result = SQL.TQuery(_sql);
                            if (_result.Rows.Count != 0)
                            {
                                _sql = string.Format("UPDATE Players SET " +
                                    "playername = 'Unknown', " +
                                    "last_gimme = '10/29/2000 7:30:00 AM', " +
                                    "lastkillme = '10/29/2000 7:30:00 AM', " +
                                    "playerSpentCoins = 0, " +
                                    "sessionTime = 0, " +
                                    "bikeId = 0, " +
                                    "lastBike = '10/29/2000 7:30:00 AM', " +
                                    "jailName = 'Unknown', " +
                                    "jailDate = '10/29/2000 7:30:00 AM', " +
                                    "muteName = 'Unknown', " +
                                    "muteDate = '10/29/2000 7:30:00 AM', " +
                                    "lobbyReturn = 'Unknown', " +
                                    "newTeleSpawn = 'Unknown', " +
                                    "homeposition = 'Unknown', " +
                                    "homeposition2 = 'Unknown', " +
                                    "lastsethome = '10/29/2000 7:30:00 AM', " +
                                    "lastwhisper = 'Unknown', " +
                                    "lastStuck = '10/29/2000 7:30:00 AM', " +
                                    "lastLobby = '10/29/2000 7:30:00 AM', " +
                                    "lastLog = '10/29/2000 7:30:00 AM', " +
                                    "lastDied = '10/29/2000 7:30:00 AM', " +
                                    "lastFriendTele = '10/29/2000 7:30:00 AM', " +
                                    "respawnTime = '10/29/2000 7:30:00 AM', " +
                                    "lastTravel = '10/29/2000 7:30:00 AM', " +
                                    "lastAnimals = '10/29/2000 7:30:00 AM', " +
                                    "lastVoteReward = '10/29/2000 7:30:00 AM', " +
                                    "firstClaim = 'false', " +
                                    "ismuted = 'false', " +
                                    "isjailed = 'false' " +
                                    "WHERE steamid = '{0}'", _value.ToString());
                                SQL.FastQuery(_sql);
                            }
                            _result.Dispose();
                            string _phrase401;
                            if (!Phrases.Dict.TryGetValue(401, out _phrase401))
                            {
                                _phrase401 = "You have reset the profile for Player {SteamId}.";
                            }
                            _phrase401 = _phrase401.Replace("{SteamId}", _params[0]);
                            SdtdConsole.Instance.Output(string.Format("{0}", _phrase401));
                        }
                        else
                        {
                            SdtdConsole.Instance.Output(string.Format("Player file {0}.ttp does not exist", _params[0]));
                        }
                    }
                    else
                    {
                        SdtdConsole.Instance.Output(string.Format("Player id {0} is not a valid integer", _params[0]));
                        return;
                    }
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in ResetPlayer.Run: {0}.", e));
            }
        }
    }
}