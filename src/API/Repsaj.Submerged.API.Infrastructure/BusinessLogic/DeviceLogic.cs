using Repsaj.Submerged.Common.Configurations;
using Repsaj.Submerged.Common.DeviceSchema;
using Repsaj.Submerged.Common.Helpers;
using Repsaj.Submerged.Common.Models;
using Repsaj.Submerged.Infrastructure.Exceptions;
using Repsaj.Submerged.Infrastructure.Models;
using Repsaj.Submerged.Infrastructure.Repository;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Dynamic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Text;
using System.Threading.Tasks;
using D = Dynamitey;

namespace Repsaj.Submerged.Infrastructure.BusinessLogic
{
    public class DeviceLogic : IDeviceLogic
    {
        private readonly IIotHubRepository _iotHubRepository;
        private readonly IDeviceRegistryCrudRepository _deviceRegistryCrudRepository;
        private readonly ISecurityKeyGenerator _securityKeyGenerator;
        private readonly IConfigurationProvider _configProvider;

        public DeviceLogic(IIotHubRepository iotHubRepository,  IDeviceRegistryCrudRepository deviceRegistryCrudRepository,
            ISecurityKeyGenerator securityKeyGenerator, IConfigurationProvider configProvider
            //, IVirtualDeviceStorage virtualDeviceStorage
            //IDeviceRegistryListRepository deviceRegistryListRepository, ,
            //, , IDeviceRulesLogic deviceRulesLogic
            )
        {
            _iotHubRepository = iotHubRepository;
            _deviceRegistryCrudRepository = deviceRegistryCrudRepository;
            //_deviceRegistryListRepository = deviceRegistryListRepository;
            //_virtualDeviceStorage = virtualDeviceStorage;
            _securityKeyGenerator = securityKeyGenerator;
            _configProvider = configProvider;
            //_deviceRulesLogic = deviceRulesLogic;
        }

        /// <summary>
        /// Adds a device to the Device Identity Store and Device Registry
        /// </summary>
        /// <param name="device">Device to add to the underlying repositories</param>
        /// <returns>Device created along with the device identity store keys</returns>
        public async Task<DeviceWithKeys> AddDeviceAsync(dynamic device)
        {
            // Validation logic throws an exception if it finds a validation error
            await ValidateDevice(device);

            SecurityKeys generatedSecurityKeys = _securityKeyGenerator.CreateRandomKeys();

            dynamic savedDevice = await AddDeviceToRepositoriesAsync(device, generatedSecurityKeys);
            return new DeviceWithKeys(savedDevice, generatedSecurityKeys);
        }

        /// <summary>
        /// Adds the given device and assigned keys to the underlying repositories 
        /// </summary>
        /// <param name="device">Device to add to repositories</param>
        /// <param name="securityKeys">Keys to assign to the device</param>
        /// <returns>Device that was added to the device registry</returns>
        private async Task<dynamic> AddDeviceToRepositoriesAsync(dynamic device, SecurityKeys securityKeys)
        {
            dynamic registryRepositoryDevice = null;
            ExceptionDispatchInfo capturedException = null;

            // if an exception happens at this point pass it up the stack to handle it
            // (Making this call first then the call against the Registry removes potential issues
            // with conflicting rollbacks if the operation happens to still be in progress.)
            await _iotHubRepository.AddDeviceAsync(device, securityKeys);

            try
            {
                registryRepositoryDevice = await _deviceRegistryCrudRepository.AddDeviceAsync(device);
            }
            catch (Exception ex)
            {
                // grab the exception so we can attempt an async removal of the device from the IotHub
                capturedException = ExceptionDispatchInfo.Capture(ex);

            }

            //Create a device in table storage if it is a simulated type of device 
            //and the document was stored correctly without an exception
            bool isSimulatedAsBool = false;
            try
            {
                isSimulatedAsBool = (bool)device.IsSimulatedDevice;
            }
            catch (InvalidCastException ex)
            {
                Trace.TraceError("The IsSimulatedDevice property was in an invalid format. Exception Error Message: {0}", ex.Message);
            }
            if (capturedException == null && isSimulatedAsBool)
            {
                try
                {
                    //await _virtualDeviceStorage.AddOrUpdateDeviceAsync(new InitialDeviceConfig()
                    //{
                    //    DeviceId = DeviceSchemaHelper.GetDeviceID(device),
                    //    HostName = _configProvider.GetConfigurationSettingValue("iotHub.HostName"),
                    //    Key = securityKeys.PrimaryKey
                    //});
                }
                catch (Exception ex)
                {
                    //if we fail adding to table storage for the device simulator just continue
                    Trace.TraceError("Failed to add simulated device : {0}", ex.Message);
                }
            }


            // Since the rollback code runs async and async code cannot run within the catch block it is run here
            if (capturedException != null)
            {
                // This is a lazy attempt to remove the device from the Iot Hub.  If it fails
                // the device will still remain in the Iot Hub.  A more robust rollback may be needed
                // in some scenarios.
                //await _iotHubRepository.TryRemoveDeviceAsync(DeviceSchemaHelper.GetDeviceID(device));
                capturedException.Throw();
            }

            return registryRepositoryDevice;
        }

