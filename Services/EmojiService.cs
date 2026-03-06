using System;
using System.Collections.Generic;
using QuickWheel.Models;

namespace QuickWheel.Services
{
    /// <summary>
    /// 智能 Emoji 图标服务
    /// </summary>
    public static class EmojiService
    {
        // 文件扩展名到 Emoji 的映射
        private static readonly Dictionary<string, string> FileExtensionEmojis = new(StringComparer.OrdinalIgnoreCase)
        {
            // 文档
            [".txt"] = "📝",
            [".doc"] = "📄",
            [".docx"] = "📄",
            [".pdf"] = "📑",
            [".xls"] = "📊",
            [".xlsx"] = "📊",
            [".ppt"] = "📽️",
            [".pptx"] = "📽️",

            // 图片
            [".jpg"] = "🖼️",
            [".jpeg"] = "🖼️",
            [".png"] = "🖼️",
            [".gif"] = "🖼️",
            [".bmp"] = "🖼️",
            [".svg"] = "🎨",
            [".psd"] = "🎨",

            // 音频
            [".mp3"] = "🎵",
            [".wav"] = "🎵",
            [".flac"] = "🎵",
            [".aac"] = "🎵",
            [".ogg"] = "🎵",

            // 视频
            [".mp4"] = "🎬",
            [".avi"] = "🎬",
            [".mkv"] = "🎬",
            [".mov"] = "🎬",
            [".wmv"] = "🎬",

            // 压缩文件
            [".zip"] = "📦",
            [".rar"] = "📦",
            [".7z"] = "📦",
            [".tar"] = "📦",
            [".gz"] = "📦",

            // 代码文件
            [".cs"] = "💻",
            [".js"] = "💻",
            [".ts"] = "💻",
            [".html"] = "🌐",
            [".css"] = "🎨",
            [".py"] = "🐍",
            [".java"] = "☕",
            [".cpp"] = "💻",
            [".c"] = "💻",
            [".go"] = "💻",
            [".rs"] = "🦀",
            [".php"] = "🐘",
            [".rb"] = "💎",
            [".swift"] = "🦉",
            [".kt"] = "💻",
            [".sql"] = "🗄️",
            [".json"] = "📋",
            [".xml"] = "📋",
            [".yaml"] = "📋",
            [".yml"] = "📋",

            // 可执行文件
            [".exe"] = "⚙️",
            [".msi"] = "📦",
            [".bat"] = "📜",
            [".cmd"] = "📜",
            [".ps1"] = "📜",
            [".sh"] = "📜",

            // 其他
            [".lnk"] = "🔗",
            [".url"] = "🔗",
        };

        // 应用程序名称到 Emoji 的映射
        private static readonly Dictionary<string, string> AppNameEmojis = new(StringComparer.OrdinalIgnoreCase)
        {
            ["notepad"] = "📝",
            ["notepad++"] = "📝",
            ["word"] = "📄",
            ["excel"] = "📊",
            ["powerpoint"] = "📽️",
            ["chrome"] = "🌐",
            ["firefox"] = "🦊",
            ["edge"] = "🌐",
            ["opera"] = "🌐",
            ["safari"] = "🧭",
            ["vscode"] = "💻",
            ["code"] = "💻",
            ["visual studio"] = "💻",
            ["cmd"] = "💻",
            ["powershell"] = "💻",
            ["terminal"] = "💻",
            ["calculator"] = "🧮",
            ["calc"] = "🧮",
            ["explorer"] = "📁",
            ["steam"] = "🎮",
            ["discord"] = "💬",
            ["slack"] = "💬",
            ["teams"] = "💬",
            ["zoom"] = "📹",
            ["skype"] = "📞",
            ["spotify"] = "🎵",
            ["itunes"] = "🎵",
            ["vlc"] = "🎬",
            ["photoshop"] = "🎨",
            ["gimp"] = "🎨",
            ["illustrator"] = "🎨",
            ["premiere"] = "🎬",
            ["after effects"] = "🎬",
            ["blender"] = "🎨",
            ["obs"] = "📹",
            ["docker"] = "🐳",
            ["git"] = "🌿",
            ["github"] = "🐙",
            ["python"] = "🐍",
            ["node"] = "🟢",
            ["nodejs"] = "🟢",
            ["java"] = "☕",
            ["mysql"] = "🐬",
            ["postgres"] = "🐘",
            ["mongodb"] = "🍃",
            ["redis"] = "🔴",
        };

