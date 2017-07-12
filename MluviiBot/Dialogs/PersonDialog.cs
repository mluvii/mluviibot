using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ContosoFlowers.BotAssets;
using ContosoFlowers.Properties;
using Microsoft.Bot.Builder.Dialogs;

namespace ContosoFlowers.Dialogs
{
    public class PersonDialog : IDialog<Models.Person>
    {
        private readonly int PersonCounter;

        public PersonDialog(int personCounter)
        {
            PersonCounter = personCounter;
        }

        public async Task StartAsync(IDialogContext context)
        {
            PromptDialog.Text(context, this.AddPerson, $"{PersonCounter + 1}. osobu prosím. Format: Jméno, Příjmení, datum narození", Resources.RetryText, 3);
        }

        private async Task AddPerson(IDialogContext context, IAwaitable<string> result)
        {
            var personsString = await result;
            if (!Regex.IsMatch(personsString, RegexConstants.PersonDetail))
            {
                context.Fail(new Exception(Resources.RetryText));
                return;
            }

            var strings = personsString.Split(',').Select(x => x.Trim()).ToList();
            var person = new Models.Person();
            person.FirstName = strings[0];
            person.LastName = strings[1];
            person.DateOfBirth = DateTime.ParseExact(strings[2], "dd.MM.yyyy", null);

            context.Done(person);
        }
    }
}