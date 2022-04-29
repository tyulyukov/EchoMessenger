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
        public readonly MessagesView MessagesView;
        public readonly SettingsView SettingsView;
        public readonly SearchView SearchView;

        public MessengerWindow()
        {
            InitializeComponent();

            MessagesView = new MessagesView();
            SettingsView = new SettingsView(this);
            SearchView = new SearchView();

            OpenTab(MessagesView);
        }

        private void OpenTab(UserControl tab)
        {
            OpenedTab.Children.Clear();
            OpenedTab.Children.Add(tab);
        }
        
        private void ButtonOpenChat_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            OpenTab(MessagesView);
        }

        private void ButtonSettings_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            SettingsView.Open();
            OpenTab(SettingsView);
        }

        private void ButtonSearch_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            OpenTab(SearchView);
        }
    }
}
