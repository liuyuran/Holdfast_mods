/**
 * @file UserManage.cs
 * @brief 玩家模块文件
 * @details 这个文件包含了玩家进场出场以及ID映射的相关逻辑
 * @author 夏洛特
 * @version 0.1b
 * @date 2019-07-04
 */

 using Harmony12;
using HoldfastGame;
using System.Collections;
using uLink;

namespace ServerModFramework
{
    public delegate void PlayerJoin(ulong steamId);
    public delegate void PlayerLeave(ulong steamId);
    public static partial class Framework
    {
        public static PlayerJoin playerJoinDelegate = null;
        public static PlayerLeave playerLeaveDelegate = null;

        private static Hashtable steamIdToLocalId = new Hashtable();
        private static Hashtable netIdToSteamId = new Hashtable();

        [HarmonyPatch(typeof(ServerPlayerActionsLogFileHandler), "AddPlayerJoinedEntry")]
        private static class PlayerJoin_Patch
        {
            static void Postfix(int playerID)
            {
                ulong steamid;
                string tmp;
                ServerComponentReferenceManager.ServerInstance.serverGameManager.GetPlayerDetails(playerID, out steamid, out tmp, out tmp);
                if (!steamIdToLocalId.ContainsKey(steamid)) steamIdToLocalId.Add(steamid, playerID);
            }
        }

        [HarmonyPatch(typeof(ServerRoundPlayerManager), "AddInstantiatedPlayer")]
        private static class Send_Patch
        {
            static void Postfix(RoundPlayerInformation roundPlayerInformation)
            {
                netIdToSteamId.Add(roundPlayerInformation.NetworkPlayer.id, roundPlayerInformation.SteamID);
                playerJoinDelegate?.Invoke(roundPlayerInformation.SteamID);
            }
        }

        [HarmonyPatch(typeof(ServerRoundPlayerManager), "RemovePlayer")]
        private static class PlayerRemove_Patch
        {
            static void Postfix(NetworkPlayer networkPlayer)
            {
                playerLeaveDelegate?.Invoke((ulong)netIdToSteamId[networkPlayer.id]);
                netIdToSteamId.Remove(networkPlayer.id);
            }
        }
    }
}
