using EchoMessenger.Core;
using EchoMessenger.Helpers.Api;
using EchoMessenger.Models;
using System;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace EchoMessenger.UI.Controls.Cards.Dialog
{
    public partial class DialogCard : UserControl
    {
        private UserInfo targetUser;

        public DialogCard(UserInfo targetUser)
        {
            InitializeComponent();

            UpdateTargetUser(targetUser);
        }

        public void UpdateTargetUser(UserInfo user)
        {
            targetUser = user;

            Dispatcher.Invoke(() =>
            {
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(Host.Combine(targetUser.avatarUrl), UriKind.Absolute);
                bitmap.EndInit();

                Avatar.Background = new ImageBrush() { ImageSource = bitmap, Stretch = Stretch.UniformToFill };

                Nickname.Text = targetUser.username;
            }, null);
        }
    }
}
