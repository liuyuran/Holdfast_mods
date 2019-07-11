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
        /// 玩家进入监听器
        public static event PlayerJoin playerJoinDelegate;
        /// 玩家离开监听器
        public static event PlayerLeave playerLeaveDelegate;

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
                if(netIdToSteamId.ContainsKey(roundPlayerInformation.NetworkPlayer.id))
                    netIdToSteamId.Add(roundPlayerInformation.NetworkPlayer.id, roundPlayerInformation.SteamID);
                playerJoinDelegate(roundPlayerInformation.SteamID);
            }
        }

        [HarmonyPatch(typeof(ServerRoundPlayerManager), "RemovePlayer")]
        private static class PlayerRemove_Patch
        {
            static bool Prefix(NetworkPlayer networkPlayer)
            {
                if (networkPlayer == null) return true;
                netIdToSteamId.Remove(networkPlayer.id);
                if (netIdToSteamId[networkPlayer.id] == null || playerLeaveDelegate == null) return true;
                playerLeaveDelegate((ulong)netIdToSteamId[networkPlayer.id]);
                return true;
            }
        }
    }
}
