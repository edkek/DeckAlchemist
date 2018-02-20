﻿using System.Collections.Generic;

namespace DeckAlchemist.Api.Objects.Mtg.Decks
{
    public class MtgDeck : IMtgDeck
    {
        public string Name { get; set; }
        public double Meta { get; set; }
        public string id { get; set; }
        public IDictionary<string, IMtgDeckCard> Cards { get; set; }

        public static MtgDeck FromMongo(MongoMtgDeck deck)
        {
            return new MtgDeck()
            {
                Name = deck.Name,
                Meta = deck.Meta,
                Cards = deck.Cards
            };
        }
    }
}
