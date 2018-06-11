using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Tooling.Connector;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OptimisticConcurrencyConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            var service = GetCrmConnection();

            Entity contact = RetrieveContact(service);
            UpdateContact(service, contact);
            UpdateContactWithConcurrencyControl(service, contact);
        }

        private static Entity RetrieveContact(IOrganizationService service)
        {
            var query = new QueryExpression("contact");
            query.ColumnSet = new ColumnSet("contactid", "fullname", "emailaddress1");
            query.Criteria.AddCondition("fullname", ConditionOperator.Equal, "JR Riquelme");

            var contacts = service.RetrieveMultiple(query);

            if (contacts.Entities.Count == 1)
            {
                return contacts.Entities.First();
            }
            else
            {
                throw new Exception("Contact not found");
            }
        }

        private static void UpdateContact(IOrganizationService service, Entity contact)
        {
            if (contact.GetAttributeValue<string>("emailaddress1") == "jrr@gmail.com")
            {
                contact["emailaddress1"] = "jrr@yahoo.com";
            }
            else
            {
                contact["emailaddress1"] = "jrr@gmail.com";
            }

            service.Update(contact);
        }

        private static void UpdateContactWithConcurrencyControl(IOrganizationService service, Entity contact)
        {
            if (contact.GetAttributeValue<string>("emailaddress1") == "test@gmail.com")
            {
                contact["emailaddress1"] = "jrr@yahoo.com";
            }
            else
            {
                contact["emailaddress1"] = "jrr@gmail.com";
            }

            var updateRequest = new UpdateRequest();
            updateRequest.Target = contact;
            updateRequest.Target.RowVersion = contact.RowVersion;
            updateRequest.ConcurrencyBehavior = ConcurrencyBehavior.IfRowVersionMatches;

            service.Execute(updateRequest);
        }

        private static IOrganizationService GetCrmConnection()
        {
            var conn = new CrmServiceClient(ConfigurationManager.ConnectionStrings["CRM"].ConnectionString);

            return (IOrganizationService)conn.OrganizationWebProxyClient != null ?
                (IOrganizationService)conn.OrganizationWebProxyClient :
                (IOrganizationService)conn.OrganizationServiceProxy;
        }
    }
}
