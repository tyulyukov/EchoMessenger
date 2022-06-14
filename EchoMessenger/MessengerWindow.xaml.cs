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
using System.Windows.Media;

namespace EchoMessenger
{
    public partial class MessengerWindow : Window
    {
        public readonly Dictionary<String, MessagesView> MessagesViews;
        public readonly SettingsView SettingsView;
        public readonly SearchView SearchView;

        private UserControl? openedTab;
        
        private readonly Dictionary<String, KeyValuePair<Chat, UserIcon>> OpenedChats;
        private readonly List<String> OnlineChats;
        private UserIcon? firstIcon;

        private UserInfo currentUser;
        private SynchronizationContext uiSync;
        private bool isLoading;

        private Border? selectedButton;
        private SelectionLine selectionLine;
        private TimeSpan selectionDuration = TimeSpan.FromMilliseconds(250);

        public MessengerWindow(UserInfo user)
        {
            InitializeComponent();
            uiSync = SynchronizationContext.Current;

            currentUser = user;
            
            isLoading = false;

            MessagesViews = new Dictionary<String, MessagesView>();
            SettingsView = new SettingsView(this, currentUser);
            SearchView = new SearchView(this);

            selectedButton = null;
            selectionLine = new SelectionLine();

            OpenedChats = new Dictionary<string, KeyValuePair<Chat, UserIcon>>();
            OnlineChats = new List<String>();
        }

        public void UpdateUser(UserInfo user)
        {
            if (user == null)
                return;

            user.createdAt = user.createdAt.ToLocalTime();

            currentUser = user;
            
            SettingsView.UpdateUser(user);

            if (openedTab is MessagesView messageView)
            {
                messageView.UpdateUser(user);
            }

            uiSync.Send(async s =>
            {
                await LoadChats();
            }, null);
        }

        public void ShowLoading(bool visible)
        {
            LoadingSpinner.Visibility = visible ? Visibility.Visible : Visibility.Collapsed;
            isLoading = visible;
        }

        public bool OpenTab(UserControl tab)
        {
            if (openedTab == tab)
                return false;

            uiSync.Post((s) => {
                OpenedTab.Children.Clear();
                OpenedTab.Children.Add(tab);

                openedTab = tab;
            }, null);

            return true;
        }

        public void SelectButton(Border button)
        {
            if (selectedButton == button)
                return;

            uiSync.Post((s) => {
                selectionLine.Opacity = 0;

                Grid grid;

                if (selectedButton != null)
                {
                    grid = (Grid)selectedButton.Child;

                    if (grid == null)
                        return;

                    grid.Children.Remove(selectionLine);
                }

                selectedButton = button;

                if (selectedButton is UserIcon icon)
                    SetNotificationsToIcon(icon, 0);

                grid = (Grid)selectedButton.Child;

                if (grid == null)
                    return;

                grid.Children.Add(selectionLine);

                selectionLine.ChangeVisibility(true, selectionDuration);
            }, null);
        }

