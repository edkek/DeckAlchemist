﻿using System.Collections.Generic;

namespace DeckAlchemist.Support.Objects.User
{
    public class User : IUser
    {
        public string UserId { get; set; }
        public string Email { get; set; }
        public string UserName { get; set; }
        public string CollectionId { get; set; }
        public List<string> Groups { get; set; }
        public List<string> Decks { get; set; }
    }
}
