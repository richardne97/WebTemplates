using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Owin;
using Owin;
using System.Web.Routing;
using System.Web.Http;
using JwtOAuth2Server.IdentityValidator;
using Microsoft.Owin.Security.OAuth;
using Microsoft.Extensions.DependencyInjection;
using System.Web.Mvc;
using System.Web.Http.Controllers;
using JwtOAuth2Server.Injections;

namespace JwtOAuth2Server
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            IIdentityValidator iv = new StaticIdentityValidator();

            var services = new ServiceCollection();

            //Injection setting for controllers. 
            services.AddSingleton(typeof(IIdentityValidator), iv);
            services.AddControllersAsServices(typeof(Startup).Assembly.GetExportedTypes()
                    .Where(t => !t.IsAbstract && !t.IsGenericTypeDefinition)
                    .Where(t => typeof(IController).IsAssignableFrom(t)
                    || typeof(IHttpController).IsAssignableFrom(t)));

            DefaultDependencyResolver resolver = new DefaultDependencyResolver(services.BuildServiceProvider());
            
            //For MVC injection
            DependencyResolver.SetResolver(resolver);

            ConfigureAuth(app, iv);

            app.UseCors(Microsoft.Owin.Cors.CorsOptions.AllowAll);
            RouteConfig.RegisterRoutes(RouteTable.Routes);

            HttpConfiguration config = new HttpConfiguration();
            config.SuppressDefaultHostAuthentication();

            config.Filters.Add(new HostAuthenticationFilter(OAuthDefaults.AuthenticationType));

            //For Web API injection
            config.DependencyResolver = resolver; 

            app.UseWebApi(config);
        }
    }

  
}
