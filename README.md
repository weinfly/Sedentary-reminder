# Sedentary Reminder 久坐提醒工具

> 克隆自 https://github.com/wjbgis/Sedentary-reminder，进行了大量优化和增强。

## ✨ 功能特性

### 核心功能
- ⏱️ **定时倒计时** - 可配置工作时间和休息时间
- 🔒 **输入锁定** - 休息时可锁定键盘和鼠标（需要管理员权限）
- 🖥️ **多显示器支持** - 支持自定义倒计时窗口显示位置和显示器
- 🔔 **温馨提醒** - 倒计时结束前15秒提醒，倒计时结束后全屏遮罩

### 调度服务（新增）
- ⚙️ **Windows Service** - 可作为系统服务运行，自动定时启动/停止
- 📅 **智能调度** - 支持配置多个工作时间段，周末自动跳过
- 🔄 **整点对齐** - 倒计时基于整点计算，精确提醒

### UI 优化（新增）
- 🎨 **现代化界面** - Material Design 风格配色
- 📱 **响应式布局** - 使用 TableLayoutPanel 自适应不同分辨率
- ✨ **Emoji 图标** - 直观的视觉提示

## 📋 系统要求

- Windows 7/10/11
- .NET Framework 4.8

## 🚀 快速开始

### 方式一：直接运行（推荐日常使用）

1. 运行 `Reminder.exe`
2. 程序会自动最小化到系统托盘
3. 右键托盘图标 → "打开设置" 配置参数
4. 点击 "开始计时" 启动倒计时

### 方式二：安装为 Windows Service（推荐自动调度）

1. **以管理员身份**运行 `Scheduler\install_service.bat`
2. 选择 "1. 安装服务"
3. 选择 "3. 启动服务"
4. 服务会自动在工作时间段启动倒计时

**卸载服务**：运行同样的脚本，选择 "2. 卸载服务"

## ⚙️ 配置说明

编辑 `Reminder\app.config` 或 `Scheduler\app.config`：

```xml
<appSettings>
    <!-- 工作时间（分钟），默认45 -->
    <add key="WorkTimeValue" value="45"/>
    
    <!-- 休息时间（分钟），默认15 -->
    <add key="RestTimeValue" value="15"/>
    
    <!-- 
      工作时间段配置，支持两种格式：
      
      格式1（简单）：使用 AutoStartHours 和 AutoStopHours
      例如：9,13 和 12,18 表示 9:00-12:00 和 13:00-18:00
      
      格式2（精确到分钟）：使用 WorkPeriods
      格式：开始小时:开始分钟-结束小时:结束分钟
      例如：9:0-12:0,13:0-16:30 表示 9:00-12:00 和 13:00-16:30
    -->
    <add key="AutoStartHours" value="9,13"/>
    <add key="AutoStopHours" value="12,18"/>
    
    <!-- 精确格式（优先使用此配置） -->
    <!-- <add key="WorkPeriods" value="9:0-12:0,13:0-16:0"/> -->
    
    <!-- 多显示器设置 -->
    <!-- WorkFormScreen: 0=第一个显示器, 1=第二个显示器 -->
    <add key="WorkFormScreen" value="0"/>
    
    <!-- 倒计时窗口位置偏移（从右下角） -->
    <add key="WorkFormOffsetX" value="160"/>
    <add key="WorkFormOffsetY" value="90"/>
</appSettings>
```

## 📁 项目结构

```
├── Reminder/                    # 客户端 UI 程序
│   ├── MainFrm.cs              # 主设置窗口
│   ├── WorkFrm.cs              # 工作倒计时窗口
│   ├── RestFrm.cs              # 休息遮罩窗口
│   ├── KeyboardBlocker.cs      # 键盘鼠标锁定
│   ├── Program.cs              # 程序入口
│   └── app.config              # 配置文件
│
├── Scheduler/                   # Windows Service 调度服务
│   ├── SchedulerService.cs     # 服务核心逻辑
│   ├── ScheduleConfig.cs       # 调度配置管理
│   ├── ClientLauncher.cs       # 客户端启动器（Session 切换）
│   ├── SchedulerInstaller.cs   # 服务安装程序
│   ├── Program.cs              # 服务入口（支持控制台调试模式）
│   ├── app.config              # 服务配置文件
│   └── install_service.bat     # 安装/卸载脚本
│
└── Reminder.sln                # Visual Studio 解决方案
```

## 🔧 开发说明

### 构建项目

```bash
# 使用 dotnet CLI 构建
dotnet build Reminder.sln

# 或使用 Visual Studio 打开 Reminder.sln 构建
```

### 调试调度服务

```bash
# 以控制台模式运行调度服务（便于调试）
Scheduler\SedentaryReminder.Scheduler.exe --console
```

## 📸 界面预览

### 设置界面
- Material Design 绿色主题
- 清晰的工作/休息时间配置
- 一键锁定选项开关

### 工作倒计时
- 简洁的悬浮窗口设计
- 大字体时间显示
- 暂停/推迟按钮

### 休息遮罩
- 全屏半透明遮罩
- 运动图标提示
- 解锁倒计时

## 📝 更新日志

### v2.0 (2024)
- ✨ 新增 Windows Service 调度服务
- ✨ 支持多工作时间段配置
- ✨ 周末自动跳过
- 🎨 UI 全面美化，Material Design 风格
- 🐛 修复推迟功能 Bug
- 🧹 清理废弃代码

### v1.0 (2019)
- 初始版本
- 基本倒计时功能
- 键盘鼠标锁定

## 🙏 致谢

感谢 [wjbgis/Sedentary-reminder](https://github.com/wjbgis/Sedentary-reminder) 提供的原始代码。

## 📄 License

MIT