
using System.Collections.Generic;
using Nop.Plugin.Misc.AdvRedirect.Rules;

namespace Nop.Plugin.Misc.AdvRedirect.Models
{
    public class RulesModel
    {
        public List<IRule> Rules;
        public Dictionary<string, MatchRule> Match;
    }
}
