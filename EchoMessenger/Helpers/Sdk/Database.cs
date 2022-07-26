using EchoMessenger.Models;
using EchoMessenger.Helpers.Api;
using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Net;
using System.Threading.Tasks;
using EchoMessenger.Core;

namespace EchoMessenger.Helpers.Server
{
    public static class Database
    {
        private static Rest rest = new Rest();

        public async static Task<UserInfo?> ConfirmJwt()
        {
            var jwt = RegistryManager.GetCurrentJwt();
            if (String.IsNullOrEmpty(jwt))
                return null;

            var response = await rest.Get("auth/jwt");

            if (response == null)
                return null;

            if (response.StatusCode == (HttpStatusCode)200)
            {
                if (response.Content == null)
                    return null;

                var result = JObject.Parse(response.Content);
                var user = result.ToObject<UserInfo>();

                return user;
            }

            return null;
        }

        public static async Task<RestResponse?> RegisterAsync(String username, String password)
        {
            return await rest.Post("auth/register", new { username = username, password = password });
        }

        public static async Task<RestResponse?> LoginAsync(String username, String password)
        {
            return await rest.Post("auth/login", new { username = username, password = password });
        }

        public static async Task<RestResponse?> SearchUsers(String query)
        {
            return await rest.Post("users/search", new { query = query });
        }

        public static async Task<RestResponse?> CreateChat(String receiverId)
        {
            return await rest.Post("chats/create", new { receiverId = receiverId });
        }

        public static async Task<RestResponse?> GetLastChats()
        {
            return await rest.Get("chats");
        }

        public static async Task<RestResponse?> LoadMessages(String chatId, int from, int count)
        {
            return await rest.Get("chats/" + chatId + "/messages?from=" + from + "&count=" + count);
        }
    }
}
