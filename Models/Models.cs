using System.ComponentModel;

namespace QuickWheel.Models
{
    public enum ActionType
    {
        [Description("打开文件")]
        OpenFile,
        [Description("打开网址")]
        OpenUrl,
        [Description("打开文件夹")]
        OpenFolder,
        [Description("执行命令")]
        ExecuteCommand,
        [Description("快捷键")]
        Shortcut
    }

    public class ShortcutItem : INotifyPropertyChanged
    {
        private string _id = string.Empty;
        private string _name = string.Empty;
        private string _icon = string.Empty;
        private ActionType _actionType;
        private string _target = string.Empty;
        private string _arguments = string.Empty;
        private int _order;

        public string Id
        {
            get => _id;
            set { _id = value; OnPropertyChanged(nameof(Id)); }
        }

        public string Name
        {
            get => _name;
            set { _name = value; OnPropertyChanged(nameof(Name)); }
        }

        public string Icon
        {
            get => _icon;
            set { _icon = value; OnPropertyChanged(nameof(Icon)); }
        }

        // 程序图标 Base64 编码（自动提取）
        private string _iconData = string.Empty;
        public string IconData
        {
            get => _iconData;
            set { _iconData = value; OnPropertyChanged(nameof(IconData)); }
        }

        // 用户手动设置的图标（优先显示）
        private string _customIcon = string.Empty;
        public string CustomIcon
        {
            get => _customIcon;
            set { _customIcon = value; OnPropertyChanged(nameof(CustomIcon)); }
        }

        public ActionType ActionType
        {
            get => _actionType;
            set { _actionType = value; OnPropertyChanged(nameof(ActionType)); }
        }

        public string Target
        {
            get => _target;
            set { _target = value; OnPropertyChanged(nameof(Target)); }
        }

        public string Arguments
        {
            get => _arguments;
            set { _arguments = value; OnPropertyChanged(nameof(Arguments)); }
        }

        public int Order
        {
            get => _order;
            set { _order = value; OnPropertyChanged(nameof(Order)); }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public enum WheelPositionMode
    {
        [Description("鼠标中心")]
        MouseCenter,
        [Description("屏幕中心")]
        ScreenCenter
    }

    public class WheelSettings
    {
        public double ItemSize { get; set; } = 60;
        public double WheelRadius { get; set; } = 150;
        public int MaxItems { get; set; } = 8;
        public bool ShowLabels { get; set; } = true;

        // 镂空大小比例 (0.0 - 1.0)，控制内径占外径的比例
        public double InnerRadiusRatio { get; set; } = 0.35;

        // 中央文字大小
        public double CenterTextSize { get; set; } = 10;

        // 颜色设置 (ARGB 格式)
        public string SectorColor { get; set; } = "#CC333333";      // 扇区颜色
        public string HighlightColor { get; set; } = "#FF444444";   // 高亮颜色
        public string TextColor { get; set; } = "#FFFFFFFF";        // 文字颜色
        public string CenterCircleColor { get; set; } = "#FF222222"; // 中央圆颜色

        // 位置模式
        public WheelPositionMode PositionMode { get; set; } = WheelPositionMode.MouseCenter;

        // 测试模式
        public bool TestMode { get; set; } = false;  // 开启后轮盘常驻显示在屏幕中央
    }

    public class HotkeySettings
    {
        public string ModifierKeys { get; set; } = "Ctrl+Alt";
        public string Key { get; set; } = "Q";
    }

    public class AppConfig
    {
        // 配置版本号，用于迁移
        public int ConfigVersion { get; set; } = 1;
        
        // 语言设置
        public string Language { get; set; } = "zh-CN";
        
        public HotkeySettings Hotkey { get; set; } = new();
        public List<ShortcutItem> Shortcuts { get; set; } = new();
        public WheelSettings Wheel { get; set; } = new();
        public bool StartWithWindows { get; set; } = false;
        public bool StartMinimized { get; set; } = true;
        public bool EnableNotifications { get; set; } = false;

        public static AppConfig CreateDefault()
        {
            return new AppConfig
            {
                StartWithWindows = false,
                StartMinimized = true,
                Hotkey = new HotkeySettings
                {
                    ModifierKeys = "Ctrl+Alt",
                    Key = "Q"
                },
                Wheel = new WheelSettings
                {
                    ItemSize = 60,
                    WheelRadius = 150,
                    MaxItems = 8,
                    ShowLabels = true
                },
                Shortcuts = new List<ShortcutItem>
                {
                    new ShortcutItem
                    {
                        Id = Guid.NewGuid().ToString(),
                        Name = "Notepad",
                        Icon = "📝",
                        ActionType = ActionType.OpenFile,
                        Target = @"C:\Windows\System32\notepad.exe",
                        Arguments = "",
                        Order = 0
                    },
                    new ShortcutItem
                    {
                        Id = Guid.NewGuid().ToString(),
                        Name = "Calculator",
                        Icon = "🧮",
                        ActionType = ActionType.OpenFile,
                        Target = @"C:\Windows\System32\calc.exe",
                        Arguments = "",
                        Order = 1
                    },
                    new ShortcutItem
                    {
                        Id = Guid.NewGuid().ToString(),
                        Name = "Documents",
                        Icon = "📁",
                        ActionType = ActionType.OpenFolder,
                        Target = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                        Arguments = "",
                        Order = 2
                    },
                    new ShortcutItem
                    {
                        Id = Guid.NewGuid().ToString(),
                        Name = "Google",
                        Icon = "🌐",
                        ActionType = ActionType.OpenUrl,
                        Target = "https://www.google.com",
                        Arguments = "",
                        Order = 3
                    },
                    new ShortcutItem
                    {
                        Id = Guid.NewGuid().ToString(),
                        Name = "Command Prompt",
                        Icon = "⌨️",
                        ActionType = ActionType.OpenFile,
                        Target = @"C:\Windows\System32\cmd.exe",
                        Arguments = "",
                        Order = 4
                    }
                }
            };
        }
    }
}
