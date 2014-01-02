using Facebook;
using KarmaWebApp.Code;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;

namespace KarmaWebApp
{

    public class FacebookFriend
    {
        public string FacebookId {get; private set;}
        public string Name{get; private set;}
        public bool IsKarmaUser { get; private set; }

        public FacebookFriend(dynamic friend)
        {
            this.FacebookId = friend.id;
            this.Name = friend.ContainsKey("name") ? friend.name : "Unknown";
            this.IsKarmaUser = friend.ContainsKey("installed") ? friend.installed : false;
        }
    }

    public class FacebookGroup
    {
        public string Id {get;private set;}
        public string Name { get; private set; }

        public FacebookGroup(dynamic group)
        {
            this.Id = group.id;
            this.Name = group.name;
        }
    }


    public class KaramFacebookUser 
    {
        private FacebookClient _facebookClient;
        public KaramFacebookUser(string accessToken)
        {
            this._facebookClient = new FacebookClient(accessToken);
            this.FbFriends = new List<FacebookFriend>();
            this.FbGroups = new List<FacebookGroup>();
        }
        
        public string  FacebookId {get; private set;}
        public string Name { get; private set; }
        public string FirstName { get; private set; }
        public string PictureUrl { get; private set; }
        public List<FacebookFriend> FbFriends { get; private set; }
        public List<FacebookGroup>  FbGroups {get;private set;}
        public string Location { get; private set; }
        public string Email { get; private set; }
        public Gender Gender { get; private set; }

        public bool ValidateUser()
        {
            try
            {
                if (String.IsNullOrEmpty(FacebookId))
                {
                    this.Gender = Code.Gender.Unknown;
                    //dynamic meResults = _facebookClient.Get("me", new { fields = "name,first_name,id,picture,location,email,gender" });
                    dynamic meResults = _facebookClient.Get("me", new { fields = "name,first_name,id,picture,location,email,gender" });
                    this.FacebookId = meResults.id;
                    this.FirstName = meResults.first_name;
                    this.Name = meResults.ContainsKey("name") ? meResults.name : "Unknown";
                    this.PictureUrl = meResults.ContainsKey("picture") ? meResults.picture.data.url : null;
                    this.Location = meResults.ContainsKey("location") ? meResults.location.name : "unknown";
                    this.Email = meResults.ContainsKey("email") ? meResults.email : "unknown";
                    if (meResults.ContainsKey("gender"))
                    {
                        if (String.Compare(meResults.gender, "female", true) == 0)
                        {
                            this.Gender = Code.Gender.Female;
                        }
                        else if (String.Compare(meResults.gender, "male", true) == 0)
                        {
                            this.Gender = Code.Gender.Male;
                        }
                    }
                        
                }
                Logger.WriteLine("Facebook: Read Basic Information for:" + this.FacebookId);
                return true;
            }
            catch (Exception ex)
            {
                Logger.WriteLine("Exception in ReadBasicInfo:" + ex);
                return false;
            }
        }

        public bool ReadExtendedInformation()
        {
            try
            {
                dynamic result = _facebookClient.Batch(
                    new FacebookBatchParameter("me", new { fields = "name,first_name,id,picture,location,email,gender" }),
                    new FacebookBatchParameter("me/friends", new { fields = "id,name,installed" }),
                    new FacebookBatchParameter("me/groups", new { fields = "id,name" })
                    );

                if (result[0] is Exception)
                {
                    var ex = (Exception)result[0];
                    // handle exception
                    Logger.WriteLine("result[0] is Exception in ReadExtendedInformation:" + ex);
                }

                if (result[1] is Exception)
                {
                    var ex = (Exception)result[1];
                    // handle exception
                    Logger.WriteLine("result[1] is Exception in ReadExtendedInformation:" + ex);
                }
                else
                {
                    this.FacebookId = result[0].id;
                    this.FirstName = result[0].ContainsKey("first_name") ? result[0].first_name : "Unknown";
                    this.Name = result[0].ContainsKey("name") ? result[0].name : "Unknown";
                    this.PictureUrl = result[0].ContainsKey("picture") ? result[0].picture.data.url : null;
                    this.Location = result[0].ContainsKey("location") ? result[0].location.name : "unknown";
                    this.Email = result[0].ContainsKey("email") ? result[0].email : "unknown";
                    this.Gender = result[0].ContainsKey("gender") ? result[0].gender : "female"; 

                    foreach (var frienddata in result[1].data)
                    {
                        this.FbFriends.Add(new FacebookFriend(frienddata));
                    }

                    if (result[2] is Exception)
                    {
                        var ex = (Exception)result[2];
                        Logger.WriteLine("result[2] is Exception in ReadExtendedInformation:" + ex);
                    }
                    else
                    {
                        if (result[2].ContainsKey("data"))
                        {
                            foreach (var group in result[2].data)
                            {
                                this.FbGroups.Add(new FacebookGroup(group));
                            }
                        }
                    }

                    Logger.WriteLine("Facebook: Read ReadExtendedInformation for:" + this.FacebookId);
                    return true;
                }
            }
            catch (FacebookApiException ex)
            {
                Logger.WriteLine("FacebookApiException in ReadExtendedInformation:" + ex);
            }
            catch (Exception ex)
            {
                Logger.WriteLine("Exception in ReadExtendedInformation:" + ex);
            }
            return false;
        }
    }
}

/*
 * me/groups?fields=name,id
{
  "data": [
    {
      "name": "Indians_In_Seattle", 
      "id": "9816235049"
    }, 
    {
      "name": "Seattle KVO", 
      "id": "451787661507429"
    }, 
    {
      "name": "Family", 
      "id": "316322418384169"
    }, 
    {
      "name": "Family", 
      "id": "180674795316830"
    }, 
    {
      "name": "ALL Friends", 
      "id": "120231981383002"
    }, 
    {
      "name": "Rivertrail Residents", 
      "id": "173929715962866"
    }
  ], 
  "paging": {
    "next": "https://graph.facebook.com/628825055/groups?fields=name,id&icon_size=16&limit=5000&offset=5000&__after_id=173929715962866"
  }
} 
*/