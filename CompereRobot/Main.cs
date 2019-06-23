using ServerModFramework;
using UnityModManagerNet;
using Harmony12;
using System.Reflection;

namespace CompereRobot
{
    public class Main
    {
        private static UnityModManager.ModEntry.ModLogger logger;
        public static bool Load(UnityModManager.ModEntry modEntry)
        {
            logger = modEntry.Logger;
            var harmony = HarmonyInstance.Create(modEntry.Info.Id);
            harmony.PatchAll(Assembly.GetExecutingAssembly());
            Framework.countDownDelegate += timelineLoop;
            Framework.adminCommandDelegate += serverCommand;
            logger.Log("虚拟主持人加载完成");
            return true;
        }

        private static string serverCommand(object[] arguments, int adminID, out bool success)
        {
            success = true;
            return "运行正常";
        }

        private static void timelineLoop(int time)
        {
            //
        }
    }
}
