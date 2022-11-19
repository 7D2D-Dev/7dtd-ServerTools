﻿using HarmonyLib;
using ServerTools;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;

public static class Injections
{

    public static bool PlayerLoginRPC_Prefix(ClientInfo _cInfo, string _playerName, ValueTuple<PlatformUserIdentifierAbs, string> _platformUserAndToken, ValueTuple<PlatformUserIdentifierAbs, string> _crossplatformUserAndToken, out bool __state)
    {
        __state = false;
        try
        {
            if (GameManager.Instance != null && GameManager.Instance.World != null)
            {
                if (_platformUserAndToken.Item1 != null && _crossplatformUserAndToken.Item1 != null)
                {
                    if (ReservedSlots.IsEnabled)
                    {
                        int maxPlayers = GamePrefs.GetInt(EnumGamePrefs.ServerMaxPlayerCount);
                        if (ConnectionManager.Instance.ClientCount() > maxPlayers && ReservedSlots.FullServer(_cInfo, _platformUserAndToken.Item1, _crossplatformUserAndToken.Item1))
                        {
                            GamePrefs.Set(EnumGamePrefs.ServerMaxPlayerCount, maxPlayers + 1);
                            __state = true;
                        }
                    }
                }
            }
            else
            {
                return false;
            }
        }
        catch (Exception e)
        {
            Log.Out(string.Format("[SERVERTOOLS] Error in Injections.PlayerLoginRPC_Prefix: {0}", e.Message));
        }
        return true;
    }

    public static void PlayerLoginRPC_Postfix(bool __state)
    {
        try
        {
            if (__state)
            {
                int maxPlayers = GamePrefs.GetInt(EnumGamePrefs.ServerMaxPlayerCount);
                GamePrefs.Set(EnumGamePrefs.ServerMaxPlayerCount, maxPlayers - 1);
            }
        }
        catch (Exception e)
        {
            Log.Out(string.Format("[SERVERTOOLS] Error in Injections.PlayerLoginRPC_Postfix: {0}", e.Message));
        }
    }

    public static bool GameManager_ChangeBlocks_Prefix(GameManager __instance, PlatformUserIdentifierAbs persistentPlayerId, List<BlockChangeInfo> _blocksToChange)
    {
        try
        {
            return BlockChange.ProcessBlockChange(__instance, persistentPlayerId, _blocksToChange);
        }
        catch (Exception e)
        {
            Log.Out(string.Format("[SERVERTOOLS] Error in Injections.GameManager_ChangeBlocks_Prefix: {0}", e.Message));
        }
        return true;
    }

    public static void ServerConsoleCommand_Postfix(ClientInfo _cInfo, string _cmd)
    {
        try
        {
            ConsoleCommandLog.Exec(_cInfo, _cmd);
        }
        catch (Exception e)
        {
            Log.Out(string.Format("[SERVERTOOLS] Error in Injections.ServerConsoleCommand_Postfix: {0}", e.Message));
        }
    }

    public static void AddFallingBlock_Prefix(World __instance, Vector3i _blockPos)
    {
        try
        {
            if (FallingBlocks.IsEnabled && _blockPos != null)
            {
                BlockValue blockValue = __instance.GetBlock(_blockPos);
                if (blockValue.ischild || blockValue.Block.StabilityIgnore || blockValue.isair)
                {
                    return;
                }
                else
                {
                    GameManager.Instance.World.SetBlockRPC(_blockPos, BlockValue.Air);
                }
            }
        }
        catch (Exception e)
        {
            Log.Out(string.Format("[SERVERTOOLS] Error in Injections.AddFallingBlock_Prefix: {0}", e.Message));
        }
    }

