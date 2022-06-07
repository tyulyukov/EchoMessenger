using dotenv.net;
using EchoMessenger.Helpers.Server;
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
            DotEnv.Load();

            var user = await Database.ConfirmJwt();

            if (user != null)
                new MessengerWindow(user).Show();
            else
                new LoginWindow().Show();
        }
    }
}
