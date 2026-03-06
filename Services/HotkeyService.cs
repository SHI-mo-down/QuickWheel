using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;

namespace QuickWheel.Services
{
    public class HotkeyService : IDisposable
    {
        private static HotkeyService? _instance;
        private IntPtr _windowHandle;
        private HwndSource? _source;
        private bool _isRegistered;
        private bool _disposed;
        private bool _isProcessing;
        private ModifierKeys _currentModifiers;
        private Key _currentKey;
        private int _currentKeyVk;
        private const int HOTKEY_ID = 9000;
        private const int WM_HOTKEY = 0x0312;

        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        [DllImport("user32.dll")]
        private static extern short GetAsyncKeyState(int vKey);

        [Flags]
        public enum KeyModifiers : uint
        {
            None = 0,
            Alt = 1,
            Control = 2,
            Shift = 4,
            Win = 8
        }

        public static HotkeyService Instance => _instance ??= new HotkeyService();

        public event EventHandler? HotkeyPressed;
        public event EventHandler? HotkeyReleased;

        public bool IsProcessing
        {
            get => _isProcessing;
            set => _isProcessing = value;
        }

        private HotkeyService() { }

        public bool Register(Window window, ModifierKeys modifiers, Key key)
        {
            try
            {
                // 保存当前热键配置用于释放检测
                _currentModifiers = modifiers;
                _currentKey = key;
                _currentKeyVk = KeyInterop.VirtualKeyFromKey(key);

                var helper = new WindowInteropHelper(window);
                _windowHandle = helper.EnsureHandle();

                _source = HwndSource.FromHwnd(_windowHandle);
                _source?.AddHook(HwndHook);

                uint mod = 0;
                if (modifiers.HasFlag(ModifierKeys.Alt)) mod |= (uint)KeyModifiers.Alt;
                if (modifiers.HasFlag(ModifierKeys.Control)) mod |= (uint)KeyModifiers.Control;
                if (modifiers.HasFlag(ModifierKeys.Shift)) mod |= (uint)KeyModifiers.Shift;
                if (modifiers.HasFlag(ModifierKeys.Windows)) mod |= (uint)KeyModifiers.Win;

                uint vk = (uint)_currentKeyVk;

                _isRegistered = RegisterHotKey(_windowHandle, HOTKEY_ID, mod, vk);

                if (!_isRegistered)
                {
                    System.Diagnostics.Debug.WriteLine("[HotkeyService] 热键注册失败");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"[HotkeyService] 热键注册成功: {modifiers} + {key}");
                }

                return _isRegistered;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[HotkeyService] 热键注册错误: {ex.Message}");
                return false;
            }
        }

        public void Unregister()
        {
            try
            {
                if (_isRegistered && _windowHandle != IntPtr.Zero)
                {
                    UnregisterHotKey(_windowHandle, HOTKEY_ID);
                    _isRegistered = false;
                    System.Diagnostics.Debug.WriteLine("[HotkeyService] 热键已注销");
                }

                if (_source != null)
                {
                    _source.RemoveHook(HwndHook);
                    _source = null;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[HotkeyService] 热键注销错误: {ex.Message}");
            }
        }

        private IntPtr HwndHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == WM_HOTKEY && wParam.ToInt32() == HOTKEY_ID)
            {
                if (_isProcessing)
                {
                    return IntPtr.Zero;
                }
                handled = true;
                OnHotkeyPressed();
                StartHotkeyMonitor();
            }
            return IntPtr.Zero;
        }

        private System.Threading.Timer? _monitorTimer;
        private bool _isMonitoring;

        private void StartHotkeyMonitor()
        {
            if (_isMonitoring) return;
            _isMonitoring = true;

            _monitorTimer = new System.Threading.Timer(MonitorHotkeyState, null, 50, 50);
            System.Diagnostics.Debug.WriteLine("[HotkeyService] 开始监控热键状态");
        }

        private void StopHotkeyMonitor()
        {
            _isMonitoring = false;
            _monitorTimer?.Dispose();
            _monitorTimer = null;
            System.Diagnostics.Debug.WriteLine("[HotkeyService] 停止监控热键状态");
        }

        private void MonitorHotkeyState(object? state)
        {
            try
            {
                // 动态检测当前绑定的修饰键
                bool modifiersPressed = true;
                
                if (_currentModifiers.HasFlag(ModifierKeys.Control))
                    modifiersPressed &= (GetAsyncKeyState(0x11) & 0x8000) != 0;
                if (_currentModifiers.HasFlag(ModifierKeys.Alt))
                    modifiersPressed &= (GetAsyncKeyState(0x12) & 0x8000) != 0;
                if (_currentModifiers.HasFlag(ModifierKeys.Shift))
                    modifiersPressed &= (GetAsyncKeyState(0x10) & 0x8000) != 0;
                if (_currentModifiers.HasFlag(ModifierKeys.Windows))
                    modifiersPressed &= (GetAsyncKeyState(0x5B) & 0x8000) != 0 || (GetAsyncKeyState(0x5C) & 0x8000) != 0;

                // 检测主键
                bool keyPressed = (GetAsyncKeyState(_currentKeyVk) & 0x8000) != 0;

                // 当任意一个键松开时，触发释放事件
                if (!modifiersPressed || !keyPressed)
                {
                    System.Diagnostics.Debug.WriteLine($"[HotkeyService] 热键释放 detected: modifiers={modifiersPressed}, key={keyPressed}");
                    StopHotkeyMonitor();
                    Application.Current?.Dispatcher.BeginInvoke(() =>
                    {
                        OnHotkeyReleased();
                    });
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[HotkeyService] 监控错误: {ex.Message}");
            }
        }

        private void OnHotkeyPressed()
        {
            Application.Current?.Dispatcher.BeginInvoke(() =>
            {
                System.Diagnostics.Debug.WriteLine("[HotkeyService] 热键按下事件");
                HotkeyPressed?.Invoke(this, EventArgs.Empty);
            });
        }

        private void OnHotkeyReleased()
        {
            Application.Current?.Dispatcher.BeginInvoke(() =>
            {
                System.Diagnostics.Debug.WriteLine("[HotkeyService] 热键释放事件");
                HotkeyReleased?.Invoke(this, EventArgs.Empty);
            });
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
                    StopHotkeyMonitor();
                    Unregister();
                }
                _disposed = true;
            }
        }

        ~HotkeyService()
        {
            Dispose(false);
        }
    }
}
