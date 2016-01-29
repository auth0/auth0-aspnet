namespace Auth0.AspNet
{
    using Microsoft.IdentityModel.Claims;
    using Microsoft.IdentityModel.Web;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    public static class SessionAuthenticationModuleExtensions
    {
        /// <summary>
        /// Creates a ClaimsPrincipal from the specified user data, sets the current HTTP context and thread principal
        /// and writes a session cookie from the resulting SessionSecurityToken using the specified parameters.
        /// </summary>
        /// <param name="sessionAuthenticationModule"></param>
        /// <param name="user">The user data that will be used to create the claims for the ClaimsPrincipal. 
        /// Array values are converted to multiple claims with the same key.</param>
        /// <param name="context">An application defined context string.</param>
        /// <param name="domain">The domain used for the session cookie.</param>
        /// <param name="path">The virtual path used for the session cookie.</param>
        /// <param name="requireSsl">Indicates if the session cookie should only be used with SSL.</param>
        /// <param name="httpOnly">Indicates whether the session cookie should be hidden from client script.</param>
        /// <param name="cookieName">Indicates the name of the session cookie.</param>
        /// <param name="sessionCookieLifetime">The lifetime for the sesion cookie. A null value indicates no expiration.</param>
        /// <param name="persistent">Indicates if the user agent should persist the session cookie. </param>
        public static void CreateSessionCookie(
            this SessionAuthenticationModule sessionAuthenticationModule,
            IEnumerable<KeyValuePair<string, object>> user,
            string context = null,
            string domain = null,
            string path = null,
            bool requireSsl = false,
            bool httpOnly = true,
            string cookieName = null,
            TimeSpan? sessionCookieLifetime = null,
            bool persistent = false)
        {
            if (!string.IsNullOrEmpty(domain))
            {
                sessionAuthenticationModule.CookieHandler.Domain = domain;
            }

            if (!string.IsNullOrEmpty(path))
            {
                sessionAuthenticationModule.CookieHandler.Path = path;
            }

            if (!string.IsNullOrEmpty(cookieName))
            {
                sessionAuthenticationModule.CookieHandler.Name = cookieName;
            }

            sessionAuthenticationModule.CookieHandler.RequireSsl = requireSsl;
            sessionAuthenticationModule.CookieHandler.HideFromClientScript = httpOnly;

            var claims = new List<Claim>();

            if (user.Any(a => a.Key == "name"))
            {
                claims.Add(new Claim(ClaimTypes.Name, user.First(a => a.Key == "name").Value.ToString()));
            }

            if (user.Any(a => a.Key == "user_id"))
            {
                claims.Add(new Claim(ClaimTypes.NameIdentifier, user.First(a => a.Key == "user_id").Value.ToString()));
            }

            if (user.Any(a => a.Key == "connection"))
            {
                claims.Add(
                    new Claim(
                        "http://schemas.microsoft.com/accesscontrolservice/2010/07/claims/identityprovider",
                        user.First(a => a.Key == "connection").Value.ToString()));
            }

            foreach (var attribute in user)
            {
                var claimType = attribute.Key;

                if (attribute.Value != null && attribute.Value.GetType().IsArray)
                {
                    // Attribute contains an array of values (e.g.: "group" => [ "sales", "producers" ])
                    foreach (var subattribute in attribute.Value as IEnumerable)
                    {
                        claims.Add(new Claim(claimType, subattribute.ToString()));
                    }
                }
                else
                {
                    claims.Add(
                        new Claim(claimType, attribute.Value != null ? attribute.Value.ToString() : string.Empty));
                }
            }

            var principal = new ClaimsPrincipal(new ClaimsIdentity[] { new ClaimsIdentity(claims, "Auth0") });
            var session = sessionAuthenticationModule.CreateSessionSecurityToken(
                principal,
                context,
                DateTime.UtcNow,
                sessionCookieLifetime.HasValue
                    ? DateTime.UtcNow.Add(sessionCookieLifetime.Value)
                    : DateTime.MaxValue.ToUniversalTime(),
                persistent);
            sessionAuthenticationModule.AuthenticateSessionSecurityToken(session, true);
        }
    }
}
