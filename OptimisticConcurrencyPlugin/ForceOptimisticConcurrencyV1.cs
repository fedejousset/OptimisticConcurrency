using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OptimisticConcurrencyPlugin
{
    /// <summary>
    /// Forces an optimistic concurrency control for update messages
    /// This approach doesn't work because the RowVersion attribute in the Target object is always null (v8.2/9.0)
    /// </summary>
    [Obsolete("Not working in v8.2/v9.0 - RowVersion property in Target is always null")]
    public class ForceOptimisticConcurrencyV1 : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            IOrganizationServiceFactory serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);

            if (context.MessageName != "Update") return;

            context.InputParameters["ConcurrencyBehavior"] = ConcurrencyBehavior.IfRowVersionMatches;
        }
    }
}
