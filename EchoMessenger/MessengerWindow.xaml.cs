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
using System.Windows.Shapes;

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

        private Border? activeButton;
        private Line selectionLine;
        private TimeSpan selectionDuration = TimeSpan.FromMilliseconds(250);

        public MessengerWindow(UserInfo user)
        {
            InitializeComponent();
            uiSync = SynchronizationContext.Current;

            currentUser = user;
            
            isLoading = false;

            MessagesView = new MessagesView(this, currentUser);
            SettingsView = new SettingsView(this, currentUser);
            SearchView = new SearchView(this);

            activeButton = null;
            selectionLine = UIElementsFactory.CreateSelectionLine();
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
            Messages.Configure();
            await Messages.Connect();
        }

        private async void Window_Unloaded(object sender, RoutedEventArgs e)
        {
            await Messages.Disconnect();
        }

        private async Task LoadChats()
        {
            ShowLoading(true);

            try
            {
                MessagesView.ClearLoadedChats();
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
                        var icon = AddUserIcon(targetUser, chat);
                        MessagesView.LoadChat(targetUser._id, chat, icon);
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

        public void SelectButton(Border button)
        {
            if (activeButton == button)
                return;

            uiSync.Post((s) => {
                selectionLine.Opacity = 0;

                if (activeButton != null)
                    activeButton.Child = null;

                activeButton = button;
                activeButton.Child = selectionLine;

                selectionLine.ChangeVisibility(true, selectionDuration);
            }, null);
        }

        public Border AddUserIcon(UserInfo targetUser, Chat chat, bool select = false)
        {
            Border? icon = null;

            uiSync.Send((s) => {
                icon = UIElementsFactory.CreateUserIcon(targetUser.avatarUrl);

                if (select)
                    SelectButton(icon);

                icon.MouseLeftButtonUp += (sender, e) =>
                {
                    _ = Task.Run(() =>
                    {
                        OpenTab(MessagesView);
                        MessagesView.OpenChat(chat);
                    });
                };

                ChatsMenu.Children.Insert(0, icon);
                icon.SetSlideFromLeftOnLoad();
            }, null);

            return icon;
        }

        public void PushUserIcon(Border icon)
        {
            uiSync.Post((s) => {
                ChatsMenu.Children.Remove(icon);
                ChatsMenu.Children.Insert(0, icon);
                icon.ChangeVisibility(true, selectionDuration);
            }, null);
        } 

        public void SetUserIconBadge(int count)
        {
            String countRepresentation = count.ToString();

            if (count >= 100)
                countRepresentation = "99+";

            // Add a badge to chat icon
        }

        private void ButtonSettings_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            SelectButton((Border)sender);

            SettingsView.Open();
            OpenTab(SettingsView);
        }

        private void ButtonSearch_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            SelectButton((Border)sender);

            OpenTab(SearchView);
        }
    }
}
