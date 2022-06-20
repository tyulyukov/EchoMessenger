using EchoMessenger.Helpers.Api;
using RestSharp;
using System;
using System.Threading.Tasks;

namespace EchoMessenger.Helpers.Server
{
    public static class Storage
    {
        private static Rest rest = new Rest();

        public static async Task<RestResponse?> UploadAvatar(String imagePath)
        {
            return await rest.Post("media/upload/avatar", null, imagePath);
        }
    }
}
