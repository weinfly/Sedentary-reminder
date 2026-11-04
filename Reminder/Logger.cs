using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Reminder
{
    public static class Logger
    {
    private static readonly object _logLock = new object();

    public static void Log(string message)
    {
        try
        {
            // 固定路径到程序目录，避免相对路径歧义；加锁避免多线程并发写文件抛 IOException
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "log.txt");
            string logMessage = $"{DateTime.Now}: {message}";
            lock (_logLock)
            {
                File.AppendAllText(path, logMessage + Environment.NewLine);
            }
        }
        catch
        {
            // 日志写入失败不应影响主流程
        }
    }
    }
}
