using System;
using System.Threading.Tasks;
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
        public static Action<SocketIOResponse>? OnChatCreated;
        public static Action<SocketIOResponse>? OnMessageSent;
        public static Action<SocketIOResponse>? OnMessageSendFailed;
        public static Action<SocketIOResponse>? OnUserUpdated;
        public static Action<SocketIOResponse>? OnUserTyping;
        public static Action<SocketIOResponse>? OnMessageRead;
        public static Action<SocketIOResponse>? OnMessageDeleted;

        private static SocketIO? client;

        public static bool Configure()
        {
            var jwt = RegistryManager.GetCurrentJwt();
            if (String.IsNullOrEmpty(jwt))
                return false;

            var builder = new SocketBuilder().Host(Host.Url).Jwt(jwt);

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

            if (OnChatCreated != null)
                client.On("chat created", OnChatCreated);

            if (OnMessageSent != null)
                client.On("message sent", OnMessageSent);

            if (OnMessageSendFailed != null)
                client.On("send message failed", OnMessageSendFailed);

            if (OnUserUpdated != null)
                client.On("user updated", OnUserUpdated);

            if (OnUserTyping != null)
                client.On("user typing", OnUserTyping);

            if (OnMessageRead != null)
                client.On("message read", OnMessageRead);

            if (OnMessageDeleted != null)
                client.On("message deleted", OnMessageDeleted);

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

        public static async Task SendMessage(String messageId, String chatId, String content)
        {
            if (client != null)
                await client.EmitAsync("send message", messageId, chatId, content);
        }

        public static async Task ReplyMessage(String messageId, String chatId, String content, String repliedOn)
        {
            if (client != null)
                await client.EmitAsync("send message", messageId, chatId, content, repliedOn);
        }

        public static async Task SendTyping(String targetUserId)
        {
            if (client != null)
                await client.EmitAsync("user typing", targetUserId);
        }

        public static async Task ReadMessage(String messageId)
        {
            if (client != null)
                await client.EmitAsync("read message", messageId);
        }

        public static async Task DeleteMessage(String messageId)
        {
            if (client != null)
                await client.EmitAsync("delete message", messageId);
        }
    }
}
