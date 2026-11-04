# 代码审查报告 · Sedentary Reminder（C# WinForms, .NET Framework 4.8）

审查范围：本次 `git` 工作区全量源码（`Reminder/` 主工程，`Scheduler/` 未纳入主 csproj）。
审查依据：逐文件人工审查 + 576MB 运行日志实证 + 与你既往项目记忆（线程安全/循环健壮性）交叉印证。
说明：静态分析脚本 `analyze.py` 为 Python AST 专用工具，对 C# 不适用，故以人工深度审查替代；以下每条均附 `file:line` 证据与可照抄方案。

---

## 一、Critical / Major（务必先修）

### M1. 后台线程直接 `Dispose()` WinForms 控件（跨线程违规）
- **位置**：`Program.cs` `CleanupExistingForms` (L193–203)、`CleanupWorkForm` (L285–292)。
- **证据**：这两个方法被 `System.Timers.Timer` 的回调（`startTimer`/`stopTimer`/`checkTimer`，L96–98）调用，回调运行于**线程池后台线程**；方法内直接 `form.Dispose()`。而 `CloseAllWorkForms` 已加了 `IsHandleCreated` + `Invoke` 守卫（L353–356），此处却遗漏。
- **根因（推断）**：混淆了 `System.Timers.Timer`（Elapsed 在池线程）与 `System.Windows.Forms.Timer`（Tick 在 UI 线程）。上次修复只补了一半。
- **影响**：WinForms 控件只能在其创建线程（UI 线程）上 Dispose。后台线程 Dispose 会抛 `InvalidOperationException`（Cross-thread）或导致句柄/资源损坏、偶发 `ObjectDisposedException`。在高概率的「整点自动停止（12/18 点）」与「分钟级启动检查」路径上触发。
- **正确方案**：与已修的 `CloseAllWorkForms` 保持一致，强制 marshal 到 UI 线程：
  ```csharp
  private static void CleanupExistingForms()
  {
      foreach (Form form in Application.OpenForms)
      {
          if (form is WorkFrm || form is RestFrm)
          {
              var f = form;
              string name = f.Name;            // 先取名字，避免后台线程碰控件
              if (f.IsHandleCreated)
                  f.Invoke((Action)(() => f.Dispose()));
              else
                  f.Dispose();
              Logger.Log($"已关闭：{name}--{DateTime.Now}");
          }
      }
  }
  ```
  `CleanupWorkForm` 同理用 `activeWorkFrm.IsHandleCreated ? Invoke : Dispose`。

### M2. `WorkFrm.LogException` 绕过 `Logger` 的新锁（并发写日志）
- **位置**：`WorkFrm.cs` `LogException` (L521–527)，调用点 `Timer1_Tick` catch (L389) 与两处构造函数 catch。
- **证据**：`Logger.Log` 已加 `_logLock`（L21–24）写同一 `log.txt`；而 `LogException` 直接 `File.AppendAllText(logPath, ...)` 无锁。
- **根因**：上次加锁修复时遗留了重复实现，未统一收口。
- **影响**：UI 线程（Timer 异常）与后台线程（`Program` 的 `Logger.Log`）并发写同一文件 → `IOException` / 行交错损坏。等于把“576MB 日志刷屏”的根因**只修了一半**。
- **正确方案**：删除 `LogException`，全量走 `Logger.Log`：
  ```csharp
  // WorkFrm.cs 删除 LogException 方法；catch 内改为：
  catch (Exception ex) { Logger.Log($"[ERR] Timer error: {ex.Message}\n{ex.StackTrace}"); timerWrk.Stop(); }
  ```

