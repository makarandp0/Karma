using KarmaDb.Types;
using KarmaGraph.Types;
using KarmaWeb.Utilities;
using KarmaWebApp;
using KarmaWebApp.Code;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;

namespace KarmaGraph
{
    public class Graph : KarmaObject
    {
        private KarmaDb.KarmaDb Database { get; set; }
        public Dictionary<string, KarmaUser> Users {get;private set;}
        public Dictionary<string, KarmaGroup> Groups { get; private set; }
        public Dictionary<string, KarmaRequest> Requests { get; private set; }
        public KarmaBackgroundWorker Worker { get; set; }

        // once graph is ready to accept background work, it setups the worker.
        private void SetBackgroundWorker(KarmaBackgroundWorker KarmaBackgroundWorker)
        {
            this.Worker = KarmaBackgroundWorker;
            this.Worker.RegisterWorkItem("DelayedTask_UpdateFriends", new KarmaBackgroundWorker.WorkItemDelegate(DelayedTask_UpdateFriends));
            this.Worker.RegisterWorkItem("DelayedTask_ProcessRequest", new KarmaBackgroundWorker.WorkItemDelegate(DelayedTask_ProcessRequest));
            this.Worker.RegisterWorkItem("BroadCast_AddToGraph", new KarmaBackgroundWorker.WorkItemDelegate(BroadCast_AddToGraph));
        }

        private void BroadCast_AddToGraph(string workId)
        {
            throw new NotImplementedException();
        }

        private void DelayedTask_ProcessRequest(string workId)
        {
            throw new NotImplementedException();
        }


        public Graph()
        {
            this.Users = new Dictionary<string, KarmaUser>();
            this.Groups = new Dictionary<string, KarmaGroup>();
            this.Requests = new Dictionary<string, KarmaRequest>();
        }

        #region Private Methods
        private void AddRequestToGraph(DbRequest request)
        {
            var graphRequest = KarmaRequest.FromDB(request, this);
            this.Requests.Add(request.requestId, graphRequest);

            // update appropriate inboxes and outboxes for this request.
            graphRequest.from.outbox.Add(graphRequest);
            foreach (var to in graphRequest.delieverTo)
            {
                if (graphRequest.from.HasBlocked(to) || to.HasBlocked(graphRequest.from))
                {
                    Logger.Info("request was not added to inbox, because users dont trust each other.");
                }
                else
                {
                    to.inbox.Add(graphRequest);
                }
            }
        }

        private void UpdateGroupMembers(KarmaUser graphUser, IEnumerable<string> groupIds)
        {
            int error = 0;
            foreach (var groupId in groupIds)
            {
                KarmaGroup graphGroup;
                if (this.Groups.TryGetValue(groupId, out graphGroup))
                {
                    graphUser.memberofGroups.Add(graphGroup);
                    graphGroup.members.Add(graphUser);
                }
                else
                {
                    this.Logger.Warn("Group not found:" + groupId);
                    error++;
                }
            }
        }

        public static string FillStringWithUsers(List<KarmaUser> users)
        {
            string result = string.Empty;
            foreach (var u in users)
            {
                if (!string.IsNullOrEmpty(result))
                {
                    result += ",";
                }
                result += u.id;
            }
            return result;
        }

        public int FillListWithUsers(List<KarmaUser> list, IEnumerable<string> userIds)
        {
            int error = 0;
            foreach (var userid in userIds)
            {
                KarmaUser graphUser;
                if (this.Users.TryGetValue(userid, out graphUser))
                {
                    list.Add(graphUser);
                }
                else
                {
                    this.Logger.Warn("User not found:" + userid);
                    error++;
                }
            }
            return error;
        }
        #endregion


