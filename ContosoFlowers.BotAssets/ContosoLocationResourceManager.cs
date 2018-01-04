namespace ContosoFlowers.BotAssets
{
    using System;
    using Microsoft.Bot.Builder.Location;
    using Properties;

    [Serializable]
    public class ContosoLocationResourceManager : LocationResourceManager
    {
        public override string ConfirmationAsk => Resources.Location_ConfirmationAsk;
        public override string SelectLocation => "Teď budu potřebovat Vaši fakturační adresu.";
        public override string InvalidLocationResponse => "Hmm, to nemůže být správně. Nepřepsal jste se? Zkuste to prosím znovu.";
        public override string LocationNotFound => "Tuto adresu jsem nenašel :( Zkuste to prosím znovu";
        public override string MultipleResultsFound => "Našel jsem více míst, vyberte prosím to správné.";
        public override string SingleResultFound => "Je to tato adresa?";
        public override string DialogStartBranchAsk => "Napište svou adresu";
        public override string HelpCommand => "pomoc";
        public override string HelpMessage => "Není Vám pomoci";
        public override string Locality => "Kraj";
        public override string PostalCode => "PSČ";
        public override string Region => "Oblast";
        public override string ResetCommand => "znovu";
        public override string ResetPrompt => "Dobře začínáme znovu";
        public override string StreetAddress => "Adresa";
        public override string TitleSuffix => "Napište svou adresu";
    }
}
