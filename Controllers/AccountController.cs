using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace React.Sample.Webpack.CoreMvc.Controllers
{
	public class AccountController : Controller
    {
		[Route("Account/SignIn")]
		[ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
		public void SignIn()
		{
			var user = new ApplicationUser();
			user.AccessList = new List<AccessHistory>();
			var history = new AccessHistory();
			history.TenantID = "__YOUR TENANT__";
			history.ClientId = "__APP CLIENT ID__";
			history.Redirect = new Uri("http://localhost:9457/home");
			history.GrantType = "implicit";
			history.ResponseType = "code";
			history.Scope = "User.Read";
			history.AADEndPoint = "v2.0";
			user.AccessList.Add(history);
			TempData["User"] = JsonConvert.SerializeObject(user);
			Response.Redirect("https://login.microsoftonline.com/" + user.AccessList.LastOrDefault().TenantID
				+ "/oauth2/" + history.AADEndPoint + "/authorize?client_id=" + user.AccessList.LastOrDefault().ClientId + "&redirect_uri=" + user.AccessList.LastOrDefault().Redirect + "&grant_type=" + user.AccessList.LastOrDefault().GrantType + "&response_type=" + user.AccessList.LastOrDefault().ResponseType + "&scope=" + user.AccessList.LastOrDefault().Scope);
		}
	}
}
