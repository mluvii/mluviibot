using System;
using System.Collections.Generic;
using MluviiBot.BLL;
using MluviiBot.Contracts;
using UniqaFlowers;

namespace ContosoFlowers.BLL
{
    public class CRMService : ICRMService
    {
        private readonly LLPCRMClient client = new LLPCRMClient();

        public void CreatePerson(CRMPerson person)
        {
            throw new NotImplementedException();
        }

        public IList<InsurancePackage> GetInsurancePackages(InsuranceDemand insuranceDemand)
        {
            return client.GetProductsForChatBot();
        }

        public CRMPerson GetPerson(string email, string phoneNumber)
        {
            throw new NotImplementedException();
        }
    }
}