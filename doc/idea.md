# 注入区域

## 计划列表

### 客户端MOD

汉化（半成品）

### 服务端MOD

统计战队人数
出勤日志
主持人
增强AI

## 服务器日志注入区域

类：ServerRoundPlayerManager
方法：AddInstantiatedPlayer
读取参数：roundPlayerInformation
参数类：RoundPlayerInformation
目标属性：SteamID和InitialDetails.Name

## 管理员公告注入区域

类：ClientAdminBroadcastMessageManager
方法：AdminMessage
读取参数：message
参数类：string

## 全命令列表

类：ServerConsoleCommandsInitializer

## 获取所有服务器的ip地址

类：ClientLobbyManager
方法：UpdateServerLatency

## AI 行动规则

类：ServerCarbonPlayersManager
方法：Update

## 寻路方式

### 整体结构

确定队列坐标
按照距离分配最近的对象填充
确定射线公式，并转向
添加超时时间，如果有障碍物，则横向移动若干个周期
确定全部到位后，按照同一个射线公式修改截距后移动
判断对方对象和己方的最近距离
接近后瞄准三秒后释放
行进装弹，直到抵达目标距离
战损后向军官方向补齐，军官死亡后向中间（优先左边）补齐
返回复活点，等待己方人数大于三人，期间不做任何操作
三人成纵队移动，靠近线列一定距离后散开分配位置

### 需求数学函数

根据起点和射线上一点求射线公式
两点求距离
根据射线求反向延长线上若干个点

## 笔记

SpreadOnMap
将所有机器人分布传送到不同的位置，但似乎只有增减机器人的时候才会调用，可能是为了防止魔法现象
GroupOnMap
将同阵营机器人传送到相近的位置，但似乎只有增减机器人的时候才会调用，可能是为了防止魔法现象
AddCarbonPlayers
增加指定数量的机器人，但似乎是基于协程的异步操作
RemoveCarbonPlayer
移除某一个特定的机器人
RequestingInitialDetailsRPC
似乎是在某处拼接了一个连接数据包，然后将机器人随机取名并虚拟为一个玩家连入游戏，之后或许可以改一下名字生成规则（在另一个类里）
SpawnCarbonPlayer
被上一个函数调用，使机器人出生到地图里
ClientChosenSpawnSettings
生成出生选项，比如阵营等，之后需要改成平均分配
UpdateCarbonPlayerInput
用来根据设定随机进行动作
SayPhrase
喊叫
SayRandomPhrase
全体随机喊叫
TeleportToPlayer
将机器人全部传送到特定玩家附近
SwitchCarbonPlayersWeapon
切换武器 / 随机切换武器
FixedUpdate
修复性刷新
Update
常规刷新
InitializeOnMap
加载地图

Terrain
这个类属于Unity自带的地形类，包括树、石头之类的地形，但是土地和山丘是否包含尚未可知
上面的AI控制类中有关于地图的引用，地图类中有关于地形的数组元素，究竟怎么读取还要反复试验
扫描形成地图之后，就可以使用A*寻路算法了
但是这类扫描必然很费时间，需要加入缓存

Unity3D的渲染循环
void FixedUpdate(){}
这个是对时间高度敏感的，如果需要精确计算时间可以用到
void Update(){}
渲染前的参数设定，所有和输入、游戏参数相关的设置都要放在这里
void LateUpdate(){}
必定在渲染完成后调用，用来做一些渲染后才能进行的操作比如截图

一个避开障碍物的可能用到U3D自身函数的方式
https://blog.csdn.net/qq_27880427/article/details/72781269