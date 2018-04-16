﻿promiseQueue = [];
var apiEndPoint = "http://" + window.location.hostname + ":5000/";
/*
    * Fetches a resource from an endpoint that expects authorization.
    * Achieves this by automatically appending the firebase id token
    * for the currently signed in user to the request header. Returns a promise
    * .then(function(result)): result=response
    * .catch(function(error)): error = either firebase error or fetch error
    * Params: [url] = authenticated endpoint, [fetchProps] = optional properties to use with the fetch call
    * For more information on how to use fetch: "https://developer.mozilla.org/en-US/docs/Web/API/Fetch_API/Using_Fetch"
    */
function fetchWithAuth(url, fetchProps = {}) {
    return new Promise(function (resolve, reject) {
        try {
            var u = firebase.auth().currentUser;

            var toRum = function doAuth(currentUser) {
                try {
                    currentUser.getIdToken(true).then(function (idToken) {
                        if (!fetchProps.headers) fetchProps.headers = {};
                        fetchProps.headers['Authorization'] = "Bearer " + idToken;
                        fetch(url, fetchProps).then(function (result) {
                            resolve(result);
                        }).catch(function (error) {
                            reject(error);
                        });
                    });
                } catch (error) {
                    reject(error);
                }
            };

            if (u == null) {
                promiseQueue.push(toRum);
            } else {
                toRum(u);
            }


        } catch (error) {
            reject(error);
        }
    });
}

function buildTableFromDeck(deck) {
    var cardInfo = deck.cardInfo;
    var uDeck = deck.userDeck.cardsAndAmounts;

    var result = [];

    var id = 1;
    for (var name in uDeck) {
        if (uDeck.hasOwnProperty(name)) {
            /*
            int
             */
            var c = uDeck[name];

            /*
            cmc
            colors
            imageName
            layout
            legality
            manaCost
            name
            power
            subTypes
            text
            toughness
            type
            types
            _id
             */
            var info = cardInfo[name];

            var newCard = Object.assign({
                amount: c,
                id: id
            }, info);

            result.push(newCard);
            id++;
        }
    }

    return result;
}

function buildBorrowedTableFromCollection(collection) {
    var cardInfo = collection.cardInfo;
    var borrowed = collection.userCollection.borrowedCards;

    var result = [];

    var id = 1;
    var promiseHell = [];
    for (var name in borrowed) {
        if (borrowed.hasOwnProperty(name)) {
            /*
            cardId
            lender
            amountBorrowed
            lenderUserName
             */
            var c = borrowed[name][Object.keys(borrowed[name])[0]];

            /*
            cmc
            colors
            imageName
            layout
            legality
            manaCost
            name
            power
            subTypes
            text
            toughness
            type
            types
            _id
             */
            var info = cardInfo[c.cardId];

            var newCard = Object.assign({
                amountBorrowed: c.amountBorrowed,
                lender: c.lenderUserName,
                lenderId: c.lender,
                id: id
            }, info);

            result.push(newCard);
            id++;

            //promiseHell.push(
            //getUserName(c.lender).then(function (value) { 
            //newCard.lender = value;
            //})
            //);
        }
    }

    return new Promise(function (resolve, reject) {
        Promise.all(promiseHell).then(function (value) {
            resolve(result);
        }).catch(function (reason) {
            resolve(result);
        })
    });
}

function buildTableFromCollection(collection) {
    var cardInfo = collection.cardInfo;
    var owned = collection.userCollection.ownedCards;
    var borrowed = collection.userCollection.borrowedCards;

    var result = [];

    var id = 1;
    for (var name in owned) {
        if (owned.hasOwnProperty(name)) {
            /*
            available
            cardId
            inDecks
            lentTo
            totalAmount
            lendable
             */
            var c = owned[name];

            /*
            cmc
            colors
            imageName
            layout
            legality
            manaCost
            name
            power
            subTypes
            text
            toughness
            type
            types
            _id
             */
            var info = cardInfo[c.cardId];

            var newCard = Object.assign({
                lendable: c.lendable,
                available: c.available,
                inDecks: c.inDecks,
                lentTo: c.lentTo,
                totalAmount: c.totalAmount,
                id: id
            }, info);

            result.push(newCard);
            id++;
        }
    }

    return result;
}

