﻿using System.Collections.Generic;
using System.Timers;
using UnityEngine;

namespace ServerTools
{
    class Timers
    {
        public static bool CoreIsRunning = false, HalfSecondIsRunning = false;
        public static int StopServerMinutes = 0, eventTime = 0;
        private static int twoSecondTick, fiveSecondTick, tenSecondTick, twentySecondTick, oneMinTick, fiveMinTick, stopServerSeconds, eventInvitation,
            eventOpen, horde, kickVote, muteVote, newPlayer, restartVote, bloodMoans;

        private static readonly System.Timers.Timer Core = new System.Timers.Timer();
        private static readonly System.Timers.Timer HalfSecond = new System.Timers.Timer();

        public static void CoreTimerStart()
        {
            CoreIsRunning = true;
            Core.Interval = 1000;
            Core.Start();
            Core.Elapsed += new ElapsedEventHandler(Tick);
        }

        public static void CoreTimerStop()
        {
            CoreIsRunning = false;
            Core.Stop();
            Core.Close();
            Core.Dispose();
        }

        public static void HalfSecondTimerStart()
        {
            HalfSecondIsRunning = true;
            HalfSecond.Interval = 500;
            HalfSecond.Start();
            HalfSecond.Elapsed += new ElapsedEventHandler(HalfSecondElapsed);
        }

        public static void HalfSecondTimerStop()
        {
            HalfSecondIsRunning = false;
            HalfSecond.Stop();
            HalfSecond.Close();
            HalfSecond.Dispose();
        }

        private static void Tick(object sender, ElapsedEventArgs e)
        {
            twoSecondTick++;
            fiveSecondTick++;
            tenSecondTick++;
            twentySecondTick++;
            oneMinTick++;
            fiveMinTick++;
            Exec();
        }

        private static void HalfSecondElapsed(object sender, ElapsedEventArgs e)
        {
            
            if (!PlayerChecks.HalfSecondRunning)
            {
                PlayerChecks.HalfSecondExec();
            }
        }

        public static void Custom_SingleUseTimer(int _delay, string _playerId, List<string> _commands, string _trigger)
        {
            if (_delay > 180)
            {
                _delay = 180;
            }
            int delayAdjusted = _delay * 1000;
            System.Timers.Timer singleUseTimer = new System.Timers.Timer(delayAdjusted)
            {
                AutoReset = false
            };
            singleUseTimer.Start();
            singleUseTimer.Elapsed += (sender, e) =>
            {
                Init1(_playerId, _commands, _trigger);
                singleUseTimer.Stop();
                singleUseTimer.Close();
                singleUseTimer.Dispose();
            };
        }

        public static void Wallet_Add_SingleUseTimer(string _playerId, int _amount, bool _allowed)
        {
            System.Timers.Timer singleUseTimer = new System.Timers.Timer(1000)
            {
                AutoReset = false
            };
            singleUseTimer.Start();
            singleUseTimer.Elapsed += (sender, e) =>
            {
                Init2(_playerId, _amount, _allowed);
                singleUseTimer.Stop();
                singleUseTimer.Close();
                singleUseTimer.Dispose();
            };
        }

        public static void StartingItemsTimer(ClientInfo _cInfo)
        {
            System.Timers.Timer singleUseTimer = new System.Timers.Timer(3000)
            {
                AutoReset = false
            };
            singleUseTimer.Start();
            singleUseTimer.Elapsed += (sender, e) =>
            {
                Init3(_cInfo);
                singleUseTimer.Stop();
                singleUseTimer.Close();
                singleUseTimer.Dispose();
            };
        }

        public static void DisconnectHardcorePlayer(ClientInfo _cInfo)
        {
            System.Timers.Timer singleUseTimer = new System.Timers.Timer(20000)
            {
                AutoReset = false
            };
            singleUseTimer.Start();
            singleUseTimer.Elapsed += (sender, e) =>
            {
                Init4(_cInfo);
                singleUseTimer.Stop();
                singleUseTimer.Close();
                singleUseTimer.Dispose();
            };
        }

