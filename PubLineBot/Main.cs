using Harmony12;
using HoldfastGame;
using System.Reflection;
using uLink;
using UnityModManagerNet;

namespace PubLineBot
{
    public class Main
    {
        private static UnityModManager.ModEntry.ModLogger logger;
        public static bool Load(UnityModManager.ModEntry modEntry)
        {
            logger = modEntry.Logger;
            var harmony = HarmonyInstance.Create(modEntry.Info.Id);
            harmony.PatchAll(Assembly.GetExecutingAssembly());
            logger.Log("服务器基础框架加载完成");
            return true;
        }
    }
}
