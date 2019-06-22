# What's that
A sample holdfast plugin project

# How to compile it
1. Please open the sln file with Visual Studio 2019
2. Set dll path for projects in IDE
3. Run the command below in powershell at the folder has sln file: (you should change paths in command if your path is not same as me.)
```
& 'C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\MSBuild.exe' Serv
erModFramework\ServerModFramework.csproj -tv:3.5 -p:Configuration=Debug
```
4. Copy the target dll file (example: ServerModFramework\bin\Debug\ServerModFramework.dll) to your mod path, enjoy it.

# Where is the translate file?
Well...I'm not sure how to sync it, so please download the release version and get it.
