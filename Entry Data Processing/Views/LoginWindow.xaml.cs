using Entry_Data_Processing.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace Entry_Data_Processing.Views
{
    /// <summary>
    /// Interaction logic for LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {
        private readonly LoginViewModel _viewModel;
        public LoginWindow(LoginViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            DataContext = _viewModel;
            _viewModel.LoginSucceeded += OnLoginSucceeded;
        }

        private async void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            var loginButton = (Button)sender;
            loginButton.IsEnabled = false;
            try
            {
                var password = PasswordBox.Visibility == Visibility.Visible
                    ? PasswordBox.Password
                    : VisiblePasswordBox.Text;
                await _viewModel.LoginCommand.ExecuteAsync(password);
            }
            finally
            {
                loginButton.IsEnabled = true;
            }
        }

        private void PasswordToggle_Click(object sender, RoutedEventArgs e)
        {
            if (PasswordBox.Visibility == Visibility.Visible)
            {
                VisiblePasswordBox.Text = PasswordBox.Password;
                PasswordBox.Visibility = Visibility.Collapsed;
                VisiblePasswordBox.Visibility = Visibility.Visible;
                PasswordToggleButton.ToolTip = "Sembunyikan password";
            }
            else
            {
                PasswordBox.Password = VisiblePasswordBox.Text;
                VisiblePasswordBox.Visibility = Visibility.Collapsed;
                PasswordBox.Visibility = Visibility.Visible;
                PasswordToggleButton.ToolTip = "Tampilkan password";
            }
        }

        private void OnLoginSucceeded()
        {
            this.DialogResult = true;
            this.Close();
        }
    }
}
