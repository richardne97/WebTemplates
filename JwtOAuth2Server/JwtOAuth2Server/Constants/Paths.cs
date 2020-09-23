using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace JwtOAuth2Server
{
    static public class Paths
    {
        static public string Authorize = "/OAuth2/Authorize";
        static public string Token = "/OAuth2/Token";

        public static string Login = "/Account/Login";
        public static string Logout = "/Account/Logout";
    }
}