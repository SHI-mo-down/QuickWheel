# QuickWheel

一个简洁高效的 Windows 快捷轮盘工具，通过热键呼出圆形菜单，快速执行常用操作。

![QuickWheel](Resources/quickwheel.png)

## 功能特点

- 🎯 **快捷轮盘** - 按住热键呼出圆形菜单，鼠标移动选择，松手执行
- ⚡ **多种操作类型** - 支持打开文件、文件夹、网址、执行命令、发送快捷键
- 🎨 **高度自定义** - 可自定义轮盘颜色、图标、热键等
- 🖱️ **拖拽排序** - 支持拖拽快捷项进行排序
- 📝 **模板变量** - 支持 `{Clipboard}`、`{SelectedText}`、`{Date}`、`{Time}` 等变量
- 🌐 **多语言支持** - 支持中文和英文
- 🔔 **执行通知** - 可选的操作执行通知

## 安装

### Microsoft Store（推荐）

从 Microsoft Store 下载安装：

[![Microsoft Store](https://developer.microsoft.com/store/badges/images/Chinese_Simplified_get-it-from-MS.png)](https://apps.microsoft.com/detail/9NLGMPFBDJS5)

### 手动安装

1. 下载最新版本的 `.msix` 安装包
2. 双击安装包进行安装
3. 如果提示需要安装证书，请先安装证书

## 使用说明

### 基本操作

1. **呼出轮盘**：按下热键（默认 `Ctrl + Alt + Q`）
2. **选择项目**：鼠标移动到对应的扇区
3. **执行操作**：松开热键即可执行选中的快捷项

### 添加快捷项

1. 右键点击系统托盘图标，选择"设置"
2. 点击"添加"按钮
3. 设置名称、图标和目标路径
4. 点击"保存"

### 操作类型详解

#### 1. 打开文件 (OpenFile)
打开指定的可执行文件或文档。

**示例：**
- 名称: `记事本`
- 图标: `📝`
- 目标: `C:\Windows\System32\notepad.exe`

#### 2. 打开文件夹 (OpenFolder)
打开指定的文件夹。

**示例：**
- 名称: `下载`
- 图标: `📁`
- 目标: `C:\Users\YourName\Downloads`

#### 3. 打开网址 (OpenUrl)
使用默认浏览器打开网址。

**示例：**
- 名称: `Google`
- 图标: `🌐`
- 目标: `https://www.google.com`

#### 4. 执行命令 (ExecuteCommand)
执行任意命令行命令，支持参数。

**示例：**
- 名称: `搜索剪贴板`
- 图标: `🔍`
- 目标: `cmd`
- 参数: `/c start https://www.google.com/search?q={Clipboard}`

**功能说明：** 这个命令会读取当前剪贴板的内容，然后在 Google 上搜索。非常适合快速搜索复制的内容。

#### 5. 发送快捷键 (SendHotkey)
发送键盘快捷键到活动窗口。

**示例：**
- 名称: `复制`
- 图标: `📋`
- 快捷键: `Ctrl+C`

### 模板变量

在目标路径或参数中可以使用以下变量：

| 变量 | 说明 | 示例 |
|------|------|------|
| `{Clipboard}` | 剪贴板内容 | 搜索复制的文本 |
| `{SelectedText}` | 当前选中的文本 | 搜索选中的内容 |
| `{Date}` | 当前日期 (yyyy-MM-dd) | 2024-01-15 |
| `{Time}` | 当前时间 (HH:mm:ss) | 14:30:00 |
| `{DateTime}` | 当前日期时间 | 2024-01-15 14:30:00 |

### 实用示例

#### 示例 1：搜索剪贴板内容
快速搜索你复制到剪贴板的任何内容：
- **名称**: 搜索剪贴板
- **图标**: 🔍
- **操作类型**: ExecuteCommand
- **目标**: `cmd`
- **参数**: `/c start https://www.google.com/search?q={Clipboard}`

#### 示例 2：用记事本打开选中的文本
将选中的文本快速粘贴到记事本：
- **名称**: 记事本
- **图标**: 📝
- **操作类型**: ExecuteCommand
- **目标**: `cmd`
- **参数**: `/c echo {SelectedText} > %temp%\temp.txt && notepad %temp%\temp.txt`

#### 示例 3：创建带时间戳的文件
快速创建一个带当前时间戳的文本文件：
- **名称**: 新建笔记
- **图标**: 📄
- **操作类型**: ExecuteCommand
- **目标**: `cmd`
- **参数**: `/c notepad "笔记_{Date}.txt"`

## 系统要求

- Windows 10 版本 1809 或更高版本
- Windows 11

## 隐私政策

QuickWheel 是一款本地桌面应用程序，不收集任何用户数据。所有配置数据仅存储在用户的本地计算机上。

[隐私政策](PRIVACY.md)

## 开源协议

本项目采用 MIT 协议开源。

## 贡献

欢迎提交 Issue 和 Pull Request！

## 作者

- GitHub: [SHI-mo-down](https://github.com/SHI-mo-down)
- 仓库: https://github.com/SHI-mo-down/QuickWheel
