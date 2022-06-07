using EchoMessenger.Entities;
using EchoMessenger.Helpers;
using EchoMessenger.Helpers.Server;
using System;
using System.Net;
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
            this.Hide();
            new LoginWindow().Show();
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
                MessageBox.Show(this, "Username must contain at least 5 symbols and less than 20 symbols. Username must have only latin letters or/and digits. Allowed special symbols: . - _", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!LogInManager.ValidatePassword(password))
            {
                MessageBox.Show(this, "Password must contain at least 8 symbols. It must have letters and digits", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var response = await Database.RegisterAsync(username, password);

            if (response == null || response.StatusCode == (HttpStatusCode)0)
            {
                MessageBox.Show(this, "Can`t establish connection", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (response.StatusCode == (HttpStatusCode)500)
            {
                MessageBox.Show(this, "Oops... Something went wrong", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else if (response.StatusCode == (HttpStatusCode)201)
            {
                this.Hide();
                var window = new LoginWindow();
                window.Login(username, password);
                this.Close();
            }
            else if (response.StatusCode == (HttpStatusCode)405)
            {
                MessageBox.Show(this, "This username is not free", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
           
        }
    }
}
