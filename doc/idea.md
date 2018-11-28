# 注入区域

## 计划列表

### 客户端MOD

汉化（半成品）

### 服务端MOD

进出日志
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