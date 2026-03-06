using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace QuickWheel.Views
{
    public partial class ColorPickerDialog : Window
    {
        public string SelectedColor { get; private set; } = "#FFFFFFFF";

        // 预设颜色表 - 包含常用颜色和透明度选项
        private readonly string[,] _colorPalette = {
            // 灰度
            {"#FF000000", "#FF333333", "#FF666666", "#FF999999", "#FFCCCCCC", "#FFFFFFFF"},
            // 红色系
            {"#FF8B0000", "#FFFF0000", "#FFFF6347", "#FFFF7F50", "#FFFFA07A", "#FFFF4500"},
            // 橙色系
            {"#FFFF8C00", "#FFFFA500", "#FFFFD700", "#FFFFB347", "#FFFFDAB9", "#FFFFE4B5"},
            // 黄色系
            {"#FFFFD700", "#FFFFFF00", "#FFFFFACD", "#FFFFFACD", "#FFFFFFE0", "#FFFFF8DC"},
            // 绿色系
            {"#FF006400", "#FF228B22", "#FF32CD32", "#FF00FF00", "#FF90EE90", "#FF98FB98"},
            // 青色系
            {"#FF008B8B", "#FF20B2AA", "#FF48D1CC", "#FF00FFFF", "#FFAFEEEE", "#FFE0FFFF"},
            // 蓝色系
            {"#FF00008B", "#FF0000CD", "#FF4169E1", "#FF1E90FF", "#FF87CEEB", "#FFB0E0E6"},
            // 紫色系
            {"#FF4B0082", "#FF800080", "#FF8A2BE2", "#FF9370DB", "#FFBA55D3", "#FFDDA0DD"},
            // 粉色系
            {"#FFFF1493", "#FFFF69B4", "#FFFFB6C1", "#FFFFC0CB", "#FFFFE4E1", "#FFFFF0F5"},
            // 棕色系
            {"#FF8B4513", "#FFA0522D", "#FFCD853F", "#FFD2691E", "#FFDEB887", "#FFF4A460"},
            // 半透明深色 (适合扇区)
            {"#CC000000", "#CC333333", "#CC666666", "#CC1E90FF", "#CC4169E1", "#CC800080"},
            // 半透明浅色
            {"#80FFFFFF", "#80CCCCCC", "#80999999", "#8066CCFF", "#8099CCFF", "#80FFCCFF"}
        };

        public ColorPickerDialog(string defaultColor = "#FFFFFFFF")
        {
            InitializeComponent();
            SelectedColor = defaultColor;
            BuildColorPalette();
            UpdatePreview();
        }

        private void BuildColorPalette()
        {
            int rows = _colorPalette.GetLength(0);
            int cols = _colorPalette.GetLength(1);

            for (int row = 0; row < rows; row++)
            {
                // 添加行分隔
                if (row > 0)
                {
                    ColorPanel.Children.Add(new Border
                    {
                        Height = 8,
                        Background = Brushes.Transparent
                    });
                }

                var rowPanel = new WrapPanel();

                for (int col = 0; col < cols; col++)
                {
                    string colorString = _colorPalette[row, col];
                    var colorButton = CreateColorButton(colorString);
                    rowPanel.Children.Add(colorButton);
                }

                ColorPanel.Children.Add(rowPanel);
            }
        }

        private Button CreateColorButton(string colorString)
        {
            var button = new Button
            {
                Width = 32,
                Height = 32,
                Margin = new Thickness(2),
                Background = (Brush)new BrushConverter().ConvertFromString(colorString)!,
                BorderBrush = Brushes.Gray,
                BorderThickness = new Thickness(1),
                Tag = colorString,
                ToolTip = colorString
            };

            button.Click += (s, e) =>
            {
                SelectedColor = (string)((Button)s!).Tag!;
                UpdatePreview();
            };

            return button;
        }

        private void UpdatePreview()
        {
            PreviewBorder.Background = (Brush)new BrushConverter().ConvertFromString(SelectedColor)!;
            ColorValueText.Text = SelectedColor;
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
