using System;

namespace EchoMessenger.Entities
{
    public class Edit
    {
        public String _id { get; set; }
        public String content { get; set; }
        public DateTime date { get; set; }
        public DateTime dateLocal => date.ToLocalTime();
    }
}
