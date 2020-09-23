using Microsoft.Owin.Security;
using System;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Logging;

namespace JwtOAuth2Server
{
    public class JwtFormat : ISecureDataFormat<AuthenticationTicket>
    {
        private readonly string _issuer = string.Empty;
        private string _audience = null;
        private string _securityKey = null;

        public JwtFormat(string issuer, string audience, string securityKey)
        {
            _issuer = issuer;
            _audience = audience;
            _securityKey = securityKey;
        }

        public string Protect(AuthenticationTicket data)
        {
            if (data == null)
            {
                throw new ArgumentNullException("data");
            }

            //使用對稱式加密
            SymmetricSecurityKey securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_securityKey));
            //Assign SecurityAlgorithm
            SigningCredentials signingCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256Signature);

            DateTimeOffset issued = data.Properties.IssuedUtc ?? DateTimeOffset.Now;
            DateTimeOffset expires = data.Properties.ExpiresUtc ?? DateTimeOffset.Now.AddMinutes(30);

            IdentityModelEventSource.ShowPII = true;

            SecurityTokenDescriptor securityTokenDescriptor = new SecurityTokenDescriptor()
            {
                ///發行者，通常為公司名稱，公司網址
                Issuer = _issuer,
                ///聽眾(Token接收者)，接收者必須與此同名
                Audience = _audience,
                ///發行日期
                IssuedAt = issued.UtcDateTime,
                ///失效日期
                Expires = expires.UtcDateTime,
                ///在此日期前，Token尚未生效
                NotBefore = issued.UtcDateTime,
                ///使用者自訂主題，例如使用者名稱，角色等
                Subject = data.Identity,
                ///加密金鑰
                SigningCredentials = signingCredentials,
            };

            var handler = new JwtSecurityTokenHandler();

            ///建立Token
            SecurityToken token = handler.CreateToken(securityTokenDescriptor);
            return handler.WriteToken(token);
        }

        public AuthenticationTicket Unprotect(string protectedText)
        {
            throw new NotImplementedException();
        }
    }
}