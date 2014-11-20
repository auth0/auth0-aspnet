Configure Auth0 with ASP.NET

1. Install-Package Auth0-ASPNET
2. Create the app in Auth0 and follow the tutorial for ASP.NET
3. Enjoy!

## Mocking the Login

You can easily create a login session by creating a handler `mocklogin.ashx` with this code. This could be useful if you are doing automated tests or load testing.

    public class MockLogin : IHttpHandler
    {
        public void ProcessRequest(HttpContext context)
        {
            var user = new List<KeyValuePair<string, object>>
            {
                new KeyValuePair<string, object>("name", "pipe"),
                new KeyValuePair<string, object>("email", "pipe.rodriguez"),
                new KeyValuePair<string, object>("family_name", "Rodriguez"),
                new KeyValuePair<string, object>("gender", "male"),
                new KeyValuePair<string, object>("given_name", "Pipe"),
                new KeyValuePair<string, object>("nickname", "pipe"),
                new KeyValuePair<string, object>("picture", "http://pic"),
                new KeyValuePair<string, object>("user_id", "auth0|123456"),
                new KeyValuePair<string, object>("connection", "Username-Password-Authentication"),
                new KeyValuePair<string, object>("provider", "auth0")
 
            };
            FederatedAuthentication.SessionAuthenticationModule.CreateSessionCookie(user);
 
            context.Response.Redirect("/");
        }
 
        public bool IsReusable { get { return false; } }
    }

## Issue Reporting

If you have found a bug or if you have a feature request, please report them at this repository issues section. Please do not report security vulnerabilities on the public GitHub issue tracker. The [Responsible Disclosure Program](https://auth0.com/whitehat) details the procedure for disclosing security issues.
