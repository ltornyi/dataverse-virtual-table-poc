using Microsoft.Xrm.Sdk;
using System;
using System.IO;
using System.Net;
using System.Text.Json;

namespace FacilityDataProvider
{
    public class Retrieve : IPlugin
    {
        string apihost = "\nhttps://XXXXXXXXXXXXXXX.azurewebsites.net";
        string apipath = "api/facility";
        string apikey = "XXXXXXXXXXXXXXXXX";

        public void Execute(IServiceProvider serviceProvider)
        {
            var context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            var tracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));

            Entity entity = null;

            tracingService.Trace("Starting to retrieve Facility data");

            try
            {
                var guid = context.PrimaryEntityId;
                tracingService.Trace("Guid: {0}", guid);

                var webRequest = WebRequest.Create($"{apihost}/{apipath}/{guid}?code={apikey}") as HttpWebRequest;

                if (webRequest != null)
                {
                    webRequest.ContentType = "application/json";
                    using (var s = webRequest.GetResponse().GetResponseStream())
                    {
                        using (var sr = new StreamReader(s))
                        {
                            var facilityAsJson = sr.ReadToEnd();
                            tracingService.Trace("Response: {0}", facilityAsJson);

                            Facility facility = null;
                            facility = JsonSerializer.Deserialize<Facility>(facilityAsJson);

                            if (facility != null)
                            {
                                entity = facility.ToEntity(tracingService);
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                tracingService.Trace("Exception with message: {0}", e.Message);
            }

            // Set output parameter
            context.OutputParameters["BusinessEntity"] = entity;
            tracingService.Trace("End of retrieve facility data");
        }
    }
}
