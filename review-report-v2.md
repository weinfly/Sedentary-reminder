# 代码审查报告（复审 v2）

## 审查概览
- 审查目标：`Reminder/`（久坐提醒 WinForms 客户端，.NET Framework 4.8）
- 审查范围：全量（基于 `a427b65` / `316e593` 最新提交态），并对照上一轮 `code-review-report.md` 的 12 项修复做回归确认
- 技术栈：C# / WinForms / .NET Framework 4.8 / app.config / user32 BlockInput
- 总体评分：7.5 / 10（相较上一轮 5.5 明显提升；核心循环健壮性已修复，但残留 2 项 Major 与新增强项建议）
- 合并建议：⚠️ 修改后可合并（2 项 Major 建议在合入前处理；UI/功能建议可排期）
- 静态分析：专家自带 `analyze.py` 为 Python AST 专用分析器，对 C# 不适用（未产生命中），以下结论均为人工逐文件审查所得，均有代码/行号依据。

---

## 风险摘要
最需优先处理的两类风险：
1. **输入锁定的安全性**：`KeyboardBlocker.Block()` 通过 `user32!BlockInput` 进行**整机级**鼠标/键盘锁定，且在 `RestFrm` 中默认锁**整个休息时段**（默认 15 分钟）。若程序在锁定期间崩溃、挂起或被强杀，系统输入将持续锁定直至重启——这是一个能让用户“被锁在机器外”的高危设计，必须加逃生通道与自动解锁。
2. **配置持久化在 Program Files 下失效**：`SetConfigValue` / `GetAppConfiguration` 均使用 `ConfigurationUserLevel.None`，配置写入 `Reminder.exe.config`（位于程序目录）。当用户把程序放进 `C:\Program Files\...` 或以受限账户运行时，写入被拒绝且异常被静默吞掉，**所有设置（工作时长/休息时长/锁定/隐藏/主题）都不会保存**——这是多数“安装即失效”投诉的根因。

---

## 回归确认（上一轮 12 项修复，已逐项核对代码，无回归）

| 项 | 状态 | 行号依据 |
|----|------|----------|
| M1 后台线程跨线程 Dispose | ✅ 已修复 | `Program.SafeDispose` L206、`CleanupExistingForms` L188、`CloseAllWorkForms` L355 |
| M2 LogException 绕过日志锁 | ✅ 已修复 | `WorkFrm.LogException` L530 改调 `Logger.Log` |
| M3 OnClosed 竞态双会话 | ✅ 已修复 | `WorkFrm.OnClosed` L190，`currentInstance` 守护 |
| M4 命名反义 + 退出解锁 | ✅ 已修复 | `KeyboardBlocker` 更名 `Block/Unblock`；`MainFrm.退出` L209 `Unblock()` |
| M5 首轮忽略 LockInput | ✅ 已修复 | `MainFrm.First_Start` L145 三参构造 |
| M6 双 STA 消息泵 | ✅ 已修复 | `Program.CreateAndShowWorkForm` L273 仅 `Invoke` 创建 |
| m7 -1:59 闪现 | ✅ 已修复 | `WorkFrm.Timer1_Tick` L367 归零即停表建休息窗 |
| m8 int.Parse 脏配置 | ✅ 已修复 | `RestFrm.ShowRestEndMessage` L284 `TryParse` |
| m9 重复菜单处理 | ✅ 已修复 | `主窗体ToolStripMenuItem_Click` L190 委托 `MainMenuItemClick` |
| m10 死代码 | ✅ 已修复 | csproj 移除 `ReminderService`/`ProjectInstaller` 并取消跟踪 |
| m11 计时器重启死代码 | ✅ 已修复 | `RestartTimerIfStopped` 已删除 |
| m12 Mutex 提前释放 | ✅ 已修复 | `RunAsApplication` L61 持有至进程退出 |

**结论**：上一轮修复完整、无引入回归；核心“第一轮休息后卡死”问题已从根因层面解决。

---

## 关键问题（Major）

