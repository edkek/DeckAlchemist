﻿using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;


namespace DeckAlchemist.Support.Objects.Collection
{
    public class MongoCollection : ICollection
    {
        public static MongoCollection FromCollection(ICollection collec)
        {
            return new MongoCollection
            {
                BorrowedCards = collec.BorrowedCards,
                CollectionId = collec.CollectionId,
                OwnedCards = collec.OwnedCards,
                UserId = collec.UserId
            };
        }
        
        [BsonId]
        public ObjectId _id { get; set; }
        public string UserId { get; set; }
        public string CollectionId { get; set; }
        public IDictionary<string, IOwnedCard> OwnedCards { get; set; }
        public IDictionary<string, IBorrowedCard> BorrowedCards { get; set; }
    }
}