        public static void ExitWithCommand(int _id, int _time)
        {
            System.Timers.Timer singleUseTimer = new System.Timers.Timer(_time * 1000)
            {
                AutoReset = false
            };
            singleUseTimer.Start();
            singleUseTimer.Elapsed += (sender, e) =>
            {
                Init5(_id);
                singleUseTimer.Stop();
                singleUseTimer.Close();
                singleUseTimer.Dispose();
            };
        }

        public static void ExitWithoutCommand(ClientInfo _cInfo, string _ip)
        {
            System.Timers.Timer singleUseTimer = new System.Timers.Timer(1500)
            {
                AutoReset = false
            };
            singleUseTimer.Start();
            singleUseTimer.Elapsed += (sender, e) =>
            {
                Init6(_cInfo, _ip);
                singleUseTimer.Stop();
                singleUseTimer.Close();
                singleUseTimer.Dispose();
            };
        }

        public static void Zone_SingleUseTimer(int _delay, string _playerId, List<string> _commands)
        {
            if (_delay > 180)
            {
                _delay = 180;
            }
            int delayAdjusted = _delay * 1000;
            System.Timers.Timer singleUseTimer = new System.Timers.Timer(delayAdjusted)
            {
                AutoReset = false
            };
            singleUseTimer.Start();
            singleUseTimer.Elapsed += (sender, e) =>
            {
                Init7(_playerId, _commands);
                singleUseTimer.Stop();
                singleUseTimer.Close();
                singleUseTimer.Dispose();
            };
        }

        public static void Level_SingleUseTimer(int _delay, string _playerId, List<string> _commands)
        {
            if (_delay > 180)
            {
                _delay = 180;
            }
            int delayAdjusted = _delay * 1000;
            System.Timers.Timer singleUseTimer = new System.Timers.Timer(delayAdjusted)
            {
                AutoReset = false
            };
            singleUseTimer.Start();
            singleUseTimer.Elapsed += (sender, e) =>
            {
                Init8(_playerId, _commands);
                singleUseTimer.Stop();
                singleUseTimer.Close();
                singleUseTimer.Dispose();
            };
        }

        public static void Chunk_Region_ResetTimer()
        {
            System.Timers.Timer singleUseTimer = new System.Timers.Timer(10000)
            {
                AutoReset = false
            };
            singleUseTimer.Start();
            singleUseTimer.Elapsed += (sender, e) =>
            {
                Init9();
                singleUseTimer.Stop();
                singleUseTimer.Close();
                singleUseTimer.Dispose();
            };
        }

        public static void Wallet_Remove_SingleUseTimer(string _playerId, int _amount)
        {
            System.Timers.Timer singleUseTimer = new System.Timers.Timer(1000)
            {
                AutoReset = false
            };
            singleUseTimer.Start();
            singleUseTimer.Elapsed += (sender, e) =>
            {
                Init10(_playerId, _amount);
                singleUseTimer.Stop();
                singleUseTimer.Close();
                singleUseTimer.Dispose();
            };
        }

        public static void Rio_SingleUseTimer(string _ip)
        {
            System.Timers.Timer singleUseTimer = new System.Timers.Timer(60000)
            {
                AutoReset = false
            };
            singleUseTimer.Start();
            singleUseTimer.Elapsed += (sender, e) =>
            {
                Init11(_ip);
                singleUseTimer.Stop();
                singleUseTimer.Close();
                singleUseTimer.Dispose();
            };
        }

        public static void Speed_SingleUseTimer(ClientInfo _cInfo)
        {
            System.Timers.Timer singleUseTimer = new System.Timers.Timer(2000)
            {
                AutoReset = false
            };
            singleUseTimer.Start();
            singleUseTimer.Elapsed += (sender, e) =>
            {
                Init12(_cInfo);
                singleUseTimer.Stop();
                singleUseTimer.Close();
                singleUseTimer.Dispose();
            };
        }

