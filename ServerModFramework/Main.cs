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