### [M-1] 全局 BlockInput 持续整个休息时段，崩溃即锁死整机 — 🟠 Major（安全/可用性）
- 位置：`RestFrm.SetupUI` L61-67（`KeyboardBlocker.Block()`）；`KeyboardBlocker.cs` L33-45；`RestFrm.RestFrm_FormClosing` L400-403
- 问题：`BlockInput(true)` 锁的是**整台机器**的输入（不限于本程序窗口）。`RestFrm` 在显示休息遮罩时调用 `Block()`，而解除依赖 `FormClosing`/`退出` 中的 `Unblock()`。若程序在休息期间崩溃、假死或被任务管理器强杀，没有任何代码路径会执行 `Unblock()`，用户鼠标键盘将持续锁定，只能重启或 Ctrl+Alt+Del 自救。这是“功能”背后的高危副作用。
- 根因：把“强制休息”与“系统级输入锁”直接绑定，且锁的持有期 = 整个休息时长，缺少逃生通道与兜底。
- 正确方案（二选一，推荐 A）：
  - **A. 缩短强制锁 + 逃生热键**：默认只在前几秒强制锁（例如 10s 宽限），随后自动 `Unblock()`；并在遮罩显著位置提示「按 Ctrl+Alt+Del 可随时解锁」。
  - **B. 用低层钩子替代 BlockInput**：自行安装 `WH_KEYBOARD_LL` 钩子，默认拦截除一组逃生组合（如 Ctrl+Shift+Q）外的输入；进程退出时卸载钩子。这样即便强杀，钩子随进程销毁，输入自动恢复。
  - 同时在 `KeyboardBlocker` 增加“持有期超时自动解锁”守卫：

```csharp
// KeyboardBlocker 增加安全护栏
private static System.Threading.Timer _autoUnlock;
public static bool Block(int holdSeconds = 0)
{
    if (!IsAdministrator()) { Logger.Log("[WARN] 需管理员权限，锁输入未生效"); return false; }
    BlockInput(true);
    if (holdSeconds > 0)
    {
        _autoUnlock?.Dispose();
        _autoUnlock = new System.Threading.Timer(_ => Unblock(), null, holdSeconds * 1000, Timeout.Infinite);
    }
    return true;
}
public static bool Unblock()
{
    if (!IsAdministrator()) return false;
    _autoUnlock?.Dispose(); _autoUnlock = null;
    BlockInput(false);
    return true;
}
```

### [M-2] 配置写入 exe 同级 app.config，Program Files 下静默失效 — 🟠 Major（正确性/数据持久化）
- 位置：`MainFrm.SetConfigValue` L505（`ConfigurationUserLevel.None`）；`Program.GetAppConfiguration` L131；`ThemeManager.SaveMode` L203
- 问题：`ConfigurationUserLevel.None` 会把设置写进 `Reminder.exe.config`（程序所在目录）。当程序安装在 `C:\Program Files\...` 或当前用户无该目录写权限时，`config.Save()` 抛 `ConfigurationErrorsException`，被 `catch` 吞掉（L517），**所有设置改完即丢**，用户看到“改了没用”。
- 根因：把用户可写配置放在了程序目录，违反了 UAC 下的“程序目录只读”约定。
- 正确方案（推荐引入独立 JSON 设置仓库，彻底解耦 app.config 写权限问题）：

```csharp
// Settings.cs —— 用户级、可写、与安装位置无关
public static class AppSettings
{
    private static readonly string Path =
        System.IO.Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "SedentaryReminder", "settings.json");

    public static T Get<T>(string key, T def)
    {
        try
        {
            if (!System.IO.File.Exists(Path)) return def;
            var json = System.IO.File.ReadAllText(Path);
            var dict = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
            return dict.TryGetValue(key, out var v) ? (T)Convert.ChangeType(v, typeof(T)) : def;
        }
        catch { return def; }
    }
    public static void Set<T>(string key, T val)
    {
        try
        {
            System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(Path));
            Dictionary<string, object> dict;
            if (System.IO.File.Exists(Path))
                dict = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, object>>(
                    System.IO.File.ReadAllText(Path)) ?? new();
            else dict = new();
            dict[key] = val;
            System.IO.File.WriteAllText(Path, Newtonsoft.Json.JsonConvert.SerializeObject(dict, Newtonsoft.Json.Formatting.Indented));
        }
        catch { /* 仍失败则忽略，但至少不会误导用户“已保存” */ }
    }
}
```
之后 `MainFrm`/`Program`/`RestFrm`/`ThemeManager` 全部改用 `AppSettings.Get/Set`，不再触碰 `ConfigurationManager` 写路径。最小临时补丁：把三处 `ConfigurationUserLevel.None` 改为 `ConfigurationUserLevel.PerUserRoaming`（写入用户 AppData 的 `user.config`，可写），但仍有 roaming 路径限制，建议直接采用 JSON 方案。

