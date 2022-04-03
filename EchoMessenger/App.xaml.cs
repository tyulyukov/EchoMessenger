using EchoMessenger.Helpers;
using System;
using System.Windows;

namespace EchoMessenger
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private async void Application_Startup(object sender, StartupEventArgs e)
        {
            Database.Configure();
            Storage.Configure();

            var userInfo = LogInManager.GetCurrentUser();

            if (userInfo == null)
            {
                new LoginWindow().Show();
                return;
            }

            if (!await Database.LoginUserWithHashAsync(userInfo.Name, userInfo.PasswordHash))
            {
                new LoginWindow().Show();
                return;
            }

            new MessengerWindow().Show();
        }
    }
}
