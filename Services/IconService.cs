using System;
using System.Collections.Concurrent;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using QuickWheel.Models;

namespace QuickWheel.Services
{
    public static class IconService
    {
        [DllImport("shell32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr SHGetFileInfo(string pszPath, uint dwFileAttributes, ref SHFILEINFO psfi, uint cbFileInfo, uint uFlags);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        private struct SHFILEINFO
        {
            public IntPtr hIcon;
            public int iIcon;
            public uint dwAttributes;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            public string szDisplayName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
            public string szTypeName;
        }

        private const uint SHGFI_ICON = 0x000000100;
        private const uint SHGFI_LARGEICON = 0x000000000;
        private const uint SHGFI_SMALLICON = 0x000000001;
        private const uint SHGFI_USEFILEATTRIBUTES = 0x000000010;

        // 图标缓存，避免重复获取
        private static readonly ConcurrentDictionary<string, BitmapSource?> _iconCache = new();
        private const int MaxCacheSize = 100;

        /// <summary>
        /// 从文件路径获取图标（带缓存）
        /// </summary>
        public static BitmapSource? GetIconForFile(string filePath, bool largeIcon = true)
        {
            try
            {
                if (string.IsNullOrEmpty(filePath))
                    return null;

                // 创建缓存键
                string cacheKey = $"{filePath}_{largeIcon}";

                // 检查缓存
                if (_iconCache.TryGetValue(cacheKey, out var cachedIcon))
                {
                    return cachedIcon;
                }

                // 检查文件是否存在（对于系统文件如 explorer.exe 可能不存在于指定路径）
                if (!File.Exists(filePath) && !IsSystemFile(filePath))
                    return null;

                // 如果是快捷方式，获取目标路径
                if (filePath.EndsWith(".lnk", StringComparison.OrdinalIgnoreCase))
                {
                    filePath = GetShortcutTarget(filePath) ?? filePath;
                    cacheKey = $"{filePath}_{largeIcon}";
                }

                // 使用 Shell API 获取图标
                SHFILEINFO shinfo = new SHFILEINFO();
                uint flags = SHGFI_ICON | (largeIcon ? SHGFI_LARGEICON : SHGFI_SMALLICON);

                IntPtr hImg = SHGetFileInfo(filePath, 0, ref shinfo, (uint)Marshal.SizeOf(shinfo), flags);

                if (shinfo.hIcon == IntPtr.Zero)
                    return null;

                // 转换为 BitmapSource
                BitmapSource icon = Imaging.CreateBitmapSourceFromHIcon(
                    shinfo.hIcon,
                    Int32Rect.Empty,
                    BitmapSizeOptions.FromEmptyOptions());

                // 冻结图标以提高性能
                icon.Freeze();

                // 销毁图标句柄
                DestroyIcon(shinfo.hIcon);

                // 添加到缓存（限制缓存大小）
                if (_iconCache.Count < MaxCacheSize)
                {
                    _iconCache.TryAdd(cacheKey, icon);
                }

                return icon;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetIconForFile error: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// 检查是否为系统文件
        /// </summary>
        private static bool IsSystemFile(string fileName)
        {
            string[] systemFiles = { "explorer.exe", "cmd.exe", "notepad.exe" };
            return Array.Exists(systemFiles, f => f.Equals(fileName, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// 清除图标缓存
        /// </summary>
        public static void ClearCache()
        {
            _iconCache.Clear();
        }

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool DestroyIcon(IntPtr hIcon);

        /// <summary>
        /// 获取快捷方式的目标路径
        /// </summary>
        private static string? GetShortcutTarget(string shortcutPath)
        {
            try
            {
                // 使用 WScript.Shell 解析快捷方式
                Type? shellType = Type.GetTypeFromProgID("WScript.Shell");
                if (shellType == null) return null;

                dynamic? shell = Activator.CreateInstance(shellType);
                if (shell == null) return null;

                dynamic? shortcut = shell.CreateShortcut(shortcutPath);
                if (shortcut == null) return null;

                string targetPath = shortcut.TargetPath;
                // 释放 COM 对象
                Marshal.ReleaseComObject(shortcut);
                Marshal.ReleaseComObject(shell);

                return targetPath;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// 根据 ActionType 和目标路径获取合适的图标
        /// </summary>
        public static BitmapSource? GetIconForAction(ActionType actionType, string target)
        {
            return actionType switch
            {
                ActionType.OpenFile => GetIconForFile(target),
                ActionType.OpenFolder => GetIconForFile(target),
                ActionType.OpenUrl => GetDefaultUrlIcon(),
                ActionType.ExecuteCommand => GetDefaultCommandIcon(),
                ActionType.Shortcut => GetDefaultShortcutIcon(),
                _ => null
            };
        }

        private static BitmapSource? GetDefaultUrlIcon()
        {
            // 返回浏览器图标或默认网页图标
            return GetIconForFile("explorer.exe");
        }

        private static BitmapSource? GetDefaultCommandIcon()
        {
            // 返回命令提示符图标
            return GetIconForFile("cmd.exe");
        }

        private static BitmapSource? GetDefaultShortcutIcon()
        {
            // 返回键盘图标（使用系统图标）
            return GetIconForFile("notepad.exe");
        }

        /// <summary>
        /// 将 BitmapSource 转换为 Base64 字符串存储
        /// </summary>
        public static string? IconToBase64(BitmapSource? icon)
        {
            if (icon == null) return null;

            try
            {
                var encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(icon));
                
                using var ms = new MemoryStream();
                encoder.Save(ms);
                return Convert.ToBase64String(ms.ToArray());
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// 从 Base64 字符串还原图标
        /// </summary>
        public static BitmapSource? IconFromBase64(string? base64String)
        {
            if (string.IsNullOrEmpty(base64String)) return null;

            try
            {
                byte[] bytes = Convert.FromBase64String(base64String);
                var ms = new MemoryStream(bytes);
                
                var decoder = new PngBitmapDecoder(ms, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
                var frame = decoder.Frames[0];
                
                // 冻结图像以提高性能并避免跨线程问题
                if (frame.CanFreeze)
                {
                    frame.Freeze();
                }
                
                return frame;
            }
            catch
            {
                return null;
            }
        }
    }
}
