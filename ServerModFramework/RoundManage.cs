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
using System.Timers;

namespace ServerModFramework
{
    public delegate void CountDown(int roundTime);
    public delegate void RoundStart(GameDetails detail);
    public delegate void RoundEnd(GameDetails detail);
    public static partial class Framework
    {
        public static CountDown countDownDelegate = null;
        public static RoundStart roundStartDelegate = null;
        public static RoundEnd roundEndDelegate = null;

        private static void startTimer()
        {
            System.Timers.Timer timer = new System.Timers.Timer();
            timer.Enabled = true;
            timer.Interval = 1000;
            timer.Start();
            timer.Elapsed += new System.Timers.ElapsedEventHandler(delegate (object source, ElapsedEventArgs e)
            {
                countDownDelegate?.Invoke(getRoundTime());
            });
        }

        [HarmonyPatch(typeof(ServerGameManager), "ChangeGameMode")]
        private static class ChangeGameMode_Patch
        {
            static bool Prefix(GameDetails gameDetails)
            {
                roundStartDelegate?.Invoke(gameDetails);
                return true;
            }

            static void Postfix(GameDetails gameDetails)
            {
                roundEndDelegate?.Invoke(gameDetails);
            }
        }

        private static int getRoundTime()
        {
            return (int)ServerComponentReferenceManager.ServerInstance.serverRoundTimerManager.secondsSinceRoundStarted;
        }
    }
}