---

## 一般问题（Minor）

### [m-1] WorkFrm 两参构造函数已成为死代码 — 🟡 Minor（可维护性）
- 位置：`WorkFrm.cs` L62-87（`public WorkFrm(int, int)`）
- 问题：全仓已无 `new WorkFrm(a, b)` 调用（仅剩三参 `new WorkFrm(a,b,flag)`，见 L145/175/285/337/507），两参构造器仅以 `input_flag=false` 硬编码，无人使用。
- 正确方案：删除该构造函数；若需保留“不锁输入”的语义，统一用三参 `(wrk, rst, false)`。

### [m-2] WorkPeriods 配置从未被读取，README 却声称支持 — 🟡 Minor（一致性/文档）
- 位置：`README.md` L64-72（声称支持 `WorkPeriods` 精确时段）；客户端代码未引用 `WorkPeriods`（`Program` 仅用 `AutoStartHours`/`AutoStopHours`）。
- 问题：文档宣传的能力在客户端并未实现，属“文档超前于代码”；且 `Program` 的小时级调度无法表达“13:00-16:30”这类非整点区间。
- 正确方案：要么在 `Program` 中实现 `WorkPeriods` 解析（用 `DateTime.Now` 比对区间，替代小时集合），要么在 README 明确标注“暂未实现，仅 AutoStartHours/AutoStopHours 生效”。推荐前者，改动集中在 `ShouldStartWorkForm`/`CheckAndStopWorkFrm`。

### [m-3] RestFrm 读取配置缺少与 WorkFrm 一致的合法性校验 — 🟡 Minor（正确性）
- 位置：`RestFrm.GetConfigValue(string,int)` L377（仅 `TryParse`，无 `>0` 校验）；对比 `WorkFrm.GetConfigValue` L31（校验 `>0` 且 `WorkTimeValue≤480`）。
- 问题：若 `WorkTimeValue` 被误配为 `0`，`RestFrm.StartNextWorkForm` 会用 `0` 建 `WorkFrm`，导致“工作 0 秒→立即休息→再 0 秒”的死循环闪烁。`Program.GetConfigValue`（L134）有校验，但 `RestFrm` 内部用的是自己的无校验版本。
- 正确方案：`RestFrm.GetConfigValue` 复用 `Program.GetConfigValue` 的校验逻辑或抽取公共 `Config` 帮助类，统一 `WorkTimeValue>0`、`RestTimeValue>0` 的下限校验。

### [m-4] 多显示器下输入锁解除不一致 — 🟡 Minor（正确性）
- 位置：`RestFrm.SetupUI` L66（每个 RestFrm 都 `Block()`）、`RestFrm_FormClosing` L400-403（仅本实例 `Unblock()`）
- 问题：多屏休息时每个屏一个 `RestFrm` 都调用 `Block()`；若用户在某一屏按 Alt+F4 关闭该屏休息窗，`FormClosing` 立即 `Unblock()`，但其它屏休息窗仍在显示并期望输入锁定——状态不一致。
- 正确方案：用引用计数管理“锁定中”实例数（`Block`/`Unblock` 配对计数），仅当最后一个休息窗关闭时才 `Unblock()`；或在 `KeyboardBlocker` 内部用 `Interlocked` 计数。

### [m-5] 临近提醒阶段强制把窗体拉回主屏中心 — 🟡 Minor（UX）
- 位置：`WorkFrm.Warn` L423-425（`this.Location = new Point(primaryCenter...)`）
- 问题：倒计时最后 16 秒每帧都把悬浮窗移到**主屏**中心，忽略用户拖拽保存的位置（`LoadSavedPosition`）与多显示器布局；若悬浮窗在副屏，会“跳”回主屏。
- 正确方案：仅首次进入提醒态时居中一次（用 `bool _warnedOnce` 守卫），或维持用户位置仅做颜色/透明度变化。

