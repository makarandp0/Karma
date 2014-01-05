/// <reference path="../typings/knockout/knockout.d.ts" />
module KarmaTypes {

    class Dictionary<T> {
        private items = [];

        public Add(key: string, value: T) {
            this.items.push(value);
            this.items[key] = value;
        }

        public GetByIndex(index: number) {
            return this.items[index];
        }

        public GetByKey(key: string) {
            return this.items[key];
        }

        public Length() {
            return this.items.length;
        }

        public Exists(key: string) {
            return typeof (this.items[key]) !== 'undefined';
        }
    }

    export class User {
        public Id: string;
        public Name: string;
        public Pic: string;
        public IsMale: boolean;
        constructor(jsonUser: any) {
            // { id: "676783107", ismale: true, name: "Lloyd Bond", pic: "https://fbcdn-profile-a.akamaihd.net/hprofile-ak-prn2/1117691_676783107_853903468_q.jpg" },
            this.Id = jsonUser.id;
            this.Name = jsonUser.name;
            this.Pic = jsonUser.pic;
            this.IsMale = jsonUser.ismale;
        }
        public hehimhis(hehimhis) {
            if (!this.IsMale) {
                if (hehimhis === "he") return "she";
                if (hehimhis === "He") return "She";
                if (hehimhis === "his") return "her";
                if (hehimhis === "His") return "Her";
                if (hehimhis === "him") return "her";
                if (hehimhis === "Him") return "Her";
            }
            return hehimhis;
        }

        public FirstName() {
            return this.Name.split(" ")[0].substring(0, 20); // get 1st part before space, and return only 1st 20 characters.
        }
    }

    export class Friend extends User {
        public IsBlocked = ko.observable(true);
        private _parent : KarmaViewModel;
        constructor(jsondata: any, parent: KarmaViewModel) {
            // { blocked: false, id: "676783107", ismale: true, name: "Lloyd Bond", pic: "https://fbcdn-profile-a.akamaihd.net/hprofile-ak-prn2/1117691_676783107_853903468_q.jpg" },
            super(jsondata);
            this.IsBlocked(jsondata.blocked);
            this._parent = parent;
        }

        public BlockFriend() {
            this.IsBlocked(true);
        }
        public UnBlockFriend() {
            this.IsBlocked(false);
        }
    }

    export class Request {
        public Id: string;
        public Name: string;
        public Date: string;
        public Status: string;
        public Location: string;
        constructor(jsondata: any) {
            /*
            {
                id: "628825055_201412312020",
                date: "12/13", // TODO: think more about date format
                title: "Need a Ride to Airport",
                status: "open",
                location: "seattle, WA",
            }
            */
            this.Id = jsondata.id;
            this.Date = jsondata.date;
            this.Name = jsondata.title;
            this.Status = jsondata.status;
            this.Location = jsondata.location;
        }
    }

    export class HelpOffer {
        public Id: string;
        public From: Friend;
        public Response = ko.observable<string>("none");

        constructor(jsonOffer: any) {
            // { id: "628825055_201412312020_575635813", response: "none", from: "575635813" },
            this.Id = jsonOffer.id;
            this.Response(jsonOffer.response);
            this.From = KarmaViewModel.staticFriendsDictionary.GetByKey(jsonOffer.from);
        }

        public AcceptHelp() {
            this.AcceptIgnoreOffer("yes");
        }

        public IgnoreHelp() {
            this.AcceptIgnoreOffer("no");
        }

        // accept=yes|no
        public AcceptIgnoreOffer(accept: string) {
            this.Response(accept);
            var createOfferURL = '/Api/accepthelp/?requestId=' + this.Id + '&accept=' + accept;
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
        }

    }

    export class MyRequest extends Request{
        public helpOffered: HelpOffer[] = [];
        private _parent: KarmaViewModel;

