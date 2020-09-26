using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.IdentityModel.Tokens;

namespace  JwtWebApiIIS
{
    public static class JwtTokenParameters
    {
        public static TimeSpan TokenExpireTime = TimeSpan.FromMinutes(30);
        public static string Audience = Properties.Settings.Default.JwtAudience;
        public static string Issuer = Properties.Settings.Default.JwtIssuer;
        public static string SecurityKey = Properties.Settings.Default.JwtSecurityKey;
    }
}