using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using QuickWheel.Models;
using System.Windows;

namespace QuickWheel.ViewModels
{
    public class RadialMenuViewModel : INotifyPropertyChanged
    {
        private ObservableCollection<ShortcutItem> _shortcuts = new();
        private int _selectedIndex = -1;
        private double _menuRadius = 150;
        private double _itemSize = 60;
        private bool _showLabels = true;
        private Point _centerPosition;
        private bool _isVisible;
        private string _sectorColor = "#CC333333";
        private string _highlightColor = "#FF444444";
        private string _textColor = "#FFFFFFFF";
        private string _centerCircleColor = "#FF222222";
        private double _innerRadiusRatio = 0.35;
        private double _centerTextSize = 10;
        private bool _testMode = false;

        public ObservableCollection<ShortcutItem> Shortcuts
        {
            get => _shortcuts;
            set { _shortcuts = value; OnPropertyChanged(); OnPropertyChanged(nameof(SectorCount)); }
        }

        public int SelectedIndex
        {
            get => _selectedIndex;
            set
            {
                if (_selectedIndex != value)
                {
                    _selectedIndex = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(SelectedItem));
                }
            }
        }

        public ShortcutItem? SelectedItem => SelectedIndex >= 0 && SelectedIndex < Shortcuts.Count ? Shortcuts[SelectedIndex] : null;

        public double MenuRadius
        {
            get => _menuRadius;
            set { _menuRadius = value; OnPropertyChanged(); }
        }

        public double InnerRadius => MenuRadius * InnerRadiusRatio;

        public double ItemSize
        {
            get => _itemSize;
            set { _itemSize = value; OnPropertyChanged(); }
        }

        public bool ShowLabels
        {
            get => _showLabels;
            set { _showLabels = value; OnPropertyChanged(); }
        }

        public int SectorCount => Shortcuts.Count;

        public Point CenterPosition
        {
            get => _centerPosition;
            set { _centerPosition = value; OnPropertyChanged(); }
        }

        public bool IsVisible
        {
            get => _isVisible;
            set { _isVisible = value; OnPropertyChanged(); }
        }

        public string SectorColor
        {
            get => _sectorColor;
            set { _sectorColor = value; OnPropertyChanged(); }
        }

        public string HighlightColor
        {
            get => _highlightColor;
            set { _highlightColor = value; OnPropertyChanged(); }
        }

        public string TextColor
        {
            get => _textColor;
            set { _textColor = value; OnPropertyChanged(); }
        }

        public string CenterCircleColor
        {
            get => _centerCircleColor;
            set { _centerCircleColor = value; OnPropertyChanged(); }
        }

        public double InnerRadiusRatio
        {
            get => _innerRadiusRatio;
            set { _innerRadiusRatio = value; OnPropertyChanged(); }
        }

        public double CenterTextSize
        {
            get => _centerTextSize;
            set { _centerTextSize = value; OnPropertyChanged(); }
        }

        public bool TestMode
        {
            get => _testMode;
            set { _testMode = value; OnPropertyChanged(); }
        }

        public ICommand? SelectItemCommand { get; set; }

        public RadialMenuViewModel()
        {
            LoadDefaultShortcuts();
        }

        public RadialMenuViewModel(IEnumerable<ShortcutItem> shortcuts)
        {
            Shortcuts = new ObservableCollection<ShortcutItem>(shortcuts);
        }

        public void LoadDefaultShortcuts()
        {
            var config = AppConfig.CreateDefault();
            Shortcuts = new ObservableCollection<ShortcutItem>(config.Shortcuts.OrderBy(s => s.Order));
            MenuRadius = config.Wheel.WheelRadius;
            InnerRadiusRatio = config.Wheel.InnerRadiusRatio;
            ItemSize = config.Wheel.ItemSize;
            ShowLabels = config.Wheel.ShowLabels;
        }

        public void UpdateFromConfig(AppConfig config)
        {
            Shortcuts = new ObservableCollection<ShortcutItem>(config.Shortcuts.OrderBy(s => s.Order));
            MenuRadius = config.Wheel.WheelRadius;
            InnerRadiusRatio = config.Wheel.InnerRadiusRatio;
            ItemSize = config.Wheel.ItemSize;
            ShowLabels = config.Wheel.ShowLabels;
            SectorColor = config.Wheel.SectorColor;
            HighlightColor = config.Wheel.HighlightColor;
            TextColor = config.Wheel.TextColor;
            CenterCircleColor = config.Wheel.CenterCircleColor;
            InnerRadiusRatio = config.Wheel.InnerRadiusRatio;
            CenterTextSize = config.Wheel.CenterTextSize;
            TestMode = config.Wheel.TestMode;
        }

        public void ResetSelection()
        {
            SelectedIndex = -1;
        }

        public void Clear()
        {
            Shortcuts.Clear();
            ResetSelection();
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
