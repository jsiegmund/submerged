using Autofac;
using Autofac.Core;
using Repsaj.Submerged.GatewayApp.Device;
using Repsaj.Submerged.GatewayApp.Universal.Azure;
using Repsaj.Submerged.GatewayApp.Universal.Commands;
using Repsaj.Submerged.GatewayApp.Universal.Device;
using Repsaj.Submerged.GatewayApp.Universal.Modules;
using Repsaj.Submerged.GatewayApp.Universal.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Repsaj.Submerged.GatewayApp
{
    class Autofac
    {
        private static IContainer _container;

        static internal IContainer Container
        {
            get { return _container; }
        }

        static internal void InitializeAutofac()
        {
            var builder = new ContainerBuilder();

            // register all implementations
            builder.RegisterType<StorageRepository>().As<IStorageRepository>();
            builder.RegisterType<ConfigurationRepository>().As<IConfigurationRepository>();
            builder.RegisterType<SensorDatastore>().As<ISensorDataStore>();
            builder.RegisterType<AzureConnection>().As<IAzureConnection>();

            // register single instances
            builder.RegisterType<CommandProcessorFactory>().As<ICommandProcessorFactory>().SingleInstance();
            builder.RegisterType<ModuleConnectionManager>().As<IModuleConnectionManager>().SingleInstance();
            builder.RegisterType<ModuleConnectionFactory>().As<IModuleConnectionFactory>().SingleInstance();

            builder.RegisterType<DeviceManager>().As<IDeviceManager>().SingleInstance()
                   .WithParameter(new ResolvedParameter(
                        (pi, ctx) => pi.ParameterType == typeof(SynchronizationContext),
                        (pi, ctx) => SynchronizationContext.Current));

            // register command processors
            builder.RegisterType<SwitchRelayCommandProcessor>().As<SwitchRelayCommandProcessor>();
            builder.RegisterType<DeviceInfoCommandProcessor>().As<DeviceInfoCommandProcessor>();
            builder.RegisterType<ModuleCommandProcessor>().As<ModuleCommandProcessor>();

            _container = builder.Build();
        }
    }
}
