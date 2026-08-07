using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

namespace Reminder
{
    /// <summary>
    /// 单日使用统计
    /// </summary>
    public class DayStat
    {
        public int FocusSessions;
        public int RestSessions;
        public int FocusMinutes;
        public int RestMinutes;
    }

    /// <summary>
    /// 轻量使用统计：按日期记录专注/休息次数与时长，持久化于
    /// %LocalAppData%/SedentaryReminder/stats.txt（简单文本格式，无第三方依赖）。
    /// 文本格式：每行 `yyyy-MM-dd|专注次数|休息次数|专注分钟|休息分钟`
    /// </summary>
    public static class StatsTracker
    {
        private static readonly string FilePath =
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "SedentaryReminder", "stats.txt");

        private static Dictionary<string, DayStat> _store;

        private static Dictionary<string, DayStat> Store
        {
            get { return _store ?? (_store = Load()); }
        }

        private static Dictionary<string, DayStat> Load()
        {
            var dict = new Dictionary<string, DayStat>();
            try
            {
                if (File.Exists(FilePath))
                {
                    foreach (var line in File.ReadAllLines(FilePath))
                    {
                        var p = line.Split('|');
                        if (p.Length == 5 && int.TryParse(p[1], out int fs) && int.TryParse(p[2], out int rs)
                            && int.TryParse(p[3], out int fm) && int.TryParse(p[4], out int rm))
                        {
                            dict[p[0]] = new DayStat
                            {
                                FocusSessions = fs,
                                RestSessions = rs,
                                FocusMinutes = fm,
                                RestMinutes = rm
                            };
                        }
                    }
                }
            }
            catch
            {
                dict = new Dictionary<string, DayStat>();
            }

            return dict;
        }

        private static void Save(Dictionary<string, DayStat> dict)
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(FilePath));
                var lines = new List<string>(dict.Count);
                foreach (var kv in dict)
                {
                    lines.Add($"{kv.Key}|{kv.Value.FocusSessions}|{kv.Value.RestSessions}|{kv.Value.FocusMinutes}|{kv.Value.RestMinutes}");
                }

                File.WriteAllLines(FilePath, lines);
            }
            catch
            {
                // 无写入权限时忽略
            }
        }

        /// <summary>记录一次完成的专注时段（分钟）</summary>
        public static void RecordFocusSession(int minutes)
        {
            var key = DateTime.Today.ToString("yyyy-MM-dd");
            if (!Store.TryGetValue(key, out var d))
            {
                d = new DayStat();
                Store[key] = d;
            }

            d.FocusSessions++;
            d.FocusMinutes += Math.Max(0, minutes);
            Save(Store);
        }

        /// <summary>记录一次完成的休息时段（分钟）</summary>
        public static void RecordRestSession(int minutes)
        {
            var key = DateTime.Today.ToString("yyyy-MM-dd");
            if (!Store.TryGetValue(key, out var d))
            {
                d = new DayStat();
                Store[key] = d;
            }

            d.RestSessions++;
            d.RestMinutes += Math.Max(0, minutes);
            Save(Store);
        }

        /// <summary>今日统计</summary>
        public static DayStat GetToday()
        {
            var key = DateTime.Today.ToString("yyyy-MM-dd");
            return Store.TryGetValue(key, out var d) ? d : new DayStat();
        }

        /// <summary>累计统计（全部日期汇总）</summary>
        public static DayStat GetTotal()
        {
            var total = new DayStat();
            foreach (var d in Store.Values)
            {
                total.FocusSessions += d.FocusSessions;
                total.RestSessions += d.RestSessions;
                total.FocusMinutes += d.FocusMinutes;
                total.RestMinutes += d.RestMinutes;
            }

            return total;
        }
    }
}
