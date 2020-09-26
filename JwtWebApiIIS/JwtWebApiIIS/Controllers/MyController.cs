using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace JwtWebApiIIS.Controllers
{
    [Authorize]
    [RoutePrefix("My")]
    public class MyController : ApiController
    {
        private object _injectObject;

        public MyController(object injectObject)
        {
            _injectObject = injectObject;
        }

        [HttpGet]
        [Route("")]
        public string MyGetAction([FromUri] string message)
        {
            return $"{User.Identity.Name}, I got your message. '{message}'";
        }

    }
}
