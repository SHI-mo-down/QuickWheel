using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace QuickWheel.Services
{
    /// <summary>
    /// 提供执行反馈通知服务
    /// </summary>
    public class NotificationService
    {
        private static NotificationService? _instance;
        public static NotificationService Instance => _instance ??= new NotificationService();

        private Window? _notificationWindow;
        private bool _isEnabled = false;

        private NotificationService() { }

        /// <summary>
        /// 是否启用通知
        /// </summary>
        public bool IsEnabled
        {
            get => _isEnabled;
            set => _isEnabled = value;
        }

        /// <summary>
        /// 显示执行成功通知
        /// </summary>
        public void ShowSuccess(string title, string message)
        {
            if (!_isEnabled) return;
            ShowNotification(title, message, Brushes.Green);
        }

        /// <summary>
        /// 显示执行失败通知
        /// </summary>
        public void ShowError(string title, string message)
        {
            if (!_isEnabled) return;
            ShowNotification(title, message, Brushes.Red);
        }

        /// <summary>
        /// 显示执行信息通知
        /// </summary>
        public void ShowInfo(string title, string message)
        {
            if (!_isEnabled) return;
            ShowNotification(title, message, Brushes.DodgerBlue);
        }

        private void ShowNotification(string title, string message, Brush accentBrush)
        {
            try
            {
                Application.Current?.Dispatcher.Invoke(() =>
                {
                    CreateAndShowNotification(title, message, accentBrush);
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[NotificationService] 显示通知失败: {ex.Message}");
            }
        }

        private void CreateAndShowNotification(string title, string message, Brush accentBrush)
        {
            // 关闭之前的通知
            _notificationWindow?.Close();

            // 创建通知窗口 - 使用自适应大小
            _notificationWindow = new Window
            {
                WindowStyle = WindowStyle.None,
                AllowsTransparency = true,
                Background = Brushes.Transparent,
                ShowInTaskbar = false,
                Topmost = true,
                Width = 320,
                SizeToContent = SizeToContent.Height,
                ResizeMode = ResizeMode.NoResize
            };

            // 定位到屏幕右下角
            var screen = SystemParameters.WorkArea;
            _notificationWindow.Left = screen.Right - 340;
            _notificationWindow.Top = screen.Bottom - 120;

            // 创建通知内容
            var border = new Border
            {
                Background = new SolidColorBrush(Color.FromArgb(240, 40, 40, 40)),
                CornerRadius = new CornerRadius(8),
                BorderThickness = new Thickness(2),
                BorderBrush = accentBrush,
                Margin = new Thickness(10),
                Effect = new System.Windows.Media.Effects.DropShadowEffect
                {
                    BlurRadius = 10,
                    ShadowDepth = 2,
                    Opacity = 0.3
                }
            };

            var stackPanel = new StackPanel
            {
                Margin = new Thickness(15, 10, 15, 10)
            };

            var titleBlock = new TextBlock
            {
                Text = title,
                Foreground = accentBrush,
                FontWeight = FontWeights.Bold,
                FontSize = 14,
                TextTrimming = TextTrimming.CharacterEllipsis,
                MaxWidth = 280
            };

            var messageBlock = new TextBlock
            {
                Text = message,
                Foreground = Brushes.White,
                FontSize = 12,
                TextWrapping = TextWrapping.Wrap,
                MaxWidth = 280,
                Margin = new Thickness(0, 4, 0, 0)
            };

            stackPanel.Children.Add(titleBlock);
            stackPanel.Children.Add(messageBlock);
            border.Child = stackPanel;
            _notificationWindow.Content = border;

            // 显示窗口
            _notificationWindow.Show();

            // 淡入动画
            var fadeIn = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(200));
            _notificationWindow.BeginAnimation(UIElement.OpacityProperty, fadeIn);

            // 3秒后自动关闭
            var timer = new System.Windows.Threading.DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(3)
            };
            timer.Tick += (s, e) =>
            {
                timer.Stop();
                CloseNotification();
            };
            timer.Start();

            // 点击关闭
            _notificationWindow.MouseLeftButtonDown += (s, e) => CloseNotification();
        }

        private void CloseNotification()
        {
            if (_notificationWindow == null) return;

            try
            {
                var fadeOut = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(200));
                fadeOut.Completed += (s, e) =>
                {
                    _notificationWindow?.Close();
                    _notificationWindow = null;
                };
                _notificationWindow.BeginAnimation(UIElement.OpacityProperty, fadeOut);
            }
            catch
            {
                _notificationWindow?.Close();
                _notificationWindow = null;
            }
        }
    }
}
