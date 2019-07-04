using HoldfastGame;
using System.Timers;

namespace ServerModFramework
{
    public static partial class Framework
    {
        public static CountDown countDownDelegate = delegate (int roundTime) { };

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

        private static int getRoundTime()
        {
            return (int)ServerComponentReferenceManager.ServerInstance.serverRoundTimerManager.secondsSinceRoundStarted;
        }
    }
}
