using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace QuickWheel.Controls
{
    public class CircularPanel : Canvas
    {
        public static readonly DependencyProperty ItemCountProperty =
            DependencyProperty.Register(nameof(ItemCount), typeof(int), typeof(CircularPanel),
                new PropertyMetadata(0, OnLayoutPropertyChanged));

        public int ItemCount
        {
            get => (int)GetValue(ItemCountProperty);
            set => SetValue(ItemCountProperty, value);
        }

        public static readonly DependencyProperty InnerRadiusProperty =
            DependencyProperty.Register(nameof(InnerRadius), typeof(double), typeof(CircularPanel),
                new PropertyMetadata(0.0, OnLayoutPropertyChanged));

        public double InnerRadius
        {
            get => (double)GetValue(InnerRadiusProperty);
            set => SetValue(InnerRadiusProperty, value);
        }

        public static readonly DependencyProperty OuterRadiusProperty =
            DependencyProperty.Register(nameof(OuterRadius), typeof(double), typeof(CircularPanel),
                new PropertyMetadata(100.0, OnLayoutPropertyChanged));

        public double OuterRadius
        {
            get => (double)GetValue(OuterRadiusProperty);
            set => SetValue(OuterRadiusProperty, value);
        }

        public static readonly DependencyProperty CenterXProperty =
            DependencyProperty.Register(nameof(CenterX), typeof(double), typeof(CircularPanel),
                new PropertyMetadata(100.0, OnLayoutPropertyChanged));

        public double CenterX
        {
            get => (double)GetValue(CenterXProperty);
            set => SetValue(CenterXProperty, value);
        }

        public static readonly DependencyProperty CenterYProperty =
            DependencyProperty.Register(nameof(CenterY), typeof(double), typeof(CircularPanel),
                new PropertyMetadata(100.0, OnLayoutPropertyChanged));

        public double CenterY
        {
            get => (double)GetValue(CenterYProperty);
            set => SetValue(CenterYProperty, value);
        }

        private static void OnLayoutPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is CircularPanel panel)
            {
                panel.InvalidateArrange();
            }
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            double width = OuterRadius * 2;
            double height = OuterRadius * 2;

            if (!double.IsInfinity(availableSize.Width))
                width = availableSize.Width;
            if (!double.IsInfinity(availableSize.Height))
                height = availableSize.Height;

            foreach (UIElement child in Children)
            {
                child.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            }

            return new Size(width, height);
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            int count = ItemCount;
            if (count == 0 || Children.Count == 0)
                return finalSize;

            double angleStep = 360.0 / count;
            double outerRadius = OuterRadius;
            double innerRadius = InnerRadius;

            for (int i = 0; i < Children.Count; i++)
            {
                UIElement child = Children[i];
                
                if (child is RadialSectorControl sector)
                {
                    double startAngle, endAngle;
                    
                    if (count == 1)
                    {
                        // 只有一个项目时，显示完整圆环（稍微留一点缝隙避免渲染问题）
                        startAngle = 0;
                        endAngle = 2 * Math.PI - 0.001; // 接近360度但略小
                    }
                    else
                    {
                        startAngle = (i * angleStep - 90) * Math.PI / 180;
                        endAngle = ((i + 1) * angleStep - 90) * Math.PI / 180;
                    }

                    sector.StartAngle = startAngle;
                    sector.EndAngle = endAngle;
                    sector.OuterRadius = outerRadius;
                    sector.InnerRadius = innerRadius;

                    double childSize = outerRadius * 2;
                    sector.Width = childSize;
                    sector.Height = childSize;

                    sector.Arrange(new Rect(0, 0, childSize, childSize));
                }
            }

            return new Size(outerRadius * 2, outerRadius * 2);
        }
    }
}
