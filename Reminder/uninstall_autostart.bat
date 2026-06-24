@echo off
chcp 65001 >nul
echo ============================================
echo   Sedentary Reminder - 卸载开机自启动
echo ============================================
echo.

set SHORTCUT_NAME=Sedentary Reminder

echo [1/2] 正在删除注册表启动项...

reg delete "HKCU\Software\Microsoft\Windows\CurrentVersion\Run" /v "%SHORTCUT_NAME%" /f

if %ERRORLEVEL% EQU 0 (
    echo     [成功] 注册表启动项已删除
) else (
    echo     [警告] 注册表启动项不存在或删除失败
)

echo.
echo [2/2] 正在删除启动文件夹中的快捷方式...

powershell -Command ^
    "$shortcutPath = [Environment]::GetFolderPath('Startup') + '\%SHORTCUT_NAME%.lnk'; ^
     if (Test-Path $shortcutPath) { Remove-Item $shortcutPath; Write-Host '     [成功] 快捷方式已删除' } else { Write-Host '     [警告] 快捷方式不存在' }"

echo.
echo ============================================
echo   卸载完成！
echo ============================================
echo.
pause