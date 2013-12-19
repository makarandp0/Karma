using KarmaBackEnd;
using KarmaWebApp.Code;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KarmaWebApp
{
    // maintains context browser sessions context
    public class KarmaSessionContext
    {
        public IKaramActiveUser ActiveUser = null;
    }
}