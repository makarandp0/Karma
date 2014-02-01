using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Web;
using KarmaWebApp.Code;
using Microsoft.WindowsAzure.Storage.Queue;
using KarmaGraph;
using KarmaGraph.Types;
using KarmaWebApp.Code.API;

namespace KarmaWebApp
{
    public class KarmaBackEnd
    {
        private static Graph PeopleGraph = new Graph();
        private static KarmaDb.KarmaDb Database = new KarmaDb.KarmaDb();
        private static KarmaBackEnd BackEnd = new KarmaBackEnd();
        private static KarmaBackgroundWorker worker;

        // private static CloudTable PeopleTable;
        private const int NUM_FRIENDS_TO_SEND_REQUEST_PER_BATCH = 3;
        private static readonly TimeSpan ONE_DAY = new TimeSpan(1, 0, 0, 0);
        private static readonly TimeSpan TWO_DAYS = new TimeSpan(1, 0, 0, 0);
        private static readonly TimeSpan ONE_MINUTE = new TimeSpan(0, 1, 0);
        private static readonly TimeSpan TWENTY_MINUTES = new TimeSpan(0, 20, 0);
        private static readonly TimeSpan TIME_BEFORE_NEW_FRIENDS_BATCH = TWENTY_MINUTES;
        private static readonly TimeSpan TIME_BEFORE_REQUEST_CLOSES_FOR_LACK_OF_RESPONSE = TWO_DAYS;

        public static void LoadDatabase()
        {
            // Retrieve the storage account from configuration.
            var configToUse = ConfigurationManager.AppSettings["UseConfig"];
            var storageEndpointName = "StorageEndPoint-" + configToUse;
            var storageEndpointValue = ConfigurationManager.AppSettings[storageEndpointName];
            var storageAccount = CloudStorageAccount.Parse(storageEndpointValue);

            worker = new KarmaBackgroundWorker(storageAccount);
            Database.SetStorageAccount(storageAccount);

            PeopleGraph.Generate(Database, worker);
            worker.Start();
        }

        private void DelayedTask_ProcessRequest(string workId)
        {
            throw new NotImplementedException();
        }

