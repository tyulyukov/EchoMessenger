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
        public static Action<SocketIOResponse>? OnUsersOnlineReceived;
        public static Action<SocketIOResponse>? OnUserConnected;
        public static Action<SocketIOResponse>? OnUserDisconnected;

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

            if (OnUsersOnlineReceived != null)
                client.On("users online", OnUsersOnlineReceived);

            if (OnUserConnected != null)
                client.On("user connected", OnUserConnected);

            if (OnUsersOnlineReceived != null)
                client.On("user disconnected", OnUserDisconnected);

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
