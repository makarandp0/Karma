using KarmaWebApp.Code.Graph;
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

namespace KarmaWebApp
{
  
    public class KarmaDatabase
    {
        private static KarmaDatabase TheDatabase = new KarmaDatabase();
        private static KarmaGraph<KarmaPerson> PeopleGraph = new KarmaGraph<KarmaPerson>();
        private static KarmaBackgroundWorker KarmaBackgroundWorker;
        
        private static CloudTable PeopleTable;
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

            // create/open people table.
            var tableClient = storageAccount.CreateCloudTableClient();
            PeopleTable = tableClient.GetTableReference("karmapeople");
            PeopleTable.CreateIfNotExists();

            KarmaBackgroundWorker = new KarmaBackgroundWorker(storageAccount);
            KarmaBackgroundWorker.RegisterWorkItem("DelayedTask_UpdateFriends", new KarmaBackgroundWorker.WorkItemDelegate(TheDatabase.DelayedTask_UpdateFriends));
            KarmaBackgroundWorker.RegisterWorkItem("DelayedTask_ProcessRequest", new KarmaBackgroundWorker.WorkItemDelegate(TheDatabase.DelayedTask_ProcessRequest));
            KarmaBackgroundWorker.RegisterWorkItem("BroadCast_AddToGraph", new KarmaBackgroundWorker.WorkItemDelegate(TheDatabase.BroadCast_AddToGraph));
            