        /// <summary>
        /// generates graph from database.
        /// </summary>
        public void Generate(KarmaDb.KarmaDb database, KarmaBackgroundWorker worker)
        {
            this.Logger.Info("Generating Graph");
            this.Database = database;
            //
            // now ask database to read all the data async
            // and setup the delegates to update the graph as data comes in.
            // TODO: make this async. so that we can populate the graph as 
            // data comes in.
            List<DbUserBasic> userBasic;
            List<DbGroup> groups;
            List<DbRequest> requests;

            if (!this.Database.ReadAll(out userBasic, out groups, out requests))
            {
                this.Logger.Error("_Database.ReadAll");
            }

            // add all groups to the graph.
            foreach (var group in groups)
            {
                var graphGroup = KarmaGroup.FromDbGroup(group);
                this.Groups.Add(group.fbId, graphGroup);
            }

            // add all users to graph
            foreach (var user in userBasic)
            {
                this.Users.Add(user.fbId, KarmaUser.FromDbUser(user));
            }

            // setup friends, blocked friends links.
            foreach (var user in userBasic)
            {
                var graphUser = this.Users[user.fbId];

                FillListWithUsers(graphUser.friends, ListUtils.ListFromCSV(user.karmaFriends));

                FillListWithUsers(graphUser.blockedFriends, ListUtils.ListFromCSV(user.blockedFriends));

                // TODO setup groups for this user.
                UpdateGroupMembers(graphUser, ListUtils.ListFromCSV(user.groups));
            }

            // add all requests, and setup inbox/outboxes
            foreach (var request in requests)
            {
                AddRequestToGraph(request);
            }

            SetBackgroundWorker(worker);

            this.Logger.Info("GenerateGraph:Created Nodes:" + this.Users.Count);
        }



        public KarmaUser GetUser(string fbId)
        {
            KarmaUser user = null;
            if (this.Users.TryGetValue(fbId, out user))
                return user;
            return null;
        }

        /// <summary>
        /// Creates a new user account using supplied facebook data.
        /// 1. It creates entries in database for the user and any groups that user belongs to.
        /// 2. It updates the user entry to the graph.
        /// 3. It upddates links between friends and user.
        /// 4. sets up a delayed task to update friends records.
        /// </summary>
        /// <param name="client"></param>
        /// <returns></returns>
        internal KarmaUser CreateUser(KaramFacebookUser client)
        {
            var userBasic = new DbUserBasic(client.FacebookId);

            // basic info.
            userBasic.name = client.Name;
            userBasic.firstname = client.FirstName;
            userBasic.gender = GenderUtil.ToDbGender(client.Gender);
            userBasic.pic = client.PictureUrl;
            userBasic.email = client.Email;
            LocationUtil.ToDbLocation(client.location, userBasic);

            // karma friends.
            bool bFirst = true;
            userBasic.karmaFriends = "";
            foreach (var fbFriend in client.FbFriends)
            {
                if (fbFriend.IsKarmaUser)
                {
                    if (!bFirst)
                    {
                        userBasic.karmaFriends += ",";;
                    }
                    bFirst = false;
                    userBasic.karmaFriends += fbFriend.FacebookId;
                }
            }

            userBasic.userflags = 0; // to start with!

            var userExtended = new DbUserExtended(client.FacebookId);
            userExtended.accessToken = client.AccessToken;

            userExtended.nonkarmaFriends = "";
            bFirst = true;
            foreach (var fbFriend in client.FbFriends)
            {
                if (!fbFriend.IsKarmaUser)
                {
                    if (!bFirst)
                    {
                        userExtended.nonkarmaFriends += ","; ;
                    }
                    bFirst = false;
                    userExtended.nonkarmaFriends += fbFriend.FacebookId;
                }
            }

            // now look at the groups.
            List<DbGroup> newGroups = new List<DbGroup>();
            bFirst = true;
            foreach (var group in client.FbGroups)
            {
                if (!bFirst)
                {
                    userBasic.groups += ",";
                }
                bFirst = false;
                userBasic.groups += group.Id;

                KarmaGroup graphGroup = null;
                if (!this.Groups.TryGetValue(group.Id, out graphGroup))
                {
                    var newDBGroup = new DbGroup(group.Id);
                    newDBGroup.name = group.Name;
                    newDBGroup.flags = 0;
                    newGroups.Add(newDBGroup);
                }
            }
            
            // when we add user to database, we must also add his friends entries in database to link to the friend.
            // however we dont want to do this sync. Create a background task for this.
            if (this.Worker.QueueWorkItem("DelayedTask_UpdateFriends", client.FacebookId))
            {
                // write the new information to database.
                // now create the bunch of objects that we have created.
                // need to check for error values.
                // TODO: check for return values.
                foreach (var dbGroup in newGroups)
                {
                    Database.InsertOrReplace(dbGroup);
                }

                Database.InsertOrReplace(userBasic);
                Database.InsertOrReplace(userExtended);
            }

            // once done with database additions
            // udpate the graph.

            // 1st add new groups to the graph.
            foreach (var dbGroup in newGroups)
            {
                this.Groups.Add(dbGroup.fbId, KarmaGroup.FromDbGroup(dbGroup));
            }
            
            var graphUser = KarmaUser.FromDbUser(userBasic);

            this.Users.Add(graphUser.id, graphUser);
            FillListWithUsers(graphUser.friends, ListUtils.ListFromCSV(userBasic.karmaFriends));
            FillListWithUsers(graphUser.blockedFriends, ListUtils.ListFromCSV(userBasic.blockedFriends));

            // setup links in graph for the user's groups.
            UpdateGroupMembers(graphUser, ListUtils.ListFromCSV(userBasic.groups));

            // and finally update friends to link back to this new user.
            foreach (var friend in graphUser.friends)
            {
                friend.friends.Add(graphUser);
            }

            // add the user to the graph.
            return graphUser;
        }

