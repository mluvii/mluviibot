using System.Collections.Generic;
using MluviiBot.Contracts;
using UniqaFlowers;

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

        public IList<InsurancePackage> GetInsurancePackages(InsuranceDemand insuranceDemand)
        {
            return new List<InsurancePackage>()
            {
                new InsurancePackage() {ImageUrl = "https://appdev.mluvii.com/Bots/gold.png", Name = "Bronze", Price = 999.99m, IsUpsell = false},
                new InsurancePackage() {ImageUrl = "https://appdev.mluvii.com/Bots/silver.png", Name = "Silver", Price = 9999.99m, IsUpsell = false},
                new InsurancePackage() {ImageUrl = "https://appdev.mluvii.com/Bots/bronze.png", Name = "Gold", Price = 99999.99m, IsUpsell = false},
            };
        }
    }
}