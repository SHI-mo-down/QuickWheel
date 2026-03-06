using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using QuickWheel.Controls;
using QuickWheel.Models;
using QuickWheel.Services;
using QuickWheel.ViewModels;
using System.Runtime.InteropServices;

namespace QuickWheel.Views
{
    public partial class RadialMenuWindow : Window
    {
        private RadialMenuViewModel? _viewModel;
        private Point _canvasCenter;
        private readonly List<RadialSectorControl> _sectorControls = new();
        private bool _isMouseOverWindow;
        private bool _isHotkeyReleased;
        private Point _showPosition;
        private bool _isPositionLocked;
        private bool _isExecuting;
        private bool _isClosed;
        private bool _hasNewIcons; // 标记是否有新提取的图标需要保存

        [DllImport("user32.dll")]
        private static extern bool GetCursorPos(out POINT lpPoint);

        [StructLayout(LayoutKind.Sequential)]
        private struct POINT
        {
            public int X;
            public int Y;
        }

        public event EventHandler<ShortcutItem>? ItemExecuted;
        public event EventHandler? MenuClosed;

        public new double Left
        {
            get => base.Left;
            set
            {
                if (!_isPositionLocked)
                    base.Left = value;
            }
        }

        public new double Top
        {
            get => base.Top;
            set
            {
                if (!_isPositionLocked)
                    base.Top = value;
            }
        }

        public RadialMenuWindow()
        {
            InitializeComponent();
            _viewModel = new RadialMenuViewModel();
            DataContext = _viewModel;
        }

        public void Initialize(RadialMenuViewModel viewModel)
        {
            _viewModel = viewModel;
            DataContext = _viewModel;
        }

        public void ShowAtPosition(Point screenPosition)
        {
            try
            {
                double menuRadius = _viewModel?.MenuRadius ?? 150;
                double menuSize = menuRadius * 2 + 100;
                
                Width = menuSize;
                Height = menuSize;

                double halfSize = menuSize / 2;
                
                Left = screenPosition.X - halfSize;
                Top = screenPosition.Y - halfSize;

                _canvasCenter = new Point(halfSize, halfSize);

                _viewModel!.CenterPosition = new Point(screenPosition.X, screenPosition.Y);
                _viewModel.ResetSelection();

                BuildMenu();

                _isHotkeyReleased = false;
            _isMouseOverWindow = false;
            _isClosed = false;
            _isExecuting = false;
            _hasNewIcons = false; // 重置图标保存标记

            _showPosition = screenPosition;
            _isPositionLocked = true;

                Show();
                Activate();
                Focus();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ShowAtPosition error: {ex.Message}");
            }
        }

        public void ShowAtCurrentMouse()
        {
            if (GetCursorPos(out POINT point))
            {
                ShowAtPosition(new Point(point.X, point.Y));
            }
        }

