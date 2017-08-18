using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace TestSite.Controllers
{
	public class HomeController : Controller
	{
		public ActionResult Index()
		{
			return View();
		}

		public ActionResult About()
		{
			ViewBag.Message = "Your application description page.";

			return View();
		}

		public ActionResult Contact()
		{
			ViewBag.Message = "Your contact page.";

			return View();
		}

		public ActionResult Bot()
		{
			return View();
		}

		[HttpPost]
		public async Task<ActionResult> PerformLogin(string username, string password)
		{
			var client = new HttpClient();

			var content = new FormUrlEncodedContent(new Dictionary<string, string>
			{
				["email"] = username,
				["password"] = password
			});

			var result = await client.PostAsync("https://university.xamarin.com/api/v2/login", content);
			var jsonLoginInfo = await result.Content.ReadAsStringAsync();

			var loginInfo = JsonConvert.DeserializeObject<LoginInfo>(jsonLoginInfo);

			ViewBag.Token = loginInfo.Token;

			return View("Bot");
		}
	}

	public class LoginInfo
	{
		public string Token { get; set; }
		public object Username { get; set; }
		public string NickName { get; set; }
		public string FirstName { get; set; }
		public string LastName { get; set; }
		public string Email { get; set; }
		public string GravatarUrl { get; set; }
	}
}