using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace JwtOAuth2Server.IdentityValidator
{
    public interface IIdentityValidator
    {
        /// <summary>
        /// 用使用者名稱取得所屬角色
        /// </summary>
        /// <param name="userName"></param>
        /// <returns></returns>
        List<string> GetRoles(string userName);

        /// <summary>
        /// 取得使用者名稱
        /// </summary>
        /// <param name="clientId"></param>
        /// <returns></returns>
        string GetUserName(string clientId);

        /// <summary>
        /// 驗證 ClientId 和 ClientSecret 是否正確
        /// </summary>
        /// <param name="clientId"></param>
        /// <param name="clientSecret"></param>
        /// <returns></returns>
        bool VerifyClientIdAndSecretPair(string clientId, string clientSecret);

        /// <summary>
        /// 取得 Callback Url
        /// </summary>
        /// <param name="clientId"></param>
        /// <returns></returns>
        string GetCallbackUrl(string clientId);

        /// <summary>
        /// 驗證帳號密碼是
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        bool VerifyUserNameAndPasswordPair(string userName, string password);
    }
}