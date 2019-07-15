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
        private static ServerComponentReferenceManager instant => ServerComponentReferenceManager.ServerInstance;
        public static bool Load(UnityModManager.ModEntry modEntry)
        {
            logger = modEntry.Logger;
            var harmony = HarmonyInstance.Create(modEntry.Info.Id);
            harmony.PatchAll(Assembly.GetExecutingAssembly());
            Framework.playerSpawnDelegate += Framework_playerSpawnDelegate;
            logger.Log("线列机器人加载完成");
            return true;
        }

        private static void Framework_playerSpawnDelegate(ulong steamId)
        {
            ServerRoundPlayer serverRoundPlayer = 
                instant.serverRoundPlayerManager.ResolveServerRoundPlayer(Framework.steamIdToLocalId[steamId]);
            Vector3 position = serverRoundPlayer.PlayerTransform.position;
            List<int> ids = new List<int>();
            for(int i=0;i<5;i++)
            {
                ids.Add(Framework.addCarbonPlayer("test"));
            }
            ids.ForEach((id) => {
                position.z += 100;
                Framework.getCarbonPlayer(id).teleport(position);
            });
        }
    }
}
