using Harmony12;
using HoldfastGame;
using ServerModFramework;
using System.Collections.Generic;
using System.Reflection;
using uLink;
using UnityEngine;
using UnityModManagerNet;

namespace PubLineBot
{
    public class Main
    {
        private static UnityModManager.ModEntry.ModLogger logger;
        private static List<ServerRoundPlayer> players = new List<ServerRoundPlayer>();
        private static Dictionary<int, Vector3> posCache = new Dictionary<int, Vector3>();
        private static ServerComponentReferenceManager instant
        {
            get
            {
                return ServerComponentReferenceManager.ServerInstance;
            }
        }
        public static bool Load(UnityModManager.ModEntry modEntry)
        {
            logger = modEntry.Logger;
            var harmony = HarmonyInstance.Create(modEntry.Info.Id);
            harmony.PatchAll(Assembly.GetExecutingAssembly());
            Framework.playerSpawnDelegate += Framework_playerSpawnDelegate;
            logger.Log("线列机器人加载完成");
            return true;
        }

        private static void Framework_playerSpawnDelegate(int playerId)
        {
            logger.Log("玩家进入战场，机器人开始入场");
            ServerRoundPlayer serverRoundPlayer = 
                instant.serverRoundPlayerManager.ResolveServerRoundPlayer(playerId);
            if (serverRoundPlayer.NetworkPlayer.isCarbonPlayer && serverRoundPlayer.SpawnData.ClassType != PlayerClass.ArmyInfantryOfficer) {
                if (!posCache.ContainsKey(playerId)) return;
                Vector3 vector = posCache[playerId];
                vector = vector.ReplaceY(200f);
                Ray ray = new Ray(vector, Vector3.down);
                RaycastHit raycastHit2;
                bool flag2 = Physics.Raycast(ray, out raycastHit2, 500f, instant.commonGlobalVariables.layers.walkable);
                if (flag2)
                {
                    vector = vector.ReplaceY(raycastHit2.point.y);
                    serverRoundPlayer.PlayerBase.Teleport(vector);
                }
                logger.Log("定位目标:" + vector.ToString());
                serverRoundPlayer.PlayerBase.Teleport(vector);
                posCache.Remove(playerId);
            }
            else {
                players.Add(serverRoundPlayer);
            }
        }

        [HarmonyPatch(typeof(ServerGameManager), "SpawnFromQueuedSettings")]
        private static class SpawnFromQueuedSettings_Patch
        {
            static void Postfix()
            {
                players.ForEach((ServerRoundPlayer serverRoundPlayer) =>
                {
                    Vector3 position = serverRoundPlayer.PlayerTransform.position;
                    List<int> ids = new List<int>();
                    for (int i = 0; i < 5; i++)
                    {
                        ids.Add(Framework.addCarbonPlayer("test"));
                    }
                    logger.Log("传送基点:" + position.ToString());
                    ids.ForEach((id) =>
                    {
                        position.z += 1;
                        Framework.CarbonPlayer player = Framework.getCarbonPlayer(id);
                        player.spawn(serverRoundPlayer.PlayerStartData.Faction, PlayerClass.ArmyLineInfantry);
                        posCache.Add(id, position);
                    });
                });
                players.Clear();
            }
        }
    }
}