    public static void AddFallingBlocks_Prefix(World __instance, IList<Vector3i> _list)
    {
        try
        {
            if (FallingBlocks.IsEnabled && _list != null)
            {
                for (int i = 0; i < _list.Count; i++)
                {
                    BlockValue blockValue = __instance.GetBlock(_list[i]);
                    if (blockValue.ischild || blockValue.Block.StabilityIgnore || blockValue.isair)
                    {
                        return;
                    }
                    else
                    {
                        GameManager.Instance.World.SetBlockRPC(_list[i], BlockValue.Air);
                    }
                }
                if (_list.Count > FallingBlocks.Max_Blocks && FallingBlocks.OutputLog)
                {
                    EntityPlayer closestPlayer = __instance.GetClosestPlayer(_list[0].x, _list[0].y, _list[0].z, -1, 75);
                    if (closestPlayer != null)
                    {
                        Log.Out(string.Format("[SERVERTOOLS] Removed '{0}' falling blocks around '{1}'. The closest player entity id was '{2}' named '{3}' @ '{4}'", _list.Count, _list[0], closestPlayer.entityId, closestPlayer.EntityName, closestPlayer.position));
                    }
                    else
                    {
                        closestPlayer = __instance.GetClosestPlayer(_list[_list.Count - 1].x, _list[_list.Count - 1].y, _list[_list.Count - 1].z, -1, 75);
                        if (closestPlayer != null)
                        {

                            Log.Out(string.Format("[SERVERTOOLS] Removed '{0}' falling blocks around '{1}'. The closest player entity id was '{2}' named '{3}' @ '{4}'", _list.Count, _list[_list.Count - 1], closestPlayer.entityId, closestPlayer.EntityName, closestPlayer.position));
                        }
                        else
                        {
                            Log.Out(string.Format("[SERVERTOOLS] Removed '{0}' falling blocks around '{1}'. No players were located near by", _list.Count, _list[0]));
                        }
                    }
                }
            }
        }
        catch (Exception e)
        {
            Log.Out(string.Format("[SERVERTOOLS] Error in Injections.AddFallingBlocks_Prefix: {0}", e.Message));
        }
    }

    public static void ChatMessageServer_Postfix(ClientInfo _cInfo, EChatType _chatType, string _msg, string _mainName)
    {
        try
        {
            if (DiscordBot.IsEnabled && DiscordBot.Webhook != "" && DiscordBot.Webhook.StartsWith("https://discord.com/api/webhooks") &&
                _chatType == EChatType.Global && !string.IsNullOrWhiteSpace(_mainName) && !_mainName.Contains(DiscordBot.Prefix))
            {
                if (_msg.Contains("[") && _msg.Contains("]"))
                {
                    _msg = Regex.Replace(_msg, @"\[.*?\]", "");
                }
                if (!GeneralFunction.InvalidPrefix.Contains(_msg[0]))
                {
                    if (_mainName.Contains("[") && _mainName.Contains("]"))
                    {
                        _mainName = Regex.Replace(_mainName, @"\[.*?\]", "");
                    }
                    if (_cInfo != null)
                    {
                        if (DiscordBot.LastEntry != _msg)
                        {
                            DiscordBot.LastPlayer = _cInfo.PlatformId.ToString();
                            DiscordBot.LastEntry = _msg;
                            DiscordBot.Queue.Add("[Game] **" + _mainName + "** : " + DiscordBot.LastEntry);
                        }
                        else if (DiscordBot.LastPlayer != _cInfo.PlatformId.ToString())
                        {
                            DiscordBot.LastPlayer = _cInfo.PlatformId.ToString();
                            DiscordBot.LastEntry = _msg;
                            DiscordBot.Queue.Add("[Game] **" + _mainName + "** : " + DiscordBot.LastEntry);
                        }
                    }
                    else if (DiscordBot.LastEntry != _msg)
                    {
                        DiscordBot.LastPlayer = "-1";
                        DiscordBot.LastEntry = _msg;
                        DiscordBot.Queue.Add("[Game] **" + _mainName + "** : " + DiscordBot.LastEntry);
                    }
                }
            }
        }
        catch (Exception e)
        {
            Log.Out(string.Format("[SERVERTOOLS] Error in Injections.ChatMessageServer_Postfix: {0}", e.Message));
        }
    }

