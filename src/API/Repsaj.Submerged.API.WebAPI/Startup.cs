using Microsoft.Owin.Security.ActiveDirectory;
using Microsoft.Owin;
using Owin;
using System.Web.Http;
using System.Configuration;
using Repsaj.Submerged.Common.Configurations;
using Repsaj.Submerged.API.Live;
using System.Threading.Tasks;
using Autofac;

namespace Repsaj.Submerged.API
{
    public partial class Startup
    {
        public static HttpConfiguration HttpConfiguration { get; private set; }

        public void Configuration(IAppBuilder app)
        {
            Startup.HttpConfiguration = new System.Web.Http.HttpConfiguration();
            ConfigurationProvider configProvider = new ConfigurationProvider();

            ConfigureAuth(app, configProvider);
            IContainer container = ConfigureAutofac(app);

            ConfigureJson(app);

            ConfigureSignalR(app);

            //ConfigureMobileApp(app);

            // WebAPI call must come after Autofac
            // Autofac hooks into the HttpConfiguration settings
            ConfigureWebApi(app);

            // Start a background task to monitor for incoming messages and send those out via SignalR hub
            ILiveBroadcaster broadcaster = container.Resolve<ILiveBroadcaster>();
            Task.Factory.StartNew(() => broadcaster.Start());
        }
    }
}