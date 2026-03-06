using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using QuickWheel.Models;

namespace QuickWheel.Services
{
    /// <summary>
    /// 提供参数模板替换服务
    /// 支持的模板:
    /// {date} - 当前日期 (yyyy-MM-dd)
    /// {time} - 当前时间 (HH:mm:ss)
    /// {datetime} - 完整日期时间
    /// {year} - 年份
    /// {month} - 月份
    /// {day} - 日期
    /// {clipboard} - 剪贴板内容
    /// {random:N} - 随机数 0-N
    /// {env:NAME} - 环境变量
    /// {guid} - GUID
    /// {timestamp} - Unix时间戳
    /// </summary>
    public class TemplateService
    {
        private static TemplateService? _instance;
        public static TemplateService Instance => _instance ??= new TemplateService();

        // 随机数生成器
        private readonly Random _random = new Random();
        
        // 执行计数器（用于 {count} 模板）
        private readonly Dictionary<string, int> _counters = new Dictionary<string, int>();

        private TemplateService() { }

        /// <summary>
        /// 替换字符串中的所有模板变量
        /// </summary>
        public string ReplaceTemplates(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            var now = DateTime.Now;
            string result = input;

            // 日期时间模板
            result = result.Replace("{date}", now.ToString("yyyy-MM-dd"));
            result = result.Replace("{time}", now.ToString("HH:mm:ss"));
            result = result.Replace("{datetime}", now.ToString("yyyy-MM-dd HH:mm:ss"));
            result = result.Replace("{year}", now.Year.ToString());
            result = result.Replace("{month}", now.Month.ToString("D2"));
            result = result.Replace("{day}", now.Day.ToString("D2"));
            result = result.Replace("{hour}", now.Hour.ToString("D2"));
            result = result.Replace("{minute}", now.Minute.ToString("D2"));
            result = result.Replace("{second}", now.Second.ToString("D2"));

            // 星期相关
            result = result.Replace("{weekday}", now.ToString("dddd"));
            result = result.Replace("{weekdayshort}", now.ToString("ddd"));

            // 时间戳
            result = result.Replace("{timestamp}", DateTimeOffset.Now.ToUnixTimeSeconds().ToString());
            result = result.Replace("{ticks}", DateTime.Now.Ticks.ToString());

            // GUID
            result = result.Replace("{guid}", Guid.NewGuid().ToString("N"));
            result = result.Replace("{guid:short}", Guid.NewGuid().ToString("N")[..8]);

            // 剪贴板内容
            result = ReplaceClipboardTemplates(result);

            // 随机数
            result = ReplaceRandomTemplates(result);

            // 环境变量
            result = ReplaceEnvironmentVariables(result);

            // 计数器
            result = ReplaceCounterTemplates(result);

            // 字符串处理
            result = ReplaceStringTemplates(result);

            return result;
        }

        /// <summary>
        /// 替换剪贴板相关模板
        /// </summary>
        private string ReplaceClipboardTemplates(string input)
        {
            string result = input;
            
            // 基础剪贴板
            if (result.Contains("{clipboard}"))
            {
                string clipboardText = GetClipboardText();
                result = result.Replace("{clipboard}", clipboardText);
            }

            // URL编码的剪贴板（用于网址参数）
            if (result.Contains("{clipboard:url}"))
            {
                string clipboardText = GetClipboardText();
                string encoded = Uri.EscapeDataString(clipboardText);
                result = result.Replace("{clipboard:url}", encoded);
            }

            // 文件名安全的剪贴板（移除非法字符）
            if (result.Contains("{clipboard:file}"))
            {
                string clipboardText = GetClipboardText();
                string safe = MakeFileNameSafe(clipboardText);
                result = result.Replace("{clipboard:file}", safe);
            }

            // 限制长度的剪贴板
            var match = Regex.Match(result, @"\{clipboard:(\d+)\}");
            while (match.Success)
            {
                int maxLength = int.Parse(match.Groups[1].Value);
                string clipboardText = GetClipboardText();
                if (clipboardText.Length > maxLength)
                {
                    clipboardText = clipboardText.Substring(0, maxLength);
                }
                result = result.Replace(match.Value, clipboardText);
                match = Regex.Match(result, @"\{clipboard:(\d+)\}");
            }

            return result;
        }

        /// <summary>
        /// 替换随机数模板 {random} 或 {random:100}
        /// </summary>
        private string ReplaceRandomTemplates(string input)
        {
            string result = input;

            // {random} - 0-9999
            if (result.Contains("{random}"))
            {
                result = result.Replace("{random}", _random.Next(0, 10000).ToString());
            }

            // {random:N} - 0-N
            var match = Regex.Match(result, @"\{random:(\d+)\}");
            while (match.Success)
            {
                int max = int.Parse(match.Groups[1].Value);
                int value = _random.Next(0, max + 1);
                result = result.Replace(match.Value, value.ToString());
                match = Regex.Match(result, @"\{random:(\d+)\}");
            }

            // {random:N,M} - N-M
            match = Regex.Match(result, @"\{random:(\d+),(\d+)\}");
            while (match.Success)
            {
                int min = int.Parse(match.Groups[1].Value);
                int max = int.Parse(match.Groups[2].Value);
                int value = _random.Next(min, max + 1);
                result = result.Replace(match.Value, value.ToString());
                match = Regex.Match(result, @"\{random:(\d+),(\d+)\}");
            }

            return result;
        }

