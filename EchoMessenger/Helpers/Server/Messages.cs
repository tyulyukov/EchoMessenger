using System;
using System.Threading.Tasks;
using dotenv.net;
using EchoMessenger.Helpers.Api;
using SocketIOClient;

namespace EchoMessenger.Helpers.Server
{
    public static class Messages
    {
        public static EventHandler<int>? OnReconnect;
        public static EventHandler<int>? OnReconnected;
        public static EventHandler? OnConnected;
        public static EventHandler<String>? OnError;

        private static SocketIO? client;

        public static bool Configure()
        {
            var jwt = RegistryManager.GetCurrentJwt();
            if (String.IsNullOrEmpty(jwt))
                return false;

            var builder = new SocketBuilder().Host(DotEnv.Read()["SERVER_HOST"]).Jwt(jwt);

            if (OnReconnect != null && OnReconnected != null)
                builder.Reconection(OnReconnect, OnReconnected);

            if (OnConnected != null)
                builder.Connection(OnConnected);

            if (OnError != null)
                builder.Error(OnError);

            client = builder.Build();
            return true;
        }

        public static async Task Connect()
        {
            if (client != null)
                await client.ConnectAsync();
        }

        public static async Task Disconnect()
        {
            if (client != null)
                await client.DisconnectAsync();
        }
    }
}
