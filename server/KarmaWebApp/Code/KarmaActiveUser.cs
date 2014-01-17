using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KarmaGraph.Types;
using KarmaDb.Types;

namespace KarmaWebApp.Code.API
{
    public class KarmaActiveUser : IActiveUser
    {
        private KarmaBackEnd karmaBackEnd {get;set;}

        private KarmaUser me {get;set;}

        public KarmaActiveUser(KarmaUser person, KarmaBackEnd karmaBackEnd)
        {
            this.me = person;
            this.karmaBackEnd = karmaBackEnd;
        }

        
        public JsonCreateRequestResponse CreateRequest(string subject, KarmaDate dateTime, Location location)
        {
            var response = new JsonCreateRequestResponse();
            var request = this.karmaBackEnd.CreateRequest(this.me, location, subject, dateTime);
            if (request == null)
            {
                response.seterror("karmaBackEnd.CreateRequest failed");
            }

            response.request = new JsonOutboxEntry();
            response.request.date = request.dueDate.ToJsonDate();
            response.request.id = request.requestId;
            response.request.location = request.location.name;
            response.request.status = EDBRequestState.isOpen(request.state) ? "open" : "closed";
            response.request.title = request.title;
            return response;
        }


        public JsonGetAllResponse GetAll()
        {
            // me
            var response = new JsonGetAllResponse();
            response.me = JsonUserFromKarmaUser(this.me);
            
            // friends
            response.friends = new List<JsonFriend>();
            foreach (var friend in this.me.friends)
            {
                response.friends.Add(JsonFriendFromKarmaUser(this.me, friend));
            }
            
            // inbox
            response.inbox = new List<JsonInboxEntry>();
            foreach (var inboxItem in this.me.inbox)
            {
                var jsonInboxItem = new JsonInboxEntry();
                jsonInboxItem.from = inboxItem.from.id;
                jsonInboxItem.id = inboxItem.requestId;
                jsonInboxItem.title = inboxItem.title;
                jsonInboxItem.date = inboxItem.dueDate.ToJsonDate();
                jsonInboxItem.location = inboxItem.location.name;

                if (inboxItem.offeredBy.Contains(this.me))
                    jsonInboxItem.response = "yes";
                else if (inboxItem.ignoredBy.Contains(this.me))
                    jsonInboxItem.response = "no";
                else
                    jsonInboxItem.response = "none";

                response.inbox.Add(jsonInboxItem);
            }

            // outbox
            response.outbox = new List<JsonOutboxEntry>();
            foreach (var outboxItem in this.me.outbox)
            {
                var jsonOutboxItem = new JsonOutboxEntry();
                jsonOutboxItem.title = outboxItem.title;
                jsonOutboxItem.id = outboxItem.requestId;
                jsonOutboxItem.location = outboxItem.location.name;
                jsonOutboxItem.helpOffers = new List<JsonhelpOfferEntry>();
                jsonOutboxItem.date = outboxItem.dueDate.ToJsonDate();
                foreach(var offer in outboxItem.offeredBy)
                {
                    var offerEntry = new JsonhelpOfferEntry();
                    offerEntry.id = MakeOfferId(outboxItem.requestId, offer.id);
                    
                    offerEntry.from = offer.id;
                    
                    if (outboxItem.ignoredFrom.Contains(offer))
                        offerEntry.response = "no";
                    else if (outboxItem.acecptedFrom.Contains(offer))
                        offerEntry.response = "yes";
                    else 
                        offerEntry.response = "none";

                    jsonOutboxItem.helpOffers.Add(offerEntry);
                }

                response.outbox.Add(jsonOutboxItem);
            }
            return response;
        }

        public const char OFFER_SEPERATOR = '#';
        private string MakeOfferId(string requestId, string offeredBy)
        {
            if (requestId.Contains(OFFER_SEPERATOR) || offeredBy.Contains(OFFER_SEPERATOR))
                throw new ArgumentException("request id or offeredBy contains \""+ OFFER_SEPERATOR + "\"");
            return requestId + OFFER_SEPERATOR + offeredBy; // requestid_responderid
        }

        private string ExtractRequestId(string offerid)
        {
            var items = offerid.Split(OFFER_SEPERATOR);
            if (items.Length != 2) throw new ArgumentException("offerid must contain only one \""+ OFFER_SEPERATOR + "\"");
            return items[0];
        }
        private string ExtractOfferedBy(string offerid)
        {
            var items = offerid.Split(OFFER_SEPERATOR);
            if (items.Length != 2) throw new ArgumentException("offerid must contain only one \"" + OFFER_SEPERATOR + "\"");
            return items[1];
        }

        public JsonData OfferHelp(string requestId, bool offered)
        {
            var response = new JsonData();
            if (!this.karmaBackEnd.OfferHelp(this.me, requestId, offered))
            {
                response.seterror("karmaBackEnd.OfferHelp failed");
            }

            return response;
        }

        public JsonData AcceptHelp(string offerId, bool accpeted)
        {
            var response = new JsonData();
            var requestId = ExtractRequestId(offerId);
            var offeredBy = ExtractOfferedBy(offerId);
            if (!this.karmaBackEnd.AcceptHelp(this.me, requestId, offeredBy, accpeted))
            {
                response.seterror("karmaBackEnd.AcceptHelp failed");
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
            to.firstname = from.firstName;
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
