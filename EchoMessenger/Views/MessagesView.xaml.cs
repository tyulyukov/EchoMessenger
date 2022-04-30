using EchoMessenger.Entities;
using EchoMessenger.Helpers;
using Firebase.Database;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace EchoMessenger
{
    /// <summary>
    /// Interaction logic for MessagesView.xaml
    /// </summary>
    public partial class MessagesView : UserControl
    {
        private FirebaseObject<Chat> currentChat;
        public MessengerWindow? Owner;

        public MessagesView(Window owner)
        {
            InitializeComponent();

            Owner = (MessengerWindow?)owner;
        }

        public void OpenChat(FirebaseObject<Chat> chat)
        {
            currentChat = chat;

            MessagesStackPanel.Children.Clear();
            TargetUserName.Content = chat.Object.TargetUser.Name;

            if (currentChat.Object.Messages == null)
                return;

            foreach (var message in currentChat.Object.Messages)
            {
                Border messageBorder;

                if (message.Sender.Name == Database.User?.Object.Name)
                    messageBorder = UIElementsFactory.CreateOwnMessage(message.Text, message.SentAt);
                else
                    messageBorder = UIElementsFactory.CreateForeignMessage(message.Text, message.SentAt);

                MessagesStackPanel.Children.Add(messageBorder);
            }
        }

        private async void SendMessageButtonClick(object sender, RoutedEventArgs e)
        {
            if (String.IsNullOrWhiteSpace(MessageTextBox.Text))
                return;

            var message = new Message(Database.User.Object, MessageTextBox.Text);
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
    }
}
