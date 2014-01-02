using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KarmaWeb.Utilities
{
    /// <summary>
    /// implementation of methods common to all objects
    /// </summary>
    public class KarmaObject
    {
        private ILogger _logger = new NullLogger();
        public ILogger Logger
        { 
            get
            {
                return this._logger;
            }
            set 
            {
                this._logger = value;
            }
        }
    }
}
