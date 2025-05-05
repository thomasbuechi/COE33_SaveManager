using System.Windows;

namespace SaveFile_Manager
{
    public partial class InputDialog : Window {
        public string InputText { get; private set; }

        public InputDialog(string defaultText = "")
        {
            InitializeComponent();
            InputBox.Text = defaultText;
            InputBox.Focus();
            InputBox.SelectAll();
        }

        private void OK_Click(object sender, RoutedEventArgs e)
        {
            InputText = InputBox.Text;
            DialogResult = true;
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}