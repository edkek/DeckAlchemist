﻿using System;
using System.Collections.Generic;
using System.Linq;
using DeckAlchemist.Api.Sources.Collection;
using DeckAlchemist.Api.Sources.Cards.Mtg;
using DeckAlchemist.Api.Sources.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DeckAlchemist.Api.Contracts;
using DeckAlchemist.Support.Objects.Collection;
using System.Net.Http;
using DeckAlchemist.Api.Utility;
using Newtonsoft.Json;
using DeckAlchemist.Support.Objects.Cards;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.Threading.Tasks;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace DeckAlchemist.Api.Controllers
{
    [Authorize(Policy = "Email")]
    [Route("api/collection")]
    public class CollectionsController : Controller
    {
        readonly ICollectionSource _collectionSource;
        readonly IMtgCardSource _cardSource;
        readonly IUserSource _userSource;

        public CollectionsController(ICollectionSource collectionSource, IMtgCardSource cardSource, IUserSource userSource)
        {
            _collectionSource = collectionSource;
            _cardSource = cardSource;
            _userSource = userSource;
        }
        
        [HttpGet]

        public CollectionModel GetCollection()
        {
            var uId = HttpContext.User.Id();
            var result = _collectionSource.GetCollection(uId);
            if (result == null || result.OwnedCards == null || result.BorrowedCards == null) return null;
            var uniqueCardNames = GetUniqueCardNames(result.OwnedCards.Keys, result.BorrowedCards.Keys);
            var cardInfo = GetCardInfo(uniqueCardNames);

            var newBorrowedCards = new Dictionary<string, IDictionary<string, IBorrowedCard>>();
            var userids = result.BorrowedCards.SelectMany(card => card.Value.Select(user => user.Key)).ToArray();
            var userNames = _userSource.GetUserNamesByUserIds(userids);
            foreach(var borrowedCards in result.BorrowedCards) {
                var cardName = borrowedCards.Key;
                var borrowed = borrowedCards.Value;
                foreach(var user in borrowed) {
                    var bCard = new BorrowedCardModel
                    {
                        AmountBorrowed = user.Value.AmountBorrowed,
                        CardId = user.Value.CardId,
                        Lender = user.Value.Lender,
                        LenderUserName = userNames[user.Value.Lender]
                    };
                    if (!newBorrowedCards.ContainsKey(cardName))
                        newBorrowedCards[cardName] = new Dictionary<string, IBorrowedCard>();

                    newBorrowedCards[cardName].Add(user.Value.Lender, bCard);
                }

            }
            var model = new CollectionModel
            {
                CardInfo = cardInfo,
                UserCollection = new Collection
                {
                    BorrowedCards = newBorrowedCards,
                    CollectionId = result.CollectionId,
                    OwnedCards = result.OwnedCards,
                    UserId = result.UserId
                }
            };
            return model;  
        } 

        [HttpPost]
        public OwnedCardsModel GetOthersOwnedCollection([FromBody] string otherUID)
        {
            var result = _collectionSource.GetCollection(otherUID);
            if (result == null || result.OwnedCards == null || result.BorrowedCards == null) return null;

            //Only lendable cards should be here
            var ownedCards = result.OwnedCards;
            var lendableOwnedCards = new Dictionary<string, IOwnedCard>(ownedCards.Where(card => card.Value.Lendable));
            var cardInfo = GetCardInfo(lendableOwnedCards.Keys);
            var model = new OwnedCardsModel
            {
                OwnedCards = lendableOwnedCards,
                CardInfo = cardInfo
            };
            return model;
        }


        [HttpGet("slim")]
        public CollectionModel GetCollectionSlim() 
        {
            var uId = HttpContext.User.Id();
            var result = _collectionSource.GetCollection(uId);
            if (result == null) return null;
            var model = new CollectionModel
            {
                UserCollection = result
            };
            return model;
        }

        IEnumerable<string> GetUniqueCardNames(IEnumerable<string> owned, IEnumerable<string> borrowed)
        {
            var cardNames = new HashSet<string>();
            if(owned != null) foreach (var card in owned)
                cardNames.Add(card);
            if(borrowed != null) foreach (var card in borrowed)
                cardNames.Add(card);

            return cardNames;
        }

        IDictionary<string, IMtgCard> GetCardInfo(IEnumerable<string> cardNames) 
        {
            var cardsResult = _cardSource.GetCardsByNames(cardNames.ToArray());
            var index = new Dictionary<string, IMtgCard>();
            foreach(var card in cardsResult) 
            {
                index.Add(card.Name, card);    
            }
            return index;
        }

        //add one or many cards
        [HttpPut("cards")]
        public IActionResult AddCardToCollection([FromBody]IList<string> cardnames)
        {
            try
            {
                var uId = Utility.UserInfo.Id(HttpContext.User);
                var userEmail = Utility.UserInfo.Email(HttpContext.User);
                var cardExists = _cardSource.CheckExistance(cardnames);
                if (cardExists.Any()) return StatusCode(400);
                var result = _collectionSource.AddCardToCollection(uId, cardnames);
                if (result) return StatusCode(200);
                return StatusCode(500);
            }
            catch(Exception e)
            {
                return StatusCode(500);
            }
        }

        //remove one or many cards
        [HttpDelete("cards")]
        public IActionResult RemoveCardsFromCollection([FromBody]string[] cardnames)
        {
            try
            {
                var uId = Utility.UserInfo.Id(HttpContext.User);
                var userEmail = Utility.UserInfo.Email(HttpContext.User);
                var cardExists = _cardSource.CheckExistance(cardnames);
                if (cardExists.Any()) return StatusCode(401);
                var result = _collectionSource.RemoveCardFromCollection(uId, cardnames);
                if (result) return StatusCode(200);
                return StatusCode(500);
            }
            catch (Exception)
            {
                return StatusCode(500);
            }
        }
        //lend one ore menay cards
        [HttpPost("lend")]
        public IActionResult LendcardsTo([FromBody] LendContract lendContract){
            try
            {
                var uId = Utility.UserInfo.Id(HttpContext.User);
                var userEmail = Utility.UserInfo.Email(HttpContext.User);
                var reciverExists = _userSource.UserExists(lendContract.Lender);
                if (!reciverExists) return StatusCode(401);
                var markAsLent = _collectionSource.MarkCardAsLent(uId, lendContract.Lendee, lendContract.CardsAndAmounts);
                var uIdOfRevicer = lendContract.Lendee;
                var reciveCard = _collectionSource.AddCardAsLent(uId, lendContract.Lendee, lendContract.CardsAndAmounts);
                if (markAsLent && reciveCard) return StatusCode(200);
                return StatusCode(500);

            }
            catch (Exception)
            {
                return StatusCode(500);
            }
        }

        [HttpPost("mark")]
        public IActionResult MarkCardsAsLendable([FromBody] IEnumerable<LendableContract> cardNames)
        {
            var userId = HttpContext.User.Id();
            var lendableDictionary = new Dictionary<string, bool>();
            foreach (var lendable in cardNames)
                lendableDictionary[lendable.CardName] = lendable.Lenable;
            if (_collectionSource.MarkCardsAsLendable(userId, lendableDictionary))
                return StatusCode(200);
            return StatusCode(500);
        }

        [HttpPost("csv")]
        public IEnumerable<string> AddCardsFromCsv()
        {
            var csv = Request.Form.Files.FirstOrDefault();
            //CSV Must be less than 5MB
            if (csv == null) return null;
            if(csv.Length > 5242880) {
                return null;
            }
            var uId = HttpContext.User.Id();
            var tempFile = CreateTempFileAndAcceptUpload(csv.OpenReadStream());
            var entries = GetCsvEntries(tempFile);
            var toDict = new Dictionary<string, int>();
            foreach(var entry in entries) {
                if (toDict.ContainsKey(entry.CardName))
                    toDict[entry.CardName] += entry.Amount;
                else
                    toDict.Add(entry.CardName, entry.Amount);
            }
            var notExistingCards = _cardSource.CheckExistance(toDict.Keys.ToList());
            if (notExistingCards.Any()) foreach (var card in notExistingCards) toDict.Remove(card);
            _collectionSource.AddCardToCollection(uId, toDict);
            
           return notExistingCards;     
        }

        string CreateTempFileAndAcceptUpload(Stream upload)
        {
            var tempFilePath = Path.GetTempFileName();

            using(var writer = new FileStream(tempFilePath, FileMode.OpenOrCreate))
            {
                upload.CopyTo(writer);
                writer.Flush();
            }
                
            //Streams stored in local memory, attempt to release
            upload.Close();
            upload.Dispose();
            return tempFilePath;
        }

        IEnumerable<CollectionCsvEntry> GetCsvEntries(string path)
        {
            IEnumerable<CollectionCsvEntry> entries = null;
            using (var csvReader = new CsvHelper.CsvReader(new StreamReader(new FileStream(path, FileMode.Open))))
            {
                entries = csvReader.GetRecords<CollectionCsvEntry>().ToList();
            }
            System.IO.File.Delete(path);
            return entries;
        }


    }
}