        private void BuildMenu()
        {
            SectorPanel.Children.Clear();
            _sectorControls.Clear();

            if (_viewModel == null || _viewModel.Shortcuts.Count == 0)
                return;

            // 应用颜色设置
            ApplyColorSettings();

            int itemCount = _viewModel.Shortcuts.Count;
            double sectorAngle = 360.0 / itemCount;
            double outerRadius = _viewModel.MenuRadius;
            double innerRadius = _viewModel.InnerRadius;

            SectorPanel.ItemCount = itemCount;
            SectorPanel.OuterRadius = outerRadius;
            SectorPanel.InnerRadius = innerRadius;

            // 获取扇区颜色和高亮颜色
            var sectorColor = ParseColor(_viewModel.SectorColor);
            var sectorBrush = new SolidColorBrush(sectorColor);
            var highlightColor = ParseColor(_viewModel.HighlightColor);
            var highlightBrush = new SolidColorBrush(highlightColor);

            for (int i = 0; i < itemCount; i++)
            {
                var item = _viewModel.Shortcuts[i];

                // 图标优先级：1.用户自定义图标 2.自动提取的程序图标 3.智能Emoji
                ImageSource? iconSource = null;
                string emojiIcon = item.Icon;
                
                // 1. 检查用户自定义图标（优先）
                if (!string.IsNullOrEmpty(item.CustomIcon))
                {
                    emojiIcon = item.CustomIcon;
                }
                // 2. 尝试加载自动提取的程序图标
                else if (!string.IsNullOrEmpty(item.IconData))
                {
                    iconSource = IconService.IconFromBase64(item.IconData);
                    // 如果没有手动设置Emoji，使用智能匹配
                    if (string.IsNullOrEmpty(item.Icon))
                    {
                        emojiIcon = EmojiService.GetSmartEmoji(item);
                    }
                }
                // 3. 没有缓存，尝试提取程序图标
                else if (item.ActionType == ActionType.OpenFile || item.ActionType == ActionType.OpenFolder)
                {
                    var icon = IconService.GetIconForAction(item.ActionType, item.Target);
                    if (icon != null)
                    {
                        iconSource = icon;
                        // 缓存图标数据（延迟保存，关闭时统一保存）
                        item.IconData = IconService.IconToBase64(icon) ?? "";
                        _hasNewIcons = true; // 标记有新图标需要保存
                    }
                    // 如果没有手动设置Emoji，使用智能匹配
                    if (string.IsNullOrEmpty(item.Icon))
                    {
                        emojiIcon = EmojiService.GetSmartEmoji(item);
                    }
                }
                // 4. 其他类型使用智能Emoji
                else if (string.IsNullOrEmpty(item.Icon))
                {
                    emojiIcon = EmojiService.GetSmartEmoji(item);
                }

                var sector = new RadialSectorControl
                {
                    SectorIndex = i,
                    Icon = emojiIcon,
                    IconSource = iconSource,
                    Text = item.Name,
                    IconSize = _viewModel.ItemSize / 2.5,
                    TextSize = 10,
                    ShowLabel = _viewModel.ShowLabels && item.Name.Length <= 4,
                    OuterRadius = outerRadius,
                    InnerRadius = innerRadius,
                    SectorFill = sectorBrush,
                    HighlightFill = highlightBrush,
                    SectorStroke = Brushes.Transparent,
                    Command = new RelayCommand<int>(OnSectorClicked),
                    CommandParameter = i
                };

                _sectorControls.Add(sector);
                SectorPanel.Children.Add(sector);
            }

            // 调整中央圆大小匹配内径
            double centerCircleSize = _viewModel.InnerRadius * 2;
            CenterCircle.Width = centerCircleSize;
            CenterCircle.Height = centerCircleSize;

            UpdateVisualSelection();
        }

