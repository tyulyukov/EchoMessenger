using System;
using System.Collections.Generic;
using System.Linq;

namespace EchoMessenger.Entities
{
    public class Chat
    {
        public UserInfo FromUser { get; set; }
        public UserInfo TargetUser { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<Message> Messages { get; set; }

        public Chat(UserInfo fromUser, UserInfo targetUser, List<Message> messages = null)
        {
            FromUser = fromUser;
            TargetUser = targetUser;
            CreatedAt = DateTime.Now;

            if (messages != null)
                Messages = messages;
            else
                Messages = new List<Message>();
        }

        public DateTime GetLastSentAt()
        {
            if (Messages.Count == 0)
                return CreatedAt;

            return Messages.OrderBy(m => m.SentAt).Last().SentAt;
        }

        public MessagesCollection GetMessagesCollection()
        {
            return new MessagesCollection(Messages);
        }
    }
}
