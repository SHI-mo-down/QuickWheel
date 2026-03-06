using Microsoft.Win32;
using System.Diagnostics;
using System.Reflection;

namespace QuickWheel.Services
{
    /// <summary>
    /// 管理应用程序开机启动设置
    /// </summary>
    public class StartupService
    {
        private static StartupService? _instance;
        public static StartupService Instance => _instance ??= new StartupService();

        private const string RegistryKeyName = "QuickWheel";
        private readonly string _appPath;

        private StartupService()
        {
            _appPath = Assembly.GetExecutingAssembly().Location;
            // 如果是 .dll 路径，转换为 .exe
            if (_appPath.EndsWith(".dll", StringComparison.OrdinalIgnoreCase))
            {
                _appPath = _appPath[..^4] + ".exe";
            }
        }

        /// <summary>
        /// 检查是否已设置为开机启动
        /// </summary>
        public bool IsStartupEnabled()
        {
            try
            {
                using var key = Registry.CurrentUser.OpenSubKey(
                    @"Software\Microsoft\Windows\CurrentVersion\Run", false);
                
                if (key == null)
                    return false;

                var value = key.GetValue(RegistryKeyName) as string;
                return !string.IsNullOrEmpty(value);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[StartupService] 检查启动项失败: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 设置为开机启动
        /// </summary>
        public bool EnableStartup()
        {
            try
            {
                using var key = Registry.CurrentUser.OpenSubKey(
                    @"Software\Microsoft\Windows\CurrentVersion\Run", true);
                
                if (key == null)
                {
                    Debug.WriteLine("[StartupService] 无法打开注册表项");
                    return false;
                }

                // 添加启动项，使用引号包裹路径以支持空格
                key.SetValue(RegistryKeyName, $"\"{_appPath}\"");
                Debug.WriteLine($"[StartupService] 已启用开机启动: {_appPath}");
                return true;
            }
            catch (UnauthorizedAccessException)
            {
                Debug.WriteLine("[StartupService] 权限不足，无法设置开机启动");
                return false;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[StartupService] 启用启动项失败: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 取消开机启动
        /// </summary>
        public bool DisableStartup()
        {
            try
            {
                using var key = Registry.CurrentUser.OpenSubKey(
                    @"Software\Microsoft\Windows\CurrentVersion\Run", true);
                
                if (key == null)
                {
                    Debug.WriteLine("[StartupService] 无法打开注册表项");
                    return false;
                }

                // 删除启动项
                key.DeleteValue(RegistryKeyName, false);
                Debug.WriteLine("[StartupService] 已禁用开机启动");
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[StartupService] 禁用启动项失败: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 根据配置同步启动项设置
        /// </summary>
        public void SyncStartupSetting(bool shouldStartWithWindows)
        {
            bool currentState = IsStartupEnabled();
            
            if (shouldStartWithWindows && !currentState)
            {
                EnableStartup();
            }
            else if (!shouldStartWithWindows && currentState)
            {
                DisableStartup();
            }
        }
    }
}
