using System;
using System.Collections.Generic;
using System.Linq;

namespace EchoMessenger.Entities
{
    public class Chat
    {
        public String _id { get; set; }
        public UserInfo sender { get; set; }
        public UserInfo receiver { get; set; }
        public DateTime createdAt { get; set; }
        public DateTime createdAtLocal => createdAt.ToLocalTime();
        public List<Message> messages { get; set; }
        public int unreadMessagesCount { get; set; }

        public DateTime GetLastSentAt()
        {
            if (messages == null || messages.Count == 0)
                return createdAtLocal;

            return messages.OrderBy(m => m.sentAtLocal).Last().sentAtLocal;
        }
    }
}
