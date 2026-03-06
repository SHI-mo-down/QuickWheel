using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using QuickWheel.Services;
using QuickWheel.Views;

namespace QuickWheel
{
    public partial class App : Application
    {
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern bool GetCursorPos(out POINT lpPoint);

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern int GetSystemMetrics(int nIndex);

        [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
        private struct POINT
        {
            public int X;
            public int Y;
        }

        private const int SM_XVIRTUALSCREEN = 76;
        private const int SM_YVIRTUALSCREEN = 77;
        private const int SM_CXVIRTUALSCREEN = 78;
        private const int SM_CYVIRTUALSCREEN = 79;

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            MainWindow = new MainWindow();
            MainWindow.Show();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            try
            {
                AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
                DispatcherUnhandledException += OnDispatcherUnhandledException;

                var config = JsonDataService.Instance.LoadConfig();
                System.Diagnostics.Debug.WriteLine($"[App] 配置已加载，快捷项数量: {config.Shortcuts.Count}");

                // 同步开机启动设置
                StartupService.Instance.SyncStartupSetting(config.StartWithWindows);

                // 初始化通知设置
                NotificationService.Instance.IsEnabled = config.EnableNotifications;

                TrayService.Instance.Initialize();

                TrayService.Instance.SettingsRequested += OnSettingsRequested;
                TrayService.Instance.ExitRequested += OnExitRequested;

                RadialMenuService.Instance.Initialize();
                RadialMenuService.Instance.UpdateFromConfig(config);

                RegisterHotkey(config);

                JsonDataService.Instance.ConfigChanged += OnConfigChanged;

                // 检查测试模式
                if (config.Wheel.TestMode)
                {
                    System.Diagnostics.Debug.WriteLine("[App] 测试模式已开启，显示轮盘在屏幕中央");
                    RadialMenuService.Instance.ShowTestMode();
                }

                if (MainWindow != null)
                {
                    MainWindow.ShowInTaskbar = false;
                    MainWindow.WindowState = WindowState.Minimized;
                    MainWindow.Hide();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Startup error: {ex.Message}");
                MessageBox.Show($"启动失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                Shutdown(1);
            }
        }

        private void RegisterHotkey(Models.AppConfig config)
        {
            try
            {
                HotkeyService.Instance.HotkeyPressed -= OnHotkeyPressed;
                HotkeyService.Instance.HotkeyReleased -= OnHotkeyReleased;
                HotkeyService.Instance.Unregister();

                var modifierKeys = ParseModifierKeys(config.Hotkey.ModifierKeys);
                var key = (Key)Enum.Parse(typeof(Key), config.Hotkey.Key);

                bool registered = HotkeyService.Instance.Register(MainWindow, modifierKeys, key);

                if (registered)
                {
                    System.Diagnostics.Debug.WriteLine($"[App] 热键注册成功: {config.Hotkey.ModifierKeys} + {config.Hotkey.Key}");
                    HotkeyService.Instance.HotkeyPressed += OnHotkeyPressed;
                    HotkeyService.Instance.HotkeyReleased += OnHotkeyReleased;
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"[App] 热键注册失败: {config.Hotkey.ModifierKeys} + {config.Hotkey.Key}");
                    MessageBox.Show($"热键 {config.Hotkey.ModifierKeys} + {config.Hotkey.Key} 注册失败，可能被其他程序占用", 
                        "警告", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[App] 注册热键错误: {ex.Message}");
            }
        }

        private ModifierKeys ParseModifierKeys(string modifiers)
        {
            ModifierKeys result = ModifierKeys.None;
            var parts = modifiers.Split('+');
            foreach (var part in parts)
            {
                switch (part.Trim().ToLower())
                {
                    case "ctrl":
                    case "control":
                        result |= ModifierKeys.Control;
                        break;
                    case "alt":
                        result |= ModifierKeys.Alt;
                        break;
                    case "shift":
                        result |= ModifierKeys.Shift;
                        break;
                    case "win":
                    case "windows":
                        result |= ModifierKeys.Windows;
                        break;
                }
            }
            return result;
        }

        private void OnConfigChanged(object? sender, EventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                try
                {
                    var config = JsonDataService.Instance.Config;
                    System.Diagnostics.Debug.WriteLine("[App] 配置已变更，刷新相关服务");

                    // 同步开机启动设置
                    StartupService.Instance.SyncStartupSetting(config.StartWithWindows);

                    // 更新通知设置
                    NotificationService.Instance.IsEnabled = config.EnableNotifications;

                    RadialMenuService.Instance.UpdateFromConfig(config);
                    RegisterHotkey(config);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[App] OnConfigChanged error: {ex.Message}");
                }
            });
        }

        private Point GetCurrentMousePosition()
        {
            if (GetCursorPos(out POINT point))
            {
                Point screenPos = new Point(point.X, point.Y);
                
                try
                {
                    var mainWindow = Application.Current.MainWindow;
                    if (mainWindow != null)
                    {
                        var source = PresentationSource.FromVisual(mainWindow);
                        if (source?.CompositionTarget != null)
                        {
                            var transform = source.CompositionTarget.TransformFromDevice;
                            screenPos = transform.Transform(screenPos);
                        }
                    }
                }
                catch
                {
                }
                
                return screenPos;
            }
            return new Point(SystemParameters.PrimaryScreenWidth / 2, SystemParameters.PrimaryScreenHeight / 2);
        }

        private void OnHotkeyPressed(object? sender, EventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                try
                {
                    HotkeyService.Instance.IsProcessing = true;
                    
                    var mousePosition = GetCurrentMousePosition();
                    RadialMenuService.Instance.Show(mousePosition);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"OnHotkeyPressed error: {ex.Message}");
                }
            });
        }

        private void OnHotkeyReleased(object? sender, EventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                try
                {
                    var radialMenu = RadialMenuService.Instance;
                    if (radialMenu.IsVisible)
                    {
                        radialMenu.Hide();
                    }
                    HotkeyService.Instance.IsProcessing = false;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"OnHotkeyReleased error: {ex.Message}");
                }
            });
        }

        private void OnSettingsRequested(object? sender, EventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                RadialMenuService.Instance.Hide();
                var settingsWindow = new SettingsWindow();
                settingsWindow.ShowDialog();
            });
        }

        private void OnExitRequested(object? sender, EventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                Shutdown();
            });
        }

        protected override void OnExit(ExitEventArgs e)
        {
            try
            {
                JsonDataService.Instance.ConfigChanged -= OnConfigChanged;

                HotkeyService.Instance.HotkeyPressed -= OnHotkeyPressed;
                HotkeyService.Instance.HotkeyReleased -= OnHotkeyReleased;
                HotkeyService.Instance.Dispose();
                RadialMenuService.Instance.Dispose();
                TrayService.Instance.Dispose();

                JsonDataService.Instance.SaveConfig();
                System.Diagnostics.Debug.WriteLine("[App] 配置已保存");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Exit cleanup error: {ex.Message}");
            }

            base.OnExit(e);
        }

        private void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var ex = e.ExceptionObject as Exception;
            System.Diagnostics.Debug.WriteLine($"Unhandled exception: {ex?.Message}");
            MessageBox.Show($"未处理的异常: {ex?.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void OnDispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine($"Dispatcher unhandled exception: {e.Exception.Message}");
            MessageBox.Show($"UI异常: {e.Exception.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            e.Handled = true;
        }
    }
}
