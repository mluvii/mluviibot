using System;
using System.Collections.Generic;

namespace MluviiBot.Models
{
    [Serializable]
    public class Order
    {
        public string ClientID { get; set; }

        public Person CustomerDetails { get; set; }

        public Order()
        {
            CustomerDetails = new Person();
        }
    }
}