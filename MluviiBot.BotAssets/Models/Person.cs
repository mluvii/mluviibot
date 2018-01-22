using System;
using Microsoft.Bot.Builder.FormFlow;
using Microsoft.Bot.Builder.FormFlow.Advanced;
using MluviiBot.BotAssets;

namespace MluviiBot.Models
{
    [Serializable]
    public class Person
    {
        [Prompt("Jaké je Vaše jméno?", "Napište mi prosím Vaše jméno")]
        [Template(TemplateUsage.NotUnderstood, "Zadejte prosím jméno")]
        public string FirstName { get; set; }

        [Prompt("Takže {FirstName} a příjmení?", FieldCase = CaseNormalization.None)]
        [Template(TemplateUsage.NotUnderstood, "Zadejte prosím příjmení")]
        public string LastName { get; set; }

        [Prompt("Zadejte prosím Vaše telefonní číslo (9 míst)", "Teď bych potřeboval Vaše telefonní číslo (9 míst)")]
        [Template(TemplateUsage.Help, "Telefonní číslo na 9 míst (bez mezinárodní předvolby)")]
        [Template(TemplateUsage.NotUnderstood, "Tento telefon nevypadá správně, pište prosím pouze 9 čísel.")]
        [Pattern(RegexConstants.Phone)]
        public string Phone { get; set; }

        [Prompt("Těď Vás poprosím o email.")]
        [Template(TemplateUsage.NotUnderstood, "Tento email nevypadá správně, zkuste to prosím znovu.")]
        [Pattern(RegexConstants.Email)]
        public string Email { get; set; }
        
        [Optional]
        public string Address { get; set; }

        public static IForm<Person> BuildForm()
        {
            return new FormBuilder<Person>()
                .Field(nameof(FirstName))
                .Field(nameof(LastName))
                .Message("Děkuji, takže {FirstName} {LastName}.")
                .Field(new FieldReflector<Person>(nameof(Address)).SetActive(_  => false))
                .AddRemainingFields()
                .Build();
        }
    }
}