/**
 * @file RoundManage.cs
 * @brief 战场模块文件
 * @details 这个文件包含了计时器和切换地图相关逻辑
 * @author 夏洛特
 * @version 0.1b
 * @date 2019-07-04
 */

using Harmony12;
using HoldfastGame;
using System;
using System.Timers;

namespace ServerModFramework
{
    public delegate void CountDown(int roundTime);
    public delegate void RoundStart(GameDetails detail);
    public delegate void RoundEnd(GameDetails detail);
    public static partial class Framework
    {
        /// 时间监听器
        public static event CountDown countDownDelegate;
        /// 地图切换完成监听器
        public static event RoundStart roundStartDelegate;
        /// 地图开始切换监听器
        public static event RoundEnd roundEndDelegate;

        private static void startTimer()
        {
            System.Timers.Timer timer = new System.Timers.Timer();
            timer.Enabled = true;
            timer.Interval = 1000;
            timer.Start();
            timer.Elapsed += new ElapsedEventHandler(delegate (object source, ElapsedEventArgs e)
            {
                countDownDelegate(getRoundTime());
            });
        }

        [HarmonyPatch(typeof(ServerGameManager), "ChangeGameMode", new Type[] { typeof(GameDetails)})]
        private static class ChangeGameMode_Patch
        {
            static bool Prefix(GameDetails gameDetails)
            {
                if (gameDetails != null && roundStartDelegate != null) roundStartDelegate(gameDetails);
                return true;
            }

            static void Postfix(GameDetails gameDetails)
            {
                if (gameDetails != null && roundEndDelegate != null) roundEndDelegate(gameDetails);
                    logger.Log("detail test:" + gameDetails.MaxPlayerRespawns);
            }
        }

        private static int getRoundTime()
        {
            return (int)ServerComponentReferenceManager.ServerInstance.serverRoundTimerManager.secondsSinceRoundStarted;
        }
    }
}
