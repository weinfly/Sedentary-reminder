﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace Reminder
{
    public partial class RestFrm : Form
    {
        private int rst_m;
        private int wrk_m;
        private int rst_m2;
        private bool input_flag;
        int rst_s = 0;
        private bool main_screen;

        public RestFrm()
        {
            InitializeComponent();
        }
        public RestFrm(int rst_minutes, int wrk_minutes, bool input_flag, bool main_screen, Point location)
        {
            InitializeComponent();
            this.rst_m = rst_minutes;
            this.wrk_m = wrk_minutes;
            this.rst_m2 = rst_minutes;
            this.input_flag = input_flag;
            // create the workFrm on the main screen, so use this to check
            this.main_screen = main_screen;
            // Manual that the form will be created on the position depend on the location
            this.StartPosition = FormStartPosition.Manual;
            this.Location = location;
        }

        private void RestFrm_Load(object sender, EventArgs e)
        {           
            if (input_flag)
            {
                lblText.Text = "久坐对身体不好！您已久坐" + wrk_m.ToString() + "分钟了，键盘和鼠标被锁定，站起来活动下！";
            }
            else
            {
                lblText.Text = "久坐对身体不好！您已久坐" + wrk_m.ToString() + "分钟了，站起来活动下！Alt+F4 退出本界面。";
            }


            timerRst.Enabled = true;            
            this.TopMost = true;
           
            this.WindowState = FormWindowState.Maximized;
            this.Opacity = 0.75;
            if (input_flag)
            {
                KeyboardBlocker.off();//锁定键盘               
            }

            if (rst_s >= 10)
            {
                lbl_seconds.Text = rst_s.ToString();
            }
            else
            {
                lbl_seconds.Text = "0"+rst_s.ToString();
            }


            if (rst_m >= 10)
            {
                lbl_minutes.Text = rst_m.ToString();
            }
            else
            {
                lbl_minutes.Text = "0" + rst_m.ToString();
            }
            
            

        }

        private void TimerRst_Tick(object sender, EventArgs e)
        {
            timing();
        }
        private int GetConfigValue(string key, int defaultValue)
        {
            int value = defaultValue;
            string configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "app.config");
            ExeConfigurationFileMap configFileMap = new ExeConfigurationFileMap
            {
                ExeConfigFilename = configPath
            };

            Configuration config = ConfigurationManager.OpenMappedExeConfiguration(configFileMap, ConfigurationUserLevel.None);

            if (config.AppSettings.Settings.AllKeys.Contains(key))
            {
                string valueString = config.AppSettings.Settings[key].Value;
                if (!string.IsNullOrEmpty(valueString))
                {
                    int.TryParse(valueString, out value);
                }
            }

            return value;
        }
        private void timing()
        {
            if (rst_s > 0)
            {
                rst_s = rst_s - 1;
                if (rst_s >= 10)
                {
                    lbl_seconds.Text = rst_s.ToString();
                }
                else
                {
                    lbl_seconds.Text = "0"+rst_s.ToString();
                }
                
            }
            else //秒=0时，分钟-1
            {
                timerRst.Enabled = false;
                rst_m--;
                if (rst_m>=10) {
                    lbl_minutes.Text = rst_m.ToString();
                }
                else
                {
                    lbl_minutes.Text = "0"+rst_m.ToString();
                }
                
                if (rst_m > -1) //若分钟不为0，秒回到60，继续递归
                {
                    timerRst.Enabled = true;
                    rst_s = 59;
                    timing();
                }
                else
                {                    
                    if (input_flag)
                    {                       
                        KeyboardBlocker.on();//解锁键盘
                    }

                    //修改这里，默认使用配置文件的值重新进行计时，而不是原来代码里面的，使用主窗体界面文本框里的值。因为那里我经常会修改
                    if (rst_s == 0 && main_screen)
                    {
                        int workTimeValue = GetConfigValue("WorkTimeValue", 45); // 默认值为 45 分钟
                        int restTimeValue = GetConfigValue("RestTimeValue", 15); // 默认值为 15 分钟

                        WorkFrm workFrm = new WorkFrm(workTimeValue, restTimeValue, input_flag);
                        workFrm.Show();
                    }
                    this.Close();
                }
            }
        }

        private void RestFrm_FormClosed(object sender, FormClosedEventArgs e)
        {

        }

        private void RestFrm_FormClosing(object sender, FormClosingEventArgs e)
        {
            //WorkFrm workFrm = new WorkFrm(wrk_m, rst_m2, input_flag);
           // workFrm.Show();
        }

        private void lblText_Click(object sender, EventArgs e)
        {

        }
    }
}