        public static void StartingItemsDelayTimer(ClientInfo _cInfo, List<string> _items)
        {
            System.Timers.Timer singleUseTimer = new System.Timers.Timer(1000)
            {
                AutoReset = false
            };
            singleUseTimer.Start();
            singleUseTimer.Elapsed += (sender, e) =>
            {
                Init13(_cInfo, _items);
                singleUseTimer.Stop();
                singleUseTimer.Close();
                singleUseTimer.Dispose();
            };
        }

        public static void ResetPlayerProfileDelayTimer(string _id)
        {
            System.Timers.Timer singleUseTimer = new System.Timers.Timer(2000)
            {
                AutoReset = false
            };
            singleUseTimer.Start();
            singleUseTimer.Elapsed += (sender, e) =>
            {
                Init14(_id);
                singleUseTimer.Stop();
                singleUseTimer.Close();
                singleUseTimer.Dispose();
            };
        }

        public static void MazeGenerationDelayTimer(List<BlockChangeInfo> blockList)
        {
            System.Timers.Timer singleUseTimer = new System.Timers.Timer(5000)
            {
                AutoReset = false
            };
            singleUseTimer.Start();
            singleUseTimer.Elapsed += (sender, e) =>
            {
                Init15(blockList);
                singleUseTimer.Stop();
                singleUseTimer.Close();
                singleUseTimer.Dispose();
            };
        }

        public static void SABGenerationDelayTimer(List<BlockChangeInfo> blockList)
        {
            System.Timers.Timer singleUseTimer = new System.Timers.Timer(5000)
            {
                AutoReset = false
            };
            singleUseTimer.Start();
            singleUseTimer.Elapsed += (sender, e) =>
            {
                Init16(blockList);
                singleUseTimer.Stop();
                singleUseTimer.Close();
                singleUseTimer.Dispose();
            };
        }

        public static void WebPanelAlertTimer()
        {
            System.Timers.Timer singleUseTimer = new System.Timers.Timer(2000)
            {
                AutoReset = false
            };
            singleUseTimer.Start();
            singleUseTimer.Elapsed += (sender, e) =>
            {
                Init17();
                singleUseTimer.Stop();
                singleUseTimer.Close();
                singleUseTimer.Dispose();
            };
        }

        public static void InsideBlockTimer(ClientInfo _cInfo, Vector3 _position)
        {
            System.Timers.Timer singleUseTimer = new System.Timers.Timer(5000)
            {
                AutoReset = false
            };
            singleUseTimer.Start();
            singleUseTimer.Elapsed += (sender, e) =>
            {
                Init18(_cInfo, _position);
                singleUseTimer.Stop();
                singleUseTimer.Close();
                singleUseTimer.Dispose();
            };
        }

        public static void HardcoreDeleteFiles(ClientInfo _cInfo)
        {
            System.Timers.Timer singleUseTimer = new System.Timers.Timer(5000)
            {
                AutoReset = false
            };
            singleUseTimer.Start();
            singleUseTimer.Elapsed += (sender, e) =>
            {
                Init19(_cInfo);
                singleUseTimer.Stop();
                singleUseTimer.Close();
                singleUseTimer.Dispose();
            };
        }

        public static void PersistentDataSave()
        {
            System.Timers.Timer saveDelay = new System.Timers.Timer(60000)
            {
                AutoReset = true
            };
            saveDelay.Start();
            saveDelay.Elapsed += (sender, e) =>
            {
                PersistentContainer.Instance.Save(false);
            };
        }