        constructor(jsonRequest: any, parent: KarmaViewModel) {
            /*
            {
                ...
                helpOffers: [
                    { id: "628825055_201412312020_648352020", response: "none", from: "648352020" },
                    { id: "628825055_201412312020_631784108", response: "none", from: "631784108" },
                ]
            },
            */
            super(jsonRequest);
            this._parent = parent;
            for (var i = 0; i < jsonRequest.helpOffers.length; ++i) {
                var offer = new HelpOffer(jsonRequest.helpOffers[i]);
                this.helpOffered.push(offer);
            }
        }
    }

    export class FriendsRequest extends Request{
        public From: Friend;
        public Response: KnockoutObservable<string>;
        private _parent: KarmaViewModel;

        constructor(jsonRequest: any, parent: KarmaViewModel) {
            /*
            {
                id: "615700133_201312312020",
                date: "12/23",
                status: "open",
                location: "seattle, WA",
                title: "a job",

                response: "none",
                from: "615700133"

            },
            */
            
            super(jsonRequest);
            this._parent = parent;
            this.From = KarmaViewModel.staticFriendsDictionary.GetByKey(jsonRequest.from);
            this.Response = ko.observable<string>(jsonRequest.response);
        }

        public OfferHelp() {
            this.OfferDenyHelp("yes");
        }
        public DenyHelp() {
            this.OfferDenyHelp("no");
        }

