using EchoMessenger.Entities;
using Firebase.Database;
using Firebase.Database.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EchoMessenger.Helpers
{
    public static class Database
    {
        public static FirebaseObject<User> User { get; private set; }

        private static FirebaseClient? firebase;

        public static void Configure()
        {
            firebase = new FirebaseClient("https://echo-c09f3-default-rtdb.europe-west1.firebasedatabase.app/");
            User = null;
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

        public static async Task<IEnumerable<FirebaseObject<User>>?> SearchUsers(Func<FirebaseObject<User>, bool> predicate)
        {
            if (firebase == null || User == null)
                return null;

            var users = (await firebase.Child("users").OnceAsync<User>()).Where(predicate);

            foreach (var user in users)
                user.Object.PasswordHash = String.Empty;

            return users;
        }
    }
}
