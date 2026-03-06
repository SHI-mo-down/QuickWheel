using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using QuickWheel.Models;

namespace QuickWheel.Services
{
    public class ExecutionService
    {
        private static ExecutionService? _instance;
        public static ExecutionService Instance => _instance ??= new ExecutionService();

        private ExecutionService() { }

        /// <summary>
        /// 执行快捷项，返回执行结果和详细信息
        /// </summary>
        public (bool success, string message) ExecuteWithDetails(ShortcutItem item)
        {
            if (item == null || string.IsNullOrWhiteSpace(item.Target))
            {
                string errorMsg = "目标路径为空";
                string itemName = item?.Name ?? "未知快捷项";
                Debug.WriteLine($"[ExecutionService] 执行失败: {itemName} - {errorMsg}");
                NotificationService.Instance.ShowError($"✗ {itemName}", errorMsg);
                return (false, errorMsg);
            }

            try
            {
                // 处理参数模板
                var processedItem = TemplateService.Instance.ProcessShortcut(item);

                Debug.WriteLine($"[ExecutionService] 执行: {processedItem.Name} ({processedItem.ActionType})");
                Debug.WriteLine($"  目标: {processedItem.Target}");
                if (!string.IsNullOrEmpty(processedItem.Arguments))
                    Debug.WriteLine($"  参数: {processedItem.Arguments}");

                (bool success, string message) result = processedItem.ActionType switch
                {
                    ActionType.OpenFile => ExecuteOpenFileWithDetails(processedItem.Target, processedItem.Arguments),
                    ActionType.OpenUrl => OpenUrlWithDetails(processedItem.Target),
                    ActionType.OpenFolder => OpenFolderWithDetails(processedItem.Target),
                    ActionType.ExecuteCommand => ExecuteCommandWithDetails(processedItem.Target, processedItem.Arguments),
                    ActionType.Shortcut => SendShortcutKeysWithDetails(processedItem.Target),
                    _ => (false, "未知的操作类型")
                };

                if (result.success)
                {
                    Debug.WriteLine($"[ExecutionService] 执行成功: {item.Name}");
                    NotificationService.Instance.ShowSuccess($"✓ {item.Name}", result.message);
                }
                else
                {
                    Debug.WriteLine($"[ExecutionService] 执行失败: {item.Name} - {result.message}");
                    NotificationService.Instance.ShowError($"✗ {item.Name}", result.message);
                }

                return result;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ExecutionService] 执行异常: {ex.Message}");
                NotificationService.Instance.ShowError($"✗ {item.Name}", $"执行异常: {ex.Message}");
                return (false, $"执行异常: {ex.Message}");
            }
        }

        /// <summary>
        /// 执行快捷项（兼容旧接口）
        /// </summary>
        public bool Execute(ShortcutItem item)
        {
            var (success, _) = ExecuteWithDetails(item);
            return success;
        }

        #region 详细执行方法

        private (bool success, string message) ExecuteOpenFileWithDetails(string path, string arguments = "")
        {
            if (string.IsNullOrWhiteSpace(path))
                return (false, "文件路径为空");

            try
            {
                string expandedPath = Environment.ExpandEnvironmentVariables(path);

                if (expandedPath.EndsWith(".lnk", StringComparison.OrdinalIgnoreCase))
                {
                    return ExecuteShortcutWithDetails(expandedPath, arguments);
                }

                if (IsExecutableFile(expandedPath))
                {
                    return LaunchExecutableWithDetails(expandedPath, arguments);
                }

                return OpenWithDefaultProgramWithDetails(expandedPath);
            }
            catch (Exception ex)
            {
                return (false, $"打开文件失败: {ex.Message}");
            }
        }

        private (bool success, string message) LaunchExecutableWithDetails(string path, string arguments = "")
        {
            try
            {
                string? fullPath = FindExecutablePath(path);
                if (string.IsNullOrEmpty(fullPath))
                {
                    return (false, $"找不到可执行文件: {path}");
                }

                var startInfo = new ProcessStartInfo
                {
                    FileName = fullPath,
                    Arguments = arguments,
                    UseShellExecute = true,
                    WorkingDirectory = Path.GetDirectoryName(fullPath) ?? ""
                };

                Process.Start(startInfo);
                return (true, $"已启动: {Path.GetFileName(fullPath)}");
            }
            catch (Exception ex)
            {
                return (false, $"启动程序失败: {ex.Message}");
            }
        }

