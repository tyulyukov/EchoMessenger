using EchoMessenger.Entities;
using EchoMessenger.Helpers;
using EchoMessenger.Views;
using EchoMessenger.Views.Settings;
using Firebase.Database;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace EchoMessenger
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MessengerWindow : Window
    {
        private readonly MessagesView messagesView;
        private readonly SettingsView settingsView;
        private readonly SearchView searchView;

        public MessengerWindow()
        {
            InitializeComponent();

            messagesView = new MessagesView();
            settingsView = new SettingsView(this);
            searchView = new SearchView();

            OpenTab(messagesView);
        }

        private void OpenTab(UserControl tab)
        {
            OpenedTab.Children.Clear();
            OpenedTab.Children.Add(tab);
        }
        
        private void ButtonOpenChat_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            OpenTab(messagesView);
        }

        private void ButtonSettings_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            settingsView.Open();
            OpenTab(settingsView);
        }

        private void ButtonSearch_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            OpenTab(searchView);
        }
    }
}
