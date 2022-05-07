using EchoMessenger.Entities;
using EchoMessenger.Helpers;
using Firebase.Database;
using System;
using System.Linq;
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

        private MessagesCollection messagesCollection;
        private FirebaseObject<Chat> currentChat;
        private double prevHeight = 0;
        private bool isLoadingMessages = false;
        private bool loaded = false;
        private DateTime? lastMessageSentAt = null;
        private DateTime? firstMessageSentAt = null;

        public MessagesView(MessengerWindow owner)
        {
            InitializeComponent();

            Owner = owner;

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
                    MessageTextBox.Text += "\n";
                else if (e.Key == Key.Enter)
                    SendMessageHandle();
            };
        }

        public void OpenChat(FirebaseObject<Chat> chat)
        {
            if (currentChat == chat)
                return;

            MessagesStackPanel.Children.Clear();
            currentChat = chat;

            TargetUserName.Content = chat.Object.TargetUser.Name;

            if (currentChat.Object.Messages == null)
                return;

            messagesCollection = currentChat.Object.GetMessagesCollection();

            LoadOlderMessages();
            MessagesScroll.ScrollToBottom();
        }

        private void LoadOlderMessages()
        {
            var messages = messagesCollection.Load(LoadingMessagesCount);

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

                if ((lastMessageSentAt?.Year == message.SentAt.Year && lastMessageSentAt?.DayOfYear > message.SentAt.DayOfYear) || lastMessageSentAt?.Year > message.SentAt.Year)
                    MessagesStackPanel.Children.Insert(0, UIElementsFactory.CreateDateCard((DateTime)lastMessageSentAt));

                Border messageBorder;

                if (message.Sender.Name == Database.User?.Object.Name)
                    messageBorder = UIElementsFactory.CreateOwnMessage(message.Text, message.SentAt);
                else
                    messageBorder = UIElementsFactory.CreateForeignMessage(message.Text, message.SentAt);

                lastMessageSentAt = message.SentAt;

                MessagesStackPanel.Children.Insert(0, messageBorder);
            }

            if (messagesCollection.IsAllLoaded)
                MessagesStackPanel.Children.Insert(0, UIElementsFactory.CreateDateCard((DateTime)lastMessageSentAt));

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
            if (String.IsNullOrWhiteSpace(MessageTextBox.Text))
                return;

            var message = new Message(Database.User.Object, MessageTextBox.Text.Trim());
            MessageTextBox.Text = String.Empty;

            if (!await Database.SendMessage(currentChat, message))
            {
                MessageBox.Show("Something went wrong...");
                return;
            }

            if ((firstMessageSentAt?.Year == message.SentAt.Year && firstMessageSentAt?.DayOfYear < message.SentAt.DayOfYear) || firstMessageSentAt?.Year < message.SentAt.Year)
                MessagesStackPanel.Children.Add(UIElementsFactory.CreateDateCard(message.SentAt));

            var messageBorder = UIElementsFactory.CreateOwnMessage(message.Text, message.SentAt);

            MessagesStackPanel.Children.Add(messageBorder);
            MessagesScroll.ScrollToBottom();
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
    }
}