function formWithAuth(aUrl, aData, aType) {
    return new Promise(function (resolve, reject) {
        try {
            var u = firebase.auth().currentUser;

            var toRun = function (currentUser) {
                try {
                    currentUser.getIdToken(true).then(function (idToken) {
                        $.ajax({
                            type: aType,
                            url: aUrl,
                            beforeSend: function (request) {
                                request.setRequestHeader("Authorization", "Bearer " + idToken);
                            },
                            data: aData,
                            contentType: false,
                            processData: false,
                            success: function (data) {
                                resolve(data);
                            },
                            error: function (xhr, textStatus, error) {
                                console.log(xhr.statusText);
                                console.log(textStatus);
                                console.log(error);
                                reject(xhr);
                            }
                        });
                    })
                } catch (error) {
                    reject(error);
                }
            };

            if (u == null) {
                promiseQueue.push(toRun);
            } else {
                toRun(u);
            }
        } catch (error) {
            reject(error);
        }
    });
}

function ajaxWithAuth(aUrl, aData, aType) {
    return new Promise(function (resolve, reject) {
        try {
            var u = firebase.auth().currentUser;

            var toRun = function (currentUser) {
                try {
                    currentUser.getIdToken(true).then(function (idToken) {
                        $.ajax({
                            type: aType,
                            contentType: "application/json",
                            url: aUrl,
                            beforeSend: function (request) {
                                request.setRequestHeader("Authorization", "Bearer " + idToken);
                            },
                            data: JSON.stringify(aData),
                            success: function (data) {
                                resolve(data);
                            },
                            error: function (xhr, textStatus, error) {
                                console.log(xhr.statusText);
                                console.log(textStatus);
                                console.log(error);
                                reject(xhr);
                            },
                            traditional: true
                        });
                    })
                } catch (error) {
                    reject(error);
                }
            };

            if (u == null) {
                promiseQueue.push(toRun);
            } else {
                toRun(u);
            }
        } catch (error) {
            reject(error);
        }
    });
}

function postWithAuth(aUrl, aData) {
    return ajaxWithAuth(aUrl, aData, "POST");
}

function putWithAuth(aUrl, aData) {
    return ajaxWithAuth(aUrl, aData, "PUT");
}

function deleteWithAuth(aUrl, aData) {
    return ajaxWithAuth(aUrl, aData, "DELETE");
}

function getCardImage(cardName) {
    const scryImageSearchURI = "https://api.scryfall.com/cards/named?exact=";
    return new Promise(function (resolve, reject) {
        try {
            fetch(scryImageSearchURI + cardName).then(function (result) {
                return result.json()
            }).then(function (json) {
                if (json.image_uris == null) {
                    resolve(json.card_faces[0].image_uris);
                } else {
                    resolve(json.image_uris)
                }
            }).catch(function (error) {
                reject(error)
            })
        } catch (error) {
            reject(error)
        }
    });
}

function getGroups() {
    return new Promise(function (resolve, reject) {
        fetchWithAuth(apiEndPoint + "api/user").then(function (result) {
            return result.json();
        }).then(function (json) {
            resolve(json.groups)
        }).catch(function (error) {
            reject(error)
        });
    })
}

function getAllUserGroups() {
    return new Promise(function (resolve, reject) {
        fetchWithAuth(apiEndPoint + "api/group/all").then(function (result) {
            return result.json();
        }).then(function (json) {
            resolve(json)
        }).catch(function (error) {
            reject(error);
        })
    })
}

function getGroupInfo(groupId) {
    return new Promise(function (resolve, reject) {
        fetchWithAuth(apiEndPoint + "api/group/" + groupId).then(function (result) {
            return result.json();
        }).then(function (json) {
            resolve(json);
        }).catch(function (error) {
            reject(error)
        })
    });
}

function getUserNamesByUserIds(userIds) {
    return new Promise(function (resolve, reject) {
        fetchWithAuth(apiEndPoint + "api/user/names",
            {
                method: "POST",
                body: JSON.stringify(userIds),
                headers: {
                    'content-type': "application/json"
                }
            }).then(function (result) {
                return result.json();
            }).then(function (json) {
                resolve(json);
            }).catch(function (error) {
                reject(error)
            })
    });
}

