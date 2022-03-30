using EchoMessenger.Entities;
using Firebase.Database;
using Firebase.Database.Query;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace EchoMessenger.Helpers
{
    public static class Database
    {
        private static FirebaseClient? firebase;

        public static void Configure()
        {
            firebase = new FirebaseClient("https://echo-c09f3-default-rtdb.europe-west1.firebasedatabase.app/");
        }

        public static async Task<FirebaseObject<User>?> RegisterUserAsync(User user)
        {
            if (firebase == null)
                return null;

            user.Name = user.Name.ToLower();

            return await firebase.Child("users").PostAsync(user);
        }

        public static async Task<FirebaseObject<User>?> LoginUserAsync(String username, String password)
        {
            if (firebase == null)
                return null;

            var users = await firebase.Child("users").OnceAsync<User>();

            foreach (var user in users)
                if (user.Object.Name.ToLower() == username.ToLower() && LogInManager.VerifyPassword(user.Object.PasswordHash, password))
                    return user;

            return null;
        }

        public static async Task<FirebaseObject<User>?> LoginUserWithHashAsync(String username, String passwordHash)
        {
            if (firebase == null)
                return null;

            var users = await firebase.Child("users").OnceAsync<User>();

            foreach (var user in users)
                if (user.Object.Name.ToLower() == username.ToLower() && user.Object.PasswordHash == passwordHash)
                    return user;

            return null;
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
    }
}
