using System.Text.RegularExpressions;

namespace MluviiBot.BotAssets
{
    public static class RegexConstants
    {
        public const string Email = @"[a-z0-9!#$%&'*+\/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+\/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?";

        public const string Phone = @"^\d{9}$";

        public static readonly Regex CONFUSED = new Regex(@"^.*(pomoc|nevim|kurva|debil|prdel|co dál|co teď|co můžu říct).*$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    }
}