        // offer = yes|no
        public OfferDenyHelp(offer: string) {
            var self = this;
            this.Response(offer);
            var createOfferURL = '/Api/offerhelp/?requestId=' + this.Id + '&offer=' + offer;
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
                    // self.selectedinbox.valueHasMutated();
                });
        }

    }

    export class SelectableList<T> {

        public Items = ko.observableArray<T>(null);
        public SelectedItem = ko.observable<T>(null);
        public PanelHeader = ko.observable<string>("Loading...");

        public SelectItem(item: T) {
            this.SelectedItem(item);
        }

        // sets the header and selects the 1st time
        public SelectDefault(header: string) {
            if (this.Items().length > 0) {
                this.SelectedItem(this.Items()[0]);
                this.SetHeader(header);
            }
            else {
                this.SetHeader(header + " none found!");
            }
        }

        public SetHeader(header: string) {
            this.PanelHeader(header);
        }

        public RefreshSelection() {
            this.SelectedItem.valueHasMutated();
        }
    }

    export class KarmaViewModel {

        static staticFriendsDictionary = new Dictionary<Friend>();
        public self: KarmaViewModel;

        // reads all the information from json object passed.
        public ReadFromJSON(jsonObject:any) {

            console.log("reading from Json");
            this.Me(new User(jsonObject.me));

            // setup friends dictionary and the friends observable array.
            for (var i = 0; i < jsonObject.friends.length; ++i) {
                var friend = new Friend(jsonObject.friends[i], this);
                KarmaViewModel.staticFriendsDictionary.Add(friend.Id, friend);
                this.MyFriends.Items.push(friend);
            }

            this.MyFriends.SelectDefault("Your Friends:");

            // setup inbox
            for (var i = 0; i < jsonObject.inbox.length; ++i) {
                var inboxItem = new FriendsRequest(jsonObject.inbox[i], this);
                this.MyInbox.Items.push(inboxItem);
            }

            this.MyInbox.SelectDefault("Your Inbox:");

            // setup outbox.
            for (var i = 0; i < jsonObject.outbox.length; ++i) {
                var outboxItem = new MyRequest(jsonObject.outbox[i], this);
                this.MyOutbox.Items.push(outboxItem);
            }

            this.MyOutbox.SelectDefault("Your Outbox:");
        }

        public Me = ko.observable<User>(null);

        public MyFriends = new SelectableList<Friend>();
        public MyInbox = new SelectableList<FriendsRequest>();
        public MyOutbox = new SelectableList<MyRequest>(); 

        // new request
        public requestText = ko.observable<string>("");          // request header text value.
        public requestDateTime = ko.observable<string>("");      // request date/time.
        public request_location = ko.observable<string>("");     // request location.
        public requestOnPage = ko.observable<number>(1);         // which page are we on.
        public showRequestError = ko.observable<boolean>(false); // need to show error message about next button?
        public activePannel = ko.observable<string>("you");   // currently active pannel.

        // invite friends
        public inviteText = ko.observable<string>("Hello friends, Please join karmaweb!");
        public InviteFriends() {
            alert("implement this:" + this.inviteText());
        }

        public GotoPrevRequestPage() {
            this.requestOnPage(this.requestOnPage() - 1);
        }

        public SelectPanel(pannelName: string) {
            var self = this;
            var oldPanel = self.activePannel();
            $("#" + oldPanel).slideUp("fast", function () {
                $('.friends-details, .inbox-details, .outbox-details').hide();
                self.activePannel(pannelName);
                $("#" + pannelName).slideDown();
                $('.friends-details, .inbox-details, .outbox-details').hide();
            });
        }
        public SelectYou() {
            this.SelectPanel("you");
        }
        
        public SelectFriends() {
            
            this.SelectPanel('friends');
        }
        public SelectInbox() {
            this.SelectPanel('inbox');
        }
        public SelectOutbox() {
            this.SelectPanel('outbox');
        }

        public GotoNextRequestPage () {
            if (this.requestText().length < 5) {

                // if request is not value
                // show error message
                // and setup callback to remove the error message.
                this.showRequestError(true);
                var self = this;
                $('#textticker').focus(function () {
                    self.showRequestError(false);
                });

            } else {
                this.requestOnPage(this.requestOnPage() + 1);
            }
        }

        public CreateRequest() {
            var self = this;
            var createrequestURL = '/Api/createrequest/?title=' + encodeURIComponent(this.requestText()) + '&date=' + encodeURIComponent(this.requestDateTime()) + '&location=' + encodeURIComponent(this.request_location());
            var createRequest = $.getJSON(createrequestURL, function () {
                console.log(createrequestURL + ":success");
            })
                .done(function (data) {
                    if (!data.error) {
                        var newrequest = new MyRequest(data.request, self);
                        self.MyOutbox.Items.push(newrequest);
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
        }

        public UseCurrentLocation() {
            console.log("WIll user current location");
            if (navigator.geolocation) {
                navigator.geolocation.getCurrentPosition(function (position) {
                    console.log("got current location:" + "lat:" + position.coords.latitude + ",lan:" + position.coords.longitude);
                    this.request_location("lat:" + position.coords.latitude + ",lan:" + position.coords.longitude);
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
                            default:
                                console.log("navigator.geolocation.getCurrentPosition unknown error occurred:" + error.code);
                                break;
                        }
                    });
            }
            else {
                console.log("!navigator.geolocation");
            }
        }


        static getURLParameter(name: string) {
            return decodeURIComponent((new RegExp('[?|&]' + name + '=' + '([^&;]+?)(&|#|;|$)').exec(location.search) || [, ""])[1].replace(/\+/g, '%20')) || null;
        }

        static SetupKnockOut() {
            var myViewModel = new KarmaViewModel();
            ko.applyBindings(myViewModel);

            // check if url has parameter specified for accesstoken
            // 
            var getAllUrl = "/Api/getAll/";
            var accessToken = KarmaViewModel.getURLParameter("accessToken");
            console.log("accessToken:" + accessToken);
            if (accessToken == null) {
                accessToken = "fake";
            } 
            getAllUrl += "?accessToken=" + encodeURIComponent(accessToken);

            var getAllRequest = $.getJSON(getAllUrl, function () {
                console.log(getAllUrl + ":success");
            })
                .done(function (data) {
                    if (!data.error) {
                        myViewModel.ReadFromJSON(data);
                    }
                    else {
                        // myViewModel.friendsPanelHeader("Error retriving Friends" + data.errorcode);
                    }
                })
                .fail(function () {
                    // myViewModel.friendsPanelHeader("unknown error retriving friends");
                })
                .always(function () {
                    // myViewModel.retrivingFriends(false);
                });
        }

        constructor() {
            // this._friendsDictionary = new Dictionary<Friend>();
            // this.self = this;
        }
    }
} 
