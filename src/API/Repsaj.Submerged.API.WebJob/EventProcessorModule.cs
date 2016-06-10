using Autofac;
using Repsaj.Submerged.Common.Configurations;
using Repsaj.Submerged.Infrastructure.BusinessLogic;
using Repsaj.Submerged.Infrastructure.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Repsaj.Submerged.WebJob.Processors;
using Repsaj.Submerged.Common.Utility;

namespace Repsaj.Submerged.WebJob
{
    public sealed class EventProcessorModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<ConfigurationProvider>()
                .As<IConfigurationProvider>()
                .SingleInstance();

            builder.RegisterType<DeviceEventProcessor>()
                .As<IDeviceEventProcessor>()
                .SingleInstance();

            builder.RegisterType<ActionEventProcessor>()
                .As<IActionEventProcessor>()
                .SingleInstance();

            builder.RegisterType<DeviceLogic>()
                .As<IDeviceLogic>();

            builder.RegisterType<DeviceRulesLogic>()
                .As<IDeviceRulesLogic>();

            builder.RegisterType<DeviceRegistryRepository>()
                .As<IDeviceRegistryCrudRepository>();

            builder.RegisterType<SubscriptionRepository>()
                .As<ISubscriptionRepository>();

            builder.RegisterType<SubscriptionLogic>()
                .As<ISubscriptionLogic>();

            //builder.RegisterType<DeviceRegistryRepository>()
            //    .As<IDeviceRegistryListRepository>();

            builder.RegisterType<DeviceRulesRepository>()
                .As<IDeviceRulesRepository>();

            builder.RegisterType<IotHubRepository>()
                .As<IIotHubRepository>();

            builder.RegisterType<SecurityKeyGenerator>()
                .As<ISecurityKeyGenerator>();

            //builder.RegisterType<VirtualDeviceTableStorage>()
            //    .As<IVirtualDeviceStorage>();

            //builder.RegisterType<ActionMappingLogic>()
             //   .As<IActionMappingLogic>();

            //builder.RegisterType<ActionMappingRepository>()
            //    .As<IActionMappingRepository>();

            builder.RegisterType<ActionLogic>()
                .As<IActionLogic>();

            builder.RegisterType<ActionRepository>()
                .As<IActionRepository>();

            builder.RegisterType<DocDbRestUtility>()
                .As<IDocDbRestUtility>();

            builder.RegisterType<DocDbOperations>()
                .As<IDocDbOperations>();

            //builder.RegisterType<MessageFeedbackProcessor>()
            //    .As<IMessageFeedbackProcessor>().SingleInstance();
        }
    }
}
