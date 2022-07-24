using System;

namespace EchoMessenger.Models
{
    public class UserInfo : IEquatable<UserInfo>
    {
        public String _id { get; set; }
        public String username { get; set; }
        public String description { get; set; }
        public String avatarUrl { get; set; }
        public String originalAvatarUrl { get; set; }
        public DateTime createdAt { get; set; }
        public DateTime createdAtLocal => createdAt.ToLocalTime();
        public DateTime lastOnlineAt { get; set; }
        public DateTime lastOnlineAtLocal => lastOnlineAt.ToLocalTime();

        public static bool operator ==(UserInfo? user1, UserInfo? user2)
        {
            if (user1 is null && user2 is null)
                return true;
            else if (user1 is null && user2 is not null)
                return false;
            else if (user1 is not null && user2 is null)
                return false;
            else
                return user1.Equals(user2);
        }

        public static bool operator !=(UserInfo? user1, UserInfo? user2) => !(user1 == user2);

        public bool Equals(UserInfo? other) => this._id == other?._id;

        public override bool Equals(object? obj)
        {
            if (obj is null)
                return false;
            else if (obj is UserInfo user)
                return user.Equals(this);
            else return false;
        }
    }
}