        /// <summary>
        /// Retrieves the device with the provided device id from the device registry
        /// </summary>
        /// <param name="deviceId">ID of the device to retrieve</param>
        /// <returns>Fully populated device from the device registry</returns>
        public async Task<dynamic> GetDeviceAsync(string deviceId)
        {
            return await _deviceRegistryCrudRepository.GetDeviceAsync(deviceId);
        }

        /// <summary>
        /// Send a command to a device based on the provided device id
        /// </summary>
        /// <param name="deviceId">The Device's ID</param>
        /// <param name="commandName">The name of the command</param>
        /// <param name="parameters">The parameters to send</param>
        /// <returns></returns>
        public async Task SendCommandAsync(string deviceId, string commandName, dynamic parameters)
        {
            dynamic device = await GetDeviceAsync(deviceId);

            if (device == null)
            {
                throw new DeviceNotRegisteredException(deviceId);
            }

            await SendCommandAsyncWithDevice(device, commandName, parameters);
        }

        /// <summary>
        /// Sends a command to the provided device and updates the command history of the device
        /// </summary>
        /// <param name="device">Device to send the command to</param>
        /// <param name="commandName">Name of the command to send</param>
        /// <param name="parameters">Parameters to send with the command</param>
        /// <returns></returns>
        private async Task<dynamic> SendCommandAsyncWithDevice(dynamic device, string commandName, dynamic parameters)
        {
            string deviceId;

            if (device == null)
            {
                throw new ArgumentNullException("device");
            }

            //bool canDevicePerformCommand = CommandSchemaHelper.CanDevicePerformCommand(device, commandName);

            deviceId = DeviceSchemaHelper.GetDeviceID(device);

            //if (!canDevicePerformCommand)
            //{
            //    throw new UnsupportedCommandException(deviceId, commandName);
            //}

            dynamic command = CommandHistorySchemaHelper.BuildNewCommandHistoryItem(commandName);
            CommandHistorySchemaHelper.AddParameterCollectionToCommandHistoryItem(command, parameters);

            CommandHistorySchemaHelper.AddCommandToHistory(device, command);

            await _iotHubRepository.SendCommand(deviceId, command);
            //await _deviceRegistryCrudRepository.UpdateDeviceAsync(device);

            return command;
        }

        private async Task ValidateDevice(dynamic device)
        {
            List<string> validationErrors = new List<string>();

            if (ValidateDeviceId(device, validationErrors))
            {
                await CheckIfDeviceExists(device, validationErrors);
            }

            if (validationErrors.Count > 0)
            {
                var validationException =
                    new DeviceValidationException(DeviceSchemaHelper.GetDeviceProperties(device) != null ? DeviceSchemaHelper.GetDeviceID(device) : null);

                foreach (string error in validationErrors)
                {
                    validationException.Errors.Add(error);
                }

                throw validationException;
            }
        }


        private async Task CheckIfDeviceExists(dynamic device, List<string> validationErrors)
        {
            // check if device exists
            if (await GetDeviceAsync(DeviceSchemaHelper.GetDeviceID(device)) != null)
            {
                validationErrors.Add(Strings.ValidationDeviceExists);
            }
        }

