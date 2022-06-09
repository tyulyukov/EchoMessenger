using dotenv.net;
using EchoMessenger.Helpers.Api;
using RestSharp;
using System;
using System.Threading.Tasks;

namespace EchoMessenger.Helpers.Server
{
    public static class Storage
    {
        private static Rest rest = new Rest(DotEnv.Read()["SERVER_HOST"]);

        public static async Task<RestResponse?> UploadAvatar(String imagePath)
        {
            return await rest.Post("media/upload/avatar", null, imagePath);
        }
    }
}
