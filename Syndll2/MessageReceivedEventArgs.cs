using System;

namespace Syndll2
{
    public class MessageReceivedEventArgs : EventArgs
    {
        public PushNotification Notification { get; set; }
    }
}