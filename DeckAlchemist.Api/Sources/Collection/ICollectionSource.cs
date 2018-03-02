﻿using System;
using System.Collections.Generic;
using DeckAlchemist.Support.Objects.Collection;

namespace DeckAlchemist.Api.Sources.Collection
{
    public interface ICollectionSource
    {
        bool addCardToCollection(string uId, IEnumerable<string> cardName);
    }
}
