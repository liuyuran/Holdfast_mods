/**
 * @file BroadcastManage.cs
 * @brief 广播模块文件
 * @details 这个文件包含了公告和管理私聊相关逻辑
 * @author 夏洛特
 * @version 0.1b
 * @date 2019-07-04
 */

using HoldfastGame;

namespace ServerModFramework
{
    public static partial class Framework
    {
        /**
        * @brief 发送管理私聊
        *
        * @param steamId 发送目标的数字id
        * @param message 要发送的信息
        */
        public static void sendMessage(ulong steamId, string message)
        {
            if (!steamIdToLocalId.ContainsKey(steamId)) return;
            ServerComponentReferenceManager.ServerInstance.serverAdminBroadcastMessageManager
                .PrivateMessage(-1, (int) steamIdToLocalId[steamId], message);
        }

        /**
        * @brief 发送公告
        *
        * @param message 要发送的信息
        */
        public static void sendMessage(string message)
        {
            ServerComponentReferenceManager.ServerInstance.serverAdminBroadcastMessageManager.BroadcastAdminMessage(message);
        }
    }
}
