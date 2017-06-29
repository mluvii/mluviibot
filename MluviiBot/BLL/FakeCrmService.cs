using System.Collections.Generic;
using MluviiBot.Contracts;

namespace MluviiBot.BLL
{
    public class FakeCrmService : ICRMService
    {
        public CRMPerson GetPerson(string email, string phoneNumber)
        {
            throw new System.NotImplementedException();
        }

        public void CreatePerson(CRMPerson person)
        {
            throw new System.NotImplementedException();
        }

        public IList<InsurancePackage> GetInsurancePackages(string email)
        {
            throw new System.NotImplementedException();
        }
    }
}