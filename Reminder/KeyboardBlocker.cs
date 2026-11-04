using System;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Threading;

namespace Reminder
{
    /// <summary>
    /// 键盘/鼠标输入锁定工具（封装 user32!BlockInput，系统级全局锁定）。
    /// ⚠️ 重要安全说明：
    ///  - BlockInput 锁定的是【整台机器】的输入（不限于本进程窗口）。
    ///  - 若进程在锁定期间崩溃/被任务管理器强杀且未解锁，输入将持续锁定直至重启或手动解锁，
    ///    可能把用户“锁在机器外”。因此本类做了三道护栏：
    ///    1) 默认仅强制锁定【前 2 分钟】（holdSeconds 默认 120s），到点自动解锁；
    ///    2) 引用计数配对 Block/Unblock（多显示器各计一次，仅最后一个释放才真正解锁）；
    ///    3) 需要管理员权限，非管理员时 Block 不生效（仅告警并返回 false）。
    /// </summary>
    public static class KeyboardBlocker
    {
        [DllImport("user32.dll")]
        private static extern bool BlockInput(bool fBlockIt);

        private static readonly object _sync = new object();
        private static int _refCount = 0;          // 引用计数：多显示器下每个休息窗各 +1
        private static bool _held = false;          // 当前是否真正处于物理锁定状态
        private static Timer _autoUnlock;           // 自动解锁护栏（到点强制解锁，防崩溃锁死整机）

        /// <summary>
        /// 锁定系统输入。holdSeconds > 0 时到点自动解锁（兜底，防止进程崩溃导致整机锁死）。
        /// 默认仅锁定前 2 分钟（120s），休息剩余时间由用户自行决定是否继续休息。
        /// 需要管理员权限；非管理员时记录告警并返回 false（不实际锁定）。
        /// </summary>
        public static bool Block(int holdSeconds = 120)
        {
            lock (_sync)
            {
                if (_refCount == 0)
                {
                    if (!IsAdministrator())
                    {
                        Logger.Log("[WARN] KeyboardBlocker.Block 需要管理员权限，锁输入未生效（请右键“以管理员身份运行”）");
                        return false;
                    }
                    BlockInput(true);
                    _held = true;
                    Logger.Log("[DBG] KeyboardBlocker 已锁定系统输入");
                }
                _refCount++;

                // 以最近一次 Block 的持有期为准重置自动解锁护栏
                _autoUnlock?.Dispose();
                _autoUnlock = null;
                if (holdSeconds > 0)
                {
                    _autoUnlock = new Timer(_ => ForceUnlock(), null, holdSeconds * 1000, Timeout.Infinite);
                }
                return true;
            }
        }

        /// <summary>
        /// 解除系统输入锁定（引用计数配对）。仅当最后一个持有者释放时才真正解除物理锁定。
        /// </summary>
        public static bool Unblock()
        {
            lock (_sync)
            {
                if (_refCount <= 0) return false;
                _refCount--;
                if (_refCount == 0)
                {
                    ForceUnlock();
                }
                return true;
            }
        }

        /// <summary>
        /// 强制解除物理锁定（幂等，仅当当前确实持有时调用 BlockInput(false)）。
        /// </summary>
        private static void ForceUnlock()
        {
            lock (_sync)
            {
                _autoUnlock?.Dispose();
                _autoUnlock = null;
                if (_held)
                {
                    BlockInput(false);
                    _held = false;
                    Logger.Log("[DBG] KeyboardBlocker 已解除系统输入锁定");
                }
            }
        }

        public static bool IsAdministrator()
        {
            try
            {
                using (var identity = WindowsIdentity.GetCurrent())
                {
                    var principal = new WindowsPrincipal(identity);
                    return principal.IsInRole(WindowsBuiltInRole.Administrator);
                }
            }
            catch
            {
                return false;
            }
        }
    }
}