        private (bool success, string message) OpenWithDefaultProgramWithDetails(string path)
        {
            try
            {
                if (!File.Exists(path) && !Directory.Exists(path))
                {
                    return (false, $"文件或文件夹不存在: {path}");
                }

                var startInfo = new ProcessStartInfo
                {
                    FileName = path,
                    UseShellExecute = true,
                    Verb = "open"
                };

                Process.Start(startInfo);
                return (true, $"已打开: {Path.GetFileName(path)}");
            }
            catch (Exception ex)
            {
                return (false, $"打开失败: {ex.Message}");
            }
        }

        private (bool success, string message) OpenUrlWithDetails(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return (false, "网址为空");

            try
            {
                string urlToOpen = url.Trim();
                if (!urlToOpen.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
                    !urlToOpen.StartsWith("https://", StringComparison.OrdinalIgnoreCase) &&
                    !urlToOpen.StartsWith("ftp://", StringComparison.OrdinalIgnoreCase))
                {
                    urlToOpen = "https://" + urlToOpen;
                }

                var startInfo = new ProcessStartInfo
                {
                    FileName = urlToOpen,
                    UseShellExecute = true
                };

                Process.Start(startInfo);
                return (true, $"已打开网址: {urlToOpen}");
            }
            catch (Exception ex)
            {
                return (false, $"打开网址失败: {ex.Message}");
            }
        }

        private (bool success, string message) OpenFolderWithDetails(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return (false, "文件夹路径为空");

            try
            {
                string expandedPath = Environment.ExpandEnvironmentVariables(path);

                if (Directory.Exists(expandedPath))
                {
                    Process.Start("explorer.exe", expandedPath);
                    return (true, $"已打开文件夹: {Path.GetFileName(expandedPath)}");
                }
                else if (File.Exists(expandedPath))
                {
                    Process.Start("explorer.exe", $"/select,\"{expandedPath}\"");
                    return (true, $"已定位文件: {Path.GetFileName(expandedPath)}");
                }
                else
                {
                    return (false, $"路径不存在: {expandedPath}");
                }
            }
            catch (Exception ex)
            {
                return (false, $"打开文件夹失败: {ex.Message}");
            }
        }

