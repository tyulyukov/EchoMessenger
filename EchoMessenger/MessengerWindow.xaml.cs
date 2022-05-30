using EchoMessenger.Entities;
using EchoMessenger.Helpers;
using EchoMessenger.Views;
using EchoMessenger.Views.Settings;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace EchoMessenger
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MessengerWindow : Window
    {
        public readonly MessagesView MessagesView;
        public readonly SettingsView SettingsView;
        public readonly SearchView SearchView;
        private readonly UserInfo currentUser;

        public MessengerWindow(UserInfo user)
        {
            InitializeComponent();
            
            currentUser = user;

            MessagesView = new MessagesView(this);
            SettingsView = new SettingsView(this, currentUser);
            SearchView = new SearchView(this);

            /*ChatsMenu.Children.Clear();
            var chats = Database.GetLastChats();

            if (chats == null)
                return;

            foreach (var chat in chats)
            {
                var targetUser = chat.Object.FromUser.Name == Database.User.Object.Name ? chat.Object.TargetUser : chat.Object.FromUser;

                var icon = UIElementsFactory.CreateUserIcon(targetUser.AvatarUrl);

                icon.MouseLeftButtonUp += async (object sender, MouseButtonEventArgs e) =>
                {
                    var chat = await Database.GetChat(targetUser);

                    if (chat == null)
                        return;

                    MessagesView.OpenChat(chat);
                    OpenTab(MessagesView);
                };

                ChatsMenu.Children.Add(icon);
            }*/
        }

        public void OpenTab(UserControl tab)
        {
            OpenedTab.Children.Clear();
            OpenedTab.Children.Add(tab);
        }

        private void ButtonSettings_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            SettingsView.Open();
            OpenTab(SettingsView);
        }

        private void ButtonSearch_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            OpenTab(SearchView);
        }
    }
}