### M3. `isRunning` / `currentInstance` 被“先关后建”竞态破坏（单例跟踪失真）
- **位置**：`WorkFrm.cs` `OnClosed` (L190–195)、静态字段 `isRunning`/`currentInstance` (L18–19)；触发点 `MainFrm.Btn_start_Click` (L164–176)、`First_Start` (L134–148)、`button1_Click` +10 分钟 (L495–501)。
- **证据**：`OnClosed` 无条件 `isRunning=false`，且**从不重置 `currentInstance`**。`Close()` 对 modeless 窗体不保证同步触发 `OnClosed`；当“关闭旧 WorkFrm → 立即 `new WorkFrm()`（构造里 `isRunning=true; currentInstance=this`）”时，若旧窗 `OnClosed` 延后执行，会把 `isRunning` 翻回 `false`。
- **根因**：用静态单例字段 + 异步 `OnClosed` 做会话跟踪，且 close 是延迟的。
- **影响**：① 托盘状态 `UpdateTrayStatus`（L282）在“正在工作”时误显示“未开始”；② `WorkFrm.IsRunning()` 返回 false → 用户再点“开始”会**再建一个 WorkFrm → 两个工作倒计时并存**；③ `currentInstance` 永不清除，可能悬空指向已关闭实例。
- **正确方案**：关闭时仅当“关闭的就是当前跟踪实例”才清除，并从 `currentInstance` 推导 `isRunning`：
  ```csharp
  protected override void OnClosed(EventArgs e)
  {
      base.OnClosed(e);
      if (currentInstance == this) currentInstance = null;
      isRunning = (currentInstance != null);
  }
  ```
  并对 `SetHidden`/`RemainingTime` 增加 `if (this.IsDisposed) return;` 防御。

### M4. `KeyboardBlocker` 方法命名反义 + 系统级 `BlockInput` 安全隐患
- **位置**：`KeyboardBlocker.cs` `off()` (L22–31)=`BlockInput(true)`（**锁定**输入）、`on()` (L32–41)=`BlockInput(false)`（**解锁**）。
- **证据**：方法名与行为完全相反。`RestFrm.SetupUI` (L66) `if(input_flag) KeyboardBlocker.off()` 实际是**锁定**；`RestFrm_FormClosing` (L401) `if(input_flag) KeyboardBlocker.on()` 实际是**解锁**。
- **根因**：命名时语义写反，后续调用者只能“反向记忆”，极易再踩坑。
- **影响（安全/可用）**：`BlockInput` 是**系统级全局**锁。若启用 `input_flag` 且进程在休息期间被强杀（崩溃/任务管理器），输入可能全局锁死直到再次 `BlockInput(false)` 或重启——这是真实的安全/可用性风险。此外 `IsAdministrator()` 为 false 时 `off()` 静默返回，锁功能**悄悄失效**。
- **正确方案**：
  1. 重命名为 `Block()` / `Unblock()`（或 `EnableInputBlocking()`/`DisableInputBlocking()`），并加注释说明全局锁风险；
  2. 在进程退出路径（`Program` 退出/`AppDomain` 卸载）与异常兜底里**确保 `BlockInput(false)`**；
  3. 非管理员时给出明确提示而非静默 no-op。

### M5. 首个工作会话忽略 `LockInput` 设置（行为不一致）
- **位置**：`MainFrm.First_Start` (L145)、`Program.CreateAndShowWorkForm` (L266)、`Program` 自动启动链路 → 均用 **2 参** `WorkFrm(wrkTime, rstTime)` → `input_flag=false`。只有 `RestFrm.StartNextWorkForm` (L336) 才从配置读 `LockInput`。
- **根因**：两个构造函数默认不一致，Program/MainFrm 首启动未传入持久化偏好。
- **影响**：每次启动后的**第一轮** 45 分钟永不锁键盘，第二轮起才锁 → 用户“锁键盘”偏好在第一段被忽略。
- **正确方案**：统一入口读取 `LockInput`（如新增 `Config.GetLockInput()`），所有 `WorkFrm` 构造都传入；或让 `WorkFrm` 构造内自行从配置读 `LockInput`，消除 2/3 参差异。

