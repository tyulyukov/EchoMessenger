using SocketIOClient;
using System;
using System.Collections.Generic;

namespace EchoMessenger.Helpers.Api
{
    public class SocketBuilder
    {
        private String host;
        private String jwt;
        private EventHandler<int> reconnect;
        private EventHandler<int> reconnected;
        private EventHandler connected;
        private EventHandler<String> onError;

        public SocketBuilder Host(String host)
        {
            var builder = this;
            builder.host = host;
            return builder;
        }

        public SocketBuilder Jwt(String jwt)
        {
            var builder = this;
            builder.jwt = jwt;
            return builder;
        }

        public SocketBuilder Reconection(EventHandler<int> reconnect, EventHandler<int> reconnected)
        {
            var builder = this;
            builder.reconnect = reconnect;
            builder.reconnected = reconnected;
            return builder;
        }

        public SocketBuilder Connection(EventHandler connected)
        {
            var builder = this;
            builder.connected = connected;
            return builder;
        }

        public SocketBuilder Error(EventHandler<String> onError)
        {
            var builder = this;
            builder.onError = onError;
            return builder;
        }

        public SocketIO Build()
        {
            var client = new SocketIO(host, new SocketIOOptions
            {
                Query = new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>("authorization", jwt),
                },
            });

            client.OnReconnectAttempt += reconnect;
            client.OnReconnected += reconnected;
            client.OnConnected += connected;
            client.OnError += onError;

            return client;
        }
    }
}
