using EchoMessenger.Models;
using EchoMessenger.UI.Controls.Buttons;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace EchoMessenger.Views.Settings
{
    public partial class SettingsListView : UserControl
    {
        private SynchronizationContext uiSync;
        private MessengerWindow owner;

        private SettingsButton? selectedButton;
        private ITab? openedTab;

        private readonly MyAccountView accountView;

        private readonly Dictionary<SettingsButton, ITab> tabs;

        private ImageBrush myAccountImage = Core.Resources.Find<ImageBrush>("UserImage");

        public SettingsListView(MessengerWindow owner, UserInfo user)
        {
            InitializeComponent();

            uiSync = SynchronizationContext.Current;
            this.owner = owner;

            tabs = new Dictionary<SettingsButton, ITab>();

            accountView = new MyAccountView(owner, user);

            LoadSetting("My Account", myAccountImage, accountView);
        }

        public void Open()
        {
            if (openedTab is null)
            {
                if (SettingsStackPanel.Children.Count > 0)
                    Button_MouseLeftButtonUp(SettingsStackPanel.Children[0], null);
            }
            else
            {
                OpenTab(openedTab);
            }
        }

        public void UpdateUser(UserInfo user)
        {
            accountView.UpdateUser(user);
        }

        private void LoadSetting(String settingName, ImageBrush settingIcon, ITab tab)
        {
            var button = new SettingsButton(settingName, settingIcon);
            button.MouseLeftButtonUp += Button_MouseLeftButtonUp;

            tabs.Add(button, tab);

            SettingsStackPanel.Children.Add(button);
        }

        private void Button_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var button = sender as SettingsButton;

            if (button is null)
                return;

            if (selectedButton == button)
                return;

            button.Select();

            if (selectedButton is not null)
                selectedButton.Deselect();

            selectedButton = button;

            OpenTab(tabs[selectedButton]);
        }

        private void OpenTab(ITab tab)
        {
            tab.Open();
            openedTab = tab;
            owner.OpenTab(tab as UserControl);
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
