using EchoMessenger.Entities;
using EchoMessenger.Helpers;
using Firebase.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace EchoMessenger.Views
{
    /// <summary>
    /// Interaction logic for SearchView.xaml
    /// </summary>
    public partial class SearchView : UserControl
    {
        public SearchView()
        {
            InitializeComponent();
        }

        private async void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox? textBox = sender as TextBox;

            if (textBox == null || String.IsNullOrWhiteSpace(textBox.Text))
                return;

            var users = await SearchUsers(textBox.Text);
            
            UsersStackPanel.Children.Clear();

            if (users == null)
                return;

            foreach (var user in users)
                UsersStackPanel.Children.Add(UIElementsFactory.CreateUsersCard(user.Object.AvatarUrl, user.Object.Name));
        }

        private async Task<IEnumerable<FirebaseObject<User>>?> SearchUsers(String contains)
        {
            return await Database.SearchUsers(u => u.Object.Name.ToLower().Contains(contains.Trim().ToLower()) && u.Key != Database.User.Key);
        }

        private void ButtonSearch_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            SearchTextBox_TextChanged(SearchTextBox, null);
        }
    }
}
