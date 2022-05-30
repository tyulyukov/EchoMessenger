using System;
using System.Linq;

namespace EchoMessenger.Helpers
{
    public class LogInManager
    {
        private static char[] allowedSymbols = { '-', '_', '.' };

        public static bool ValidateUsername(String username)
        {
            if (username.Length < 5 || username.Length >= 20)
                return false;

            if (username.Contains(" "))
                return false;

            return username.All(c => Char.IsLetterOrDigit(c) || IsAllowedSymbol(c));
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

        private static bool IsAllowedSymbol(char symbol) => allowedSymbols.Contains(symbol);
    }
}
