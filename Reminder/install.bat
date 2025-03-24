@echo off
setlocal

:: 获取当前脚本所在目录
set "currentDir=%~dp0"

:: 检查是否以管理员身份运行
net session >nul 2>&1
if %errorLevel% == 0 (
    echo 正在安装服务...
    "%currentDir%InstallUtil.exe" "%currentDir%Reminder.exe"
    echo 安装完成
    pause
) else (
    echo 请以管理员身份运行此脚本
    pause
    exit /b 1
)

endlocal
