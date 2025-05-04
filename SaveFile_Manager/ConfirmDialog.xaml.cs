using System.Windows;


namespace SaveFile_Manager {
    public partial class ConfirmDialog : Window {
        public MessageBoxResult Result { get; private set; }

        public ConfirmDialog(string message)
        {
            InitializeComponent();
            MessageText.Text = message;  // Assuming you have a TextBlock named "MessageText" in your XAML
        }

        private void YesButton_Click(object sender, RoutedEventArgs e)
        {
            Result = MessageBoxResult.Yes;
            Close();
        }

        private void NoButton_Click(object sender, RoutedEventArgs e)
        {
            Result = MessageBoxResult.No;
            Close();
        }
    }
}
