using System;
using ContosoFlowers.BotAssets;
using Microsoft.Bot.Builder.FormFlow;
using MluviiBot.BotAssets;

namespace MluviiBot.Models
{
    [Serializable]
    public class Person
    {
        [Prompt]
        public string FirstName { get; set; }

        [Prompt(FieldCase = CaseNormalization.None)]
        public string LastName { get; set; }

        [Prompt]
        [Pattern(RegexConstants.Phone)]
        public long Phone { get; set; }

        [Prompt]
        [Pattern(RegexConstants.Email)]
        public string Email { get; set; }

        [Prompt]
        [Pattern(RegexConstants.Date)]
        public string DateOfBirthString { get; set; }

        public DateTime DateOfBirth { get; set; }

        public bool AskDetails { get; set; }

        public static IForm<Person> BuildOrderForm()
        {
            return new FormBuilder<Person>()
                .Field(nameof(FirstName))
                .Field(nameof(LastName))
                .Field(nameof(Phone))
                .Field(nameof(Email))
                .Field(nameof(DateOfBirthString))
                .Field(nameof(Phone), state => state.AskDetails)
                .Field(nameof(Email), state => state.AskDetails)
                .Build();
        }
    }
}