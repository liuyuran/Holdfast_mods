using Harmony12;
using ServerModFramework;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityModManagerNet;

namespace TargetBot
{
    public class Main
    {
        public static bool enabled;
        public static UnityModManager.ModEntry.ModLogger logger;
        private static List<int> botList = new List<int>();

        public static bool Load(UnityModManager.ModEntry modEntry)
        {
            modEntry.OnToggle = OnToggle;
            logger = modEntry.Logger;
            logger.Log("插件开始加载 patch id:" + modEntry.Info.Id);
            Framework.playerCommandDelegate += Framework_playerCommandDelegate;
            Framework.roundEndDelegate += Framework_roundEndDelegate;
            var harmony = HarmonyInstance.Create(modEntry.Info.Id);
            harmony.PatchAll(Assembly.GetExecutingAssembly());
            logger.Log("插件注入完毕");
            return true;
        }

        private static void Framework_roundEndDelegate(HoldfastGame.GameDetails detail)
        {
            botList.ForEach((int item)=>
            {
                Framework.removeCarbonPlayer(item);
            });
        }

        private static string Framework_playerCommandDelegate(string modName, object[] arguments, ulong steamID, out bool success)
        {
            if (modName != "targetBot")
            {
                success = false;
                return null;
            }
            string command = (string)arguments[0];
            switch (command)
            {
                case "add":
                default:
                    success = false;
                    return string.Format("不合法的操作符: %s", command);
            }
        }

        public static bool OnToggle(UnityModManager.ModEntry modEntry, bool value)
        {
            enabled = value;
            return true;
        }

        private static void spawnTarget(Vector3 position, Vector3 forward, int distance, int num) { }

        private static void clearTarget() { }
    }
}
