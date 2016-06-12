using Microsoft.Azure.Mobile.Server.Config;
using Owin;
using Repsaj.Submerged.API.Handlers;
using System.Web.Http;

namespace Repsaj.Submerged.API
{
    public partial class Startup
    {
        public void ConfigureWebApi(IAppBuilder app)
        {
            HttpConfiguration config = Startup.HttpConfiguration;

            config.MapHttpAttributeRoutes();
            config.EnableSystemDiagnosticsTracing();

            //config.MessageHandlers.Add(new AzureADAuthorizationHandler());

            new MobileAppConfiguration()
                .UseDefaultConfiguration()
                .ApplyTo(config);

            app.UseWebApi(config);

        }
    }
}