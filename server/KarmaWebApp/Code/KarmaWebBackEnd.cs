using Facebook;
using KarmaWebApp;
using KarmaWebApp.Code;
using KarmaWebApp.Code.Graph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;


namespace KarmaBackEnd
{
    public class KarmaBackend : IKarmaWebBackEnd
    {
        // interval at which we would refresh user data from facebook.
        public TimeSpan RefreUserDataAfterTimeSpan = new TimeSpan(10, 0, 0, 0, 0); // 10 HOURS

        public IKaramActiveUser LogonUserUsingFB(string accessToken)
        {
            KarmaGraphNode<KarmaPerson> person = null;

            // verify the access token with facebook.
            var client = new KaramFacebookClient(accessToken);
            if (client.ReadBasicInfo())
            {
                // see if we have the person's info in our cache.
                if (!KarmaDatabase.GetPersonEntry(client.FacebookId, out person))
                {
                    // we did not find the entry in cache.
                    // or the entry is stale. lets get more info from facebook
                    // and add new entry to our database.
                    if (client.ReadExtendedInformation())
                    {
                        // store create comma seperated friends list
                        var nonKarmaFriends = new List<string>();
                        var karmaFriends = new List<string>();
                        
                        foreach (var friend in client.Friends)
                        {
                            if (friend.IsKarmaUser)
                            {
                                karmaFriends.Add(friend.FacebookId);
                            }
                            else
                            {
                                nonKarmaFriends.Add(friend.FacebookId);
                            }
                        }

                        person = KarmaDatabase.CreatePersonEntry(client.FacebookId, client.FirstName, client.Name, client.PictureUrl, client.Location, client.Email, karmaFriends, nonKarmaFriends);
                    }
                }
            }

            
            if (person != null)
            {
                var activeUser = new KarmaActiveUser(person);
                return activeUser;
            }
            else
            {
                return null;
            }
        }
    }

    public class KarmaFriend : IKarmaFriend
    {
        private KarmaGraphNode<KarmaPerson> _activeUser;
        private KarmaGraphNode<KarmaPerson> _karmaFriend;

        public KarmaFriend(KarmaGraphNode<KarmaPerson> activeUser, KarmaGraphNode<KarmaPerson> karmaFriend)
        {
            this._activeUser = activeUser;
            this._karmaFriend = karmaFriend;
        }

        public bool IsBlocked()
        {
            throw new NotImplementedException();
        }

        public void BlockFriend(IKarmaUser friend, bool block)
        {
            throw new NotImplementedException();
        }

        public int karmaPoints
        {
            get 
            {
                return this._karmaFriend.GetValue().KarmaPoints;
            }
        }

        public string FacebookId
        {
            get
            {
                return this._karmaFriend.GetValue().FacebookId();
            }
        }

        public string Name
        {
            get 
            {
                return this._karmaFriend.GetValue().Name;
            }
        }

        public string FirstName
        {
            get 
            {
                return this._karmaFriend.GetValue().FirstName;
            }
        }

        public string PictureUrl
        {
            get 
            {
                return this._karmaFriend.GetValue().PictureUrl;
            }
        }

        public string Location
        {
            get 
            {
                return this._karmaFriend.GetValue().Location;
            }
        }
    }

    public class KarmaRequest : IKarmaRequest
    {
        private string _id;

        public KarmaRequest(string requestId)
        {
            this._id = requestId;
        }

        public string Id
        {
            get { return this._id; }
        }

        public string Subject
        {
            get { throw new NotImplementedException(); }
        }

        public string Message
        {
            get { throw new NotImplementedException(); }
        }

        public DateTime DueDate
        {
            get { throw new NotImplementedException(); }
        }

        public bool IsOpen
        {
            get { throw new NotImplementedException(); }
        }

        public IKarmaUser CreatedBy
        {
            get { throw new NotImplementedException(); }
        }
    }

    public class KarmaActiveUser : IKaramActiveUser
    {
        private KarmaGraphNode<KarmaPerson> _person;

        public string FacebookId 
        { 
            get { return _person.GetValue().FacebookId(); } 
        }

        public string Name 
        {
            get { return _person.GetValue().Name; }  
        }

        public string FirstName 
        {
            get { return _person.GetValue().FirstName; }
        } 

        public string PictureUrl 
        {
            get { return _person.GetValue().PictureUrl; }
        }

        public string Location
        {
            get { return _person.GetValue().Location; }
        }

        public int karmaPoints
        {
            get { return _person.GetValue().KarmaPoints; }
        }

        public KarmaActiveUser(KarmaGraphNode<KarmaPerson> person)
        {
            this._person = person;
        }

        List<IMyRequest> IKaramActiveUser.MyOpenRequests
        {
            get { throw new NotImplementedException(); }
        }

        List<IMyRequest> IKaramActiveUser.MyAllRequests
        {
            get { throw new NotImplementedException(); }
        }

        List<ITheirRequest> IKaramActiveUser.FriendsRequests
        {
            get { throw new NotImplementedException(); }
        }

        public List<IKarmaFriend> Friends()
        {
            var list = new List<IKarmaFriend>();
            var allFriends = this._person.Friends;
            foreach (var f in allFriends)
            {
                list.Add(new KarmaFriend(this._person, f));
            }

            return list;
        }

        public IKarmaRequest CreateRequest(string subject, string message, string strLocation, Decimal lat, Decimal lang, string closedateUTC)
        {

            var requestId = KarmaDatabase.CreateRequest(this._person.GetValue().FacebookId(), lat, lang, strLocation, subject, message, closedateUTC);
            return new KarmaRequest(requestId);
        }

        public IKarmaRequest CreateRequest(string subject, string message, DateTime closedate)
        {
            return CreateRequest(subject, message, "", Decimal.Zero, Decimal.Zero, closedate.ToString("s"));
        }

        public T GetNotificationDetails<T>(string id)
        {
            throw new NotImplementedException();
        }


    }
}
