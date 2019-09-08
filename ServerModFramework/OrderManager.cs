/**
 * @file OrderManage.cs
 * @brief 军官命令模块文件
 * @details 这个文件包含了军官命令相关逻辑
 * @author 夏洛特
 * @version 0.1b
 * @date 2019-08-23
 */

using Harmony12;
using HoldfastGame;
using uLink;

namespace ServerModFramework
{
    public delegate void OfficerOrder(bool isStart, RequestStartOfficerOrderPacket currentRequestPacket);
    public static partial class Framework
    {
        public static event OfficerOrder officerOrderDelegate;
        [HarmonyPatch(typeof(ServerOfficerOrderManager), "BroadcastStartOfficerOrder")]
        private static class OrderManager_BroadcastStartOfficerOrder_Patch
        {
            static void Postfix(RequestStartOfficerOrderPacket currentRequestPacket)
            {
                if (officerOrderDelegate == null) return;
                officerOrderDelegate(true, currentRequestPacket);
            }
        }

        [HarmonyPatch(typeof(ServerOfficerOrderManager), "BroadcastStopOfficerOrder")]
        private static class OrderManager_BroadcastStopOfficerOrder_Patch
        {
            static void Postfix(ActiveOfficerOrderInfo orderToRemove)
            {
                if (officerOrderDelegate == null) return;
                RequestStartOfficerOrderPacket packet = new RequestStartOfficerOrderPacket();
                packet.officerNetworkPlayer = orderToRemove.officerPlayer.NetworkPlayer;
                packet.officerOrderType = orderToRemove.officerOrderType;
                packet.orderPosition = orderToRemove.spawnedPosition;
                packet.orderRotationY = orderToRemove.spawnedRotationY;
                officerOrderDelegate(false, packet);
            }
        }
    }
}