        // 网址域名到 Emoji 的映射
        private static readonly Dictionary<string, string> DomainEmojis = new(StringComparer.OrdinalIgnoreCase)
        {
            ["google"] = "🔍",
            ["baidu"] = "🔍",
            ["bing"] = "🔍",
            ["youtube"] = "📺",
            ["bilibili"] = "📺",
            ["github"] = "🐙",
            ["gitlab"] = "🦊",
            ["stackoverflow"] = "📚",
            ["reddit"] = "🔴",
            ["twitter"] = "🐦",
            ["x.com"] = "🐦",
            ["facebook"] = "📘",
            ["instagram"] = "📷",
            ["linkedin"] = "💼",
            ["amazon"] = "📦",
            ["taobao"] = "🛒",
            ["tmall"] = "🛒",
            ["jd"] = "🛒",
            ["pdd"] = "🛒",
            ["weibo"] = "📱",
            ["wechat"] = "💬",
            ["qq"] = "🐧",
            ["zhihu"] = "❓",
            ["wikipedia"] = "📚",
            ["translate"] = "🌐",
            ["map"] = "🗺️",
            ["mail"] = "📧",
            ["outlook"] = "📧",
            ["gmail"] = "📧",
            ["drive"] = "☁️",
            ["dropbox"] = "📦",
            ["onedrive"] = "☁️",
            ["cloud"] = "☁️",
        };

        /// <summary>
        /// 根据快捷项信息获取智能 Emoji
        /// </summary>
        public static string GetSmartEmoji(ShortcutItem item)
        {
            return item.ActionType switch
            {
                ActionType.OpenFile => GetFileEmoji(item.Target),
                ActionType.OpenFolder => "📁",
                ActionType.OpenUrl => GetUrlEmoji(item.Target),
                ActionType.ExecuteCommand => "⚙️",
                ActionType.Shortcut => "⌨️",
                _ => "📄"
            };
        }

        /// <summary>
        /// 根据文件路径获取 Emoji
        /// </summary>
        private static string GetFileEmoji(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
                return "📄";

            try
            {
                // 1. 尝试匹配文件名
                string fileName = System.IO.Path.GetFileNameWithoutExtension(filePath);
                foreach (var pair in AppNameEmojis)
                {
                    if (fileName.Contains(pair.Key, StringComparison.OrdinalIgnoreCase))
                        return pair.Value;
                }

                // 2. 尝试匹配扩展名
                string extension = System.IO.Path.GetExtension(filePath);
                if (FileExtensionEmojis.TryGetValue(extension, out string? emoji))
                    return emoji;

                // 3. 可执行文件默认图标
                if (extension.Equals(".exe", StringComparison.OrdinalIgnoreCase))
                    return "⚙️";
            }
            catch { }

            return "📄";
        }

        /// <summary>
        /// 根据 URL 获取 Emoji
        /// </summary>
        private static string GetUrlEmoji(string url)
        {
            if (string.IsNullOrEmpty(url))
                return "🌐";

            try
            {
                if (Uri.TryCreate(url, UriKind.Absolute, out Uri? uri))
                {
                    string host = uri.Host.ToLowerInvariant();
                    
                    // 移除 www. 前缀
                    if (host.StartsWith("www."))
                        host = host.Substring(4);

                    // 尝试匹配域名
                    foreach (var pair in DomainEmojis)
                    {
                        if (host.Contains(pair.Key, StringComparison.OrdinalIgnoreCase))
                            return pair.Value;
                    }
                }
            }
            catch { }

            return "🌐";
        }
    }
}
