using EchoMessenger.Models;
using EchoMessenger.Helpers;
using EchoMessenger.Helpers.Server;
using EchoMessenger.Views.Chats;
using EchoMessenger.Views.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using EchoMessenger.UI.Controls.Messages;

namespace EchoMessenger
{
    public partial class MessengerWindow : Window
    {
        private readonly SettingsListView SettingsList;
        private readonly ChatsListView ChatsList;

        private UserControl? openedTab;
        private UserControl? openedList;
        
        private UserInfo currentUser;
        private SynchronizationContext uiSync;

        public MessengerWindow(UserInfo user)
        {
            InitializeComponent();
            uiSync = SynchronizationContext.Current;

            currentUser = user;

            SettingsList = new SettingsListView(this, currentUser);
            ChatsList = new ChatsListView(this, currentUser);
        }

        public void UpdateUser(UserInfo user)
        {
            if (user == null)
                return;

            currentUser = user;

            SettingsList.UpdateUser(user);
            ChatsList.UpdateUser(user);
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

        public bool OpenList(UserControl list)
        {
            if (openedList == list)
                return false;

            uiSync.Post((s) => {
                OpenedList.Children.Clear();
                OpenedList.Children.Add(list);

                openedList = list;
            }, null);

            return true;
        }

        public void EditMessage(Message message)
        {
            var targetUser = message.chat.sender == currentUser ? message.chat.receiver : message.chat.sender;

            if (ChatsList.OpenedChats.TryGetValue(targetUser._id, out var chat))
                chat.Value.EditMessage(message);
        }


        /*public void SelectButton(Border button)
        {
            if (button == null)
                return;

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

                grid = (Grid)selectedButton.Child;

                if (grid == null)
                    return;

                grid.Children.Add(selectionLine);

                selectionLine.ChangeVisibility(true, selectionDuration);
            }, null);
        }*/

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
            Messages.OnMessageRead += MessageHaveSeen;
            Messages.OnMessageDeleted += MessageDeleted;
            Messages.OnMessageEdited += MessageEdited;
            MessageBorder.OnDeleteButtonClick += DeleteMessage;
            MessageBorder.OnReplyButtonClick += EnterReplying;
            MessageBorder.OnEditButtonClick += EnterEditing;
            MessageBorder.OnHistoryButtonClick += ShowMessageHistory;

            Messages.Configure();
            await Messages.Connect();
            
            OpenList(ChatsList);
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
            Messages.OnMessageRead -= MessageHaveSeen;
            Messages.OnMessageDeleted -= MessageDeleted;
            Messages.OnMessageEdited -= MessageEdited;
            MessageBorder.OnDeleteButtonClick -= DeleteMessage;
            MessageBorder.OnReplyButtonClick -= EnterReplying;
            MessageBorder.OnEditButtonClick -= EnterEditing;
            MessageBorder.OnHistoryButtonClick -= ShowMessageHistory;

            await Messages.Disconnect();
        }

        private void OnlineChatsReceived(SocketIOClient.SocketIOResponse response)
        {
            var users = response.GetValue<IEnumerable<String>>();

            foreach (var userId in users)
                ChatsList.SetOnlineStatus(userId, true);
        }

        private void OnUserOnline(SocketIOClient.SocketIOResponse response)
        {
            var userId = response.GetValue<String>();
            ChatsList.SetOnlineStatus(userId, true);
        }

        private void OnUserOffline(SocketIOClient.SocketIOResponse response)
        {
            var userId = response.GetValue<String>();

            ChatsList.SetOnlineStatus(userId, false);
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
        }

        private void OnChatCreated(SocketIOClient.SocketIOResponse response)
        {
            var chat = response.GetValue<Chat>();

            var targetUser = chat.sender == currentUser ? chat.receiver : chat.sender;
            var icon = ChatsList.AddUserCard(targetUser, chat, chat.messages.FirstOrDefault(), ChatsList.OnlineChats.Contains(targetUser._id));
            ChatsList.LoadChat(targetUser._id, chat, icon, ChatsList.OnlineChats.Contains(targetUser._id));
        }

        private void MessageSent(SocketIOClient.SocketIOResponse response)
        {
            var message = response.GetValue<Message>();

            if (ChatsList.MessagesViews.TryGetValue(message.chat._id, out var messageView))
            {
                var targetUser = message.chat.sender == currentUser ? message.chat.receiver : message.chat.sender;
                var icon = ChatsList.OpenedChats[targetUser._id].Value;

                uiSync.Send((s) =>
                {
                    icon.UpdateLastMessage(message);
                    ChatsList.PushUserIcon(icon);

                    if (message.sender != currentUser)
                        icon.NotificationBadge.AddNotification();
                }, null);

                messageView.AddMessage(message);
            }
        }

        private void MessageSentFailed(SocketIOClient.SocketIOResponse response)
        {
            var messageId = response.GetValue<String>();

            foreach (var view in ChatsList.MessagesViews.Values)
            {
                view.FailLoadingMessage(messageId);
            }
        }

