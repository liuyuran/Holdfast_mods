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

namespace ServerModFramework
{
    public static partial class Framework
    {
        [HarmonyPatch(typeof(ServerCarbonPlayersManager), "AddCarbonPlayers")]
        private static class CarbonAddCarbonPlayer_Patch
        {
            static bool Prefix(int total, float interval, ref bool __result)
            {
                __result = true;
                return false;
            }
        }

        [HarmonyPatch(typeof(ServerCarbonPlayersManager), "RemoveCarbonPlayer")]
        private static class CarbonRemoveCarbonPlayer_Patch
        {
            static bool Prefix(int playerID)
            {
                return false;
            }
        }

        [HarmonyPatch(typeof(ServerCarbonPlayersManager), "Update")]
        private static class CarbonUpdate_Patch
        {
            static bool Prefix()
            {
                return false;
            }
        }

        [HarmonyPatch(typeof(ServerCarbonPlayersManager), "FixedUpdate")]
        private static class CarbonFixedUpdate_Patch
        {
            static bool Prefix()
            {
                return false;
            }
        }

        [HarmonyPatch(typeof(ServerCarbonPlayersManager), "InitializeOnMap")]
        private static class CarbonInitializeOnMap_Patch
        {
            static bool Prefix()
            {
                return false;
            }
        }
    }
}
