using EchoMessenger.Helpers.Api;
using RestSharp;
using System;
using System.Threading.Tasks;

namespace EchoMessenger.Helpers.Server
{
    public static class Profile
    {
        private static Rest rest = new Rest();

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
