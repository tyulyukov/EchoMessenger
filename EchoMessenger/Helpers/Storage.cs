﻿using System;
using System.IO;
using System.Threading.Tasks;
using Firebase.Storage;

namespace EchoMessenger.Helpers
{
    public static class Storage
    {
        private static FirebaseStorage? firebase;

        public static void Configure()
        {
            firebase = new FirebaseStorage("echo-c09f3.appspot.com");
        }

        public static async Task<String> UploadAvatarAsync(Stream imageStream)
        {
            if (firebase == null || Database.User == null)
                return String.Empty;

            return await firebase.Child("avatars").Child($"avatar-{Database.User.Object.Name}-{DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss-ffff")}").PutAsync(imageStream);
        }
    }
}