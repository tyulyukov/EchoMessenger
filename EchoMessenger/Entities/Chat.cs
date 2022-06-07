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
        public List<Message> messages { get; set; }

        public DateTime GetLastSentAt()
        {
            if (messages == null || messages.Count == 0)
                return createdAt;

            return messages.OrderBy(m => m.sentAt).Last().sentAt;
        }
    }
}
