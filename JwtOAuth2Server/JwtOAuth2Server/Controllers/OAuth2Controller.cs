using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Web;
using System.Web.Mvc;
using System.Net;
using System.Net.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;
using Microsoft.Owin.Security.DataProtection;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.OAuth;
using Microsoft.Owin.Security.DataHandler;

namespace JwtOAuth2Server.Controllers
{
    [AllowAnonymous]
    [RoutePrefix("OAuth2")]
    public class OAuth2Controller : Controller
    {
        [Route("Authorize")]
        public ActionResult Authorize()
        {
            if (Response.StatusCode != 200)
            {
                return View("AuthorizeError");
            }

            var authentication = HttpContext.GetOwinContext().Authentication;
            var ticket = authentication.AuthenticateAsync("Application").ConfigureAwait(false).GetAwaiter().GetResult();

            var identity = ticket != null ? ticket.Identity : null;
            if (identity == null)
            {
                authentication.Challenge("Application");
                return new HttpUnauthorizedResult();
            }

            var scopes = (Request.QueryString.Get("scope") ?? "").Split(' ');

            if (Request.HttpMethod == "POST")
            {
                if (!string.IsNullOrEmpty(Request.Form.Get("submit.Grant")))
                {
                    identity = new ClaimsIdentity(identity.Claims, "Bearer", identity.NameClaimType, identity.RoleClaimType);
                    foreach (var scope in scopes)
                    {
                        identity.AddClaim(new Claim("urn:oauth:scope", scope));
                    }
                    authentication.SignIn(identity);
                }
                if (!string.IsNullOrEmpty(Request.Form.Get("submit.Login")))
                {
                    authentication.SignOut("Application");
                    authentication.Challenge("Application");
                    return new HttpUnauthorizedResult();
                }
            }
            return View();
        }

        [Route("Callback")]
        [HttpGet]
        public void Callback(string code)
        {
            //string token = GetToken(new Uri("http://localhost:2793/Token"), "p1RE8WuCQgzJ1Vs7eFf4lxc7X5430EH7", "C7FAfFWk4oC8uzEuU7Gqlw==", "http://localhost:2793/callback", code);
        }

        private class MachineKeyProtector : IDataProtector
        {
            private readonly string[] _purpose =
            {
                typeof(OAuthAuthorizationServerMiddleware).Namespace,
                "Access_Token",
                "v1"
            };

            public byte[] Protect(byte[] userData)
            {
                throw new NotImplementedException();
            }

            public byte[] Unprotect(byte[] protectedData)
            {
                return System.Web.Security.MachineKey.Unprotect(protectedData, _purpose);
            }
        }

        private string GetToken(Uri tokenUri, string clientId, string clientSecret, string redirectUri, string code)
        {
            HttpClient httpClient = new HttpClient();

            httpClient.Timeout = new TimeSpan(0, 0, 60);
            httpClient.BaseAddress = tokenUri;

            FormUrlEncodedContent content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("grant_type", "authorization_code"),
                new KeyValuePair<string, string>("code", code),
                new KeyValuePair<string, string>("redirect_uri", redirectUri),
                new KeyValuePair<string, string>("client_id", clientId),
                new KeyValuePair<string, string>("client_secret", clientSecret)
            });

            Task<HttpResponseMessage> responseTask = httpClient.PostAsync("", content);
            responseTask.Wait(5000);

            if (responseTask.IsCompleted)
            {
                HttpResponseMessage response = responseTask.Result;

                Task<string> responseContectTask = response.Content.ReadAsStringAsync();
                responseContectTask.Wait(5000);

                if (responseContectTask.IsCompleted)
                {
                    string responseContent = responseContectTask.Result;
                    RequestTokenResponsePrivate tokenResponse = Newtonsoft.Json.JsonConvert.DeserializeObject<RequestTokenResponsePrivate>(responseContent);
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        return tokenResponse.AccessToken;
                    }
                    else
                    {
                        if (tokenResponse.Message == "invalid code")
                        {
                            return null;
                        }
                    }
                }
            }
            return null;
        }

        private struct RequestTokenResponsePrivate
        {

            public int Status { get; set; }

            public string AccessToken { get; set; }

            public string Message { get; set; }

        }

    }
}