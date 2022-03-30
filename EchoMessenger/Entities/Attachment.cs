using System;

namespace EchoMessenger.Entities
{
    public class Attachment
    {
        public String FileUrl { get; set; }
        public String FileName { get; set; }
        public AttachmentType Type { get; set; }
    }

    public enum AttachmentType 
    { 
        Photo,
        File,
    }
}
