@echo off
setlocal

:: 获取.NET Framework安装目录
for /f "tokens=*" %%i in ('reg query "HKLM\SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full" /v InstallPath') do (
    set "netPath=%%i"
)
set "netPath=%netPath:~76%"

:: 复制InstallUtil.exe到输出目录
copy "%netPath%InstallUtil.exe" "$(TargetDir)"

endlocal
