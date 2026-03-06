using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace QuickWheel.Services
{
    /// <summary>
    /// 本地化服务，提供多语言支持
    /// </summary>
    public class LocalizationService : INotifyPropertyChanged
    {
        private static readonly Lazy<LocalizationService> _instance = new(() => new LocalizationService());
        public static LocalizationService Instance => _instance.Value;

        private string _currentLanguage = "zh-CN";
        private Dictionary<string, Dictionary<string, string>> _translations;

        public event PropertyChangedEventHandler? PropertyChanged;

        public string CurrentLanguage
        {
            get => _currentLanguage;
            set
            {
                if (_currentLanguage != value)
                {
                    _currentLanguage = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(CurrentCulture));
                    // 通知所有绑定属性更新
                    OnPropertyChanged(string.Empty);
                }
            }
        }

        public CultureInfo CurrentCulture => new CultureInfo(_currentLanguage);

        private LocalizationService()
        {
            _translations = new Dictionary<string, Dictionary<string, string>>
            {
                ["zh-CN"] = new Dictionary<string, string>
                {
                    // 通用
                    ["AppName"] = "QuickWheel",
                    ["Settings"] = "设置",
                    ["Save"] = "保存",
                    ["Cancel"] = "取消",
                    ["Add"] = "添加",
                    ["Delete"] = "删除",
                    ["Edit"] = "编辑",
                    ["Confirm"] = "确认",
                    ["Error"] = "错误",
                    ["Success"] = "成功",
                    ["Warning"] = "警告",
                    ["Info"] = "提示",
                    ["Help"] = "帮助",
                    ["Close"] = "关闭",
                    ["Yes"] = "是",
                    ["No"] = "否",
                    ["Import"] = "导入",
                    ["Export"] = "导出",
                    ["DragTip"] = "提示：拖拽可排序",

                    // 设置窗口
                    ["WindowTitle_Settings"] = "QuickWheel 设置",
                    
                    // 设置窗口标签
                    ["Tab_Shortcuts"] = "快捷项",
                    ["Tab_HotkeyAndWheel"] = "热键与轮盘",
                    ["Tab_About"] = "关于",

                    // DataGrid列头
                    ["Col_Icon"] = "图标",
                    ["Col_Name"] = "名称",
                    ["Col_Type"] = "类型",
                    ["Col_Target"] = "目标路径",

                    // 按钮
                    ["Btn_Add"] = "添加",
                    ["Btn_Delete"] = "删除",
                    ["Btn_Import"] = "导入",
                    ["Btn_Export"] = "导出",
                    ["Btn_Help"] = "帮助",
                    ["Btn_Reset"] = "重置",
                    ["Btn_Clear"] = "清除",

                    // 动作类型
                    ["Action_OpenFile"] = "打开文件",
                    ["Action_OpenUrl"] = "打开网址",
                    ["Action_OpenFolder"] = "打开文件夹",
                    ["Action_ExecuteCommand"] = "执行命令",
                    ["Action_Shortcut"] = "快捷键",
                    ["Action_Auto"] = "自动",

                    // 热键设置
                    ["Hotkey_Current"] = "热键:",
                    ["Hotkey_Record"] = "修改热键",
                    ["Hotkey_Recording"] = "按组合键录制...",

                    // 通用设置
                    ["Setting_StartWithWindows"] = "开机自动启动",
                    ["Setting_EnableNotifications"] = "启用执行通知",
                    ["Setting_Language"] = "语言/Language:",

                    // 轮盘设置
                    ["Wheel_Colors"] = "轮盘颜色",
                    ["Wheel_ColorSector"] = "扇区:",
                    ["Wheel_ColorHighlight"] = "高亮:",
                    ["Wheel_ColorText"] = "文字:",
                    ["Wheel_InnerRadius"] = "镂空:",
                    ["Wheel_InnerRadiusHint"] = "10% - 70%",
                    ["Wheel_TextSize"] = "文字大小:",
                    ["Wheel_TextSizeHint"] = "6 - 24",

                    // 关于页面
                    ["About_Title"] = "QuickWheel",
                    ["About_Subtitle"] = "快捷轮盘工具",
                    ["About_Version"] = "版本: 1.0.0",
                    ["About_Author"] = "作者: Your Name",
                    ["About_Github"] = "GitHub: https://github.com/SHI-mo-down/QuickWheel",
                    ["About_DataManagement"] = "数据管理",
                    ["About_ResetConfig"] = "重置配置",
                    ["About_ResetConfig_Desc"] = "将所有设置恢复为默认值，但不会删除快捷项",
                    ["About_ClearData"] = "清除所有数据",
                    ["About_ClearData_Desc"] = "删除所有配置和快捷项数据，此操作不可撤销",
                    ["About_License"] = "开源协议",
                    ["About_LicenseType"] = "MIT License",

                    // 底部
                    ["Footer_Version"] = "v1.0.0",

                    // 确认对话框
                    ["Confirm_ResetConfig_Title"] = "确认重置",
                    ["Confirm_ResetConfig_Msg"] = "确定要将所有设置恢复为默认值吗？\n\n快捷项数据不会被删除。",
                    ["Confirm_ClearData_Title"] = "确认清除",
                    ["Confirm_ClearData_Msg"] = "确定要删除所有数据吗？\n\n这将删除所有配置和快捷项，此操作不可撤销！",
                    ["Confirm_ClearData_Final_Title"] = "最终确认",
                    ["Confirm_ClearData_Final_Msg"] = "再次确认：所有数据将被永久删除！\n\n确定要继续吗？",

                    // 提示信息
                    ["Msg_ConfigReset_Title"] = "完成",
                    ["Msg_ConfigReset_Msg"] = "配置已重置为默认值",
                    ["Msg_DataCleared_Title"] = "完成",
                    ["Msg_DataCleared_Msg"] = "所有数据已清除。程序将关闭，下次启动时将创建新的默认配置。",
                    ["Msg_Error_Title"] = "错误",

                    // 托盘菜单
                    ["Tray_Tooltip"] = "QuickWheel - 快捷轮盘",
                    ["Tray_Settings"] = "设置",
                    ["Tray_Exit"] = "退出",

                    // 新建快捷项默认名称
                    ["NewShortcut_DefaultName"] = "新建快捷项",

                    // 对话框
                    ["Dialog_Import_Title"] = "导入配置",
                    ["Dialog_Import_Filter"] = "JSON配置文件|*.json|所有文件|*.*",
                    ["Dialog_Export_Title"] = "导出配置",
                    ["Dialog_Export_Filter"] = "JSON配置文件|*.json|所有文件|*.*",

                    // 消息提示
                    ["Msg_ImportSuccess_Title"] = "成功",
                    ["Msg_ImportSuccess_Msg"] = "配置导入成功！",
                    ["Msg_ImportFailed_Msg"] = "配置导入失败，请检查文件格式。",
                    ["Msg_ExportSuccess_Title"] = "成功",
                    ["Msg_ExportSuccess_Msg"] = "配置已导出到：\n{0}",
                    ["Msg_ExportFailed_Msg"] = "配置导出失败。",
                    ["Msg_ReloadIcons_Title"] = "完成",
                    ["Msg_ReloadIcons_Msg"] = "图标重新加载完成！\n\n已清除: {0} 个\n已加载: {1} 个",
                    ["Msg_ReloadIconsError_Msg"] = "重新加载图标时出错：{0}",
                    ["Msg_ConfigResetError_Msg"] = "重置配置时出错：{0}",
                    ["Msg_ClearData_Title"] = "完成",
                    ["Msg_ClearData_Msg"] = "所有数据已清除。程序将关闭，下次启动时将创建新的默认配置。",
                    ["Msg_ClearDataError_Msg"] = "清除数据时出错：{0}",

                    // 按钮
                    ["Btn_OK"] = "确定",
                    ["Btn_Cancel"] = "取消",

                    // 帮助窗口
                    ["WindowTitle_Help"] = "帮助",
                    ["Help_UsageTitle"] = "使用说明",
                    ["Help_Usage1"] = "1. 按热键（默认 Ctrl+Alt+Q）呼出轮盘",
                    ["Help_Usage2"] = "2. 鼠标移动到对应扇区，松开按键执行",
                    ["Help_Usage3"] = "3. 在设置中可以添加、删除、排序快捷项",
                    ["Help_Usage4"] = "4. 支持拖拽快捷项进行排序",
                    ["Help_Usage5"] = "5. 支持打开文件、文件夹、网址、执行命令、发送快捷键",
                    ["Help_VariablesTitle"] = "模板变量",
                    ["Help_VarClipboard"] = "{} {Clipboard} - 剪贴板内容",
                    ["Help_VarSelectedText"] = "{} {SelectedText} - 选中的文本",
                    ["Help_VarDate"] = "{} {Date} - 当前日期",
                    ["Help_VarTime"] = "{} {Time} - 当前时间",
                    ["Help_IconsTitle"] = "图标说明",
                    ["Help_IconAuto"] = "🤖 - 自动匹配图标（根据目标类型）",
                    ["Help_IconManual"] = "📄📁🌐 等 - 手动选择 Emoji 图标",

                    // 颜色选择器
                    ["WindowTitle_ColorPicker"] = "选择颜色",
                    ["ColorPicker_Title"] = "选择颜色",
                    ["ColorPicker_ClickHint"] = "点击选择颜色",
                    ["ColorPicker_Current"] = "当前选择:",

                    // 输入对话框
                    ["InputDialog_OK"] = "确定",
                    ["InputDialog_Cancel"] = "取消",

                    // 轮盘窗口
                    ["RadialMenu_Title"] = "QuickWheel RadialMenu",
                },
                ["en-US"] = new Dictionary<string, string>
                {
                    // General
                    ["AppName"] = "QuickWheel",
                    ["Settings"] = "Settings",
                    ["Save"] = "Save",
                    ["Cancel"] = "Cancel",
                    ["Add"] = "Add",
                    ["Delete"] = "Delete",
                    ["Edit"] = "Edit",
                    ["Confirm"] = "Confirm",
                    ["Error"] = "Error",
                    ["Success"] = "Success",
                    ["Warning"] = "Warning",
                    ["Info"] = "Information",
                    ["Help"] = "Help",
                    ["Close"] = "Close",
                    ["Yes"] = "Yes",
                    ["No"] = "No",
                    ["Import"] = "Import",
                    ["Export"] = "Export",
                    ["DragTip"] = "Tip: Drag to reorder",

                    // Settings Window
                    ["WindowTitle_Settings"] = "QuickWheel Settings",
                    
                    // Settings Tabs
                    ["Tab_Shortcuts"] = "Shortcuts",
                    ["Tab_HotkeyAndWheel"] = "Hotkey & Wheel",
                    ["Tab_About"] = "About",

                    // DataGrid Columns
                    ["Col_Icon"] = "Icon",
                    ["Col_Name"] = "Name",
                    ["Col_Type"] = "Type",
                    ["Col_Target"] = "Target Path",

                    // Buttons
                    ["Btn_Add"] = "Add",
                    ["Btn_Delete"] = "Delete",
                    ["Btn_Import"] = "Import",
                    ["Btn_Export"] = "Export",
                    ["Btn_Help"] = "Help",
                    ["Btn_Reset"] = "Reset",
                    ["Btn_Clear"] = "Clear",

                    // Action Types
                    ["Action_OpenFile"] = "Open File",
                    ["Action_OpenUrl"] = "Open URL",
                    ["Action_OpenFolder"] = "Open Folder",
                    ["Action_ExecuteCommand"] = "Execute Command",
                    ["Action_Shortcut"] = "Shortcut Key",
                    ["Action_Auto"] = "Auto",

                    // Hotkey
                    ["Hotkey_Current"] = "Hotkey:",
                    ["Hotkey_Record"] = "Change Hotkey",
                    ["Hotkey_Recording"] = "Press key combination...",

                    // General Settings
                    ["Setting_StartWithWindows"] = "Start with Windows",
                    ["Setting_EnableNotifications"] = "Enable Notifications",
                    ["Setting_Language"] = "Language/语言:",

                    // Wheel Settings
                    ["Wheel_Colors"] = "Wheel Colors",
                    ["Wheel_ColorSector"] = "Sector:",
                    ["Wheel_ColorHighlight"] = "Highlight:",
                    ["Wheel_ColorText"] = "Text:",
                    ["Wheel_InnerRadius"] = "Inner:",
                    ["Wheel_InnerRadiusHint"] = "10% - 70%",
                    ["Wheel_TextSize"] = "Text Size:",
                    ["Wheel_TextSizeHint"] = "6 - 24",

                    // About
                    ["About_Title"] = "QuickWheel",
                    ["About_Subtitle"] = "Quick Wheel Tool",
                    ["About_Version"] = "Version: 1.0.0",
                    ["About_Author"] = "Author: Your Name",
                    ["About_Github"] = "GitHub: https://github.com/SHI-mo-down/QuickWheel",
                    ["About_DataManagement"] = "Data Management",
                    ["About_ResetConfig"] = "Reset Settings",
                    ["About_ResetConfig_Desc"] = "Reset all settings to default values without deleting shortcuts",
                    ["About_ClearData"] = "Clear All Data",
                    ["About_ClearData_Desc"] = "Delete all settings and shortcuts. This action cannot be undone",
                    ["About_License"] = "License",
                    ["About_LicenseType"] = "MIT License",

                    // Footer
                    ["Footer_Version"] = "v1.0.0",

                    // Confirmations
                    ["Confirm_ResetConfig_Title"] = "Confirm Reset",
                    ["Confirm_ResetConfig_Msg"] = "Are you sure you want to reset all settings to default values?\n\nShortcut data will not be deleted.",
                    ["Confirm_ClearData_Title"] = "Confirm Clear",
                    ["Confirm_ClearData_Msg"] = "Are you sure you want to delete all data?\n\nThis will delete all settings and shortcuts. This action cannot be undone!",
                    ["Confirm_ClearData_Final_Title"] = "Final Confirmation",
                    ["Confirm_ClearData_Final_Msg"] = "Final confirmation: All data will be permanently deleted!\n\nDo you want to continue?",

                    // Messages
                    ["Msg_ConfigReset_Title"] = "Complete",
                    ["Msg_ConfigReset_Msg"] = "Settings have been reset to default values",
                    ["Msg_DataCleared_Title"] = "Complete",
                    ["Msg_DataCleared_Msg"] = "All data has been cleared. The application will close and create new default settings on next startup.",
                    ["Msg_Error_Title"] = "Error",

                    // Tray
                    ["Tray_Tooltip"] = "QuickWheel - Quick Wheel",
                    ["Tray_Settings"] = "Settings",
                    ["Tray_Exit"] = "Exit",

                    // New shortcut default name
                    ["NewShortcut_DefaultName"] = "New Shortcut",

                    // Dialogs
                    ["Dialog_Import_Title"] = "Import Configuration",
                    ["Dialog_Import_Filter"] = "JSON Config Files|*.json|All Files|*.*",
                    ["Dialog_Export_Title"] = "Export Configuration",
                    ["Dialog_Export_Filter"] = "JSON Config Files|*.json|All Files|*.*",

                    // Messages
                    ["Msg_ImportSuccess_Title"] = "Success",
                    ["Msg_ImportSuccess_Msg"] = "Configuration imported successfully!",
                    ["Msg_ImportFailed_Msg"] = "Failed to import configuration. Please check the file format.",
                    ["Msg_ExportSuccess_Title"] = "Success",
                    ["Msg_ExportSuccess_Msg"] = "Configuration exported to:\n{0}",
                    ["Msg_ExportFailed_Msg"] = "Failed to export configuration.",
                    ["Msg_ReloadIcons_Title"] = "Complete",
                    ["Msg_ReloadIcons_Msg"] = "Icons reloaded!\n\nCleared: {0}\nReloaded: {1}",
                    ["Msg_ReloadIconsError_Msg"] = "Error reloading icons: {0}",
                    ["Msg_ConfigResetError_Msg"] = "Error resetting configuration: {0}",
                    ["Msg_ClearData_Title"] = "Complete",
                    ["Msg_ClearData_Msg"] = "All data cleared. Application will close and create new default settings on next startup.",
                    ["Msg_ClearDataError_Msg"] = "Error clearing data: {0}",

                    // Buttons
                    ["Btn_OK"] = "OK",
                    ["Btn_Cancel"] = "Cancel",

                    // Help Window
                    ["WindowTitle_Help"] = "Help",
                    ["Help_UsageTitle"] = "Usage Instructions",
                    ["Help_Usage1"] = "1. Press hotkey (default Ctrl+Alt+Q) to show the wheel",
                    ["Help_Usage2"] = "2. Move mouse to the desired sector, release key to execute",
                    ["Help_Usage3"] = "3. Add, delete, and reorder shortcuts in Settings",
                    ["Help_Usage4"] = "4. Drag shortcuts to reorder them",
                    ["Help_Usage5"] = "5. Supports opening files, folders, URLs, executing commands, and sending shortcuts",
                    ["Help_VariablesTitle"] = "Template Variables",
                    ["Help_VarClipboard"] = "{} {Clipboard} - Clipboard content",
                    ["Help_VarSelectedText"] = "{} {SelectedText} - Selected text",
                    ["Help_VarDate"] = "{} {Date} - Current date",
                    ["Help_VarTime"] = "{} {Time} - Current time",
                    ["Help_IconsTitle"] = "Icon Description",
                    ["Help_IconAuto"] = "🤖 - Auto-match icon (based on target type)",
                    ["Help_IconManual"] = "📄📁🌐 etc. - Manually select Emoji icon",

                    // Color Picker
                    ["WindowTitle_ColorPicker"] = "Select Color",
                    ["ColorPicker_Title"] = "Select Color",
                    ["ColorPicker_ClickHint"] = "Click to select color",
                    ["ColorPicker_Current"] = "Current:",

                    // Input Dialog
                    ["InputDialog_OK"] = "OK",
                    ["InputDialog_Cancel"] = "Cancel",

                    // Radial Menu
                    ["RadialMenu_Title"] = "QuickWheel RadialMenu",
                }
            };
        }

        /// <summary>
        /// 获取本地化字符串
        /// </summary>
        public string this[string key]
        {
            get
            {
                if (_translations.TryGetValue(_currentLanguage, out var dict))
                {
                    if (dict.TryGetValue(key, out var value))
                    {
                        return value;
                    }
                }
                // 回退到英文
                if (_translations.TryGetValue("en-US", out var enDict))
                {
                    if (enDict.TryGetValue(key, out var enValue))
                    {
                        return enValue;
                    }
                }
                return key;
            }
        }

        /// <summary>
        /// 获取本地化字符串（带格式化）
        /// </summary>
        public string GetString(string key, params object[] args)
        {
            string format = this[key];
            return string.Format(format, args);
        }

        /// <summary>
        /// 获取动作类型的本地化名称
        /// </summary>
        public string GetActionTypeName(Models.ActionType actionType)
        {
            return actionType switch
            {
                Models.ActionType.OpenFile => this["Action_OpenFile"],
                Models.ActionType.OpenUrl => this["Action_OpenUrl"],
                Models.ActionType.OpenFolder => this["Action_OpenFolder"],
                Models.ActionType.ExecuteCommand => this["Action_ExecuteCommand"],
                Models.ActionType.Shortcut => this["Action_Shortcut"],
                _ => actionType.ToString()
            };
        }

        /// <summary>
        /// 切换语言
        /// </summary>
        public void ChangeLanguage(string languageCode)
        {
            if (_translations.ContainsKey(languageCode))
            {
                CurrentLanguage = languageCode;
                
                // 保存到配置
                var config = JsonDataService.Instance.Config;
                config.Language = languageCode;
                JsonDataService.Instance.SaveConfig();
            }
        }

        /// <summary>
        /// 从配置加载语言设置
        /// </summary>
        public void LoadLanguageFromConfig()
        {
            var config = JsonDataService.Instance.Config;
            if (!string.IsNullOrEmpty(config.Language) && _translations.ContainsKey(config.Language))
            {
                CurrentLanguage = config.Language;
            }
        }

        /// <summary>
        /// 获取支持的语言列表
        /// </summary>
        public IEnumerable<string> SupportedLanguages => _translations.Keys;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
