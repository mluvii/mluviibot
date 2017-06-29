using System;

namespace MluviiBot.Contracts
{
    public class InsurancePackage
    {
        public string Name { get; set; }
        public string ImageUrl { get; set; }
        public Decimal Price { get; set; }
        public string UpsellMessage { get; set; }
        public bool IsUpsell { get; set; }
    }

}