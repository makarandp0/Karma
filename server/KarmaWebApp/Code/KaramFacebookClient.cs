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


    public class KaramFacebookClient 
    {
        private FacebookClient _facebookClient;
        public KaramFacebookClient(string accessToken)
        {
            _facebookClient = new FacebookClient(accessToken);
            Friends = new List<FacebookFriend>();
        }
        
        public string  FacebookId {get; private set;}
        public string Name { get; private set; }
        public string FirstName { get; private set; }
        public string PictureUrl { get; private set; }
        public List<FacebookFriend> Friends { get; private set; }
        public string Location { get; private set; }
        public string Email { get; private set; }

        public bool ReadBasicInfo()
        {
            try
            {
                if (String.IsNullOrEmpty(FacebookId))
                {
                    //dynamic meResults = _facebookClient.Get("me", new { fields = "name,first_name,id,picture,location,email" });
                    dynamic meResults = _facebookClient.Get("me", new { fields = "name,first_name,id,picture,location,email" });
                    this.FacebookId = meResults.id;
                    this.FirstName = meResults.first_name;
                    this.Name = meResults.ContainsKey("name") ? meResults.name : "Unknown";
                    this.PictureUrl = meResults.ContainsKey("picture") ? meResults.picture.data.url : null;
                    this.Location = meResults.ContainsKey("location") ? meResults.location.name : "unknown";
                    this.Email = meResults.ContainsKey("email") ? meResults.email : "unknown"; 
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
                    new FacebookBatchParameter("me", new { fields = "name,first_name,id,picture,location,email" }),
                    new FacebookBatchParameter("me/friends", new { fields = "id,name,installed" })
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

                    foreach (var frienddata in result[1].data)
                    {
                        this.Friends.Add(new FacebookFriend(frienddata));
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