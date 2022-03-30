using System;
using System.Collections.Generic;

namespace EchoMessenger.Entities
{
    public class Message
    {
        public String Sender { get; set; }
        public String Receiver { get; set; }
        public String Text { get; set; }
        public DateTime SentAt { get; set; }
        public bool IsEdited { get; set; }
        public bool HaveSeen { get; set; }
        public List<Attachment> Attachments { get; set; } 

        public Message(string sender, string receiver, string text, DateTime sentAt, List<Attachment> attachments = null)
        {
            Sender = sender;
            Receiver = receiver;
            Text = text;
            SentAt = sentAt;
            IsEdited = false;
            HaveSeen = false;

            if (attachments != null)
                Attachments = attachments;
            else
                Attachments = new List<Attachment>();
        }
    }
}
