﻿using Harmony12;
using HoldfastGame;
using RCONServerLib;
using System;
using System.Net;
using System.Reflection;
using System.Text;
using UnityModManagerNet;

namespace RconRepeater
{
    public static class Main
    {
        private static UnityModManager.ModEntry.ModLogger logger;
        private static int port = 27015;
        private static RemoteConServer server = null;
        public static bool Load(UnityModManager.ModEntry modEntry)
        {
            logger = modEntry.Logger;
            var harmony = HarmonyInstance.Create(modEntry.Info.Id);
            harmony.PatchAll(Assembly.GetExecutingAssembly());
            logger.Log("RCON扩展服务加载完成");
            return true;
        }

        [HarmonyPatch(typeof(ServerConfigurationFileManager), "ReadConfigurationFile")]
        private static class ServerConfigurationFileManager_Patch {
            static void Postfix()
            {
                if (server != null) server.Password = ServerConfigurationFileManager.CurrentConfigurationFile.ServerAdminPassword;
                else startRconServer();
            }
        }
        private static void startRconServer() {
            server = new RemoteConServer(IPAddress.Any, port)
            {
                SendAuthImmediately = true,
                Debug = true,
                UseCustomCommandHandler = true,
                EnableIpWhitelist = false,
                MaxConnections = uint.MaxValue,
                MaxConnectionsPerIp = uint.MaxValue,
                MaxPasswordTries = 3,
                BanMinutes = 60 * 24,
                Password = ServerConfigurationFileManager.CurrentConfigurationFile.ServerAdminPassword
            };
            server.OnCommandReceived += Server_OnCommandReceived;
            server.StartListening();
        }

        private static string Server_OnCommandReceived(string command, System.Collections.Generic.IList<string> args)
        {
            string text;
            bool flag2;
            Exception ex;
            ServerComponentReferenceManager.ServerInstance.console
                            .ExecuteInput(UnicodeToString(command), -1, out text, out flag2, out ex, true);
            return StringToUnicode(text);
        }

        private static string StringToUnicode(string s)
        {
            char[] charbuffers = s.ToCharArray();
            byte[] buffer;
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < charbuffers.Length; i++)
            {
                buffer = Encoding.Unicode.GetBytes(charbuffers[i].ToString());
                sb.Append(string.Format("\\u{0:X2}{1:X2}", buffer[1], buffer[0]));
            }
            return sb.ToString();
        }

        private static string UnicodeToString(string srcText)
        {
            string dst = "";
            string src = srcText;
            int len = srcText.Length / 6;
            for (int i = 0; i <= len - 1; i++)
            {
                string str = src.Substring(0, 6).Substring(2);
                src = src.Substring(6);
                byte[] bytes = new byte[2];
                bytes[1] = byte.Parse(int.Parse(str.Substring(0, 2), System.Globalization.NumberStyles.HexNumber).ToString());
                bytes[0] = byte.Parse(int.Parse(str.Substring(2, 2), System.Globalization.NumberStyles.HexNumber).ToString());
                dst += Encoding.Unicode.GetString(bytes);
            }
            return dst;
        }
    }
}
