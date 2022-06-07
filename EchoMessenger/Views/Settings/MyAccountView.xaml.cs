using EchoMessenger.Entities;
using EchoMessenger.Helpers;
using EchoMessenger.Helpers.Server;
using Microsoft.Win32;
using Newtonsoft.Json.Linq;
using System;
using System.Net;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace EchoMessenger.Views.Settings
{
    public partial class MyAccountView : UserControl
    {
        private MessengerWindow owner;
        private UserInfo currentUser;

        public MyAccountView(MessengerWindow owner, UserInfo user)
        {
            InitializeComponent();
            this.owner = owner;

            currentUser = user;

            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.UriSource = new Uri(Database.HostUrl(currentUser.avatarUrl), UriKind.Absolute);
            bitmap.EndInit();

            Avatar.Background = new ImageBrush() { ImageSource = bitmap, Stretch = Stretch.UniformToFill };
        }

        public void Open()
        {
            UsernameBox.Text = currentUser.username;
        }

        public void UpdateUser(UserInfo user)
        {
            currentUser = user;
        }

        private void LogOutButton_Click(object sender, RoutedEventArgs e)
        {
            RegistryManager.ForgetJwt();
            new LoginWindow().Show();
            owner?.Close();
        }

        private async void ButtonSaveUsername_Click(object sender, RoutedEventArgs e)
        {
            String username = UsernameBox.Text;

            try
            {
                _ = Task.Run(() => owner?.SettingsView.StartFillingProgressBar());

                if (String.IsNullOrWhiteSpace(username))
                {
                    MessageBox.Show("Username must not be empty");
                    return;
                }

                if (!LogInManager.ValidateUsername(username))
                {
                    MessageBox.Show(owner, "Username must contain at least 5 symbols and less than 20 symbols. Username must have only latin letters or/and digits. Allowed special symbols: . - _", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var response = await Profile.UpdateUsername(username);

                if (response == null || response.StatusCode == (HttpStatusCode)0)
                {
                    MessageBox.Show(owner, "Can`t establish connection", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                else if (response.StatusCode == (HttpStatusCode)500)
                {
                    MessageBox.Show(owner, "Oops... Something went wrong", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                else if (response.StatusCode == (HttpStatusCode)200)
                {
                    MessageBox.Show(owner, $"Username is changed successfully", "Success", MessageBoxButton.OK, MessageBoxImage.Information);

                    UsernameBox.Text = username;

                    currentUser.username = username;
                    owner.UpdateUser(currentUser);
                }
                else if (response.StatusCode == (HttpStatusCode)401)
                {
                    RegistryManager.ForgetJwt();
                    owner.Hide();
                    new LoginWindow().Show();
                    owner.Close();
                    return;
                }
            }
            finally
            {
                _ = Task.Run(() => owner?.SettingsView.EndFillingProgressBar());
            }
        }

        private async void ButtonSavePassword_Click(object sender, RoutedEventArgs e)
        {
            String oldPassword = OldPasswordBox.Password;
            String newPassword = NewPasswordBox.Password;

            try
            {
                _ = Task.Run(() => owner?.SettingsView.StartFillingProgressBar());

                if (String.IsNullOrWhiteSpace(oldPassword) || String.IsNullOrWhiteSpace(newPassword))
                {
                    MessageBox.Show(owner, "Fields must not be empty", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (oldPassword == newPassword)
                {
                    MessageBox.Show(owner, "New password must be different", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (!LogInManager.ValidatePassword(newPassword))
                {
                    MessageBox.Show(owner, "Password must contain at least 8 symbols. It must have letters and digits", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var response = await Profile.UpdatePassword(oldPassword, newPassword);

                if (response == null || response.StatusCode == (HttpStatusCode)0)
                {
                    MessageBox.Show(owner, "Can`t establish connection", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                else if (response.StatusCode == (HttpStatusCode)500)
                {
                    MessageBox.Show(owner, "Oops... Something went wrong", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                else if (response.StatusCode == (HttpStatusCode)406)
                {
                    MessageBox.Show(owner, "Old password is incorrect", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                else if (response.StatusCode == (HttpStatusCode)200)
                {
                    MessageBox.Show(owner, $"Password is changed successfully", "Success", MessageBoxButton.OK, MessageBoxImage.Information);

                    OldPasswordBox.Password = String.Empty;
                    NewPasswordBox.Password = String.Empty;
                }
                else if (response.StatusCode == (HttpStatusCode)401)
                {
                    RegistryManager.ForgetJwt();
                    owner.Hide();
                    new LoginWindow().Show();
                    owner.Close();
                    return;
                }
            }
            finally
            {
                _ = Task.Run(() => owner?.SettingsView.EndFillingProgressBar());
            }
        }

        private void Avatar_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            AvatarOverlay.Visibility = Visibility.Visible;
        }

        private void Avatar_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            AvatarOverlay.Visibility = Visibility.Hidden;
        }

        private async void AvatarOverlay_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            String avatarUrl = String.Empty;
            String originalAvatarUrl = String.Empty;

            var open = new OpenFileDialog();
            open.Multiselect = false;
            open.CheckFileExists = true;
            open.Filter = "Image Files|*.jpg;*.jpeg;*.png;";

            if (open.ShowDialog(owner) == true)
            {
                try
                {
                    _ = Task.Run(() => owner?.SettingsView.StartFillingProgressBar());

                    var response = await Storage.UploadAvatar(open.FileName);

                    if (response == null || response.StatusCode == (HttpStatusCode)0)
                    {
                        MessageBox.Show(owner, "Can`t establish connection", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                    else if (response.StatusCode == (HttpStatusCode)500)
                    {
                        MessageBox.Show(owner, "Oops... Something went wrong", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                    else if (response.StatusCode == (HttpStatusCode)200)
                    {
                        if (response.Content == null)
                            return;

                        var result = JObject.Parse(response.Content);

                        avatarUrl = result["avatarUrl"].ToString();
                        originalAvatarUrl = result["originalAvatarUrl"].ToString();
                    }
                    else if (response.StatusCode == (HttpStatusCode)401)
                    {
                        RegistryManager.ForgetJwt();
                        owner.Hide();
                        new LoginWindow().Show();
                        owner.Close();
                        return;
                    }

                    if (String.IsNullOrWhiteSpace(avatarUrl) || String.IsNullOrWhiteSpace(originalAvatarUrl))
                    {
                        owner?.SettingsView.EndFillingProgressBar();
                        MessageBox.Show("Something went wrong...");
                        return;
                    }

                    var changeResponse = await Profile.UpdateAvatar(avatarUrl, originalAvatarUrl);

                    if (changeResponse == null || changeResponse.StatusCode == (HttpStatusCode)0)
                    {
                        MessageBox.Show(owner, "Can`t establish connection", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                    else if (changeResponse.StatusCode == (HttpStatusCode)500)
                    {
                        MessageBox.Show(owner, "Oops... Something went wrong", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                    else if (changeResponse.StatusCode == (HttpStatusCode)200)
                    {
                        if (changeResponse.Content == null)
                            return;

                        var bitmap = new BitmapImage();
                        bitmap.BeginInit();
                        bitmap.UriSource = new Uri(Database.HostUrl(avatarUrl), UriKind.Absolute);
                        bitmap.EndInit();

                        Avatar.Background = new ImageBrush() { ImageSource = bitmap, Stretch = Stretch.UniformToFill };
                    }
                    else if (changeResponse.StatusCode == (HttpStatusCode)401)
                    {
                        RegistryManager.ForgetJwt();
                        owner.Hide();
                        new LoginWindow().Show();
                        owner.Close();
                        return;
                    }
                }
                finally
                {
                    _ = Task.Run(() => owner?.SettingsView.EndFillingProgressBar());
                }
            }
        }
    }
}
