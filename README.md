克隆自https://github.com/wjbgis/Sedentary-reminder
进行了一下的修改：
1、将工作时间和休息时间修改为读取配置文件方式，配置文件为app.config.
2、程序运行就启动倒计时以及最小化主窗体(隐藏暂时没实现，代码未成功)。
3、添加开机启动功能。

W:

20240806增加遮罩定时退出功能，以及站立时间到达提醒功能







# Sedentary-reminder 久坐提醒小工具

## Repo rosters
[![Stargazers repo roster for @wjbgis/Sedentary-reminder](https://reporoster.com/stars/wjbgis/Sedentary-reminder)](https://github.com/wjbgis/Sedentary-reminder/stargazers)
[![Forkers repo roster for @wjbgis/Sedentary-reminder](https://reporoster.com/forks/wjbgis/Sedentary-reminder)](https://github.com/wjbgis/Sedentary-reminder/network/members)

## 下载  

[Download](https://github.com/wjbgis/Sedentary-reminder/releases)

## 介绍

![](https://github.com/wjbgis/Sedentary-reminder/blob/master/ScreenShot/0.png)

​	偶然看到人民日报公众号这篇文章，如果数字相对准确，那确实有点震惊。感觉自己明知久坐有害，但就是不自觉，电脑前一坐就是一上午、一下午。于是就想是否有一款软件能定时提醒自己不要久坐，网上搜寻了半天，感觉找到的软件都不能“完全阻止”我久坐的行为，那干脆自己写一个算了。

---

* 主界面很简单，也很丑，用的WinForm

  ![](https://github.com/wjbgis/Sedentary-reminder/blob/master/ScreenShot/1.png)

* 点击`开始`之后就开始倒计时

  ![](https://github.com/wjbgis/Sedentary-reminder/blob/master/ScreenShot/2.1.png)

* 工作倒计时剩余15秒时，提示用户即将锁定输入

  ![](https://github.com/wjbgis/Sedentary-reminder/blob/master/ScreenShot/4.png)

* 倒计时结束，显示遮罩层，屏蔽鼠标、键盘（为防止意外，未屏蔽`ctrl`+`alt`+`del`组合按键，可通过该组合键关机，但无法使用任务管理器）

  ![](https://github.com/wjbgis/Sedentary-reminder/blob/master/ScreenShot/3.png)
  
* 休息结束，继续开始工作倒计时

* **支持系统：Win7/10**



下面的是W新增的功能：

1、支持开机自启

2、启动时，自动开始倒计时，无需点击开始

3、支持暂停倒计时

4、倒计时支持延长时间

5、计划新增功能:到指定时间自动退出(我是强迫症患者，目的是想实现每天整时的自动启动，自动退出)

## 致谢

感谢 [netnr](https://github.com/netnr) 提的宝贵建议，让我有了继续更新的动力

