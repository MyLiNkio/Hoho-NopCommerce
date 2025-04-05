using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Plugin.Misc.AdvRedirect.Rules
{
    public class MatchRule : IRule
    {
        private string _matchUrl;
        private bool _useQryString;
        private string _redirectUrl;

        public MatchRule(string url, string redirectUrl, bool useQryString)
        {
            _redirectUrl = redirectUrl;
            _matchUrl = url;
            _useQryString = useQryString;
        }


        public string RedirectUrl
        {
            get
            {
                return _redirectUrl;
            }
        }

        
        public bool Match(string url, string qry)
        {
            string key = _useQryString ? url + qry : url;
            return _matchUrl == key;
        }
    }
}
