/**
 * @file main.cs
 * @brief 插件入口文件
 * @details 这个文件是框架插件的总入口，负责加载注入功能
 * @author 夏洛特
 * @version 0.1b
 * @date 2019-07-04
 */

using Harmony12;
using System.Reflection;
using UnityModManagerNet;

namespace ServerModFramework
{
    /**
    * @brief 插件主类
    * 
    * 该类作为所有衍生插件的基础设施，应当托管绝大部分的信息交互
    */
    public static partial class Framework
    {
        private static UnityModManager.ModEntry.ModLogger logger;
        /**
        * @brief 入口函数，不要调用，除非你知道你在做什么
        *
        * @param modEntry mod基础信息
        * @return 是否启动成功
        */
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
