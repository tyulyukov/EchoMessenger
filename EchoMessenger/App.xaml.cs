using EchoMessenger.Helpers.Server;
using System;
using System.Windows;

namespace EchoMessenger
{
    public partial class App : Application
    {
        private async void Application_Startup(object sender, StartupEventArgs e)
        {
            var user = await Database.ConfirmJwt();

            if (user != null)
                new MessengerWindow(user).Show();
            else
                new LoginWindow().Show();
        }
    }
}
