# 注入区域

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

## 歌曲名翻译

类：InstrumentSongOptionsRepository
可疑方法A：ResolveSongs
可疑方法B：ResolveRandomSong
可疑方法C：ResolveSong

## 全命令列表

类：ServerConsoleCommandsInitializer

## 获取所有服务器的ip地址

类：ClientLobbyManager
方法：UpdateServerLatency