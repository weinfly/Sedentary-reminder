using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Principal;

namespace SedentaryReminder.Scheduler
{
    /// <summary>
    /// 客户端启动器 - 在用户会话中启动 UI 程序
    /// Windows Service 运行在 Session 0，需要使用 WTSQueryUserToken + CreateProcessAsUser
    /// 才能在用户桌面会话中启动 GUI 应用
    /// </summary>
    public class ClientLauncher : IDisposable
    {
        [System.Runtime.InteropServices.DllImport("kernel32.dll")]
        private static extern uint WTSGetActiveConsoleSessionId();

        [System.Runtime.InteropServices.DllImport("wtsapi32.dll")]
        private static extern bool WTSQueryUserToken(uint SessionId, out IntPtr phToken);

        [System.Runtime.InteropServices.DllImport("advapi32.dll", SetLastError = true)]
        private static extern bool DuplicateToken(IntPtr ExistingTokenHandle, int SECURITY_IMPERSONATION_LEVEL, out IntPtr DuplicateTokenHandle);

        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern bool CreateProcessAsUser(
            IntPtr hToken,
            string lpApplicationName,
            string lpCommandLine,
            IntPtr lpProcessAttributes,
            IntPtr lpThreadAttributes,
            bool bInheritHandle,
            uint dwCreationFlags,
            IntPtr lpEnvironment,
            string lpCurrentDirectory,
            ref STARTUPINFO lpStartupInfo,
            out PROCESS_INFORMATION lpProcessInformation);

        [System.Runtime.InteropServices.DllImport("kernel32.dll")]
        private static extern bool CloseHandle(IntPtr hObject);

        [System.Runtime.InteropServices.DllImport("userenv.dll", SetLastError = true)]
        private static extern bool CreateEnvironmentBlock(out IntPtr lpEnvironment, IntPtr hToken, bool bInherit);

        private const uint CREATE_UNICODE_ENVIRONMENT = 0x00000400;
        private const int SecurityIdentification = 1;

        [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
        struct STARTUPINFO
        {
            public Int32 cb;
            public string lpReserved;
            public string lpDesktop;
            public string lpTitle;
            public Int32 dwX;
            public Int32 dwY;
            public Int32 dwXSize;
            public Int32 dwYSize;
            public Int32 dwXCountChars;
            public Int32 dwYCountChars;
            public Int32 dwFillAttribute;
            public Int32 dwFlags;
            public Int16 wShowWindow;
            public Int16 cbReserved2;
            public IntPtr lpReserved2;
            public IntPtr hStdInput;
            public IntPtr hStdOutput;
            public IntPtr hStdError;
        }

        [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
        struct PROCESS_INFORMATION
        {
            public IntPtr hProcess;
            public IntPtr hThread;
            public int dwProcessId;
            public int dwThreadId;
        }

        private IntPtr _primaryToken = IntPtr.Zero;
        private IntPtr _duplicateToken = IntPtr.Zero;

        /// <summary>
        /// 在用户会话中启动客户端程序
        /// </summary>
        public bool LaunchClient(string exePath, string arguments = "")
        {
            if (string.IsNullOrEmpty(exePath) || !File.Exists(exePath))
            {
                Log($"客户端程序不存在: {exePath}");
                return false;
            }

            try
            {
                // 获取当前活跃控制台会话 ID
                uint sessionId = WTSGetActiveConsoleSessionId();
                Log($"活跃会话 ID: {sessionId}");

                // 获取用户 token
                IntPtr userToken = IntPtr.Zero;
                if (!WTSQueryUserToken(sessionId, out userToken))
                {
                    Log($"WTSQueryUserToken 失败，错误码: {Marshal.GetLastWin32Error()}");
                    return false;
                }

                try
                {
                    // 复制 token
                    if (!DuplicateToken(userToken, SecurityIdentification, out _primaryToken))
                    {
                        Log($"DuplicateToken 失败，错误码: {Marshal.GetLastWin32Error()}");
                        return false;
                    }

                    // 创建环境变量块
                    IntPtr env = IntPtr.Zero;
                    if (!CreateEnvironmentBlock(out env, _primaryToken, false))
                    {
                        Log($"CreateEnvironmentBlock 失败，错误码: {Marshal.GetLastWin32Error()}");
                    }

                    // 准备启动信息
                    var si = new STARTUPINFO();
                    si.cb = System.Runtime.InteropServices.Marshal.SizeOf(si);
                    si.lpDesktop = "Winsta0\\Default"; // 使用默认桌面

                    var pi = new PROCESS_INFORMATION();
                    string cmdLine = $"\"{exePath}\" {arguments}";
                    string workDir = Path.GetDirectoryName(exePath);

                    Log($"启动客户端: {cmdLine}");

                    bool result = CreateProcessAsUser(
                        _primaryToken,
                        exePath,
                        cmdLine,
                        IntPtr.Zero,
                        IntPtr.Zero,
                        false,
                        CREATE_UNICODE_ENVIRONMENT,
                        env,
                        workDir,
                        ref si,
                        out pi);

                    if (result)
                    {
                        Log($"客户端启动成功，PID: {pi.dwProcessId}");
                        CloseHandle(pi.hThread);
                        CloseHandle(pi.hProcess);
                    }
                    else
                    {
                        Log($"CreateProcessAsUser 失败，错误码: {Marshal.GetLastWin32Error()}");
                    }

                    if (env != IntPtr.Zero)
                    {
                        CreateEnvironmentBlock(out env, IntPtr.Zero, true); // Clean up
                    }

                    return result;
                }
                finally
                {
                    if (userToken != IntPtr.Zero)
                        CloseHandle(userToken);
                }
            }
            catch (Exception ex)
            {
                Log($"启动客户端异常: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 检查客户端是否正在运行
        /// </summary>
        public bool IsClientRunning(string exeName)
        {
            try
            {
                var processes = Process.GetProcessesByName(Path.GetFileNameWithoutExtension(exeName));
                return processes.Length > 0;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 停止客户端程序
        /// </summary>
        public bool StopClient(string exeName)
        {
            try
            {
                var processes = Process.GetProcessesByName(Path.GetFileNameWithoutExtension(exeName));
                bool stopped = false;

                foreach (var process in processes)
                {
                    try
                    {
                        // 检查是否是当前会话中的进程（跳过 Session 0）
                        if (process.SessionId != 0)
                        {
                            process.Kill();
                            process.WaitForExit(5000);
                            Log($"已停止客户端进程 PID: {process.Id}");
                            stopped = true;
                        }
                    }
                    catch (Exception ex)
                    {
                        Log($"停止进程失败 PID:{process.Id}, 错误:{ex.Message}");
                    }
                }

                return stopped;
            }
            catch (Exception ex)
            {
                Log($"停止客户端异常: {ex.Message}");
                return false;
            }
        }

        private void Log(string message)
        {
            try
            {
                var logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "scheduler.log");
                File.AppendAllText(logPath, $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}{Environment.NewLine}");
            }
            catch
            {
                // 忽略日志写入错误
            }
        }

        public void Dispose()
        {
            if (_primaryToken != IntPtr.Zero)
            {
                CloseHandle(_primaryToken);
                _primaryToken = IntPtr.Zero;
            }
            if (_duplicateToken != IntPtr.Zero)
            {
                CloseHandle(_duplicateToken);
                _duplicateToken = IntPtr.Zero;
            }
        }
    }
}