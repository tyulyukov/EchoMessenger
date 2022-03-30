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

            var firebaseUser = await Database.LoginUserAsync(username, password);

            if (firebaseUser == null)
            {
                MessageBox.Show("Invalid username or password");
                return;
            }

            LogInManager.Remember(firebaseUser.Object);

            MessengerWindow messengerWindow = new MessengerWindow(firebaseUser);
            messengerWindow.Show();
            this.Close();
        }

        private void RegisterButton_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            RegistrationWindow registrationWindow = new RegistrationWindow();
            registrationWindow.Show();
            this.Close();
        }
    }
}