    public static void GameManager_Cleanup_Finalizer()
    {
        try
        {
            Log.Out("[SERVERTOOLS] SHUTDOWN");
            Process process = Process.GetCurrentProcess();
            if (process != null)
            {
                process.Kill();
            }
        }
        catch (Exception e)
        {
            Log.Out(string.Format("[SERVERTOOLS] Error in Injections.GameManager_Cleanup_Finalizer: {0}", e.Message));
        }
    }

    public static bool GameManager_CollectEntityServer_Prefix(int _entityId)
    {
        try
        {
            if (GeneralFunction.No_Vehicle_Pickup)
            {
                Entity entity = GeneralFunction.GetEntity(_entityId);
                if (entity != null && entity is EntityVehicle)
                {
                    if (GeneralFunction.Allow_Bicycle && entity is EntityBicycle)
                    {
                        return true;
                    }
                    return false;
                }
            }
        }
        catch (Exception e)
        {
            Log.Out(string.Format("[SERVERTOOLS] Error in Injections.GameManager_CollectEntityServer_Prefix: {0}", e.Message));
        }
        return true;
    }

    public static bool GameManager_ExplosionServer_Prefix(int _playerId)
    {
        try
        {
            Entity entity = GeneralFunction.GetEntity(_playerId);
            if ((entity == null || !entity.IsSpawned()) && _playerId != -1)
            {
                return false;
            }
        }
        catch (Exception e)
        {
            Log.Out(string.Format("[SERVERTOOLS] Error in Injections.GameManager_ExplosionServer_Prefix: {0}", e.Message));
        }
        return true;
    }

