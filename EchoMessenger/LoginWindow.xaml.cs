using EchoMessenger.Helpers;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace EchoMessenger
{
    /// <summary>
    /// Interaction logic for LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
        }

        private async void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            var username = UsernameBox.Text;
            var password = PasswordBox.Password;

            if (String.IsNullOrWhiteSpace(username) || String.IsNullOrWhiteSpace(password))
                return;

            if (!await Database.LoginUserAsync(username, password))
            {
                MessageBox.Show("Invalid username or password");
                return;
            }

            LogInManager.Remember(Database.User.Object);

            new MessengerWindow().Show();
            this.Close();
        }

        private void RegisterButton_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            new RegistrationWindow().Show();
            this.Close();
        }
    }
}
