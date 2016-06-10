using Repsaj.Submerged.Common.Configurations;
using Repsaj.Submerged.Infrastructure.BusinessLogic;
using Microsoft.ServiceBus.Messaging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repsaj.Submerged.WebJob.Processors
{
    public class DeviceAdministrationProcessorFactory : IEventProcessorFactory
    {
        private readonly IDeviceLogic _deviceLogic;
        private readonly ISubscriptionLogic _subscriptionLogic;
        private readonly IConfigurationProvider _configurationProvider;

        readonly ConcurrentDictionary<string, DeviceAdministrationProcessor> eventProcessors = new ConcurrentDictionary<string, DeviceAdministrationProcessor>();
        readonly ConcurrentQueue<DeviceAdministrationProcessor> closedProcessors = new ConcurrentQueue<DeviceAdministrationProcessor>();

        public DeviceAdministrationProcessorFactory(IDeviceLogic deviceLogic, ISubscriptionLogic subscriptionLogic, IConfigurationProvider configurationProvider)
        {
            _deviceLogic = deviceLogic;
            _subscriptionLogic = subscriptionLogic;
            _configurationProvider = configurationProvider;
        }

        public int ActiveProcessors
        {
            get { return this.eventProcessors.Count; }
        }

        public int TotalMessages
        {
            get
            {
                var amount = this.eventProcessors.Select(p => p.Value.TotalMessages).Sum();
                amount += this.closedProcessors.Select(p => p.TotalMessages).Sum();
                return amount;
            }
        }

        public IEventProcessor CreateEventProcessor(PartitionContext context)
        {
            var processor = new DeviceAdministrationProcessor(_deviceLogic, _subscriptionLogic, _configurationProvider);
            processor.ProcessorClosed += this.ProcessorOnProcessorClosed;
            this.eventProcessors.TryAdd(context.Lease.PartitionId, processor);
            return processor;
        }

        public Task WaitForAllProcessorsInitialized(TimeSpan timeout)
        {
            return this.WaitForAllProcessorsCondition(p => p.IsInitialized, timeout);
        }

        public Task WaitForAllProcessorsClosed(TimeSpan timeout)
        {
            return this.WaitForAllProcessorsCondition(p => p.IsClosed, timeout);
        }

        public async Task WaitForAllProcessorsCondition(Func<DeviceAdministrationProcessor, bool> predicate, TimeSpan timeout)
        {
            TimeSpan sleepInterval = TimeSpan.FromSeconds(2);
            while (!this.eventProcessors.Values.All(predicate))
            {
                if (timeout > sleepInterval)
                {
                    timeout = timeout.Subtract(sleepInterval);
                }
                else
                {
                    throw new TimeoutException("Condition not satisfied within expected timeout.");
                }
                await Task.Delay(sleepInterval);
            }
        }

        public void ProcessorOnProcessorClosed(object sender, EventArgs eventArgs)
        {
            var processor = sender as DeviceAdministrationProcessor;
            if (processor != null)
            {
                this.eventProcessors.TryRemove(processor.Context.Lease.PartitionId, out processor);
                this.closedProcessors.Enqueue(processor);
            }
        }
    }
}
