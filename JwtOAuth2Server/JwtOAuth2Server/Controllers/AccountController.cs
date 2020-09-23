using System.Web;
using System.Web.Mvc;
using System.Security.Claims;
using System.Security.Principal;
using System;
using Microsoft.Owin.Security;
using JwtOAuth2Server.IdentityValidator;

namespace JwtOAuth2Server
{
    [AllowAnonymous]
    [RoutePrefix("Account")]
    public class AccountController : Controller
    {
        private IIdentityValidator _identityValidator;

        public AccountController(IIdentityValidator identityValidator)
        {
            _identityValidator = identityValidator;
        }


        [Route("")]
        public string Index()
        {
            return "account controller is live";
        }

        [Route("Login")]
        public ActionResult Login()
        {
            var authentication = HttpContext.GetOwinContext().Authentication;
            if (Request.HttpMethod == "POST")
            {
                var isPersistent = !string.IsNullOrEmpty(Request.Form.Get("isPersistent"));
                string userName = Request.Form["username"];
                string password = Request.Form["password"];
                if (!string.IsNullOrEmpty(Request.Form.Get("submit.Signin")))
                {
                    bool result = _identityValidator.VerifyUserNameAndPasswordPair(userName, password);

                    if(result)
                    {
                        authentication.SignIn(
                            new AuthenticationProperties { IsPersistent = isPersistent },
                            new ClaimsIdentity(new[] { new Claim(ClaimsIdentity.DefaultNameClaimType, userName) }, "Application"));
                        ViewBag.message = "Sign in ok";
                        return View();
                    }
                    else
                    {
                        ViewBag.message = "username or password incorrect";
                    }
                }
            }
            return View();
        }

        public ActionResult Logout()
        {
            var authentication = HttpContext.GetOwinContext().Authentication;
            authentication.SignOut("Application");
            return View();
        }

    }
}