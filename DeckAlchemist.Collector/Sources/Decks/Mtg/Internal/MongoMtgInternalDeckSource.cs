﻿using System.Linq;
using System.Collections.Generic;
using DeckAlchemist.Collector.Objects.Decks;
using MongoDB.Driver;
using MongoDB.Bson;

namespace DeckAlchemist.Collector.Sources.Decks.Mtg.Internal
{
    public class MongoMtgInternalDeckSource : IMtgInternalDeckSource
    {
        const string MongoConnectionString = "mongodb://localhost:27017";
        const string MongoDatabase = "Decks";
        const string MongoCollection = "Mtg";

        readonly IMongoCollection<MongoMtgDeck> collection;
        readonly FilterDefinitionBuilder<MongoMtgDeck> _filter = Builders<MongoMtgDeck>.Filter;

        public MongoMtgInternalDeckSource()
        {
            var client = new MongoClient(MongoConnectionString);
            var database = client.GetDatabase(MongoDatabase);
            EnsureCollectionExists(database);
            collection = database.GetCollection<MongoMtgDeck>(MongoCollection);
        }


        public void UpdateAllDecks(IEnumerable<IMtgDeck> externalDecks)
        {
            var existingDecks = FindDecksByName(externalDecks.Select(deck => deck.Name).ToList());
            var plan = CreateDeckUpdatePlan(existingDecks, externalDecks);
            collection.BulkWrite(plan);
        }

        IDictionary<string, MongoMtgDeck> FindDecksByName(IEnumerable<string> names)
        {
            var decksByNameFilter = _filter.In("Name", names);
            var result = collection.Find(decksByNameFilter);
            return result.ToEnumerable().ToDictionary(deck => deck.Name);
        }

        IEnumerable<WriteModel<MongoMtgDeck>> CreateDeckUpdatePlan(IDictionary<string, MongoMtgDeck> internalDecks, IEnumerable<IMtgDeck> externalDecks)
        {
            var plan = new LinkedList<WriteModel<MongoMtgDeck>>();

            foreach(var externalDeck in externalDecks)
            {
                if(internalDecks.ContainsKey(externalDeck.Name))
                {
                    var internalDeck = internalDecks[externalDeck.Name];
                    if(DifferencesExist(internalDeck, externalDeck))
                    {
                        var mongoDeck = MongoMtgDeck.FromMtgDeck(externalDeck);
                        mongoDeck._id = internalDeck._id;
                        var replaceOneFilter = _filter.Eq("_id", mongoDeck._id);
                        var newPlan = new ReplaceOneModel<MongoMtgDeck>(
                            replaceOneFilter,
                            mongoDeck
                        );
                        plan.AddLast(newPlan);
                    }
                }
                else
                {
                    var newPlan = new InsertOneModel<MongoMtgDeck>(MongoMtgDeck.FromMtgDeck(externalDeck));
                    plan.AddLast(newPlan);
                }
            }

            return plan;
        }

        bool DifferencesExist(IMtgDeck deck1, IMtgDeck deck2)
        {
            //TODO: Determin proper differences
            return deck1.Name != deck2.Name ||
                        System.Math.Abs(deck1.Meta - deck2.Meta) > 0.001 ||
                        CardDifferencesExist(deck1.Cards, deck2.Cards);
        }

        bool CardDifferencesExist(IDictionary<string, IMtgDeckCard> cardSet1, IDictionary<string, IMtgDeckCard> cardSet2)
        {
            if(cardSet1 == null || cardSet2 == null)
            {
                if (cardSet1 == cardSet2) return false;
                return true;
            }

            foreach(var cardSet1KV in cardSet1)
            {
                if(cardSet2.ContainsKey(cardSet1KV.Key))
                {
                    if (cardSet2[cardSet1KV.Key].Count != cardSet1KV.Value.Count) return true;
                }
                else
                {
                    return true;
                }
            }

            return false;
        }

        void EnsureCollectionExists(IMongoDatabase database)
        {
            var filter = new BsonDocument("name", MongoCollection);
            //filter by collection name
            var collections = database.ListCollections(new ListCollectionsOptions { Filter = filter });
            //check for existence
            var exists = collections.Any();

            if (!exists)
                database.CreateCollection(MongoCollection);
        }
    }
}
