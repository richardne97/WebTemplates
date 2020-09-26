using JwtWebApiIIS.Injections;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Owin.Security.OAuth;
using Owin;
using Swashbuckle.Application;
using System;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Mvc;

namespace JwtWebApiIIS
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            HttpConfiguration config = new HttpConfiguration();

            // Configure Swagger help page
            config.EnableSwagger(c =>
            {
                c.SingleApiVersion("v1", "My API").License(lc => lc.Name("My Company").Url("https://github.com/richardne97/"));
                c.IncludeXmlComments($"{AppContext.BaseDirectory}{Assembly.GetExecutingAssembly().GetName().Name}.xml");
                c.DescribeAllEnumsAsStrings();
                c.ApiKey("Authorization").Description("OAuth2 JWT for accessing secure APIs").Name("Authorization").In("header");
            })
            .EnableSwaggerUi(u =>
            {
                u.DocumentTitle("My API");
                u.EnableApiKeySupport("Authorization", "header");
            });

            //Enable JWT Authentication
            config.SuppressDefaultHostAuthentication();
            config.Filters.Add(new HostAuthenticationFilter(OAuthDefaults.AuthenticationType));

            //Validating model format while receving request.
            config.Filters.Add(new Filter.ModelValidatorFilter());

            //Enable Attribute Route
            config.MapHttpAttributeRoutes();

            //Injection settings
            var services = new ServiceCollection();

            //Injection setting for controllers. 
            services.AddControllersAsServices(typeof(Startup).Assembly.GetExportedTypes()
                    .Where(t => !t.IsAbstract && !t.IsGenericTypeDefinition)
                    .Where(t => typeof(IController).IsAssignableFrom(t)
                    || typeof(IHttpController).IsAssignableFrom(t)));

            object injectObject = Guid.NewGuid();
            services.AddSingleton(typeof(object), injectObject);

            var resolver = new DefaultDependencyResolver(services.BuildServiceProvider());

            //For MVC
            DependencyResolver.SetResolver(resolver);

            //For Web API
            config.DependencyResolver = resolver; //For Web API

            //Auth Setting
            app.UseOAuthBearerAuthentication(new OAuthBearerAuthenticationOptions()
            {
                AccessTokenFormat = new JwtFormat(JwtTokenParameters.Issuer, JwtTokenParameters.Audience, JwtTokenParameters.SecurityKey)
            });

            //Enable Web Api
            app.UseWebApi(config);
        }
    }
}