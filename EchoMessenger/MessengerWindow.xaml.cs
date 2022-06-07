using EchoMessenger.Entities;
using EchoMessenger.Helpers;
using EchoMessenger.Helpers.Server;
using EchoMessenger.Views;
using EchoMessenger.Views.Settings;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
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

        private UserInfo currentUser;
        private SynchronizationContext uiSync;
        private bool isLoading;

        public MessengerWindow(UserInfo user)
        {
            InitializeComponent();
            uiSync = SynchronizationContext.Current;

            currentUser = user;
            
            isLoading = false;

            MessagesView = new MessagesView(this, currentUser);
            SettingsView = new SettingsView(this, currentUser);
            SearchView = new SearchView(this);
        }

        public async void UpdateUser(UserInfo user)
        {
            if (user == null)
                return;

            currentUser = user;
            MessagesView.UpdateUser(user);
            SettingsView.UpdateUser(user);

            await LoadChats();
        }

        public void ShowLoading(bool visible)
        {
            LoadingSpinner.Visibility = visible ? Visibility.Visible : Visibility.Collapsed;
            isLoading = visible;
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadChats();
        }

        private async Task LoadChats()
        {
            ShowLoading(true);

            try
            {
                ChatsMenu.Children.Clear();
                var response = await Database.GetLastChats();

                if (response == null || response.StatusCode == (HttpStatusCode)0)
                {
                    MessageBox.Show(this, "Can`t establish connection", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else if (response.StatusCode == (HttpStatusCode)500)
                {
                    MessageBox.Show(this, "Oops... Something went wrong", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else if (response.StatusCode == (HttpStatusCode)200)
                {
                    if (String.IsNullOrEmpty(response.Content))
                        return;

                    var result = JArray.Parse(response.Content);

                    var chats = result?.ToObject<IEnumerable<Chat>>();

                    if (chats == null)
                        return;

                    foreach (var chat in chats.OrderBy(c => c.GetLastSentAt()))
                    {
                        var targetUser = chat.sender.username == currentUser.username ? chat.receiver : chat.sender;
                        MessagesView.LoadChat(targetUser._id, chat);

                        AddUserIcon(targetUser, chat);
                    }
                }
                else if (response.StatusCode == (HttpStatusCode)401)
                {
                    RegistryManager.ForgetJwt();
                    Hide();
                    new LoginWindow().Show();
                    Close();
                }
            }
            finally
            {
                ShowLoading(false);
            }
        }

        public void OpenTab(UserControl tab)
        {
            uiSync.Post((s) => {
                OpenedTab.Children.Clear();
                OpenedTab.Children.Add(tab);
            }, null);
        }

        public void AddUserIcon(UserInfo targetUser, Chat chat)
        {
            uiSync.Post((s) => {
                var icon = UIElementsFactory.CreateUserIcon(targetUser.avatarUrl);

                icon.MouseLeftButtonUp += (s, e) =>
                {
                    _ = Task.Run(() =>
                    {
                        OpenTab(MessagesView);
                        MessagesView.OpenChat(chat);
                    });
                };

                ChatsMenu.Children.Insert(0, icon);
            }, null);
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