        private static void Exec()
        {
            GeneralOperations.CheckArea();
            if (twoSecondTick >= 2)
            {
                twoSecondTick = 0;
                if (GodMode.IsEnabled || PlayerChecks.SpectatorEnabled || FlyingDetector.IsEnabled || SpeedDetector.IsEnabled)
                {
                    PlayerChecks.TwoSecondExec();
                }
                if (WorldRadius.IsEnabled)
                {
                    WorldRadius.Exec();
                }
                if (Zones.IsEnabled)
                {
                    Zones.HostileCheck();
                }
            }
            if (fiveSecondTick >= 5)
            {
                fiveSecondTick = 0;
                if (PlayerStats.IsEnabled)
                {
                    PlayerStats.Exec();
                }
            }
            if (tenSecondTick >= 10)
            {
                tenSecondTick = 0;
                EventSchedule.Exec();
                if (EntityCleanup.IsEnabled)
                {
                    EntityCleanup.EntityCheck();
                }
            }
            if (twentySecondTick >= 20)
            {
                twentySecondTick = 0;
                Track.Exec();
                if (HighPingKicker.IsEnabled)
                {
                    HighPingKicker.Exec();
                }
                if (InvalidItems.IsEnabled)
                {
                    InvalidItems.CheckInv();
                }
                if (LevelUp.IsEnabled)
                {
                    LevelUp.Exec();
                }
            }
            if (oneMinTick >= 60)
            {
                oneMinTick = 0;
                if (Jail.IsEnabled && Jail.Jailed.Count > 0)
                {
                    Jail.Clear();
                }
                if (Mute.IsEnabled && Mute.Mutes.Count > 0)
                {
                    Mute.Clear();
                }
                if (BloodmoonWarrior.IsEnabled)
                {
                    BloodmoonWarrior.Exec();
                }
            }
            if (InvalidItems.IsEnabled)
            {
                if (fiveMinTick >= 300)
                {
                    fiveMinTick = 0;
                    InvalidItems.CheckStorage();
                }
            }
            if (BloodMoans.IsEnabled)
            {
                bloodMoans++;
                if (bloodMoans >= BloodMoans.Countdown)
                {
                    bloodMoans = 0;
                    if (GeneralOperations.IsBloodmoon())
                    {
                        BloodMoans.Exec();
                    }
                }
            }
            if (GeneralOperations.NewPlayerQue.Count > 0)
            {
                newPlayer++;
                if (newPlayer >= 5)
                {
                    newPlayer = 0;
                    ClientInfo cInfo = GeneralOperations.NewPlayerQue[0];
                    GeneralOperations.NewPlayerQue.RemoveAt(0);
                    API.NewPlayerExec(cInfo);
                }
            }
            if (RestartVote.IsEnabled && RestartVote.VoteOpen)
            {
                restartVote++;
                if (restartVote >= 60)
                {
                    restartVote = 0;
                    RestartVote.VoteOpen = false;
                    RestartVote.ProcessRestartVote();
                }
            }
            if (MuteVote.IsEnabled && MuteVote.VoteOpen)
            {
                muteVote++;
                if (muteVote >= 60)
                {
                    muteVote = 0;
                    MuteVote.VoteOpen = false;
                    MuteVote.ProcessMuteVote();
                }
            }
            if (KickVote.IsEnabled && KickVote.VoteOpen)
            {
                kickVote++;
                if (kickVote >= 60)
                {
                    kickVote = 0;
                    KickVote.VoteOpen = false;
                    KickVote.ProcessKickVote();
                }
            }
            if (Hordes.IsEnabled)
            {
                horde++;
                if (horde >= 1200)
                {
                    horde = 0;
                    Hordes.Exec();
                }
            }
            if (Event.Invited)
            {
                eventInvitation++;
                if (eventInvitation >= 900)
                {
                    eventInvitation = 0;
                    Event.Invited = false;
                    Event.CheckOpen();
                }
            }
            if (Jail.IsEnabled)
            {
                Jail.StatusCheck();
            }
            if (DiscordBot.IsEnabled && DiscordBot.Queue.Count > 0)
            {
                DiscordBot.WebHook();
            }
            if (Shutdown.ShuttingDown)
            {
                stopServerSeconds++;
                if (stopServerSeconds >= 60)
                {
                    stopServerSeconds = 0;
                    StopServerMinutes--;
                    if (StopServerMinutes > 1)
                    {
                        Shutdown.TimeRemaining(StopServerMinutes);
                    }
                    else if (StopServerMinutes == 1)
                    {
                        Shutdown.OneMinute();
                    }
                    else if (StopServerMinutes == 0)
                    {
                        Shutdown.ShuttingDown = false;
                        Shutdown.Close();
                    }
                }
                if (StopServerMinutes == 1)
                {
                    if (Shutdown.UI_Lock && stopServerSeconds == 15)
                    {
                        Shutdown.Lock();
                    }
                    else if (stopServerSeconds == 30)
                    {
                        Shutdown.Kick();
                    }
                }
            }
            if (Event.Open)
            {
                eventOpen++;
                if (eventOpen == eventTime / 2)
                {
                    Event.HalfTime();
                }
                if (eventOpen == eventTime - 300)
                {
                    Event.FiveMin();
                }
                if (eventOpen >= eventTime)
                {
                    eventOpen = 0;
                    Event.EndEvent();
                }
            }
        }