### [m-6] Logger 日志无限增长、无轮转 — 🟡 Minor（性能/可维护性）
- 位置：`Logger.Log` L14-30（每次 `File.AppendAllText` 追加，从不清理）
- 问题：即便修复了并发写（M2 的锁），`log.txt` 仍会无限膨胀（早期曾出现 576MB）。长期运行机器上占用磁盘且难排查。
- 正确方案：按日期分文件（`log-YYYYMMDD.txt`）或单文件超过 1MB 时备份并截断；调试 `[DBG]` 日志可改为仅 Debug 构建输出。

### [m-7] KeyboardBlocker 遗留的特权常量均为死代码 — 🟡 Minor（可维护性）
- 位置：`KeyboardBlocker.cs` L17-28（`SE_PRIVILEGE_ENABLED` 等 + `TokPriv1Luid` 结构体）
- 问题：注释称“保留以避免误删”，但实际 `BlockInput` 已足够、这些常量/结构体从未被引用，属误导性死代码。
- 正确方案：删除，或在注释中明确“如未来需要 SeShutdownPrivilege 再启用”——当前可直接清理。

### [m-8] MainFrm 开机启动相关代码异味 — 🟡 Minor（可维护性）
- 位置：`MainFrm.CreateShortcut` L394（`bool res = ...` 未使用）、`GetQuickFromFolder` L451（`string tmpStr = null;` 赋值后即被覆盖）、`DeleteFile` L444 在删除文件分支弹“取消开机启动成功”但目录分支无提示。
- 正确方案：删除未使用变量；统一反馈文案；`CreateShortcut` 返回 bool 后据实提示。

---

## 优化建议（Nit）
- `WorkFrm.button1_Click`（+10 分钟）新建 `WorkFrm` 时 `wrk_m` 取“当前剩余分钟”，若后续加入进度条会算错总时长；建议把 `wrk_m` 作为“原始设定值”传入（当前 WorkFrm 未渲染进度条，暂不影响显示）。
- `WorkFrm` 的 `MainFrm_FormClosing` / `LblSecond_Click` 为空实现，应删除对应事件绑定或保留并加注释。
- `RestFrm.GetConfigValue(string,int)` 与 `(string,string)` 两个重载可合并为带默认值的单方法。
- `ThemeManager.Number` 字体名 `Segoe UI Semibold` 在非英文/老系统可能缺失，建议回退到 `Microsoft Sans Serif` 或沿用 `微软雅黑`。
- `MainFrm` 多处重复 `SetConfigValue("WorkTimeValue"...)` 等键值，建议抽常量类避免拼写漂移。

---

## 架构评估
- 模块边界：入口 `Program` 负责调度/单例/清理；`WorkFrm`/`RestFrm` 是视图；`MainFrm` 是设置窗；`ThemeManager`/`Logger`/`KeyboardBlocker`/`ThemeDialog` 是横切工具。边界大体清晰。
- **核心架构风险：循环状态散落为跨类的多个静态字段**。`Program.activeWorkFrm/activeRestFrm`、`WorkFrm.isRunning/currentInstance`、`RestFrm.messageShown/isMessageShowing` 三处静态状态需手动同步，正是上一轮“卡死/双会话”类问题的温床。上一轮靠守卫条件缓解了症状，但未消除根因。
  - 正确方向：引入单一 `SessionCoordinator`（可复用 `Program` 静态方法收敛为一个类），把“当前会话/阶段（Working/Resting/Idle）”建模为显式状态机；`WorkFrm`/`RestFrm` 仅作为**纯视图**，通过事件/回调上报“倒计时归零”“用户跳过”，由协调器统一决定下一步。这样跨窗体的 `isRunning/messageShown` 等标志消失，循环正确性由状态机保证，可测试性也大幅提升。
- 调度逻辑重复：`Program`（客户端）与 `Scheduler/`（独立服务工程）都实现了“工作/休息时段调度”，且 README 声称服务可独立运行 GUI——但 `Program.cs` 明确注释“Windows 服务不适合需要显示 GUI 的应用（Session 0 隔离）”。存在定位冲突，建议明确以**客户端内调度为准**，`Scheduler` 仅作为“无桌面的后台启动器”或直接弃用，并在 README 纠正“可作为系统服务运行”的措辞。
- 可扩展性：主题/字体已集中（好）；配置读写仍分散且依赖 app.config（见 M-2），是后续最大的改造点。

