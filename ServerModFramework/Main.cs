using Harmony12;
using HoldfastGame;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Timers;
using uLink;
using UnityModManagerNet;

namespace ServerModFramework
{
    public delegate void CountDown(int roundTime);
    public delegate void PlayerJoin(ulong steamId);
    public delegate void PlayerLeave(ulong steamId);
    public delegate string AdminMessage(string command);
    public delegate IConsoleCommand AddCommandList();

    public class Framework
    {
        public static CountDown countDownDelegate = delegate (int roundTime) { };
        public static PlayerJoin playerJoinDelegate = delegate (ulong steamId) { };
        public static PlayerLeave playerLeaveDelegate = delegate (ulong steamId) { };
        public static AdminMessage adminMessageDelegate = delegate (string message) { return ""; };
        public static AddCommandList addCommandListDelegate = delegate () { return null; };

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
                countDownDelegate(getRoundTime());
            });
        }

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
                playerLeaveDelegate((ulong) netIdToSteamId[networkPlayer.id]);
                netIdToSteamId.Remove(networkPlayer.id);
            }
        }

        [HarmonyPatch(typeof(ServerChatHandler), "HandleAsAdminCommand")]
        private static class ChatAdminCommand_Patch
        {
            static void Postfix(string entryText, RoundPlayer player, out bool __result)
            {
                string text;
                bool flag2;
                Exception ex;
                string input = null;
                foreach (AdminMessage processor in adminMessageDelegate.GetInvocationList())
                {
                    input = processor(entryText);
                    if (input != null)
                    {
                        ServerComponentReferenceManager.ServerInstance.console
                            .ExecuteInput(input, player.NetworkPlayerID, out text, out flag2, out ex, true);
                        break;
                    }
                }
                __result = input != null;
            }
        }

        [HarmonyPatch(typeof(GameConsolePanel), "SetCommands")]
        private static class ServerCommands_Patch
        {
            static void Prefix(ref IEnumerable<IConsoleCommand> commands)
            {
                List<IConsoleCommand> list = new List<IConsoleCommand>(commands);
                IConsoleCommand command = null;
                foreach (AddCommandList processor in addCommandListDelegate.GetInvocationList())
                {
                    command = processor();
                    if (command != null)
                    {
                        commands.Add(command);
                    }
                }
                commands = list;
            }
        }

        public static void sendMessage(int steamId, string message)
        {
            ServerComponentReferenceManager.ServerInstance.serverAdminBroadcastMessageManager.PrivateMessage(-1, steamId, message);
        }

        public static void sendMessage(string message)
        {
            ServerComponentReferenceManager.ServerInstance.serverAdminBroadcastMessageManager.BroadcastAdminMessage(message);
        }

        private static int getRoundTime()
        {
            return (int) ServerComponentReferenceManager.ServerInstance.serverRoundTimerManager.secondsSinceRoundStarted;
        }
    }
}
