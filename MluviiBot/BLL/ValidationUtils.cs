using System.Text.RegularExpressions;

namespace ContosoFlowers.BLL
{
    public class ValidationUtils
    {
        public static bool Validate(string input, Regex pattern)
        {
            return pattern.IsMatch(input);
        }
    }
}