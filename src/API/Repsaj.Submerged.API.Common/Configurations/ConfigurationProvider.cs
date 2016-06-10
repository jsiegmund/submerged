using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.ServiceRuntime;
using System.Configuration;

namespace Repsaj.Submerged.Common.Configurations
{
    public class ConfigurationProvider : IConfigurationProvider, IDisposable
    {
        readonly Dictionary<string, string> configuration = new Dictionary<string, string>();
        //EnvironmentDescription environment = null;
        const string ConfigToken = "config:";
        bool _disposed = false;

        public string GetConfigurationSettingValue(string configurationSettingName)
        {
            return this.GetConfigurationSettingValueOrDefault(configurationSettingName, string.Empty);
        }

        public string GetConfigurationSettingValueOrDefault(string configurationSettingName, string defaultValue)
        {
            try
            {
                if (!this.configuration.ContainsKey(configurationSettingName))
                {
                    string configValue = string.Empty;
                    bool isEmulated = true;
                    bool isAvailable = false;
                    try
                    {
                        isAvailable = RoleEnvironment.IsAvailable;
                    }
                    catch (TypeInitializationException) { }
                    if (isAvailable)
                    {
                        configValue = RoleEnvironment.GetConfigurationSettingValue(configurationSettingName);
                        isEmulated = RoleEnvironment.IsEmulated;
                    }
                    else
                    {
                        configValue = ConfigurationManager.AppSettings[configurationSettingName];
                        isEmulated = Environment.CommandLine.Contains("iisexpress.exe") ||
                            Environment.CommandLine.Contains("WebJob.vshost.exe");
                    }

                    try
                    {
                        this.configuration.Add(configurationSettingName, configValue);
                    }
                    catch (ArgumentException)
                    {
                        // at this point, this key has already been added on a different
                        // thread, so we're fine to continue
                    }
                }
            }
            catch (RoleEnvironmentException)
            {
                if (string.IsNullOrEmpty(defaultValue))
                    throw;

                this.configuration.Add(configurationSettingName, defaultValue);
            }
            return this.configuration[configurationSettingName];
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
        }

        ~ConfigurationProvider()
        {
            Dispose(false);
        }
    }
}