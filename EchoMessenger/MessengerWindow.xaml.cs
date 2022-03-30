using EchoMessenger.Entities;
using EchoMessenger.Helpers;
using Firebase.Database;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace EchoMessenger
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MessengerWindow : Window
    {
        private FirebaseObject<User> user;

        public MessengerWindow(FirebaseObject<User> user)
        {
            InitializeComponent();
            this.user = user;

            var message = UIMessageFactory.CreateForeignMessage("ты че даун?", DateTime.Now);
            MessagesStackPanel.Children.Add(message);
        }

        private void SendMessageButtonClick(object sender, RoutedEventArgs e)
        {
            if (String.IsNullOrWhiteSpace(MessageTextBox.Text))
                return;

            var message = UIMessageFactory.CreateOwnMessage(MessageTextBox.Text, DateTime.Now);
            MessageTextBox.Text = String.Empty;

            MessagesStackPanel.Children.Add(message);
            MessagesScroll.ScrollToBottom();
        }
    }
}
