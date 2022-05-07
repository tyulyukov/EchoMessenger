using System;
using System.Collections.Generic;
using System.Linq;

namespace EchoMessenger.Entities
{
    public class MessagesCollection
    {
        public IReadOnlyList<Message> Messages => messages;

        private List<Message> messages;
        private List<Message> remainingLoadedMessages;
        
        public MessagesCollection(IEnumerable<Message> messages)
        {
            this.messages = messages.OrderBy(m => m.SentAt).ToList();
            this.remainingLoadedMessages = messages.OrderByDescending(m => m.SentAt).ToList();
        }

        public IEnumerable<Message> Load(int count)
        {
            List<Message> range;

            if (remainingLoadedMessages.Count < count)
            {
                range = remainingLoadedMessages;
                remainingLoadedMessages = new List<Message>();
            }
            else
            {
                range = remainingLoadedMessages.GetRange(0, count);
                remainingLoadedMessages.RemoveRange(0, count);
            }
            
            return range;
        }
    }
}
