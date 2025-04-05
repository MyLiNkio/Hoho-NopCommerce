using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Nop.Core;
using Nop.Plugin.Misc.AdvRedirect.Domain;
using Nop.Plugin.Misc.AdvRedirect.Enums;
using Nop.Plugin.Misc.AdvRedirect.Models.Redirections;

namespace Nop.Plugin.Misc.AdvRedirect.Services
{
    public interface IDataIOService
    {
		Task<List<(int line, InsertRedirectionResult result)>> Import(string csvText);

		Task<string> Export();
	}
}