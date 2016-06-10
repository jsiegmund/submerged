using Autofac;
using Autofac.Integration.SignalR;
using Microsoft.AspNet.SignalR;
using Microsoft.IdentityModel.Protocols;
using Microsoft.Owin.Cors;
using Microsoft.Owin.Security.ActiveDirectory;
using Microsoft.Owin.Security.OAuth;
using Repsaj.Submerged.API.Live;
using Owin;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IdentityModel.Tokens;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace Repsaj.Submerged.API
{
    partial class Startup
    {
        public void ConfigureSignalR(IAppBuilder app)
        {
            //var hubConfiguration = new HubConfiguration();
            //hubConfiguration.EnableDetailedErrors = true;
            //hubConfiguration.EnableJavaScriptProxies = true;
            //app.MapSignalR("/signalr", hubConfiguration);

            var builder = new ContainerBuilder();

            // Register your SignalR hubs.
            builder.RegisterHubs(Assembly.GetExecutingAssembly());

            // OWIN SIGNALR SETUP

            // Set the dependency resolver to be Autofac.
            var container = builder.Build();
            AutofacDependencyResolver dependencyResolver = new AutofacDependencyResolver(container);

            // Update the globalhost resolver to use Autofac
            GlobalHost.DependencyResolver = dependencyResolver;

            // Get your HubConfiguration. In OWIN, you'll create one
            // rather than using GlobalHost.
            var config = new HubConfiguration();
            config.EnableDetailedErrors = true;
            config.EnableJavaScriptProxies = true;
            config.Resolver = dependencyResolver;

            // Register the Autofac middleware FIRST, then the standard SignalR middleware.
            app.UseAutofacMiddleware(container);
            app.MapSignalR("/signalr", config);

            //app.MapSignalR("/signalr", map =>
            //{
            //    map.UseCors(CorsOptions.AllowAll);

            //    //map.UseWindowsAzureActiveDirectoryBearerAuthentication(new WindowsAzureActiveDirectoryBearerAuthenticationOptions
            //    //{
            //    //    Tenant = ConfigurationManager.AppSettings["ida:Tenant"],
            //    //    TokenValidationParameters = new TokenValidationParameters
            //    //    {
            //    //        ValidAudience = ConfigurationManager.AppSettings["ida:Audience"]
            //    //    },
            //    //});

            //    var hubConfiguration = new HubConfiguration();
            //    hubConfiguration.EnableDetailedErrors = true;
            //    hubConfiguration.EnableJavaScriptProxies = true;
            //    map.RunSignalR(hubConfiguration);
            //});
        }
    }
}