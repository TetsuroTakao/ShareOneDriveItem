using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace React.Sample.Webpack.CoreMvc.Controllers
{
	public class HomeController : Controller
	{
		private const int COMMENTS_PER_PAGE = 3;

		private readonly IDictionary<string, AuthorModel> _authors;
		private readonly IList<CommentModel> _comments;

		public HomeController()
		{
			// In reality, you would use a repository or something for fetching data
			// For clarity, we'll just use a hard-coded list.
			_authors = new Dictionary<string, AuthorModel>
			{
				{"daniel", new AuthorModel { Name = "Daniel Lo Nigro", GithubUsername = "Daniel15" }},
				{"vjeux", new AuthorModel { Name = "Christopher Chedeau", GithubUsername = "vjeux" }},
				{"cpojer", new AuthorModel { Name = "Christoph Pojer", GithubUsername = "cpojer" }},
				{"jordwalke", new AuthorModel { Name = "Jordan Walke", GithubUsername = "jordwalke" }},
				{"zpao", new AuthorModel { Name = "Paul O'Shannessy", GithubUsername = "zpao" }},
			};
			_comments = new List<CommentModel>
			{
				new CommentModel { Author = _authors["daniel"], Text = "First!!!!111!" },
				new CommentModel { Author = _authors["zpao"], Text = "React is awesome!" },
				new CommentModel { Author = _authors["cpojer"], Text = "Awesome!" },
				new CommentModel { Author = _authors["vjeux"], Text = "Hello World" },
				new CommentModel { Author = _authors["daniel"], Text = "Foo" },
				new CommentModel { Author = _authors["daniel"], Text = "Bar" },
				new CommentModel { Author = _authors["daniel"], Text = "FooBarBaz" },
			};
		}

		public ActionResult Index()
		{
			ApplicationUser user = null;
			var userInfo = TempData["User"];
			if (!string.IsNullOrEmpty(userInfo as string))
			{
				user = JsonConvert.DeserializeObject<ApplicationUser>(userInfo as string);
			}
			var context = this.Url.ActionContext.HttpContext;
			var query = context.Request.Query;
			var code = string.Empty;
			var state = string.Empty;
			if (query.ContainsKey("code")) code = query["code"];
			if (query.ContainsKey("session_state")) state = query["session_state"];
			var signIn = false;
			var token = string.Empty;
			if (!string.IsNullOrEmpty(code))
			{
				user.AccessList.LastOrDefault().Secret = "__CLIENT SECRET__";
				user.AccessList.LastOrDefault().AuthCode = code;
				user.AccessList.LastOrDefault().AADEndPoint = "v2.0";
				signIn = new AccessGraph().GetToken(user);
				if (signIn)
				{
					new AccessGraph().SetUserInfo(user);
					//Regist '˚¸îˆ ìNòNÅiTetsuro TakaoÅj' as Azure Active Directory account.
					user.GlobalName = string.Join("", Regex.Matches(user.displayName, @"[a-z | A-Z]*")).Trim();
					ViewBag.AccountName = user.GlobalName;
					var grant = "{\"password\":\"__PASSWORD__\",\"username\":\"__ACCOUNT__\"}";
					var history = new AccessHistory();
					history.GrantType = grant;
					history.Scope = "files.readwrite";
					history.TenantID = user.AccessList.FirstOrDefault().TenantID;
					history.Redirect = user.AccessList.FirstOrDefault().Redirect;
					history.ClientId = user.AccessList.FirstOrDefault().ClientId;
					history.Secret = user.AccessList.FirstOrDefault().Secret;
					user.AccessList.Add(history);
					if (new AccessGraph().GetToken(user))
					{
						var webLink = new AccessGraph().GetLink("01N7LZHZCQ2D4PKWJ4OJAKWEOBBJWTCOWG", user);
					}
				}
			}
			ViewBag.IsSignin = signIn;
			ViewBag.Menu = new List<string>();
			ViewBag.Message = string.Empty;
			ViewBag.Title = "Show user information";
			return View(new IndexViewModel
			{
				Comments = _comments.Take(COMMENTS_PER_PAGE).ToList().AsReadOnly(),
				CommentsPerPage = COMMENTS_PER_PAGE,
				Page = 1
			});
		}

		public ActionResult Comments(int page)
		{
			var comments = _comments.Skip((page - 1) * COMMENTS_PER_PAGE).Take(COMMENTS_PER_PAGE);
			var hasMore = page * COMMENTS_PER_PAGE < _comments.Count;

			if (ControllerContext.HttpContext.Request.ContentType == "application/json")
			{
				return new JsonResult(new
				{
					comments = comments,
					hasMore = hasMore
				});
			}
			else
			{
				return View("~/Views/Home/Index.cshtml", new IndexViewModel
				{
					Comments = _comments.Take(COMMENTS_PER_PAGE * page).ToList().AsReadOnly(),
					CommentsPerPage = COMMENTS_PER_PAGE,
					Page = page
				});
			}
		}

		public class AuthorModel
		{
			public string Name { get; set; }
			public string GithubUsername { get; set; }
		}

		public class CommentModel
		{
			public AuthorModel Author { get; set; }
			public string Text { get; set; }
		}

		public class IndexViewModel
		{
			public IReadOnlyList<CommentModel> Comments { get; set; }
			public int CommentsPerPage { get; set; }
			public int Page { get; set; }
		}
	}
}
