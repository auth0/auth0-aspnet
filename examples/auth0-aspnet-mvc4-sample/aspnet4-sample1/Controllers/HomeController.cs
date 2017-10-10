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
            string name = ClaimsPrincipal.Current.FindFirst("name")?.Value;
            ViewBag.Name = name;

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
    }
}