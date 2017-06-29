using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using ContosoFlowers.Models;
using Microsoft.Bot.Builder.ConnectorEx;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.FormFlow;
using Microsoft.Bot.Connector;
using MluviiBot.BLL;

namespace ContosoFlowers.Dialogs
{
    public class InsuranceDialog: IDialog<Models.Order>
    {
        private const string RetryText = "Nerozuměl jsem, můžeš mi to napsat ještě jednou?";
        private const int MaxAttempts = 10;
        private ConversationReference conversationReference;
        private Models.Order order;
        private readonly ICRMService crmService;
        private readonly IMluviiBotDialogFactory dialogFactory;

        public InsuranceDialog(ICRMService crmService, IMluviiBotDialogFactory dialogFactory)
        {
            this.crmService = crmService;
            this.dialogFactory = dialogFactory;
        }

        public async Task StartAsync(IDialogContext context)
        {
            context.Wait(this.MessageReceivedAsync);
        }

        public virtual async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            var message = await result;

            if (this.conversationReference == null)
            {
                this.conversationReference = message.ToConversationReference();
            }
            this.order = new Order();
            PromptDialog.Text(context, this.OnCountrySelected, "Kam se chystáš vyrazit?");
        }

        private async Task OnCountrySelected(IDialogContext context, IAwaitable<string> result)
        {
            var message = await result;
            this.order.Country = message;
            await context.PostAsync(string.Format(CultureInfo.CurrentCulture, $"Dobre, {this.order.Country}"));
            PromptDialog.Text(context, this.OnDateFromSelected, "Kdy chceš odjíždět? Napiš mi prosím datum ve formátu DD.MM.RRRR", RetryText, MaxAttempts);
        }

        private async Task OnDateFromSelected(IDialogContext context, IAwaitable<string> result)
        {
            var message = await result;
            DateTime dateFrom;
            if (!DateTime.TryParseExact(message, "dd.MM.yyyy", CultureInfo.CurrentCulture, DateTimeStyles.AssumeLocal, out dateFrom))
            {
                PromptDialog.Text(context, this.OnDateFromSelected, "Promin nerozumel jsem. Napiš mi prosím datum ve formátu DD.MM.RRRR", RetryText, MaxAttempts);
                return;
            }


            order.DateFrom = dateFrom;
            PromptDialog.Text(context, this.OnDateToSelected, "Kdy se budeš vracet? Napiš mi prosím datum ve formátu DD.MM.RRRR", RetryText, MaxAttempts);
        }

        private async Task OnDateToSelected(IDialogContext context, IAwaitable<string> result)
        {
            var message = await result;
            DateTime dateTo;
            if (!DateTime.TryParseExact(message, "dd.MM.yyyy", CultureInfo.CurrentCulture, DateTimeStyles.AssumeLocal, out dateTo))
            {
                PromptDialog.Text(context, this.OnDateToSelected, "Promin nerozumel jsem. Napiš mi prosím datum ve formátu DD.MM.RRRR", RetryText, MaxAttempts);
                return;
            }

            order.DateFrom = dateTo;
            await context.PostAsync(string.Format(CultureInfo.CurrentCulture, $"Takze prijezd {this.order.DateFrom} a odjezd {this.order.DateTo}"));
            PromptDialog.Choice(context, this.OnPartySelected, new[] { "Sam", "S nekym" }, "Cestuješ sám nebo ještě s někým?", RetryText, MaxAttempts);
        }

        private async Task OnPartySelected(IDialogContext context, IAwaitable<string> result)
        {
            var message = await result;

            if (message.Equals("sam", StringComparison.InvariantCultureIgnoreCase))
            {
                AskInsurancePackage(context);
            }
            else
            {
                PromptDialog.Number(context, this.OnAdditionalPersons, "Mužes mi napsat jejich počet?", RetryText, MaxAttempts);
            }
        }

        private void AskInsurancePackage(IDialogContext context)
        {
            context.Done(order);
            //            context.Call(this.dialogFactory.Create<BouquetsDialog, string>(this.order.FlowerCategoryName), this.AfterBouquetSelected);
        }

        private void AskPersonalDetails(IDialogContext context)
        {
            var person = new Models.Person();
            var orderForm = new FormDialog<Models.Person>(person, Models.Person.BuildOrderForm, FormOptions.PromptInStart);
            context.Call(orderForm, this.AfterOrderForm);
        }

        private async Task OnAdditionalPersons(IDialogContext context, IAwaitable<long> result)
        {

            int count = (int)await result;
            order.PersonCount = count;

            await context.PostAsync(string.Format(CultureInfo.CurrentCulture, $"{count} dekuji, teď potřeboval znát jejich jména a datumy narození."));
            order.Persons = new List<Person>();
            context.Call(this.dialogFactory.Create<PersonDialog, int>(order.Persons.Count), this.AddPerson);
        }

        private async Task AddPerson(IDialogContext context, IAwaitable<Person> result)
        {
            var person = await result;
            order.Persons.Add(person);
            if (order.Persons.Count < order.PersonCount)
            {
                var personDialog = new PersonDialog(order.Persons.Count);
                context.Call(personDialog, this.AddPerson);
            }

            context.Done(order);
        }

        private async Task AfterOrderForm(IDialogContext context, IAwaitable<Person> result)
        {
            context.Done(order);
        }
    }
}