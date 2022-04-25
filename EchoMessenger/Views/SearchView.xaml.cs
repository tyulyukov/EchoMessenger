using EchoMessenger.Entities;
using EchoMessenger.Helpers;
using Firebase.Database;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace EchoMessenger.Views
{
    /// <summary>
    /// Interaction logic for SearchView.xaml
    /// </summary>
    public partial class SearchView : UserControl
    {
        private bool isSearching = false;
        private double progress = 0;
        private SynchronizationContext? uiSync;
        private IEnumerable<FirebaseObject<User>>? users;
        private object locker = new object();

        public SearchView()
        {
            InitializeComponent();
            uiSync = SynchronizationContext.Current;
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox? textBox = sender as TextBox;

            if (textBox == null || String.IsNullOrWhiteSpace(textBox.Text))
            {
                UsersStackPanel.Children.Clear();
                return;
            }

            _ = Task.Run(StartFillingProgressBar);
            users = SearchUsers(textBox.Text);
            _ = Task.Run(EndFillingProgressBar);

            if (users == null)
                return;

            lock (locker)
            {
                UsersStackPanel.Children.Clear();

                foreach (var user in users)
                    UsersStackPanel.Children.Add(UIElementsFactory.CreateUsersCard(user.Object.AvatarUrl, user.Object.Name));
            }
        }

        private IEnumerable<FirebaseObject<User>>? SearchUsers(String contains)
        {
            return Database.SearchUsers(u => u.Object.Name.ToLower().Contains(contains.Trim().ToLower()) && u.Key != Database.User?.Key);
        }

        private void ButtonSearch_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            SearchTextBox_TextChanged(SearchTextBox, (TextChangedEventArgs)EventArgs.Empty);
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