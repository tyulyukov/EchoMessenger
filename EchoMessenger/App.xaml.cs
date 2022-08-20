using EchoMessenger.Core;
using EchoMessenger.Helpers.Server;
using EchoMessenger.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Net;
using System.Threading.Tasks;
using System.Windows;

namespace EchoMessenger
{
    public partial class App : Application
    {
        private async void Application_Startup(object sender, StartupEventArgs e)
        {
            var loader = new PreloaderWindow();
            TimeSpan? waitingDuration = null;

            loader.Show();

            while (true)
            {
                if (waitingDuration is not null)
                {
                    await Task.Delay(waitingDuration.Value);
                    loader.Retrying();
                }

                var response = await Database.ConfirmJwt();

                if (response is not null)
                {
                    if (response.StatusCode == (HttpStatusCode)200 && response.Content is not null)
                    {
                        var result = JObject.Parse(response.Content);
                        var user = result.ToObject<UserInfo>();

                        if (user != null)
                            new MessengerWindow(user).Show();
                        else
                            new LoginWindow().Show();

                        loader.Close();
                        break;
                    }
                    else if (response.StatusCode == (HttpStatusCode)0)
                    {
                        waitingDuration = loader.NoConnection();
                    }
                    else if (response.StatusCode == (HttpStatusCode)401)
                    {
                        new LoginWindow().Show();
                        loader.Close();
                        break;
                    }
                    else
                    {
                        waitingDuration = loader.ServerError();
                    }
                }
                else
                {
                    waitingDuration = loader.NoConnection();
                }
            }
        }
    }
}
