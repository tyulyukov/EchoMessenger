using dotenv.net;
using RestSharp;
using System;
using System.Threading.Tasks;

namespace EchoMessenger.Helpers.Server
{
    public static class Profile
    {
        private static Rest rest = new Rest(DotEnv.Read()["SERVER_HOST"]);

        public static async Task<RestResponse?> UpdateAvatar(String avatarUrl, String originalAvatarUrl)
        {
            return await rest.Post("profile/update/avatar", new { avatarUrl = avatarUrl, originalAvatarUrl = originalAvatarUrl });
        }

        public static async Task<RestResponse?> UpdateUsername(String username)
        {
            return await rest.Post("profile/update/username", new { username = username });
        }

        public static async Task<RestResponse?> UpdatePassword(String oldPassword, String newPassword)
        {
            return await rest.Post("profile/update/password", new { oldPassword = oldPassword, newPassword = newPassword });
        }
    }
}
