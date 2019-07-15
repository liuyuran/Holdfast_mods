/**
* @file CarbonManage.cs
* @brief AI模块文件
* @details 这个文件包含了机器人相关逻辑
* @author 夏洛特
* @version 0.1b
* @date 2019-07-04
*/

using Harmony12;
using HoldfastGame;
using System.Collections.Generic;
using uLink;
using UnityEngine;

namespace ServerModFramework
{
    public static partial class Framework
    {
        private static List<int> carbonList = new List<int>();
        private static Dictionary<int, CarbonPlayer> carbonLink = new Dictionary<int, CarbonPlayer>();
        private static Dictionary<int, string> carbonNameLink = new Dictionary<int, string>();
        private static ServerComponentReferenceManager instant => ServerComponentReferenceManager.ServerInstance;
        public class CarbonPlayer
        {
            private NetworkPlayer networkPlayer;
            public NetworkPlayer getNetworkPlayer => networkPlayer;
            public CarbonPlayerRepresentation representation { get; private set; }
            public CarbonPlayer(NetworkPlayer networkPlayer)
            {
                this.networkPlayer = networkPlayer;
            }
            private CharacterVoiceIdentifier getDefaultVoice(FactionCountry factionCountry)
            {
                switch(factionCountry)
                {
                    case FactionCountry.British:
                        return CharacterVoiceIdentifier.Joseph_British_1;
                    case FactionCountry.French:
                        return CharacterVoiceIdentifier.Herve_French_1;
                    case FactionCountry.Prussian:
                        return CharacterVoiceIdentifier.Fabrec_Prussian_1;
                    default:
                        return CharacterVoiceIdentifier.Joseph_British_1;
                }
            }
            public void bindRepresentation(CarbonPlayerRepresentation representation)
            {
                this.representation = representation;
            }
            public void spawn(FactionCountry factionCountry, PlayerClass playerClass, SpawnSection spawnSection)
            {
                if (representation == null) return;
                int currentRoundIdentifier = instant.serverGameManager.CurrentRoundIdentifier;
                SpawnSectionCriteria spawnSectionCriteria = ComponentReferenceManager.genericObjectPools.spawnSectionCriteria.Obtain();
                spawnSectionCriteria.Faction = factionCountry;
                dfList<SpawnSection> dfList = instant.serverSpawnSectionManager.QuerySpawnSections(spawnSectionCriteria, false);
                ComponentReferenceManager.genericObjectPools.spawnSectionCriteria.Release(spawnSectionCriteria);
                if (dfList.Count == 0)
                {
                    dfList.Release();
                    return;
                }
                dfList.Release();
                int sectionIdentifier = spawnSection.sectionIdentifier;
                int characterHeadIdentifier = 1;
                CharacterVoiceIdentifier characterVoiceIdentifier = getDefaultVoice(factionCountry);
                ClientChosenSpawnSettings spawnSettings = new ClientChosenSpawnSettings
                {
                    RoundIdentifier = currentRoundIdentifier,
                    CharacterVoiceIdentifier = characterVoiceIdentifier,
                    CharacterHeadIdentifier = characterHeadIdentifier,
                    Faction = factionCountry,
                    ClassType = playerClass,
                    SpawnSectionID = sectionIdentifier
                };
                representation.timeLastAttemptedToSpawn = Time.time;
                instant.serverGameManager.QueueClientChosenSpawnSettings(networkPlayer, spawnSettings, false);
            }
            public void carbonCommandSwitchWeapon(WeaponType weaponType)
            {
                ServerRoundPlayer serverRoundPlayer = instant.serverRoundPlayerManager.ResolveServerRoundPlayer(networkPlayer.id);
                if (serverRoundPlayer != null)
                {
                    Weapon weapon = serverRoundPlayer.WeaponHolder.AvailableWeapons.GetWeapon(weaponType, serverRoundPlayer.SpawnData.Faction);
                    if (!(weapon == null))
                    {
                        instant.serverWeaponHolderManager.BroadcastSwitchWeapon(serverRoundPlayer.NetworkPlayerID, weapon.identifier.ID);
                    }
                }
            }
            public void teleport(Vector3 vector) {
                ServerRoundPlayer serverRoundPlayer = instant.serverRoundPlayerManager.ResolveServerRoundPlayer(networkPlayer.id);
                if (serverRoundPlayer != null)
                {
                    vector = vector.ReplaceY(200f);
                    Ray ray = new Ray(vector, Vector3.down);
                    RaycastHit raycastHit2;
                    bool flag2 = Physics.Raycast(ray, out raycastHit2, 500f, instant.commonGlobalVariables.layers.walkable);
                    if (flag2)
                    {
                        vector = vector.ReplaceY(raycastHit2.point.y);
                        serverRoundPlayer.PlayerBase.Teleport(vector);
                    }
                }
            }
            public void activeAction(PlayerActions playerAction, Vector3 look, MeleeStrikeType meleeStrike = MeleeStrikeType.None)
            {
                ServerRoundPlayer player = instant.serverRoundPlayerManager.ResolveServerRoundPlayer(networkPlayer.id);
                if (player == null) return;
                AutonomousPlayerInputHandler input = representation.input;
                OwnerPacketToServer ownerPacketToServer = ComponentReferenceManager.genericObjectPools.ownerPacketToServer.Obtain();
                byte spawnInstance = player.PlayerBase.PlayerStartData.SpawnInstance;
                Vector2 axis = input.Axis;
                axis.x = look.x;
                axis.y = look.z;
                ownerPacketToServer.Instance = new byte?(spawnInstance);
                ownerPacketToServer.OwnerInputAxis = new Vector2?(axis);
                ownerPacketToServer.OwnerRotationY = new float?(look.y);
                ownerPacketToServer.Swimming = false;
                if (playerAction != PlayerActions.None)
                {
                    EnumCollection<PlayerActions> enumCollection = ComponentReferenceManager.genericObjectPools.playerActionsEnumCollection.Obtain();
                    enumCollection.Add((int)playerAction);
                    if (playerAction == PlayerActions.FireFirearm)
                    {
                        double networkTime = uLinkNetworkConnectionsCollection.networkTime;
                        ownerPacketToServer.CameraForward = new Vector3?(player.PlayerTransform.forward);
                        ownerPacketToServer.CameraPosition = new Vector3?(player.PlayerTransform.position);
                        ownerPacketToServer.PacketTimestamp = new double?(networkTime);
                    }
                    ownerPacketToServer.ActionCollection = enumCollection;
                }
                player.uLinkStrictPlatformerCreator.HandleOwnerPacketToServer(ownerPacketToServer);
                if (meleeStrike != MeleeStrikeType.None)
                {
                    PlayerMeleeStrikePacket playerMeleeStrikePacket = ComponentReferenceManager.genericObjectPools.playerMeleeStrikePacket.Obtain();
                    playerMeleeStrikePacket.AttackTime = uLinkNetworkConnectionsCollection.networkTime;
                    playerMeleeStrikePacket.AttackingPlayerID = representation.playerID;
                    playerMeleeStrikePacket.AttackingPlayerMeleeWeaponDamageDealerTypeID = player.WeaponHolder.ActiveWeaponDetails.damageDealerTypeID;
                    playerMeleeStrikePacket.MeleeStrikeType = meleeStrike;
                    instant.meleeStrikeManager.MeleeAttackStrike(playerMeleeStrikePacket);
                }
            }
        }

