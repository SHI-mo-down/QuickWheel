using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace QuickWheel.Controls
{
    public partial class RadialSectorControl : UserControl
    {
        public RadialSectorControl()
        {
            InitializeComponent();
            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            // 确保初始状态正确
            HasIconImage = IconSource != null;
        }

        public static readonly DependencyProperty StartAngleProperty =
            DependencyProperty.Register(nameof(StartAngle), typeof(double), typeof(RadialSectorControl),
                new PropertyMetadata(0.0, OnGeometryPropertyChanged));

        public double StartAngle
        {
            get => (double)GetValue(StartAngleProperty);
            set => SetValue(StartAngleProperty, value);
        }

        public static readonly DependencyProperty EndAngleProperty =
            DependencyProperty.Register(nameof(EndAngle), typeof(double), typeof(RadialSectorControl),
                new PropertyMetadata(1.047, OnGeometryPropertyChanged));

        public double EndAngle
        {
            get => (double)GetValue(EndAngleProperty);
            set => SetValue(EndAngleProperty, value);
        }

        public static readonly DependencyProperty InnerRadiusProperty =
            DependencyProperty.Register(nameof(InnerRadius), typeof(double), typeof(RadialSectorControl),
                new PropertyMetadata(0.0, OnGeometryPropertyChanged));

        public double InnerRadius
        {
            get => (double)GetValue(InnerRadiusProperty);
            set => SetValue(InnerRadiusProperty, value);
        }

        public static readonly DependencyProperty OuterRadiusProperty =
            DependencyProperty.Register(nameof(OuterRadius), typeof(double), typeof(RadialSectorControl),
                new PropertyMetadata(100.0, OnGeometryPropertyChanged));

        public double OuterRadius
        {
            get => (double)GetValue(OuterRadiusProperty);
            set => SetValue(OuterRadiusProperty, value);
        }

        public static readonly DependencyProperty IconProperty =
            DependencyProperty.Register(nameof(Icon), typeof(string), typeof(RadialSectorControl),
                new PropertyMetadata(string.Empty));

        public string Icon
        {
            get => (string)GetValue(IconProperty);
            set => SetValue(IconProperty, value);
        }

        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register(nameof(Text), typeof(string), typeof(RadialSectorControl),
                new PropertyMetadata(string.Empty));

        public string Text
        {
            get => (string)GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }

        public static readonly DependencyProperty IsSelectedProperty =
            DependencyProperty.Register(nameof(IsSelected), typeof(bool), typeof(RadialSectorControl),
                new PropertyMetadata(false, OnVisualStateChanged));

        public bool IsSelected
        {
            get => (bool)GetValue(IsSelectedProperty);
            set => SetValue(IsSelectedProperty, value);
        }

        public static readonly DependencyProperty SectorIndexProperty =
            DependencyProperty.Register(nameof(SectorIndex), typeof(int), typeof(RadialSectorControl),
                new PropertyMetadata(0));

        public int SectorIndex
        {
            get => (int)GetValue(SectorIndexProperty);
            set => SetValue(SectorIndexProperty, value);
        }

        public static readonly DependencyProperty CommandProperty =
            DependencyProperty.Register(nameof(Command), typeof(ICommand), typeof(RadialSectorControl),
                new PropertyMetadata(null));

        public ICommand Command
        {
            get => (ICommand)GetValue(CommandProperty);
            set => SetValue(CommandProperty, value);
        }

        public static readonly DependencyProperty CommandParameterProperty =
            DependencyProperty.Register(nameof(CommandParameter), typeof(object), typeof(RadialSectorControl),
                new PropertyMetadata(null));

        public object CommandParameter
        {
            get => GetValue(CommandParameterProperty);
            set => SetValue(CommandParameterProperty, value);
        }

        public static readonly DependencyProperty IconSizeProperty =
            DependencyProperty.Register(nameof(IconSize), typeof(double), typeof(RadialSectorControl),
                new PropertyMetadata(24.0));

        public double IconSize
        {
            get => (double)GetValue(IconSizeProperty);
            set => SetValue(IconSizeProperty, value);
        }

        public static readonly DependencyProperty TextSizeProperty =
            DependencyProperty.Register(nameof(TextSize), typeof(double), typeof(RadialSectorControl),
                new PropertyMetadata(10.0));

        public double TextSize
        {
            get => (double)GetValue(TextSizeProperty);
            set => SetValue(TextSizeProperty, value);
        }

        public static readonly DependencyProperty ShowLabelProperty =
            DependencyProperty.Register(nameof(ShowLabel), typeof(bool), typeof(RadialSectorControl),
                new PropertyMetadata(true));

        public bool ShowLabel
        {
            get => (bool)GetValue(ShowLabelProperty);
            set => SetValue(ShowLabelProperty, value);
        }

        public static readonly DependencyProperty StrokeThicknessProperty =
            DependencyProperty.Register(nameof(StrokeThickness), typeof(double), typeof(RadialSectorControl),
                new PropertyMetadata(1.0));

        public double StrokeThickness
        {
            get => (double)GetValue(StrokeThicknessProperty);
            set => SetValue(StrokeThicknessProperty, value);
        }

        public static readonly DependencyProperty SectorGeometryProperty =
            DependencyProperty.Register(nameof(SectorGeometry), typeof(Geometry), typeof(RadialSectorControl),
                new PropertyMetadata(null));

        public Geometry SectorGeometry
        {
            get => (Geometry)GetValue(SectorGeometryProperty);
            set => SetValue(SectorGeometryProperty, value);
        }

        public static readonly DependencyProperty SectorFillProperty =
            DependencyProperty.Register(nameof(SectorFill), typeof(Brush), typeof(RadialSectorControl),
                new PropertyMetadata(new SolidColorBrush(Color.FromArgb(80, 0, 0, 0))));

        public Brush SectorFill
        {
            get => (Brush)GetValue(SectorFillProperty);
            set => SetValue(SectorFillProperty, value);
        }

        public static readonly DependencyProperty SectorStrokeProperty =
            DependencyProperty.Register(nameof(SectorStroke), typeof(Brush), typeof(RadialSectorControl),
                new PropertyMetadata(new SolidColorBrush(Color.FromArgb(60, 255, 255, 255))));

        public Brush SectorStroke
        {
            get => (Brush)GetValue(SectorStrokeProperty);
            set => SetValue(SectorStrokeProperty, value);
        }

        public static readonly DependencyProperty HighlightFillProperty =
            DependencyProperty.Register(nameof(HighlightFill), typeof(Brush), typeof(RadialSectorControl),
                new PropertyMetadata(new SolidColorBrush(Color.FromArgb(255, 68, 68, 68))));

        public Brush HighlightFill
        {
            get => (Brush)GetValue(HighlightFillProperty);
            set => SetValue(HighlightFillProperty, value);
        }

        public static readonly DependencyProperty IconXProperty =
            DependencyProperty.Register(nameof(IconX), typeof(double), typeof(RadialSectorControl),
                new PropertyMetadata(0.0));

        public double IconX
        {
            get => (double)GetValue(IconXProperty);
            set => SetValue(IconXProperty, value);
        }

        public static readonly DependencyProperty IconYProperty =
            DependencyProperty.Register(nameof(IconY), typeof(double), typeof(RadialSectorControl),
                new PropertyMetadata(0.0));

        public double IconY
        {
            get => (double)GetValue(IconYProperty);
            set => SetValue(IconYProperty, value);
        }

        public static readonly DependencyProperty TextXProperty =
            DependencyProperty.Register(nameof(TextX), typeof(double), typeof(RadialSectorControl),
                new PropertyMetadata(0.0));

        public double TextX
        {
            get => (double)GetValue(TextXProperty);
            set => SetValue(TextXProperty, value);
        }

        public static readonly DependencyProperty TextYProperty =
            DependencyProperty.Register(nameof(TextY), typeof(double), typeof(RadialSectorControl),
                new PropertyMetadata(0.0));

        public double TextY
        {
            get => (double)GetValue(TextYProperty);
            set => SetValue(TextYProperty, value);
        }

        // 程序图标相关属性
        public static readonly DependencyProperty IconSourceProperty =
            DependencyProperty.Register(nameof(IconSource), typeof(ImageSource), typeof(RadialSectorControl),
                new PropertyMetadata(null, OnIconSourceChanged));

        public ImageSource? IconSource
        {
            get => (ImageSource?)GetValue(IconSourceProperty);
            set => SetValue(IconSourceProperty, value);
        }

        public static readonly DependencyProperty HasIconImageProperty =
            DependencyProperty.Register(nameof(HasIconImage), typeof(bool), typeof(RadialSectorControl),
                new PropertyMetadata(false));

        public bool HasIconImage
        {
            get => (bool)GetValue(HasIconImageProperty);
            set => SetValue(HasIconImageProperty, value);
        }

        private static void OnIconSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is RadialSectorControl control)
            {
                control.HasIconImage = e.NewValue != null;
            }
        }

        // 使用静态字段避免重复创建 StringBuilder
        private static readonly System.Text.StringBuilder _pathBuilder = new(256);

        private static void OnGeometryPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is RadialSectorControl control)
            {
                // 使用延迟更新避免频繁重绘
                control.Dispatcher.BeginInvoke(() => control.UpdateSectorGeometry(), System.Windows.Threading.DispatcherPriority.Render);
            }
        }

        private static void OnVisualStateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is RadialSectorControl control)
            {
                control.UpdateVisualState();
            }
        }

        private void UpdateVisualState()
        {
            if (IsSelected)
            {
                // 使用配置的高亮颜色填充
                SectorPath.Fill = HighlightFill;
                
                // 播放选中动画
                PlayStoryboard("ScaleUpStoryboard");
                PlayStoryboard("ShadowInStoryboard");
                PlayStoryboard("OpacityInStoryboard");
            }
            else
            {
                // 恢复原始填充色
                SectorPath.Fill = SectorFill;
                
                // 播放取消选中动画
                PlayStoryboard("ScaleDownStoryboard");
                PlayStoryboard("ShadowOutStoryboard");
                PlayStoryboard("OpacityOutStoryboard");
            }
        }

        private void PlayStoryboard(string resourceKey)
        {
            if (Resources[resourceKey] is Storyboard storyboard)
            {
                storyboard.Begin();
            }
        }

        private void UpdateSectorGeometry()
        {
            double innerRadius = InnerRadius;
            double outerRadius = OuterRadius;
            double startAngle = StartAngle;
            double endAngle = EndAngle;

            if (outerRadius <= 0)
            {
                SectorGeometry = Geometry.Empty;
                return;
            }

            double controlSize = outerRadius * 2;
            double centerX = outerRadius;
            double centerY = outerRadius;

            double startRad = startAngle;
            double endRad = endAngle;

            Point innerStartPoint = new Point(
                centerX + innerRadius * Math.Cos(startRad),
                centerY + innerRadius * Math.Sin(startRad));

            Point innerEndPoint = new Point(
                centerX + innerRadius * Math.Cos(endRad),
                centerY + innerRadius * Math.Sin(endRad));

            Point outerStartPoint = new Point(
                centerX + outerRadius * Math.Cos(startRad),
                centerY + outerRadius * Math.Sin(startRad));

            Point outerEndPoint = new Point(
                centerX + outerRadius * Math.Cos(endRad),
                centerY + outerRadius * Math.Sin(endRad));

            bool isLargeArc = (endAngle - startAngle) > Math.PI;

            // 使用 StringBuilder 构建路径数据，避免字符串拼接开销
            _pathBuilder.Clear();
            _pathBuilder.Append('M');
            _pathBuilder.Append(outerStartPoint.X.ToString("F2"));
            _pathBuilder.Append(',');
            _pathBuilder.Append(outerStartPoint.Y.ToString("F2"));
            _pathBuilder.Append(' ');
            _pathBuilder.Append('A');
            _pathBuilder.Append(outerRadius.ToString("F2"));
            _pathBuilder.Append(',');
            _pathBuilder.Append(outerRadius.ToString("F2"));
            _pathBuilder.Append(" 0 ");
            _pathBuilder.Append(isLargeArc ? '1' : '0');
            _pathBuilder.Append(" 1 ");
            _pathBuilder.Append(outerEndPoint.X.ToString("F2"));
            _pathBuilder.Append(',');
            _pathBuilder.Append(outerEndPoint.Y.ToString("F2"));
            _pathBuilder.Append(' ');
            _pathBuilder.Append('L');
            _pathBuilder.Append(innerEndPoint.X.ToString("F2"));
            _pathBuilder.Append(',');
            _pathBuilder.Append(innerEndPoint.Y.ToString("F2"));
            _pathBuilder.Append(' ');
            _pathBuilder.Append('A');
            _pathBuilder.Append(innerRadius.ToString("F2"));
            _pathBuilder.Append(',');
            _pathBuilder.Append(innerRadius.ToString("F2"));
            _pathBuilder.Append(" 0 ");
            _pathBuilder.Append(isLargeArc ? '1' : '0');
            _pathBuilder.Append(" 0 ");
            _pathBuilder.Append(innerStartPoint.X.ToString("F2"));
            _pathBuilder.Append(',');
            _pathBuilder.Append(innerStartPoint.Y.ToString("F2"));
            _pathBuilder.Append(' ');
            _pathBuilder.Append('Z');

            try
            {
                SectorGeometry = Geometry.Parse(_pathBuilder.ToString());
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Geometry parse error: {ex.Message}");
                SectorGeometry = Geometry.Empty;
                return;
            }

            // 计算内容位置（扇区的视觉中心）
            double midAngle = (startAngle + endAngle) / 2;
            double contentRadius = (innerRadius + outerRadius) / 2;

            IconX = centerX + contentRadius * Math.Cos(midAngle) - IconSize / 2;
            IconY = centerY + contentRadius * Math.Sin(midAngle) - IconSize / 2;

            TextX = centerX + contentRadius * Math.Cos(midAngle) - 15;
            TextY = centerY + contentRadius * Math.Sin(midAngle) + IconSize / 2 - 2;
        }

        protected override void OnMouseEnter(MouseEventArgs e)
        {
            base.OnMouseEnter(e);
            if (!IsSelected)
            {
                SectorPath.Opacity = 0.9;
                if (SectorShadow != null)
                {
                    SectorShadow.BlurRadius = 8;
                    SectorShadow.Opacity = 0.5;
                }
            }
        }

        protected override void OnMouseLeave(MouseEventArgs e)
        {
            base.OnMouseLeave(e);
            if (!IsSelected)
            {
                SectorPath.Opacity = 0.6;
                if (SectorShadow != null)
                {
                    SectorShadow.BlurRadius = 0;
                    SectorShadow.Opacity = 0;
                }
            }
        }

        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonUp(e);
            if (Command?.CanExecute(CommandParameter) == true)
            {
                Command.Execute(CommandParameter);
            }
            e.Handled = true;
        }
    }
}
