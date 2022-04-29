using Firebase.Database;
using System;
using System.Collections.Generic;

namespace EchoMessenger.Entities
{
    public class Chat
    {
        public readonly FirebaseObject<User> TargetUser;
        public IReadOnlyList<FirebaseObject<Message>>? Messages;

        public Chat(FirebaseObject<User> targetUser, List<FirebaseObject<Message>>? messages)
        {
            TargetUser = targetUser;
            Messages = messages;
        }
    }
}