function createGroup(groupName) {
    return new Promise(function (resolve, reject) {
        fetchWithAuth(apiEndPoint + "api/group/" + groupName + "/create", { method: "POST" }).then(function () {
            resolve()
        }).catch(function (error) {
            reject(error)
        })
    });
}

function sendUserMessage(message) {
    return new Promise(function (resolve, reject) {
        fetchWithAuth(apiEndPoint + "api/message/send/user",
            {
                method: "POST",
                body: JSON.stringify(message),
                headers: {
                    'content-type': "application/json"
                }
            }).then(function () {
                resolve()
            }).catch(function (error) {
                reject(error)
            })
    })
}

function sendGroupInvite(message) {
    return new Promise(function (resolve, reject) {
        fetchWithAuth(apiEndPoint + "api/message/send/invite",
            {
                method: "POST",
                body: JSON.stringify(message),
                headers: {
                    'content-type': "application/json"
                }
            }).then(function (result) {
                if(result.status == 404) reject("user not found")
                else resolve()
            }).catch(function (error) {
                reject(error)
            })
    })
}

function deleteMessage(messageId) {
    return new Promise(function (resolve, reject) {
        fetchWithAuth(apiEndPoint + "api/message/delete/" + messageId, {
            method: "DELETE"
        }).then(function (result) {
            resolve()
        }).catch(function (error) {
            reject(error)
        })
    })
}

function getOwnedCardsForUser(userId) {
    return new Promise(function (resolve, reject) {
        fetchWithAuth(apiEndPoint + "api/collection", {
            method: "POST",
            body: JSON.stringify(userId),
            headers: {
                'content-type': "application/json"
            }
        }).then(function (result) {
            return result.json();
        }).then(function (json) {
            resolve(json)
        }).catch(function (error) {
            reject(error);
        })

    })

}

function acceptLoanRequest(messageId) {
    return new Promise(function (resolve, reject) {
        fetchWithAuth(apiEndPoint + "api/message/accept/loan", {
            method: "POST",
            body: JSON.stringify(messageId),
            headers: {
                'content-type': "application/json"
            }
        }).then(function (response) {
            resolve()
        }).catch(function (error) {
            reject(error)
        })
    })
}

function acceptGroupInvite(messageId) {
    return new Promise(function (resolve, reject) {
        fetchWithAuth(apiEndPoint + "api/message/accept/invite", {
            method: "POST",
            body: JSON.stringify(messageId),
            headers: {
                'content-type': "application/json"
            }
        }).then(function (response) {
            resolve();
        }).catch(function (error) {
            reject(error)
        })
    })
}

function sendLoanRequest(message) {
    return new Promise(function (resolve, reject) {
        fetchWithAuth(apiEndPoint + "api/message/send/loan", {
            method: "POST",
            body: JSON.stringify(message),
            headers: {
                'content-type': "application/json"
            }
        }).then(function () {
            resolve();
        }).catch(function (error) {
            reject(error);
        })
    })
}

function getMessages() {
    return new Promise(function (resolve, reject) {
        fetchWithAuth(apiEndPoint + "api/message/all").then(function (response) {
            return response.json();
        }).then(function (json) {
            resolve(json);
        }).catch(function (error) {
            reject(error);
        })
    });
}


function getUserName(userId) {
    return new Promise(function (resolve, reject) {
        fetchWithAuth(apiEndPoint + "api/user/name/" + userId).then(function (result) {
            return result.text();
        }).then(function (json) {
            resolve(json);
        }).catch(function (error) {
            reject(error);
        })
    })
}

function forgotPassword(email) {
    firebase.auth().sendPasswordResetEmail(email).then(function (result) {
        swal("sent")
    })
}

function authorizeOrLogin() {
    //if(firebase.auth().currentUser == null) window.location = "/"
}

$(document).ready(function () {
    "use strict";
    firebase.auth().onAuthStateChanged(function (user) {
        if (user) {
            //Flush
            promiseQueue.forEach(function (f) {
                f(user);
            })
        }
    });
});
