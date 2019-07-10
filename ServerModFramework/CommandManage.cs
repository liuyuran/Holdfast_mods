/**
 * @file CommandManage.cs
 * @brief 命令系统模块文件
 * @details 这个文件包含了玩家命令和管理员命令相关逻辑
 * @author 夏洛特
 * @version 0.1b
 * @date 2019-07-04
 */

using Harmony12;
using HoldfastGame;
using System;
using System.Collections.Generic;
using uLink;

namespace ServerModFramework
{
    public delegate string AdminMessage(string command);
    public delegate string PlayerCommand(string modName, object[] arguments, ulong steamID, out bool success);
    public delegate string AdminCommand(string modName, object[] arguments, int adminID, out bool success);
    public static partial class Framework
    {
        /// 管理员频道监听器
        public static event AdminMessage adminMessageDelegate;
        /// 管理员命令监听器
        public static event AdminCommand adminCommandDelegate;
        /// 用户命令监听器
        public static event PlayerCommand playerCommandDelegate;

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
                        text = processor((string)arguments[0], arguments.RangeSubset(1, arguments.Length - 1), adminID, out success);
                        if (text != null || success) break;
                    }
                }
                if (text == null) return "mod command not found";
                return text;
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
                    if (input != null)
                    {
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
                if (channel != TextChatChannel.Round) return true;
                if (!entryText.StartsWith("/mod ")) return true;
                string text = null;
                bool success = false;
                ulong steamId = (ulong)netIdToSteamId[messageInfo.sender.id];
                string[] arguments = entryText.Split(' ');
                if (arguments.Length >= 2)
                {
                    foreach (PlayerCommand processor in playerCommandDelegate.GetInvocationList())
                    {
                        text = processor(arguments[1], arguments.RangeSubset(2, arguments.Length - 2), steamId, out success);
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
    }
}
