using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace BlakeServerSide.Data
{
    public class Message {
        public DateTimeOffset TimeStamp { get; } = DateTimeOffset.Now;
        public string Sender { get; set; }
        public string Text { get; set; }
    }
    public class ChatService
    {
        public ObservableCollection<Message> Messages { get; } = new ObservableCollection<Message>();

        public void Send(string message, string sender = null) {
            if (string.IsNullOrEmpty(sender)) {
                sender = "Anonymous";
            }

            Messages.Add(new Message {
                Sender = sender,
                Text = message
            });
        }
    }
}