        private void UserUpdated(SocketIOClient.SocketIOResponse response)
        {
            var user = response.GetValue<UserInfo>();

            if (user == currentUser)
            {
                UpdateUser(user);
            }
            else if (ChatsList.OpenedChats.TryGetValue(user._id, out var chat))
            {
                uiSync.Send((s) =>
                {
                    chat.Value.UpdateTargetUser(user);
                }, null);

                ChatsList.MessagesViews[chat.Key._id].UpdateTargetUser(user);
            }
        }

        private void UserTyping(SocketIOClient.SocketIOResponse response)
        {
            var userId = response.GetValue<String>();

            if (ChatsList.OpenedChats.TryGetValue(userId, out var chat))
            {
                uiSync.Send((s) =>
                {
                    chat.Value.UserTyping();
                }, null);

                ChatsList.MessagesViews[chat.Key._id].UserTyping();
            }
        }

        private void MessageHaveSeen(SocketIOClient.SocketIOResponse response)
        {
            var userId = response.GetValue<String>();
            var messageId = response.GetValue<String>(1);

            if (ChatsList.OpenedChats.TryGetValue(userId, out var chat))
            {
                uiSync.Send((s) =>
                {
                    chat.Value.SetHaveSeen(messageId);
                }, null);

                ChatsList.MessagesViews[chat.Key._id].MessageRead(messageId);
            }
        }

        private void MessageDeleted(SocketIOClient.SocketIOResponse response)
        {
            var userId = response.GetValue<String>();
            var messageId = response.GetValue<String>(1);

            if (ChatsList.OpenedChats.TryGetValue(userId, out var chat))
            {
                if (ChatsList.MessagesViews.TryGetValue(chat.Key._id, out var messagesView))
                {
                    messagesView.MessageDeleted(messageId);

                    if (messagesView.IsUnreadMessage(messageId))
                        uiSync.Send((s) =>
                        {
                            chat.Value.NotificationBadge.RemoveNotification();
                        }, null);

                    ChatsList.RenderChats(ChatsList.OpenedChats.Values.Select(c => c.Key).ToList());
                }
            }
        }

        private void MessageEdited(SocketIOClient.SocketIOResponse response)
        {
            var message = response.GetValue<Message>();

            if (ChatsList.MessagesViews.TryGetValue(message.chat._id, out var messageView)) 
                messageView.MessageEdited(message);

            message.edits.Add(new Edit());
            EditMessage(message);
        }

        private async void DeleteMessage(Message message)
        {
            var targetUser = message.chat.sender == currentUser ? message.chat.receiver : message.chat.sender;

            if (ChatsList.OpenedChats.TryGetValue(targetUser._id, out var chat))
            {
                if (ChatsList.MessagesViews.TryGetValue(chat.Key._id, out var messagesView))
                {
                    messagesView.MessageDeleted(message._id);
                    ChatsList.RenderChats(ChatsList.OpenedChats.Values.Select(c => c.Key).ToList());
                }
            }

            await Messages.DeleteMessage(message._id);
        }

        private void EnterReplying(Message message)
        {
            if (MessageBorder.OpenedPopup is not null)
                MessageBorder.OpenedPopup.IsOpen = false;

            var targetUser = message.chat.sender == currentUser ? message.chat.receiver : message.chat.sender;

            if (ChatsList.OpenedChats.TryGetValue(targetUser._id, out var chat))
                if (ChatsList.MessagesViews.TryGetValue(chat.Key._id, out var messagesView))
                    messagesView.EnterReplying(message);
        }

        private void EnterEditing(Message message)
        {
            if (MessageBorder.OpenedPopup is not null)
                MessageBorder.OpenedPopup.IsOpen = false;

            var targetUser = message.chat.sender == currentUser ? message.chat.receiver : message.chat.sender;

            if (ChatsList.OpenedChats.TryGetValue(targetUser._id, out var chat))
                if (ChatsList.MessagesViews.TryGetValue(chat.Key._id, out var messagesView))
                    messagesView.EnterEditing(message);
        }

        private void ShowMessageHistory(Message message)
        {
            if (MessageBorder.OpenedPopup is not null)
                MessageBorder.OpenedPopup.IsOpen = false;

            MessagesHistoryPopupStackPanel.Children.Clear();

            bool isOwn = message.sender == currentUser;

            foreach (var edit in message.edits)
            {
                var editedMessageBorder = new MessageBorder(edit.content, isOwn);
                editedMessageBorder.SetLoaded(message);

                MessagesHistoryPopupStackPanel.Children.Add(editedMessageBorder);
            }

            var currentMessageBorder = new MessageBorder(message.content, isOwn);
            currentMessageBorder.SetLoaded(message);

            MessagesHistoryPopupStackPanel.Children.Add(currentMessageBorder);

            MessageHistoryPopup.Visibility = MessageHistoryPopupBackground.Visibility = Visibility.Visible;
        }

        private void ButtonSettings_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            // SelectButton((Border)sender);

            SettingsList.Open();
            OpenList(SettingsList);
        }

        private void ButtonChats_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            OpenList(ChatsList); 
        }

        private void CloseMessagesHistory_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            MessageHistoryPopup.Visibility = MessageHistoryPopupBackground.Visibility = Visibility.Collapsed;
        }

        private void MessageHistoryPopup_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            MessageHistoryPopup.Visibility = MessageHistoryPopupBackground.Visibility = Visibility.Collapsed;
        }
    }
}
