using EchoMessenger.Helpers;
using EchoMessenger.Helpers.Server;
using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace EchoMessenger
{
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
            ErrorAlertTextBlock.Visibility = Visibility.Collapsed;

            var username = UsernameBox.Text.ToLower();
            var password = PasswordBox.Password;

            if (String.IsNullOrWhiteSpace(username) || String.IsNullOrWhiteSpace(password))
            {
                ErrorAlertTextBlock.Text = "Fields must be not empty";
                ErrorAlertTextBlock.Visibility = Visibility.Visible;
                return;
            }

            /*if (!LogInManager.ValidateUsername(username))
            {
                ErrorAlertTextBlock.Text = "Username must contain at least 5 symbols and less than 20 symbols. Username must have only latin letters or/and digits. Allowed special symbols: . - _";
                ErrorAlertTextBlock.Visibility = Visibility.Visible;
                return;
            }

            if (!LogInManager.ValidatePassword(password))
            {
                ErrorAlertTextBlock.Text = "Password must contain at least 8 symbols. It must have letters and digits";
                ErrorAlertTextBlock.Visibility = Visibility.Visible;
                return;
            }*/

            var response = await Database.RegisterAsync(username, password);

            if (response == null || response.StatusCode == (HttpStatusCode)0)
            {
                ErrorAlertTextBlock.Text = "Can`t establish connection";
                ErrorAlertTextBlock.Visibility = Visibility.Visible;
            }
            else if (response.StatusCode == (HttpStatusCode)500)
            {
                ErrorAlertTextBlock.Text = "Oops... Something went wrong";
                ErrorAlertTextBlock.Visibility = Visibility.Visible;
            }
            else if (response.StatusCode == (HttpStatusCode)201)
            {
                this.Hide();
                var window = new LoginWindow();
                window.Login(username, password);
                this.Close();
            }
            else if (response.StatusCode == (HttpStatusCode)403)
            {
                ErrorAlertTextBlock.Text = "Password must contain at least 8 symbols. It must have letters and digits";
                ErrorAlertTextBlock.Visibility = Visibility.Visible;
            }
            else if (response.StatusCode == (HttpStatusCode)406)
            {
                ErrorAlertTextBlock.Text = "Username must contain at least 5 symbols and less than 20 symbols. Username must have only latin letters or/and digits. Allowed special symbols: . - _";
                ErrorAlertTextBlock.Visibility = Visibility.Visible;
            }
            else if (response.StatusCode == (HttpStatusCode)405)
            {
                ErrorAlertTextBlock.Text = "This username is not free";
                ErrorAlertTextBlock.Visibility = Visibility.Visible;
            }
           
        }
    }
}
