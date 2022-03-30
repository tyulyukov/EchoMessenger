using EchoMessenger.Entities;
using EchoMessenger.Helpers;
using Firebase.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        }

        private void SendMessageButtonClick(object sender, RoutedEventArgs e)
        {
            if (String.IsNullOrWhiteSpace(MessageTextBox.Text))
                return;

            var message = CreateMyMessage(MessageTextBox.Text);
            MessageTextBox.Text = String.Empty;

            MessagesStackPanel.Children.Add(message);
            MessagesScroll.ScrollToBottom();
        }

        // NOTE: Create a fabric of UIElements
        private UIElement CreateMyMessage(String text)
        {
            var border = new Border();
            border.MinHeight = 50;
            border.MaxWidth = 500;
            border.BorderBrush = new SolidColorBrush(Colors.White);
            border.BorderThickness = new Thickness(1);
            border.Margin = new Thickness(10);
            border.HorizontalAlignment = HorizontalAlignment.Right;
            border.Background = (SolidColorBrush)new BrushConverter().ConvertFrom("#283EFF");
            border.CornerRadius = new CornerRadius(30);

            var textBlock = new TextBlock();
            textBlock.Text = text;
            textBlock.TextWrapping = TextWrapping.Wrap;
            textBlock.VerticalAlignment = VerticalAlignment.Center;
            textBlock.Foreground = new SolidColorBrush(Colors.White);
            textBlock.FontSize = 14;
            textBlock.HorizontalAlignment = HorizontalAlignment.Left;
            textBlock.Margin = new Thickness(10);

            border.Child = textBlock;

            return border;
        }
    }
}