    public static bool GameManager_OpenTileEntityAllowed_Prefix(ref bool __result, ref bool __state, int _entityIdThatOpenedIt, ref TileEntity _te)
    {
        try
        {
            ClientInfo cInfo = GeneralFunction.GetClientInfoFromEntityId(_entityIdThatOpenedIt);
            if (cInfo != null)
            {
                if (GameManager.Instance.adminTools.GetUserPermissionLevel(cInfo.PlatformId) > 0 &&
                    GameManager.Instance.adminTools.GetUserPermissionLevel(cInfo.CrossplatformId) > 0)
                {
                    if (DroppedBagProtection.IsEnabled && _te is TileEntityLootContainer)
                    {
                        TileEntityLootContainer lootContainer = _te as TileEntityLootContainer;
                        if (lootContainer.bPlayerBackpack)
                        {
                            if (!DroppedBagProtection.IsAllowed(_entityIdThatOpenedIt, lootContainer))
                            {
                                Phrases.Dict.TryGetValue("DroppedBagProtection1", out string phrase);
                                ChatHook.ChatMessage(cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                                __result = false;
                                return false;
                            }
                        }
                    }
                    if (Shutdown.UI_Locked)
                    {
                        if (_te is TileEntityLootContainer)
                        {
                            TileEntityLootContainer lootContainer = _te as TileEntityLootContainer;
                            if (lootContainer.bPlayerBackpack)
                            {
                                return true;
                            }
                        }
                        if (_te is TileEntityWorkstation || _te is TileEntityLootContainer || _te is TileEntitySecureLootContainer
                        || _te is TileEntityVendingMachine || _te is TileEntityTrader)
                        {
                            if (_te is TileEntityTrader)
                            {
                                __state = true;
                            }
                            Phrases.Dict.TryGetValue("Shutdown3", out string phrase);
                            ChatHook.ChatMessage(cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                            __result = false;
                            return false;
                        }
                    }
                    if (WorkstationLock.IsEnabled && _te is TileEntityWorkstation)
                    {
                        EntityPlayer entityPlayer = GeneralFunction.GetEntityPlayer(cInfo.entityId);
                        if (entityPlayer != null)
                        {
                            EnumLandClaimOwner owner = GeneralFunction.ClaimedByWho(cInfo.CrossplatformId, new Vector3i(entityPlayer.position));
                            if (owner != EnumLandClaimOwner.Self && owner != EnumLandClaimOwner.Ally && !GeneralFunction.ClaimedByNone(new Vector3i(entityPlayer.position)))
                            {
                                Phrases.Dict.TryGetValue("WorkstationLock1", out string phrase);
                                ChatHook.ChatMessage(cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                                __result = false;
                                return false;
                            }
                        }
                    }
                }
            }
        }
        catch (Exception e)
        {
            Log.Out(string.Format("[SERVERTOOLS] Error in Injections.GameManager_OpenTileEntityAllowed_Prefix: {0}", e.Message));
        }
        return true;
    }

    public static void GameManager_OpenTileEntityAllowed_Postfix(bool __state, int _entityIdThatOpenedIt, TileEntity _te)
    {
        try
        {
            if (__state)
            {
                ClientInfo cInfo = GeneralFunction.GetClientInfoFromEntityId(_entityIdThatOpenedIt);
                if (cInfo != null)
                {
                    cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageConsoleCmdClient>().Setup("xui close trader", true));
                }
            }
        }
        catch (Exception e)
        {
            Log.Out(string.Format("[SERVERTOOLS] Error in Injections.GameManager_OpenTileEntityAllowed_Postfix: {0}", e.Message));
        }
    }

    public static bool EntityAlive_OnEntityDeath_Prefix(EntityAlive __instance)
    {
        try
        {
            if (__instance is EntityZombie)
            {
                if (ReservedSlots.IsEnabled && ReservedSlots.Bonus_Exp > 0)
                {
                    EntityAlive entityAlive = __instance.GetAttackTarget();
                    if (entityAlive != null && entityAlive is EntityPlayer)
                    {
                        ClientInfo cInfo = GeneralFunction.GetClientInfoFromEntityId(entityAlive.entityId);
                        if (cInfo != null)
                        {
                            if (ReservedSlots.Dict.ContainsKey(cInfo.PlatformId.CombinedString) || ReservedSlots.Dict.ContainsKey(cInfo.CrossplatformId.CombinedString))
                            {
                                if (ReservedSlots.Bonus_Exp > 100)
                                {
                                    ReservedSlots.Bonus_Exp = 100;
                                }
                                int experience = EntityClass.list[__instance.entityClass].ExperienceValue;
                                float percent = ReservedSlots.Bonus_Exp / 100f;
                                float bonus = experience * percent;
                                experience = (int)bonus / 2;
                                cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageEntityAddExpClient>().Setup(entityAlive.entityId, experience, Progression.XPTypes.Kill));
                            }
                            int experienceBoost = PersistentContainer.Instance.Players[cInfo.CrossplatformId.CombinedString].ExperienceBoost;
                            if (experienceBoost > 0)
                            {
                                cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageEntityAddExpClient>().Setup(entityAlive.entityId, experienceBoost, Progression.XPTypes.Kill));
                            }
                        }
                    }
                }
            }
            else if (__instance is EntityPlayer)
            {
                if (Hardcore.IsEnabled)
                {
                    ClientInfo cInfo = GeneralFunction.GetClientInfoFromEntityId(__instance.entityId);
                    if (cInfo != null)
                    {
                        if (Hardcore.Optional)
                        {
                            if (PersistentContainer.Instance.Players[cInfo.CrossplatformId.CombinedString].HardcoreEnabled)
                            {
                                Hardcore.Check(cInfo, __instance as EntityPlayer, true);
                            }
                        }
                        else
                        {
                            Hardcore.Check(cInfo, __instance as EntityPlayer, true);
                        }
                    }
                }
            }
        }
        catch (Exception e)
        {
            Log.Out(string.Format("[SERVERTOOLS] Error in Injections.EntityAlive_OnEntityDeath_Prefix: {0}", e.Message));
        }
        return true;
    }

