# 修复：休息遮罩布局错乱（RestFrm）— 第二版（真正根因）

## 现象
休息遮罩中倒计时显示为两个孤立的大数字（如「2」「5」），冒号几乎不可见，数字与屏幕左右边缘相距极远；
同时顶部提示、图标、小标题字号偏小，整体比例失衡。任何分辨率下都会出现，高 DPI 下更明显。

## 真正根因：pt 与 px 混算（上一版修改未触及）
1. `ThemeManager.Number(size)` 内部是 `new Font(..., size, ..., GraphicsUnit.Point)` —— **字号单位是 pt（点）**。
2. 旧 `LayoutTimeDisplay` 却把该数值当**像素**用：
   - 数字占位 `numW = fs × 1.55`（像素）
   - 行高 `lineH = fs × 1.3`（像素）
3. 1pt ≈ 1.333px，因此 **96 DPI 下字体实际就比框大 33%**；125% / 150% 缩放下放大到 1.67 / 2 倍。
   → 每个数字框装不下「02」「50」，`Label` 直接裁切，只剩「2」「5」；中间的冒号被两侧巨大文本挤到几乎看不见。
   → `TextAlign=MiddleCenter` 又把可见部分推到框中央，视觉上表现为「两个数字被拉到屏幕两端」。

> 这说明：**上一版把系数从 1.55 调到 1.85、加"宽度约束"都没用**，因为误差来源是单位换算，不是系数大小。

附带发现（同一次修复一并处理）：
4. **构图问题**：中间区原为「图标 50% / 倒计时 50%」双列。在非 16:9 或超宽屏上，倒计时整体偏右、图标偏远左，
   缩放因子再怎么调也无法居中——这是截图里图标偏左、标题偏右的直接原因。
5. **最大化不可靠**已在上版改为显式 `Bounds = 目标屏幕`，保留。

## 本次改动（仅 `Reminder/RestFrm.cs`）
1. **全部改用像素字号**：新增 `PxFont(family, pixelAt96, style)`，以 `GraphicsUnit.Pixel` 建字体，
   尺寸 = 96 DPI 基准像素 × `uiScale`。彻底消除 pt/px 换算误差（`lblText` 30px、`lblIcon` 120px、
   `lblTimerTitle` 26px、`lblHint` 20px，均按 1920×1080 基准）。
2. **倒计时改为自绘面板 `TimeDisplay`**：
   - 用 `TextRenderer.MeasureText` 对容器**实测宽高做二分查找**，取能放下的最大像素字号；
   - 整串 `"MM:SS"` 一次绘制（冒号是同一个字符串的一部分，不可能丢失）；
   - `NoPadding + Horizontal/VerticalCenter` 精确居中；
   - 上限 `MaxFontPx = 200 × uiScale`，防止大屏上过度膨胀；
   - 开启 `OptimizedDoubleBuffer | AllPaintingInWmPaint | ResizeRedraw`，秒针跳动不闪。
   - 原 `lbl_minutes / labelColon / lbl_seconds` 置为不可见（保留字段，避免改 Designer）。
3. **中间区重构为单列居中**：`RestructureCenterLayout()` 将 1×2 列改为 1 列 2 行
   （图标 34% / 标题+倒计时 66%），全部相对整屏居中——不再受左右 50% 分割影响。
   实现上先 `Controls.Clear()` 再收敛 `ColumnCount`，避免控件残留在被移除的列上。
4. 移除 `LayoutTimeDisplay()` 及其 `Resize` 订阅（自绘面板自带 `ResizeRedraw`，无需外部重排）。

## 验证要点（本机无 MSBuild，需 VS 重新生成）
1. 单屏 1080p / 1440p / 4K，缩放 100% / 125% / 150% / 250%：数字完整显示为「02:45」形式，冒号清晰。
2. 倒计时从「15:00」到「00:00」：数字宽度变化不引起位置跳动（整串居中）。
3. 双屏（不同分辨率/缩放）：每屏一个遮罩，精确覆盖，内容各自居中。
4. 相邻两轮工作—休息切换，遮罩每次都在正确屏幕、正确尺寸。

## 风险与回退
- 改动集中在 `RestFrm.cs`，未改 Designer、未改其它窗体；若自绘异常可回退本文件至 `3e46df1`。
- `TimeDisplay` 为 `RestFrm` 私有嵌套类，不对外暴露。
