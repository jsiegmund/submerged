using Autofac;
using Repsaj.Submerged.Common.Configurations;
using Repsaj.Submerged.Common.Utility;
using Repsaj.Submerged.Infrastructure.BusinessLogic;
using Repsaj.Submerged.Infrastructure.Repository;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repsaj.Submerged.APITests
{
    public class TestBase
    {
        private IContainer _autofacContainer;
        protected IContainer AutofacContainer
        {
            get
            {
                if (_autofacContainer == null)
                {
                    var builder = new ContainerBuilder();

                    // register configuration provider
                    builder.RegisterType<ConfigurationProvider>().As<IConfigurationProvider>();

                    // Logic
                    builder.RegisterType<DeviceTelemetryLogic>().As<IDeviceTelemetryLogic>();
                    builder.RegisterType<DeviceRulesLogic>().As<IDeviceRulesLogic>();
                    builder.RegisterType<DeviceLogic>().As<IDeviceLogic>();
                    builder.RegisterType<SubscriptionLogic>().As<ISubscriptionLogic>();

                    // Repositories
                    builder.RegisterType<DeviceTelemetryRepository>().As<IDeviceTelemetryRepository>();
                    builder.RegisterType<DeviceRulesRepository>().As<IDeviceRulesRepository>();
                    builder.RegisterType<DeviceRegistryRepository>().As<IDeviceRegistryCrudRepository>();
                    builder.RegisterType<IotHubRepository>().As<IIotHubRepository>();
                    builder.RegisterType<SubscriptionRepository>().As<ISubscriptionRepository>();

                    // Utilities
                    builder.RegisterType<DocDbOperations>().As<IDocDbOperations>();
                    builder.RegisterType<DocDbRestUtility>().As<IDocDbRestUtility>();
                    builder.RegisterType<SecurityKeyGenerator>().As<ISecurityKeyGenerator>();

                    var container = builder.Build();

                    _autofacContainer = container;
                }

                return _autofacContainer;
            }
        }

        public string DeviceId
        {
            get { return ConfigurationManager.AppSettings["deviceId"]; }
        }

        public string SubscriptionUser
        {
            get { return ConfigurationManager.AppSettings["subscriptionUser"]; }
        } 

        public string SubscriptionName
        {
            get { return ConfigurationManager.AppSettings["subscriptionName"]; }
        }

        public string TankName
        {
            get { return ConfigurationManager.AppSettings["tankName"]; }
        }

        protected IDeviceTelemetryRepository DeviceTelemetryRepository
        {
            get
            {
                return AutofacContainer.Resolve<IDeviceTelemetryRepository>();
            }
        }

        protected IDeviceTelemetryLogic DeviceTelemetryLogic
        {
            get
            {
                return AutofacContainer.Resolve<IDeviceTelemetryLogic>();
            }
        }

        protected IDeviceRulesRepository DeviceRulesRepository
        {
            get
            {
                return AutofacContainer.Resolve<IDeviceRulesRepository>();
            }
        }

        protected IDeviceRulesLogic DeviceRulesLogic
        {
            get
            {
                return AutofacContainer.Resolve<IDeviceRulesLogic>();
            }
        }

        protected IDeviceLogic DeviceLogic
        {
            get
            {
                return AutofacContainer.Resolve<IDeviceLogic>();
            }
        }

        protected IDocDbOperations DocDbOperations
        {
            get
            {
                return AutofacContainer.Resolve<IDocDbOperations>();
            }
        }

        protected ISubscriptionLogic SubscriptionLogic
        {
            get
            {
                return AutofacContainer.Resolve<ISubscriptionLogic>();
            }
        }
    }
}
