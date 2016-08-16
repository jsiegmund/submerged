using Microsoft.AspNet.SignalR;
using Repsaj.Submerged.API.Helpers;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Formatting;
using System.Web;

namespace Repsaj.Submerged.API
{
    public partial class Startup
    {
        public void ConfigureJson(IAppBuilder app)
        {
            MediaTypeFormatterCollection formatters = Startup.HttpConfiguration.Formatters;

            JsonMediaTypeFormatter jsonFormatter = formatters.JsonFormatter;
            jsonFormatter.UseDataContractJsonSerializer = false;

            JsonSerializerSettings settings = jsonFormatter.SerializerSettings;
            settings.ContractResolver = new CamelCasePropertyNamesContractResolver();

            // JSON serializer settings for AutoFac have been moved 

            //// Create serializer object with camlcasing settings to use with SignalR hub
            //var signalRettings = new JsonSerializerSettings();
            //signalRettings.ContractResolver = new SignalRContractResolver();
            //var serializer = JsonSerializer.Create(signalRettings);
            //GlobalHost.DependencyResolver.Register(typeof(JsonSerializer), () => serializer);
        }
    }
}