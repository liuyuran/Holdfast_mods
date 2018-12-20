using Harmony12;
using HoldfastGame;
using System.Collections;
using System.Reflection;
using UnityEngine;
using UnityModManagerNet;

namespace CleverRobot
{
    public class Main
    {
        public static UnityModManager.ModEntry.ModLogger logger;
        private static RobotManager robotManager;

        public static bool Load(UnityModManager.ModEntry modEntry)
        {
            logger = modEntry.Logger;
            robotManager = new RobotManager();
            var harmony = HarmonyInstance.Create(modEntry.Info.Id);
            harmony.PatchAll(Assembly.GetExecutingAssembly());
            logger.Log("增强AI加载完成");
            return true;
        }

        [HarmonyPatch(typeof(ServerCarbonPlayersManager), "Update")]
        public static class Manager_Patch
        {
            static void Postfix(ServerCarbonPlayersManager __instance)
            {
                //
            }
        }

        [HarmonyPatch(typeof(AutonomousPlayerInputHandler), "ManualUpdate")]
        public static class InputHandle_Patch
        {
            static void Postfix(AutonomousPlayerInputHandler __instance)
            {
                Traverse.Create(__instance).Field("currentAxis").SetValue(new Vector2(0f, 0f));
            }
        }
    }
}
