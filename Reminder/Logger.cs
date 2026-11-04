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
            // m-6：按日期分文件（logs/log-YYYYMMDD.txt），避免单文件无限膨胀（早期曾出现 576MB）；
            // 单一日志文件超过 1MB 时归档为 .old.txt 并新建，保留近况又不失控。
            string dir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs");
            Directory.CreateDirectory(dir);
            string path = Path.Combine(dir, $"log-{DateTime.Now:yyyyMMdd}.txt");
            string logMessage = $"{DateTime.Now}: {message}";
            lock (_logLock)
            {
                if (File.Exists(path) && new FileInfo(path).Length > 1_000_000)
                {
                    string archived = path.Replace(".txt", ".old.txt");
                    try { File.Delete(archived); } catch { }
                    try { File.Move(path, archived); } catch { }
                }
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
