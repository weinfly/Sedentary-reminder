# 久坐提醒（Sedentary-reminder）代码评审全部修复 — 完成概述

老李，本轮已按 `code-review-report.md` 把 12 项问题全部修复，并本地提交（`a427b65`，未 push）。

## 修复清单（M1–M6 主 + m7–m12 次）

| 编号 | 文件 | 问题 | 修复要点 |
|---|---|---|---|
| M1 | Program.cs | 后台线程直接 `Dispose()` 控件（跨线程违规） | 新增 `SafeDispose()`：`IsHandleCreated` 时 `Invoke` 释放，否则直接释放；`CleanupExistingForms`/`CleanupWorkForm` 改用之 |
| M2 | WorkFrm.cs | `LogException` 绕过 `Logger` 的锁，并发写 `log.txt` | 改为调用 `Logger.Log(...)`（带锁） |
| M3 | WorkFrm.cs | `OnClosed` 无条件清 `isRunning`，“先关后建”竞态 → 双会话 | `currentInstance==this` 才清空，`isRunning=(currentInstance!=null)` |
| M4 | KeyboardBlocker.cs / RestFrm.cs / MainFrm.cs | `off()/on()` 命名反义；系统级 `BlockInput` 强杀后锁死风险 | 更名 `Block()/Unblock()`；退出前 `Unblock()`；非管理员仅告警 |
| M5 | MainFrm.cs | 首轮 `WorkFrm` 用 2 参构造忽略 `LockInput` | `First_Start` 改用 3 参 `(wrkTime,rstTime,ckBoxInput.Checked)` |
| M6 | Program.cs | 回退分支双 STA `Application.Run` + 返回 null | 移除第二次 `Application.Run`，仅在 UI 线程 `Invoke` 创建 |
| m7 | WorkFrm.cs | 倒计时闪 `-1:59` | 归零（00:00）立即停表并建休息窗 |
| m8 | RestFrm.cs | `AutoStopHours` 用 `int.Parse` 不一致 | 改 `int.TryParse` + 过滤负数 |
| m9 | MainFrm.cs | 菜单处理重复实现 | `主窗体ToolStripMenuItem_Click` 委托 `MainMenuItemClick` |
| m10 | csproj + git | `ReminderService`/`ProjectInstaller` 死代码 | 移出 csproj 并 `git rm --cached` 取消跟踪 |
| m11 | Program.cs | `RestartTimerIfStopped` 永不触发的死代码 | 删除 |
| m12 | Program.cs | `mutex` 在 `finally` 提前释放 → 非启动瞬间多实例 | 持有互斥量至进程退出 |

## 重要环境事项（请注意）

- 本工程位于 **OneDrive 同步目录**。本会话 `git rm`/`git checkout` 等触碰磁盘的 git 操作，
  曾导致整个 `Reminder/` 目录被 OneDrive 从本地驱逐、工作树全损（已发生两次，前序会话一次）。
- 本次规避方式：源码改动用直接文件写；删除死代码用 `git rm --cached`（仅改索引、不碰磁盘）。
- **建议**：把本工程目录从 OneDrive 同步中排除，或对其设置“始终保留在此设备”，可彻底消除该风险。
- 物理上 `ReminderService.cs`/`ProjectInstaller.cs` 仍留在磁盘且为未跟踪状态（未用 `git rm` 删除），
  你可在确认后手动删除，或我另行处理。

## 待你确认/执行

1. **编译验证**：本机无 MSBuild，请在 Visual Studio 重新生成；并实跑 2–3 轮（单屏+双屏），
   重点验证休息结束提示弹出瞬间按 Alt+F4 不再卡死。
2. **是否 push**：当前仅本地提交 `a427b65`，按既往工作流需你确认后再 push。
