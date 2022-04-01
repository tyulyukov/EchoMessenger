using System;
using System.Collections.Generic;

namespace EchoMessenger.Entities
{
    public class Message
    {
        public User Sender { get; set; }
        public User Receiver { get; set; }
        public String Text { get; set; }
        public Message RepliedOn { get; set; }
        public DateTime SentAt { get; set; }
        public bool IsEdited { get; set; }
        public bool HaveSeen { get; set; }
        public List<Attachment> Attachments { get; set; }

        public Message(User sender, User receiver, string text, DateTime sentAt, Message repliedOn = null, List<Attachment> attachments = null)
        {
            Sender = sender;
            Receiver = receiver;
            Text = text;
            SentAt = sentAt;
            RepliedOn = repliedOn;
            IsEdited = false;
            HaveSeen = false;

            if (attachments != null)
                Attachments = attachments;
            else
                Attachments = new List<Attachment>();
        }
    }
}
