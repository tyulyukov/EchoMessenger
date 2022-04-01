using System;

namespace EchoMessenger.Entities
{
    public class Attachment
    {
        public String FileUrl { get; set; }
        public String FileName { get; set; }
        public AttachmentType Type { get; set; }

        public Attachment(string fileUrl, string fileName, AttachmentType type)
        {
            FileUrl = fileUrl;
            FileName = fileName;
            Type = type;
        }

    }

    public enum AttachmentType 
    { 
        Photo,
        File,
    }
}
