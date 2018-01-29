using System;
using System.Collections.Generic;
using System.Linq;

namespace MluviiBot.Models
{
    public class GetAvailableOperatorsResponse
    {
        public IList<AvailableOperatorInfo> AvailableOperators { get; set; }

        public GetAvailableOperatorsResponse()
        {
            AvailableOperators = new List<AvailableOperatorInfo>();
        }
        
        public override string ToString()
        {
           return String.Join(", ", AvailableOperators.Select(ao => ao.ToString()));
        }
    }
    
    public class AvailableOperatorInfo
    {
        public string DisplayName { get; set; }
        public int UserId { get; set; }

        public override string ToString()
        {
            return $"{nameof(DisplayName)}: {DisplayName}, {nameof(UserId)}: {UserId}";
        }
    }
}