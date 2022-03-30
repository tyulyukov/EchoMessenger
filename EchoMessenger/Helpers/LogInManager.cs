using EchoMessenger.Entities;
using Microsoft.Win32;
using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace EchoMessenger.Helpers
{
    public static class LogInManager
    {
        private static RegistryKey echoRegistry = Registry.CurrentUser.CreateSubKey("Echo");
        private static char[] allowedSymbols = { '-', '_', '.' };

        public static String GetPasswordHash(String str)
        {
            if (String.IsNullOrWhiteSpace(str))
                return String.Empty;

            using (HashAlgorithm algorithm = SHA256.Create())
            {
                byte[] buffer = algorithm.ComputeHash(GetByteArray(str));

                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < buffer.Length; i++)
                    builder.Append(buffer[i].ToString("x2"));

                return builder.ToString();
            }
        }
        
        public static bool VerifyPassword(String passwordHash, String password)
        {
            return passwordHash == GetPasswordHash(password);
        }

        public static bool ValidatePassword(String password)
        {
            if (password.Length < 8)
                return false;

            if (password.Contains(" "))
                return false;

            bool hasLetters = false;
            bool hasDigits = false;

            foreach (char symbol in password)
            {
                if (char.IsLetter(symbol))
                    hasLetters = true;

                if (char.IsDigit(symbol))
                    hasDigits = true;
            }

            if (!hasLetters || !hasDigits)
                return false;

            return true;
        }

        public static void Remember(User user)
        {
            using (RegistryKey rememberedUser = echoRegistry.CreateSubKey("RememberedUser"))
            {
                rememberedUser.SetValue("Username", user.Name);
                rememberedUser.SetValue("PasswordHash", user.PasswordHash);
            }
        }

        public static void ForgetCurrentUser()
        {
            echoRegistry.DeleteSubKeyTree("RememberedUser");
        }

        public static UserInfo? GetCurrentUser()
        {
            var rememberedUser = echoRegistry.OpenSubKey("RememberedUser");

            if (rememberedUser == null)
                return null;

            var username = rememberedUser.GetValue("Username");
            var passwordHash = rememberedUser.GetValue("PasswordHash");

            if (username == null || passwordHash == null)
                return null;

            return new UserInfo() { Name = (String)username, PasswordHash = (String)passwordHash };
        }

        public static bool ValidateUsername(String username)
        {
            if (username.Length < 5 || username.Length >= 20)
                return false;

            if (username.Contains(" "))
                return false;

            return username.All(c => Char.IsLetterOrDigit(c) || IsAllowedSymbol(c));
        }

        private static byte[] GetByteArray(String str) => Encoding.UTF8.GetBytes(str);

        private static String GetString(byte[] buf) => Encoding.UTF8.GetString(buf, 0, buf.Length);

        private static bool IsAllowedSymbol(char symbol) => allowedSymbols.Contains(symbol);
    }
}