        public async Task OpenChat(String userId)
        {
            Chat? chat = null;

            if (OpenedChats.ContainsKey(userId))
            {
                chat = OpenedChats[userId].Key;
            }
            else
            {
                try
                {
                    uiSync.Post((s) => { ShowLoading(true); }, null);  // MOVE TO SEARCH
                    var chatResponse = await Database.CreateChat(userId);

                    if (chatResponse == null || chatResponse.StatusCode == (HttpStatusCode)0)
                    {
                        MessageBox.Show(this, "Can`t establish connection", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                    else if (chatResponse.StatusCode == (HttpStatusCode)500)
                    {
                        MessageBox.Show(this, "Oops... Something went wrong", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                    else if (chatResponse.StatusCode == (HttpStatusCode)201)
                    {
                        if (chatResponse.Content == null)
                            return;

                        var result = JObject.Parse(chatResponse.Content);

                        chat = result?.ToObject<Chat>();

                        if (chat == null)
                            return;

                        chat.createdAt = chat.createdAt.ToLocalTime();

                        var targetUser = chat.sender._id == currentUser._id ? chat.receiver : chat.sender;
                        var icon = AddUserIcon(targetUser, chat, OnlineChats.Contains(targetUser._id), true);

                        LoadChat(userId, chat, icon, OnlineChats.Contains(targetUser._id));
                    }
                    else if (chatResponse.StatusCode == (HttpStatusCode)401)
                    {
                        RegistryManager.ForgetJwt();
                        Hide();
                        new LoginWindow().Show();
                        Close();
                        return;
                    }
                }
                finally
                {
                    uiSync.Post((s) => { ShowLoading(false); }, null);  // MOVE TO SEARCH
                }
            }

            if (chat == null)
                return;

            if (OpenTab(MessagesViews[chat._id]))
            {
                MessagesViews[chat._id].Open();
                var chatByUserId = OpenedChats.FirstOrDefault(o => o.Value.Key == chat);
                var icon = chatByUserId.Value.Value;
                SelectButton(icon);
            }
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Messages.OnUsersOnlineReceived += OnlineChatsReceived;
            Messages.OnUserConnected += OnUserOnline;
            Messages.OnUserDisconnected += OnUserOffline;
            Messages.OnError += SocketError;
            Messages.OnChatCreated += OnChatCreated;
            Messages.OnMessageSent += MessageSent;
            Messages.OnMessageSendFailed += MessageSentFailed;
            Messages.OnUserUpdated += UserUpdated;
            Messages.OnUserTyping += UserTyping;
            
            Messages.Configure();
            await Messages.Connect();
            
            await LoadChats();
        }

        private async void Window_Unloaded(object sender, RoutedEventArgs e)
        {
            Messages.OnUsersOnlineReceived -= OnlineChatsReceived;
            Messages.OnUserConnected -= OnUserOnline;
            Messages.OnUserDisconnected -= OnUserOffline;
            Messages.OnError -= SocketError;
            Messages.OnChatCreated -= OnChatCreated;
            Messages.OnMessageSent -= MessageSent;
            Messages.OnMessageSendFailed -= MessageSentFailed;
            Messages.OnUserUpdated -= UserUpdated;
            Messages.OnUserTyping -= UserTyping;

            await Messages.Disconnect();
        }

        private void OnlineChatsReceived(SocketIOClient.SocketIOResponse response)
        {
            var users = response.GetValue<IEnumerable<String>>();

            foreach (var userId in users)
                SetOnlineStatus(userId, true);
        }

        private void OnUserOnline(SocketIOClient.SocketIOResponse response)
        {
            var userId = response.GetValue<String>();
            SetOnlineStatus(userId, true);
        }

        private void OnUserOffline(SocketIOClient.SocketIOResponse response)
        {
            var userId = response.GetValue<String>();
            SetOnlineStatus(userId, false);
        }

        private void SocketError(object? sender, String arg)
        {
            if (arg == "401")
            {
                uiSync.Post((s) =>
                {
                    RegistryManager.ForgetJwt();
                    Hide();
                    new LoginWindow().Show();
                    Close();
                }, null);
            }
            else if (arg == "500")
            {
                MessageBox.Show(Owner, "Oops... Something went wrong", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else
            {
                MessageBox.Show(Owner, arg, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OnChatCreated(SocketIOClient.SocketIOResponse response)
        {
            var chat = response.GetValue<Chat>();

            chat.createdAt = chat.createdAt.ToLocalTime();

            var targetUser = chat.sender._id == currentUser._id ? chat.receiver : chat.sender;
            var icon = AddUserIcon(targetUser, chat, OnlineChats.Contains(targetUser._id));
            LoadChat(targetUser._id, chat, icon, OnlineChats.Contains(targetUser._id));
        }

        private void MessageSent(SocketIOClient.SocketIOResponse response)
        {
            var message = response.GetValue<Message>();
            message.sentAt = message.sentAt.ToLocalTime();
            foreach (var edit in message.edits)
            {
                edit.date = edit.date.ToLocalTime();
            }

            if (MessagesViews.TryGetValue(message.chat._id, out var messageView))
            {
                var targetUser = message.chat.sender._id == currentUser._id ? message.chat.receiver : message.chat.sender;
                var icon = OpenedChats[targetUser._id].Value;

                PushUserIcon(icon);

                if (openedTab != messageView && message.sender.username != currentUser.username)
                    AddNotificationsToIcon(icon);

                messageView.AddMessage(message);
            }
        }

        private void MessageSentFailed(SocketIOClient.SocketIOResponse response)
        {
            var messageId = response.GetValue<String>();

            foreach (var view in MessagesViews.Values)
            {
                view.FailLoadingMessage(messageId);
            }
        }

        private void UserUpdated(SocketIOClient.SocketIOResponse response)
        {
            var user = response.GetValue<UserInfo>();
            user.createdAt = user.createdAt.ToLocalTime();

            if (user._id == currentUser._id)
            {
                UpdateUser(user);
            }
            else if (OpenedChats.TryGetValue(user._id, out var chat))
            {
                uiSync.Send((s) =>
                {
                    chat.Value.UpdateAvatar(user.avatarUrl);
                }, null);

                MessagesViews[chat.Key._id].UpdateTargetUser(user);
            }
        }

        private void UserTyping(SocketIOClient.SocketIOResponse response)
        {
            var userId = response.GetValue<String>();

            if (OpenedChats.TryGetValue(userId, out var chat))
                MessagesViews[chat.Key._id].UserTyping();
        }

        private async Task LoadChats()
        {
            ButtonRetry.Visibility = Visibility.Collapsed;
            ShowLoading(true);

            try
            {
                MessagesViews.Clear();
                OpenedChats.Clear();
                ChatsMenu.Children.Clear();
                var response = await Database.GetLastChats();

                if (response == null || response.StatusCode == (HttpStatusCode)0 || response.StatusCode == (HttpStatusCode)500)
                {
                    throw new Exception();
                }
                else if (response.StatusCode == (HttpStatusCode)200)
                {
                    if (String.IsNullOrEmpty(response.Content))
                        throw new Exception();

                    var result = JArray.Parse(response.Content);

                    var chats = result?.ToObject<IEnumerable<Chat>>();

                    if (chats == null)
                        throw new Exception();

                    chats = chats.OrderBy(c => c.GetLastSentAt());
                    foreach (var chat in chats)
                    {
                        chat.createdAt = chat.createdAt.ToLocalTime();

                        var targetUser = chat.sender._id == currentUser._id ? chat.receiver : chat.sender;
                        var icon = AddUserIcon(targetUser, chat, OnlineChats.Contains(targetUser._id));
                        LoadChat(targetUser._id, chat, icon, OnlineChats.Contains(targetUser._id));

                        if (chat == chats.Last())
                            firstIcon = icon;
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
            catch
            {
                ButtonRetry.Visibility = Visibility.Visible;
            }
            finally
            {
                ShowLoading(false);
            }
        }

        private UserIcon AddUserIcon(UserInfo targetUser, Chat chat, bool isOnline, bool select = false)
        {
            UserIcon? icon = null;

            uiSync.Send((s) => {
                icon = new UserIcon(targetUser.avatarUrl, isOnline);

                if (select)
                    SelectButton(icon);

                icon.MouseLeftButtonUp += (sender, e) =>
                {
                    _ = Task.Run(() =>
                    {
                        if (OpenTab(MessagesViews[chat._id]))
                        {
                            MessagesViews[chat._id].Open();
                            var chatByUserId = OpenedChats.FirstOrDefault(o => o.Value.Key == chat);
                            var icon = chatByUserId.Value.Value;
                            SelectButton(icon);
                        }
                    });
                };

                icon.SetSlideFromLeftOnLoad();
                ChatsMenu.Children.Insert(0, icon.ConvertToSelectable());
            }, null);

            return icon;
        }

        private void LoadChat(String userId, Chat chat, UserIcon icon, bool isOnline)
        {
            uiSync.Send((s) =>
            {
                if (!MessagesViews.ContainsKey(chat._id))
                    MessagesViews.Add(chat._id, new MessagesView(this, currentUser, chat, isOnline));

                if (!OpenedChats.ContainsKey(userId))
                    OpenedChats.Add(userId, new KeyValuePair<Chat, UserIcon>(chat, icon));
            }, null);
        }

        private void PushUserIcon(UserIcon icon)
        {
            if (firstIcon != null && firstIcon == icon)
                return;

            uiSync.Post((s) => {
                icon.Parent.RemoveChild(icon);
                ChatsMenu.Children.Remove((UIElement)icon.Parent);
                ChatsMenu.Children.Insert(0, icon.ConvertToSelectable());
                firstIcon = icon;
                icon.ChangeVisibility(true, selectionDuration);
            }, null);
        } 

        private void SetNotificationsToIcon(UserIcon icon, int count)
        {
            uiSync.Send((s) =>
            {
                if (count < 0)
                    return;

                if (count == 0)
                {
                    icon.NotificationBadge.Visibility = Visibility.Collapsed;
                    icon.NotificationBadge.NotificationTextBlock.Text = "0";
                    return;
                }

                String countRepresentation = count.ToString();

                if (count >= 100)
                    countRepresentation = "99+";

                icon.NotificationBadge.NotificationTextBlock.Text = countRepresentation;
                icon.NotificationBadge.Visibility = Visibility.Visible;
            }, null);
        }

        private void AddNotificationsToIcon(UserIcon icon)
        {
            uiSync.Send((s) =>
            {
                String countRepresentation;

                int count = Convert.ToInt32(icon.NotificationBadge.NotificationTextBlock.Text);
                count++;

                if (count >= 100)
                    countRepresentation = "99+";
                else
                    countRepresentation = count.ToString();

                icon.NotificationBadge.NotificationTextBlock.Text = countRepresentation;
                icon.NotificationBadge.Visibility = Visibility.Visible;
            }, null);
        }

        private void SetOnlineStatus(String userId, bool isOnline)
        {
            if (!OnlineChats.Contains(userId) && isOnline)
                OnlineChats.Add(userId);
            else if (OnlineChats.Contains(userId) && !isOnline)
                OnlineChats.Remove(userId);

            if (OpenedChats.TryGetValue(userId, out var chat))
            {
                uiSync.Send((s) =>
                {
                    if (isOnline)
                        chat.Value.OnlineStatusIcon.SetOnline();
                    else
                        chat.Value.OnlineStatusIcon.SetOffline();

                    if (MessagesViews.TryGetValue(chat.Key._id, out var view))
                        view.SetOnlineStatus(isOnline);
                }, null);
            }
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

        private async void ButtonRetry_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            await LoadChats();
        }
    }
}