    public static void World_SpawnEntityInWorld_Postfix(Entity _entity)
    {
        try
        {
            if (DroppedBagProtection.IsEnabled && _entity != null && _entity is EntityBackpack)
            {
                List<ClientInfo> clientList = GeneralFunction.ClientList();
                if (clientList != null)
                {
                    for (int i = 0; i < clientList.Count; i++)
                    {
                        ClientInfo cInfo = clientList[i];
                        if (cInfo.latestPlayerData != null && cInfo.latestPlayerData.droppedBackpackPosition != null && cInfo.latestPlayerData.droppedBackpackPosition == new Vector3i(_entity.position))
                        {
                            if (!DroppedBagProtection.Backpacks.ContainsKey(_entity.entityId))
                            {
                                DroppedBagProtection.Backpacks.Add(_entity.entityId, cInfo.entityId);
                                PersistentContainer.Instance.Backpacks.Add(_entity.entityId, cInfo.entityId);
                            }
                            else
                            {
                                DroppedBagProtection.Backpacks[_entity.entityId] = cInfo.entityId;
                                PersistentContainer.Instance.Backpacks[_entity.entityId] = cInfo.entityId;
                            }
                            PersistentContainer.DataChange = true;
                            return;
                        }
                    }
                }
            }
        }
        catch (Exception e)
        {
            Log.Out(string.Format("[SERVERTOOLS] Error in Injections.World_SpawnEntityInWorld_Postfix: {0}", e.Message));
        }
    }

    public static void GameManager_PlayerSpawnedInWorld_Postfix(ClientInfo _cInfo, RespawnType _respawnReason, Vector3i _pos, int _entityId)
    {
        try
        {
            if (SpeedDetector.Flags.ContainsKey(_cInfo.entityId))
            {
                SpeedDetector.Flags.Remove(_cInfo.entityId);
            }
        }
        catch (Exception e)
        {
            Log.Out(string.Format("[SERVERTOOLS] Error in Injections.GameManager_PlayerSpawnedInWorld_Postfix: {0}", e.Message));
        }
    }

    public static void EntityAlive_SetDead_Postfix(EntityAlive __instance)
    {
        try
        {
            if (__instance is EntityPlayer)
            {
                API.PlayerDied(__instance);
            }
        }
        catch (Exception e)
        {
            Log.Out(string.Format("[SERVERTOOLS] Error in Injections.EntityAlive_SetDead_Postfix: {0}", e.Message));
        }
    }

    public static void NetPackagePlayerInventory_ProcessPackage_Prefix(NetPackagePlayerInventory __instance)
    {
        try
        {
            if (__instance.Sender != null)
            {
                InfiniteAmmo.Process(__instance);
            }
        }
        catch (Exception e)
        {
            Log.Out(string.Format("[SERVERTOOLS] Error in Injections.NetPackagePlayerInventory_ProcessPackage_Prefix: {0}", e.Message));
        }
    }

    public static void NetPackagePlayerInventory_ProcessPackage_Postfix(NetPackagePlayerInventory __instance)
    {
        try
        {
            if (__instance.Sender != null && Wallet.UpdateRequired.ContainsKey(__instance.Sender.entityId))
            {
                ClientInfo cInfo = __instance.Sender;
                Wallet.UpdateRequired.TryGetValue(cInfo.entityId, out int value);
                Wallet.UpdateRequired.Remove(cInfo.entityId);
                Wallet.AddCurrency(cInfo.CrossplatformId.CombinedString, value, false);
            }
        }
        catch (Exception e)
        {
            Log.Out(string.Format("[SERVERTOOLS] Error in Injections.NetPackagePlayerInventory_ProcessPackage_Postfix: {0}", e.Message));
        }
    }

    public static bool ClientInfoCollection_GetForNameOrId_Prefix(ref ClientInfo __result, string _nameOrId)
    {
        try
        {
            ClientInfo clientInfo = null;
            int entityId;
            if (int.TryParse(_nameOrId, out entityId))
            {
                clientInfo = GeneralFunction.GetClientInfoFromEntityId(entityId);
                if (clientInfo != null)
                {
                    __result = clientInfo;
                    return false;
                }
            }
            clientInfo = GeneralFunction.GetClientInfoFromName(_nameOrId);
            if (clientInfo != null)
            {
                __result = clientInfo;
                return false;
            }
            if (_nameOrId.Contains("Local_") || _nameOrId.Contains("EOS_") || _nameOrId.Contains("Steam_") || _nameOrId.Contains("XBL_") || _nameOrId.Contains("PSN_") || _nameOrId.Contains("EGS_"))
            {
                PlatformUserIdentifierAbs userIdentifier;
                if (PlatformUserIdentifierAbs.TryFromCombinedString(_nameOrId, out userIdentifier))
                {
                    clientInfo = GeneralFunction.GetClientInfoFromUId(userIdentifier);
                    if (clientInfo != null)
                    {
                        __result = clientInfo;
                        return false;
                    }
                }
            }
        }
        catch (Exception e)
        {
            Log.Out(string.Format("[SERVERTOOLS] Error in Injections.ClientInfoCollection_GetForNameOrId_Prefix: {0}", e.Message));
        }
        return false;
    }

