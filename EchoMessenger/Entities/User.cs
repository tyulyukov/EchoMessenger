using System;

namespace EchoMessenger.Entities
{
    public class User
    {
        public String Name { get; set; }
        public String Description { get; set; }
        public String PasswordHash { get; set; }
        public String AvatarUrl { get; set; }
        public DateTime CreatedAt { get; set; }

        public User(String name, String password, String description = null, String avatarUrl = null)
        {
            CreatedAt = DateTime.Now;
            Name = name;
            Description = description;
            PasswordHash = Helpers.LogInManager.GetPasswordHash(password);

            if (avatarUrl != null)
                AvatarUrl = avatarUrl;
            else
                AvatarUrl = "https://firebasestorage.googleapis.com/v0/b/echo-c09f3.appspot.com/o/avatars%2Fdefault%20avatar.png?alt=media&token=3d11a976-b768-4895-acd5-cfb5fa6ba1b2";
        }

        public UserInfo GetUserInfo()
        {
            return new UserInfo() { Name = this.Name, PasswordHash = this.PasswordHash };
        }
    }
}
