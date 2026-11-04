# 复审 v2 修复落地概览（Major + Minor）

> 基于 `review-report-v2.md`，按指示实现全部 Major（M-1/M-2）与 Minor（m-1~m-8）。
> 提交：`a4bddd5`（代码/文档修复）+ `c6934ff`（纳入复审报告）。
> 本机无 MSBuild，需在 Visual Studio 重新生成验证；尚未 push（按既往工作流待你确认）。

## Major

### M-1 输入锁安全（默认仅锁前 2 分钟，按你的要求）
- `KeyboardBlocker` 重写：
  - `Block(int holdSeconds = 120)` —— **默认只锁定前 2 分钟（120 秒）**，到点由线程池 Timer 触发 `ForceUnlock()` 自动解锁，杜绝“进程崩溃/被强杀 → 整机输入永久锁死”。
  - **引用计数**配对 `Block/Unblock`：多显示器下每个休息窗各计一次，仅最后一个窗体释放时才真正解锁（解决 m-4 多屏解锁不一致）。
  - 非管理员仍仅告警、不实际锁定；遮罩 `lblHint` 增加「Ctrl+Alt+Delete 紧急解锁」提示文案。
- 文件：`KeyboardBlocker.cs`、`RestFrm.cs`（调用处 + 提示）。

### M-2 配置持久化
- `MainFrm.SetConfigValue` / `Program.GetAppConfiguration` / `ThemeManager.SaveMode` 一律改用 `ConfigurationUserLevel.PerUserRoaming`（写入用户 AppData 的 `user.config`，可写），消除安装在 `C:\Program Files` 或受限账户下“设置改完即丢”。
- 保存失败由原先的 `Console.WriteLine` / 空 catch 改为 `Logger.Log` 记录，便于排查。

## Minor
- **m-1** 删除 `WorkFrm` 两参死构造器（grep 确认全仓已无 `new WorkFrm(a,b)` 调用）。
- **m-2** README 标注 `WorkPeriods` 暂未实现，仅 `AutoStartHours/AutoStopHours` 生效（消除文档超前于代码）。
- **m-3** `RestFrm.GetConfigValue` 增加 `WorkTimeValue/RestTimeValue >0` 校验，防误配 0 触发“工作0秒→休息→再0秒”死循环。
- **m-4**（随 M-1 引用计数一并解决）多屏解锁一致性。
- **m-5** `WorkFrm.Warn` 仅首次进入提醒态居中一次（`_warnedOnce`），之后维持用户拖拽/多屏位置，不再每帧跳回主屏。
- **m-6** `Logger.Log` 改为按日期分文件（`logs/log-YYYYMMDD.txt`）+ 单文件超 1MB 归档为 `.old.txt`，防 `log.txt` 无限膨胀（早期曾达 576MB）。
- **m-7** 清理 `KeyboardBlocker` 未使用的特权常量/结构体死代码。
- **m-8** 清理 `MainFrm` 开机启动相关代码异味（未用 `bool res`、冗余 `string tmpStr = null`）。

## 验证重点（请在 VS 重新生成后实跑）
1. **M-1 输入锁**：以管理员运行，开启输入锁定，确认休息**前 2 分钟**键盘鼠标被锁、2 分钟后自动恢复；多屏时任一屏 Alt+F4 不影响其余屏锁定，全部关闭后才解锁；进程强杀后输入在 2 分钟内自动恢复（不再锁死整机）。
2. **M-2 配置**：将程序安装到 `C:\Program Files\...` 或以受限账户运行，修改工作时长/休息时长/锁定/主题后重开，确认设置被保留。
3. 全链路单屏 + 双屏各跑 2~3 轮，重点复测“休息结束提示弹出瞬间按 Alt+F4”不再卡死（上轮 M3 修复的关键验收点）。

## 待你确认
- 当前仅本地提交，**未 push**。是否需要我推送到 `origin/master`？
- 磁盘上 `Reminder/ProjectInstaller.cs`、`ReminderService.cs` 仍为未跟踪死文件（上轮 m10 刻意留盘规避 OneDrive 灾难），可在排除 OneDrive 同步后清理。
