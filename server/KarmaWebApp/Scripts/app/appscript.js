
var ViewModelClass = (function (name, pic) {
    function ViewModel(name, pic) {
        this.personName = name;
        this.pictureUrl = pic;

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

        this.CreateRequest = function () {
            if (this.requestText().length < 5) {

                // if request is not value
                // show error message
                // and setup callback to remove the error message.
                this.showRequestError(true);
                $('#textticker').focus(function () {
                    myViewModel.showRequestError(false);
                });

            } else {
                $.post('/Api/createrequest', { 'Title': this.requestText() },
                        function (data, statusText) {
                            console.log('server returned. data.error ' + data.error + ', data.errorcode:' + data.errorcode + ', statusText' + statusText);
                        });
            }
        };
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

