using EchoMessenger.Entities;
using EchoMessenger.Helpers;
using Newtonsoft.Json.Linq;
using System;
using System.Net;
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

        public async void Login(String username, String password)
        {
            if (String.IsNullOrWhiteSpace(username) || String.IsNullOrWhiteSpace(password))
                return;

            var response = await Database.LoginAsync(username, password);

            if (response == null || response.StatusCode == (HttpStatusCode)0)
            {
                MessageBox.Show(this, "Can`t establish connection", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (response.StatusCode == (HttpStatusCode)500)
            {
                MessageBox.Show(this, "Oops... Something went wrong", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else if (response.StatusCode == (HttpStatusCode)200)
            {
                if (response.Content == null)
                    return;

                var result = JObject.Parse(response.Content);

                if (result == null)
                    return;

                var jwt = result["token"];
                var user = result["user"];

                if (jwt == null || user == null)
                    return;

                RegistryManager.RememberJwt(jwt.ToString());

                this.Hide();
                new MessengerWindow(user.ToObject<UserInfo>()).Show();
                this.Close();
            }
            else if (response.StatusCode == (HttpStatusCode)400)
            {
                MessageBox.Show(this, "Incorrect username or password", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
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
