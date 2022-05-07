using EchoMessenger.Entities;
using EchoMessenger.Helpers;
using Firebase.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

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
                Border messageBorder;

                if (message.Sender.Name == Database.User?.Object.Name)
                    messageBorder = UIElementsFactory.CreateOwnMessage(message.Text, message.SentAt);
                else
                    messageBorder = UIElementsFactory.CreateForeignMessage(message.Text, message.SentAt);

                MessagesStackPanel.Children.Insert(0, messageBorder);
            }
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
            if (MessagesScroll.VerticalOffset + 50 > MessagesScroll.ScrollableHeight)
                ButtonGoBottom.Visibility = Visibility.Collapsed;
            else
                ButtonGoBottom.Visibility = Visibility.Visible;

            if (MessagesScroll.VerticalOffset == 0 && !isLoadingMessages)
                LoadOlderMessages();
        }
    }
}
