using System;
using System.Threading;

namespace EchoMessenger.Helpers
{
    public class TypeAssistant
    {
        public event EventHandler Idled = delegate { };
        public int WaitingMilliSeconds { get; set; }
        
        private Timer waitingTimer;

        public TypeAssistant(int waitingMilliSeconds = 600)
        {
            WaitingMilliSeconds = waitingMilliSeconds;
            waitingTimer = new Timer(p =>
            {
                Idled(this, EventArgs.Empty);
            });
        }
        public void TextChanged()
        {
            waitingTimer.Change(WaitingMilliSeconds, Timeout.Infinite);
        }
    }
}
