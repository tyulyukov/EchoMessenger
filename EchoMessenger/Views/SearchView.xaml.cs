using EchoMessenger.Entities;
using EchoMessenger.Helpers;
using EchoMessenger.Helpers.Server;
using EchoMessenger.Helpers.UI;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;

namespace EchoMessenger.Views
{
    public partial class SearchView : UserControl
    {
        private bool isSearching = false;
        private double progress = 0;
        private SynchronizationContext? uiSync;
        private object locker = new object();
        private MessengerWindow owner;
        private TypeAssistant typeAssistant;

        public SearchView(MessengerWindow owner)
        {
            InitializeComponent();
            uiSync = SynchronizationContext.Current;

            typeAssistant = new TypeAssistant(350);
            typeAssistant.Idled += TypeAssistant_Idled;

            this.owner = owner;
        }

        private void TypeAssistant_Idled(object? sender, EventArgs e)
        {
            Dispatcher.Invoke(async () =>
            {
                var query = SearchTextBox.Text;

                if (String.IsNullOrWhiteSpace(query))
                {
                    _ = Task.Run(EndFillingProgressBar);
                    foreach (Border control in UsersStackPanel.Children)
                        control.ChangeVisibility(false);

                    if (UsersStackPanel.Children.Count != 0)
                        await Task.Delay(150).ContinueWith((task) => { uiSync?.Send(state => { UsersStackPanel.Children.Clear(); }, null); }); 

                    return;
                }

                _ = Task.Run(StartFillingProgressBar);

                try
                {
                    var response = await Database.SearchUsers(query);

                    if (response == null)
                        return;

                    if (response.StatusCode == (HttpStatusCode)200)
                    {
                        if (response.Content == null)
                            return;

                        var result = JArray.Parse(response.Content);

                        var users = result?.ToObject<IEnumerable<UserInfo>>();

                        if (users == null)
                            return;

                        lock (locker)
                        {
                            UsersStackPanel.Children.Clear();

                            foreach (var user in users)
                            {
                                var userCard = new SearchResultUserCard(user.avatarUrl, user.username);

                                userCard.MouseLeftButtonUp += (s, e) =>
                                {
                                    _ = Task.Run(async () =>
                                    {
                                        await owner.OpenChat(user._id);
                                    });
                                };

                                if (user == users.First())
                                    userCard.SetFirst();

                                if (user == users.Last())
                                    userCard.SetLast();

                                userCard.SetSlideFromLeftOnLoad();
                                UsersStackPanel.Children.Add(userCard);
                            }
                        }
                    }
                    else if (response.StatusCode == (HttpStatusCode)401)
                    {
                        RegistryManager.ForgetJwt();
                        owner.Hide();
                        new LoginWindow().Show();
                        owner.Close();
                    }
                }
                finally
                {
                    _ = Task.Run(EndFillingProgressBar);
                }
            });
        }

        private async void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            typeAssistant.TextChanged();
        }

        private void ButtonSearch_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            SearchTextBox_TextChanged(SearchTextBox, new TextChangedEventArgs(e.RoutedEvent, UndoAction.None));
        }

        private void StartFillingProgressBar()
        {
            isSearching = true;
            SetProgress(0, TimeSpan.FromMilliseconds(100));

            while (progress != 95)
            {
                if (!isSearching)
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

        private void SetProgress(double progress, TimeSpan duration)
        {
            this.progress = progress;
            uiSync?.Send(state => { SearchProgressBar.SetPercent((double)state, duration); }, progress);
        }

        private void EndFillingProgressBar()
        {
            isSearching = false;
            double delay = 100 - progress;

            SetProgress(100, TimeSpan.FromMilliseconds(delay));
            Thread.Sleep((int)delay);
            SetProgress(0, TimeSpan.FromMilliseconds(100));
        }
    }
}