        private void ApplyColorSettings()
        {
            try
            {
                var config = JsonDataService.Instance.LoadConfig();
                var wheelSettings = config.Wheel;

                // 应用文字颜色
                if (CenterTextBrush != null && !string.IsNullOrEmpty(wheelSettings.TextColor))
                {
                    CenterTextBrush.Color = ParseColor(wheelSettings.TextColor);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ApplyColorSettings error: {ex.Message}");
            }
        }

        private static Color ParseColor(string colorString)
        {
            if (string.IsNullOrEmpty(colorString))
                return Colors.Transparent;

            try
            {
                if (colorString.StartsWith("#"))
                {
                    var brush = (SolidColorBrush)new BrushConverter().ConvertFromString(colorString)!;
                    return brush.Color;
                }
            }
            catch { }

            return Colors.Transparent;
        }

        private void OnSectorClicked(int index)
        {
            ExecuteSelectedItem(index);
        }

        private void UpdateSelection()
        {
            if (_viewModel == null || _sectorControls.Count == 0) return;

            var mousePos = Mouse.GetPosition(SectorPanel);
            double dx = mousePos.X - _canvasCenter.X;
            double dy = mousePos.Y - _canvasCenter.Y;
            double distance = Math.Sqrt(dx * dx + dy * dy);

            int newIndex = -1;

            // 使用 WPF 命中测试来检测鼠标在哪个扇区上
            // 这比角度计算更准确，因为考虑了实际的扇区几何形状
            var hitTestResult = VisualTreeHelper.HitTest(SectorPanel, mousePos);
            if (hitTestResult != null)
            {
                // 查找命中的扇区控件
                DependencyObject? hitObject = hitTestResult.VisualHit;
                while (hitObject != null)
                {
                    if (hitObject is RadialSectorControl sector)
                    {
                        newIndex = sector.SectorIndex;
                        break;
                    }
                    hitObject = VisualTreeHelper.GetParent(hitObject);
                }
            }

            // 如果没有命中任何扇区，但鼠标在轮盘范围内，使用角度计算作为备选
            if (newIndex == -1 && distance >= _viewModel.InnerRadius && distance <= _viewModel.MenuRadius)
            {
                int itemCount = _viewModel.Shortcuts.Count;
                if (itemCount == 1)
                {
                    // 只有一个项目时，直接选中
                    newIndex = 0;
                }
                else if (itemCount > 0)
                {
                    double angle = Math.Atan2(dy, dx) * MathHelper.RadiansToDegrees;
                    angle += 90;
                    if (angle < 0) angle += 360;

                    double sectorAngle = 360.0 / itemCount;
                    newIndex = (int)(angle / sectorAngle);
                    if (newIndex >= itemCount) newIndex = itemCount - 1;
                }
            }

            // 在内圈时，保持当前选中状态
            if (newIndex == -1 && distance < _viewModel.InnerRadius)
            {
                newIndex = _viewModel.SelectedIndex;
            }

            if (_viewModel.SelectedIndex != newIndex)
            {
                _viewModel.SelectedIndex = newIndex;
                UpdateVisualSelection();
            }
        }

        private void UpdateVisualSelection()
        {
            if (_viewModel == null || _sectorControls.Count == 0) return;

            // 更新所有扇区的选中状态
            for (int i = 0; i < _sectorControls.Count; i++)
            {
                _sectorControls[i].IsSelected = (i == _viewModel.SelectedIndex);
            }

            // 更新中央文字显示当前选中项的名称
            if (_viewModel.SelectedIndex >= 0 && _viewModel.SelectedIndex < _viewModel.Shortcuts.Count)
            {
                CenterText.Text = _viewModel.Shortcuts[_viewModel.SelectedIndex].Name;
            }
            else
            {
                CenterText.Text = "QuickWheel";
            }
        }

        private void ExecuteSelectedItem(int index)
        {
            if (_isExecuting || _isClosed || _viewModel == null)
                return;

            _isExecuting = true;

            try
            {
                if (index >= 0 && index < _viewModel.Shortcuts.Count)
                {
                    var item = _viewModel.Shortcuts[index];
                    ItemExecuted?.Invoke(this, item);
                }
                CloseMenu();
            }
            finally
            {
                _isExecuting = false;
            }
        }

        public void CloseMenu()
        {
            if (_isClosed)
                return;

            _isClosed = true;
            _isPositionLocked = false;

            try
            {
                // 如果有新提取的图标，延迟保存配置（性能优化）
                if (_hasNewIcons)
                {
                    JsonDataService.Instance.SaveConfig();
                    _hasNewIcons = false;
                    System.Diagnostics.Debug.WriteLine("[RadialMenuWindow] 图标缓存已保存");
                }

                Hide();
                MenuClosed?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"CloseMenu error: {ex.Message}");
            }
        }

        private void Window_Deactivated(object sender, EventArgs e)
        {
            // 测试模式下不自动关闭
            if (_viewModel?.TestMode == true)
                return;

            if (!_isHotkeyReleased)
            {
                CloseMenu();
            }
        }

        private void Window_MouseMove(object sender, MouseEventArgs e)
        {
            _isMouseOverWindow = IsMouseOverWindow();
            UpdateSelection();
        }

        private void Window_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (_isClosed || _isExecuting)
                return;

            if (_viewModel?.SelectedIndex >= 0)
            {
                ExecuteSelectedItem(_viewModel.SelectedIndex);
            }
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                CloseMenu();
            }
        }

        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
        }

        private bool IsMouseOverWindow()
        {
            var point = Mouse.GetPosition(this);
            return point.X >= 0 && point.X <= Width && point.Y >= 0 && point.Y <= Height;
        }

        public void SetHotkeyReleased(bool released)
        {
            _isHotkeyReleased = released;
            if (released)
            {
                if (_viewModel?.SelectedItem != null)
                {
                    var item = _viewModel.SelectedItem;
                    ItemExecuted?.Invoke(this, item);
                }
                CloseMenu();
            }
        }
    }

    public class RelayCommand<T> : ICommand
    {
        private readonly Action<T> _execute;
        private readonly Func<T, bool>? _canExecute;

        public RelayCommand(Action<T> execute, Func<T, bool>? canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public event EventHandler? CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }

        public bool CanExecute(object? parameter)
        {
            return _canExecute == null || _canExecute((T)parameter!);
        }

        public void Execute(object? parameter)
        {
            _execute((T)parameter!);
        }
    }
}
