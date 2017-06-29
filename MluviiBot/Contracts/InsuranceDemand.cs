using System;

namespace MluviiBot.Contracts
{
    public class InsuranceDemand
    {
        public string ClientID { get; set; }
        public string Country { get; set; }
        public int PersonCount { get; set; }
        public DateTime DateFrom { get; set; }
        public DateTime DateTo { get; set; }
    }
}