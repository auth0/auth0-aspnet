using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;

namespace WebFormsSample
{
    public class Auth0Client
    {
        private readonly string _clientId;
        private readonly string _clientSecret;
        private readonly RestClient _restClient;

        public Auth0Client(string domain, string clientId, string clientSecret)
        {
            this._clientId = clientId;
            this._clientSecret = clientSecret;
            _restClient = new RestClient("https://" + domain);
        }

        public TokenResult ExchangeAuthorizationCodePerAccessToken(string code, string redirectUri)
        {
            RestRequest restRequest = new RestRequest("/oauth/token", Method.POST);
            restRequest.AddHeader("accept", "application/json");
            restRequest.AddParameter("client_id", (object)_clientId, ParameterType.GetOrPost);
            restRequest.AddParameter("client_secret", (object)_clientSecret, ParameterType.GetOrPost);
            restRequest.AddParameter("code", (object)code, ParameterType.GetOrPost);
            restRequest.AddParameter("grant_type", (object)"authorization_code", ParameterType.GetOrPost);
            restRequest.AddParameter("redirect_uri", (object)redirectUri, ParameterType.GetOrPost);
            Dictionary<string, string> data = _restClient.Execute<Dictionary<string, string>>((IRestRequest)restRequest).Data;
            if (data.ContainsKey("error") || data.ContainsKey("error_description"))
                throw new OAuthException(data["error_description"], data["error"]);
            return new TokenResult()
            {
                AccessToken = data["access_token"],
                IdToken = data.ContainsKey("id_token") ? data["id_token"] : string.Empty
            };
        }

        public UserProfile GetUserInfo(TokenResult tokenResult)
        {
            if (tokenResult == null)
                throw new ArgumentNullException("tokenResult");
            UserProfile userProfileFromJson = this.GetUserProfileFromJson(!string.IsNullOrEmpty(tokenResult.IdToken) ? this.GetJsonProfileFromIdToken(tokenResult.IdToken) : this.GetJsonProfileFromAccessToken(tokenResult.AccessToken));
            if (!string.IsNullOrEmpty(userProfileFromJson.UserId))
                return userProfileFromJson;
            return this.GetUserInfo(new TokenResult()
            {
                AccessToken = tokenResult.AccessToken
            });
        }

        private UserProfile GetUserProfileFromJson(string jsonProfile)
        {
            string[] ignoredProperties = new string[5]
            {
        "iss",
        "sub",
        "aud",
        "exp",
        "iat"
            };
            string[] mappedProperties = new string[10]
            {
        "email",
        "family_name",
        "gender",
        "given_name",
        "locale",
        "name",
        "nickname",
        "picture",
        "user_id",
        "identities"
            };
            UserProfile userProfile = JsonConvert.DeserializeObject<UserProfile>(jsonProfile);
            Dictionary<string, object> responseData = JsonConvert.DeserializeObject<Dictionary<string, object>>(jsonProfile);
            userProfile.ExtraProperties = responseData != null ? responseData.Keys.Where<string>((Func<string, bool>)(x => !((IEnumerable<string>)mappedProperties).Contains<string>(x) && !((IEnumerable<string>)ignoredProperties).Contains<string>(x))).ToDictionary<string, string, object>((Func<string, string>)(x => x), (Func<string, object>)(x => responseData[x])) : new Dictionary<string, object>();
            for (int index = 0; index < userProfile.ExtraProperties.Count; ++index)
            {
                KeyValuePair<string, object> keyValuePair = userProfile.ExtraProperties.ElementAt<KeyValuePair<string, object>>(index);
                if (keyValuePair.Value is JArray)
                {
                    string[] array = ((IEnumerable<JToken>)keyValuePair.Value).Select<JToken, string>((Func<JToken, string>)(v => v.ToString())).ToArray<string>();
                    userProfile.ExtraProperties.Remove(keyValuePair.Key);
                    userProfile.ExtraProperties.Add(keyValuePair.Key, (object)array);
                }
            }
            return userProfile;
        }

        private string GetJsonProfileFromIdToken(string idToken)
        {
            return Encoding.UTF8.GetString(Base64UrlDecode(idToken.Split('.')[1]));
        }

        private string GetJsonProfileFromAccessToken(string accessToken)
        {
            RestRequest restRequest = new RestRequest("/userinfo?access_token={accessToken}");
            restRequest.AddHeader("accept", "application/json");
            restRequest.AddParameter("accessToken", (object)accessToken, ParameterType.UrlSegment);
            IRestResponse restResponse = _restClient.Execute((IRestRequest)restRequest);
            if (restResponse.StatusCode == HttpStatusCode.Unauthorized)
                throw new InvalidOperationException(GetErrorDetails(restResponse.Content));
            return restResponse.Content;
        }

        private byte[] Base64UrlDecode(string input)
        {
            string s = input.Replace('-', '+').Replace('_', '/');
            switch (s.Length % 4)
            {
                case 0:
                    return Convert.FromBase64String(s);
                case 2:
                    s += "==";
                    goto case 0;
                case 3:
                    s += "=";
                    goto case 0;
                default:
                    throw new InvalidOperationException("Illegal base64url string!");
            }
        }

        private string GetErrorDetails(string resultContent)
        {
            try
            {
                return JsonConvert.DeserializeObject(resultContent).ToString();
            }
            catch (JsonReaderException ex)
            {
            }
            return resultContent;
        }
        public class TokenResult
        {
            public string AccessToken { get; set; }

            public string IdToken { get; set; }
        }

        public class OAuthException : Exception
        {
            public string Code { get; private set; }

            public string Description
            {
                get
                {
                    return this.Message;
                }
            }

            public OAuthException(string description, string code)
              : base(description)
            {
                this.Code = code;
            }
        }

        [DataContract]
        public class UserProfile
        {
            [DataMember(Name = "email")]
            public string Email { get; set; }

            [DataMember(Name = "family_name")]
            public string FamilyName { get; set; }

            [DataMember(Name = "gender")]
            public string Gender { get; set; }

            [DataMember(Name = "given_name")]
            public string GivenName { get; set; }

            [DataMember(Name = "locale")]
            public string Locale { get; set; }

            [DataMember(Name = "name")]
            public string Name { get; set; }

            [DataMember(Name = "nickname")]
            public string Nickname { get; set; }

            [DataMember(Name = "picture")]
            public string Picture { get; set; }

            [DataMember(Name = "user_id")]
            public string UserId { get; set; }

            [DataMember(Name = "identities")]
            public IEnumerable<UserProfile.Identity> Identities { get; set; }

            public Dictionary<string, object> ExtraProperties { get; set; }

            [DataContract]
            public class Identity
            {
                [DataMember(Name = "access_token")]
                public string AccessToken { get; set; }

                [DataMember(Name = "provider")]
                public string Provider { get; set; }

                [DataMember(Name = "user_id")]
                public string UserId { get; set; }

                [DataMember(Name = "connection")]
                public string Connection { get; set; }

                [DataMember(Name = "isSocial")]
                public bool IsSocial { get; set; }
            }
        }
    }
}