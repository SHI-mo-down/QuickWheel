using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using QuickWheel.Models;

namespace QuickWheel.Services
{
    public class JsonDataService
    {
        private static readonly Lazy<JsonDataService> _instance = new(() => new JsonDataService());

        // 静态 JsonSerializerSettings 避免重复创建
        private static readonly JsonSerializerSettings _jsonSettings = new()
        {
            Formatting = Formatting.Indented,
            NullValueHandling = NullValueHandling.Ignore,
            DefaultValueHandling = DefaultValueHandling.Ignore
        };

        private readonly string _configPath;
        private AppConfig _config;

        public event EventHandler? ConfigChanged;

        public static JsonDataService Instance => _instance.Value;

        public AppConfig Config => _config;

        private JsonDataService()
        {
            string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string configDir = Path.Combine(appDataPath, "QuickWheel");
            
            if (!Directory.Exists(configDir))
            {
                Directory.CreateDirectory(configDir);
            }

            _configPath = Path.Combine(configDir, "config.json");
            _config = GetDefaultConfig();
        }

        public AppConfig LoadConfig()
        {
            try
            {
                if (File.Exists(_configPath))
                {
                    string json = File.ReadAllText(_configPath);
                    var loaded = JsonConvert.DeserializeObject<AppConfig>(json);
                    if (loaded != null)
                    {
                        _config = loaded;
                        
                        // 配置版本迁移
                        MigrateConfigIfNeeded();
                        
                        System.Diagnostics.Debug.WriteLine($"[JsonDataService] 配置已加载: {_configPath}");
                        return _config;
                    }
                }

                System.Diagnostics.Debug.WriteLine("[JsonDataService] 配置文件不存在，创建默认配置");
                _config = GetDefaultConfig();
                SaveConfig();
                return _config;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[JsonDataService] 加载配置失败: {ex.Message}");
                _config = GetDefaultConfig();
                return _config;
            }
        }
        
        /// <summary>
        /// 配置版本迁移
        /// </summary>
        private void MigrateConfigIfNeeded()
        {
            bool needsSave = false;
            
            // 版本 0 -> 1: 添加 ConfigVersion 和 Language
            if (_config.ConfigVersion < 1)
            {
                _config.ConfigVersion = 1;
                if (string.IsNullOrEmpty(_config.Language))
                {
                    _config.Language = "zh-CN";
                }
                needsSave = true;
                System.Diagnostics.Debug.WriteLine("[JsonDataService] 配置已从版本 0 迁移到 1");
            }
            
            // 确保必要字段不为 null
            if (_config.Hotkey == null)
            {
                _config.Hotkey = new HotkeySettings();
                needsSave = true;
            }
            if (_config.Wheel == null)
            {
                _config.Wheel = new WheelSettings();
                needsSave = true;
            }
            if (_config.Shortcuts == null)
            {
                _config.Shortcuts = new List<ShortcutItem>();
                needsSave = true;
            }
            
            if (needsSave)
            {
                SaveConfig();
            }
        }
        
        /// <summary>
        /// 重置所有配置到默认值
        /// </summary>
        public void ResetToDefaults()
        {
            try
            {
                _config = GetDefaultConfig();
                SaveConfig();
                NotifyConfigChanged();
                System.Diagnostics.Debug.WriteLine("[JsonDataService] 配置已重置为默认值");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[JsonDataService] 重置配置失败: {ex.Message}");
            }
        }
        
        /// <summary>
        /// 删除所有用户数据（包括配置文件）
        /// </summary>
        public bool DeleteAllUserData()
        {
            try
            {
                // 删除配置文件
                if (File.Exists(_configPath))
                {
                    File.Delete(_configPath);
                }
                
                // 删除配置目录
                string configDir = Path.GetDirectoryName(_configPath)!;
                if (Directory.Exists(configDir))
                {
                    Directory.Delete(configDir, true);
                }
                
                System.Diagnostics.Debug.WriteLine("[JsonDataService] 所有用户数据已删除");
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[JsonDataService] 删除用户数据失败: {ex.Message}");
                return false;
            }
        }

        public void SaveConfig()
        {
            try
            {
                string json = JsonConvert.SerializeObject(_config, _jsonSettings);
                File.WriteAllText(_configPath, json);
                System.Diagnostics.Debug.WriteLine($"[JsonDataService] 配置已保存: {_configPath}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[JsonDataService] 保存配置失败: {ex.Message}");
            }
        }

        public void NotifyConfigChanged()
        {
            ConfigChanged?.Invoke(this, EventArgs.Empty);
        }

        public AppConfig GetDefaultConfig()
        {
            return AppConfig.CreateDefault();
        }

        /// <summary>
        /// 导出配置到指定路径
        /// </summary>
        public bool ExportConfig(string exportPath)
        {
            try
            {
                string json = JsonConvert.SerializeObject(_config, _jsonSettings);
                File.WriteAllText(exportPath, json);
                System.Diagnostics.Debug.WriteLine($"[JsonDataService] 配置已导出: {exportPath}");
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[JsonDataService] 导出配置失败: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 从指定路径导入配置
        /// </summary>
        public bool ImportConfig(string importPath)
        {
            try
            {
                if (!File.Exists(importPath))
                {
                    System.Diagnostics.Debug.WriteLine($"[JsonDataService] 导入文件不存在: {importPath}");
                    return false;
                }

                string json = File.ReadAllText(importPath);
                var loaded = JsonConvert.DeserializeObject<AppConfig>(json);
                
                if (loaded == null)
                {
                    System.Diagnostics.Debug.WriteLine("[JsonDataService] 导入的配置文件格式无效");
                    return false;
                }

                // 验证必要字段
                if (loaded.Hotkey == null || loaded.Wheel == null)
                {
                    System.Diagnostics.Debug.WriteLine("[JsonDataService] 导入的配置缺少必要字段");
                    return false;
                }

                _config = loaded;
                SaveConfig();
                NotifyConfigChanged();
                
                System.Diagnostics.Debug.WriteLine($"[JsonDataService] 配置已导入: {importPath}");
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[JsonDataService] 导入配置失败: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 获取配置文件的默认导出文件名
        /// </summary>
        public string GetDefaultExportFileName()
        {
            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            return $"QuickWheel_Config_{timestamp}.json";
        }
    }
}
