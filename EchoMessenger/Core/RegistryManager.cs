using Microsoft.Win32;
using System;

namespace EchoMessenger.Core
{
    public static class RegistryManager
    {
        public static String Jwt { get; private set; } = String.Empty;

        private static RegistryKey echoRegistry = Registry.CurrentUser.CreateSubKey("Echo");

        public static void RememberJwt(String jwt)
        {
            Jwt = jwt;

            using (RegistryKey account = echoRegistry.CreateSubKey("Account"))
                account.SetValue("JWT", jwt);
        }

        public static void ForgetJwt()
        {
            Jwt = String.Empty;
            echoRegistry.DeleteSubKeyTree("Account");
        }

        public static String GetCurrentJwt()
        {
            if (!String.IsNullOrEmpty(Jwt))
                return Jwt;

            var account = echoRegistry.OpenSubKey("Account");

            if (account == null)
                return String.Empty;

            var jwt = account.GetValue("JWT");

            if (jwt == null)
                return String.Empty;

            Jwt = (String)jwt;

            return (String)jwt;
        }
    }
}
