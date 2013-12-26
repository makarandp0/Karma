
var ViewModelClass = (function (name, pic) {
    function ViewModel(name, pic) {
        var self = this;
        this.personName = name;
        this.pictureUrl = pic;
        this.firstName = function (person) {
            return person.name.split(" ")[0].substring(0, 20); // get 1st part before space, and return only 1st 20 characters.
        }
        this.hehimhis = function (person, hehimhis) {
            var returnvalue = hehimhis;
            if (person.gender === "female") {
                if (hehimhis === "he") return "she";
                if (hehimhis === "He") return "She";
                if (hehimhis === "his") return "her";
                if (hehimhis === "His") return "Her";
                if (hehimhis === "him") return "her";
                if (hehimhis === "Him") return "Her";
                console.log("Error: what do you mean by:" + hehimhis);
            }
            return returnvalue;
        };

        // my friends.
        this.selectedfriend = ko.observable();
        this.retrivingFriends = ko.observable(true);
        this.friendsPanelHeader = ko.observable("Loading your friend list...");
        this.friends =  ko.observableArray([]);        // friends array
        this.selectfriend = function (item) {
            this.selectedfriend(item);
        };

        // new request
        this.requestText = ko.observable("");         // request header text value.
        this.requestDateTime = ko.observable("");     // request date/time.
        this.request_location = ko.observable("");    // request location.
        this.requestOnPage = ko.observable(1);        // which page are we on.
        this.showRequestError = ko.observable(false); // need to show error message about next button?

        // inbox
        this.inbox = ko.observableArray([]);          // inbox
        this.selectedinbox = ko.observable();
        this.retrivingInbox = ko.observable(true);
        this.inboxPanelHeader = ko.observable("Loading your Inbox...");
        this.selectInboxItem = function (item) {
            this.selectedinbox(item);
        };

        // outbox
        this.outbox = ko.observableArray([]);
        this.selectedoutbox = ko.observable();
        this.retrivingOutbox = ko.observable(true);
        this.outboxPanelHeader = ko.observable("Loading your Outbox...");
        this.selectOutboxItem = function (item) {
            this.selectedoutbox(item);
        };

        // invite friends
        this.inviteText = ko.observable("Hello friends, Please join karmaweb!");
        this.InviteFriends = function () {
            alert("implement this:" + this.inviteText());
        };

        this.GotoPrevPage = function () {
            self.requestOnPage(self.requestOnPage()-1 );
        }

        this.GotoNextPage = function () {
            if (this.requestText().length < 5) {

                // if request is not value
                // show error message
                // and setup callback to remove the error message.
                self.showRequestError(true);
                $('#textticker').focus(function () {
                    self.showRequestError(false);
                });

            } else {
                self.requestOnPage(self.requestOnPage() + 1);
            }
        };

        this.CreateRequest = function () {
            {
                $.post('/Api/createrequest', { 'Title': this.requestText() },
                        function (data, statusText) {
                            console.log('server returned. data.error ' + data.error + ', data.errorcode:' + data.errorcode + ', statusText' + statusText);
                        });
            }
        };
        function showposition(position) {
            console.log("got current location");
            console.log("lat:" + position.coords.latitude);
            console.log("lan:" + position.coords.longitude);
            self.request_location("lat:" + position.coords.latitude + ",lan:" + position.coords.longitude);
        }
        function showError(error) {
            switch (error.code) {
                case error.PERMISSION_DENIED:
                    console.log("User denied the request for Geolocation.");
                    break;
                case error.POSITION_UNAVAILABLE:
                    console.log("Location information is unavailable.");
                    break;
                case error.TIMEOUT:
                    console.log("The request to get user location timed out.");
                    break;
                case error.UNKNOWN_ERROR:
                    console.log("An unknown error occurred.");
                    break;
            }
        }
        this.UseCurrentLocation = function () {
            console.log("WIll user current location");
            if (navigator.geolocation) {
                navigator.geolocation.getCurrentPosition(showposition, showError);
            }
            else {
                console.log("!navigator.geolocation");
            }
        }

        this.BlockSelectedFriend = function () {
            this.selectedfriend().blocked = true;
            this.selectedfriend.valueHasMutated();
        };
        this.UnBlockSelectedFriend = function () {
            this.selectedfriend().blocked = false;
            this.selectedfriend.valueHasMutated();
        };
    }
    return ViewModel;
})();

function SetupKnockOut(username, userpic)
{
    console.log("SetupKnockOut!");
    var myViewModel = new ViewModelClass(username, userpic);
    ko.applyBindings(myViewModel);

    // make a request to get friends.
    var getFriendsRequest = $.getJSON("/Api/getFriends", function () {
        console.log("success");
    })
        .done(function (data) {
            if (!data.error) {
                myViewModel.friends(data.friends);
                if (data.friends == 0) {
                    myViewModel.friendsPanelHeader("sorry you have no requests");
                }
                else {
                    myViewModel.friendsPanelHeader("Your Frineds:");
                    myViewModel.selectedfriend(data.friends[0]);
                }
            }
            else {
                myViewModel.friendsPanelHeader("Error retriving Friends" + data.errorcode);
            }
        })
        .fail(function () {
            myViewModel.friendsPanelHeader("unknown error retriving friends");
        })
        .always(function () {
            myViewModel.retrivingFriends(false);
        });

    // make a request to get inbox.
    var getInboxRequest = $.getJSON("/Api/getInbox", function () {
        console.log("success");
    })
        .done(function (data) {
            if (!data.error) {
                myViewModel.inbox(data.inbox);
                if (data.inbox == 0) {
                    myViewModel.inboxPanelHeader("no requests to show");
                }
                else {
                    myViewModel.inboxPanelHeader("Inbox:");
                    myViewModel.selectedinbox(data.inbox[0]);
                }
            }
            else {
                myViewModel.inboxPanelHeader("Error retriving Inbox:" + data.errorcode);
            }
        })
        .fail(function () {
            myViewModel.inboxPanelHeader("unknown error retriving Inbox");
        })
        .always(function () {
            myViewModel.retrivingInbox(false);
        });

    // make a request to get inbox.
    var getInboxRequest = $.getJSON("/Api/getOutbox", function () {
        console.log("success");
    })
        .done(function (data) {
            if (!data.error) {
                myViewModel.outbox(data.outbox);
                if (data.outbox == 0) {
                    myViewModel.outboxPanelHeader("no requests to show");
                }
                else {
                    myViewModel.outboxPanelHeader("Outbox:");
                    myViewModel.selectedoutbox(data.outbox[0]);
                }
            }
            else {
                myViewModel.outboxPanelHeader("Error retriving Inbox:" + data.errorcode);
            }
        })
        .fail(function () {
            myViewModel.outboxPanelHeader("unknown error retriving Inbox");
        })
        .always(function () {
            myViewModel.retrivingOutbox(false);
        });


    // Perform other work here ...

    // Set another completion function for the request above
    getFriendsRequest.complete(function () {
        console.log("getFriendsRequest complete");
    });
}

