﻿using System.Collections.Generic;
using System.Linq;
using DeckAlchemist.Api.Sources.Cards.Mtg;
using DeckAlchemist.Support.Objects.Cards;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace DeckAlchemist.Api.Controllers
{
    [Route("api/card")]
    public class CardsController : Controller
    {
        readonly IMtgCardSource _source;

        public CardsController(IMtgCardSource source)
        {
            _source = source;
        }

        [HttpGet("search/{byName}")]
        public IEnumerable<IMtgCard> Search(string byName)
        {
            return _source.SearchByName(byName).Select(card => MtgCard.FromMtg(card));
        }


        [HttpPost("names")]
        public IEnumerable<IMtgCard> GetCardsByName([FromBody] string[] names)
        {
            var result = _source.GetCardsByNames(names).Select(MtgCard.FromMtg);
            return result;
        }

    }
}
