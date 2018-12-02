using Harmony12;
using HoldfastGame;
using System.Collections;
using System.Reflection;
using uLink;
using UnityModManagerNet;

namespace LadiesAndGentleman
{
    public class Main
    {
        public static bool enabled;
        public static UnityModManager.ModEntry.ModLogger logger;
        public static ServerComponentReferenceManager serverInstance = null;
        public static ServerAdminBroadcastMessageManager broadcastInstance = null;
        public static ServerGameManager gameManager = null;
        public static Hashtable link = new Hashtable();
        public static Hashtable linkR = new Hashtable();
        public static Hashtable translateReplace = new Hashtable();

        public static bool Load(UnityModManager.ModEntry modEntry)
        {
            logger = modEntry.Logger;
            var harmony = HarmonyInstance.Create("chpr");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
            logger.Log("先生们女士们加载完成");
            return true;
        }

        [HarmonyPatch(typeof(ServerPlayerActionsLogFileHandler), "AddPlayerJoinedEntry")]
        public static class Join_Patch
        {
            static void Postfix(int playerID)
            {
                if (serverInstance == null) serverInstance = ServerComponentReferenceManager.ServerInstance;
                if (broadcastInstance == null) broadcastInstance = serverInstance.serverAdminBroadcastMessageManager;
                if (gameManager == null) gameManager = serverInstance.serverGameManager;
                ulong steamid;
                string arg;
                string arg2;
                gameManager.GetPlayerDetails(playerID, out steamid, out arg, out arg2);
                if(!link.ContainsKey(steamid)) link.Add(steamid, playerID);
            }
        }

        [HarmonyPatch(typeof(ServerAdminBroadcastMessageManager), "PrivateMessage")]
        public static class Message_Patch
        {
            static void Prefix(ref string message)
            {
                foreach (string key in translateReplace.Keys)
                {
                    message = message.Replace(key, (string)translateReplace[key]);
                }
            }
        }

        [HarmonyPatch(typeof(ServerRoundPlayerManager), "AddInstantiatedPlayer")]
        public static class Send_Patch
        {
            static void Postfix(RoundPlayerInformation roundPlayerInformation)
            {
                linkR.Add(roundPlayerInformation.NetworkPlayer.id, roundPlayerInformation.SteamID);
                broadcastInstance.PrivateMessage(-1, (int)link[roundPlayerInformation.SteamID],
                    "您好，欢迎加入我的实验服务器，本服务器致力于挖掘官方服务端" +
                    "中潜藏的功能并加以改进，在此过程中难免有不稳定的现象，我会" +
                    "尽量加以改进，如果对您的游玩造成了干扰，我很抱歉，但这无法" +
                    "避免");
            }
        }

        [HarmonyPatch(typeof(ServerRoundPlayerManager), "RemovePlayer")]
        public static class Remove_Patch
        {
            static void Postfix(NetworkPlayer networkPlayer)
            {
                link.Remove((string)linkR[networkPlayer]);
                linkR.Remove(networkPlayer.id);
            }
        }
    }
}
