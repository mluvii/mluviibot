using System.Text.RegularExpressions;

namespace MluviiBot.BLL
{
    public class ValidationUtils
    {
        public static bool Validate(string input, Regex pattern)
        {
            return pattern.IsMatch(input);
        }
    }
}