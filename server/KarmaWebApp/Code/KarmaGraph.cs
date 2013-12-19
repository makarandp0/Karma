using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;

namespace KarmaWebApp.Code.Graph
{
    public class KarmaGraphNode<T>
    {
        private T _me;

        public List<KarmaGraphNode<T>> Friends = new List<KarmaGraphNode<T>>();

        public KarmaGraphNode(T value)
        {
            _me = value;
        }

        public T GetValue() { return _me; }
    }

    public class KarmaGraph<T> : Dictionary<string, KarmaGraphNode<T>>
    {
        public KarmaGraphNode<T> AddNode(string key, T value)
        {
            var node = new KarmaGraphNode<T>(value);
            this.Add(key, node);
            return node;
        }

        /// <summary>
        /// establishs friends links. parameter oneWay Specifies
        /// if only one way entry would be established.
        /// when creating database for the 1st time, its easier
        /// to use oneway entries as going thru all nodes.
        /// but once graph is fully setup, when adding any additional 
        /// nodes, its better to setup two way entries.
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="twoWay"></param>
        /// <returns></returns>
        public bool AddLink(string from, string to, bool twoWay)
        {
            KarmaGraphNode<T> fromNode;
            KarmaGraphNode<T> toNode;
            if (!base.TryGetValue(from, out fromNode))
            {
                Debug.Write("From node not found:" + from);
                return false;
            }

            if (!base.TryGetValue(to, out toNode))
            {
                Debug.Write("To node not found:" + to);
                return false;
            }

            fromNode.Friends.Add(toNode);

            if (twoWay)
            {
                toNode.Friends.Add(fromNode);
            }

            return true;
        }

        public bool RemoveFriendShip(string from, string to)
        {
            KarmaGraphNode<T> fromNode;
            KarmaGraphNode<T> toNode;
            if (!base.TryGetValue(from, out fromNode))
            {
                Debug.Write("From node not found:" + from);
                return false;
            }

            if (!base.TryGetValue(to, out toNode))
            {
                Debug.Write("To node not found:" + to);
                return false;
            }

            fromNode.Friends.Remove(toNode);
            toNode.Friends.Remove(fromNode);
            return true;
        }


        internal bool TryGetValue(string facebookid, out T person)
        {
            KarmaGraphNode<T> graphNode;
            if (!base.TryGetValue(facebookid, out graphNode))
            {
                person = default(T);
                return false;
            }

            person = graphNode.GetValue();
            return true;
        }
    }
}