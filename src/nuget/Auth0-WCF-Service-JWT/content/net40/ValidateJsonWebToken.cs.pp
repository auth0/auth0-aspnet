using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using Microsoft.IdentityModel;
using Microsoft.IdentityModel.Configuration;
using Microsoft.IdentityModel.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Collections.ObjectModel;
using System.ServiceModel.Security;
using System.Xml;
using System.ServiceModel.Channels;
using System.Configuration;

namespace $rootnamespace$
{
    public class ValidateJsonWebToken : ServiceAuthorizationManager
    {
        private static string AllowedAudience = ConfigurationManager.AppSettings["jwt:AllowedAudience"];
        private static string AllowedIssuer = ConfigurationManager.AppSettings["jwt:AllowedIssuer"];
        private static string SymmetricKey = ConfigurationManager.AppSettings["jwt:SymmetricKey"];

         public override bool CheckAccess(OperationContext operationContext)
        {
            HttpRequestMessageProperty httpRequestMessage;
            object httpRequestMessageObject;

            if (operationContext.RequestContext.RequestMessage.Properties.TryGetValue(HttpRequestMessageProperty.Name, out httpRequestMessageObject))
            {
                httpRequestMessage = httpRequestMessageObject as HttpRequestMessageProperty;
                if (string.IsNullOrEmpty(httpRequestMessage.Headers["Authorization"]))
                {
                    return false;
                }

                if (!httpRequestMessage.Headers["Authorization"].StartsWith("Bearer "))
                {
                    return false;
                }
                
                string jwt = httpRequestMessage.Headers["Authorization"].Split(' ')[1];
                if (string.IsNullOrEmpty(jwt))
                {
                    return false;
                }
                
                var key = Convert.FromBase64String(SymmetricKey.Replace("_", "/").Replace("-", "+"));
                Dictionary<string, object> json = JWT.JsonWebToken.DecodeToObject(jwt, key, verify: true, checkExpiration: true, audience: AllowedAudience, issuer: AllowedIssuer) as Dictionary<string, object>;
                var claims = json.Where(kv => kv.Value != null)
                                 .Select(c => new Claim(c.Key, c.Value.ToString())).ToList();

                claims.Add(new Claim(ClaimTypes.Name, GetClaimWithFallback(claims, "name", "sub")));
                IClaimsPrincipal claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity[] { new ClaimsIdentity(claims) });

                SetPrincipal(operationContext, claimsPrincipal);

                return true;
            }

            return false;
        }

        private string GetClaimWithFallback(IEnumerable<Claim> claims, params string[] claimTypes)
        {
            foreach (var claimType in claimTypes)
            {
                if (claims.Count(c => c.ClaimType == claimType) > 0)
                {
                    return claims.SingleOrDefault(c => c.ClaimType == claimType).Value;
                }
            }

            return null;
        }

        private void SetPrincipal(OperationContext operationContext, IClaimsPrincipal principal)
        {
            var properties = operationContext.ServiceSecurityContext.AuthorizationContext.Properties;

            if (!properties.ContainsKey("Principal"))
            {
                properties.Add("Principal", principal);
            }
            else
            {
                properties["Principal"] = principal;
            }
        }
    }
}