        /// <summary>
        /// 替换环境变量模板 {env:NAME}
        /// </summary>
        private string ReplaceEnvironmentVariables(string input)
        {
            string result = input;
            var match = Regex.Match(result, @"\{env:([^}]+)\}");
            
            while (match.Success)
            {
                string varName = match.Groups[1].Value;
                string? varValue = Environment.GetEnvironmentVariable(varName);
                result = result.Replace(match.Value, varValue ?? "");
                match = Regex.Match(result, @"\{env:([^}]+)\}");
            }

            return result;
        }

        /// <summary>
        /// 替换计数器模板 {count} 或 {count:name}
        /// </summary>
        private string ReplaceCounterTemplates(string input)
        {
            string result = input;

            // {count} - 全局计数器
            if (result.Contains("{count}"))
            {
                int count = GetAndIncrementCounter("global");
                result = result.Replace("{count}", count.ToString());
            }

            // {count:name} - 命名计数器
            var match = Regex.Match(result, @"\{count:([^}]+)\}");
            while (match.Success)
            {
                string counterName = match.Groups[1].Value;
                int count = GetAndIncrementCounter(counterName);
                result = result.Replace(match.Value, count.ToString());
                match = Regex.Match(result, @"\{count:([^}]+)\}");
            }

            return result;
        }

        /// <summary>
        /// 替换字符串处理模板
        /// </summary>
        private string ReplaceStringTemplates(string input)
        {
            string result = input;

            // {newline} 或 {nl} - 换行符
            result = result.Replace("{newline}", Environment.NewLine);
            result = result.Replace("{nl}", Environment.NewLine);

            // {tab} - 制表符
            result = result.Replace("{tab}", "\t");

            // {space} - 空格（用于避免trim）
            result = result.Replace("{space}", " ");

            return result;
        }

        /// <summary>
        /// 获取并递增计数器
        /// </summary>
        private int GetAndIncrementCounter(string name)
        {
            if (!_counters.ContainsKey(name))
            {
                _counters[name] = 0;
            }
            return _counters[name]++;
        }

        /// <summary>
        /// 重置计数器（可以在设置中提供重置按钮）
        /// </summary>
        public void ResetCounter(string name)
        {
            _counters[name] = 0;
        }

        /// <summary>
        /// 重置所有计数器
        /// </summary>
        public void ResetAllCounters()
        {
            _counters.Clear();
        }

        /// <summary>
        /// 替换快捷项中的所有模板
        /// </summary>
        public ShortcutItem ProcessShortcut(ShortcutItem item)
        {
            var processed = new ShortcutItem
            {
                Id = item.Id,
                Name = item.Name,
                Icon = item.Icon,
                ActionType = item.ActionType,
                Target = ReplaceTemplates(item.Target),
                Arguments = ReplaceTemplates(item.Arguments),
                Order = item.Order
            };

            return processed;
        }

        /// <summary>
        /// 获取剪贴板文本内容
        /// </summary>
        private string GetClipboardText()
        {
            try
            {
                if (Clipboard.ContainsText())
                {
                    string text = Clipboard.GetText();
                    // 限制长度，避免命令行参数过长
                    if (text.Length > 1000)
                    {
                        text = text.Substring(0, 1000) + "...";
                    }
                    // 移除换行符，避免破坏命令行
                    text = text.Replace("\r\n", " ").Replace("\n", " ").Replace("\r", " ");
                    return text;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[TemplateService] 获取剪贴板失败: {ex.Message}");
            }
            return "";
        }

        /// <summary>
        /// 将字符串转换为安全的文件名
        /// </summary>
        private string MakeFileNameSafe(string input)
        {
            if (string.IsNullOrEmpty(input))
                return "";

            // Windows 文件名非法字符
            char[] invalidChars = Path.GetInvalidFileNameChars();
            string result = input;
            
            foreach (char c in invalidChars)
            {
                result = result.Replace(c, '_');
            }

            // 限制长度
            if (result.Length > 50)
            {
                result = result.Substring(0, 50);
            }

            return result.Trim();
        }

        /// <summary>
        /// 获取所有可用模板的说明
        /// </summary>
        public static Dictionary<string, string> GetAvailableTemplates()
        {
            return new Dictionary<string, string>
            {
                // 最常用
                ["{clipboard}"] = "剪贴板内容",
                ["{clipboard:url}"] = "URL编码的剪贴板",
                ["{clipboard:file}"] = "文件名安全的剪贴板",
                
                // 日期时间
                ["{date}"] = "当前日期 (yyyy-MM-dd)",
                ["{time}"] = "当前时间 (HH:mm:ss)",
                ["{datetime}"] = "日期时间 (yyyy-MM-dd HH:mm:ss)",
                ["{year}"] = "年份",
                ["{month}"] = "月份 (01-12)",
                ["{day}"] = "日期 (01-31)",
                ["{hour}"] = "小时 (00-23)",
                ["{minute}"] = "分钟 (00-59)",
                ["{second}"] = "秒 (00-59)",
                ["{weekday}"] = "星期几 (完整)",
                ["{weekdayshort}"] = "星期几 (缩写)",
                ["{timestamp}"] = "Unix时间戳",
                
                // 随机和唯一值
                ["{random}"] = "随机数 (0-9999)",
                ["{random:100}"] = "随机数 (0-100)",
                ["{guid}"] = "GUID (唯一标识)",
                ["{guid:short}"] = "短GUID (8位)",
                
                // 计数器
                ["{count}"] = "执行计数 (从0开始)",
                ["{count:name}"] = "命名计数器",
                
                // 环境变量
                ["{env:USERNAME}"] = "当前用户名",
                ["{env:COMPUTERNAME}"] = "计算机名",
                ["{env:USERPROFILE}"] = "用户文件夹",
                
                // 特殊字符
                ["{newline}"] = "换行符",
                ["{tab}"] = "制表符"
            };
        }
    }
}
