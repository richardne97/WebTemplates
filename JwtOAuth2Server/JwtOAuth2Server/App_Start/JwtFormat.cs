using Microsoft.Owin.Security;
using System;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Logging;
using System.Linq;
using System.Security.Claims;

namespace JwtOAuth2Server
{
    public class JwtFormat : ISecureDataFormat<AuthenticationTicket>
    {
        private readonly string _issuer = String.Empty;
        private readonly string _audience = null;
        private readonly TokenValidationParameters _tokenValidationParameters;
        private readonly SymmetricSecurityKey _securityKey;
        private readonly SigningCredentials _signingCredentials;
        private readonly JwtSecurityTokenHandler _handler = new JwtSecurityTokenHandler();
        
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="issuer">Issuer of JWT</param>
        /// <param name="audience">Audience of JWT</param>
        /// <param name="securityKey">Security of JWT</param>
        public JwtFormat(string issuer, string audience, string securityKey)
        {
            _issuer = issuer;
            _audience = audience;

            //Symmetric security key
            _securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(securityKey));

            //Assign SecurityAlgorithm
            _signingCredentials = new SigningCredentials(_securityKey, SecurityAlgorithms.HmacSha256Signature);

            IdentityModelEventSource.ShowPII = true;

            _tokenValidationParameters = new TokenValidationParameters
            {
                ValidAudience = _audience,
                ValidIssuer = _issuer,
                IssuerSigningKey = _securityKey,
                ValidateLifetime = true,
                ValidateAudience = true,
                ValidateIssuer = true,
                RequireSignedTokens = true,
                RequireExpirationTime = true,
                ValidateIssuerSigningKey = true
            };
        }

        public string Protect(AuthenticationTicket data)
        {
            if (data == null)
            {
                throw new ArgumentNullException("data");
            }

            DateTimeOffset issued = data.Properties.IssuedUtc ?? DateTimeOffset.Now;
            DateTimeOffset expires = data.Properties.ExpiresUtc ?? DateTimeOffset.Now.AddMinutes(30);

            SecurityTokenDescriptor securityTokenDescriptor = new SecurityTokenDescriptor()
            {
                ///Issuer, usually is domain name or company name
                Issuer = _issuer,
                ///Audience, the web api domain name or resouce name
                Audience = _audience,
                ///JWT issue date
                IssuedAt = issued.UtcDateTime,
                ///JWT expire date
                Expires = expires.UtcDateTime,
                ///The JWT is not valid before the date
                NotBefore = issued.UtcDateTime,
                ///User defined subject
                Subject = data.Identity,
                ///JWT key
                SigningCredentials = _signingCredentials,
            };

            ///Create and return token
            return _handler.WriteToken(_handler.CreateToken(securityTokenDescriptor));
        }

        public AuthenticationTicket Unprotect(string protectedText)
        {
            try
            {
                JwtSecurityToken jwtSecurityToken = _handler.ReadJwtToken(protectedText);
                ClaimsPrincipal principal = _handler.ValidateToken(jwtSecurityToken.RawData, _tokenValidationParameters, out SecurityToken token);
                return new AuthenticationTicket(principal.Identities.First(), new AuthenticationProperties());
            }
            catch
            {
                //Decode fail, invalid token
                return null;
            }
        }
    }
}