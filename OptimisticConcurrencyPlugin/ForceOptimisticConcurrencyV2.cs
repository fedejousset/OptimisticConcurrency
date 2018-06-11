using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OptimisticConcurrencyPlugin
{
    /// <summary>
    /// Forces an optimistic concurrency control for update messages
    /// This approach uses a custom hidden field on the form and using the 
    /// undocumented formContext.data.entity.getRowVersion() function
    /// </summary>
    public class ForceOptimisticConcurrencyV2 : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            IOrganizationServiceFactory serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);

            if (context.MessageName != "Update") return;

            var request = new UpdateRequest() { Parameters = context.InputParameters };

            if (!string.IsNullOrEmpty(request.Target.GetAttributeValue<string>("fjo_rowversion")))
            {
                // The following lines don't work (hence we're forced to use lines 34~38 to manually validate the rowVersion) in v8.2/9.0
                // If used in PreOperation stage, CRM will ignore completely both properties an no validation will take place
                // If used in PreValidation stage, it looks like the ConcurrencyBehavior is set but the rowVersion not and CRM will 
                // return the following error: "The RowVersion property must be provided when the value of ConcurrencyBehavior is IfVersionMatches"
                //request.Target.RowVersion = request.Target.GetAttributeValue<string>("fjo_rowversion");
                //request.ConcurrencyBehavior = ConcurrencyBehavior.IfRowVersionMatches;

                var currentRecord = service.Retrieve(request.Target.LogicalName, request.Target.Id, new ColumnSet(false));
                if (currentRecord.RowVersion != request.Target.GetAttributeValue<string>("fjo_rowversion"))
                {
                    throw new InvalidPluginExecutionException("The record you're trying to update has changed, please refresh your screen to get the latest version.");
                }
            }
        }
    }
}
