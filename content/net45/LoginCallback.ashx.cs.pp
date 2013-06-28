using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace $rootnamespace$
{
    public class LoginCallback : IHttpHandler
    {
        private readonly Auth0.Client client = new Auth0.Client(
                                ConfigurationManager.AppSettings["auth0:ClientId"],
                                ConfigurationManager.AppSettings["auth0:ClientSecret"],
                                ConfigurationManager.AppSettings["auth0:Domain"]);

        public void ProcessRequest(HttpContext context)
        {
            var token = client.ExchangeAuthorizationCodePerAccessToken(context.Request.QueryString["code"], ConfigurationManager.AppSettings["auth0:CallbackUrl"]);
            var profile = client.GetUserInfo(token.AccessToken);

            var user = new List<KeyValuePair<string, object>>
            {
                new KeyValuePair<string, object>("name", profile.Name),
                new KeyValuePair<string, object>("email", profile.Email),
                new KeyValuePair<string, object>("family_name", profile.FamilyName),
                new KeyValuePair<string, object>("gender", profile.Gender),
                new KeyValuePair<string, object>("given_name", profile.GivenName),
                new KeyValuePair<string, object>("nickname", profile.Nickname),
                new KeyValuePair<string, object>("picture", profile.Picture),
                new KeyValuePair<string, object>("user_id", profile.UserId),
                new KeyValuePair<string, object>("id_token", token.IdToken),
                new KeyValuePair<string, object>("access_token", token.IdToken),
                new KeyValuePair<string, object>("connection", profile.Identities.First().Connection),
                new KeyValuePair<string, object>("provider", profile.Identities.First().Provider)
            };

            // NOTE: Uncomment the following code in order to include claims from associated identities
            //profile.Identities.ToList().ForEach(i =>
            //{
            //    new KeyValuePair<string, object>(i.Connection + ".access_token", i.AccessToken);
            //    new KeyValuePair<string, object>(i.Connection + ".provider", i.Provider);
            //    new KeyValuePair<string, object>(i.Connection + ".user_id", i.UserId);
            //});

            // NOTE: this will set a cookie with all the user claims that will be converted 
            //       to a ClaimsPrincipal for each request using the ClaimsCookie HttpModule . 
            //       You can choose your own mechanism to keep the user authenticated (FormsAuthentication, Session, etc.)
            ClaimsCookie.ClaimsCookieModule.Instance.CreateSessionSecurityToken(user);

            context.Response.Redirect("/");
        }

        public bool IsReusable { get { return false; } }
    }
}