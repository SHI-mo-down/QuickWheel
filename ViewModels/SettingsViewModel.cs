using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Win32;
using QuickWheel.Models;
using QuickWheel.Services;

namespace QuickWheel.ViewModels
{
    public class SettingsViewModel : INotifyPropertyChanged
    {
        private ObservableCollection<ShortcutItemViewModel> _shortcuts;
        private string _hotkeyDisplay = "Ctrl + Alt + Q";
        private bool _isRecordingHotkey;
        private bool _startWithWindows;
        private bool _enableNotifications;
        private ShortcutItemViewModel? _selectedShortcut;

        // 颜色设置
        private string _sectorColor = "#CC333333";
        private string _highlightColor = "#FF444444";
        private string _textColor = "#FFFFFFFF";

        // 镂空大小比例 (0.0 - 1.0)
        private double _innerRadiusRatio = 0.35;

        // 中央文字大小
        private double _centerTextSize = 10;

        // 当前语言
        private string _selectedLanguage = "zh-CN";

        // 可用的图标列表（自动 + 常用 Emoji）
        public static readonly string[] AvailableIcons = new[]
        {
            "🤖",  // 自动
            "📄", "📁", "🌐", "⚙️", "💻", "🎨", "🎵", "🎬", "🎮", "📧",
            "🔍", "⚡", "🔧", "", "⭐", "🔥", "❤️", "✅", "❌"
        };

        public event PropertyChangedEventHandler? PropertyChanged;
        public event EventHandler? RequestClose;

        public ObservableCollection<ShortcutItemViewModel> Shortcuts
        {
            get => _shortcuts;
            set { _shortcuts = value; OnPropertyChanged(); }
        }

        public ShortcutItemViewModel? SelectedShortcut
        {
            get => _selectedShortcut;
            set { _selectedShortcut = value; OnPropertyChanged(); }
        }

        public string HotkeyDisplay
        {
            get => _hotkeyDisplay;
            set { _hotkeyDisplay = value; OnPropertyChanged(); }
        }

        public bool IsRecordingHotkey
        {
            get => _isRecordingHotkey;
            set 
            { 
                _isRecordingHotkey = value; 
                OnPropertyChanged();
                OnPropertyChanged(nameof(RecordButtonText));
                OnPropertyChanged(nameof(RecordButtonBrush));
            }
        }

        public string RecordButtonText => IsRecordingHotkey
            ? LocalizationService.Instance["Hotkey_Recording"]
            : LocalizationService.Instance["Hotkey_Record"];
        
        public Brush RecordButtonBrush => IsRecordingHotkey
            ? new SolidColorBrush(Color.FromRgb(244, 67, 54))  // Red when recording
            : new SolidColorBrush(Color.FromRgb(33, 150, 243)); // Blue when idle

        public bool StartWithWindows
        {
            get => _startWithWindows;
            set { _startWithWindows = value; OnPropertyChanged(); }
        }

        public bool EnableNotifications
        {
            get => _enableNotifications;
            set { _enableNotifications = value; OnPropertyChanged(); }
        }

        // 颜色属性
        public string SectorColor
        {
            get => _sectorColor;
            set
            {
                if (_sectorColor != value)
                {
                    _sectorColor = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(SectorColorBrush));
                }
            }
        }

