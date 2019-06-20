using Harmony12;
using HoldfastGame;
using System.Reflection;
using UnityModManagerNet;

namespace ServerModFramework
{
    public class Framework
    {
        public static UnityModManager.ModEntry.ModLogger logger;
        public static bool Load(UnityModManager.ModEntry modEntry)
        {
            logger = modEntry.Logger;
            var harmony = HarmonyInstance.Create(modEntry.Info.Id);
            harmony.PatchAll(Assembly.GetExecutingAssembly());
            logger.Log("先生们女士们加载完成");
            return true;
        }
    }
}
