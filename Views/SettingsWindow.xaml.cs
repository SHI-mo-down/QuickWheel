using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using QuickWheel.Services;
using QuickWheel.ViewModels;

namespace QuickWheel.Views
{
    public partial class SettingsWindow : Window
    {
        private readonly SettingsViewModel _viewModel;

        public SettingsWindow()
        {
            InitializeComponent();
            _viewModel = (SettingsViewModel)DataContext;
            _viewModel.RequestClose += OnRequestClose;
            _viewModel.PropertyChanged += OnViewModelPropertyChanged;
            
            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            var config = JsonDataService.Instance.LoadConfig();
            _viewModel.LoadFromConfig(config);
        }

        private void OnViewModelPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(SettingsViewModel.IsRecordingHotkey))
            {
                // 根据录制状态添加/移除键盘事件监听
                if (_viewModel.IsRecordingHotkey)
                {
                    PreviewKeyDown += OnRecordingKeyDown;
                    PreviewKeyUp += OnRecordingKeyUp;
                }
                else
                {
                    PreviewKeyDown -= OnRecordingKeyDown;
                    PreviewKeyUp -= OnRecordingKeyUp;
                }
            }
        }

        private void OnRecordingKeyDown(object sender, KeyEventArgs e)
        {
            Key actualKey = e.Key == Key.System ? e.SystemKey : e.Key;

            // 忽略修饰键本身
            if (KeyHelper.IsIgnoredKey(actualKey))
            {
                e.Handled = true;
                return;
            }

            // 获取当前按下的修饰键
            var modifiers = Keyboard.Modifiers;
            
            // 构建修饰键字符串
            var modifierParts = new List<string>();
            if (modifiers.HasFlag(ModifierKeys.Control))
                modifierParts.Add("Ctrl");
            if (modifiers.HasFlag(ModifierKeys.Alt))
                modifierParts.Add("Alt");
            // Shift 不能单独作为修饰键，必须配合 Ctrl/Alt/Win 使用
            if (modifiers.HasFlag(ModifierKeys.Shift))
                modifierParts.Add("Shift");
            if (modifiers.HasFlag(ModifierKeys.Windows))
                modifierParts.Add("Win");

            // 必须包含至少一个非Shift修饰键（Ctrl/Alt/Win）
            bool hasRequiredModifier = modifiers.HasFlag(ModifierKeys.Control) 
                || modifiers.HasFlag(ModifierKeys.Alt) 
                || modifiers.HasFlag(ModifierKeys.Windows);

            if (!hasRequiredModifier)
            {
                // 不阻止事件，让用户可以继续按键
                return;
            }

            // 获取主键显示名称
            string keyName = KeyHelper.KeyToString(actualKey);
            
            // 组合热键字符串
            string modifierStr = string.Join("+", modifierParts);
            
            // 更新热键
            _viewModel.UpdateHotkey(modifierStr, keyName);
            
            // 停止录制
            _viewModel.IsRecordingHotkey = false;
            
            e.Handled = true;
        }

        private void OnRecordingKeyUp(object sender, KeyEventArgs e)
        {
            // 在录制模式下，阻止所有按键事件冒泡，避免干扰其他控件
            if (_viewModel.IsRecordingHotkey)
            {
                e.Handled = true;
            }
        }

        private void OnRequestClose(object? sender, System.EventArgs e)
        {
            // 确保停止录制
            if (_viewModel.IsRecordingHotkey)
            {
                _viewModel.IsRecordingHotkey = false;
            }
            Close();
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            // 清理事件监听
            if (_viewModel.IsRecordingHotkey)
            {
                _viewModel.IsRecordingHotkey = false;
            }
            _viewModel.PropertyChanged -= OnViewModelPropertyChanged;
            base.OnClosing(e);
        }

        private void IconButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is ShortcutItemViewModel item)
            {
                // 创建弹出窗口
                var popupWindow = new Window
                {
                    Title = "选择图标",
                    Width = 280,
                    Height = 160,
                    WindowStyle = WindowStyle.ToolWindow,
                    ResizeMode = ResizeMode.NoResize,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner,
                    Owner = this,
                    Background = System.Windows.Media.Brushes.White
                };

                // 创建网格布局
                var grid = new Grid { Margin = new Thickness(8) };
                
                // 定义列（每行5个）
                for (int i = 0; i < 5; i++)
                {
                    grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
                }
                
                // 添加图标按钮
                int col = 0, row = 0;
                foreach (var icon in SettingsViewModel.AvailableIcons)
                {
                    if (col == 0)
                    {
                        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                    }
                    
                    var iconButton = new Button
                    {
                        Content = icon,
                        FontSize = 20,
                        Width = 44,
                        Height = 36,
                        Margin = new Thickness(2),
                        Padding = new Thickness(0),
                        Background = System.Windows.Media.Brushes.Transparent,
                        BorderThickness = new Thickness(0)
                    };
                    
                    var currentIcon = icon; // 捕获变量
                    iconButton.Click += (s, args) =>
                    {
                        item.Icon = currentIcon;
                        popupWindow.Close();
                    };
                    
                    Grid.SetRow(iconButton, row);
                    Grid.SetColumn(iconButton, col);
                    grid.Children.Add(iconButton);
                    
                    col++;
                    if (col >= 5)
                    {
                        col = 0;
                        row++;
                    }
                }
                
                popupWindow.Content = grid;
                popupWindow.ShowDialog();
                e.Handled = true;
            }
        }

        #region 拖拽排序

        private Point _dragStartPoint;
        private int _dragStartIndex = -1;
        private DataGridRow? _draggedRow;

        private void ShortcutsGrid_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // 获取点击的行
            _draggedRow = FindVisualParent<DataGridRow>(e.OriginalSource as DependencyObject);
            if (_draggedRow != null)
            {
                _dragStartPoint = e.GetPosition(ShortcutsGrid);
                _dragStartIndex = _draggedRow.GetIndex();
            }
        }

        private void ShortcutsGrid_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (_draggedRow == null || e.LeftButton != MouseButtonState.Pressed)
                return;

            var currentPosition = e.GetPosition(ShortcutsGrid);
            var diff = _dragStartPoint - currentPosition;

            // 判断移动距离是否超过阈值（开始拖拽）
            if (Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance ||
                Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance)
            {
                // 开始拖拽
                if (_dragStartIndex >= 0)
                {
                    DragDrop.DoDragDrop(_draggedRow, _dragStartIndex, DragDropEffects.Move);
                }
            }
        }

        private void ShortcutsGrid_DragOver(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.Move;
            e.Handled = true;

            // 获取目标行索引
            var targetRow = FindVisualParent<DataGridRow>(e.OriginalSource as DependencyObject);
            if (targetRow != null)
            {
                // 可以在这里添加视觉反馈，比如高亮目标行
            }
        }

        private void ShortcutsGrid_Drop(object sender, DragEventArgs e)
        {
            if (_dragStartIndex < 0)
                return;

            // 获取目标行
            var targetRow = FindVisualParent<DataGridRow>(e.OriginalSource as DependencyObject);
            if (targetRow == null)
                return;

            int targetIndex = targetRow.GetIndex();
            if (targetIndex < 0 || targetIndex == _dragStartIndex)
                return;

            // 执行排序
            _viewModel.MoveShortcut(_dragStartIndex, targetIndex);

            // 重置状态
            _dragStartIndex = -1;
            _draggedRow = null;
            e.Handled = true;
        }

        /// <summary>
        /// 查找可视树中的父元素
        /// </summary>
        private static T? FindVisualParent<T>(DependencyObject? child) where T : DependencyObject
        {
            while (child != null)
            {
                if (child is T parent)
                    return parent;
                child = System.Windows.Media.VisualTreeHelper.GetParent(child);
            }
            return null;
        }

        #endregion

        #region 帮助窗口

        private void HelpButton_Click(object sender, RoutedEventArgs e)
        {
            var helpWindow = new HelpWindow
            {
                Owner = this
            };
            helpWindow.ShowDialog();
        }

        #endregion
    }
}
