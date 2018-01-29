using System;
using Microsoft.Bot.Builder.Location;
using MluviiBot.BotAssets.Properties;

namespace MluviiBot.BotAssets
{
    [Serializable]
    public class MluviiLocationResourceManager : LocationResourceManager
    {
        public override string ConfirmationAsk => Resources.Location_ConfirmationAsk;
        public override string SelectLocation => "Teď budu potřebovat Vaši fakturační adresu.";
        public override string InvalidLocationResponse => "Hmm, to nemůže být správně. Nepřepsal jste se? Zkuste to prosím znovu.";
        public override string LocationNotFound => "Tuto adresu jsem nenašel :( Zkuste to prosím znovu";
        public override string MultipleResultsFound => "Našel jsem více míst, vyberte prosím to správné.";
        public override string SingleResultFound => "Je to tato adresa?";
        public override string DialogStartBranchAsk => "Napište svou adresu ve formátu \"Ulice číslo město\". Napřiklad: Sokolovská 1 Praha";
        public override string HelpCommand => "pomoc";
        public override string HelpMessage => "Není Vám pomoci";
        public override string Locality => "kraj";
        public override string PostalCode => "PSČ";
        public override string Region => "kraj";
        public override string ResetCommand => "znovu";
        public override string ResetPrompt => "Dobře začínáme znovu";
        public override string StreetAddress => "ulice";
        public override string TitleSuffix => "Napište svou adresu ve formátu \"Ulice číslo město\". Napřiklad: Sokolovská 1 Praha";
        public override string AskForTemplate => " Ještě mi chybí {0}.";
        public override string AskForPrefix => "Dobře {0}";
        public override string AskForEmptyAddressTemplate => "Ještě mi chybí {0}";
    }
}