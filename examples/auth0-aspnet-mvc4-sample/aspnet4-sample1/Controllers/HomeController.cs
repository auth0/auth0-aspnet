using System.Configuration;
using System.Globalization;
using System.IdentityModel.Services;
using System.Linq;
using System.Security.Claims;
using System.Web.Mvc;

namespace aspnet4_sample1.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            var nameClaim = ClaimsPrincipal.Current.FindFirst("name");

            if (nameClaim != null && !string.IsNullOrEmpty(nameClaim.Value))
                ViewBag.Name = nameClaim.Value;

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

        public ActionResult Logout()
        {
            FederatedAuthentication.SessionAuthenticationModule.SignOut();

            // Redirect to Auth0's logout endpoint
            var returnTo = Url.Action("Index", "Home", null, protocol: Request.Url.Scheme);
            return Redirect(
              string.Format(CultureInfo.InvariantCulture,
                "https://{0}/v2/logout?returnTo={1}&client_id={2}",
                ConfigurationManager.AppSettings["auth0:Domain"],
                Server.UrlEncode(returnTo),
                ConfigurationManager.AppSettings["auth0:ClientId"]));
        }
    }
}