using HoldfastGame;

namespace ServerModFramework
{
    public static partial class Framework
    {
        public static void sendMessage(ulong steamId, string message)
        {
            if (!steamIdToLocalId.ContainsKey(steamId)) return;
            ServerComponentReferenceManager.ServerInstance.serverAdminBroadcastMessageManager
                .PrivateMessage(-1, (int) steamIdToLocalId[steamId], message);
        }

        public static void sendMessage(string message)
        {
            ServerComponentReferenceManager.ServerInstance.serverAdminBroadcastMessageManager.BroadcastAdminMessage(message);
        }
    }
}
