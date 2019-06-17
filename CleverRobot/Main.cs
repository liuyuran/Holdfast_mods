using Harmony12;
using HoldfastGame;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityModManagerNet;

namespace CleverRobot
{
    static public class Main
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

        [HarmonyPatch(typeof(ServerCarbonPlayersManager), "RequestingInitialDetailsRPC")]
        public static class CarbonName_Patch
        {
            static bool Prefix(ServerCarbonPlayersManager __instance, uLink.NetworkPlayer networkPlayer)
            {
                return true;
            }
        }

        [HarmonyPatch(typeof(ServerCarbonPlayersManager), "UpdateCarbonPlayerInput")]
        public static class Manager_Patch
        {
            static bool Prefix(ServerCarbonPlayersManager __instance)
            {
                return false;
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
