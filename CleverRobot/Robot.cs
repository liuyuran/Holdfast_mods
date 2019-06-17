using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CleverRobot
{
    enum OrderType
    {
        Veer = 0, // 转向
        MOVE = 1, // 移动
        PRESENT = 2, // 举枪
        FIRE = 3, // 瞄准
        WAIT = 4 // 射击
    }

    class Order
    {
        public OrderType type;
        public Matrix matrix;
    }

    class Matrix : IComparable
    {
        public float x;
        public float y;

        public bool Equals(Matrix mat)
        {
            return (x == mat.x) && (y == mat.y);
        }

        public int CompareTo(object obj)
        {
            Matrix mat = (Matrix)obj;
            return ((x == mat.x) && (y == mat.y)) ? 0 : 1;
        }
    }

    class Robot
    {
        public Matrix position;
        public Matrix direction;
        public Queue<Order> orders = new Queue<Order>();
    }

    class RobotManager
    {
        public List<Robot> robotList;

        public static int splitDistance = 5; // 每个单位的中心点相隔5个单位距离

        public RobotManager()
        {
            robotList = new List<Robot>();
        }
        /** 增加一个实体对象 */
        public void addRobot(float x, float y)
        {
            // TODO 第一个必须是领头的
            Robot robot = new Robot();
            robot.position = new Matrix();
            robot.position.x = x;
            robot.position.y = y;
            robot.direction = new Matrix();
            robot.direction.x = x;
            robot.direction.y = y;
            robotList.Add(robot);
        }

        public int Count() {
            return robotList.Count;
        }

        /** 更新位置 */
        public void updatePosition(int index, float x, float y)
        {
            robotList[index].position.x = x;
            robotList[index].position.y = y;
        }

        /** 判定多少人进入线列 */
        public bool checkInline(int min)
        {
            int count = 0;
            for (int i = 1; i < robotList.Count; i++)
            {
                if (robotList[i].position.CompareTo(robotList[i].direction) == 0) count++;
            }
            return min <= count;
        }

        /** 获取本次渲染循环的命令列表 */
        public List<Order> getOrder()
        {
            Order waitOrder = new Order();
            waitOrder.type = OrderType.WAIT;
            Matrix matrix = new Matrix();
            matrix.x = 0;
            matrix.y = 0;
            waitOrder.matrix = matrix;
            List<Order> orders = new List<Order>();
            foreach (Robot robot in robotList)
            {
                if (robot.orders.Count == 0) orders.Add(waitOrder);
                else orders.Add(robot.orders.Dequeue());
            }
            return orders;
        }

        /** 根据军官位置向任意方向集合为线列 */
        public void mass()
        {
            if (robotList.Count < 2) return;
            Robot leader = robotList[0];
            Matrix position = leader.position;
            Matrix direction = leader.direction;
            for (int i = 1; i < robotList.Count; i++)
            {
                Matrix matrix = new Matrix();
                double k = (position.y - direction.y) / (position.x - direction.x);
                double zoom = Math.Sqrt(k * k + 1);
                matrix.x = position.x + Convert.ToInt32(k * splitDistance / zoom);
                matrix.y = position.y + Convert.ToInt32(splitDistance / zoom);
                robotList[i].direction = matrix;
            }
        }

        /** 前进 */
        public void moveForward() { }

        /** 后退 */
        public void moveBackward() { }

        /** 原地横队转向 */
        public void rotate(Matrix direction) {
            if (robotList.Count < 2) return;
            robotList[0].direction = direction;
            mass();
        }
    }
}
