# What's that 这是啥
A sample holdfast plugin project.
一个HF的插件工程。

# How to compile it 如何编译它
1. Please open the sln file with Visual Studio 2019. 打开VS2019，社区版就够了。
2. Set dll path for projects in IDE. 根据你的安装路径，重新指定那些我依赖的dll文件的路径。
3. Run the command below in powershell at the folder has sln file: (you should change paths in command if your path is not same as me.) 在powershell里执行这个命令，不同系统不同安装习惯可能路径有所不同，但我相信一个入门的C#开发者不可能不会修改这个命令里的路径。
```
& 'C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\MSBuild.exe' Serv
erModFramework\ServerModFramework.csproj -tv:3.5 -p:Configuration=Debug
```
4. Copy the target dll file (example: ServerModFramework\bin\Debug\ServerModFramework.dll) to your mod path, enjoy it. 把生成出的dll文件扔进mod目录下，覆盖掉源文件，如果你写了一个新的mod，仿写一个mod路径，并编辑mods文件夹下的xml文件启用它即可。

# Where is the translate file? 文本文件呢？
Well...I'm not sure how to sync it, so please download the release version and get it.
你不会指望我把一个每周都会更新的玩意放到git里吧……去release界面自己下载自己改良去。


# Plugin list 插件列表
TranslateCN
client plugin, used by improve language pack
客户端插件，用于改良汉化

ServerModFramework
server plugin, used by offer some method to develop other plugins
服务端插件，是其他插件的中间层，用于隔离游戏内部代码和插件代码

CompereRobot
example server plugin
一个我写着玩的简单服务器插件示例