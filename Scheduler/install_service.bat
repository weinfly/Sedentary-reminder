@echo off
chcp 65001 >nul
echo ============================================
echo   久坐提醒调度服务 - 安装/卸载工具
echo ============================================
echo.

set SERVICE_NAME=SedentaryReminderScheduler
set EXE_PATH=%~dp0SedentaryReminder.Scheduler.exe
set FRAMEWORK=%WINDIR%\Microsoft.NET\Framework\v4.0.30319\installutil.exe

:menu
echo 请选择操作：
echo   1. 安装服务
echo   2. 卸载服务
echo   3. 启动服务
echo   4. 停止服务
echo   5. 查看服务状态
echo   6. 退出
echo.
set /p choice=请输入选项 (1-6): 

if "%choice%"=="1" goto install
if "%choice%"=="2" goto uninstall
if "%choice%"=="3" goto start
if "%choice%"=="4" goto stop
if "%choice%"=="5" goto status
if "%choice%"=="6" goto end

echo 无效选项，请重新输入！
goto menu

:install
echo.
echo 正在安装服务...
%FRAMEWORK% %EXE_PATH%
if %errorlevel% equ 0 (
    echo 服务安装成功！
) else (
    echo 服务安装失败！请以管理员身份运行此脚本。
)
pause
goto menu

:uninstall
echo.
echo 正在卸载服务...
%FRAMEWORK% /u %EXE_PATH%
if %errorlevel% equ 0 (
    echo 服务卸载成功！
) else (
    echo 服务卸载失败！
)
pause
goto menu

:start
echo.
echo 正在启动服务...
net start %SERVICE_NAME%
if %errorlevel% equ 0 (
    echo 服务启动成功！
) else (
    echo 服务启动失败！
)
pause
goto menu

:stop
echo.
echo 正在停止服务...
net stop %SERVICE_NAME%
if %errorlevel% equ 0 (
    echo 服务停止成功！
) else (
    echo 服务停止失败！
)
pause
goto menu

:status
echo.
echo 服务状态：
sc query %SERVICE_NAME%
pause
goto menu

:end
echo.
echo 再见！
exit /b 0