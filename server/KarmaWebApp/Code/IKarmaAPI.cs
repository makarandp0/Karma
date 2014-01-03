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

    /// <summary>
    /// represents You - a logged on user.
    /// </summary>
    public interface IKaramActiveUser
    {
        string CreateRequest(string Title, string p, DateTime dateTime);
    }

}