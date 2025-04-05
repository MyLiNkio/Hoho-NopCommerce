

namespace Nop.Plugin.Misc.AdvRedirect.Rules
{
    public interface IRule
    {
        public string RedirectUrl { get; }

        
        public bool Match(string url, string qry);
    }
}
