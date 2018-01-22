using System;
using System.Collections.Generic;

namespace MluviiBot.Models
{
    [Serializable]
    public class UserPreferences
    {
        public string SenderEmail { get; set; }

        public string SenderPhoneNumber { get; set; }

        public Dictionary<string, string> BillingAddresses { get; set; }
    }
}