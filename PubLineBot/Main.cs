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
        private static Dictionary<int, Vector3> rotationCache = new Dictionary<int, Vector3>();
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
            Framework.playerActionUpdateDelegate += Framework_playerActionUpdateDelegate;
            Framework.officerOrderDelegate += Framework_officerOrderDelegate;
            Framework.roundEndDelegate += Framework_roundEndDelegate;
            logger.Log("线列机器人加载完成");
            return true;
        }

        private static void Framework_officerOrderDelegate(bool isStart, RequestStartOfficerOrderPacket currentRequestPacket)
        {
            int officerId = currentRequestPacket.officerNetworkPlayer.id;
            logger.Log(currentRequestPacket.officerOrderType.ToString());
            playerToBot[officerId].ForEach((int id) => {
                switch(currentRequestPacket.officerOrderType)
                {
                    case OfficerOrderType.FormLine:
                        Vector3 target = new Vector3();
                        target.x = 0;
                        target.z = 0;
                        target.y = currentRequestPacket.orderRotationY - 180;
                        if (rotationCache.ContainsKey(id)) rotationCache[id] = target;
                        else rotationCache.Add(id, target);
                        Framework.getCarbonPlayer(id).activeAction(PlayerActions.None, target, MeleeStrikeType.None);
                        break;
                    case OfficerOrderType.MakeReady:
                        Framework.getCarbonPlayer(id).activeAction(PlayerActions.StartAimingFirearm, rotationCache[id], MeleeStrikeType.None);
                        break;
                    case OfficerOrderType.Fire:
                        Framework.getCarbonPlayer(id).activeAction(PlayerActions.FireFirearm, rotationCache[id], MeleeStrikeType.None);
                        break;
                }
            });
        }

        private static void Framework_roundEndDelegate(GameDetails detail)
        {
            playerToBot.ToDfList().ForEach((KeyValuePair<int, List<int>> pair) =>
            {
                pair.Value.ForEach((int id) =>
                {
                    Framework.removeCarbonPlayer(id);
                });
                playerToBot.Remove(pair.Key);
                ServerRoundPlayer serverRoundPlayer =
                    instant.serverRoundPlayerManager.ResolveServerRoundPlayer(pair.Key);
                wayPoint.Remove(serverRoundPlayer.NetworkPlayerID);
            });
        }

        private static void Framework_playerActionUpdateDelegate(ulong steamId, PlayerActions action)
        {
            // TODO 切换行进模式
            ServerRoundPlayer serverRoundPlayer = instant.serverRoundPlayerManager.ResolveServerRoundPlayer(Framework.getPlayerId(steamId));
            logger.Log(serverRoundPlayer.ServerPlayerBase.name + action.ToString() + serverRoundPlayer.PlayerBase.transform.forward.ToString());
        }

        [HarmonyPatch(typeof(ServerCarbonPlayersManager), "FixedUpdate")]
        private static class FixedUpdate_Patch
        {
            static void Postfix()
            {
                /*wayPoint.ToDfList().ForEach((KeyValuePair<int, Queue<Vector3>> pair) => {
                    ServerRoundPlayer serverRoundPlayer = instant.serverRoundPlayerManager.ResolveServerRoundPlayer(pair.Key);
                    List<Vector3> list = wayPoint[pair.Key].ToDfList().ToList();
                    if ((serverRoundPlayer.PlayerBase.transform.position - list[list.Count - 1]).magnitude < 1) return;
                    wayPoint[pair.Key].Dequeue();
                    wayPoint[pair.Key].Enqueue(serverRoundPlayer.PlayerTransform.position);
                    list = wayPoint[pair.Key].ToDfList().ToList();
                    int index = 1;
                    playerToBot[pair.Key].ForEach((int id) => {
                        ServerRoundPlayer bot = instant.serverRoundPlayerManager.ResolveServerRoundPlayer(id);
                        int nowIndex = index++;
                        Vector3 target = serverRoundPlayer.PlayerTransform.position - (nowIndex * serverRoundPlayer.PlayerTransform.forward) - bot.PlayerTransform.position;
                        if (target.magnitude < 2f)
                        {
                            target.x = 0;
                            target.z = 0;
                        }
                        target.y = getRotation(
                            bot.PlayerBase.transform.position
                            ).eulerAngles.y;
                        Framework.getCarbonPlayer(id).activeAction(PlayerActions.None, target, MeleeStrikeType.None);
                    });
                });*/
            }
        }

        private static Quaternion getRotation(Vector3 position)
        {
            return Quaternion.Euler(position.x, position.z, 0f);
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
                playerToBot[playerId].ForEach((int id) => {
                    Framework.removeCarbonPlayer(id);
                });
                playerToBot.Remove(playerId);
                players.Add(serverRoundPlayer);
            }
            else if(serverRoundPlayer.SpawnData.ClassType == PlayerClass.ArmyInfantryOfficer)
            {
                players.Add(serverRoundPlayer);
                Queue<Vector3> queue = new Queue<Vector3>();
                Vector3 basePos = serverRoundPlayer.PlayerBase.transform.position;
                wayPoint.Add(serverRoundPlayer.NetworkPlayerID, queue);
                for (int i = 0; i < maxBot; i++)
                {
                    basePos.z += 1;
                    queue.Enqueue(basePos);
                }
            } else if(wayPoint.ContainsKey(serverRoundPlayer.NetworkPlayerID))
            {
                playerToBot[playerId].ForEach((int id) => {
                    Framework.removeCarbonPlayer(id);
                });
                wayPoint.Remove(serverRoundPlayer.NetworkPlayerID);
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
                        ids.Add(Framework.addCarbonPlayer("test01"));
                    }
                    ids.ForEach((id) =>
                    {
                        position.z += 1;
                        Framework.CarbonPlayer player = Framework.getCarbonPlayer(id);
                        player.spawn(serverRoundPlayer.PlayerStartData.Faction, PlayerClass.ArmyLineInfantry);
                        posCache.Add(id, position);
                    });
                    if (playerToBot.ContainsKey(serverRoundPlayer.NetworkPlayerID)) playerToBot.Remove(serverRoundPlayer.NetworkPlayerID);
                    playerToBot.Add(serverRoundPlayer.NetworkPlayerID, ids);
                });
                players.Clear();
            }
        }

        float VectorAngle(Vector2 from, Vector2 to)
        {
            float angle;
            Vector3 cross = Vector3.Cross(from, to);
            angle = Vector2.Angle(from, to);
            return cross.z > 0 ? -angle : angle;
        }
    }
}
