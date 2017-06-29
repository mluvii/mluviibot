using System.Collections.Generic;
using MluviiBot.Contracts;
using UniqaFlowers;

namespace MluviiBot.BLL
{
    public interface ICRMService
    {
        CRMPerson GetPerson(string email, string phoneNumber);
        void CreatePerson(CRMPerson person);
        IList<InsurancePackage> GetInsurancePackages(InsuranceDemand insuranceDemand);
    }
}