using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using AutoMapper;
using Microsoft.Bot.Builder.ConnectorEx;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.FormFlow;
using Microsoft.Bot.Builder.Location;
using Microsoft.Bot.Connector;
using MluviiBot.BLL;
using MluviiBot.BotAssets;
using MluviiBot.BotAssets.Extensions;
using MluviiBot.Models;
using MluviiBot.Properties;
using Newtonsoft.Json.Linq;

namespace MluviiBot.Dialogs
{
    public class MluviiDialog: IDialog<Models.Order>
    {
        private const string RetryText = "Nerozuměl jsem, můžete mi to napsat ještě jednou?";
        private const int MaxAttempts = 10;
        private ConversationReference conversationReference;
        private Models.Order order;
        private readonly IMluviiBotDialogFactory dialogFactory;


        public MluviiDialog(IMluviiBotDialogFactory dialogFactory)
        {
            this.dialogFactory = dialogFactory;
        }

        public async Task StartAsync(IDialogContext context)
        {
            order = new Order();
            if (!context.UserData.ContainsKey(Resources.ClientID_Key))
                context.UserData.SetValue(Resources.ClientID_Key, Guid.NewGuid().ToString());
            
            if (context.UserData.ContainsKey(Resources.Person_Key))
            {
                order.CustomerDetails = context.UserData.GetValue<Person>(Resources.Person_Key);
            }
            order.ClientID = context.UserData.GetValue<string>(Resources.ClientID_Key);

            context.Wait(this.MessageReceivedAsync);
        }

        public virtual async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            var message = await result;
            if (message.Type != ActivityTypes.Message)
            {
                context.Call(this.dialogFactory.Create<MluviiDialog>(), null);
                return;
            }

            if (this.conversationReference == null)
            {
                this.conversationReference = message.ToConversationReference();
            }
            var lower = message.Text.ToLower();
            if (lower.Contains("produkt") || lower.Contains("1"))
            {
                await context.SayAsync("Děkuji, nyní bych od Vás potřeboval několik údajů:");
                OnProductInterestSelected(context);
                return;
            }
            if (lower.Contains("dotaz") || lower.Contains("2"))
            {
                await ConnectToOperator(context, "Dobře, přepojuji Vás na operátora. Hezký den.");
                return;
            }
            PromptDialog.Choice(context, 
                async (ctx, res) => await this.MessageReceivedAsync(ctx, new AwaitableFromItem<IMessageActivity>(new Activity() {Text = await res})), 
                new[] { "Zájem o produkt", "Dotaz" },
                "Omlouvám se, nerozuměl jsem. Vyberte prosím z možností", RetryText, MaxAttempts);
        }

        private void OnProductInterestSelected(IDialogContext context)
        {
            var form = new FormDialog<Person>(new Person(), Person.BuildForm, FormOptions.PromptInStart);
            context.Call(form, this.OnPersonalDetailsGiven);
        }

        private async Task OnPersonalDetailsGiven(IDialogContext context, IAwaitable<Person> result)
        {
            order.CustomerDetails = await result;
            context.UserData.SetValue(Resources.Person_Key, order.CustomerDetails);
            await context.SayAsync("Děkuji");
            
            var locationDialog = dialogFactory.Create<LocationDialog>(
                new Dictionary<string, object>()
                {
                    { "channelId", context.Activity.ChannelId }
                });

            context.Call(locationDialog, this.AfterLocation);
        }
        
        private async Task AfterLocation(IDialogContext context, IAwaitable<Place> result)
        {
            var place = await result;
            order.CustomerDetails.Address = $"{place?.Address.StreetAddress}, {place?.Address.Locality} {place?.Address.PostalCode}, {place?.Address.Country}";
            var reply = context.MakeMessage();

            var options = new[]
            {
                "1 licence",
                "Více licencí",
                "Poradit",
            };
            reply.AddHeroCard(
                "O jaký typ licence máte zájem, nebo potřebujete poradit?",
                "",
                options);

            await context.PostAsync(reply);
            context.Wait(OnLicenceTypeSelected);
        }

        private async Task OnLicenceTypeSelected(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            var lower = (await result).Text?.ToLower();
            
            if (lower.Contains("jedna") || lower.Contains("1"))
            {
                order.LicenceType = LicenceType.One;
                await AskVerification(context);
                return;
            }
            if (lower.Contains("více") || lower.Contains("vice") || lower.Contains("2"))
            {
                order.LicenceType = LicenceType.Multiple;
                await AskVerification(context);
                return;
            }
            if (lower.Contains("poradit") || lower.Contains("3"))
            {
                await ConnectToOperator(context, "Dobře, přepojuji Vás na operátora. Hezký den.");
                return;
            }
            
            PromptDialog.Text(context, async (ctx, res) => await this.OnLicenceTypeSelected(ctx, new AwaitableFromItem<IMessageActivity>(new Activity() {Text = await res})), "Omlouvám se, nerozuměl jsem. Vyberte prosím z možností 1) 1 Licence 2) Více licencí 3) Přepojit na operátora", RetryText, MaxAttempts);
        }

        private async Task AskVerification(IDialogContext context)
        {
            await context.SayAsync("Děkuji, prosím o kontrolu zadaných údajů:");
            await context.SayAsync($"Jméno, Příjmení: {order.CustomerDetails.FirstName}, {order.CustomerDetails.LastName}");
            await context.SayAsync($"Telefon: {order.CustomerDetails.Phone}");
            await context.SayAsync($"Email: {order.CustomerDetails.Email}");
            await context.SayAsync($"Adresa: {order.CustomerDetails.Address}");

            PromptDialog.Choice(context, this.OnRecapConfirmation, new[] { "Souhlasí", "Nesouhlasí" },"Je to správně?", RetryText, MaxAttempts);
        }

        private async Task OnRecapConfirmation(IDialogContext context, IAwaitable<string> result)
        {
            var lower = (await result).ToLower();
            
            if (lower.Contains("nesouhlas") || !(lower.Contains("souhlasí") || lower.Contains("souhlasi") || lower.Contains("ano") || lower.Contains("správně") || lower.Contains("spravne")))
            {
                context.Call(this.dialogFactory.Create<EditDetailsDialog, Person>(order.CustomerDetails), OnPersonalDetailsCorrected);
                return;
            }
            if (order.LicenceType == LicenceType.One)
            {
                await context.SayAsync("Gratulujeme! Objednávka je vygenerována. Následujíci pracovní den obdržíte další instrukce.");
                context.Done(order);
                return;
            }
            PromptDialog.Confirm(context, this.OnOrderFinishAnswered, "Přejete si požadavek dořesit nyní?", RetryText, MaxAttempts);
        }
        
        private async Task OnPersonalDetailsCorrected(IDialogContext context, IAwaitable<Person> result)
        {
            order.CustomerDetails = await result;
            context.UserData.SetValue(Resources.Person_Key, order.CustomerDetails);
            await this.AskVerification(context);
        }

        private async Task OnOrderFinishAnswered(IDialogContext context, IAwaitable<bool> result)
        {
            if (await result)
            {
                await ConnectToOperator(context, "Vyčkejte prosím na přepojení na operátora.");
                return;
            }

            await context.SayAsync("Na Váš telefon Vás budeme kontaktovat následující pracovní den.");
            context.Done(order);
        }

        private async Task ConnectToOperator(IDialogContext context, string message)
        {
            var data = JObject.Parse(@"{ ""Activity"": ""Forward"" }");
            var act = context.MakeMessage();
            act.ChannelData = data;
            act.Text = message;
            await context.PostAsync(act);
        }
    }
}