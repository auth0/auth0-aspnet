namespace $rootnamespace$
{
	using Auth0.AspNet;
    using Microsoft.IdentityModel.Web;
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Linq;
    using System.Web;

    public class LoginCallback : IHttpHandler
    {
        private readonly Auth0Client client = new Auth0Client(ConfigurationManager.AppSettings["auth0:Domain"],
                ConfigurationManager.AppSettings["auth0:ClientId"],
                ConfigurationManager.AppSettings["auth0:ClientSecret"]);

        public void ProcessRequest(HttpContext context)
        {
            var token = client.ExchangeAuthorizationCodePerAccessToken(context.Request.QueryString["code"], context.Request.Url.ToString());
            var profile = client.GetUserInfo(token);

            var user = new List<KeyValuePair<string, object>>
            {
                new KeyValuePair<string, object>("name", profile.Name),
                new KeyValuePair<string, object>("nickname", profile.Nickname),
                new KeyValuePair<string, object>("picture", profile.Picture)
            };

            // NOTE: Uncomment the following code in order to include claims from associated identities
            //profile.Identities.ToList().ForEach(i =>
            //{
            //    user.Add(new KeyValuePair<string, object>(i.Connection + ".access_token", i.AccessToken));
            //    user.Add(new KeyValuePair<string, object>(i.Connection + ".provider", i.Provider));
            //    user.Add(new KeyValuePair<string, object>(i.Connection + ".user_id", i.UserId));
            //});
            
            // NOTE: uncomment this if you send roles
            // user.Add(new KeyValuePair<string, object>(ClaimTypes.Role, profile.ExtraProperties["roles"]));

            // NOTE: this will set a cookie with all the user claims that will be converted 
            //       to a ClaimsPrincipal for each request using the SessionAuthenticationModule HttpModule. 
            //       You can choose your own mechanism to keep the user authenticated (FormsAuthentication, Session, etc.)
            FederatedAuthentication.SessionAuthenticationModule.CreateSessionCookie(user);
            
            var returnTo = "/";
			var state = context.Request.QueryString["state"];
            if (state != null)
            {
                var stateValues = HttpUtility.ParseQueryString(context.Request.QueryString["state"]);
                var redirectUrl = stateValues["ru"];

                // check for open redirection
                if (redirectUrl != null && IsLocalUrl(redirectUrl))
                {
                    returnTo = redirectUrl;
                }
            }

            context.Response.Redirect(returnTo);
        }

        public bool IsReusable { get { return false; } }

        private bool IsLocalUrl(string url)
        {
            return !String.IsNullOrEmpty(url)
                && url.StartsWith("/")
                && !url.StartsWith("//")
                && !url.StartsWith("/\\");
        }
    }
}