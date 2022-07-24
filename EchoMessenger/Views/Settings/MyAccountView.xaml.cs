using EchoMessenger.Models;
using EchoMessenger.Helpers;
using EchoMessenger.Helpers.Api;
using EchoMessenger.Helpers.Server;
using Microsoft.Win32;
using Newtonsoft.Json.Linq;
using System;
using System.Net;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace EchoMessenger.Views.Settings
{
    public partial class MyAccountView : UserControl
    {
        private SynchronizationContext uiSync;
        private MessengerWindow owner;
        private UserInfo currentUser;

        public MyAccountView(MessengerWindow owner, UserInfo user)
        {
            InitializeComponent();
            uiSync = SynchronizationContext.Current;

            this.owner = owner;

            UpdateUser(user);
        }

        public void Open()
        {
            uiSync.Send(s =>
            {
                UsernameSuccessAlertTextBlock.Visibility = Visibility.Collapsed;
                UsernameErrorAlertTextBlock.Visibility = Visibility.Collapsed;
                PasswordSuccessAlertTextBlock.Visibility = Visibility.Collapsed;
                PasswordErrorAlertTextBlock.Visibility = Visibility.Collapsed;
            },null);

            UpdateUser(currentUser);
        }

        public void UpdateUser(UserInfo user)
        {
            currentUser = user;

            uiSync.Send(s =>
            {
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(Host.Combine(currentUser.avatarUrl), UriKind.Absolute);
                bitmap.EndInit();

                Avatar.Background = new ImageBrush() { ImageSource = bitmap, Stretch = Stretch.UniformToFill };

                UsernameBox.Text = currentUser.username;
            }, null);
        }

        private void LogOutButton_Click(object sender, RoutedEventArgs e)
        {
            RegistryManager.ForgetJwt();
            new LoginWindow().Show();
            owner?.Close();
        }

        private async void ButtonSaveUsername_Click(object sender, RoutedEventArgs e)
        {
            UsernameSuccessAlertTextBlock.Visibility = Visibility.Collapsed;
            UsernameErrorAlertTextBlock.Visibility = Visibility.Collapsed;

            String username = UsernameBox.Text;

            try
            {
                //_ = Task.Run(() => owner?.SettingsView.StartFillingProgressBar());

                if (username == currentUser.username)
                {
                    UsernameBox.Text = currentUser.username;
                    UsernameErrorAlertTextBlock.Text = "Username must be different";
                    UsernameErrorAlertTextBlock.Visibility= Visibility.Visible;
                    return;
                }

                if (String.IsNullOrWhiteSpace(username))
                {
                    UsernameBox.Text = currentUser.username;
                    UsernameErrorAlertTextBlock.Text = "Username must not be empty";
                    UsernameErrorAlertTextBlock.Visibility = Visibility.Visible;
                    return;
                }

                if (!LogInManager.ValidateUsername(username))
                {
                    UsernameErrorAlertTextBlock.Text = "Username must contain at least 5 symbols and less than 20 symbols. Username must have only latin letters or/and digits. Allowed special symbols: . - _";
                    UsernameErrorAlertTextBlock.Visibility = Visibility.Visible;
                    return;
                }

                var response = await Profile.UpdateUsername(username);

                if (response == null || response.StatusCode == (HttpStatusCode)0)
                {
                    UsernameErrorAlertTextBlock.Text = "Can`t establish connection";
                    UsernameErrorAlertTextBlock.Visibility = Visibility.Visible;
                    return;
                }
                else if (response.StatusCode == (HttpStatusCode)500)
                {
                    UsernameErrorAlertTextBlock.Text = "Oops... Something went wrong";
                    UsernameErrorAlertTextBlock.Visibility = Visibility.Visible;
                    return;
                }
                else if (response.StatusCode == (HttpStatusCode)405)
                {
                    UsernameErrorAlertTextBlock.Text = "This username is already busy";
                    UsernameErrorAlertTextBlock.Visibility = Visibility.Visible;
                    return;
                }
                else if (response.StatusCode == (HttpStatusCode)200)
                {
                    UsernameSuccessAlertTextBlock.Text = "Username is changed successfully";
                    UsernameSuccessAlertTextBlock.Visibility = Visibility.Visible;

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
                //_ = Task.Run(() => owner?.SettingsView.EndFillingProgressBar());
            }
        }

        private async void ButtonSavePassword_Click(object sender, RoutedEventArgs e)
        {
            PasswordSuccessAlertTextBlock.Visibility = Visibility.Collapsed;
            PasswordErrorAlertTextBlock.Visibility = Visibility.Collapsed;

            String oldPassword = OldPasswordBox.Password;
            String newPassword = NewPasswordBox.Password;

            try
            {
                //_ = Task.Run(() => owner?.SettingsView.StartFillingProgressBar());

                if (String.IsNullOrWhiteSpace(oldPassword) || String.IsNullOrWhiteSpace(newPassword))
                {
                    PasswordErrorAlertTextBlock.Text = "Fields must not be empty";
                    PasswordErrorAlertTextBlock.Visibility = Visibility.Visible;
                    return;
                }

                if (oldPassword == newPassword)
                {
                    PasswordErrorAlertTextBlock.Text = "New password must be different";
                    PasswordErrorAlertTextBlock.Visibility = Visibility.Visible;
                    return;
                }

                if (!LogInManager.ValidatePassword(newPassword))
                {
                    PasswordErrorAlertTextBlock.Text = "Password must contain at least 8 symbols. It must have letters and digits";
                    PasswordErrorAlertTextBlock.Visibility = Visibility.Visible;
                    return;
                }

                var response = await Profile.UpdatePassword(oldPassword, newPassword);

                if (response == null || response.StatusCode == (HttpStatusCode)0)
                {
                    PasswordErrorAlertTextBlock.Text = "Can`t establish connection";
                    PasswordErrorAlertTextBlock.Visibility = Visibility.Visible;
                    return;
                }
                else if (response.StatusCode == (HttpStatusCode)500)
                {
                    PasswordErrorAlertTextBlock.Text = "Oops... Something went wrong";
                    PasswordErrorAlertTextBlock.Visibility = Visibility.Visible;
                    return;
                }
                else if (response.StatusCode == (HttpStatusCode)406)
                {
                    PasswordErrorAlertTextBlock.Text = "Old password is incorrect";
                    PasswordErrorAlertTextBlock.Visibility = Visibility.Visible;
                    return;
                }
                else if (response.StatusCode == (HttpStatusCode)200)
                {
                    PasswordSuccessAlertTextBlock.Text = "Password is changed successfully";
                    PasswordSuccessAlertTextBlock.Visibility = Visibility.Visible;

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
                //_ = Task.Run(() => owner?.SettingsView.EndFillingProgressBar());
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
            UsernameSuccessAlertTextBlock.Visibility = Visibility.Collapsed;
            UsernameErrorAlertTextBlock.Visibility = Visibility.Collapsed;

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
                    //_ = Task.Run(() => owner?.SettingsView.StartFillingProgressBar());

                    var response = await Storage.UploadAvatar(open.FileName);

                    if (response == null || response.StatusCode == (HttpStatusCode)0)
                    {
                        UsernameErrorAlertTextBlock.Text = "Can`t establish connection";
                        UsernameErrorAlertTextBlock.Visibility = Visibility.Visible;
                        return;
                    }
                    else if (response.StatusCode == (HttpStatusCode)500)
                    {
                        UsernameErrorAlertTextBlock.Text = "Oops... Something went wrong";
                        UsernameErrorAlertTextBlock.Visibility = Visibility.Visible;
                        return;
                    }
                    else if (response.StatusCode == (HttpStatusCode)200)
                    {
                        if (response.Content == null)
                        {
                            UsernameErrorAlertTextBlock.Text = "Oops... Something went wrong";
                            UsernameErrorAlertTextBlock.Visibility = Visibility.Visible;
                            return;
                        }

                        var result = JObject.Parse(response.Content);

                        if (result == null)
                        {
                            UsernameErrorAlertTextBlock.Text = "Oops... Something went wrong";
                            UsernameErrorAlertTextBlock.Visibility = Visibility.Visible;
                            return;
                        }

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
                        //owner?.SettingsView.EndFillingProgressBar();
                        UsernameErrorAlertTextBlock.Text = "Something went wrong...";
                        UsernameErrorAlertTextBlock.Visibility = Visibility.Visible;
                        return;
                    }

                    var changeResponse = await Profile.UpdateAvatar(avatarUrl, originalAvatarUrl);

                    if (changeResponse == null || changeResponse.StatusCode == (HttpStatusCode)0)
                    {
                        UsernameErrorAlertTextBlock.Text = "Can`t establish connection";
                        UsernameErrorAlertTextBlock.Visibility = Visibility.Visible;
                        return;
                    }
                    else if (changeResponse.StatusCode == (HttpStatusCode)500)
                    {
                        UsernameErrorAlertTextBlock.Text = "Oops... Something went wrong";
                        UsernameErrorAlertTextBlock.Visibility = Visibility.Visible;
                        return;
                    }
                    else if (changeResponse.StatusCode == (HttpStatusCode)200)
                    {
                        if (changeResponse.Content == null)
                        {
                            UsernameErrorAlertTextBlock.Text = "Oops... Something went wrong";
                            UsernameErrorAlertTextBlock.Visibility = Visibility.Visible;
                            return;
                        }

                        UsernameSuccessAlertTextBlock.Text = "Avatar changed successfully!";
                        UsernameSuccessAlertTextBlock.Visibility = Visibility.Visible;

                        currentUser.avatarUrl = avatarUrl;
                        currentUser.originalAvatarUrl = originalAvatarUrl;
                        owner.UpdateUser(currentUser);

                        var bitmap = new BitmapImage();
                        bitmap.BeginInit();
                        bitmap.UriSource = new Uri(Host.Combine(avatarUrl), UriKind.Absolute);
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
                    //_ = Task.Run(() => owner?.SettingsView.EndFillingProgressBar());
                }
            }
        }
    }
}
