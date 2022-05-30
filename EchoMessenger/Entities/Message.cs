using EchoMessenger.Helpers;
using System;
using System.Collections.Generic;

namespace EchoMessenger.Entities
{
    public class Message
    {
        public UserInfo Sender { get; set; }
        public String Text { get; set; }
        public Message RepliedOn { get; set; }
        public DateTime SentAt { get; set; }
        public bool IsEdited { get; set; }
        public bool HaveSeen { get; set; }
        public List<Attachment> Attachments { get; set; }

        public Message(UserInfo sender, string text, Message repliedOn = null, List<Attachment> attachments = null)
        {
            Sender = sender;

            Text = text;
            SentAt = DateTime.Now;
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
