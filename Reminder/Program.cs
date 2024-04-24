using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Windows.Forms;

namespace Reminder
{
    static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            bool ExisFlag = false;
            System.Diagnostics.Process currentProccess = System.Diagnostics.Process.GetCurrentProcess();
            System.Diagnostics.Process[] currentProccessArray = System.Diagnostics.Process.GetProcesses();

            foreach (System.Diagnostics.Process p in currentProccessArray)
            {
                if (p.ProcessName == currentProccess.ProcessName && p.Id != currentProccess.Id)
                {
                    ExisFlag = true;
                }
            }

            if (ExisFlag) // 防止程序二次运行
            {
                MessageBox.Show("程序运行中，见右下角系统托盘");
                return;
            }
            else
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);

                // 读取 app.config 文件中的值
                //Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                string configPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "app.config");
                ExeConfigurationFileMap configFileMap = new ExeConfigurationFileMap();
                configFileMap.ExeConfigFilename = configPath;
                Configuration config = ConfigurationManager.OpenMappedExeConfiguration(configFileMap, ConfigurationUserLevel.None);

                int workTimeValue = GetWorkTimeValue(config);
                int restTimeValue = GetRestTimeValue(config);
                MainFrm mainForm = new MainFrm();
                mainForm.SetWorkTimeValue(workTimeValue);
                mainForm.SetRestTimeValue(restTimeValue);
                Application.Run(mainForm);
            }
        }

        private static int GetWorkTimeValue(Configuration config)
        {
            int workTimeValue = 45; // 默认值为 45
            KeyValueConfigurationElement workTimeElement;
            if (config.AppSettings.Settings.AllKeys.Contains("WorkTimeValue"))
            {
                workTimeElement = config.AppSettings.Settings["WorkTimeValue"];
                if (!string.IsNullOrEmpty(workTimeElement.Value))
                {
                    int.TryParse(workTimeElement.Value, out workTimeValue);
                }
            }
            return workTimeValue;
        }

        private static int GetRestTimeValue(Configuration config)
        {
            int restTimeValue = 15; // 默认值为 15
            KeyValueConfigurationElement restTimeElement;
            if (config.AppSettings.Settings.AllKeys.Contains("RestTimeValue"))
            {
                restTimeElement = config.AppSettings.Settings["RestTimeValue"];
                if (!string.IsNullOrEmpty(restTimeElement.Value))
                {
                    int.TryParse(restTimeElement.Value, out restTimeValue);
                }
            }
            return restTimeValue;
        }
    }
}