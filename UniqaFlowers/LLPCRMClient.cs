using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Tooling.Connector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace UniqaFlowers
{
    public class LLPCRMClient
    {
        private static IOrganizationService CreateOrganizationService(string connectionString)
        {

            System.Net.ServicePointManager.Expect100Continue = false;

            //ServicePointManager.ServerCertificateValidationCallback += ValidateServerCertificate;

            // Connect to the CRM web service using a connection string.
            CrmServiceClient conn = new CrmServiceClient(connectionString);

            // Cast the proxy client to the IOrganizationService interface.
            IOrganizationService service = (IOrganizationService)conn.OrganizationWebProxyClient != null ? (IOrganizationService)conn.OrganizationWebProxyClient : (IOrganizationService)conn.OrganizationServiceProxy;

            if (service == null)
            {
                throw new Exception(conn.LastCrmError);

                //throw new SoapException("CreateOrganizationService() failed: " + conn.LastCrmError, SoapException.ClientFaultCode);
            }

            return service;
        }

        public IList<InsurancePackage> GetProductsForChatBot()
        {
            IOrganizationService svc = CreateOrganizationService("Url=https://llpmluvii.api.crm4.dynamics.com; Username=demo@llpmluvii.onmicrosoft.com; Password=1234abcD; authtype=Office365");

            var request = new OrganizationRequest("llp_chatbotgetproducts")
            {
                ["country"] = "country",
                ["dateFrom"] = new DateTime(2017, 6, 1),
                ["dateTo"] = new DateTime(2017, 6, 10),
                ["numberOfPersons"] = 2,
                ["identification"] = new Guid().ToString()
                // EntityReference to the target of the action
                //["Target"] = Entity.ToEntityReference(),
                // Custom parameter
                // Another custom parameter
                //["MyParameterName"] = "Hello"
            };

            var response = svc.Execute(request);
            var result = response.Results.FirstOrDefault().Value as EntityCollection;
            var products = result.Entities.Select(ent => new InsurancePackage
            {
                Name = (string)ent.Attributes["name"],
                Price = (decimal)ent.Attributes["llp_totalprice"],
                UpsellPrice = (decimal)ent.Attributes["llp_totalpriceupsell"],
            }).ToList();
            return products;
        }
    }
}
