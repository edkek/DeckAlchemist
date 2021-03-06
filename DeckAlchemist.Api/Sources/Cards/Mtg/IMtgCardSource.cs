﻿using System.Collections.Generic;
using DeckAlchemist.Support.Objects.Cards;

namespace DeckAlchemist.Api.Sources.Cards.Mtg
{
    public interface IMtgCardSource
    {
        IEnumerable<IMtgCard> SearchByName(string byName);
        IEnumerable<IMtgCard> GetCardsByNames(params string[] names);
        IEnumerable<string> CheckExistance(IList<string> cardNames);
    }
}
