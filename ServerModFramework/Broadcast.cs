using HoldfastGame;

namespace ServerModFramework
{
    public static partial class Framework
    {
        public static void sendMessage(int steamId, string message)
        {
            ServerComponentReferenceManager.ServerInstance.serverAdminBroadcastMessageManager.PrivateMessage(-1, steamId, message);
        }

        public static void sendMessage(string message)
        {
            ServerComponentReferenceManager.ServerInstance.serverAdminBroadcastMessageManager.BroadcastAdminMessage(message);
        }
    }
}