    public static bool NetPackagePlayerStats_ProcessPackage_Prefix(NetPackagePlayerStats __instance)
    {
        try
        {
            if (__instance.Sender != null && GeneralFunction.Net_Package_Detector && !PlayerStatsPackage.IsValid(__instance))
            {
                return false;
            }
        }
        catch (Exception e)
        {
            Log.Out(string.Format("[SERVERTOOLS] Error in Injections.NetPackagePlayerStats_ProcessPackage_Prefix: {0}", e.Message));
        }
        return true;
    }

    public static bool NetPackageEntityAddScoreServer_ProcessPackage_Prefix(NetPackageEntityAddScoreServer __instance)
    {
        try
        {
            if (__instance.Sender != null)
            {
                if (GeneralFunction.Net_Package_Detector && !EntityAddScoreServerPackage.IsValid(__instance))
                {
                    return false;
                }
                if (MagicBullet.IsEnabled && MagicBullet.Exec(__instance))
                {
                    return false;
                }
            }
        }
        catch (Exception e)
        {
            Log.Out(string.Format("[SERVERTOOLS] Error in Injections.NetPackageEntityAddScoreServer_ProcessPackage_Prefix: {0}", e.Message));
        }
        return true;
    }

    public static bool NetPackageChat_ProcessPackage_Prefix(NetPackageChat __instance)
    {
        try
        {
            if (__instance.Sender != null && GeneralFunction.Net_Package_Detector && !ChatPackage.IsValid(__instance))
            {
                return false;
            }
        }
        catch (Exception e)
        {
            Log.Out(string.Format("[SERVERTOOLS] Error in Injections.NetPackageChat_ProcessPackage_Prefix: {0}", e.Message));
        }
        return true;
    }

    public static bool NetPackageEntityPosAndRot_ProcessPackage_Prefix(NetPackageEntityPosAndRot __instance)
    {
        try
        {
            if (__instance.Sender != null && GeneralFunction.Net_Package_Detector && !EntityPosAndRotPackage.IsValid(__instance))
            {
                return false;
            }
        }
        catch (Exception e)
        {
            Log.Out(string.Format("[SERVERTOOLS] Error in Injections.NetPackageEntityPosAndRot_ProcessPackage_Prefix: {0}", e.Message));
        }
        return true;
    }

    public static bool NetPackagePlayerData_ProcessPackage_Prefix(NetPackagePlayerData __instance)
    {
        try
        {
            if (__instance.Sender != null && GeneralFunction.Net_Package_Detector && !PlayerDataPackage.IsValid(__instance))
            {
                return false;
            }
        }
        catch (Exception e)
        {
            Log.Out(string.Format("[SERVERTOOLS] Error in Injections.NetPackagePlayerData_ProcessPackage_Prefix: {0}", e.Message));
        }
        return true;
    }

    public static void NetPackageDamageEntity_ProcessPackage_Prefix(NetPackageDamageEntity __instance)
    {
        if (__instance.Sender != null)
        {
            Entity victim = GeneralFunction.GetEntity(ProcessDamage.EntityId(__instance));
            if (victim != null)
            {
                Entity attacker = GeneralFunction.GetEntity(ProcessDamage.AttackerEntityId(__instance));
                if (attacker != null)
                {
                    ProcessDamage.Exec(__instance, victim, attacker);
                }
            }
        }
    }

