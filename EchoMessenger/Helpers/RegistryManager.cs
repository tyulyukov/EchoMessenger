using Microsoft.Win32;
using System;

namespace EchoMessenger.Helpers
{
    public static class RegistryManager
    {
        private static RegistryKey echoRegistry = Registry.CurrentUser.CreateSubKey("Echo");

        public static void RememberJwt(String jwt)
        {
            using (RegistryKey account = echoRegistry.CreateSubKey("Account"))
                account.SetValue("JWT", jwt);
        }

        public static void ForgetJwt()
        {
            echoRegistry.DeleteSubKeyTree("Account");
        }

        public static String GetCurrentJwt()
        {
            var account = echoRegistry.OpenSubKey("Account");

            if (account == null)
                return String.Empty;

            var jwt = account.GetValue("JWT");

            if (jwt == null)
                return String.Empty;

            return (String)jwt;
        }
    }
}