        public string HighlightColor
        {
            get => _highlightColor;
            set
            {
                if (_highlightColor != value)
                {
                    _highlightColor = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(HighlightColorBrush));
                }
            }
        }

        public string TextColor
        {
            get => _textColor;
            set
            {
                if (_textColor != value)
                {
                    _textColor = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(TextColorBrush));
                }
            }
        }

        public Brush SectorColorBrush => ParseColorBrush(_sectorColor);
        public Brush HighlightColorBrush => ParseColorBrush(_highlightColor);
        public Brush TextColorBrush => ParseColorBrush(_textColor);

        // 镂空大小比例属性
        public double InnerRadiusRatio
        {
            get => _innerRadiusRatio;
            set
            {
                // 限制在 0.0 - 0.8 范围内
                if (value < 0.0) value = 0.0;
                if (value > 0.8) value = 0.8;
                _innerRadiusRatio = value;
                OnPropertyChanged();
            }
        }

        // 中央文字大小属性
        public double CenterTextSize
        {
            get => _centerTextSize;
            set
            {
                // 限制在 6 - 24 范围内
                if (value < 6) value = 6;
                if (value > 24) value = 24;
                _centerTextSize = value;
                OnPropertyChanged();
            }
        }

        // 支持的语言列表
        public IEnumerable<string> SupportedLanguages => LocalizationService.Instance.SupportedLanguages;

        // 当前语言
        public string SelectedLanguage
        {
            get => _selectedLanguage;
            set
            {
                if (_selectedLanguage != value)
                {
                    _selectedLanguage = value;
                    OnPropertyChanged();
                    // 切换语言
                    LocalizationService.Instance.ChangeLanguage(value);
                }
            }
        }

        private Brush ParseColorBrush(string colorString)
        {
            try
            {
                if (colorString.StartsWith("#"))
                {
                    return (Brush)new BrushConverter().ConvertFromString(colorString)!;
                }
            }
            catch { }
            return Brushes.Gray;
        }

        public ICommand AddShortcutCommand { get; }
        public ICommand DeleteShortcutCommand { get; }
        public ICommand MoveUpCommand { get; }
        public ICommand MoveDownCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }
        public ICommand ImportConfigCommand { get; }
        public ICommand ExportConfigCommand { get; }
        public ICommand StartRecordingCommand { get; }
        public ICommand SelectSectorColorCommand { get; }
        public ICommand SelectHighlightColorCommand { get; }
        public ICommand SelectTextColorCommand { get; }
        public ICommand ReloadIconsCommand { get; }
        public ICommand ResetConfigCommand { get; }
        public ICommand ClearAllDataCommand { get; }

        public SettingsViewModel()
        {
            _shortcuts = new ObservableCollection<ShortcutItemViewModel>();
            AddShortcutCommand = new RelayCommand(AddShortcut);
            DeleteShortcutCommand = new RelayCommand(DeleteShortcut);
            MoveUpCommand = new RelayCommand(MoveUp);
            MoveDownCommand = new RelayCommand(MoveDown);
            SaveCommand = new RelayCommand(Save);
            CancelCommand = new RelayCommand(Cancel);
            ImportConfigCommand = new RelayCommand(ImportConfig);
            ExportConfigCommand = new RelayCommand(ExportConfig);
            StartRecordingCommand = new RelayCommand(StartRecording);
            SelectSectorColorCommand = new RelayCommand(() => SelectColor("Sector"));
            SelectHighlightColorCommand = new RelayCommand(() => SelectColor("Highlight"));
            SelectTextColorCommand = new RelayCommand(() => SelectColor("Text"));
            ReloadIconsCommand = new RelayCommand(ReloadIcons);
            ResetConfigCommand = new RelayCommand(ResetConfig);
            ClearAllDataCommand = new RelayCommand(ClearAllData);
        }

        private void SelectColor(string colorType)
        {
            // 获取当前颜色
            string currentColor = colorType switch
            {
                "Sector" => SectorColor,
                "Highlight" => HighlightColor,
                "Text" => TextColor,
                _ => "#FFFFFFFF"
            };

            // 显示颜色选择器
            var colorPicker = new Views.ColorPickerDialog(currentColor);
            if (colorPicker.ShowDialog() == true)
            {
                string colorString = colorPicker.SelectedColor;
                
                switch (colorType)
                {
                    case "Sector":
                        SectorColor = colorString;
                        break;
                    case "Highlight":
                        HighlightColor = colorString;
                        break;
                    case "Text":
                        TextColor = colorString;
                        break;
                }
            }
        }

        private void StartRecording()
        {
            IsRecordingHotkey = true;
        }

        public void LoadFromConfig(AppConfig config)
        {
            HotkeyDisplay = $"{config.Hotkey.ModifierKeys.Replace("+", " + ")} + {config.Hotkey.Key}";
            StartWithWindows = config.StartWithWindows;
            EnableNotifications = config.EnableNotifications;

            // 加载语言设置
            SelectedLanguage = config.Language;
            LocalizationService.Instance.LoadLanguageFromConfig();

            // 加载颜色设置
            SectorColor = config.Wheel.SectorColor;
            HighlightColor = config.Wheel.HighlightColor;
            TextColor = config.Wheel.TextColor;

            // 加载镂空大小比例
            InnerRadiusRatio = config.Wheel.InnerRadiusRatio;

            // 加载中央文字大小
            CenterTextSize = config.Wheel.CenterTextSize;

            Shortcuts.Clear();
            foreach (var item in config.Shortcuts.OrderBy(s => s.Order))
            {
                // 判断是否使用自动图标（CustomIcon 为空或等于🤖表示自动）
                bool isAutoIcon = string.IsNullOrEmpty(item.CustomIcon) || item.CustomIcon == "🤖";
                
                // ComboBox 显示的值：自动时用🤖，否则用自定义图标
                string icon = isAutoIcon ? "🤖" : item.CustomIcon;

                Shortcuts.Add(new ShortcutItemViewModel
                {
                    Id = item.Id,
                    Name = item.Name,
                    Icon = icon,
                    IsAutoIcon = isAutoIcon,
                    ActionType = item.ActionType,
                    Target = item.Target,
                    Arguments = item.Arguments,
                    Order = item.Order
                });
            }
        }

        public void UpdateHotkey(string modifier, string key)
        {
            var config = JsonDataService.Instance.Config;
            config.Hotkey.ModifierKeys = modifier;
            config.Hotkey.Key = key;
            HotkeyDisplay = $"{modifier.Replace("+", " + ")} + {key}";
            
            JsonDataService.Instance.SaveConfig();
            JsonDataService.Instance.NotifyConfigChanged();
        }

        private void AddShortcut()
        {
            var item = new ShortcutItemViewModel
            {
                Id = Guid.NewGuid().ToString(),
                Name = LocalizationService.Instance["NewShortcut_DefaultName"],
                Icon = "🤖",  // 默认使用自动图标
                IsAutoIcon = true,
                Order = Shortcuts.Count
            };
            Shortcuts.Add(item);
            SelectedShortcut = item;
        }

        private void DeleteShortcut()
        {
            if (SelectedShortcut != null)
            {
                Shortcuts.Remove(SelectedShortcut);
                SelectedShortcut = null;
            }
        }

        private void MoveUp()
        {
            if (SelectedShortcut == null) return;
            var index = Shortcuts.IndexOf(SelectedShortcut);
            if (index > 0)
            {
                Shortcuts.Move(index, index - 1);
                // 更新 Order
                for (int i = 0; i < Shortcuts.Count; i++)
                {
                    Shortcuts[i].Order = i;
                }
            }
        }

        private void MoveDown()
        {
            if (SelectedShortcut == null) return;
            var index = Shortcuts.IndexOf(SelectedShortcut);
            if (index < Shortcuts.Count - 1)
            {
                Shortcuts.Move(index, index + 1);
                // 更新 Order
                for (int i = 0; i < Shortcuts.Count; i++)
                {
                    Shortcuts[i].Order = i;
                }
            }
        }

        /// <summary>
        /// 拖拽移动快捷项到指定位置
        /// </summary>
        public void MoveShortcut(int fromIndex, int toIndex)
        {
            if (fromIndex < 0 || fromIndex >= Shortcuts.Count)
                return;
            if (toIndex < 0 || toIndex >= Shortcuts.Count)
                return;
            if (fromIndex == toIndex)
                return;

            // 移动项目
            Shortcuts.Move(fromIndex, toIndex);

            // 更新 Order
            for (int i = 0; i < Shortcuts.Count; i++)
            {
                Shortcuts[i].Order = i;
            }

            // 更新选中项
            SelectedShortcut = Shortcuts[toIndex];
        }

        private void ImportConfig()
        {
            var dialog = new OpenFileDialog
            {
                Title = LocalizationService.Instance["Dialog_Import_Title"],
                Filter = LocalizationService.Instance["Dialog_Import_Filter"],
                DefaultExt = "json"
            };

            if (dialog.ShowDialog() == true)
            {
                if (JsonDataService.Instance.ImportConfig(dialog.FileName))
                {
                    // 重新加载配置
                    LoadFromConfig(JsonDataService.Instance.Config);
                    System.Windows.MessageBox.Show(
                        LocalizationService.Instance["Msg_ImportSuccess_Msg"],
                        LocalizationService.Instance["Msg_ImportSuccess_Title"],
                        System.Windows.MessageBoxButton.OK,
                        System.Windows.MessageBoxImage.Information);
                }
                else
                {
                    System.Windows.MessageBox.Show(
                        LocalizationService.Instance["Msg_ImportFailed_Msg"],
                        LocalizationService.Instance["Msg_Error_Title"],
                        System.Windows.MessageBoxButton.OK,
                        System.Windows.MessageBoxImage.Error);
                }
            }
        }

        private void ExportConfig()
        {
            var dialog = new SaveFileDialog
            {
                Title = LocalizationService.Instance["Dialog_Export_Title"],
                Filter = LocalizationService.Instance["Dialog_Export_Filter"],
                DefaultExt = "json",
                FileName = JsonDataService.Instance.GetDefaultExportFileName()
            };

            if (dialog.ShowDialog() == true)
            {
                if (JsonDataService.Instance.ExportConfig(dialog.FileName))
                {
                    System.Windows.MessageBox.Show(
                        string.Format(LocalizationService.Instance["Msg_ExportSuccess_Msg"], dialog.FileName),
                        LocalizationService.Instance["Msg_ExportSuccess_Title"],
                        System.Windows.MessageBoxButton.OK,
                        System.Windows.MessageBoxImage.Information);
                }
                else
                {
                    System.Windows.MessageBox.Show(
                        LocalizationService.Instance["Msg_ExportFailed_Msg"],
                        LocalizationService.Instance["Msg_Error_Title"],
                        System.Windows.MessageBoxButton.OK,
                        System.Windows.MessageBoxImage.Error);
                }
            }
        }

        /// <summary>
        /// 根据目标路径自动检测操作类型
        /// </summary>
        private ActionType DetectActionType(string target)
        {
            if (string.IsNullOrWhiteSpace(target))
                return ActionType.OpenFile;

            target = target.Trim();

            // 检测 URL
            if (target.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                target.StartsWith("https://", StringComparison.OrdinalIgnoreCase) ||
                target.StartsWith("ftp://", StringComparison.OrdinalIgnoreCase))
            {
                return ActionType.OpenUrl;
            }

            // 检测文件夹
            if (System.IO.Directory.Exists(target))
            {
                return ActionType.OpenFolder;
            }

            // 检测快捷键格式 (如 "Ctrl+C", "Alt+Tab")
            if (target.Contains('+') && !target.Contains('\\') && !target.Contains('/'))
            {
                var parts = target.Split('+');
                if (parts.Length >= 2 && parts.All(p => p.Length > 0))
                {
                    return ActionType.Shortcut;
                }
            }

            // 检测命令行（包含参数或特殊字符）
            if (target.Contains(' ') || target.Contains('>') || target.Contains('|') || target.Contains('&'))
            {
                return ActionType.ExecuteCommand;
            }

            // 默认为打开文件
            return ActionType.OpenFile;
        }

        private void Save()
        {
            var config = JsonDataService.Instance.Config;
            
            config.StartWithWindows = StartWithWindows;
            config.EnableNotifications = EnableNotifications;
            
            // 保存颜色设置
            config.Wheel.SectorColor = SectorColor;
            config.Wheel.HighlightColor = HighlightColor;
            config.Wheel.TextColor = TextColor;
            
            // 保存镂空大小比例
            config.Wheel.InnerRadiusRatio = InnerRadiusRatio;

            // 保存中央文字大小
            config.Wheel.CenterTextSize = CenterTextSize;

            config.Shortcuts.Clear();
            foreach (var vm in Shortcuts)
            {
                // 查找现有项以保留 IconData
                var existingItem = JsonDataService.Instance.Config.Shortcuts
                    .FirstOrDefault(s => s.Id == vm.Id);
                
                // 保存 CustomIcon：自动模式为空，手动模式为选中的 Emoji
                string customIcon = vm.IsAutoIcon ? string.Empty : vm.Icon;
                
                // 自动检测类型
                ActionType actionType = DetectActionType(vm.Target);
                
                config.Shortcuts.Add(new ShortcutItem
                {
                    Id = vm.Id,
                    Name = vm.Name,
                    Icon = vm.Icon,
                    IconData = existingItem?.IconData ?? string.Empty, // 保留图标数据
                    CustomIcon = customIcon, // 保存用户自定义图标（自动时为空）
                    ActionType = actionType, // 自动检测类型
                    Target = vm.Target,
                    Arguments = vm.Arguments,
                    Order = vm.Order
                });
            }
            
            JsonDataService.Instance.SaveConfig();
            JsonDataService.Instance.NotifyConfigChanged();
            RequestClose?.Invoke(this, EventArgs.Empty);
        }

        private void Cancel()
        {
            RequestClose?.Invoke(this, EventArgs.Empty);
        }

        private void ReloadIcons()
        {
            try
            {
                var config = JsonDataService.Instance.Config;
                int clearedCount = 0;
                int reloadedCount = 0;

                // 清除所有图标缓存
                foreach (var item in config.Shortcuts)
                {
                    if (!string.IsNullOrEmpty(item.IconData))
                    {
                        item.IconData = string.Empty;
                        clearedCount++;
                    }
                }

                // 重新提取图标
                foreach (var item in config.Shortcuts)
                {
                    if (item.ActionType == ActionType.OpenFile || item.ActionType == ActionType.OpenFolder)
                    {
                        var icon = IconService.GetIconForAction(item.ActionType, item.Target);
                        if (icon != null)
                        {
                            item.IconData = IconService.IconToBase64(icon) ?? "";
                            reloadedCount++;
                        }
                    }
                }

                // 保存配置
                JsonDataService.Instance.SaveConfig();

                System.Windows.MessageBox.Show(
                    string.Format(LocalizationService.Instance["Msg_ReloadIcons_Msg"], clearedCount, reloadedCount),
                    LocalizationService.Instance["Msg_ReloadIcons_Title"],
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(
                    string.Format(LocalizationService.Instance["Msg_ReloadIconsError_Msg"], ex.Message),
                    LocalizationService.Instance["Msg_Error_Title"],
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Error);
            }
        }

        private void ResetConfig()
        {
            var result = System.Windows.MessageBox.Show(
                LocalizationService.Instance["Confirm_ResetConfig_Msg"],
                LocalizationService.Instance["Confirm_ResetConfig_Title"],
                System.Windows.MessageBoxButton.YesNo,
                System.Windows.MessageBoxImage.Question);

            if (result == System.Windows.MessageBoxResult.Yes)
            {
                try
                {
                    // 保存当前快捷项
                    var currentShortcuts = JsonDataService.Instance.Config.Shortcuts.ToList();

                    // 重置配置
                    JsonDataService.Instance.ResetToDefaults();

                    // 恢复快捷项
                    JsonDataService.Instance.Config.Shortcuts = currentShortcuts;
                    JsonDataService.Instance.SaveConfig();

                    // 重新加载界面
                    LoadFromConfig(JsonDataService.Instance.Config);

                    System.Windows.MessageBox.Show(
                        LocalizationService.Instance["Msg_ConfigReset_Msg"],
                        LocalizationService.Instance["Msg_ConfigReset_Title"],
                        System.Windows.MessageBoxButton.OK,
                        System.Windows.MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show(
                        string.Format(LocalizationService.Instance["Msg_ConfigResetError_Msg"], ex.Message),
                        LocalizationService.Instance["Msg_Error_Title"],
                        System.Windows.MessageBoxButton.OK,
                        System.Windows.MessageBoxImage.Error);
                }
            }
        }

        private void ClearAllData()
        {
            var result = System.Windows.MessageBox.Show(
                LocalizationService.Instance["Confirm_ClearData_Msg"],
                LocalizationService.Instance["Confirm_ClearData_Title"],
                System.Windows.MessageBoxButton.YesNo,
                System.Windows.MessageBoxImage.Warning);

            if (result == System.Windows.MessageBoxResult.Yes)
            {
                // 二次确认
                var confirmResult = System.Windows.MessageBox.Show(
                    LocalizationService.Instance["Confirm_ClearData_Final_Msg"],
                    LocalizationService.Instance["Confirm_ClearData_Final_Title"],
                    System.Windows.MessageBoxButton.YesNo,
                    System.Windows.MessageBoxImage.Warning);

                if (confirmResult == System.Windows.MessageBoxResult.Yes)
                {
                    try
                    {
                        // 删除所有数据
                        JsonDataService.Instance.DeleteAllUserData();

                        System.Windows.MessageBox.Show(
                            LocalizationService.Instance["Msg_ClearData_Msg"],
                            LocalizationService.Instance["Msg_ClearData_Title"],
                            System.Windows.MessageBoxButton.OK,
                            System.Windows.MessageBoxImage.Information);

                        // 关闭程序
                        System.Windows.Application.Current.Shutdown();
                    }
                    catch (Exception ex)
                    {
                        System.Windows.MessageBox.Show(
                            string.Format(LocalizationService.Instance["Msg_ClearDataError_Msg"], ex.Message),
                            LocalizationService.Instance["Msg_Error_Title"],
                            System.Windows.MessageBoxButton.OK,
                            System.Windows.MessageBoxImage.Error);
                    }
                }
            }
        }

        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }

    public class ShortcutItemViewModel : INotifyPropertyChanged
    {
        private string _id = "";
        private string _name = "";
        private string _icon = "";
        private ActionType _actionType;
        private string _target = "";
        private string _arguments = "";
        private int _order;
        private bool _isAutoIcon = true; // 默认自动匹配

        public event PropertyChangedEventHandler? PropertyChanged;

        public string Id
        {
            get => _id;
            set { _id = value; OnPropertyChanged(); }
        }

        public string Name
        {
            get => _name;
            set { _name = value; OnPropertyChanged(); }
        }

        public string Icon
        {
            get => _icon;
            set 
            { 
                _icon = value; 
                // 如果选择的是 🤖，则根据目标自动匹配
                if (_icon == "🤖")
                {
                    _isAutoIcon = true;
                    // 触发重新计算实际显示的图标
                    OnPropertyChanged(nameof(DisplayIcon));
                }
                else
                {
                    _isAutoIcon = false;
                }
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// 是否使用自动图标匹配
        /// </summary>
        public bool IsAutoIcon
        {
            get => _isAutoIcon;
            set 
            { 
                _isAutoIcon = value; 
                OnPropertyChanged();
                OnPropertyChanged(nameof(DisplayIcon));
            }
        }

        /// <summary>
        /// 实际显示的图标（自动时根据目标匹配，否则显示选中的 Emoji）
        /// </summary>
        public string DisplayIcon
        {
            get
            {
                if (_isAutoIcon || _icon == "🤖")
                {
                    return EmojiService.GetSmartEmoji(new ShortcutItem
                    {
                        ActionType = ActionType,
                        Target = Target
                    });
                }
                return _icon;
            }
        }

        public ActionType ActionType
        {
            get => _actionType;
            set { _actionType = value; OnPropertyChanged(); }
        }

        public string Target
        {
            get => _target;
            set { _target = value; OnPropertyChanged(); }
        }

        public string Arguments
        {
            get => _arguments;
            set { _arguments = value; OnPropertyChanged(); }
        }

        public int Order
        {
            get => _order;
            set { _order = value; OnPropertyChanged(); }
        }

        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }

    public class RelayCommand : ICommand
    {
        private readonly Action _execute;
        private readonly Func<bool>? _canExecute;

        public RelayCommand(Action execute, Func<bool>? canExecute = null)
        {
            _execute = execute;
            _canExecute = canExecute;
        }

        public event EventHandler? CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public bool CanExecute(object? parameter) => _canExecute?.Invoke() ?? true;
        public void Execute(object? parameter) => _execute();
    }

    public class RelayCommand<T> : ICommand
    {
        private readonly Action<T?> _execute;
        private readonly Func<T?, bool>? _canExecute;

        public RelayCommand(Action<T?> execute, Func<T?, bool>? canExecute = null)
        {
            _execute = execute;
            _canExecute = canExecute;
        }

        public event EventHandler? CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public bool CanExecute(object? parameter) => _canExecute?.Invoke((T?)parameter) ?? true;
        public void Execute(object? parameter) => _execute((T?)parameter);
    }
}
