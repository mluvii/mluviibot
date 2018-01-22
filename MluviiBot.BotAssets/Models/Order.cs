using System;
using System.Collections.Generic;

namespace MluviiBot.Models
{
    [Serializable]
    public class Order
    {
        public string ClientID { get; set; }

        public Person CustomerDetails { get; set; }

        public string BillingAddress { get; set; }
        public LicenceType LicenceType { get; set; }

        public Order()
        {
            CustomerDetails = new Person();
        }
    }

    public enum LicenceType
    {
        One,
        Multiple
    }
}