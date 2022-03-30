using System;

namespace EchoMessenger.Entities
{
    public class User
    {
        public String Name { get; set; }
        public String PasswordHash { get; set; }
        public String AvatarUrl { get; set; }
        public DateTime CreatedAt { get; set; }

        public User(String name, String password, String avatarUrl = null)
        {
            CreatedAt = DateTime.Now;
            Name = name;
            PasswordHash = Helpers.LogInManager.GetPasswordHash(password);

            if (avatarUrl != null)
                AvatarUrl = avatarUrl;
            else
                AvatarUrl = "https://upload.wikimedia.org/wikipedia/commons/f/f4/User_Avatar_2.png?20170128014309";
        }

        public UserInfo GetUserInfo()
        {
            return new UserInfo() { Name = this.Name, PasswordHash = this.PasswordHash };
        }
    }
}
