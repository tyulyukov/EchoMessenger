using EchoMessenger.Entities;
using EchoMessenger.Helpers;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace EchoMessenger
{
    /// <summary>
    /// Interaction logic for RegistrationWindow.xaml
    /// </summary>
    public partial class RegistrationWindow : Window
    {
        public RegistrationWindow()
        {
            InitializeComponent();
        }

        private void LoginButton_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            LoginWindow loginWindow = new LoginWindow();
            loginWindow.Show();
            this.Close();
        }

        private async void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            var username = UsernameBox.Text;
            var password = PasswordBox.Password;

            if (String.IsNullOrWhiteSpace(username) || String.IsNullOrWhiteSpace(password))
                return;

            if (!LogInManager.ValidateUsername(username))
            {
                MessageBox.Show("Username must contain at least 5 symbols and less than 20 symbols. Username must have only latin letters or/and digits. Allowed special symbols: . - _");
                return;
            }

            if (!await Database.IsUsernameFree(username))
            {
                MessageBox.Show("This username is busy");
                return;
            }

            if (!LogInManager.ValidatePassword(password))
            {
                MessageBox.Show("Password must contain at least 8 symbols. It must have letters and digits");
                return;
            }

            User user = new User(username, password);

            var firebaseUser = await Database.RegisterUserAsync(user);

            if (firebaseUser == null)
            {
                MessageBox.Show("Oops! Something went wrong...");
                return;
            }

            LogInManager.Remember(user);

            MessengerWindow messengerWindow = new MessengerWindow(firebaseUser);
            messengerWindow.Show();
            this.Close();
        }
    }
}
