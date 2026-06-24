@echo off
chcp 65001 >nul
echo ============================================
echo   Sedentary Reminder - 开机自启动安装
echo ============================================
echo.

REM 获取当前用户
set CURRENT_USER=%USERNAME%

REM 获取程序路径
set EXE_PATH="%~dp0Reminder.exe"
set SHORTCUT_NAME=Sedentary Reminder

echo [1/3] 正在创建开机自启动项...

REM 创建启动文件夹中的快捷方式
powershell -Command ^
    "$WshShell = New-Object -ComObject WScript.Shell; ^
     $Shortcut = $WshShell.CreateShortcut([Environment]::GetFolderPath('Startup') + '\%SHORTCUT_NAME%.lnk'); ^
     $Shortcut.TargetPath = '%~dp0Reminder.exe'; ^
     $Shortcut.WorkingDirectory = '%~dp0'; ^
     $Shortcut.Description = 'Sedentary Reminder - 久坐提醒程序'; ^
     $Shortcut.Save()"

if %ERRORLEVEL% EQU 0 (
    echo     [成功] 开机自启动项已创建
) else (
    echo     [失败] 创建开机自启动项失败，请检查权限
    pause
    exit /b 1
)

echo.
echo [2/3] 正在添加到注册表启动项...

reg add "HKCU\Software\Microsoft\Windows\CurrentVersion\Run" /v "%SHORTCUT_NAME%" /t REG_SZ /d %EXE_PATH% /f

if %ERRORLEVEL% EQU 0 (
    echo     [成功] 注册表启动项已添加
) else (
    echo     [失败] 添加注册表启动项失败，请检查权限
    pause
    exit /b 1
)

echo.
echo [3/3] 正在启动程序...

start "" %EXE_PATH%

if %ERRORLEVEL% EQU 0 (
    echo     [成功] 程序已启动，请查看右下角系统托盘
) else (
    echo     [失败] 程序启动失败
)

echo.
echo ============================================
echo   安装完成！
echo   程序将在每次登录时自动启动
echo   如需卸载，请运行 uninstall_autostart.bat
echo ============================================
echo.
pause