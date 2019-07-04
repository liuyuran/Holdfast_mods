using Harmony12;
using HoldfastGame;
using System.Collections;
using uLink;

namespace ServerModFramework
{
    public static partial class Framework
    {
        public static PlayerJoin playerJoinDelegate = delegate (ulong steamId) { };
        public static PlayerLeave playerLeaveDelegate = delegate (ulong steamId) { };

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
                playerJoinDelegate(roundPlayerInformation.SteamID);
            }
        }

        [HarmonyPatch(typeof(ServerRoundPlayerManager), "RemovePlayer")]
        private static class PlayerRemove_Patch
        {
            static void Postfix(NetworkPlayer networkPlayer)
            {
                playerLeaveDelegate((ulong)netIdToSteamId[networkPlayer.id]);
                netIdToSteamId.Remove(networkPlayer.id);
            }
        }
    }
}