        private (bool success, string message) ExecuteCommandWithDetails(string command, string arguments = "")
        {
            if (string.IsNullOrWhiteSpace(command))
                return (false, "命令为空");

            try
            {
                string expandedCommand = Environment.ExpandEnvironmentVariables(command);
                string expandedArgs = Environment.ExpandEnvironmentVariables(arguments);

                bool needsWindow = ShouldShowCommandWindow(expandedCommand);

                var startInfo = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = $"/c \"{expandedCommand}\" {expandedArgs}",
                    UseShellExecute = true,
                    CreateNoWindow = !needsWindow,
                    WindowStyle = needsWindow ? ProcessWindowStyle.Normal : ProcessWindowStyle.Hidden
                };

                Process.Start(startInfo);
                return (true, $"已执行命令: {expandedCommand}");
            }
            catch (Exception ex)
            {
                return (false, $"执行命令失败: {ex.Message}");
            }
        }

        private (bool success, string message) ExecuteShortcutWithDetails(string shortcutPath, string arguments = "")
        {
            try
            {
                if (!File.Exists(shortcutPath))
                {
                    return (false, $"快捷方式不存在: {shortcutPath}");
                }

                var startInfo = new ProcessStartInfo
                {
                    FileName = shortcutPath,
                    Arguments = arguments,
                    UseShellExecute = true
                };

                Process.Start(startInfo);
                return (true, $"已执行快捷方式: {Path.GetFileName(shortcutPath)}");
            }
            catch (Exception ex)
            {
                return (false, $"执行快捷方式失败: {ex.Message}");
            }
        }

        private (bool success, string message) SendShortcutKeysWithDetails(string shortcut)
        {
            try
            {
                var keys = ParseShortcutKeys(shortcut);
                if (keys.Count == 0)
                {
                    return (false, $"无法解析快捷键: {shortcut}");
                }

                SendKeys.SendWait(string.Join("+", keys));
                return (true, $"已发送快捷键: {shortcut}");
            }
            catch (Exception ex)
            {
                return (false, $"发送快捷键失败: {ex.Message}");
            }
        }

        #endregion

        #region 辅助方法

        private List<string> ParseShortcutKeys(string shortcut)
        {
            var result = new List<string>();
            var parts = shortcut.Split(new[] { '+', ' ' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var part in parts)
            {
                var key = part.Trim();
                if (string.IsNullOrEmpty(key))
                    continue;

                var converted = key.ToLowerInvariant() switch
                {
                    "ctrl" or "control" => "^",
                    "alt" => "%",
                    "shift" => "+",
                    "win" or "windows" => "#",
                    _ => ConvertKeyToSendKeys(key)
                };

                result.Add(converted);
            }

            return result;
        }

        private string ConvertKeyToSendKeys(string key)
        {
            if (key.StartsWith("F", StringComparison.OrdinalIgnoreCase) &&
                int.TryParse(key.Substring(1), out int fNum) &&
                fNum >= 1 && fNum <= 24)
            {
                return $"{{F{fNum}}}";
            }

            return key.ToUpperInvariant() switch
            {
                "ENTER" or "RETURN" => "{ENTER}",
                "TAB" => "{TAB}",
                "ESC" or "ESCAPE" => "{ESC}",
                "BACK" or "BACKSPACE" => "{BACKSPACE}",
                "DELETE" or "DEL" => "{DELETE}",
                "INSERT" or "INS" => "{INSERT}",
                "HOME" => "{HOME}",
                "END" => "{END}",
                "PAGEUP" or "PGUP" => "{PGUP}",
                "PAGEDOWN" or "PGDN" => "{PGDN}",
                "UP" => "{UP}",
                "DOWN" => "{DOWN}",
                "LEFT" => "{LEFT}",
                "RIGHT" => "{RIGHT}",
                "SPACE" or " " => " ",
                _ => key.ToUpperInvariant()
            };
        }

        private bool IsExecutableFile(string path)
        {
            string ext = Path.GetExtension(path).ToLowerInvariant();
            return ext is ".exe" or ".com" or ".bat" or ".cmd" or ".ps1";
        }

        private string? FindExecutablePath(string fileName)
        {
            if (File.Exists(fileName))
                return fileName;

            string? pathEnv = Environment.GetEnvironmentVariable("PATH");
            if (string.IsNullOrEmpty(pathEnv))
                return null;

            string[] paths = pathEnv.Split(Path.PathSeparator);
            string[] extensions = IsWindows()
                ? new[] { ".exe", ".com", ".bat", ".cmd", "" }
                : new[] { "" };

            foreach (var path in paths)
            {
                foreach (var ext in extensions)
                {
                    string fullPath = Path.Combine(path, fileName + ext);
                    if (File.Exists(fullPath))
                        return fullPath;
                }
            }

            return null;
        }

        private bool ShouldShowCommandWindow(string command)
        {
            string[] interactiveCommands = new[]
            {
                "pause", "choice", "set /p", "echo", "dir", "cls",
                "help", "ipconfig", "ping", "tracert", "nslookup"
            };

            string lowerCmd = command.ToLowerInvariant();
            return interactiveCommands.Any(cmd => lowerCmd.Contains(cmd));
        }

        private static bool IsWindows() => RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

        #endregion

        #region 系统操作

        public bool LockWorkstation()
        {
            try
            {
                LockWorkStation();
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ExecutionService] 锁定工作站失败: {ex.Message}");
                return false;
            }
        }

        public bool LogOff()
        {
            try
            {
                ExitWindowsEx(0, 0);
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ExecutionService] 注销失败: {ex.Message}");
                return false;
            }
        }

        public bool Sleep(bool hibernate = false)
        {
            try
            {
                SetSuspendState(hibernate, true, false);
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ExecutionService] 睡眠/休眠失败: {ex.Message}");
                return false;
            }
        }

        [DllImport("Powrprof.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        private static extern bool SetSuspendState(bool hibernate, bool forceCritical, bool disableWakeEvent);

        [DllImport("user32.dll")]
        private static extern void LockWorkStation();

        [DllImport("user32.dll")]
        private static extern bool ExitWindowsEx(uint uFlags, uint dwReason);

        #endregion
    }
}
