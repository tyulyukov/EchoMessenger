using EchoMessenger.Entities;
using EchoMessenger.Helpers;
using System;
using System.Threading;
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
        private double progress;
        private bool isWorking = false;
        private SynchronizationContext? uiSync;

        private readonly MyAccountView accountView;

        public SettingsView(MessengerWindow owner, UserInfo user)
        {
            InitializeComponent();

            uiSync = SynchronizationContext.Current;

            accountView = new MyAccountView(owner, user);

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

        public void StartFillingProgressBar()
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
            uiSync?.Send(state => { SettingsProgressBar.SetPercent((double)state, duration); }, progress);
        }
    }
}