            GenerateGraph();
        }

        

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
                        Debug.WriteLine("DelayedTask_ProcessRequest: request:{0} state:{1} is incorrect ", workId, request.State);
                        break;
                }
            }
            else
            {
                Debug.WriteLine("Delayed_OpenRequest: request {0} state is NULL ", workId);
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
                request.ShowTo = string.Join(",", nearbyFriends);

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
            // friends whom we need to show this request to
            var showToFriends = request.ShowTo.Split(',').ToList();

            // friends whom we have already shown the request.
            var sentToFriends = request.SendTo.Split(',').ToList();

            // number of friends to whom will send this request this time.
            var numfriendsToTake  = Math.Min(showToFriends.Count(), NUM_FRIENDS_TO_SEND_REQUEST_PER_BATCH);

            // friends whome we are going to send the request this time.
            var friendsToSendThisTime = showToFriends.Take(numfriendsToTake);

            // update showToFriends to contain only remaining..
            showToFriends = showToFriends.Skip(numfriendsToTake).ToList();

            if (showToFriends.Count > 0)
            {
                request.ShowTo = string.Join(",", showToFriends);
            }
            else
            {
                request.ShowTo = "";
            }
            
            // update sentToFriends to reflect this new batch.
            sentToFriends.AddRange(friendsToSendThisTime);
            request.SendTo = string.Join(",", sentToFriends);

            foreach (var friend in friendsToSendThisTime)
            {
                SendRequestToFriend(request, friend);
            }

            TimeSpan nextProcessTimeAfter = TIME_BEFORE_NEW_FRIENDS_BATCH;
            if (showToFriends.Count > 0)
            {
                // if there are more freiends in showToFriends list.
                request.State = DbKarmaRequest.RequestState.intransitPatial;
            }
            else
            {
                request.State = DbKarmaRequest.RequestState.intransitFull;
                nextProcessTimeAfter = TIME_BEFORE_REQUEST_CLOSES_FOR_LACK_OF_RESPONSE;
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

        /// <summary>
        /// Delayed_UpdateFriends
        /// this task is queued to update friends for a newly added user
        /// when a user is added to database, his friends are not updated to link to him yet
        /// Function loads the user from database, and going thru his friends fixes the graph 
        /// links  and saves all friends into the database.
        /// </summary>
        /// <param name="personId"></param>
        private void DelayedTask_UpdateFriends(string personId)
        {
            // read the persons entry from database.
            var person = ReadPersonEntryFromDatabase(personId);
            if (person != null)
            {
                // TODO: check if batch/async operation would be appropriate?
                // and update each friends database entry with this persons link.
                foreach (var friendid in person.ReadKarmaFriends())
                {
                    Debug.WriteLine("Updating Karmafriends " + friendid);
                    var friend = ReadPersonEntryFromDatabase(friendid);
                    friend.AddKarmaFriend(person.FacebookId());
                    var updateFriend = TableOperation.InsertOrReplace(friend);
                    PeopleTable.Execute(updateFriend);
                }
            }
        }

        /// <summary>
        ///  adds person to graph, and fixes its friends link in graph. 
        /// </summary>
        /// <param name="person"></param>
        /// <returns></returns>
        KarmaGraphNode<KarmaPerson> AddToGraph(KarmaPerson person)
        {
            var node = PeopleGraph.AddNode(person.FacebookId(), person);
            foreach (var friendid in person.ReadKarmaFriends())
            {
                Debug.WriteLine("AddPersonEntry:Updating Karmafriends " + friendid);
                KarmaPerson friend;
                if (PeopleGraph.TryGetValue(friendid, out friend))
                {
                    PeopleGraph.AddLink(person.FacebookId(), friendid, true);
                }
            }
            return node;
        }


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
            if (!PeopleGraph.ContainsKey(personId))
            {
                AddToGraph(ReadPersonEntryFromDatabase(personId));
            }
        }


        // Load all entries.
        public static void GenerateGraph()
        {
            // TODO:
            // 1. This should run in seperate thread and run continuously to update the graph as table updates
            // 2. Once an initial graph is created, this should signal an event to mark it "ready" for requests 
            // 3. after marking it ready the thread should keep running continously querrying for changes.
            // 4. query logic below need to be fixed to account for continuation tokens and using simultaneous requests.
            string rowKeyFilter = TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal, KarmaPerson.ROW_ID);
            var allPeopleQuery = new TableQuery<KarmaPerson>().Where(rowKeyFilter);

            // read all entries into a dictionary.
            {
                var allpeople = PeopleTable.ExecuteQuery<KarmaPerson>(allPeopleQuery);
                foreach (var person in allpeople)
                    PeopleGraph.AddNode(person.FacebookId(), person);
            }

            Debug.WriteLine("GenerateGraph:Created Nodes:" + PeopleGraph.Count);

            // now for each entry setup graph links.
            int edges = 0;
            foreach (var node in PeopleGraph.Values)
            {
                var person = node.GetValue();
                foreach (var friend in person.ReadKarmaFriends())
                {
                    // create a one way link between friends. from => to
                    // other side of the link will be established when we process
                    // to node.
                    PeopleGraph.AddLink(person.FacebookId(), friend, false);
                    edges++;
                }
            }
            Debug.WriteLine("GenerateGraph:Done updating edges:" + edges);
        }

        
        /// <summary>
        /// reads specific person from database.
        /// </summary>
        /// <param name="facebookId"></param>
        /// <param name="person"></param>
        /// <returns></returns>
        public static KarmaPerson ReadPersonEntryFromDatabase(string facebookId)
        {
            try
            {
                var retrieveOperation = TableOperation.Retrieve<KarmaPerson>(facebookId, KarmaPerson.ROW_ID);
                var retrievedResult = PeopleTable.Execute(retrieveOperation);
                if (retrievedResult.Result != null)
                {
                    return (KarmaPerson)retrievedResult.Result;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception retriving person:{0} from database:{1}", facebookId, ex);
            }
            return null;
        }

        public static DbKarmaRequest ReadRequestEntryFromDatabase(string requestId)
        {
            try
            {
                string partitionKey;
                string rowKey;
                if (DbKarmaRequest.GetKeys(requestId, out partitionKey, out rowKey))
                {
                    var retrieveOperation = TableOperation.Retrieve<DbKarmaRequest>(partitionKey, rowKey);
                    var retrievedResult = PeopleTable.Execute(retrieveOperation);
                    if (retrievedResult.Result != null)
                    {
                        return (DbKarmaRequest)retrievedResult.Result;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception retriving request:{0} from database:{1}", requestId, ex);
            }
            return null;
        }

        /// <summary>
        /// return a KarmaPerson if it exists, Null otherwise
        /// </summary>
        /// <param name="facebookid"></param>
        public static bool GetPersonEntry(string facebookid, out KarmaGraphNode<KarmaPerson> person)
        {
            // retrive perticular user from cache.
            bool success = PeopleGraph.TryGetValue(facebookid, out person);
            Debug.WriteLine("GetPersonEntry({0} returned:{1} ", facebookid, success);
            return success;
        }

        internal static KarmaGraphNode<KarmaPerson> CreatePersonEntry(string facebookId, string firstName, string fullname, string pictureUrl, string location, string email, List<string> karmaFriends, List<string> nonKarmaFriends)
        {
            string karmaFriendString = string.Join(",", karmaFriends);
            string nonKarmaFriendString = string.Join(",", nonKarmaFriends);

            var person = new KarmaPerson(facebookId, firstName, fullname, pictureUrl, location, email, karmaFriendString, nonKarmaFriendString);
            return AddPersonEntry(person);
        }

        /// <summary>
        /// adds a new user persons.
        /// </summary>
        /// <param name="person"></param>
        private static KarmaGraphNode<KarmaPerson> AddPersonEntry(KarmaPerson person)
        {
            Debug.WriteLine("AddPersonEntry: for:" + person.FacebookId());
            if (PeopleGraph.ContainsKey(person.FacebookId()))
            {
                Debug.WriteLine("AddPersonEntry:Person already exists:" + person.FacebookId());
                return null;
            }

            //
            // we will save this person here. But not update his friends right away. Instead create a 
            // work item to update the friends database entries to  point to this person.
            //
            if (KarmaBackgroundWorker.QueueWorkItem("DelayedTask_UpdateFriends", person.FacebookId()))
            {
                person.ETag = "*"; // this tells azure that overwrite the entry even if its modified before us.
                var insertPerson = TableOperation.Insert(person);
                PeopleTable.Execute(insertPerson);
                Debug.WriteLine("AddPersonEntry:Executed InsertPerson:" + person.FacebookId());

                // let other instances know that graph need to be added to the graph
                KarmaBackgroundWorker.BroadCastWorkItem("BroadCast_AddToGraph", person.FacebookId());
                return TheDatabase.AddToGraph(person);
            }
            return null;
        }

        internal static string CreateRequest(string userid, decimal lat, decimal lang, string strLocation, string subject, string message, string closedateUTC)
        {
            var request = new DbKarmaRequest(userid, lat, lang, strLocation, subject, message, closedateUTC);
            return AddRequest(request);
        }

        private static string AddRequest(DbKarmaRequest request)
        {
            try
            {
                // before creating actual request record, create a work item to 
                // open the request that we are about to create.
                // worker thread will open all such requests and process them.
                if (KarmaBackgroundWorker.QueueWorkItem("DelayedTask_ProcessRequest", request.GetRequestId()))
                { 
                    // now store the request 
                    request.ETag = "*"; // this tells azure that overwrite the entry even if its modified before us.
                    var insertRequest = TableOperation.Insert(request);
                    PeopleTable.Execute(insertRequest);
                    return request.GetRequestId();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Failed to add request:" + request.GetRequestId() + "Exception:" + ex);
            }

            return null;
        }

    }

    /// <summary>
    /// TODO: seperate graph info from rest of the user info.
    /// we want to keep graph in memory, but not rest of info.
    /// </summary>
    public class KarmaPerson : TableEntity
    {
        public const string ROW_ID = "basic_info";

        public KarmaPerson(string facebookId, string firstName, string fullname, string pictureUrl, string location, string email, string karmaFriends, string nonKarmaFriends)
        {
            this.PartitionKey = facebookId;
            this.RowKey = KarmaPerson.ROW_ID;

            this.Name = fullname;
            this.FirstName = firstName;
            this.PictureUrl = pictureUrl;
            this.Location = location;
            this.Email = email;
            this.NonKarmaFriends = nonKarmaFriends;
            this.KarmaFriends = karmaFriends;
        }

        // TODO: this is method and not property because we dont want to get autoserialized.
        // figure out better way to control this.
        public string FacebookId() { return this.PartitionKey; }

        public KarmaPerson() { }

        public string Name { get; set; }

        public string FirstName { get; set; } // TODO: dont store this.

        public string PictureUrl { get; set; }

        public string Location { get; set; }

        public string Email { get; set; }

        public string NonKarmaFriends { get; set; }

        public string KarmaFriends { get; set; }

        public int KarmaPoints { get; set; }

        internal IEnumerable<string> ReadKarmaFriends()
        {
            if (!string.IsNullOrEmpty(KarmaFriends))
            {
                return KarmaFriends.Split(',');
            }
            else
                return new List<string>();
        }

        internal bool AddKarmaFriend(string newFriend)
        {
            var existing = ReadKarmaFriends();
            if (existing.Contains(newFriend))
            {
                Debug.WriteLine("For {0} Cannot Add:({1}), because already exists in ({2})", FacebookId(), newFriend, KarmaFriends);
                return false;
            }
            if (string.IsNullOrEmpty(KarmaFriends))
            {
                KarmaFriends = newFriend;
            }
            else
            {
                KarmaFriends += "," + newFriend;
            }
            return true;
        }
    }

    public class DbKarmaRequest : TableEntity
    {
        const char SEPERATOR = '!';
        public DbKarmaRequest(string userid, decimal lat, decimal lang, string strLocation, string title, string message, string closedateUTC)
        {
            this.PartitionKey = userid;

            // generate a unique request id.
            this.RowKey = DateTime.UtcNow.ToString("s", CultureInfo.InvariantCulture); // someting like  2008-04-10T06:30:00

            this.Latitute = lat;
            this.Longitude = lang;
            this.Title = title;
            this.MoreInfo = message;
            this.State = RequestState.created;
            this.CloseDateUTC = closedateUTC;
        }

        public string GetUserId() { return this.PartitionKey; }

        public string GetRequestId() { return this.PartitionKey + SEPERATOR + this.RowKey; } // use utc?
        
        public static bool GetKeys(string requestId, out string partitionKey, out string rowKey)
        {
            partitionKey = string.Empty;
            rowKey = string.Empty;
            var keys = requestId.Split(SEPERATOR);
            if (keys.Length == 2)
            {
                partitionKey = keys[0];
                rowKey = keys[1];
                return true;
            }
            return false;
        }

        public Decimal Latitute { get; set; }

        public Decimal Longitude{ get; set; }
        public string  LocationName {get;set;}

        public string CloseDateUTC {get; set;} 

        public enum RequestState
        {
            created,
            opening,
            opened,
            intransitPatial,    // has been send to some friends, some friends remains
            intransitFull,       // has been sent to all friends. no friends remaining.
            offered,
            accepted,
            failed,
            closed
        }
        // new  - created
        // open - added the list of friends to whom the request can be sent
        // sent - 


        public RequestState State { get; set; }

        // I Need: xyz
        public string Title { get; set; }

        public string MoreInfo {get;set;}

        public string SendTo { get; set; }

        public string SeenBy { get; set; }

        public string ShowTo { get; set; }

        public string OfferedBy { get; set; }

        public string AcceptedFrom { get; set; }
    }


} //KarmaWebApp