        // this task is setup to update the friends records when a user is created.
        // graph must be already updated by other means.
        private void DelayedTask_UpdateFriends(string userId)
        {
            var graphUser = GetUser(userId);
            if (graphUser != null)
            {
                foreach (var friend in graphUser.friends)
                {
                    // read friend from database.
                    // update the friend entry to add new friend.
                    var dbFriend = this.Database.ReadUserBasic(friend.id);
                    if (!string.IsNullOrEmpty(dbFriend.karmaFriends))
                    {
                        dbFriend.karmaFriends += ",";
                    }
                    dbFriend.karmaFriends += userId;
                    this.Database.InsertOrReplace(dbFriend);
                }
            }
        }

        /// <summary>
        /// creates a new request, and adds it to graph.
        /// it also sets up delayed task to "process" this request.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="location"></param>
        /// <param name="subject"></param>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        internal KarmaRequest CreateRequest(KarmaUser user, Location location, string subject, KarmaDate dateTime)
        {
            var dbRequest = new DbRequest(user.id);
            dbRequest.title = subject;
            dbRequest.dueDate = dateTime.ToDBDate();
            LocationUtil.ToDbLocation(location, dbRequest);

            // setup delievery
            var friendsNearBy = GetNearByFriends(user, location.lat, location.lan);
            dbRequest.delieverTo = FillStringWithUsers(friendsNearBy);

            var result = Database.InsertOrReplace(dbRequest);
            AddRequestToGraph(dbRequest);

            return KarmaRequest.FromDB(dbRequest,this);
        }


        /// <summary>
        /// computes nearby friends
        /// </summary>
        /// <param name="user"></param>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <returns></returns>
        private List<KarmaUser> GetNearByFriends(KarmaUser user, double lat, double lan)
        {
            var nearbyFriends = new List<KarmaUser>();

            // for now return all friends.
            foreach (var friend in user.friends)
            {
                nearbyFriends.Add(friend);
            }
            return nearbyFriends;
        }

        /// <summary>
        /// called when a users offers or denies help. 
        /// </summary>
        /// <param name="karmaUser"></param>
        /// <param name="requestId"></param>
        /// <param name="offered"></param>
        /// <returns></returns>
        internal bool OfferHelp(KarmaUser karmaUser, string requestId, bool offered)
        {
            // lookup the request id.
            foreach (var request in karmaUser.inbox)
            {
                // check if such request exists.
                if (string.Compare(request.requestId, requestId, true) == 0)
                {
                    if (offered)
                    {
                        request.offeredBy.Add(karmaUser);
                        request.ignoredBy.Remove(karmaUser);
                    }
                    else
                    {
                        request.offeredBy.Remove(karmaUser);
                        request.ignoredBy.Add(karmaUser);
                    }

                    SaveRequestToDatabase(request);
                    return true;
                }
            }

            // request was not found in inbox.
            return false;
        }

        internal bool AcceptHelp(KarmaUser karmaUser, string requestId, string offerFrom, bool accepted)
        {
            foreach (var request in karmaUser.outbox)
            {
                if (string.Compare(request.requestId, requestId, true) == 0)
                {
                    foreach(var offerer in request.offeredBy)
                    {
                        if (string.Compare(offerer.id, offerFrom) == 0)
                        {
                            // this is the offer we are accepting.
                            if (accepted)
                            {
                                request.acecptedFrom.Add(offerer);
                                request.ignoredFrom.Remove(offerer);
                            }
                            else
                            {
                                request.acecptedFrom.Remove(offerer);
                                request.ignoredFrom.Add(offerer);
                            }

                            // write the request to database.
                            SaveRequestToDatabase(request);
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// this functions should saves the given request to database.
        /// </summary>
        /// <param name="request"></param>
        private void SaveRequestToDatabase(KarmaRequest request)
        {
            // read DbRequst from database.
            var dbRequest = this.Database.ReadRequest(request.requestId);
            // update with the changes.
            request.UpdateDbRequest(dbRequest);
            // save to database
            this.Database.InsertOrReplace(dbRequest);
        }
    }
}