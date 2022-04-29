using System;
using System.Collections.Generic;
using System.Linq;

namespace EchoMessenger.Entities
{
    public class Chat
    {
        public User FromUser { get; set; }
        public User TargetUser { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<Message> Messages { get; set; }
        public DateTime LastDateTime { 
            get 
            {
                if (Messages.Count == 0)
                    return CreatedAt;

                return Messages.OrderBy(m => m.SentAt).Last().SentAt;
            } 
        }

        public Chat(User fromUser, User targetUser, List<Message> messages = null)
        {
            FromUser = fromUser;
            TargetUser = targetUser;
            CreatedAt = DateTime.Now;

            if (messages != null)
                Messages = messages;
            else
                Messages = new List<Message>();
        }
    }
}
