using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;

namespace SedentaryReminder.Scheduler
{
    /// <summary>
    /// 调度配置 - 管理工作时间段配置
    /// </summary>
    public class ScheduleConfig
    {
        /// <summary>
        /// 单个工作时间段
        /// </summary>
        public struct WorkPeriod
        {
            public int StartHour;
            public int StartMinute;
            public int EndHour;
            public int EndMinute;

            public WorkPeriod(int startHour, int startMinute, int endHour, int endMinute)
            {
                StartHour = startHour;
                StartMinute = startMinute;
                EndHour = endHour;
                EndMinute = endMinute;
            }

            public TimeSpan StartTime => new TimeSpan(StartHour, StartMinute, 0);
            public TimeSpan EndTime => new TimeSpan(EndHour, EndMinute, 0);
        }

        /// <summary>
        /// 默认工作时间段：9:00-12:00, 13:00-18:00
        /// </summary>
        private static readonly List<WorkPeriod> DefaultWorkPeriods = new List<WorkPeriod>
        {
            new WorkPeriod(9, 0, 12, 0),
            new WorkPeriod(13, 0, 18, 0)
        };

        /// <summary>
        /// 工作日：周一到周五 (1-5)，周六=6，周日=0
        /// </summary>
        public bool IsWorkDay(DayOfWeek day)
        {
            return day != DayOfWeek.Saturday && day != DayOfWeek.Sunday;
        }

        /// <summary>
        /// 获取配置的工作时间段
        /// </summary>
        public List<WorkPeriod> GetWorkPeriods()
        {
            // 尝试读取新格式的 WorkPeriods
            var periodsConfig = ConfigurationManager.AppSettings["WorkPeriods"];
            if (!string.IsNullOrEmpty(periodsConfig))
            {
                return ParseWorkPeriods(periodsConfig);
            }

            // 回退到旧格式
            return GetWorkPeriodsFromLegacyConfig();
        }

        private List<WorkPeriod> ParseWorkPeriods(string config)
        {
            var periods = new List<WorkPeriod>();
            var parts = config.Split(',');

            for (int i = 0; i < parts.Length - 1; i += 2)
            {
                if (i + 1 >= parts.Length) break;

                var startPart = parts[i].Trim();
                var endPart = parts[i + 1].Trim();

                int sh, sm, eh, em;
                if (TryParseTime(startPart, out sh, out sm) &&
                    TryParseTime(endPart, out eh, out em))
                {
                    periods.Add(new WorkPeriod(sh, sm, eh, em));
                }
            }

            return periods.Count > 0 ? periods : DefaultWorkPeriods;
        }

        private bool TryParseTime(string timeStr, out int hour, out int minute)
        {
            hour = 0;
            minute = 0;

            var parts = timeStr.Split(':');
            if (parts.Length == 2)
            {
                int h, m;
                if (int.TryParse(parts[0], out h) && int.TryParse(parts[1], out m))
                {
                    hour = h;
                    minute = m;
                    return hour >= 0 && hour <= 23 && minute >= 0 && minute <= 59;
                }
            }

            return false;
        }

        private List<WorkPeriod> GetWorkPeriodsFromLegacyConfig()
        {
            var startHoursStr = ConfigurationManager.AppSettings["AutoStartHours"] ?? "9,13";
            var stopHoursStr = ConfigurationManager.AppSettings["AutoStopHours"] ?? "12,18";

            var startParts = startHoursStr.Split(',');
            var stopParts = stopHoursStr.Split(',');

            List<int> startHours = new List<int>();
            foreach (var s in startParts)
            {
                int h;
                if (int.TryParse(s.Trim(), out h) && h >= 0 && h <= 23)
                {
                    startHours.Add(h);
                }
            }

            List<int> stopHours = new List<int>();
            foreach (var s in stopParts)
            {
                int h;
                if (int.TryParse(s.Trim(), out h) && h >= 0 && h <= 23)
                {
                    stopHours.Add(h);
                }
            }

            var periods = new List<WorkPeriod>();
            int count = Math.Min(startHours.Count, stopHours.Count);
            for (int i = 0; i < count; i++)
            {
                periods.Add(new WorkPeriod(startHours[i], 0, stopHours[i], 0));
            }

            return periods.Count > 0 ? periods : DefaultWorkPeriods;
        }

        /// <summary>
        /// 检查当前时间是否在工作时间段内
        /// </summary>
        public bool IsWithinWorkPeriod(DateTime now)
        {
            if (!IsWorkDay(now.DayOfWeek))
                return false;

            var current = new TimeSpan(now.Hour, now.Minute, now.Second);
            var periods = GetWorkPeriods();

            foreach (var period in periods)
            {
                if (current >= period.StartTime && current < period.EndTime)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// 检查是否刚刚进入工作时间段（用于触发启动）
        /// </summary>
        public bool ShouldTriggerStart(DateTime now, DateTime lastCheck)
        {
            if (!IsWorkDay(now.DayOfWeek))
                return false;

            return IsWithinWorkPeriod(now) && !IsWithinWorkPeriod(lastCheck);
        }

        /// <summary>
        /// 检查是否刚刚离开工作时间段（用于触发停止）
        /// </summary>
        public bool ShouldTriggerStop(DateTime now, DateTime lastCheck)
        {
            return !IsWithinWorkPeriod(now) && IsWithinWorkPeriod(lastCheck);
        }

        /// <summary>
        /// 获取工作时间（分钟）
        /// </summary>
        public int GetWorkTimeMinutes()
        {
            var value = ConfigurationManager.AppSettings["WorkTimeValue"];
            int result;
            if (int.TryParse(value, out result) && result > 0)
                return result;
            return 45;
        }

        /// <summary>
        /// 获取休息时间（分钟）
        /// </summary>
        public int GetRestTimeMinutes()
        {
            var value = ConfigurationManager.AppSettings["RestTimeValue"];
            int result;
            if (int.TryParse(value, out result) && result > 0)
                return result;
            return 15;
        }

        /// <summary>
        /// 获取客户端程序路径
        /// </summary>
        public string GetClientExePath()
        {
            var path = ConfigurationManager.AppSettings["ClientExePath"];
            if (!string.IsNullOrEmpty(path) && System.IO.File.Exists(path))
                return path;

            // 默认：与调度器同目录下的 Reminder.exe
            var defaultPath = System.IO.Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory, "Reminder.exe");
            if (System.IO.File.Exists(defaultPath))
                return defaultPath;
            return null;
        }
    }
}