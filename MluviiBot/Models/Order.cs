﻿using System;
using System.Collections.Generic;
using MluviiBot.Services.Models;
using UniqaFlowers;

namespace MluviiBot.Models
{
    [Serializable]
    public class Order
    {
        public string OrderID { get; set; }
        public string ClientID { get; set; }
        public enum PartyTypes { Single, Party }
        public InsurancePackage InsurancePackage { get; set; }
        public DateTime DateFrom { get; set; }

        public DateTime DateTo { get; set; }

        public Person CustomerDetails { get; set; }
        public List<Person> Persons { get; set; }
        public int PersonCount { get; set; }
        public string Country { get; set; }

        public string BillingAddress { get; set; }
        public LicenceType LicenceType { get; set; }

        public bool Paid { get; set; }

        public PaymentDetails PaymentDetails { get; set; }

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