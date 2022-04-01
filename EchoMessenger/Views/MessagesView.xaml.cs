using EchoMessenger.Helpers;
using System;
using System.Windows;
using System.Windows.Controls;

namespace EchoMessenger
{
    /// <summary>
    /// Interaction logic for MessagesView.xaml
    /// </summary>
    public partial class MessagesView : UserControl
    {
        public MessagesView()
        {
            InitializeComponent();

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
