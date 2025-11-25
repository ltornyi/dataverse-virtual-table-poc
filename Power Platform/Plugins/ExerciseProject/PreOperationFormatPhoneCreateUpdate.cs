using Microsoft.Xrm.Sdk;

using System.Text.RegularExpressions;

namespace ExerciseProject
{
    public class PreOperationFormatPhoneCreateUpdate : IPlugin
    {
        public PreOperationFormatPhoneCreateUpdate()
        {
        }

        public void Execute(IServiceProvider serviceProvider)
        {
            IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            
            if (!context.InputParameters.ContainsKey("Target"))
                throw new InvalidPluginExecutionException("No target found");

            var entity = context.InputParameters["Target"] as Entity;
            if (!entity.Attributes.Contains("telephone1"))
                return;

            string phoneNumber = (string)entity["telephone1"];
            var formattedNumber = Regex.Replace(phoneNumber, @"[^\d]", "");

            entity["telephone1"] = formattedNumber;
        }
    }
}