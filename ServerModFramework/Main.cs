using Harmony12;
using HoldfastGame;
using System.Collections;
using System.Reflection;
using System.Timers;
using uLink;
using UnityModManagerNet;

namespace ServerModFramework
{
    public delegate void CountDown();
    public delegate void PlayerJoin(ulong steamId);
    public delegate void PlayerLeave(ulong steamId);

    public class Framework
    {
        public static CountDown countDownDelegate = delegate () { };
        public static PlayerJoin playerJoinDelegate = delegate (ulong steamId) { };
        public static PlayerLeave playerLeaveDelegate = delegate (ulong steamId) { };

        private static UnityModManager.ModEntry.ModLogger logger;
        private static Hashtable steamIdToLocalId = new Hashtable();
        private static Hashtable netIdToSteamId = new Hashtable();
        public static bool Load(UnityModManager.ModEntry modEntry)
        {
            logger = modEntry.Logger;
            startTimer();
            var harmony = HarmonyInstance.Create(modEntry.Info.Id);
            harmony.PatchAll(Assembly.GetExecutingAssembly());
            logger.Log("服务器基础框架加载完成");
            return true;
        }

        private static void startTimer()
        {
            System.Timers.Timer timer = new System.Timers.Timer();
            timer.Enabled = true;
            timer.Interval = 1000; 
            timer.Start();
            timer.Elapsed += new System.Timers.ElapsedEventHandler(delegate (object source, ElapsedEventArgs e)
            {
                countDownDelegate();
            });
        }

        [HarmonyPatch(typeof(ServerPlayerActionsLogFileHandler), "AddPlayerJoinedEntry")]
        private static class Join_Patch
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
        private static class Remove_Patch
        {
            static void Postfix(NetworkPlayer networkPlayer)
            {
                playerLeaveDelegate((ulong) netIdToSteamId[networkPlayer.id]);
                netIdToSteamId.Remove(networkPlayer.id);
            }
        }

        public static void sendMessage(int steamId, string message)
        {
            ServerComponentReferenceManager.ServerInstance.serverAdminBroadcastMessageManager.PrivateMessage(-1, steamId, message);
        }

        public static int getRoundTime()
        {
            return (int) ServerComponentReferenceManager.ServerInstance.serverRoundTimerManager.secondsToRoundEnd;
        }
    }
}
