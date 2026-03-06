using System.Windows;

namespace QuickWheel.Views
{
    public partial class InputDialog : Window
    {
        public new string Title { get; set; }
        public string Message { get; set; }
        public string DefaultValue { get; set; }
        public string ResponseText { get; private set; } = "";

        public InputDialog(string title, string message, string defaultValue = "")
        {
            InitializeComponent();
            base.Title = title;
            Message = message;
            DefaultValue = defaultValue;
            DataContext = this;
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            ResponseText = ResponseTextBox.Text;
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
