using System.Windows;
using System.Windows.Controls;

namespace SaveFile_Manager {
    public partial class ConfirmDialog : Window {
        public MessageBoxResult Result { get; private set; }

        public ConfirmDialog(string message, bool isConfirmation = true)
        {
            InitializeComponent();
            MessageText.Text = message;

            if (!isConfirmation)
            {
                NoButton.Visibility = Visibility.Collapsed;
                YesButton.Content = "OK"; // ← Change "Yes" to "OK"
                YesButton.HorizontalAlignment = HorizontalAlignment.Center;
            }
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
