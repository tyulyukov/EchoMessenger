using dotenv.net;
using EchoMessenger.Entities;
using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Net;
using System.Threading.Tasks;

namespace EchoMessenger.Helpers
{
    public static class Database
    {
        private static Rest rest = new Rest(DotEnv.Read()["SERVER_HOST"]);

        public static String HostUrl(String source) => DotEnv.Read()["SERVER_HOST"] + "/" + source;

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

        /*public static async Task<bool> LoginUserWithHashAsync(String username, String passwordHash)
        {
            if (firebase == null)
                return false;

            var users = await firebase.Child("users").OnceAsync<User>();

            foreach (var user in users)
            {
                if (user.Object.Name.ToLower() == username.ToLower() && user.Object.PasswordHash == passwordHash)
                {
                    User = user;
                    return true;
                }
            }

            return false;
        }

        public static async Task<bool> IsUsernameFree(String username)
        {
            if (firebase == null)
                return false;

            var users = await firebase.Child("users").OnceAsync<User>();

            foreach (var user in users)
                if (user.Object.Name.ToLower() == username.ToLower())
                    return false;

            return true;
        }

        public static async Task<bool> ChangeUsername(String newUsername)
        {
            if (firebase == null || User == null)
                return false;

            User.Object.Name = newUsername;

            await firebase.Child("users").Child(User.Key).PatchAsync(User.Object);

            return true;
        }

        public static async Task<bool> ChangePassword(String newPassword)
        {
            if (firebase == null || User == null)
                return false;

            User.Object.PasswordHash = LogInManager.GetPasswordHash(newPassword);

            await firebase.Child("users").Child(User.Key).PatchAsync(User.Object);

            return true;
        }

        public static async Task<bool> ChangeAvatar(String avatarUrl)
        {
            if (firebase == null || User == null)
                return false;

            User.Object.AvatarUrl = avatarUrl;

            await firebase.Child("users").Child(User.Key).PatchAsync(User.Object);

            return true;
        }

        public static IEnumerable<FirebaseObject<User>>? SearchUsers(Func<FirebaseObject<User>, bool> predicate)
        {
            if (firebase == null || User == null || users == null)
                return null;

            var searchedUsers = users.Where(predicate);

            return searchedUsers;
        }

        public static async Task<FirebaseObject<Chat>?> GetChat(User targetUser)
        {
            if (firebase == null || User == null || users == null)
                return null;

            if (chats?.Any(c => c.Object.TargetUser.Name == targetUser.Name || c.Object.FromUser.Name == targetUser.Name) == false)
                return await firebase.Child("chats").PostAsync(new Chat(User.Object, targetUser));
            else
                return chats?.First(c => c.Object.TargetUser.Name == targetUser.Name || c.Object.FromUser.Name == targetUser.Name);
        }

        public static IEnumerable<FirebaseObject<Chat>>? GetLastChats()
        {
            if (firebase == null || User == null || users == null)
                return null;

            return chats?.Where(c => c.Object.TargetUser.Name == User.Object.Name || c.Object.FromUser.Name == User.Object.Name).OrderBy(c => c.Object.GetLastSentAt());
        }

        public static async Task<bool> SendMessage(FirebaseObject<Chat> chat, Message message)
        {
            if (firebase == null || String.IsNullOrWhiteSpace(message.Text))
                return false;

            chat.Object.Messages.Add(message);

            await firebase.Child("chats").Child(chat.Key).PatchAsync(chat.Object);

            return true;
        }

        public static FirebaseObject<User>? GetUserByName(string name)
        {
            if (firebase == null || User == null || users == null)
                return null;

            if (!users.Any(u => u.Object.Name == name))
                return null;

            return users.First(u => u.Object.Name == name);
        }

        private static async Task<IEnumerable<FirebaseObject<User>>> GetUsers()
        {
            if (firebase == null)
                return null;

            var firebaseUsers = await firebase.Child("users").OnceAsync<User>();

            foreach (var user in firebaseUsers)
                user.Object.PasswordHash = String.Empty;

            return firebaseUsers;
        }

        private static async Task<IEnumerable<FirebaseObject<Chat>>> GetChats()
        {
            if (firebase == null)
                return null;

            return await firebase.Child("chats").OnceAsync<Chat>();
        }*/
    }
}
