using Auth0.AuthenticationApi;
using Auth0.AuthenticationApi.Models;
using System;
using System.Configuration;
using System.Globalization;
using System.IdentityModel.Services;
using System.Web;
using System.Web.Mvc;

namespace aspnet4_sample1.Controllers
{
    public class AccountController : Controller
    {
        public ActionResult Login(string returnUrl)
        {
            var client = new AuthenticationApiClient(
                new Uri(string.Format("https://{0}", ConfigurationManager.AppSettings["auth0:Domain"])));


            var request = this.Request;
            var redirectUri = new UriBuilder(request.Url.Scheme, request.Url.Host, this.Request.Url.IsDefaultPort ? -1 : request.Url.Port, "LoginCallback.ashx");

            var authorizeUrlBuilder = client.BuildAuthorizationUrl()
                .WithClient(ConfigurationManager.AppSettings["auth0:ClientId"])
                .WithRedirectUrl(redirectUri.ToString())
                .WithResponseType(AuthorizationResponseType.Code)
                .WithScope("openid profile")
                // adding this audience will cause Auth0 to use the OIDC-Conformant pipeline
                // you don't need it if your client is flagged as OIDC-Conformant (Advance Settings | OAuth)
                .WithAudience("https://" + @ConfigurationManager.AppSettings["auth0:Domain"] + "/userinfo");

            if (!string.IsNullOrEmpty(returnUrl))
            {
                var state = "ru=" + HttpUtility.UrlEncode(returnUrl);
                authorizeUrlBuilder.WithState(state);
            }

            return new RedirectResult(authorizeUrlBuilder.Build().ToString());
        }

        public ActionResult Logout()
        {
            FederatedAuthentication.SessionAuthenticationModule.SignOut();

            // Redirect to Auth0's logout endpoint.
            // After terminating the user's session, Auth0 will redirect to the 
            // returnTo URL, which you will have to add to the list of allowed logout URLs for the client.
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