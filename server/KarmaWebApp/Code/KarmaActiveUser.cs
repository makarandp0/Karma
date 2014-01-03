using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KarmaGraph.Types;

namespace KarmaWebApp.Code
{
    public class KarmaActiveUser : IKaramActiveUser
    {
        private KarmaUser _activeUser;

        public KarmaActiveUser(KarmaUser person)
        {
            this._activeUser = person;
        }

        public string CreateRequest(string Title, string p, DateTime dateTime)
        {
            throw new NotImplementedException();
        }
    }
}
