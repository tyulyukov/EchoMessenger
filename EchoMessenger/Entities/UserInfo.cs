using System;

namespace EchoMessenger.Entities
{
    public class UserInfo
    {
        public String _id { get; set; }
        public String username { get; set; }
        public String description { get; set; }
        public String avatarUrl { get; set; }
        public String originalAvatarUrl { get; set; }
        public DateTime createdAt { get; set; }
    }
}
