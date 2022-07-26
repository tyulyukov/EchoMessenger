using EchoMessenger.Models;
using EchoMessenger.Helpers;
using EchoMessenger.Helpers.Server;
using Newtonsoft.Json.Linq;
using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using EchoMessenger.Core;

namespace EchoMessenger
{
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
        }

        public async void Login(String username, String password)
        {
            ErrorAlertTextBlock.Visibility = Visibility.Collapsed;

            if (String.IsNullOrWhiteSpace(username) || String.IsNullOrWhiteSpace(password))
            {
                ErrorAlertTextBlock.Text = "Fields must be not empty";
                ErrorAlertTextBlock.Visibility = Visibility.Visible;
                return;
            }

            var response = await Database.LoginAsync(username, password);

            if (response == null || response.StatusCode == (HttpStatusCode)0)
            {
                ErrorAlertTextBlock.Text = "Can`t establish connection";
                ErrorAlertTextBlock.Visibility = Visibility.Visible;
            }
            else if (response.StatusCode == (HttpStatusCode)500)
            {
                ErrorAlertTextBlock.Text = "Oops... Something went wrong";
            }
            else if (response.StatusCode == (HttpStatusCode)200)
            {
                if (response.Content == null)
                {
                    ErrorAlertTextBlock.Text = "Oops... Something went wrong";
                    ErrorAlertTextBlock.Visibility = Visibility.Visible;
                    return;
                }

                var result = JObject.Parse(response.Content);

                if (result == null)
                {
                    ErrorAlertTextBlock.Text = "Oops... Something went wrong";
                    ErrorAlertTextBlock.Visibility = Visibility.Visible;
                    return;
                }

                var jwt = result["token"];
                var user = result["user"];

                if (jwt == null || user == null)
                {
                    ErrorAlertTextBlock.Text = "Oops... Something went wrong";
                    ErrorAlertTextBlock.Visibility = Visibility.Visible;
                    return;
                }

                RegistryManager.RememberJwt(jwt.ToString());

                this.Hide();
                new MessengerWindow(user.ToObject<UserInfo>()).Show();
                this.Close();
            }
            else if (response.StatusCode == (HttpStatusCode)400)
            {
                ErrorAlertTextBlock.Text = "Incorrect username or password";
                ErrorAlertTextBlock.Visibility = Visibility.Visible;
            }
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            var username = UsernameBox.Text;
            var password = PasswordBox.Password;

            Login(username, password);
        }

        private void RegisterButton_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            this.Hide();
            new RegistrationWindow().Show();
            this.Close();
        }
    }
}