    public static void ClientInfo_SendPackage_Postfix(ClientInfo __instance, NetPackage _package)
    {
        if (__instance != null && _package != null && _package is NetPackageTeleportPlayer)
        {
            if (!TeleportDetector.Ommissions.Contains(__instance.entityId))
            {
                TeleportDetector.Ommissions.Add(__instance.entityId);
            }
        }
    }

    public static bool PersistentPlayerList_PlaceLandProtectionBlock_Prefix(PersistentPlayerList __instance, Vector3i pos, PersistentPlayerData owner)
    {
        try
        {
            if (LandClaimCount.IsEnabled && __instance != null && pos != null && owner != null)
            {
                PersistentPlayerData persistentPlayerData;
                if (__instance.m_lpBlockMap.TryGetValue(pos, out persistentPlayerData))
                {
                    persistentPlayerData.RemoveLandProtectionBlock(pos);
                }
                owner.AddLandProtectionBlock(pos);
                LandClaimCount.RemoveExtraLandClaims(owner);
                __instance.m_lpBlockMap[pos] = owner;
                return false;
            }
        }
        catch (Exception e)
        {
            Log.Out(string.Format("[SERVERTOOLS] Error in Injections.PersistentPlayerList_PlaceLandProtectionBlock_Prefix: {0}", e.Message));
        }
        return true;
    }

    public static bool NetPackageEntityAttach_ProcessPackage_Prefix(NetPackageEntityAttach __instance)
    {
        try
        {
            if (__instance.Sender == null)
            {
                return false;
            }
            else
            {
                EntityPlayer player = GeneralFunction.GetEntityPlayer(NoVehicleDrone.riderId(__instance));
                if (player != null)
                {
                    if (player.IsDead())
                    {
                        return false;
                    }
                    else if (NoVehicleDrone.IsEnabled && !NoVehicleDrone.Exec(__instance, player))
                    {
                        ClientInfo cInfo = GeneralFunction.GetClientInfoFromEntityId(player.entityId);
                        if (cInfo != null)
                        {
                            Phrases.Dict.TryGetValue("NoVehicleDrone1", out string phrase);
                            ChatHook.ChatMessage(cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                        }
                        return false;
                    }
                }
            }
        }
        catch (Exception e)
        {
            Log.Out(string.Format("[SERVERTOOLS] Error in Injections.NetPackageEntityAttach_ProcessPackage_Prefix: {0}", e.Message));
        }
        return true;
    }

    public static void LootManager_LootContainerOpened_Prefix(ref TileEntityLootContainer _tileEntity, int _entityIdThatOpenedIt)
    {
        try
        {
            if (_tileEntity != null && Vault.IsEnabled && _tileEntity.blockValue.Block.GetBlockName() == "VaultBox")
            {
                _tileEntity = Vault.Exec(_entityIdThatOpenedIt, _tileEntity);
            }
        }
        catch (Exception e)
        {
            Log.Out(string.Format("[SERVERTOOLS] Error in Injections.LootManager_LootContainerOpened_Prefix: {0}", e.Message));
        }
    }

    public static void NetPackageTileEntity_Setup_Postfix(TileEntity _te)
    {
        try
        {
            if (_te != null && _te is TileEntityLootContainer && Vault.IsEnabled && _te.blockValue.Block.GetBlockName() == "VaultBox")
            {
                Vault.UpdateData(_te as TileEntityLootContainer);
            }
        }
        catch (Exception e)
        {
            Log.Out(string.Format("[SERVERTOOLS] Error in Injections.NetPackageTileEntity_Setup_Postfix: {0}", e.Message));
        }
    }

    public static bool GameManager_DropContentOfLootContainerServer_Prefix(BlockValue _bvOld)
    {
        try
        {
            if (_bvOld.Block.GetBlockName() == "VaultBox")
            {
                return false;
            }
        }
        catch (Exception e)
        {
            Log.Out(string.Format("[SERVERTOOLS] Error in Injections.GameManager_DropContentOfLootContainerServer_Prefix: {0}", e.Message));
        }
        return true;
    }
}