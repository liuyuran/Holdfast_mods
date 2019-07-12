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
        private static ServerComponentReferenceManager instant => ServerComponentReferenceManager.ServerInstance;

        public class CarbonPlayer
        {
            private NetworkPlayer networkPlayer;
            public NetworkPlayer getNetworkPlayer => networkPlayer;
            public CarbonPlayer(NetworkPlayer networkPlayer)
            {
                this.networkPlayer = networkPlayer;
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
        }

        public static int addCarbonPlayer()
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
            static bool Prefix(NetworkPlayer networkPlayer, GameServerInitialDetails packet)
            {
                int characterFaceIdentifier = 0;
                PlayerInitialDetails playerInitialDetails = new PlayerInitialDetails
                {
                    Name = HomelessMethods.GenerateRandomWord(10),
                    CharacterVoicePitch = 1f,
                    CharacterFaceIdentifier = characterFaceIdentifier
                };
                instant.serverGameManager.HandlePlayerInitialDetails(playerInitialDetails, networkPlayer);
                AutonomousPlayerInputHandler input = new AutonomousPlayerInputHandler(instant.commonGlobalVariables.BotAutoMove);
                CarbonPlayerRepresentation value = new CarbonPlayerRepresentation
                {
                    playerID = networkPlayer.id,
                    input = input,
                    networkPlayer = networkPlayer
                };
                return false;
            }
        }
    }
}
