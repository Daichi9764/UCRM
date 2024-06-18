using System.Windows;

namespace KNXBoostDesktop
{
    public partial class CustomMessageBox : Window
    {
        public CustomMessageBox(string message)
        {
            InitializeComponent();
            MessageText.Text = message;
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }

        public static bool? Show(string message)
        {
            CustomMessageBox msgBox = new CustomMessageBox(message);
            return msgBox.ShowDialog();
        }
    }
}