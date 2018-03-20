﻿using System.Collections.Generic;

namespace DeckAlchemist.Support.Objects.Messages
{
    public class LoanRequestMessage : IMessage
    {
        public string Type { get => "Loan"; set { }}
        public string MessageId { get; set; }
        public bool UnRead { get; set; }
        public string SenderId { get; set; }
        public string GroupId { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public bool Accepted { get; set; }
        public IEnumerable<string> RequestedRecipientCardIds { get; set; }
        public string RecipientId { get; set; }
    }
}
