﻿using EchoMessenger.Helpers;
using Microsoft.Win32;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace EchoMessenger.Views.Settings
{
    /// <summary>
    /// Interaction logic for MyAccountView.xaml
    /// </summary>
    public partial class MyAccountView : UserControl
    {
        private Window owner;

        public MyAccountView(Window owner)
        {
            InitializeComponent();
            this.owner = owner;

            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.UriSource = new Uri(Database.User.Object.AvatarUrl, UriKind.Absolute);
            bitmap.EndInit();

            Avatar.Background = new ImageBrush() { ImageSource = bitmap, Stretch = Stretch.UniformToFill };
        }

        public void Open()
        {
            UsernameBox.Text = Database.User.Object.Name;
        }

        private void LogOutButton_Click(object sender, RoutedEventArgs e)
        {
            LogInManager.ForgetCurrentUser();
            new LoginWindow().Show();
            owner.Close();
        }

        private async void ButtonSaveUsername_Click(object sender, RoutedEventArgs e)
        {
            if (Database.User.Object.Name.ToLower() == UsernameBox.Text.ToLower())
            {
                UsernameBox.Text = Database.User.Object.Name.ToLower();
                return;
            }

            if (!await Database.ChangeUsername(UsernameBox.Text))
            {
                MessageBox.Show("Something went wrong... Try again later");
                return;
            }

            LogInManager.ForgetCurrentUser();
            LogInManager.Remember(Database.User.Object);

            MessageBox.Show($"Username is changed to {Database.User.Object.Name}");
        }

        private async void ButtonSavePassword_Click(object sender, RoutedEventArgs e)
        {
            if (!LogInManager.VerifyPassword(Database.User.Object.PasswordHash, OldPasswordBox.Password))
            {
                MessageBox.Show("Please type your old password correctly");
                return;
            }

            if (OldPasswordBox.Password == NewPasswordBox.Password)
            {
                MessageBox.Show("New password must be different");
                return;
            }

            if (!LogInManager.ValidatePassword(NewPasswordBox.Password))
            {
                MessageBox.Show("Password must contain at least 8 symbols. It must have letters and digits");
                return;
            }

            if (!await Database.ChangePassword(NewPasswordBox.Password))
            {
                MessageBox.Show("Something went wrong... Try again later");
                return;
            }

            LogInManager.ForgetCurrentUser();
            LogInManager.Remember(Database.User.Object);

            MessageBox.Show($"Password is changed successfully");

            OldPasswordBox.Password = String.Empty;
            NewPasswordBox.Password = String.Empty;
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
            var open = new OpenFileDialog();
            open.Multiselect = false;
            open.CheckFileExists = true;
            open.Filter = "Image Files(*.jpg; *.jpeg; *.bmp)|*.jpg; *.jpeg; *.bmp";

            if (open.ShowDialog(owner) == true)
            {
                var image = Media.ProccessImage(open.FileName);
                var avatarUrl = await Storage.UploadAvatarAsync(image);

                if (String.IsNullOrWhiteSpace(avatarUrl))
                {
                    MessageBox.Show("Something went wrong...");
                    return;
                }

                await Database.ChangeAvatar(avatarUrl);

                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(Database.User.Object.AvatarUrl, UriKind.Absolute);
                bitmap.EndInit();

                Avatar.Background = new ImageBrush() { ImageSource = bitmap, Stretch = Stretch.UniformToFill };
            }
        }
    }
}
