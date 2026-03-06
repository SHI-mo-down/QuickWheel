using System.Windows;

namespace QuickWheel
{
    public partial class MainWindow : Window
    {
        private static MainWindow? _instance;

        public static MainWindow Instance => _instance ??= new MainWindow();

        public MainWindow()
        {
            InitializeComponent();
            _instance = this;
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            Hide();
        }
    }
}
