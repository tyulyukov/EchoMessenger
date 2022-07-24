using EchoMessenger.Models;
using System;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace EchoMessenger.Views.Settings
{
    public partial class SettingsListView : UserControl
    {
        private SynchronizationContext? uiSync;
        private MessengerWindow owner;

        private Border? lastButtonTab;
        private UserControl? openedTab;
        private double progress;
        private bool isWorking = false;
        private readonly MyAccountView accountView;

        public SettingsListView(MessengerWindow owner, UserInfo user)
        {
            InitializeComponent();

            uiSync = SynchronizationContext.Current;
            this.owner = owner;

            accountView = new MyAccountView(owner, user);
        }

        public void Open()
        {
            if (openedTab == null)
                return;

            if (openedTab == accountView)
                accountView.Open();
        }

        public void UpdateUser(UserInfo user)
        {
            accountView.UpdateUser(user);
        }

        private void ButtonTab_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            HandleButtonTabClick((Border)sender);
        }

        private void HandleButtonTabClick(Border border)
        {
            if (lastButtonTab != null)
                lastButtonTab.Background = new SolidColorBrush(Colors.Transparent);

            var id = border.Uid.ToString();

            border.Background = new SolidColorBrush(Colors.DimGray);
            lastButtonTab = border;

            switch (id)
            {
                case "My account":
                    {
                        accountView.Open();
                        owner.OpenTab(accountView);
                        break;
                    }
            }
        }

        private void UserControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            HandleButtonTabClick(MyAccountButtonTab);
        }

        /*public void StartFillingProgressBar()
        {
            isWorking = true;
            SetProgress(0, TimeSpan.FromMilliseconds(100));

            while (progress != 95)
            {
                if (!isWorking)
                    break;

                SetProgress(progress + 1, TimeSpan.FromMilliseconds(50));

                if (progress < 50)
                    Thread.Sleep(50);
                else if (progress < 75)
                    Thread.Sleep(500);
                else if (progress < 90)
                    Thread.Sleep(1500);
                else if (progress < 95)
                    Thread.Sleep(5000);
                else break;
            }
        }

        public void EndFillingProgressBar()
        {
            isWorking = false;
            double delay = 100 - progress;

            SetProgress(100, TimeSpan.FromMilliseconds(delay));
            Thread.Sleep((int)delay);
            SetProgress(0, TimeSpan.FromMilliseconds(100));
        }

        private void SetProgress(double progress, TimeSpan duration)
        {
            this.progress = progress;
            uiSync?.Send(state => { SettingsProgressBar.SetPercent(progress, duration); }, null);
        }*/
    }
}
