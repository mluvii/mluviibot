namespace ContosoFlowers.BotAssets
{
    using System;
    using Microsoft.Bot.Builder.Location;
    using Properties;

    [Serializable]
    public class ContosoLocationResourceManager : LocationResourceManager
    {
        public override string ConfirmationAsk => Resources.Location_ConfirmationAsk;
        public override string SelectLocation => "Napiš svoji adresu";
        public override string InvalidLocationResponse => "Hmm, to nemůže být správně. Nepřepsal si se? Zkus to prosím znovu.";
        public override string LocationNotFound => "Tuto adresu jsem nenašel :( Zkus to prosím znovu";
        public override string MultipleResultsFound => "Našel jsem více míst, vyber prosím to správné.";
        public override string SingleResultFound => "Je to tahle adresa?";
        public override string DialogStartBranchAsk => "Napiš svoji adresu";
        public override string HelpCommand => "pomoc";
        public override string HelpMessage => "Není ti pomoci";
        public override string Locality => "Kraj";
        public override string PostalCode => "PSC";
        public override string Region => "Oblast";
        public override string ResetCommand => "znovu";
        public override string ResetPrompt => "Dobře začínáme znovu";
        public override string StreetAddress => "Adresa";
        public override string TitleSuffix => "Napiš svoji adresu";
    }
}
