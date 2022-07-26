using System;
using System.Collections.Generic;

namespace EchoMessenger.Models
{
    public class Message : IEquatable<Message>
    {
        public String _id { get; set; }
        public UserInfo sender { get; set; }
        public String content { get; set; }
        public List<Edit> edits { get; set; }
        public bool haveSeen { get; set; }
        public List<Attachment> attachments { get; set; }
        public DateTime sentAt { get; set; }
        public DateTime sentAtLocal => sentAt.ToLocalTime();
        public DateTime editedAt { get; set; }
        public DateTime editedAtLocal => editedAt.ToLocalTime();
        public Message repliedOn { get; set; }
        public Chat chat { get; set; }

        public static bool operator ==(Message? message1, Message? message2)
        {
            if (message1 is null && message2 is null)
                return true;
            else if (message1 is null && message2 is not null)
                return false;
            else if (message1 is not null && message2 is null)
                return false;
            else
                return message1.Equals(message2);
        }

        public static bool operator !=(Message? message1, Message? message2) => !(message1 == message2);

        public bool Equals(Message? other) => this._id == other?._id;

        public override bool Equals(object? obj)
        {
            if (obj is null)
                return false;
            else if (obj is Message message)
                return message.Equals(this);
            else return false;
        }
    }
}
