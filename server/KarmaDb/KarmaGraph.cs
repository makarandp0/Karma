using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KarmaDb;
using System.Diagnostics;
using KarmaDb.Types;
using KarmaWeb.Utilities;
using KarmaGraph.Types;

namespace KarmaGraph
{
    public class Graph : KarmaObject
    {
        private KarmaDb.KarmaDb _Database;
        private GraphRoot _Root = new GraphRoot();

        void SetDatabase(KarmaDb.KarmaDb database)
        {
            this._Database = database;
        }

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
            List<DbUserExtended> userExtended;
            List<DbGroup> groups;
            List<DbRequest> requests;

            if (!this._Database.ReadAll(out userBasic, out userExtended, out groups, out requests))
            {
                this.Logger.Error("_Database.ReadAll")
            }

            // add all users to graph
            foreach (var user in userBasic)
            {
                _Root.Users.Add(user.fbId, KarmaUser.FromDbRequest(user));
            }

            // add all groups and their members.
            foreach (var group in groups)
            {
                var graphGroup = KarmaGroup.FromDbGroup(group);
                _Root.Groups.Add(group.fbId, graphGroup);
                
                FillListWithUsers(graphGroup.members, ListUtils.ListFromCSV(group.members));
            }


            // setup friends, blocked friends links.
            foreach (var user in userBasic)
            {
                var graphUser = _Root.Users[user.fbId];
                
                FillListWithUsers(graphUser.friends, ListUtils.ListFromCSV(user.karmaFriends));

                FillListWithUsers(graphUser.blockedFriends, ListUtils.ListFromCSV(user.blockedFriends));

                // TODO setup groups for this user.
                FillListWithGroups(graphUser.memberofGroups, ListUtils.ListFromCSV(user.groups));

            }

            // add all requests, and setup inbox/outboxes
            foreach (var request in requests)
            {
                var graphRequest = KarmaRequest.FromDB(request);
                _Root.Requests.Add(request.requestId, graphRequest);

                KarmaUser graphUser;
                if (_Root.Users.TryGetValue(request.createdBy, out graphUser))
                {
                   graphRequest.from = graphUser;
                }
                
                FillListWithUsers(graphRequest.delieverTo, ListUtils.ListFromCSV(request.delieverTo));
                FillListWithUsers(graphRequest.delieveredTo, ListUtils.ListFromCSV(request.delieveredTo));
                FillListWithUsers(graphRequest.acecptedFrom, ListUtils.ListFromCSV(request.offerAccepted));
                FillListWithUsers(graphRequest.ignoredFrom, ListUtils.ListFromCSV(request.offersIgnored));
            }

            this.Logger.Info("GenerateGraph:Created Nodes:" + _Root.Users.Count);
        }

        private int FillListWithGroups(List<KarmaGroup> list, IEnumerable<string> groupIds)
        {
            int error = 0;
            foreach (var groupId in groupIds)
            {
                KarmaGroup graphGroup;
                if (_Root.Groups.TryGetValue(groupId, out graphGroup))
                {
                    list.Add(graphGroup);
                }
                else
                {
                    this.Logger.Warn("Group not found:" + groupId);
                    error++;
                }
            }
            return error;
        }

        private int FillListWithUsers(List<KarmaUser> list, IEnumerable<string> userIds)
        {
            int error = 0;
            foreach (var userid in userIds)
            {
                KarmaUser graphUser;
                if (_Root.Users.TryGetValue(userid, out graphUser))
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

    }
}