### M6. 双 STA 消息泵 + 回退路径返回 null（架构缺陷）
- **位置**：`Program.CreateAndShowWorkForm` 回退分支 (L271–281)。
- **证据**：当 `targetForm` 为 null/句柄未创建时，启动**新 STA 线程 `Application.Run(form)`**——与主线 `Application.Run(mainForm)` 形成**两个 UI 消息泵**；且 `form` 在新线程内赋值，方法返回时 `form` 仍为 `null` → `TryCreateWorkForm` 使 `activeWorkFrm=null`，`SetupFormClosedHandlers` 不挂接 → `activeRestFrm` 永不跟踪 → `CheckTimerStatus`/`CloseAllWorkForms` 误判“无会话”→ 可重复启动/无法停止。
- **根因**：试图在后台线程创建 UI；WinForms 正确做法是始终 `Invoke` 到已有 UI 线程。
- **正确方案**：主线程已常驻 `Application.Run(mainForm)`，永远走 `Invoke` 分支；**删除第二个 `Application.Run` 回退**，必要时 `mainForm.Invoke` 并等待窗体创建后再返回。

---

## 二、Minor / Maintainability（建议修）

- **m7 `WorkFrm.Timer1_Tick` 时间边界**（L374）：归零判断在 `wrk_minutes<0` 之后，会闪现 `-1:59` 且总时长多 1 秒。建议恰好在 `wrk_minutes==0 && wrk_seconds==0` 结束，不要减到负数。`Warn()` 也仅在分钟 0 闪一次，可一并校正。
- **m8 `RestFrm.GetConfigValue("AutoStopHours")` 用 `int.Parse`**（L284–287）：非数字 token 会抛异常（虽被 catch 兜底，但 `Program.GetConfigValues` 已做安全解析，应复用保持一致）。
- **m9 `MainFrm` 重复菜单处理**：`MainMenuItemClick` (L181) 与 `主窗体ToolStripMenuItem_Click` (L190) 完全相同，后者疑为未删除的 Designer 死处理，应移除。
- **m10 `ReminderService.cs` 是已编译但 `Program.Main` 永不调用的死代码**（Program 注释明确“服务不适合，Session 0 隔离”）。它用 `schtasks` 双触发逻辑与 Program 重复，且 `DateTime.Parse("09:00")` 文化敏感、`now.Second==0` 易漏触发。建议删除 `ReminderService.cs`/`ProjectInstaller.cs` 及 `Scheduler/*`（若确未使用），避免混淆。
- **m11 `System.Timers.Timer` 与 `RestartTimerIfStopped`**：`AutoReset` 默认 true，计时器不会自停，`CheckTimerStatus` 里的重启逻辑实际是死代码；且 `checkTimer`（每小时）与分钟级启停计时器重复。可简化。
- **m12 单实例 `Mutex` 提前释放**：`mutex.ReleaseMutex()` (L65) 在 `InitializeApplication` 之后立即释放，进程运行期间不再持有全局锁。若真要“全局单实例”，应持有到进程退出；当前仅防“启动瞬间重叠”，后续再启动一份会被允许（低风险）。

---

## 三、本次修复遗留/关联提醒
- 上次（10/30 及本次）的循环修复**做得对**，但 M1、M2 正是“只修了一半”的同类问题：锁与跨线程守卫必须覆盖**所有**写同一个文件 / 在非 UI 线程触碰控件的地方，否则症状会以别的形式复发（与项目记忆中“System.Timers.Timer 与 UI 线程并发写”“不要跨线程 Dispose 控件”的警示一致）。

---

## 四、最该先修什么（优先级）
1. **P0（立即）**：M1 跨线程 Dispose、M2 LogException 绕过锁、M3 单例跟踪失真（三者都会导致间歇性、难复现的崩溃/双会话，且直接对应历史日志里的并发异常）。
2. **P1（尽快）**：M4 键盘锁命名与系统级安全、M5 首轮锁偏好、M6 双消息泵。
3. **P2（择机）**：m7–m12。

> 以上均为代码层结论；本机无 MSBuild/dotnet 无法编译 .NET Framework 4.8，所有改动需在 Visual Studio 重新生成并实机验证（尤其 M1/M3 的双屏 + 整点停止 + 点击“开始”路径）。改完发我，我帮你复审确认。
