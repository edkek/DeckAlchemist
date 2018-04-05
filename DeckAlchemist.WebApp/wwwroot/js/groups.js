﻿var groupsModel = {}
function getGroupsAndPopulate() {
        getAllUserGroups().then(function (result) {
            groupsModel = result;
            populateGroupsList(result)
        })
    };

    function populateGroupsList(groups) {
        var groupInfoTable = $("#groupInfoTable");
        groupInfoTable.empty();
        $.each(groups, function(index) {
            var groupInfo = groups[index];
            var members = groups[index].users;
                    var row = $("<div class='row' style='width:100%;'></div>")
                    var cell = $("<div style='width:100%'></div>")
                    var link = $("<a style='width: 100%;' data-toggle='collapse' href='#group"+index+"' role='button'>"+groupInfo.groupName+"</a>")
                    var list = $("<div class='collapse' id='group"+index+"'></div>")
                    $.each(members, function(index) {
                        var member = members[index]
                        var element = $("<a>"+member.userName+"</a>")
                        var loanButton = $("<button>Loan</button>")
                        element.click(function(e){
                            createNewUserMessageDialog(groupInfo, member)
                        })
                        loanButton.click(function(e) {
                            createNewLoanDialog(groupInfo, member)
                        })
                        list.append(element)
                        list.append(loanButton)
                    })
                    var newInviteLink = $("<a>+ New Member</a><br />")
                    newInviteLink.click(function(e) {
                        createNewGroupInviteDialog(groupInfo)
                    })
                    list.append(newInviteLink)


                    cell.append(link)
                    cell.append(list)
                    cell.append($('<hr />'))
                    row.append(cell)

                    groupInfoTable.append(row)

        })
    }

    function createNewUserMessageDialog(group, user) {
        var newMessageDialog = $('#newMessageDialog')
        var userTextBox = $('#message-user')
        var sendMessageBtn = $('#create-message-btn')

        userTextBox.text(user.userName)
        sendMessageBtn.click(function(e) {
            var subjectTextBox = $('#message-subject')
            var bodyTextBox = $('#message-body')

            var message = {
                "groupId": group.groupId,
                "subject": subjectTextBox.val(),
                "body": bodyTextBox.val(),
                "recipientId": user.userId
            }
            console.log(message)
            sendUserMessage(message).then(function() {
                newMessageDialog.modal("toggle")
                subjectTextBox.val("")
                bodyTextBox.val("")
                swal("Message Sent", "", "success");
            })

        })
        newMessageDialog.modal("toggle")

    }

    function createNewGroupInviteDialog(group) {

        var modal = $('#newGroupInviteDialog')
        var sendGroupInviteBtn = $('#create-invite-btn')
        sendGroupInviteBtn.click(function(e) {
            var userNameTextBox = $('#invite-user')
            var userName = userNameTextBox.val()
            var message = {
                "groupId": group.groupId,
                "subject": "Invite!",
                "body": "No Body :(",
                "recipientUserName": userName
            }
            sendGroupInvite(message).then(function() {
                modal.modal("toggle")
                swal("Invite Sent", userName+" was invited to "+group.groupName+"!", "success");
            }).catch(function(error) {
                
                swal("Invit Failed", "Couln't invite "+userName+" (user not found)", "error");
            })
        })

        modal.modal("toggle")

   }

   function createNewLoanDialog(group, user) {
        getOwnedCardsForUser(user.userId).then(function(collection){
            collection.userCollection = {}
            collection.userCollection.ownedCards = collection.ownedCards
            var sendButton = $('#create-loan-btn')
            sendButton.click(function(e) {
                var subjectTextBox = $('#loan-sibject')
                var bodyTextBox = $('#loan-body')
                var modal = $('#newLoanDialog')
                var subject = subjectTextBox.val()
                var body = bodyTextBox.val()
                var selectedCards = $('#collectionTable').bootstrapTable('getSelections')
                var requestedCardsAndAmounts = {}
                $.each(selectedCards, function(cardIndex) {
                    var card = selectedCards[cardIndex]
                    requestedCardsAndAmounts[card.name] = 1
                })
                var message = {
                    "groupId": group.groupId,
                    "subject": subject,
                    "body": body,
                    "requestedCardsAndAmounts": requestedCardsAndAmounts,
                    "recipientId": user.userId
                }
                sendLoanRequest(message).then(function() {
                    modal.modal("toggle")
                    swal("Loan Sent", "", "success");
                })
            })
            var format = buildTableFromCollection(collection)
            var table = $('#collectionTable')
            var modal = $('#newLoanDialog')
            table.bootstrapTable('destroy')
            table.bootstrapTable({
                clickToSelect: true,
            idField: 'id',
            columns: [{
                field: 'state',
                checkbox: true
            }, {
                field: 'available',
                title: 'Cards Available',
                align: 'center',
                halign: 'center'
            }, {
                field: 'totalAmount',
                title: 'Total Amount',
                align: 'center',
                halign: 'center'
            }, {
                field: 'name',
                title: 'Name',
                class: 'name-style',
                align: 'center',
                halign: 'center'
            }, {
                field: 'cmc',
                title: 'Converted Cost',
                align: 'center',
                halign: 'center'
            }, {
                field: 'manaCost',
                title: 'Full Cost',
                align: 'center',
                halign: 'center'
            }, {
                field: 'colors',
                title: 'Colors',
                align: 'center',
                halign: 'center'
            }, {
                field: 'power',
                title: 'Power',
                align: 'center',
                halign: 'center'
            }, {
                field: 'toughness',
                title: 'Toughness',
                align: 'center',
                halign: 'center'
            }, {
                field: 'type',
                title: 'Type',
                align: 'center',
                halign: 'center'
            }, {
                field: 'layout',
                title: 'Set',
                class: 'set-style',
                align: 'center',
                halign: 'center'
            }],
            data: format
            })

            modal.modal("toggle")
        }

        );

        //TODO
   }



$(document).ready(function () {
    $('#newLoanDialog').modal("toggle")
    $('#create-group-btn').click(function(){
        var groupName = $('#group-name').val();
        createGroup(groupName).then(function() {
            $('#newGroupDialog').modal('toggle')
            getGroupsAndPopulate()
        }).catch(function(error){
            console.log("Error")
        })
    })

    getGroupsAndPopulate()
});