using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace JwtOAuth2Server.IdentityValidator
{
    public interface IIdentityValidator
    {
        List<string> GetRoles(string userName);

        string GetUserName(string clientId);

        bool VerifyClientIdAndSecretPair(string clientId, string clientSecret);

        string GetCallbackUrl(string clientId);

        bool VerifyUserNameAndPasswordPair(string userName, string password);
    }
}