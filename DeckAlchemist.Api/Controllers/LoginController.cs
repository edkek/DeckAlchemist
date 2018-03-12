﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using DeckAlchemist.Api.Auth;
using DeckAlchemist.Api.Sources.Collection;
using DeckAlchemist.Api.Sources.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace DeckAlchemist.Api.Controllers
{
    [Authorize(Policy = "Email")]
    [Route("api/login")]
    public class LoginController : Controller
    {
        ICollectionSource _collectionSource;
        IUserSource _userSource;

        [HttpGet]
        public string Login()
        {
            var userInfo = HttpContext.User;

            //Check to see if the user is created (first time login)
            CreateUserIfNotExist(userInfo);


            //Check to see if the user's collection is create
            CreateCollectionIfNotExist(userInfo);
        }

        void CreateUserIfNotExist(ClaimsPrincipal user)
        {
            var userId = UserInfo.Id(userInfo);
            if (!_userSource.UserExists(userId))
            {
                var email = UserInfo.Email(userInfo);
                _userSource.CreateUser(userId, email);
            }
        }

        void CreateCollectionIfNotExist(ClaimsPrincipal user)
        {
            var userId = UserInfo.Id(user);
            if(!_collectionSource.ExistsForUser(userId))
            {
                var user = _userSource.GetUser(userId);
                var collectionId = _collectionSource.CreateCollection(userId);
                user.CollectionId = collectionId;
                _userSource.Update(user);
            }
        }

    }
}
