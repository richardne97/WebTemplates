﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Owin;
using Microsoft.Owin;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.OAuth;
using Microsoft.Owin.Security.Infrastructure;
using System.Security.Claims;
using System.Security.Principal;
using System.Collections.Concurrent;
using JwtOAuth2Server.IdentityValidator;

namespace JwtOAuth2Server
{
    public partial class Startup
    {
        private string _jwtIssuer = Properties.Settings.Default.JwtIssuer;
        private string _jwtAudience = Properties.Settings.Default.JwtAudience;
        
        private readonly ConcurrentDictionary<string, string> _authenticationCodes = new ConcurrentDictionary<string, string>(StringComparer.Ordinal);
        private IIdentityValidator _identityValidator = null;

        public void ConfigureAuth(IAppBuilder app, IIdentityValidator identityValidator)
        {
            _identityValidator = identityValidator;

            // Enable the External Sign In Cookie.
            app.SetDefaultSignInAsAuthenticationType("Application");
            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationType = "Application",
                AuthenticationMode = Microsoft.Owin.Security.AuthenticationMode.Passive,
                CookieName = CookieAuthenticationDefaults.CookiePrefix + "Application",
                ExpireTimeSpan = TimeSpan.FromMinutes(5),
                LoginPath = new PathString(Paths.Login),
                LogoutPath = new PathString(Paths.Logout),
            });
            app.SetDefaultSignInAsAuthenticationType("Application");

            // Setup Authorization Server
            app.UseOAuthAuthorizationServer(new OAuthAuthorizationServerOptions
            {
                AuthorizeEndpointPath = new PathString(Paths.Authorize),
                TokenEndpointPath = new PathString(Paths.Token),
                ApplicationCanDisplayErrors = true,
#if DEBUG
                AllowInsecureHttp = true,
#endif
                // Authorization server provider which controls the lifecycle of Authorization Server
                Provider = new OAuthAuthorizationServerProvider
                {
                    OnValidateClientRedirectUri = ValidateClientRedirectUri,
                    OnValidateClientAuthentication = ValidateClientAuthentication,
                    OnGrantResourceOwnerCredentials = GrantResourceOwnerCredentials,
                    OnGrantClientCredentials = GrantClientCredetails
                },

                AccessTokenExpireTimeSpan = TimeSpan.FromMinutes(30),
                AccessTokenFormat = new JwtFormat(_jwtIssuer, _jwtAudience, Properties.Settings.Default.SecurityKey),

                // Authorization code provider which creates and receives the authorization code.
                AuthorizationCodeProvider = new AuthenticationTokenProvider
                {
                    OnCreate = CreateAuthenticationCode,
                    OnReceive = ReceiveAuthenticationCode,
                },

                // Refresh token provider which creates and receives refresh token.
                RefreshTokenProvider = new AuthenticationTokenProvider
                {
                    OnCreate = CreateRefreshToken,
                    OnReceive = ReceiveRefreshToken,
                }
            });
        }

        private Task ValidateClientRedirectUri(OAuthValidateClientRedirectUriContext context)
        {
            string callbackUrl = _identityValidator.GetCallbackUrl(context.ClientId);

            //檢查 ClientId
            if (callbackUrl != null)
            {
                if (callbackUrl == context.RedirectUri)
                    context.Validated(callbackUrl);
                else
                    context.SetError("callback url is incorrent");
            }
            else
                context.SetError("client id is incorrent");
            return Task.FromResult(0);
        }

        private Task ValidateClientAuthentication(OAuthValidateClientAuthenticationContext context)
        {
            string clientId;
            string clientSecret;
            if (context.TryGetBasicCredentials(out clientId, out clientSecret) || context.TryGetFormCredentials(out clientId, out clientSecret))
            {
                if (_identityValidator.VerifyClientIdAndSecretPair(clientId, clientSecret)) 
                    context.Validated(clientId);
                else
                {
                    context.SetError("invalid_grant", "The Client Id or Client Secret is incorrect.");
                    context.Response.StatusCode = (int)System.Net.HttpStatusCode.Unauthorized;
                    context.Rejected();
                }
            }
            return Task.FromResult(0);
        }

        private Task GrantResourceOwnerCredentials(OAuthGrantResourceOwnerCredentialsContext context)
        {
            var identity = new ClaimsIdentity(new GenericIdentity(
               context.UserName, OAuthDefaults.AuthenticationType),
               context.Scope.Select(x => new Claim("urn:oauth:scope", x)));

            context.Validated(identity);

            return Task.FromResult(0);
        }

        private Task GrantClientCredetails(OAuthGrantClientCredentialsContext context)
        {
            //Get userName from Database by given clientId
            string userName = _identityValidator.GetUserName(context.ClientId);

            if (userName != null)
            {
                var identity = new ClaimsIdentity(new[] { new Claim(ClaimsIdentity.DefaultNameClaimType, userName) }, OAuthDefaults.AuthenticationType);

                //Get roles from Database by given userName
                List<string>roles = _identityValidator.GetRoles(userName);

                if (roles == null)
                    context.Rejected();
                else
                {
                    foreach (string cunRole in roles)
                        identity.AddClaim(new Claim(ClaimTypes.Role, cunRole));
                    context.Validated(identity);
                }
            }
            else
                context.Rejected();

            return Task.FromResult(0);
        }

        private void CreateAuthenticationCode(AuthenticationTokenCreateContext context)
        {
            context.SetToken(Guid.NewGuid().ToString("n") + Guid.NewGuid().ToString("n"));
            _authenticationCodes[context.Token] = context.SerializeTicket();
        }

        private void ReceiveAuthenticationCode(AuthenticationTokenReceiveContext context)
        {
            string value;
            if (_authenticationCodes.TryRemove(context.Token, out value))
            {
                context.DeserializeTicket(value);
            }
        }

        private void CreateRefreshToken(AuthenticationTokenCreateContext context)
        {
            context.SetToken(context.SerializeTicket());
        }

        private void ReceiveRefreshToken(AuthenticationTokenReceiveContext context)
        {
            context.DeserializeTicket(context.Token);
        }
    }
}