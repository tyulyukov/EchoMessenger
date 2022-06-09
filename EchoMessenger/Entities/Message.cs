using System;
using System.Collections.Generic;

namespace EchoMessenger.Entities
{
    public class Message
    {
        public String _id { get; set; }
        public UserInfo sender { get; set; }
        public String content { get; set; }
        public List<Edit> edits { get; set; }
        public bool haveSeen { get; set; }
        public List<Attachment> attachments { get; set; }
        public DateTime sentAt { get; set; }
        public Message repliedOn { get; set; }
        public Chat chat { get; set; }
    }
}
