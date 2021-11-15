﻿using System;
using System.Collections.Generic;

namespace ServerTools
{
    class RemoveItemDukesConsole : ConsoleCmdAbstract
    {
        public override string GetDescription()
        {
            return "[ServerTools] - Removes an online player's items marked with the tag dukes";
        }

        public override string GetHelp()
        {
            return "Removes all items from a online player that has the tag dukes in Items.xml\n" +
                "Usage: st-rid <steamId/entityId/playerName>\n" ;
        }

        public override string[] GetCommands()
        {
            return new string[] { "st-RemoveItemDukes", "rid", "st-rid" };
        }

        public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
        {
            try
            {
                if (_params.Count != 1)
                {
                    SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Wrong number of arguments, expected 1, found {0}", _params.Count));
                    return;
                }
                ClientInfo cInfo = ConsoleHelper.ParseParamIdOrName(_params[0]);
                if (cInfo != null)
                {
                    EntityPlayer player = PersistentOperations.GetEntityPlayer(cInfo.playerId);
                    if (player != null)
                    {
                        if (GameEventManager.GameEventSequences.ContainsKey("action_dukes"))
                        {
                            GameEventManager.Current.HandleAction("action_dukes", null, player, false, "");
                            cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageGameEventResponse>().Setup("action_dukes", cInfo.playerName, "", NetPackageGameEventResponse.ResponseTypes.Approved));
                            SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Removed all items tagged dukes from inventory and backpack of player {0}", cInfo.playerId));
                            return;
                        }
                        else
                        {
                            SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Unable to locate action_dukes in the game events list"));
                            return;
                        }
                    }
                }
                else
                {
                    SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Unable to locate player {0} online", _params[0]));
                    return;
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in RemoveItemDukesConsole.Execute: {0}", e.Message));
            }
        }
    }
}
