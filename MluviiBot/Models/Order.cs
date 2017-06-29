using System;
using System.Collections.Generic;
using ContosoFlowers.BotAssets;
using ContosoFlowers.Services.Models;
using Microsoft.Bot.Builder.FormFlow;
using Microsoft.Bot.Connector;

namespace ContosoFlowers.Models
{
    [Serializable]
    public class Order
    {
        public enum PartyTypes { Single, Party }

        public DateTime DateFrom { get; set; }

        public DateTime DateTo { get; set; }

        public List<Person> Persons { get; set; }
        public int PersonCount { get; set; }
        public string Country { get; set; }

        public string BillingAddress { get; set; }

        public bool Paid { get; set; }

        public PaymentDetails PaymentDetails { get; set; }

    }
}