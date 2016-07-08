using Autofac;
using Autofac.Integration.Mvc;
using Autofac.Integration.SignalR;
using Autofac.Integration.WebApi;
//using DeviceManagement.Infrustructure.Connectivity.Models.Security;
//using DeviceManagement.Infrustructure.Connectivity.Services;
using Repsaj.Submerged.Common.Configurations;
using Repsaj.Submerged.Infrastructure.BusinessLogic;
//using Common.Repository;
//using Common.Utility;
//using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.BusinessLogic;
using Repsaj.Submerged.Infrastructure.Repository;
using Microsoft.AspNet.SignalR;
using Repsaj.Submerged.API.Helpers;
using Newtonsoft.Json;
//using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.Helpers;
using Owin;
using System;
using System.Reflection;
using System.Web.Mvc;
using Repsaj.Submerged.Common.Utility;
using Repsaj.Submerged.API.Live;

namespace Repsaj.Submerged.API
{
    public partial class Startup
    {
        public IContainer ConfigureAutofac(IAppBuilder app)
        {
            var builder = new ContainerBuilder();

            // register the class that sets up bindings between interfaces and implementation
            builder.RegisterModule(new WebAutofacModule());

            // register configuration provider
            builder.RegisterType<ConfigurationProvider>().As<IConfigurationProvider>();

            // register Autofac w/ the MVC application
            builder.RegisterControllers(typeof(MvcApplication).Assembly);

            // Register the WebAPI controllers.
            builder.RegisterApiControllers(Assembly.GetExecutingAssembly());

            // Register json serializer with camlcasing
            //builder.Register(ctx => JsonSerializerFactory.Value).As<JsonSerializer>();

            var container = builder.Build();

            // Setup Autofac dependency resolver for MVC
            DependencyResolver.SetResolver(new Autofac.Integration.Mvc.AutofacDependencyResolver(container));

            // Setup Autofac dependency resolver for WebAPI
            Startup.HttpConfiguration.DependencyResolver = new AutofacWebApiDependencyResolver(container);

            // 1.  Register the Autofac middleware 
            // 2.  Register Autofac Web API middleware,
            // 3.  Register the standard Web API middleware (this call is made in the Startup.WebApi.cs)
            app.UseAutofacMiddleware(container);
            app.UseAutofacWebApi(Startup.HttpConfiguration);

            return container;
        }

        //private static readonly Lazy<JsonSerializer> JsonSerializerFactory = new Lazy<JsonSerializer>(GetJsonSerializer);

        //private static JsonSerializer GetJsonSerializer()
        //{
        //    return new JsonSerializer
        //    {
        //        ContractResolver = new FilteredCamelCasePropertyNamesContractResolver
        //        {

        //            TypesToInclude =
        //                        {
        //                            typeof(Infrastructure.Models.DeviceTelemetryModel),
        //                        }
        //        }
        //    };
        //}
    }

    public sealed class WebAutofacModule : Autofac.Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            //Logic
            //builder.RegisterType<KeyLogic>().As<IKeyLogic>();
            builder.RegisterType<DeviceLogic>().As<IDeviceLogic>();
            builder.RegisterType<DeviceRulesLogic>().As<IDeviceRulesLogic>();
            builder.RegisterType<TankLogLogic>().As<ITankLogLogic>();
            //builder.RegisterType<DeviceTypeLogic>().As<IDeviceTypeLogic>();
            builder.RegisterType<SecurityKeyGenerator>().As<ISecurityKeyGenerator>();
            //builder.RegisterType<ActionMappingLogic>().As<IActionMappingLogic>();
            builder.RegisterType<ActionLogic>().As<IActionLogic>();

            //builder.RegisterInstance(CommandParameterTypeLogic.Instance).As<ICommandParameterTypeLogic>();
            builder.RegisterType<DeviceTelemetryLogic>().As<IDeviceTelemetryLogic>();
            builder.RegisterType<SubscriptionLogic>().As<ISubscriptionLogic>();

            //builder.RegisterType<AlertsLogic>().As<IAlertsLogic>();

            //Repositories
            builder.RegisterType<IotHubRepository>().As<IIotHubRepository>();
            //builder.RegisterType<DeviceRegistryRepository>().As<IDeviceRegistryListRepository>();
            builder.RegisterType<DeviceRegistryRepository>().As<IDeviceRegistryCrudRepository>();
            builder.RegisterType<DeviceRulesRepository>().As<IDeviceRulesRepository>();
            builder.RegisterType<TankLogRepository>().As<ITankLogRepository>();
            //builder.RegisterType<SampleDeviceTypeRepository>().As<IDeviceTypeRepository>();
            //builder.RegisterType<VirtualDeviceTableStorage>().As<IVirtualDeviceStorage>();
            //builder.RegisterType<ActionMappingRepository>().As<IActionMappingRepository>();
            //builder.RegisterType<ActionRepository>().As<IActionRepository>();
            builder.RegisterType<DeviceTelemetryRepository>().As<IDeviceTelemetryRepository>();
            //builder.RegisterType<AlertsRepository>().As<IAlertsRepository>();
            //builder.RegisterType<UserSettingsRepository>().As<IUserSettingsRepository>();
            builder.RegisterType<DocDbRestUtility>().As<IDocDbRestUtility>();
            //builder.RegisterType<ApiRegistrationRepository>().As<IApiRegistrationRepository>();
            //builder.RegisterType<JasperCredentialsProvider>().As<ICredentialProvider>();
            //builder.RegisterType<JasperCellularService>().As<IExternalCellularService>();
            builder.RegisterType<SubscriptionRepository>().As<ISubscriptionRepository>();

            //Helpers
            builder.RegisterType<DocDbOperations>().As<IDocDbOperations>();
            builder.RegisterType<DocDbRestUtility>().As<IDocDbRestUtility>();
            builder.RegisterType<LiveBroadcaster>().As<ILiveBroadcaster>();
        }
    }
}