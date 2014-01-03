using KarmaDb.Types;
using KarmaGraph.Types;
using KarmaWeb.Utilities;
using KarmaWebApp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;

namespace KarmaGraph
{
    public class Graph : KarmaObject
    {
        #region Private Methods
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

        private int FillListWithUsers(List<KarmaUser> list, IEnumerable<string> userIds)
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

        private KarmaDb.KarmaDb _Database;
        private Dictionary<string, KarmaUser> Users = new Dictionary<string, KarmaUser>();
        private Dictionary<string, KarmaGroup> Groups = new Dictionary<string, KarmaGroup>();
        private Dictionary<string, KarmaRequest> Requests = new Dictionary<string, KarmaRequest>();

        public void SetDatabase(KarmaDb.KarmaDb database)
        {
            this._Database = database;
        }

        /// <summary>
        /// generates graph from database.
        /// </summary>
        public void Generate()
        {
            this.Logger.Info("Generating Graph");
            Debug.Assert(_Database != null);

            //
            // now ask database to read all the data async
            // and setup the delegates to update the graph as data comes in.
            // TODO: make this async. so that we can populate the graph as 
            // data comes in.
            List<DbUserBasic> userBasic;
            List<DbGroup> groups;
            List<DbRequest> requests;

            if (!this._Database.ReadAll(out userBasic, out groups, out requests))
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
                var graphRequest = KarmaRequest.FromDB(request);
                this.Requests.Add(request.requestId, graphRequest);

                KarmaUser graphUser;
                if (this.Users.TryGetValue(request.createdBy, out graphUser))
                {
                    graphRequest.from = graphUser;
                }

                FillListWithUsers(graphRequest.delieverTo, ListUtils.ListFromCSV(request.delieverTo));
                FillListWithUsers(graphRequest.delieveredTo, ListUtils.ListFromCSV(request.delieveredTo));
                FillListWithUsers(graphRequest.acecptedFrom, ListUtils.ListFromCSV(request.offerAccepted));
                FillListWithUsers(graphRequest.ignoredFrom, ListUtils.ListFromCSV(request.offersIgnored));
            }

            this.Logger.Info("GenerateGraph:Created Nodes:" + this.Users.Count);
        }



        public KarmaUser GetUser(string fbId)
        {
            KarmaUser user = null;
            if (this.Users.TryGetValue(fbId, out user))
                return user;
            return null;
        }

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
            List<KarmaGroup> existingGraphGroups = new List<KarmaGroup>();
            foreach (var group in client.FbGroups)
            {
                KarmaGroup graphGroup = null;
                if (this.Groups.TryGetValue(group.Id, out graphGroup))
                {
                    existingGraphGroups.Add(graphGroup);
                }
                else
                {
                    var newDBGroup = new DbGroup(group.Id);
                    newDBGroup.name = group.Name;
                    newDBGroup.flags = 0;
                    newGroups.Add(newDBGroup);
                }
            }

            // write the new information to database.
            // now create the bunch of objects that we have created.
            // need to check for error values.
            // TODO: check for return values.
            _Database.InsertOrMerge(userBasic);


            _Database.InsertOrMerge(userExtended);
            foreach (var dbGroup in newGroups)
            {
                _Database.InsertOrMerge(dbGroup);

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

            // TODO setup groups for this user.
            UpdateGroupMembers(graphUser, ListUtils.ListFromCSV(userBasic.groups));

            // and finally update friends to link back to this new user.
            foreach (var friend in graphUser.friends)
            {
                friend.friends.Add(graphUser);
            }

            // add the user to the graph.
            return graphUser;
        }

        internal string  CreateRequest(string userid, decimal lat, decimal lang, string strLocation, string subject, string message, string closedateUTC)
        {
            throw new NotImplementedException();
        }
    }
}