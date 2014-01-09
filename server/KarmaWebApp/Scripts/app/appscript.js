
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

        this.GotoPrevRequestPage = function () {
            self.requestOnPage(self.requestOnPage()-1 );
        }

        this.GotoNextRequestPage = function () {
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
            var createrequestURL = '/Api/createrequest/?title='+ encodeURIComponent(this.requestText()) + '&date=' + encodeURIComponent(this.requestDateTime()) +'&location=' + encodeURIComponent(this.request_location());
            var createRequest = $.getJSON(createrequestURL, function () {
                console.log(createrequestURL + ":success");
            })
                .done(function (data) {
                    if (!data.error) {
                        self.outbox.push(data.request);
                    }
                    else {
                        console.log(createrequestURL + ":returned error:" + data.error + ", errorcode:" + data.errorcode);
                    }
                })
                .fail(function () {
                    console.log(createrequestURL + ":failed");
                })
                .always(function () {
                });
        };

        this.UseCurrentLocation = function () {
            console.log("WIll user current location");
            if (navigator.geolocation) {
                navigator.geolocation.getCurrentPosition(function (position) {
                        console.log("got current location:" + "lat:" + position.coords.latitude + ",lan:" + position.coords.longitude);
                        self.request_location("lat:" + position.coords.latitude + ",lan:" + position.coords.longitude);
                    }, 
                    function (error) {
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
            });
            }
            else {
                console.log("!navigator.geolocation");
            }
        }

        this.BlockSelectedFriend = function () {
            self.selectedfriend().blocked = true;
            self.selectedfriend.valueHasMutated();
        };
        this.UnBlockSelectedFriend = function () {
            self.selectedfriend().blocked = false;
            self.selectedfriend.valueHasMutated();
        };

        // offer = yes|no
        this.OfferDenyHelp = function (inboxItem, offer) {
            var createOfferURL = '/Api/offerhelp/?requestId=' + inboxItem.request.id + '&offer=' + offer;
            var createRequest = $.getJSON(createOfferURL, function () {
                console.log(createOfferURL + ":success");
            })
                .done(function (data) {
                    if (!data || data.error) {
                        console.log(createOfferURL + ":returned error:" + data.error + ", errorcode:" + data.errorcode);
                    }
                })
                .fail(function () {
                    console.log(createOfferURL + ":failed");
                })
                .always(function () {
                    self.selectedinbox.valueHasMutated();
                });
        };

        this.OfferHelp = function (inboxItem) {
            self.OfferDenyHelp(inboxItem, "yes");
            inboxItem.request.yourStatus = 1;  // offered help
            self.selectedinbox.valueHasMutated();
        };
        
        this.DenyHelp = function (inboxItem) {
            self.OfferDenyHelp(inboxItem, "no");
            inboxItem.request.yourStatus = 2; // denied help
            self.selectedinbox.valueHasMutated();
        };

        // accept=yes|no
        this.AcceptIgnoreOffer = function (offerId, accept) {
            var createOfferURL = '/Api/accepthelp/?offerId=' + offerId + '&accept=' + accept;
            var acceptIgnore = $.getJSON(createOfferURL, function () {
                console.log(createOfferURL + ":success");
            })
                .done(function (data) {
                    if (!data || data.error) {
                        console.log(createOfferURL + ":returned error:" + data.error + ", errorcode:" + data.errorcode);
                    }
                })
                .fail(function () {
                    console.log(createOfferURL + ":failed");
                })
                .always(function () {
                    // todo: add a notification here.
                });
        };

        this.AcceptHelp = function (outboxItem, offer) {

            console.log("will accept:");
            console.log("outboxItem.name:" + outboxItem.name);
            console.log("friend.name:" + offer.name);
            self.AcceptIgnoreOffer(offer.offerid, "yes");
            offer.yourStatus = 1; // accepted offer.
            self.selectedoutbox.valueHasMutated();
        };

        this.IgnoreHelp = function (outboxItem, offer) {
            console.log("will ignore:");
            console.log("outboxItem.name:" + outboxItem.name);
            console.log("friend.name:" + offer.name);
            self.AcceptIgnoreOffer(offer.offerid, "no");
            offer.yourStatus = 2; // ignored offer.
            self.selectedoutbox.valueHasMutated();
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
        console.log("/Api/getFriends:success");
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
        console.log("/Api/getInbox:success");
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
        console.log("/Api/getOutbox:success");
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

    public checkdate(input){
        var validformat=/^\d{2}\/\d{2}\/\d{4}$/ //Basic check for format validity
        var returnval=false
        if (!validformat.test(input.value))
            alert("Invalid Date Format. Please correct and submit again.")
        else{ //Detailed check for valid date ranges
            var monthfield=input.value.split("/")[0]
            var dayfield=input.value.split("/")[1]
            var yearfield=input.value.split("/")[2]
            var dayobj = new Date(yearfield, monthfield-1, dayfield)
            if ((dayobj.getMonth()+1!=monthfield)||(dayobj.getDate()!=dayfield)||(dayobj.getFullYear()!=yearfield))
                alert("Invalid Day, Month, or Year range detected. Please correct and submit again.")
            else
                returnval=true
        }
        if (returnval==false) input.select()
        return returnval
    }
}