        private static void Init1(string _playerId, List<string> _commands, string _trigger)
        {
            CustomCommands.CustomCommandDelayed(_playerId, _commands, _trigger);
        }

        private static void Init2(string _playerId, int _amount, bool _allowed)
        {
            Wallet.AddCurrency(_playerId, _amount, _allowed);
        }

        private static void Init3(ClientInfo _cInfo)
        {
            StartingItems.Exec(_cInfo, null);
        }

        private static void Init4(ClientInfo _cInfo)
        {
            Hardcore.KickPlayer(_cInfo);
        }

        private static void Init5(int _id)
        {
            ExitCommand.ExitWithCommand(_id);
        }

        private static void Init6(ClientInfo _cInfo, string _ip)
        {
            ExitCommand.ExitWithoutCommand(_cInfo, _ip);
        }

        private static void Init7(string _playerId, List<string> _commands)
        {
            Zones.ZoneCommandDelayed(_playerId, _commands);
        }

        private static void Init8(string _playerId, List<string> _commands)
        {
            LevelUp.LevelCommandDelayed(_playerId, _commands);
        }

        private static void Init9()
        {
            LoadProcess.Chunk_Region_Reset();
        }

        private static void Init10(string _playerId, int _amount)
        {
            Wallet.RemoveCurrency(_playerId, _amount);
        }

        private static void Init11(string _ip)
        {
            //RIO.RemovePlayer(_ip);
        }

        private static void Init12(ClientInfo _cInfo)
        {
            SpeedDetector.TimerExpired(_cInfo);
        }

        private static void Init13(ClientInfo _cInfo, List<string> _items)
        {
            StartingItems.Exec(_cInfo, _items);
        }

        private static void Init14(string _id)
        {
            ResetPlayerConsole.DelayedProfileDeletion(_id);
        }

        private static void Init15(List<BlockChangeInfo> blockList)
        {
            MazeConsole.Corrections(blockList);
        }

        private static void Init16(List<BlockChangeInfo> blockList)
        {
            SpawnActiveBlocksConsole.Corrections(blockList);
        }

        private static void Init17()
        {
            WebPanel.Alert = true;
            if (WebAPI.Panel_Address != "")
            {
                Log.Out(string.Format("[SERVERTOOLS] ServerTools web panel link @ '{0}'", WebAPI.Panel_Address));
            }
        }

        private static void Init18(ClientInfo _cInfo, Vector3 _position)
        {
            Teleportation.StillInsideBlock(_cInfo, _position);
        }

        private static void Init19(ClientInfo _cInfo)
        {
            Hardcore.ResetHardcoreProfile(_cInfo);
        }
    }
}
