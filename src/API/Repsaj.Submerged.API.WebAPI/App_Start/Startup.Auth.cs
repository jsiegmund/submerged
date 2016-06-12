using System;
using System.Collections.Generic;
using System.Configuration;
using System.IdentityModel.Tokens;
using System.Linq;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.ActiveDirectory;
using Owin;
using Microsoft.Owin.Security.WsFederation;
using Microsoft.Owin.Security.Cookies;
using Repsaj.Submerged.Common.Configurations;

namespace Repsaj.Submerged.API
{
    public partial class Startup
    {
        // For more information on configuring authentication, please visit http://go.microsoft.com/fwlink/?LinkId=301864
        public void ConfigureAuth(IAppBuilder app, IConfigurationProvider configProvider)
        {
            app.SetDefaultSignInAsAuthenticationType(CookieAuthenticationDefaults.AuthenticationType);

            // Primary authentication method for web site to Azure AD via the WsFederation below
            app.UseCookieAuthentication(new CookieAuthenticationOptions());

            string federationMetadataAddress = configProvider.GetConfigurationSettingValue("ida.FederationMetadataAddress");
            string federationRealm = configProvider.GetConfigurationSettingValue("ida.FederationRealm");

            if (string.IsNullOrEmpty(federationMetadataAddress) || string.IsNullOrEmpty(federationRealm))
            {
                throw new ApplicationException("Config issue: Unable to load required federation values from web.config or other configuration source.");
            }

            // check for default values that will cause app to fail to startup with an unhelpful 404 exception
            if (federationMetadataAddress.StartsWith("-- ", StringComparison.Ordinal) ||
                federationRealm.StartsWith("-- ", StringComparison.Ordinal))
            {
                throw new ApplicationException("Config issue: Default federation values from web.config need to be overridden or replaced.");
            }

            app.UseWsFederationAuthentication(
                new WsFederationAuthenticationOptions
                {
                    MetadataAddress = federationMetadataAddress,
                    Wtrealm = federationRealm
                });

            string aadTenant = configProvider.GetConfigurationSettingValue("ida.Tenant");
            string aadAudience = configProvider.GetConfigurationSettingValue("ida.Audience");

            if (string.IsNullOrEmpty(aadTenant) || string.IsNullOrEmpty(aadAudience))
            {
                throw new ApplicationException("Config issue: Unable to load required AAD values from web.config or other configuration source.");
            }

            // check for default values that will cause failure
            if (aadTenant.StartsWith("-- ", StringComparison.Ordinal) ||
                aadAudience.StartsWith("-- ", StringComparison.Ordinal))
            {
                throw new ApplicationException("Config issue: Default AAD values from web.config need to be overridden or replaced.");
            }

            app.UseWindowsAzureActiveDirectoryBearerAuthentication(
                new WindowsAzureActiveDirectoryBearerAuthenticationOptions
                { 
                    Tenant = aadTenant,
                    TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidAudience = aadAudience
                    },
                });
        }
    }
}
