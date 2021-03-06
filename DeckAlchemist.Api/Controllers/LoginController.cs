using System;
using System.Collections.Generic;
using System.Security.Claims;
using DeckAlchemist.Api.Utility;
using DeckAlchemist.Api.Sources.Collection;
using DeckAlchemist.Api.Sources.Messages;
using DeckAlchemist.Api.Sources.User;
using DeckAlchemist.Support.Objects.Collection;
using DeckAlchemist.Support.Objects.Messages;
using DeckAlchemist.Support.Objects.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace DeckAlchemist.Api.Controllers
{
    [Authorize(Policy = "Email")]
    [Route("api/login")]
    public class LoginController : Controller
    {
        readonly ICollectionSource _collectionSource;
        readonly IUserSource _userSource;
        readonly IMessageSource _messageSource;

        public LoginController(ICollectionSource collectionSource, IUserSource userSource, IMessageSource messageSource)
        {
            _collectionSource = collectionSource;
            _userSource = userSource;
            _messageSource = messageSource;
        }

        [HttpGet("")]
        public void Login()
        {
            var userInfo = HttpContext.User;
            //Check to see if the user is created (first time login)
            CreateUserIfNotExist(userInfo);
            //Check to see if the user's collection is create
            CreateCollectionIfNotExist(userInfo);
            //Create the mailbox
            CreateMailboxIfNotExist(userInfo);
        }

        void CreateUserIfNotExist(ClaimsPrincipal user)
        {
            var userId = user.Id();
            if (!_userSource.UserExists(userId))
            {
                var email = user.Email();
                var newUser = new User
                {
                    UserId = userId,
                    Email = email,
                    UserName = email,
                    Groups = new List<string>(),
                    Decks = new List<string>()
                };
                _userSource.Create(newUser);
            }
        }

        void CreateCollectionIfNotExist(ClaimsPrincipal userInfo)
        {
            var userId = userInfo.Id();
            if(!_collectionSource.ExistsForUser(userId))
            {
                var user = _userSource.Get(userId);
                var collection = new Collection
                {
                    UserId = userId,
                    CollectionId = Guid.NewGuid().ToString(),
                    BorrowedCards = new Dictionary<string, IDictionary<string,IBorrowedCard>>(),
                    OwnedCards = new Dictionary<string, IOwnedCard>()
                };
                _collectionSource.Create(collection);
                user.CollectionId = collection.CollectionId;
                _userSource.Update(user);
            }
        }

        void CreateMailboxIfNotExist(ClaimsPrincipal userInfo)
        {
            var userId = userInfo.Id();
            if(!_messageSource.ExistsForUser(userId))
            {
                var user = _userSource.Get(userId);
                var box = new MessageBox
                {
                    UserId = userId,
                    Messages = new Dictionary<string, IMessage>()
                };
                _messageSource.Create(box);
            }
        }


    }
}
