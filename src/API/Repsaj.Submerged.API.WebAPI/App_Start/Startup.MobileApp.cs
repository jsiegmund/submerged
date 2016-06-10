using Microsoft.Azure.Mobile.Server;
using Microsoft.Azure.Mobile.Server.Authentication;
using Microsoft.Azure.Mobile.Server.Config;
using Microsoft.IdentityModel.Protocols;
using Owin;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Http;

namespace Repsaj.Submerged.API
{
    public partial class Startup
    {
        public static void ConfigureMobileApp(IAppBuilder app)
        {
            HttpConfiguration config = new HttpConfiguration();

            //For more information on Web API tracing, see http://go.microsoft.com/fwlink/?LinkId=620686 
            config.EnableSystemDiagnosticsTracing();

            new MobileAppConfiguration()
                .UseDefaultConfiguration()
                .ApplyTo(config);

            // To prevent Entity Framework from modifying your database schema, use a null database initializer
            // Database.SetInitializer<repsaj_neptune_mobileappContext>(null);
            //MobileAppSettingsDictionary settings = config.GetMobileAppSettingsProvider().GetMobileAppSettings();

            //if (string.IsNullOrEmpty(settings.HostName))
            //{
            //    // This middleware is intended to be used locally for debugging. By default, HostName will
            //    // only have a value when running in an App Service application.
            //    app.UseAppServiceAuthentication(new AppServiceAuthenticationOptions
            //    {
            //        SigningKey = ConfigurationManager.AppSettings["SigningKey"],
            //        ValidAudiences = new[] { ConfigurationManager.AppSettings["ValidAudience"] },
            //        ValidIssuers = new[] { ConfigurationManager.AppSettings["ValidIssuer"] },
            //        TokenHandler = config.GetAppServiceTokenHandler()
            //    });
            //}
        }
    }
}