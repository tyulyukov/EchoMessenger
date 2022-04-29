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
        private Chat? currentChat;
        public MessengerWindow? Owner;

        public MessagesView(Window owner)
        {
            InitializeComponent();

            Owner = (MessengerWindow?)owner;
        }

        public void OpenChat(Chat chat)
        {
            currentChat = chat;

            MessagesStackPanel.Children.Clear();
            TargetUserName.Content = chat.TargetUser.Object.Name;

            if (currentChat.Messages == null)
                return;

            foreach (var message in currentChat.Messages)
            {
                Border messageBorder;

                if (message.Object.Sender.Name == Database.User?.Object.Name)
                    messageBorder = UIElementsFactory.CreateOwnMessage(message.Object.Text, message.Object.SentAt);
                else
                    messageBorder = UIElementsFactory.CreateForeignMessage(message.Object.Text, message.Object.SentAt);

                MessagesStackPanel.Children.Add(messageBorder);
            }
        }

        private async void SendMessageButtonClick(object sender, RoutedEventArgs e)
        {
            if (String.IsNullOrWhiteSpace(MessageTextBox.Text))
                return;

            var message = new Message(Database.User.Object, currentChat.TargetUser.Object, MessageTextBox.Text);
            MessageTextBox.Text = String.Empty;

            if (!await Database.SendMessage(message))
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
