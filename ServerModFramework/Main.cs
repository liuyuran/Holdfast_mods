using Harmony12;
using HoldfastGame;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Timers;
using uLink;
using UnityModManagerNet;

namespace ServerModFramework
{
    public delegate void CountDown(int roundTime);
    public delegate void PlayerJoin(ulong steamId);
    public delegate void PlayerLeave(ulong steamId);
    public delegate string AdminMessage(string command);
    public delegate string PlayerCommand(string modName, object[] arguments, ulong steamID, out bool success);
    public delegate string AdminCommand(string modName, object[] arguments, int adminID, out bool success);

    public static partial class Framework
    {

        private static UnityModManager.ModEntry.ModLogger logger;
        public static bool Load(UnityModManager.ModEntry modEntry)
        {
            logger = modEntry.Logger;
            startTimer();
            var harmony = HarmonyInstance.Create(modEntry.Info.Id);
            harmony.PatchAll(Assembly.GetExecutingAssembly());
            logger.Log("服务器基础框架加载完成");
            return true;
        }
    }
}
