using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KarmaWebApp.Code
{
    /// <summary>
    /// this class encapsulates the services offered by Web Backend.
    /// This can be used by all clients (mobile,web) of this service. 
    /// </summary>
    public interface IKarmaWebBackEnd
    {
        /// <summary>
        ///  to access services specific to a user clients must create a KarmaWebUser Object
        ///  this object is created by providing fb accessToken.
        /// </summary>
        /// <param name="accessToken">Token obtained from facebook</param>
        /// <param name="clientid">Unique string identifying client</param>
        /// <returns></returns>
        IKaramActiveUser LogonUserUsingFB(string accessToken);
    }

    /// <summary>
    ///  reprensets a karma user. A specialized version KaramActiveUser is derived from this.
    /// </summary>
    public interface IKarmaUser
    {
        string FacebookId { get; }

        string Name { get; }

        string FirstName { get; }

        string PictureUrl { get; }

        string Location { get; }

        Gender Gender { get; }

        int karmaPoints { get; }
    }

    /// <summary>
    /// represents You - a logged on user.
    /// </summary>
    public interface IKaramActiveUser : IKarmaUser
    {
        /// <summary>
        /// returns freiends that are signed up for the app.
        /// </summary>
        /// <returns></returns>
        List<IKarmaFriend> Friends();

        /// <summary>
        /// create a new request
        /// </summary>
        /// <returns></returns>
        IKarmaRequest CreateRequest(string subject, string message, DateTime closedate);

        /// <summary>
        /// list of open requests
        /// </summary>
        List<IMyRequest> MyOpenRequests { get; }

        /// <summary>
        /// list of all requests
        /// TODO: need to add a filter param to limit the output.
        /// </summary>
        List<IMyRequest> MyAllRequests { get; }

        /// <summary>
        /// lists all friends requests.
        /// </summary>
        List<ITheirRequest> FriendsRequests { get; }

        /// <summary>
        /// retrives details of the push notification given the id.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="id"></param>
        /// <returns></returns>
        T GetNotificationDetails<T>(string id);

    }

    public interface IKarmaFriend : IKarmaUser
    {
        bool IsBlocked();

        /// <summary>
        /// you can block a friend if you dont want to see any requests from her.
        /// </summary>
        /// <param name="friend"> friend to block/unblock</param>
        /// <param name="block"> if true friend is blocked, false to unblock</param>
        void BlockFriend(IKarmaUser friend, bool block);

    }


    /// <summary>
    ///  represents a Request. The specilized versions "TheirRequest" "MyRequest" are derived from this.
    /// </summary>
    public interface IKarmaRequest
    {
        string Id { get; }
        string Subject { get; }
        string Message { get; }
        DateTime DueDate { get; }
        bool IsOpen { get; }
        IKarmaUser CreatedBy { get; }
    }

    /// <summary>
    /// represents a request created by me.
    /// </summary>
    public interface IMyRequest : IKarmaRequest
    {
        List<IKarmaFriend> HelpOffers { get; }
        void AcceptOffer(IKarmaFriend friend);
        void IgnoreOffer(IKarmaFriend friend);
        void CloseRequest();
        void ReopenRequest();
    }


    /// <summary>
    /// represents a request created by friend.
    /// </summary>
    public interface ITheirRequest : IKarmaRequest
    {
        void OfferHelp();
        void Ignore();
    }

    public enum KarmaNotificationType { FriendJoined, HelpRequest, HelpOffer, HelpOfferAccepted }
    /// <summary>
    ///  basic information that will be available in push notifications that service sends.
    /// </summary>
    public interface IKarmaNotification
    {
        // notification can be of different types


        KarmaNotificationType notifyType { get; }

        string Id { get; }

        string Subject { get; }
    }

    public interface IKarmaHelpOfferNotification : IKarmaNotification
    {
        IMyRequest Request { get; }
        IKarmaFriend HelpOfferedBy { get; }
    }

    public interface IKarmaHelpOfferAcceptedNotification : IKarmaNotification
    {
        ITheirRequest Request { get; }
        IKarmaFriend OfferAcceptedBy { get; }
    }

    public interface IKarmaHelpRequestNotification : IKarmaNotification
    {
        ITheirRequest Request { get; }
    }

    public interface IKarmaFriendJoinedNotification : IKarmaNotification
    {
        IKarmaFriend Joined { get; }
    }

}