---

## UI / 功能增强建议（本轮新增，按价值排序）

### UI 体验
1. **休息遮罩增加「我已在活动」提前结束按钮**：当前休息必须等倒计时归零（`RestFrm` 无提前结束入口）。建议加一个显眼的“结束休息 / 我已活动”按钮，点击即 `CloseAllRestForms()` 并启动下一轮工作——比“等 15 秒弹窗”体验好得多。
2. **工作悬浮窗增加环形进度 + 状态文案**：现有 `lblProgress` 仅警告期有字。建议常驻显示「已静坐 X 分 / 距休息 Y 分」，并用环形进度条体现当日节奏，弱化“被催促”感。
3. **「温和模式」与「严格模式」分离**：严格模式才启用输入锁（全屏遮罩）；温和模式仅半透明横幅 + 提示音，可一键忽略。把“锁输入”从默认开启改为默认温和，降低 M-1 风险与用户抵触。
4. **托盘菜单补全快捷操作**：在现有「主窗口/隐藏倒计时/开机启动/关于/退出」之外，增加「立即休息」「暂停今日提醒」「重置计时」，减少打开设置窗的次数。
5. **休息内容可读化**：遮罩除“站起来”外，轮播拉伸/远眺/喝水等具体动作与 20-20-20 护眼提示，提升实用感。
6. **设置窗补全被遗漏的配置项 UI**：当前只能设工作时长/休息时长/锁输入/隐藏/主题；`AutoStartHours`/`AutoStopHours`、显示器选择、`WorkPeriods`（见 m-2）都应提供图形配置，避免改 app.config。

### 功能
7. **AFK（离开）自动暂停**：用 `GetLastInputInfo` 检测用户 idle，离开座位时不打扰、不累计；回到座位再续。避免“人走了还在弹提醒”。
8. **本地健康统计（轻量、正确版）**：上一轮 `7c5e0f6` 移除了有 bug 的 `StatsFrm`/`StatsTracker`。建议用本地 JSON 重新实现一个最小统计：每日坐/站次数、连续达标天数、本周曲线，无需联网，隐私可控。
9. **多类型提醒**：在“久坐”之外，支持护眼（20-20-20）、喝水、 posture 提醒，统一调度。
10. **设置导入/导出 + 便携化**：M-2 改为 JSON 设置后，天然支持 `settings.json` 的导入/导出与 U 盘便携运行。
11. **提示音可配置**：休息开始/结束增加可选的轻柔提示音（可选文件或系统音），避免纯视觉提醒在静音环境被忽略。
12. **首次运行引导优化**：首次运行已弹气泡，但“开始计时”仍需手动点；建议首跑引导里直接询问工作/休息偏好并一键启动。

### 工程/交付
13. **补充最小自动化测试**：把 `ShouldStartWorkForm`、`GetConfigValues`、配置校验等纯逻辑抽成静态方法后，可用 xUnit 覆盖（无需 UI），先把“调度/配置”这类易错逻辑锁死。
14. **README 与实现对齐**：删除“系统服务运行 GUI”的误导性描述，标注 `WorkPeriods` 状态，更新项目结构图（已移除 `ReminderService`/`ProjectInstaller`）。

---

## 优先级建议（先修什么）
1. **[M-1] 输入锁安全**（最高优先，关乎能否把用户锁在机器外）——本轮务必处理。
2. **[M-2] 配置持久化失效**（直接影响“设置不保存”类反馈）——本轮建议处理。
3. **[m-3]/[m-4]** 配置校验与多屏解锁一致性（与 M-1/M-2 同源，顺手修）。
4. 其余 Minor / Nit 与 UI·功能建议按排期迭代，不阻塞合并。
5. 架构层面建议把“跨类静态状态”收敛为单一状态机，作为下一阶段的根本性重构目标。

> 说明：本机无 MSBuild，以上结论均基于源码静态审查与上一轮运行日志实证；涉及 UI/行为的改动请在 Visual Studio 重新生成并实机验证（尤其 M-1 的输入锁路径与多显示器场景）。
