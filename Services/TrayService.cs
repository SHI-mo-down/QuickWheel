using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Hardcodet.Wpf.TaskbarNotification;

namespace QuickWheel.Services
{
    public class TrayService : IDisposable
    {
        private static TrayService? _instance;
        private TaskbarIcon? _taskbarIcon;
        private bool _disposed;

        public static TrayService Instance => _instance ??= new TrayService();

        public event EventHandler? SettingsRequested;
        public event EventHandler? ExitRequested;

        private TrayService() { }

        public void Initialize()
        {
            try
            {
                _taskbarIcon = new TaskbarIcon
                {
                    ToolTipText = LocalizationService.Instance["Tray_Tooltip"],
                    Visibility = Visibility.Visible
                };

                var contextMenu = new ContextMenu
                {
                    Style = CreateCompactMenuStyle()
                };

                var settingsItem = new MenuItem
                {
                    Header = LocalizationService.Instance["Tray_Settings"],
                    Style = CreateMinimalMenuItemStyle()
                };
                settingsItem.Click += (s, e) => SettingsRequested?.Invoke(this, EventArgs.Empty);

                var exitItem = new MenuItem
                {
                    Header = LocalizationService.Instance["Tray_Exit"],
                    Style = CreateMinimalMenuItemStyle()
                };
                exitItem.Click += (s, e) => ExitRequested?.Invoke(this, EventArgs.Empty);

                contextMenu.Items.Add(settingsItem);
                contextMenu.Items.Add(new Separator { Style = CreateCompactSeparatorStyle() });
                contextMenu.Items.Add(exitItem);

                _taskbarIcon.ContextMenu = contextMenu;
                _taskbarIcon.TrayMouseDoubleClick += (s, e) => SettingsRequested?.Invoke(this, EventArgs.Empty);

                CreateDefaultIcon();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"TrayService Initialize error: {ex.Message}");
            }
        }

        #region 紧凑菜单样式

        private Style CreateCompactMenuStyle()
        {
            var style = new Style(typeof(ContextMenu));
            style.Setters.Add(new Setter(ContextMenu.BorderThicknessProperty, new Thickness(1)));
            style.Setters.Add(new Setter(ContextMenu.BorderBrushProperty, new SolidColorBrush(Color.FromRgb(220, 220, 220))));
            style.Setters.Add(new Setter(ContextMenu.BackgroundProperty, new SolidColorBrush(Colors.White)));
            // 不设置 Padding 和 MinWidth，让菜单完全自适应
            return style;
        }

        private Style CreateMinimalMenuItemStyle()
        {
            var style = new Style(typeof(MenuItem));
            // 使用标准字体
            style.Setters.Add(new Setter(MenuItem.FontSizeProperty, 12.0));
            style.Setters.Add(new Setter(MenuItem.FontFamilyProperty, new FontFamily("Segoe UI")));
            style.Setters.Add(new Setter(MenuItem.ForegroundProperty, new SolidColorBrush(Colors.Black)));
            // 使用较小的 padding
            style.Setters.Add(new Setter(MenuItem.PaddingProperty, new Thickness(4, 2, 4, 2)));
            
            // 极简悬停：浅灰背景
            var trigger = new Trigger { Property = MenuItem.IsMouseOverProperty, Value = true };
            trigger.Setters.Add(new Setter(MenuItem.BackgroundProperty, new SolidColorBrush(Color.FromRgb(240, 240, 240))));
            trigger.Setters.Add(new Setter(MenuItem.ForegroundProperty, new SolidColorBrush(Colors.Black)));
            style.Triggers.Add(trigger);
            
            return style;
        }

        private Style CreateCompactSeparatorStyle()
        {
            var style = new Style(typeof(Separator));
            style.Setters.Add(new Setter(Separator.HeightProperty, 1.0));
            style.Setters.Add(new Setter(Separator.MarginProperty, new Thickness(4, 1, 4, 1)));
            style.Setters.Add(new Setter(Separator.BackgroundProperty, new SolidColorBrush(Color.FromRgb(230, 230, 230))));
            return style;
        }

        #endregion

        private void CreateDefaultIcon()
        {
            try
            {
                // 尝试从嵌入资源加载 ICO
                var resourceUri = new Uri("pack://application:,,,/QuickWheel;component/Resources/app.ico");
                var resourceInfo = Application.GetResourceStream(resourceUri);
                if (resourceInfo != null)
                {
                    // 必须复制到新的 MemoryStream，因为资源流不支持 Position 操作
                    using (var ms = new System.IO.MemoryStream())
                    {
                        resourceInfo.Stream.CopyTo(ms);
                        ms.Position = 0;
                        _taskbarIcon!.Icon = new System.Drawing.Icon(ms);
                    }
                }
            }
            catch
            {
                try
                {
                    _taskbarIcon!.Icon = System.Drawing.SystemIcons.Application;
                }
                catch { }
            }
        }

        public void Show()
        {
            if (_taskbarIcon != null)
            {
                _taskbarIcon.Visibility = Visibility.Visible;
            }
        }

        public void Hide()
        {
            if (_taskbarIcon != null)
            {
                _taskbarIcon.Visibility = Visibility.Collapsed;
            }
        }

        public void ShowBalloonTip(string title, string message, BalloonIcon icon = BalloonIcon.Info)
        {
            _taskbarIcon?.ShowBalloonTip(title, message, icon);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    if (_taskbarIcon != null)
                    {
                        _taskbarIcon.Dispose();
                        _taskbarIcon = null;
                    }
                }
                _disposed = true;
            }
        }

        ~TrayService()
        {
            Dispose(false);
        }
    }
}
