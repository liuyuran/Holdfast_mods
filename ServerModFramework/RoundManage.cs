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
        public static CountDown countDownDelegate = delegate (int roundTime) { };
        public static RoundStart roundStartDelegate = delegate (GameDetails detail) { };
        public static RoundEnd roundEndDelegate = delegate (GameDetails detail) { };

        private static void startTimer()
        {
            System.Timers.Timer timer = new System.Timers.Timer();
            timer.Enabled = true;
            timer.Interval = 1000;
            timer.Start();
            timer.Elapsed += new System.Timers.ElapsedEventHandler(delegate (object source, ElapsedEventArgs e)
            {
                countDownDelegate(getRoundTime());
            });
        }

        [HarmonyPatch(typeof(ServerGameManager), "ChangeGameMode")]
        private static class ChangeGameMode_Patch
        {
            static bool Prefix(GameDetails gameDetails)
            {
                roundStartDelegate(gameDetails);
                return true;
            }

            static void Postfix(GameDetails gameDetails)
            {
                roundEndDelegate(gameDetails);
            }
        }

        private static int getRoundTime()
        {
            return (int)ServerComponentReferenceManager.ServerInstance.serverRoundTimerManager.secondsSinceRoundStarted;
        }
    }
}
