using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Web;

namespace KarmaWebApp.Code
{
    public class KarmaBackgroundWorker
    {
        private Thread     _Backgroundworker;
        private CloudQueue _WorkerQueue;

        
        private static readonly TimeSpan ONE_DAY = new TimeSpan(1, 0, 0, 0);
        private static readonly TimeSpan ONE_MINUTE = new TimeSpan(0, 1, 0);
        private static readonly TimeSpan FIVE_MINUTES = new TimeSpan(0, 5, 0);
        private static readonly TimeSpan TIME_FOR_MESSAGE_TO_LIVE = ONE_DAY;
        private static readonly TimeSpan INITIAL_INVISIBILITY_TIME = FIVE_MINUTES;
        private static readonly TimeSpan TIME_TO_PROCESS_MESSAGE = ONE_MINUTE;

        private static AutoResetEvent _workAvailableEvent = new AutoResetEvent(false);
        
        public delegate void WorkItemDelegate(string workId);

        private Dictionary<string, WorkItemDelegate> _WorkItems = new Dictionary<string, WorkItemDelegate>();
        
        /// <summary>
        /// registers functions for processing work items.
        /// </summary>
        /// <param name="workType"></param>
        /// <param name="workItemDelegate"></param>
        public void RegisterWorkItem(string workType, WorkItemDelegate workItemDelegate)
        {
            _WorkItems.Add(workType, workItemDelegate);
        }


        public KarmaBackgroundWorker(CloudStorageAccount storageAccount)
        {
            var queueClient = storageAccount.CreateCloudQueueClient();
            _WorkerQueue = queueClient.GetQueueReference("workqueue");
            _WorkerQueue.CreateIfNotExists();

            _Backgroundworker = new Thread(this.DoWork);
        }


        string EncodeMessage(string workType, string workId)
        {
            Debug.Assert(!workType.Contains('+') && !workId.Contains('+'));
            return workType + '+' + workId;
        }
        string DecodeMessage(string message, out string workId)
        {
            Debug.Assert(message.Contains('+'));
            var parts = message.Split('+');
            Debug.Assert(parts.Length == 2);
            workId = parts[1];
            return parts[0];
        }

        public void DoWork()
        {
            while (true)
            {
                // TODO: when receiving queued message, add logic for poison message 
                // that is message that gets dequeued and causes the app to crash. 
                // it causes such message to go back to queue before it could be deleted by whoever processed it.

                var message = _WorkerQueue.GetMessage(TIME_TO_PROCESS_MESSAGE);
                if (message != null)
                {
                    var msgString = message.AsString;

                    string workId;
                    var msgType = DecodeMessage(msgString, out workId);
                    if (ProcessMessage(msgType, workId))
                    {
                        _WorkerQueue.DeleteMessage(message);
                        
                    }
                }
                else
                {
                    // wait for new work to be available.
                    _workAvailableEvent.WaitOne();
                }
            }
        }

        private bool ProcessMessage(string msgType, string workId)
        {
            throw new NotImplementedException();
        }

        
        internal bool BroadCastWorkItem(string workType, string workId)
        {
            //
            // TODO: connect to all instances and broadcast this work item.
            // for now simply use the queue.
            return QueueWorkItem(workType, workId);
        }

        internal bool QueueWorkItem(string workType, string workId, TimeSpan? showAfter = null)
        {

            // make sure we understand the work item.
            try
            {
                //
                // TODO: make this async.
                //
                var queueMsg = new CloudQueueMessage(EncodeMessage(workType, workId));
                if (showAfter == null)
                {
                    showAfter = INITIAL_INVISIBILITY_TIME;
                }

                TimeSpan timeToLive = TIME_FOR_MESSAGE_TO_LIVE + TIME_FOR_MESSAGE_TO_LIVE;

                _WorkerQueue.AddMessage(queueMsg, timeToLive, showAfter);

                // notify all instances that new message is available.
                NotifyMessageAvailability();
                return true;
            }
            catch(Exception ex)
            {
                Logger.WriteLine("Failed to add work item to the queue:" + ex);
            }

            return false;
        }

        private void NotifyMessageAvailability()
        {
            // TODO: this method notifies all instances of
            // same role about availability of new message in queue.
            // for now just signal an event.
            _workAvailableEvent.Set();
        }
    }
}