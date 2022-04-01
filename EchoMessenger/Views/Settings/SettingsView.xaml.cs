using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace EchoMessenger.Views.Settings
{
    /// <summary>
    /// Interaction logic for SettingsView.xaml
    /// </summary>
    public partial class SettingsView : UserControl
    {
        private Border? lastButtonTab;
        private UserControl? openedTab;

        private readonly MyAccountView accountView;

        public SettingsView(Window owner)
        {
            InitializeComponent();

            accountView = new MyAccountView(owner);

            HandleButtonTabClick(MyAccountButtonTab);
        }

        public void Open()
        {
            if (openedTab == null)
                return;

            if (openedTab == accountView)
                accountView.Open();
        }

        private void ButtonTab_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            HandleButtonTabClick((Border)sender);
        }

        private void HandleButtonTabClick(Border border)
        {
            if (lastButtonTab != null)
                lastButtonTab.Background = new SolidColorBrush(Colors.Transparent);

            var id = Convert.ToInt32(border.Uid);

            border.Background = new SolidColorBrush(Colors.DimGray);
            lastButtonTab = border;

            switch (id)
            {
                case 0:
                    {
                        accountView.Open();
                        OpenTab(accountView);
                        break;
                    }
            }

            TabName.Text = ((TextBlock)border.Child).Text;
        }

        private void OpenTab(UserControl tab)
        {
            OpenedTab.Children.Clear();
            OpenedTab.Children.Add(tab);
            openedTab = tab;
        }
    }
}
