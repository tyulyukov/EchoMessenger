using EchoMessenger.Entities;
using Firebase.Database;
using Firebase.Database.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace EchoMessenger.Helpers
{
    public static class Database
    {
        public static FirebaseObject<User>? User { get; private set; }

        private static IEnumerable<FirebaseObject<User>>? users;
        private static IEnumerable<FirebaseObject<Message>>? messages;
        private static FirebaseClient? firebase;

        public static async Task Configure()
        {
            firebase = new FirebaseClient("https://echo-c09f3-default-rtdb.europe-west1.firebasedatabase.app/");
            User = null;
            users = await GetUsers();
            messages = await GetMessages();

            var observableUsers = firebase
              .Child("users")
              .AsObservable<User>()
              .Subscribe(async u => users = await GetUsers());

            var observableMessages = firebase
              .Child("messages")
              .AsObservable<Message>()
              .Subscribe(async m => messages = await GetMessages());
        }

        public static async Task<bool> RegisterUserAsync(User user)
        {
            if (firebase == null)
                return false;

            user.Name = user.Name.ToLower();

            User = await firebase.Child("users").PostAsync(user);

            return true;
        }

        public static async Task<bool> LoginUserAsync(String username, String password)
        {
            if (firebase == null)
                return false;

            var users = await firebase.Child("users").OnceAsync<User>();

            foreach (var user in users)
            {
                if (user.Object.Name.ToLower() == username.ToLower() && LogInManager.VerifyPassword(user.Object.PasswordHash, password))
                {
                    User = user;
                    return true;
                }
            }

            return false;
        }

        public static async Task<bool> LoginUserWithHashAsync(String username, String passwordHash)
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

        public static Chat? GetChat(FirebaseObject<User> targetUser)
        {
            if (firebase == null || User == null || users == null)
                return null;

            var chatMessages = messages?.Where(m => 
                             (m.Object.Receiver.Name == targetUser.Object.Name && m.Object.Sender.Name == User.Object.Name)
                             || (m.Object.Sender.Name == targetUser.Object.Name && m.Object.Receiver.Name == User.Object.Name))
                             .OrderBy(m => m.Object.SentAt)
                             .ToList();

            return new Chat(targetUser, chatMessages);
        }

        public static async Task<bool> SendMessage(Message message)
        {
            if (firebase == null || String.IsNullOrWhiteSpace(message.Text))
                return false;

            await firebase.Child("messages").PostAsync(message);

            return true;
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

        private static async Task<IEnumerable<FirebaseObject<Message>>> GetMessages()
        {
            if (firebase == null)
                return null;

            return await firebase.Child("messages").OnceAsync<Message>();
        }
    }
}
