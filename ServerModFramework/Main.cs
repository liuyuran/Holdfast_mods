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
    public delegate string PlayerCommand(string modName, object[] arguments, ulong steamID, out bool success);
    public delegate string AdminCommand(string modName, object[] arguments, int adminID, out bool success);

    public class Framework
    {
        public static CountDown countDownDelegate = delegate (int roundTime) { };
        public static PlayerJoin playerJoinDelegate = delegate (ulong steamId) { };
        public static PlayerLeave playerLeaveDelegate = delegate (ulong steamId) { };
        public static AdminMessage adminMessageDelegate = delegate (string message) { return null; };
        public static AdminCommand adminCommandDelegate = delegate (string modName, object[] arguments, int adminID, out bool success) {
            success = false;
            return null;
        };
        public static PlayerCommand playerCommandDelegate = delegate (string modName, object[] arguments, ulong steamID, out bool success) {
            success = false;
            return null;
        };

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

        public class ModConsoleCommand : IConsoleCommand
        {
            public string Name
            {
                get
                {
                    return "mod";
                }
            }
            public string Description
            {
                get
                {
                    return "run a mod command";
                }
            }
            public IEnumerable<string> Parameters
            {
                get
                {
                    return this.Variables.Keys;
                }
            }

            public Dictionary<string, SetConsoleCommandVariable> Variables { get; private set; }

            public string Execute(object[] arguments, int adminID, out bool success)
            {
                string text = null;
                success = false;
                if (arguments.Length >= 2)
                {
                    foreach (AdminCommand processor in adminCommandDelegate.GetInvocationList())
                    {
                        text = processor((string) arguments[0], arguments.RangeSubset(1, arguments.Length - 1), adminID, out success);
                        if (text != null || success) break;
                    }
                }
                if (text == null) return "mod command not found";
                return text;
            }
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
            static bool Prefix(string entryText, RoundPlayer player, out bool __result)
            {
                string text;
                bool flag2;
                Exception ex;
                string input = null;
                foreach (AdminMessage processor in adminMessageDelegate.GetInvocationList())
                {
                    input = processor(entryText);
                    if (input != null) {
                        ServerComponentReferenceManager.ServerInstance.console
                            .ExecuteInput(input, player.NetworkPlayerID, out text, out flag2, out ex, true);
                        break;
                    }
                    
                }
                __result = input != null;
                return input == null;
            }
        }

        [HarmonyPatch(typeof(GameConsolePanel), "SetCommands")]
        private static class ServerCommands_Patch
        {
            static bool Prefix(ref IEnumerable<IConsoleCommand> commands)
            {
                commands = commands.Add(new ModConsoleCommand());
                foreach (var item in commands)
                {
                    logger.Log(item.Name);
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(ServerChatHandler), "SendChatMessage")]
        private static class PlayerCommands_Patch
        {
            static bool Prefix(string entryText, int textChannelID, NetworkMessageInfo messageInfo)
            {
                TextChatChannel channel = (TextChatChannel)textChannelID;
                if(channel != TextChatChannel.Round) return true;
                if (!entryText.StartsWith("/mod ")) return true;
                string text = null;
                bool success = false;
                ulong steamId = (ulong)netIdToSteamId[messageInfo.sender.id];
                string[] arguments = entryText.Split(' ');
                if (arguments.Length >= 2)
                {
                    foreach (PlayerCommand processor in playerCommandDelegate.GetInvocationList())
                    {
                        text = processor((string)arguments[1], arguments.RangeSubset(2, arguments.Length - 2), steamId, out success);
                        if (text != null || success)
                        {
                            break;
                        }
                    }
                }
                if (text == null) return true;
                else return false;
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