        #region request stuff
        /*
        /// <summary>
        /// once a request is created. Its opened delayed. 
        /// openeing request is updating its state from Created->Opened.
        /// </summary>
        /// <param name="workId"></param>
        private void DelayedTask_ProcessRequest(string workId)
        {
            var request = ReadRequestEntryFromDatabase(workId);
            if (request != null)
            {
                switch (request.State)
                {
                    case DbKarmaRequest.RequestState.created:
                        if (OpenRequest(request))
                        {
                            SendToNextFriends(request);
                        }
                        break;

                    case DbKarmaRequest.RequestState.opened:
                    case DbKarmaRequest.RequestState.intransitPatial:
                        SendToNextFriends(request);
                        break;

                    case DbKarmaRequest.RequestState.intransitFull:
                        CloseRequestForLackOfResponse(request); // close the request for lack of response
                        break;

                    case DbKarmaRequest.RequestState.opening:
                    case DbKarmaRequest.RequestState.offered:
                    case DbKarmaRequest.RequestState.accepted:
                    case DbKarmaRequest.RequestState.closed:
                        Logger.WriteLine("DelayedTask_ProcessRequest: request:{0} state:{1} is incorrect ", workId, request.State);
                        break;
                }
            }
            else
            {
                Logger.WriteLine("Delayed_OpenRequest: request {0} state is NULL ", workId);
            }
        }

        /// <summary>
        /// notify user that we couldnt find any responses and close the request.
        /// </summary>
        /// <param name="request"></param>
        private void CloseRequestForLackOfResponse(DbKarmaRequest request)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        ///  opens a request. That is figure out nearby friends and 
        ///  update the request with list of such friends.
        /// </summary>
        /// <param name="request"></param>
        private bool OpenRequest(DbKarmaRequest request)
        {
            // find friends to whom the request should be sent.
            var nearbyFriends = CalculateNearbyFriends(request);
            if (nearbyFriends != null && nearbyFriends.Count != 0)
            {
                // add these friends to "showto" field.
                request.FriendsNearBy = string.Join(",", nearbyFriends);

                // update the state to opend
                request.State = DbKarmaRequest.RequestState.opened;
            }
            else
            {
                // there are no friends to show this request to.
                // Nofity Request failure.
                NotifyRequestFailure("sorry no nearby friends to send request to");
                request.State = DbKarmaRequest.RequestState.failed;
            }

            SaveRequest(request);
            return request.State == DbKarmaRequest.RequestState.opened;
            
        }

        private void NotifyRequestFailure(string p)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// sends the request to next batch of friends.
        /// picks top 3 friends from request.ShowTo (and moves them to request.SendTo)
        /// </summary>
        /// <param name="request"></param>
        private void SendToNextFriends(DbKarmaRequest request)
        {
            var friendsNearBy = request.FriendsNearBy.Split(',').ToList();

            // friends whom we need to show this request to
            var friendsToSendThisTime = new List<string>();

            // friends whom we have already shown the request.
            var delieveredTo = request.DelieveredTo.Split(',').ToList();

            foreach (var nearbyFriend in friendsNearBy)
            {
                if (!delieveredTo.Contains(nearbyFriend))
                {
                    // lets add him to our list to send this time.
                    friendsToSendThisTime.Add(nearbyFriend);
                    delieveredTo.Add(nearbyFriend);
                    if (friendsToSendThisTime.Count >= NUM_FRIENDS_TO_SEND_REQUEST_PER_BATCH)
                    {
                        break; // we got enough people for this batch
                    }
                }
            }

            // update DelieveredTo to reflect this new batch.
            request.DelieveredTo = string.Join(",", delieveredTo);

            foreach (var friend in friendsToSendThisTime)
            {
                SendRequestToFriend(request, friend);
            }

            TimeSpan nextProcessTimeAfter = TIME_BEFORE_NEW_FRIENDS_BATCH;
            if (delieveredTo.Count == friendsNearBy.Count)
            {
                request.State = DbKarmaRequest.RequestState.intransitFull;
                nextProcessTimeAfter = TIME_BEFORE_REQUEST_CLOSES_FOR_LACK_OF_RESPONSE;
            }
            else
            {
                // if there are more freiends in showToFriends list.
                request.State = DbKarmaRequest.RequestState.intransitPatial;
            }

            KarmaBackgroundWorker.QueueWorkItem("DelayedTask_ProcessRequest", request.GetRequestId(), nextProcessTimeAfter);

            // Save Request.
            SaveRequest(request);
        }

        /// <summary>
        /// sends the given request to given friend.
        /// request is modified outside this function
        /// </summary>
        /// <param name="request"></param>
        /// <param name="friend"></param>
        private void SendRequestToFriend(DbKarmaRequest request, string friend)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// save updated request.
        /// </summary>
        /// <param name="request"></param>
        private void SaveRequest(DbKarmaRequest request)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// for a given request returns the friends to whom this request to be shown.
        /// this would depend on 1) location 2) Blocked Friends and any other criteria that 
        /// we could think of
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        private List<string> CalculateNearbyFriends(DbKarmaRequest request)
        {
            // TODO: implement calculate nearby friends.
            // for now we return all friends of the creator.
            throw new NotImplementedException();
        }
        */
        #endregion request stuff



        /// <summary>
        /// this is broadcasted to all instances to update a persons 
        /// graph data. We are to read persons information from database
        /// and updates his/her graph. 
        /// 
        /// </summary>
        /// <param name="workId"></param>
        private void BroadCast_AddToGraph(string personId)
        {
            // this function would get called when a new person has been added to database.
            // for some nodes (the one that created the database entry) the person would also exist in people graph.
            // for others it would not. here we update the peoplegraph for this person, and its friends.
        }



        internal static KarmaUser CreateUser(KaramFacebookUser client)
        {
           return PeopleGraph.CreateUser(client);
        }

        internal static KarmaUser GetUser(string fbId)
        {
            return PeopleGraph.GetUser(fbId);
        }
        public static IActiveUser LogonUserUsingFB(string accessToken)
        {
            // verify the access token with facebook.
            var client = new KaramFacebookUser(accessToken);
            if (client.ValidateUser())
            {
                var person = KarmaBackEnd.GetUser(client.FacebookId);
                if (person == null)
                {
                    // we did not find the entry in cache.
                    // or the entry is stale. lets get more info from facebook
                    // and add new entry to our database.
                    if (client.ReadExtendedInformation())
                    {
                        person = KarmaBackEnd.CreateUser(client);
                    }
                }

                if (person != null)
                {
                    return new KarmaActiveUser(person, KarmaBackEnd.BackEnd);
                }
            }
            return null;
        }

        internal KarmaRequest CreateRequest(KarmaUser user, Location location, string subject, KarmaDate dateTime)
        {
            return PeopleGraph.CreateRequest(user, location, subject, dateTime);
        }


        internal bool OfferHelp(KarmaUser karmaUser, string requestId, bool offered)
        {
            return PeopleGraph.OfferHelp(karmaUser, requestId, offered);
        }

        internal bool AcceptHelp(KarmaUser karmaUser, string requestId, string offerFrom, bool accpeted)
        {
            return PeopleGraph.AcceptHelp(karmaUser, requestId, offerFrom, accpeted);
        }
    }
} //KarmaWebApp