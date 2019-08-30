using Harmony12;
using HoldfastGame;
using ServerModFramework;
using System;
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
        private static Dictionary<FactionCountry, Queue<Vector3>> posList = new Dictionary<FactionCountry, Queue<Vector3>>();
        private static ServerComponentReferenceManager instant
        {
            get
            {
                return ServerComponentReferenceManager.ServerInstance;
            }
        }

        public static bool Load(UnityModManager.ModEntry modEntry)
        {
            modEntry.OnToggle = OnToggle;
            logger = modEntry.Logger;
            logger.Log("插件开始加载 patch id:" + modEntry.Info.Id);
            Framework.playerCommandDelegate += Framework_playerCommandDelegate;
            Framework.roundEndDelegate += Framework_roundEndDelegate;
            Framework.playerSpawnDelegate += Framework_playerSpawnDelegate;
            var harmony = HarmonyInstance.Create(modEntry.Info.Id);
            harmony.PatchAll(Assembly.GetExecutingAssembly());
            logger.Log("插件注入完毕");
            return true;
        }

        private static void Framework_playerSpawnDelegate(int playerId)
        {
            ServerRoundPlayer serverRoundPlayer =
                instant.serverRoundPlayerManager.ResolveServerRoundPlayer(playerId);
            if (serverRoundPlayer.NetworkPlayer.isCarbonPlayer && serverRoundPlayer.SpawnData.ClassType == PlayerClass.ArmyLineInfantry)
            {
                FactionCountry faction = serverRoundPlayer.SpawnData.Faction;
                if (!posList.ContainsKey(faction)) return;
                Vector3 vector = posList[faction].Dequeue();
                Framework.CarbonPlayer player = Framework.getCarbonPlayer(playerId);
                player.teleport(vector);
            }
        }

        private static void Framework_roundEndDelegate(HoldfastGame.GameDetails detail)
        {
            clearTarget();
        }

        private static string Framework_playerCommandDelegate(string modName, object[] arguments, ulong steamID, out bool success)
        {
            if (modName != "targetBot" || !enabled)
            {
                success = false;
                return null;
            }
            ServerRoundPlayer serverRoundPlayer = instant.serverRoundPlayerManager.ResolveServerRoundPlayer(Framework.getPlayerId(steamID));
            string command = (string)arguments[0];
            switch (command)
            {
                case "add":
                    try {
                        spawnTarget(
                        serverRoundPlayer.SpawnData.Faction,
                        serverRoundPlayer.PlayerTransform.position,
                        serverRoundPlayer.PlayerTransform.forward,
                        Convert.ToInt32(arguments[1]),
                        Convert.ToInt32(arguments[2])
                        );
                        success = true;
                        return "下一轮增援到达后，靶标就会出现";
                    } catch (Exception e) {
                        logger.Log(e.StackTrace);
                        success = false;
                        return "调用出错，请联系服务器管理员获取错误堆栈";
                    }
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

        private static void spawnTarget(FactionCountry faction, Vector3 position, Vector3 forward, int distance, int num) {
            if (!posList.ContainsKey(faction)) posList.Add(faction, new Queue<Vector3>());
            Vector3 center = position + forward.normalized * distance;
            Vector3 forward_hoz = Quaternion.Euler(0, 90, 0) * forward.normalized;
            for (int i=0; i<num;i++)
            {
                Vector3 item;
                if (i % 2 == 0)
                    item = center + (i / 2) * forward_hoz.normalized;
                else
                    item = center - (i / 2) * forward_hoz.normalized;
                posList[faction].Enqueue(item);
            }
        }

        private static void clearTarget() {
            botList.ForEach((int item) =>
            {
                Framework.removeCarbonPlayer(item);
            });
            botList.Clear();
            posList.Clear();
        }

        [HarmonyPatch(typeof(ServerGameManager), "SpawnFromQueuedSettings")]
        private static class SpawnFromQueuedSettings_Patch
        {
            static void Postfix()
            {
                posList.ToDfList().ForEach((KeyValuePair<FactionCountry, Queue<Vector3>> pair) =>
                {
                    pair.Value.ToDfList().ForEach((Vector3 pos)=>
                    {
                        int id = Framework.addCarbonPlayer("test01");
                        Framework.CarbonPlayer player = Framework.getCarbonPlayer(id);
                        player.spawn(pair.Key, PlayerClass.ArmyLineInfantry);
                    });
                });
            }
        }
    }
}
