using ServerModFramework;
using UnityModManagerNet;
using Harmony12;
using System.Reflection;
using System.Collections;
using System.IO;
using System.Text.RegularExpressions;
using System;
using HoldfastGame;

namespace CompereRobot
{
    public class Main
    {
        class ScriptItem
        {
            public int second = 0;
            public string message = "";
        }

        private static UnityModManager.ModEntry.ModLogger logger;
        private static string basePath;
        private static Queue script = new Queue();
        private static readonly Regex reg = new Regex("\\[(\\d{2}:\\d{2})\\] (.+)");
        public static bool Load(UnityModManager.ModEntry modEntry)
        {
            basePath = modEntry.Path;
            logger = modEntry.Logger;
            var harmony = HarmonyInstance.Create(modEntry.Info.Id);
            harmony.PatchAll(Assembly.GetExecutingAssembly());
            Framework.countDownDelegate += timelineLoop;
            Framework.adminCommandDelegate += serverCommand;
            Framework.playerJoinDelegate += loadPluginBroadcast;
            Framework.roundEndDelegate += clearScenario;
            logger.Log("虚拟主持人加载完成");
            return true;
        }

        private static void clearScenario(GameDetails details) {
            script.Clear();
        }

        private static void loadPluginBroadcast(ulong steamId) {
            Framework.sendMessage(steamId, ServerConfigurationFileManager.CurrentConfigurationFile.WelcomeMessage);
        }

        private static string serverCommand(string modName, object[] arguments, int adminID, out bool success)
        {
            string command = (string)arguments[0];
            if (modName != "compere")
            {
                success = false;
                return null;
            }
            switch (command)
            {
                case "play":
                    if (arguments.Length < 3)
                    {
                        success = false;
                        return "未指定剧本名";
                    }
                    return loadScenario((string)arguments[1], out success);
                default:
                    success = false;
                    return string.Format("不合法的操作符: %s", command);
            }
        }

        private static void timelineLoop(int time)
        {
            while(true)
            {
                ScriptItem item = (ScriptItem) script.Peek();
                if (item.second > time) break;
                item = (ScriptItem) script.Dequeue();
                Framework.sendMessage(item.message);
            }
        }

        private static string loadScenario(string name, out bool success)
        {
            script.Clear();
            string path = string.Format("{0}{1}.txt", basePath, name);
            if (!File.Exists(path))
            {
                success = false;
                return "剧本文件不存在";
            }
            StreamReader sr = new StreamReader(path);
            while (!sr.EndOfStream)
            {
                try
                {
                    string str = sr.ReadLine();
                    Match result = reg.Match(str);
                    ScriptItem item = new ScriptItem();
                    item.second = timeTagToSecond(result.Groups[1].Value);
                    item.message = result.Groups[2].Value;
                    script.Enqueue(item);
                }
                catch (Exception e)
                {
                    success = false;
                    return "剧本加载失败:" + e.Message;
                }
            }
            success = true;
            return "运行正常";
        }

        private static int timeTagToSecond(string tag)
        {
            string[] timeStr = tag.Split(':');
            return Convert.ToInt32(timeStr[0]) * 60 + Convert.ToInt32(timeStr[1]);
        }
    }
}
