using System;
using System.Windows.Data;
using System.Windows.Markup;

namespace QuickWheel.Services
{
    /// <summary>
    /// 本地化标记扩展，用于 XAML 绑定
    /// 用法：Text="{loc:Loc Key=Settings}"
    /// </summary>
    [MarkupExtensionReturnType(typeof(string))]
    public class LocExtension : MarkupExtension
    {
        public string Key { get; set; } = string.Empty;

        public LocExtension() { }

        public LocExtension(string key)
        {
            Key = key;
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            var binding = new Binding($"[{Key}]")
            {
                Source = LocalizationService.Instance,
                Mode = BindingMode.OneWay
            };
            return binding.ProvideValue(serviceProvider);
        }
    }
}
