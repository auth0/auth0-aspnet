namespace aspnet4_sample1
{
    using System;
    using System.Threading.Tasks;
    using Auth0.AuthenticationApi;
    using Auth0.AuthenticationApi.Models;
    using Auth0.AspNet;
    using System.Collections.Generic;
    using System.Configuration;
    using System.IdentityModel.Services;
    using System.Linq;
    using System.Web;

    public class LoginCallback : HttpTaskAsyncHandler
    {
        public override async Task ProcessRequestAsync(HttpContext context)
        {
            AuthenticationApiClient client = new AuthenticationApiClient(
                new Uri(string.Format("https://{0}", ConfigurationManager.AppSettings["auth0:Domain"])));

            var token = await client.GetTokenAsync(new AuthorizationCodeTokenRequest
            {
                ClientId = ConfigurationManager.AppSettings["auth0:ClientId"],
                ClientSecret = ConfigurationManager.AppSettings["auth0:ClientSecret"],
                Code = context.Request.QueryString["code"],
                RedirectUri = context.Request.Url.ToString()
            });

            var profile = await client.GetUserInfoAsync(token.AccessToken);

            var user = new List<KeyValuePair<string, object>>
            {
                new KeyValuePair<string, object>("name", profile.FullName ?? profile.PreferredUsername ?? profile.Email),
                new KeyValuePair<string, object>("email", profile.Email),
                new KeyValuePair<string, object>("family_name", profile.LastName),
                new KeyValuePair<string, object>("given_name", profile.FirstName),
                new KeyValuePair<string, object>("nickname", profile.NickName),
                new KeyValuePair<string, object>("picture", profile.Picture),
                new KeyValuePair<string, object>("user_id", profile.UserId),
                new KeyValuePair<string, object>("id_token", token.IdToken),
                new KeyValuePair<string, object>("access_token", token.AccessToken),
                new KeyValuePair<string, object>("refresh_token", token.RefreshToken)
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

            if (context.Request.QueryString["state"] != null && context.Request.QueryString["state"].StartsWith("ru="))
            {
                var state = HttpUtility.ParseQueryString(context.Request.QueryString["state"]);
                context.Response.Redirect(state["ru"], true);
            }

            context.Response.Redirect("/");
        }

        public bool IsReusable
        {
            get { return false; }
        }
    }
}