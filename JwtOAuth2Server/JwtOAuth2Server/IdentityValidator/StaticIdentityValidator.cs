using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace JwtOAuth2Server.IdentityValidator
{
    public class StaticIdentityValidator : IIdentityValidator
    {
        public string GetCallbackUrl(string clientId)
        {
            return "https://oauth.pstmn.io/v1/callback";
        }

        public List<string> GetRoles(string userName)
        {
            return new List<string> { "admin" };
        }

        public string GetUserName(string clientId)
        {
            return "richard";
        }

        public bool VerifyClientIdAndSecretPair(string clientId, string clientSecret)
        {
            return true;
        }

        public bool VerifyUserNameAndPasswordPair(string userName, string password)
        {
            return true;
        }
    }
}