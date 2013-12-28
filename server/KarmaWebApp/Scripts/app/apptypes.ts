/// <reference path="../typings/knockout/knockout.d.ts" />
module KarmaTypes {

    export class User {
        public Id: string;
        public Name: string;
        public Pic: string;
        public IsMale: boolean;
        constructor(name: string, id: string, pic: string, isMale:boolean) {
            this.Id = id;
            this.Name = name;
            this.Pic = pic;
            this.IsMale = isMale;
        }
        hehimhis(hehimhis) {
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
    }

    export class Friend extends User {
        public IsBlocked: boolean;
        constructor(name: string, id: string, pic: string, isMale:boolean, blocked: boolean) {
            super(name, id, pic, isMale);
            this.IsBlocked = blocked;
        }
        static fromJsonObject(jsondata: any) {
            return new Friend(jsondata.name, jsondata.id, jsondata.pic, jsondata.ismale, jsondata.blocked);
        }
    }

    export class Me extends User {
        constructor(name: string, id: string, pic: string, isMale:boolean) {
            super(name, id, pic, isMale);
        }
        static fromJsonObject(jsondata: any) {
            return new Me(jsondata.name, jsondata.id, jsondata.pic, jsondata.ismale);
        }
    }

    export class HelpOffer {
        public Id: string;
        public OfferedBy: Friend;
        public MyResponse: string;
    }

    export class MyRequest {
        public Id: string;
        public Name: string;
        public Date: string;
        public Offers: HelpOffer[];

        static fromJsonObject(jsondata: any) {
            var request = new MyRequest();
            request.Date = jsondata.date;
            request.Id = jsondata.id;
            request.Name = jsondata.name;
            request.Offers = jsondata.offers;
        }
    }

    export class FriendsRequest {
        public From: Friend;
        public MyResponse: string;
    }

    export class KarmaViewModel {
        public Me: Me;

        public Friends: KnockoutObservable<Array<Friend>>;
        public SelectedFriend: KnockoutObservable<Friend>;

        public Outbox: KnockoutObservable<Array<MyRequest>>;
        public SelectedOutbox: KnockoutObservable<MyRequest>;

        public InBox: KnockoutObservable<FriendsRequest>;
        public SelectedInbox: KnockoutObservable<FriendsRequest>;
        
        SelectFriend(friend: Friend) {
            this.SelectedFriend(friend);
        }

        SelectInboxItem(friendsRequest: FriendsRequest) {
            this.SelectedInbox(friendsRequest);
        }

        SelectOutboxItem(myRequest: MyRequest) {
            this.SelectedOutbox(myRequest);
        }
    }
} 