using System.Windows;
using System.Windows.Input;
using QuickWheel.Models;
using QuickWheel.Services;
using QuickWheel.ViewModels;
using QuickWheel.Views;

namespace QuickWheel.Services
{
    public class RadialMenuService : IDisposable
    {
        private static RadialMenuService? _instance;
        public static RadialMenuService Instance => _instance ??= new RadialMenuService();

        private RadialMenuWindow? _radialMenuWindow;
        private RadialMenuViewModel? _viewModel;
        private bool _isVisible;
        private bool _disposed;
        private bool _isHiding;

        public event EventHandler<ShortcutItem>? ItemExecuted;
        public event EventHandler? MenuShown;
        public event EventHandler? MenuHidden;

        public bool IsVisible => _isVisible;

        private RadialMenuService() { }

        public void Initialize()
        {
            try
            {
                _viewModel = new RadialMenuViewModel();
                _radialMenuWindow = new RadialMenuWindow();
                _radialMenuWindow.Initialize(_viewModel);
                _radialMenuWindow.ItemExecuted += OnItemExecuted;
                _radialMenuWindow.MenuClosed += OnMenuClosed;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"RadialMenuService Initialize error: {ex.Message}");
            }
        }

        public void LoadConfig(AppConfig config)
        {
            _viewModel?.UpdateFromConfig(config);
        }

        public void UpdateFromConfig(AppConfig config)
        {
            _viewModel?.UpdateFromConfig(config);
        }

        public void Show(Point screenPosition)
        {
            if (_radialMenuWindow == null || _viewModel == null)
                return;

            try
            {
                if (_viewModel.Shortcuts.Count == 0)
                {
                    System.Diagnostics.Debug.WriteLine("No shortcuts configured");
                    return;
                }

                _radialMenuWindow.ShowAtPosition(screenPosition);
                _isVisible = true;
                MenuShown?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"RadialMenuService Show error: {ex.Message}");
            }
        }

        public void Hide()
        {
            if (_radialMenuWindow == null || _isHiding)
                return;

            try
            {
                _isHiding = true;
                
                _radialMenuWindow.CloseMenu();
                _isVisible = false;
                MenuHidden?.Invoke(this, EventArgs.Empty);
                
                _isHiding = false;
            }
            catch (Exception ex)
            {
                _isHiding = false;
                System.Diagnostics.Debug.WriteLine($"RadialMenuService Hide error: {ex.Message}");
            }
        }

        public void ExecuteAndHide(ShortcutItem item)
        {
            try
            {
                Hide();
                ExecutionService.Instance.Execute(item);
                ItemExecuted?.Invoke(this, item);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ExecuteAndHide error: {ex.Message}");
            }
        }

        public void Toggle(Point screenPosition)
        {
            if (_isVisible)
                Hide();
            else
                Show(screenPosition);
        }

        public void ShowTestMode()
        {
            if (_radialMenuWindow == null || _viewModel == null)
                return;

            try
            {
                if (_viewModel.Shortcuts.Count == 0)
                {
                    System.Diagnostics.Debug.WriteLine("No shortcuts configured");
                    return;
                }

                // 获取屏幕中心
                var screenWidth = SystemParameters.PrimaryScreenWidth;
                var screenHeight = SystemParameters.PrimaryScreenHeight;
                var centerPosition = new Point(screenWidth / 2, screenHeight / 2);

                _radialMenuWindow.ShowAtPosition(centerPosition);
                _isVisible = true;
                MenuShown?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"RadialMenuService ShowTestMode error: {ex.Message}");
            }
        }

        private void OnItemExecuted(object? sender, ShortcutItem item)
        {
            ExecutionService.Instance.Execute(item);
            ItemExecuted?.Invoke(this, item);
        }

        private void OnMenuClosed(object? sender, EventArgs e)
        {
            _isVisible = false;
            MenuHidden?.Invoke(this, EventArgs.Empty);
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
                    if (_radialMenuWindow != null)
                    {
                        _radialMenuWindow.ItemExecuted -= OnItemExecuted;
                        _radialMenuWindow.MenuClosed -= OnMenuClosed;
                        _radialMenuWindow.Close();
                        _radialMenuWindow = null;
                    }
                }
                _disposed = true;
            }
        }

        ~RadialMenuService()
        {
            Dispose(false);
        }
    }
}