        private bool ValidateDeviceId(dynamic device, List<string> validationErrors)
        {
            if (DeviceSchemaHelper.GetDeviceProperties(device) == null || string.IsNullOrWhiteSpace(DeviceSchemaHelper.GetDeviceID(device)))
            {
                validationErrors.Add(Strings.ValidationDeviceIdMissing);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Updates the device in the device registry with the exact device provided in this call.
        /// NOTE: The device provided here should represent the entire device that will be 
        /// serialized into the device registry.
        /// </summary>
        /// <param name="device">Device to update in the device registry</param>
        /// <returns>Device that was saved into the device registry</returns>
        public async Task<dynamic> UpdateDeviceAsync(dynamic device)
        {
            return await _deviceRegistryCrudRepository.UpdateDeviceAsync(device);
        }

        /// <summary>
        /// Used by the event processor to update the initial data for the device
        /// without deleting the CommandHistory and the original created date
        /// This assumes the device controls and has full knowledge of its metadata except for:
        /// - CreatedTime
        /// - CommandHistory
        /// </summary>
        /// <param name="device">Device information to save to the backend Device Registry</param>
        /// <returns>Combined device that was saved to registry</returns>
        public async Task<dynamic> UpdateDeviceFromDeviceInfoPacketAsync(dynamic device)
        {
            if (device == null)
            {
                throw new ArgumentNullException("device");
            }

            dynamic existingDevice = await GetDeviceAsync(DeviceSchemaHelper.GetDeviceID(device));

            // Save the command history, original created date, and system properties (if any) of the existing device
            if (DeviceSchemaHelper.GetDeviceProperties(existingDevice) != null)
            {
                dynamic deviceProperties = DeviceSchemaHelper.GetDeviceProperties(device);
                deviceProperties.CreatedTime = DeviceSchemaHelper.GetCreatedTime(existingDevice);
            }

            device.CommandHistory = existingDevice.CommandHistory;

            // Copy the existing system properties, or initialize them if they do not exist
            if (existingDevice.SystemProperties != null)
            {
                device.SystemProperties = existingDevice.SystemProperties;
            }
            else
            {
                DeviceSchemaHelper.InitializeSystemProperties(device, null);
            }

            return await _deviceRegistryCrudRepository.UpdateDeviceAsync(device);
        }

        #region Device Properties
        /// <summary>
        /// Modified a Device using a list of 
        /// <see cref="DevicePropertyValueModel" />.
        /// </summary>
        /// <param name="device">
        /// The Device to modify.
        /// </param>
        /// <param name="devicePropertyValueModels">
        /// The list of <see cref="DevicePropertyValueModel" />s for modifying 
        /// <paramref name="device" />.
        /// </param>
        public void ApplyDevicePropertyValueModels(
            dynamic device,
            IEnumerable<DevicePropertyValueModel> devicePropertyValueModels)
        {
            dynamic deviceProperties;
            IDynamicMetaObjectProvider dynamicMetaObjectProvider;
            ICustomTypeDescriptor typeDescriptor;

            if (object.ReferenceEquals(device, null))
            {
                throw new ArgumentNullException("device");
            }

            if (devicePropertyValueModels == null)
            {
                throw new ArgumentNullException("devicePropertyValueModels");
            }

            deviceProperties = DeviceSchemaHelper.GetDeviceProperties(device);
            if (object.ReferenceEquals(deviceProperties, null))
            {
                throw new ArgumentException("device.DeviceProperties is a null reference.", "device");
            }

            if ((dynamicMetaObjectProvider = deviceProperties as IDynamicMetaObjectProvider) != null)
            {
                ApplyPropertyValueModels(dynamicMetaObjectProvider, devicePropertyValueModels);
            }
            else if ((typeDescriptor = deviceProperties as ICustomTypeDescriptor) != null)
            {
                ApplyPropertyValueModels(typeDescriptor, devicePropertyValueModels);
            }
            else
            {
                ApplyPropertyValueModels((object)deviceProperties, devicePropertyValueModels);
            }
        }

        /// <summary>
        /// Gets <see cref="DevicePropertyValueModel" /> for an edited Device's 
        /// properties.
        /// </summary>
        /// <param name="device">
        /// The edited Device.
        /// </param>
        /// <returns>
        /// <see cref="DevicePropertyValueModel" />s, representing 
        /// <paramref name="device" />'s properties.
        /// </returns>
        public IEnumerable<DevicePropertyValueModel> ExtractDevicePropertyValuesModels(
            dynamic device)
        {
            dynamic deviceProperties;
            IDynamicMetaObjectProvider dynamicMetaObjectProvider;
            string hostNameValue;
            IEnumerable<DevicePropertyValueModel> propValModels;
            ICustomTypeDescriptor typeDescriptor;

            if (object.ReferenceEquals(device, null))
            {
                throw new ArgumentNullException("device");
            }

            deviceProperties = DeviceSchemaHelper.GetDeviceProperties(device);
            if (object.ReferenceEquals(deviceProperties, null))
            {
                throw new ArgumentException("device.DeviceProperties is a null reference.", "device");
            }

            if ((dynamicMetaObjectProvider = deviceProperties as IDynamicMetaObjectProvider) != null)
            {
                propValModels = ExtractPropertyValueModels(dynamicMetaObjectProvider);
            }
            else if ((typeDescriptor = deviceProperties as ICustomTypeDescriptor) != null)
            {
                propValModels = ExtractPropertyValueModels(typeDescriptor);
            }
            else
            {
                propValModels = ExtractPropertyValueModels((object)deviceProperties);
            }

            hostNameValue = _configProvider.GetConfigurationSettingValue("iotHub.HostName");

            if (!string.IsNullOrEmpty(hostNameValue))
            {
                propValModels = propValModels.Concat(
                        new DevicePropertyValueModel[]
                        {
                            new DevicePropertyValueModel()
                            {
                                DisplayOrder = 0,
                                IsEditable = false,
                                IsIncludedWithUnregisteredDevices = true,
                                Name = "HostName",
                                PropertyType = Models.PropertyType.String,
                                Value = hostNameValue
                            }
                        });
            }

            return propValModels;
        }

        private static void ApplyPropertyValueModels(
            object deviceProperties,
            IEnumerable<DevicePropertyValueModel> devicePropertyValueModels)
        {
            object[] args;
            TypeConverter converter;
            Type devicePropertiesType;
            Dictionary<string, DevicePropertyMetadata> devicePropertyIndex;
            Dictionary<string, PropertyInfo> propIndex;
            PropertyInfo propInfo;
            DevicePropertyMetadata propMetadata;
            MethodInfo setter;

            Debug.Assert(deviceProperties != null, "deviceProperties is a null reference.");

            Debug.Assert(devicePropertyValueModels != null, "devicePropertyValueModels is a null reference.");

            devicePropertyIndex = GetDevicePropertyConfiguration().ToDictionary(t => t.Name);

            devicePropertiesType = deviceProperties.GetType();
            propIndex = devicePropertiesType.GetProperties().ToDictionary(t => t.Name);

            args = new object[1];
            foreach (DevicePropertyValueModel propVal in devicePropertyValueModels)
            {
                if ((propVal == null) ||
                    string.IsNullOrEmpty(propVal.Name))
                {
                    continue;
                }

                // Pass through properties that don't have a specified 
                // configuration.
                if (devicePropertyIndex.TryGetValue(propVal.Name, out propMetadata) && !propMetadata.IsEditable)
                {
                    continue;
                }

                if (!propIndex.TryGetValue(propVal.Name, out propInfo) ||
                    ((setter = propInfo.GetSetMethod()) == null) ||
                    ((converter = TypeDescriptor.GetConverter(propInfo.PropertyType)) == null))
                {
                    continue;
                }

                try
                {
                    args[0] = converter.ConvertFromString(propVal.Value);
                }
                catch (NotSupportedException ex)
                {
                    throw new InvalidOperationException(
                        string.Format(
                            CultureInfo.InvariantCulture,
                            "Unable to assign value, \"{0},\" to Device property, {1}.",
                            propVal.Value,
                            propInfo.Name),
                        ex);
                }

                setter.Invoke(deviceProperties, args);
            }
        }

        private static void ApplyPropertyValueModels(
            ICustomTypeDescriptor deviceProperties,
            IEnumerable<DevicePropertyValueModel> devicePropertyValueModels)
        {
            Dictionary<string, DevicePropertyMetadata> devicePropertyIndex;
            Dictionary<string, PropertyDescriptor> propIndex;
            PropertyDescriptor propDesc;
            DevicePropertyMetadata propMetadata;

            Debug.Assert(deviceProperties != null, "deviceProperties is a null reference.");

            Debug.Assert(devicePropertyValueModels != null, "devicePropertyValueModels is a null reference.");

            devicePropertyIndex = GetDevicePropertyConfiguration().ToDictionary(t => t.Name);

            propIndex = new Dictionary<string, PropertyDescriptor>();
            foreach (PropertyDescriptor pd in deviceProperties.GetProperties())
            {
                propIndex[pd.Name] = pd;
            }

            foreach (DevicePropertyValueModel propVal in devicePropertyValueModels)
            {
                if ((propVal == null) || string.IsNullOrEmpty(propVal.Name))
                {
                    continue;
                }

                // Pass through properties that don't have a specified 
                // configuration.
                if (devicePropertyIndex.TryGetValue(propVal.Name, out propMetadata) && !propMetadata.IsEditable)
                {
                    continue;
                }

                if (!propIndex.TryGetValue(propVal.Name, out propDesc) || propDesc.IsReadOnly)
                {
                    continue;
                }

                propDesc.SetValue(deviceProperties, propVal.Value);
            }
        }

        private static void ApplyPropertyValueModels(
            IDynamicMetaObjectProvider deviceProperties,
            IEnumerable<DevicePropertyValueModel> devicePropertyValueModels)
        {
            Dictionary<string, DevicePropertyMetadata> devicePropertyIndex;
            HashSet<string> dynamicProperties;
            DevicePropertyMetadata propMetadata;

            Debug.Assert(
                deviceProperties != null,
                "deviceProperties is a null reference.");

            Debug.Assert(
                devicePropertyValueModels != null,
                "devicePropertyValueModels is a null reference.");

            devicePropertyIndex =
                GetDevicePropertyConfiguration().ToDictionary(t => t.Name);

            dynamicProperties =
                new HashSet<string>(
                    D.Dynamic.GetMemberNames(deviceProperties, true));

            foreach (DevicePropertyValueModel propVal in devicePropertyValueModels)
            {
                if ((propVal == null) ||
                    string.IsNullOrEmpty(propVal.Name))
                {
                    continue;
                }

                if (!dynamicProperties.Contains(propVal.Name))
                {
                    continue;
                }

                // Pass through properties that don't have a specified 
                // configuration.
                if (devicePropertyIndex.TryGetValue(propVal.Name, out propMetadata) && !propMetadata.IsEditable)
                {
                    continue;
                }

                D.Dynamic.InvokeSet(
                    deviceProperties,
                    propVal.Name,
                    propVal.Value);
            }
        }


        private static IEnumerable<DevicePropertyValueModel> ExtractPropertyValueModels(
            ICustomTypeDescriptor deviceProperties)
        {
            DevicePropertyValueModel currentData;
            object currentValue;
            Dictionary<string, DevicePropertyMetadata> devicePropertyIndex;
            int editableOrdering;
            bool isDisplayedRegistered;
            bool isDisplayedUnregistered;
            bool isEditable;
            int nonediableOrdering;
            DevicePropertyMetadata propertyMetadata;

            Debug.Assert(deviceProperties != null, "deviceProperties is a null reference.");

            devicePropertyIndex = GetDevicePropertyConfiguration().ToDictionary(t => t.Name);

            // For now, display r/o properties first.
            editableOrdering = 1;
            nonediableOrdering = int.MinValue;

            foreach (PropertyDescriptor prop in deviceProperties.GetProperties())
            {
                if (devicePropertyIndex.TryGetValue(prop.Name, out propertyMetadata))
                {
                    isDisplayedRegistered = propertyMetadata.IsDisplayedForRegisteredDevices;
                    isDisplayedUnregistered = propertyMetadata.IsDisplayedForUnregisteredDevices;
                    isEditable = propertyMetadata.IsEditable;

                }
                else
                {
                    isDisplayedRegistered = isEditable = true;
                    isDisplayedUnregistered = false;
                }

                if (!isDisplayedRegistered && !isDisplayedUnregistered)
                {
                    continue;
                }

                // Mark R/O properties as not-ediable.
                if (prop.IsReadOnly)
                {
                    isEditable = false;
                }

                currentData = new DevicePropertyValueModel()
                {
                    Name = prop.Name,
                    PropertyType = propertyMetadata.PropertyType
                };

                if (isEditable)
                {
                    currentData.IsEditable = true;
                    currentData.DisplayOrder = editableOrdering++;
                }
                else
                {
                    currentData.IsEditable = false;
                    currentData.DisplayOrder = nonediableOrdering++;
                }

                currentData.IsIncludedWithUnregisteredDevices = isDisplayedUnregistered;

                currentValue = prop.GetValue(deviceProperties);
                if (currentValue == null)
                {
                    currentData.Value = string.Empty;
                }
                else
                {
                    currentData.Value = string.Format(CultureInfo.InvariantCulture, "{0}", currentValue);
                }

                yield return currentData;
            }
        }

        private static IEnumerable<DevicePropertyValueModel> ExtractPropertyValueModels(
            object deviceProperties)
        {
            DevicePropertyValueModel currentData;
            object currentValue;
            Dictionary<string, DevicePropertyMetadata> devicePropertyIndex;
            Type devicePropertiesType;
            bool isDisplayedRegistered;
            bool isDisplayedUnregistered;
            bool isEditable;
            int editableOrdering;
            MethodInfo getMethod;
            int nonediableOrdering;
            DevicePropertyMetadata propertyMetadata;

            Debug.Assert(deviceProperties != null, "deviceProperties is a null reference.");

            devicePropertyIndex = GetDevicePropertyConfiguration().ToDictionary(t => t.Name);

            // For now, display r/o properties first.
            editableOrdering = 1;
            nonediableOrdering = int.MinValue;

            devicePropertiesType = deviceProperties.GetType();
            foreach (PropertyInfo prop in devicePropertiesType.GetProperties())
            {
                if (devicePropertyIndex.TryGetValue(
                    prop.Name,
                    out propertyMetadata))
                {
                    isDisplayedRegistered = propertyMetadata.IsDisplayedForRegisteredDevices;
                    isDisplayedUnregistered = propertyMetadata.IsDisplayedForUnregisteredDevices;
                    isEditable = propertyMetadata.IsEditable;
                }
                else
                {
                    isDisplayedRegistered = isEditable = true;
                    isDisplayedUnregistered = false;
                }

                if (!isDisplayedRegistered && !isDisplayedUnregistered)
                {
                    continue;
                }

                if ((getMethod = prop.GetGetMethod()) == null)
                {
                    continue;
                }

                // Mark R/O properties as not-ediable.
                if (!prop.CanWrite)
                {
                    isEditable = false;
                }

                currentData = new DevicePropertyValueModel()
                {
                    Name = prop.Name,
                    PropertyType = propertyMetadata.PropertyType
                };

                if (isEditable)
                {
                    currentData.IsEditable = true;
                    currentData.DisplayOrder = editableOrdering++;
                }
                else
                {
                    currentData.IsEditable = false;
                    currentData.DisplayOrder = nonediableOrdering++;
                }

                currentData.IsIncludedWithUnregisteredDevices = isDisplayedUnregistered;

                currentValue = getMethod.Invoke(deviceProperties, ReflectionHelper.EmptyArray);
                if (currentValue == null)
                {
                    currentData.Value = string.Empty;
                }
                else
                {
                    currentData.Value = string.Format(CultureInfo.InvariantCulture, "{0}", currentValue);
                }

                yield return currentData;
            }
        }

        private static IEnumerable<DevicePropertyValueModel> ExtractPropertyValueModels(
            IDynamicMetaObjectProvider deviceProperties)
        {
            DevicePropertyValueModel currentData;
            object currentValue;
            Dictionary<string, DevicePropertyMetadata> devicePropertyIndex;
            int editableOrdering;
            bool isDisplayedRegistered;
            bool isDisplayedUnregistered;
            bool isEditable;
            int nonediableOrdering;
            DevicePropertyMetadata propertyMetadata;
            PropertyType propertyType;

            Debug.Assert(deviceProperties != null, "deviceProperties is a null reference.");

            devicePropertyIndex = GetDevicePropertyConfiguration().ToDictionary(t => t.Name);

            // For now, display r/o properties first.
            editableOrdering = 1;
            nonediableOrdering = int.MinValue;

            foreach (string propertyName in D.Dynamic.GetMemberNames(deviceProperties, true))
            {
                if (devicePropertyIndex.TryGetValue(propertyName, out propertyMetadata))
                {
                    isDisplayedRegistered = propertyMetadata.IsDisplayedForRegisteredDevices;
                    isDisplayedUnregistered = propertyMetadata.IsDisplayedForUnregisteredDevices;
                    isEditable = propertyMetadata.IsEditable;

                    propertyType = propertyMetadata.PropertyType;
                }
                else
                {
                    isDisplayedRegistered = isEditable = true;
                    isDisplayedUnregistered = false;

                    propertyType = PropertyType.String;
                }

                if (!isDisplayedRegistered && !isDisplayedUnregistered)
                {
                    continue;
                }

                currentData = new DevicePropertyValueModel()
                {
                    Name = propertyName,
                    PropertyType = propertyType
                };

                if (isEditable)
                {
                    currentData.IsEditable = true;
                    currentData.DisplayOrder = editableOrdering++;
                }
                else
                {
                    currentData.IsEditable = false;
                    currentData.DisplayOrder = nonediableOrdering++;
                }

                currentData.IsIncludedWithUnregisteredDevices =
                    isDisplayedUnregistered;

                currentValue = D.Dynamic.InvokeGet(deviceProperties, propertyName);
                if (currentValue == null)
                {
                    currentData.Value = string.Empty;
                }
                else
                {
                    currentData.Value = string.Format(CultureInfo.InvariantCulture, "{0}", currentValue);
                }

                yield return currentData;
            }
        }

        private static IEnumerable<DevicePropertyMetadata> GetDevicePropertyConfiguration()
        {
            // Only return metadata for fields that aren't handled in the 
            // standard way.

            // TODO: Drive this from data?
            yield return new DevicePropertyMetadata()
            {
                IsDisplayedForRegisteredDevices = true,
                IsDisplayedForUnregisteredDevices = true,
                IsEditable = false,
                Name = "DeviceID"
            };

            yield return new DevicePropertyMetadata()
            {
                IsDisplayedForRegisteredDevices = true,
                IsDisplayedForUnregisteredDevices = true,
                IsEditable = false,
                Name = "CreatedTime",
                PropertyType = PropertyType.DateTime
            };

            yield return new DevicePropertyMetadata()
            {
                IsDisplayedForRegisteredDevices = true,
                IsDisplayedForUnregisteredDevices = false,
                IsEditable = false,
                Name = "DeviceState",
                PropertyType = PropertyType.Status
            };

            yield return new DevicePropertyMetadata()
            {
                IsDisplayedForRegisteredDevices = false,
                IsDisplayedForUnregisteredDevices = false,
                IsEditable = false,
                Name = "HostName"
            };

            // Do not show a Device field, HubEnabledState.  One will be added 
            // programatically from settings.
            yield return new DevicePropertyMetadata()
            {
                IsDisplayedForRegisteredDevices = true,
                IsDisplayedForUnregisteredDevices = false,
                IsEditable = false,
                Name = "HubEnabledState",
                PropertyType = PropertyType.Status
            };

            yield return new DevicePropertyMetadata()
            {
                IsDisplayedForRegisteredDevices = true,
                IsDisplayedForUnregisteredDevices = false,
                IsEditable = false,
                Name = "UpdatedTime",
                PropertyType = PropertyType.DateTime
            };
        }
        #endregion
    }
}
