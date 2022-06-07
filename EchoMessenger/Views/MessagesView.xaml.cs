using EchoMessenger.Entities;
using EchoMessenger.Helpers;
using EchoMessenger.Helpers.Server;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace EchoMessenger
{
    /// <summary>
    /// Interaction logic for MessagesView.xaml
    /// </summary>
    public partial class MessagesView : UserControl
    {
        public const int LoadingMessagesCount = 15;
        public MessengerWindow Owner;

        private bool isLoading = false;
        private SynchronizationContext uiSync;
        private Chat currentChat;
        private UserInfo currentUser;
        private double prevHeight = 0;
        private bool isLoadingMessages = false;
        private bool loaded = false;
        private DateTime? lastMessageSentAt = null;
        private DateTime? firstMessageSentAt = null;
        private Dictionary<String, Chat> openedChats;

        public MessagesView(MessengerWindow owner, UserInfo user)
        {
            InitializeComponent();
            uiSync = SynchronizationContext.Current;

            Owner = owner;
            currentUser = user;

            openedChats = new Dictionary<String, Chat>();
        }

        public void OpenChat(Chat? chat)
        {
            if (chat == null)
                return;

            if (currentChat == chat)
                return;

            uiSync.Post((s) =>
            {
                MessagesStackPanel.Children.Clear();
                currentChat = chat;

                TargetUserName.Content = chat.sender.username == currentUser.username ? chat.receiver.username : chat.sender.username;

                LoadOlderMessages();
                MessagesScroll.ScrollToBottom();
            }, null);
        }

        public async Task OpenChat(String userId)
        {
            Chat? chat = null;

            if (openedChats.ContainsKey(userId))
            {
                chat = openedChats[userId];
            }
            else
            {
                try
                {
                    uiSync.Post((s) => { ShowLoading(true); }, null);
                    var chatResponse = await Database.CreateChat(userId);

                    if (chatResponse == null || chatResponse.StatusCode == (HttpStatusCode)0)
                    {
                        MessageBox.Show(Owner, "Can`t establish connection", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                    else if (chatResponse.StatusCode == (HttpStatusCode)500)
                    {
                        MessageBox.Show(Owner, "Oops... Something went wrong", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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

                        var targetUser = chat.sender.username == currentUser.username ? chat.receiver : chat.sender;
                        Owner.AddUserIcon(targetUser, chat);
                        openedChats.Add(userId, chat);
                    }
                    else if (chatResponse.StatusCode == (HttpStatusCode)401)
                    {
                        RegistryManager.ForgetJwt();
                        Owner.Hide();
                        new LoginWindow().Show();
                        Owner.Close();
                        return;
                    }
                }
                finally
                {
                    uiSync.Post((s) => { ShowLoading(false); }, null);
                }
            }

            OpenChat(chat);
        }

        public void LoadChat(String userId, Chat chat)
        {
            if (!openedChats.ContainsKey(userId))
                openedChats.Add(userId, chat);
        }

        public void UpdateUser(UserInfo user)
        {
            currentUser = user;
        }

        public void ShowLoading(bool visible)
        {
            LoadingBorder.Visibility = visible ? Visibility.Visible : Visibility.Collapsed;
            isLoading = visible;
        }

        private void LoadOlderMessages()
        {
            /*var messages = messagesCollection.Load(LoadingMessagesCount);

            if (messages.Count() == 0)
                return;

            if (MessagesScroll.IsLoaded)
            {
                isLoadingMessages = true;
                prevHeight = MessagesScroll.ExtentHeight;
                MessagesScroll.LayoutUpdated += MessagesScroll_LayoutUpdated;
            }

            foreach (var message in messages)
            {
                if (firstMessageSentAt == null)
                    firstMessageSentAt = message.SentAt;

                Border messageBorder;

                if (message.Sender.Name == Database.User?.Object.Name)
                    messageBorder = UIElementsFactory.CreateOwnMessage(message.Text, message.SentAt);
                else
                    messageBorder = UIElementsFactory.CreateForeignMessage(message.Text, message.SentAt);

                if ((lastMessageSentAt?.Year == message.SentAt.Year && lastMessageSentAt?.DayOfYear > message.SentAt.DayOfYear) || lastMessageSentAt?.Year > message.SentAt.Year)
                    MessagesStackPanel.Children.Insert(0, UIElementsFactory.CreateDateCard((DateTime)lastMessageSentAt));
                else if (lastMessageSentAt > message.SentAt.AddHours(1))
                    messageBorder.Margin = new Thickness(messageBorder.Margin.Left, messageBorder.Margin.Top, messageBorder.Margin.Right, messageBorder.Margin.Bottom + 10);
                else if (lastMessageSentAt > message.SentAt.AddMinutes(10))
                    messageBorder.Margin = new Thickness(messageBorder.Margin.Left, messageBorder.Margin.Top, messageBorder.Margin.Right, messageBorder.Margin.Bottom + 5);

                MessagesStackPanel.Children.Insert(0, messageBorder);
                
                lastMessageSentAt = message.SentAt;
            }

            if (messagesCollection.IsAllLoaded)
                MessagesStackPanel.Children.Insert(0, UIElementsFactory.CreateDateCard((DateTime)lastMessageSentAt));*/

        }

        private void MessagesScroll_LayoutUpdated(object? sender, EventArgs e)
        {
            if (MessagesScroll.ExtentHeight == prevHeight)
                return;

            isLoadingMessages = false;
            MessagesScroll.ScrollToVerticalOffset(MessagesScroll.ExtentHeight - prevHeight);
            MessagesScroll.LayoutUpdated -= MessagesScroll_LayoutUpdated;
        }

        private void SendMessageButtonClick(object sender, RoutedEventArgs e)
        {
            SendMessageHandle();
        }

        private async void SendMessageHandle()
        {
            /*if (String.IsNullOrWhiteSpace(MessageTextBox.Text))
                return;

            var message = new Message(Database.User.Object, MessageTextBox.Text.Trim());
            MessageTextBox.Text = String.Empty;

            if (!await Database.SendMessage(currentChat, message))
            {
                MessageBox.Show("Something went wrong...");
                return;
            }

            var messageBorder = UIElementsFactory.CreateOwnMessage(message.Text, message.SentAt);

            if ((firstMessageSentAt?.Year == message.SentAt.Year && firstMessageSentAt?.DayOfYear < message.SentAt.DayOfYear) || firstMessageSentAt?.Year < message.SentAt.Year)
                MessagesStackPanel.Children.Add(UIElementsFactory.CreateDateCard(message.SentAt));
            else if (firstMessageSentAt?.AddMinutes(10) < message.SentAt)
                messageBorder.Margin = new Thickness(messageBorder.Margin.Left, messageBorder.Margin.Top + 5, messageBorder.Margin.Right, messageBorder.Margin.Bottom);
            else if (firstMessageSentAt?.AddHours(1) < message.SentAt)
                messageBorder.Margin = new Thickness(messageBorder.Margin.Left, messageBorder.Margin.Top + 10, messageBorder.Margin.Right, messageBorder.Margin.Bottom);


            firstMessageSentAt = message.SentAt;

            MessagesStackPanel.Children.Add(messageBorder);
            MessagesScroll.ScrollToBottom();*/
        }

        private void ButtonGoBottom_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            MessagesScroll.ScrollToBottom();
        }

        private void MessagesScroll_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            bool isVisible = MessagesScroll.VerticalOffset + 50 < MessagesScroll.ScrollableHeight;
            ButtonGoBottom.ChangeVisibility(isVisible, TimeSpan.FromMilliseconds(150));

            if (MessagesScroll.VerticalOffset == 0 && !isLoadingMessages)
                LoadOlderMessages();
        }

        private void UserControl_LayoutUpdated(object sender, EventArgs e)
        {
            if ((ActualHeight > 0 || ActualWidth > 0) && !loaded)
            {
                TargetUserName.ShowSmoothly(TimeSpan.FromMilliseconds(150));

                loaded = true;
            }
            else if (ActualHeight == 0 && ActualWidth == 0)
            {
                loaded = false;
            }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            var placeholder = MessageTextBox.Text;
            bool isTextChanged = false;

            TextChangedEventHandler textChanged = (s, e) =>
            {
                isTextChanged = !String.IsNullOrEmpty(MessageTextBox.Text);
            };

            MessageTextBox.GotFocus += (s, e) =>
            {
                if (MessageTextBox.Text == placeholder && !isTextChanged)
                    MessageTextBox.Text = String.Empty;

                MessageTextBox.TextChanged += textChanged;
            };

            MessageTextBox.LostFocus += (s, e) =>
            {
                MessageTextBox.TextChanged -= textChanged;

                if (String.IsNullOrEmpty(MessageTextBox.Text))
                    MessageTextBox.Text = placeholder;
            };

            MessageTextBox.KeyDown += (s, e) =>
            {
                if (e.Key == Key.Enter && Keyboard.IsKeyDown(Key.LeftShift))
                {
                    var index = MessageTextBox.CaretIndex;
                    MessageTextBox.Text = MessageTextBox.Text.Insert(index, "\n");
                    MessageTextBox.CaretIndex = index + 1;
                }
                else if (e.Key == Key.Enter)
                {
                    SendMessageHandle();
                }
            };
        }
    }
}