        public static CarbonPlayer getCarbonPlayer(int playerId) {
            if (!carbonLink.ContainsKey(playerId)) return null;
            return carbonLink[playerId];
        }

        public static int addCarbonPlayer(string name)
        {
            NetworkPlayer networkPlayer = Network.NetworkClientAllocator.Allocate();
            networkPlayer.isCarbonPlayer = true;
            ClientSteamAuthSettings auth = new ClientSteamAuthSettings
            {
                SteamID = 0UL,
                SteamCBAuthTicket = 0,
                SteamPAuthTicket = new byte[0],
                MachineID = SystemInfo.deviceUniqueIdentifier,
                MachineSecurityID = StringCipher.Hash(SystemInfo.deviceUniqueIdentifier, 0UL)
            };
            instant.serverGameManager.HandleSteamAuthSettings(auth, networkPlayer);
            carbonList.Add(networkPlayer.id);
            carbonLink.Add(networkPlayer.id, new CarbonPlayer(networkPlayer));
            carbonNameLink.Add(networkPlayer.id, name);
            return networkPlayer.id;
        }

        public static void removeCarbonPlayer(int id)
        {
            if (!carbonList.Contains(id) || !carbonLink.ContainsKey(id)) return;
            Network.NetworkClientAllocator.Deallocate(carbonLink[id].getNetworkPlayer, 0.0);
            carbonList.Remove(id);
            carbonLink.Remove(id);
        }

        [HarmonyPatch(typeof(ServerCarbonPlayersManager), "RequestingInitialDetailsRPC")]
        private static class CarbonManager_RequestingInitialDetailsRPC_Patch
        {
            static bool Prefix(NetworkPlayer networkPlayer)
            {
                int characterFaceIdentifier = 0;
                PlayerInitialDetails playerInitialDetails = new PlayerInitialDetails
                {
                    Name = carbonNameLink[networkPlayer.id],
                    CharacterVoicePitch = 1f,
                    CharacterFaceIdentifier = characterFaceIdentifier
                };
                carbonNameLink.Remove(networkPlayer.id);
                instant.serverGameManager.HandlePlayerInitialDetails(playerInitialDetails, networkPlayer);
                AutonomousPlayerInputHandler input = new AutonomousPlayerInputHandler(instant.commonGlobalVariables.BotAutoMove);
                CarbonPlayerRepresentation value = new CarbonPlayerRepresentation
                {
                    playerID = networkPlayer.id,
                    input = input,
                    networkPlayer = networkPlayer
                };
                if(carbonLink.ContainsKey(networkPlayer.id)) carbonLink[networkPlayer.id].bindRepresentation(value);
                return false;
            }
        }
    }
}
