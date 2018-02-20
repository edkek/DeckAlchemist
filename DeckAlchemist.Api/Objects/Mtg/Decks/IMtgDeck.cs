﻿using System.Collections.Generic;
using System.Numerics;

namespace DeckAlchemist.Api.Objects.Mtg.Decks
{
    public interface IMtgDeck
    {
        string Name { get; set; }
        double Meta { get; set; }
        string id { get; set; }
        IDictionary<string, IMtgDeckCard> Cards { get; set; }
        float CompareDecks(IMtgDeck other);
    }
}