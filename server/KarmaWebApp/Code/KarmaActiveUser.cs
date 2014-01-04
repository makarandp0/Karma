using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KarmaGraph.Types;

namespace KarmaWebApp.Code.API
{
    public class KarmaActiveUser : IActiveUser
    {
        private KarmaUser person { get; set; }
        private KarmaBackEnd karmaBackEnd {get;set;}

        private KarmaUser _activeUser {get;set;}

        public KarmaActiveUser(KarmaUser person, KarmaBackEnd karmaBackEnd)
        {
            this.person = person;
            this.karmaBackEnd = karmaBackEnd;
        }

        
        public JsonCreateRequestResponse CreateRequest(string subject, KarmaDate dateTime, Location location)
        {
            var response = new JsonCreateRequestResponse();
            var request = this.karmaBackEnd.CreateRequest(this._activeUser.id, location, subject, dateTime);
            if (request == null)
            {
                response.seterror("karmaBackEnd.CreateRequest failed");
            }

            response.request = new JsonOutboxEntry();
            response.request.date = request.dueDate.ToJsonDate();
            response.request.id = request.requestId;
            response.request.location = request.location.name;
            response.request.status = RequestStateUtil.FromDBToJson(request.state);
            response.request.title = request.title;
            return response;
        }


        public JsonGetAllResponse GetAll()
        {
            var response = new JsonGetAllResponse();
            response.me = JsonUserFromKarmaUser(this._activeUser);
            
            response.friends = new List<JsonFriend>();
            foreach (var friend in this._activeUser.friends)
            {
                response.friends.Add(JsonFriendFromKarmaUser(this._activeUser, friend));
            }
            
            response.inbox = new List<JsonInboxEntry>();
            foreach (var inboxItem in this._activeUser.inbox)
            {
                var jsonInboxItem = new JsonInboxEntry();
                jsonInboxItem.from = inboxItem.from.id;
                jsonInboxItem.id = inboxItem.requestId;
                jsonInboxItem.title = inboxItem.title;
                jsonInboxItem.date = "dummy date"; // TODO: fix this
                jsonInboxItem.location = inboxItem.location.name;

                if (inboxItem.offeredBy.Contains(this._activeUser))
                    jsonInboxItem.response = "yes";
                else if (inboxItem.ignoredBy.Contains(this._activeUser))
                    jsonInboxItem.response = "no";
                else
                    jsonInboxItem.response = "none";
            }
            // TODO finish outbox.
            response.outbox = new List<JsonOutboxEntry>();
            foreach (var outboxItem in this._activeUser.outbox)
            {
                var jsonOutboxItem = new JsonOutboxEntry();
                jsonOutboxItem.title = outboxItem.title;
                jsonOutboxItem.id = outboxItem.requestId;
                jsonOutboxItem.location = outboxItem.location.name;
                jsonOutboxItem.helpOffers = new List<JsonhelpOfferEntry>();
                foreach(var offer in outboxItem.offeredBy)
                {
                    var offerEntry = new JsonhelpOfferEntry();
                    offerEntry.id = outboxItem.requestId + "_" + offer.id; // requestid_responderid
                    offerEntry.from = offer.id;
                    
                    if (outboxItem.ignoredFrom.Contains(offer))
                        offerEntry.response = "no";
                    else if (outboxItem.acecptedFrom.Contains(offer))
                        offerEntry.response = "yes";
                    else 
                        offerEntry.response = "none";

                    jsonOutboxItem.helpOffers.Add(offerEntry);
                }
            }
            return response;
        }

        #region private members
        private JsonUser JsonUserFromKarmaUser(KarmaUser karmaUser)
        {
            var jsonUser = new JsonUser();
            CopyUserData(jsonUser, karmaUser);
            return jsonUser;
        }
        private void CopyUserData(JsonUser to, KarmaUser from)
        {
            to.id = from.id;
            to.name = from.name;
            to.ismale = from.gender == EGender.Male;
            to.pic = from.pic;
        }

        private JsonFriend JsonFriendFromKarmaUser(KarmaUser me, KarmaUser friend)
        {
            var jsonFriend = new JsonFriend();
            CopyUserData(jsonFriend, friend);
            jsonFriend.blocked = me.HasBlocked(friend);
            return jsonFriend;
        }
        #endregion
    }

}
