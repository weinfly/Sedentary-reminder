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
        public static void Log(string message)
        {
            string logMessage = $"{DateTime.Now}: {message}";
            // 输出到控制台
            Console.WriteLine(logMessage);
            // 或者写入文件
            File.AppendAllText("log.txt", logMessage + Environment.NewLine);
        }
    }
}
