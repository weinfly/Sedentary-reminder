using System.Runtime.InteropServices;
using System.Security.Principal;

namespace Reminder
{
    /// <summary>
    /// 键盘/鼠标输入锁定工具。
    /// 警告：<see cref="Block"/> 通过 user32!BlockInput 进行【系统级、全局】输入锁定，会锁住整台机器的鼠标与键盘
    /// （不限于本进程窗口）。若进程被强制终止且未调用 <see cref="Unblock"/>，系统输入将保持锁定直至重启或手动解锁。
    /// 因此必须在所有退出路径（含异常）上配对调用 <see cref="Unblock"/>。
    /// 该功能需要管理员权限；非管理员时 Block 不生效（返回 false，仅记录告警）。
    /// </summary>
    class KeyboardBlocker
    {
        // 以下权限常量与结构体为早期“提升特权”实现遗留，当前 BlockInput 已足够，保留以避免误删；
        // 如后续需要显式启用 SeShutdownPrivilege 再启用。
        private const int SE_PRIVILEGE_ENABLED = 0x00000002;
        private const int TOKEN_QUERY = 0x00000008;
        private const int TOKEN_ADJUST_PRIVILEGES = 0x00000020;
        private const string SE_SHUTDOWN_NAME = "SeShutdownPrivilege";

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        private struct TokPriv1Luid
        {
            public int Count;
            public long Luid;
            public int Attr;
        }

        /// <summary>
        /// 锁定（禁用）整机的鼠标与键盘输入。需要管理员权限；非管理员时记录告警并返回 false。
        /// </summary>
        public static bool Block()
        {
            if (IsAdministrator())
            {
                BlockInput(true);//锁定鼠标及键盘
                return true;
            }
            else
            {
                Logger.Log("[WARN] KeyboardBlocker.Block 需要管理员权限，锁输入未生效（请右键“以管理员身份运行”）");
                return false;
            }
        }

        /// <summary>
        /// 解除（恢复）整机的鼠标与键盘输入锁定。
        /// </summary>
        public static bool Unblock()
        {
            if (IsAdministrator())
            {
                BlockInput(false);//解除键盘鼠标锁定
                return true;
            }
            else
            {
                // 非管理员时 Block 本就不生效，无需报错
                return false;
            }
        }

        /// <summary>
        /// 是否是管理员权限
        /// </summary>
        public static bool IsAdministrator()
        {
            WindowsIdentity current = WindowsIdentity.GetCurrent();
            WindowsPrincipal windowsPrincipal = new WindowsPrincipal(current);
            return windowsPrincipal.IsInRole(WindowsBuiltInRole.Administrator);
        }

        [DllImport("user32.dll")]
        static extern void BlockInput(bool Block);

    }
}
