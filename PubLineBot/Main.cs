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

        private static int maxBot = 5;

        private static List<ServerRoundPlayer> players = new List<ServerRoundPlayer>();
        private static Dictionary<int, Vector3> posCache = new Dictionary<int, Vector3>();
        private static Dictionary<int, List<int>> playerToBot = new Dictionary<int, List<int>>();

        private static Dictionary<int, Queue<Vector3>> wayPoint = new Dictionary<int, Queue<Vector3>>();
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
            Framework.playerDeadDelegate += Framework_playerDeadDelegate;
            Framework.playerLeaveDelegate += Framework_playerDeadDelegate;
            Framework.countDownDelegate += Framework_countDownDelegate;
            Framework.playerActionUpdateDelegate += Framework_playerActionUpdateDelegate;
            logger.Log("线列机器人加载完成");
            return true;
        }

        private static void Framework_playerActionUpdateDelegate(ulong steamId, PlayerActions action)
        {
            // TODO 切换行进模式
        }

        private static void Framework_countDownDelegate(int roundTime)
        {
            // TODO 每秒获取所有人的位置，并塞进队列，然后通知机器人行进
            wayPoint.ToDfList().ForEach((KeyValuePair<int, Queue<Vector3>> pair)=> {
                ServerRoundPlayer serverRoundPlayer = instant.serverRoundPlayerManager.ResolveServerRoundPlayer(pair.Key);
                do
                {
                    wayPoint[pair.Key].Enqueue(serverRoundPlayer.PlayerTransform.position);
                } while (wayPoint[pair.Key].Count == maxBot);
                if (wayPoint[pair.Key].Count > maxBot) wayPoint[pair.Key].Dequeue();
                /*int index = 0;
                playerToBot[pair.Key].ForEach((int id) => {
                    Framework.getCarbonPlayer(id).activeAction(PlayerActions.Run, wayPoint[pair.Key].ToDfList()[index++], MeleeStrikeType.None);
                });*/
            });
        }

        private static void Framework_playerDeadDelegate(ulong steamId)
        {
            int playerId = Framework.getPlayerId(steamId);
            if (playerId == -1) return;
            if (!playerToBot.ContainsKey(playerId)) return;
            playerToBot[playerId].ForEach((int id)=> {
                Framework.removeCarbonPlayer(id);
            });
            playerToBot.Remove(playerId);
            ServerRoundPlayer serverRoundPlayer =
                instant.serverRoundPlayerManager.ResolveServerRoundPlayer(playerId);
            wayPoint.Remove(serverRoundPlayer.NetworkPlayerID);
        }

        private static void Framework_playerSpawnDelegate(int playerId)
        {
            ServerRoundPlayer serverRoundPlayer = 
                instant.serverRoundPlayerManager.ResolveServerRoundPlayer(playerId);
            if (serverRoundPlayer.NetworkPlayer.isCarbonPlayer && serverRoundPlayer.SpawnData.ClassType != PlayerClass.ArmyInfantryOfficer) {
                logger.Log("NPC重生：" + playerId);
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
                serverRoundPlayer.PlayerBase.Teleport(vector);
                posCache.Remove(playerId);
            }
            else if(wayPoint.ContainsKey(serverRoundPlayer.NetworkPlayerID) && serverRoundPlayer.SpawnData.ClassType == PlayerClass.ArmyInfantryOfficer)
            {
                logger.Log("军官复生：" + playerId);
                playerToBot[playerId].ForEach((int id) => {
                    Framework.removeCarbonPlayer(id);
                });
                playerToBot.Remove(playerId);
                players.Add(serverRoundPlayer);
            }
            else if(serverRoundPlayer.SpawnData.ClassType == PlayerClass.ArmyInfantryOfficer)
            {
                logger.Log("军官出生：" + playerId);
                players.Add(serverRoundPlayer);
                wayPoint.Add(serverRoundPlayer.NetworkPlayerID, new Queue<Vector3>());
            } else if(wayPoint.ContainsKey(serverRoundPlayer.NetworkPlayerID))
            {
                logger.Log("军官转士兵：" + playerId);
                playerToBot[playerId].ForEach((int id) => {
                    Framework.removeCarbonPlayer(id);
                });
                wayPoint.Remove(serverRoundPlayer.NetworkPlayerID);
            } else
            {
                logger.Log("未知出生：" + playerId);
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
                    for (int i = 0; i < maxBot; i++)
                    {
                        ids.Add(Framework.addCarbonPlayer("test"));
                    }
                    ids.ForEach((id) =>
                    {
                        position.z += 1;
                        Framework.CarbonPlayer player = Framework.getCarbonPlayer(id);
                        player.spawn(serverRoundPlayer.PlayerStartData.Faction, PlayerClass.ArmyLineInfantry);
                        posCache.Add(id, position);
                    });
                    playerToBot.Add(serverRoundPlayer.NetworkPlayerID, ids);
                });
                players.Clear();
            }
        }
    }
}
