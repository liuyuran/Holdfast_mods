using Harmony12;
using HoldfastGame;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityModManagerNet;

namespace CleverRobot
{
    public class Main
    {
        public static UnityModManager.ModEntry.ModLogger logger;
        private static RobotManager robotManager = new RobotManager();
        public static Hashtable inputLinkA = new Hashtable();
        public static Hashtable inputLinkB = new Hashtable();
        public static HashSet<int> set = new HashSet<int>();
        public static int count = 0;
        public static ServerRoundPlayerManager serverRoundPlayerManager = null;

        public static bool Load(UnityModManager.ModEntry modEntry)
        {
            logger = modEntry.Logger;
            var harmony = HarmonyInstance.Create("chpr");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
            logger.Log("增强AI加载完成");
            return true;
        }

        [HarmonyPatch(typeof(ServerCarbonPlayersManager), "Update")]
        public static class Manager_Patch
        {
            static void Postfix(ServerCarbonPlayersManager __instance)
            {
                dfList<int> carbonPlayerIDs = Traverse.Create(__instance).Field("carbonPlayerIDs").GetValue<dfList<int>>();
                if (carbonPlayerIDs.Count < 6) return;
                if (serverRoundPlayerManager == null) {
                    ServerComponentReferenceManager serverInstance = ServerComponentReferenceManager.ServerInstance;
                    serverRoundPlayerManager = serverInstance.serverRoundPlayerManager;
                    return;
                }
                Dictionary<int, CarbonPlayerRepresentation> carbonPlayers = Traverse.Create(__instance).Field("carbonPlayers").GetValue<Dictionary<int, CarbonPlayerRepresentation>>();
                for (int i = 0; i < carbonPlayerIDs.Count; i++)
                {
                    int key = carbonPlayerIDs[i];
                    CarbonPlayerRepresentation carbonPlayerRepresentation = carbonPlayers[key];
                    AutonomousPlayerInputHandler input = carbonPlayerRepresentation.input;
                    inputLinkA.Add(input, key);
                    inputLinkB.Add(key, input);
                    if (!set.Contains(key)) {
                        ServerRoundPlayer serverRoundPlayer = serverRoundPlayerManager.ResolveServerRoundPlayer(key);
                        robotManager.addRobot(0, 0);
                        set.Add(key);
                    }
                }
                if (count % 60 != 0) return;
                if (count > 120) count = 0;
                if(robotManager.Count() == 6)
                {
                    robotManager.mass();
                    for (int i = 0; i < carbonPlayerIDs.Count; i++)
                    {
                        int key = carbonPlayerIDs[i];
                        ServerRoundPlayer serverRoundPlayer = serverRoundPlayerManager.ResolveServerRoundPlayer(key);
                        serverRoundPlayer.PlayerBase.Teleport(new Vector3(robotManager.robotList[i].position.x, robotManager.robotList[i].position.y));
                    }
                }
                count++;
            }
        }

        [HarmonyPatch(typeof(AutonomousPlayerInputHandler), "ManualUpdate")]
        public static class InputHandle_Patch
        {
            static void Postfix(AutonomousPlayerInputHandler __instance)
            {
                Traverse.Create(__instance).Field("currentAxis").SetValue(Vector2.zero);
                Traverse.Create(__instance).Field("currentMousePosition").SetValue(Vector2.zero);
            }
        }
    }
}
