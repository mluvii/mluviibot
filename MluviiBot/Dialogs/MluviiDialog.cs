using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
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
        private const int MaxAttempts = 5;
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
            try
            {
                await result;
            }
            catch (TooManyAttemptsException e)
            {
                context.Call(this.dialogFactory.Create<HelpDialog, bool>(false), null);
            }

            var message = await result;
            
            //Some bug in PromptDialog.Choice causes message.Type to be null
            if (message.Text == null) //message.Type != ActivityTypes.Message)
            {
                await StartAsync(context);
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
                await ConnectToOperator(context, Resources.OperatorConnect_wait);
                return;
            }
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
            await AskVerification(context);
        }

        private async Task AskVerification(IDialogContext context)
        {
            await context.SayAsync("Děkuji, prosím o kontrolu zadaných údajů:");
            await context.SayAsync($"Jméno, Příjmení: {order.CustomerDetails.FirstName}, {order.CustomerDetails.LastName}");
            await context.SayAsync($"Telefon: {order.CustomerDetails.Phone}");
            await context.SayAsync($"Email: {order.CustomerDetails.Email}");
            await context.SayAsync($"Adresa: {order.CustomerDetails.Address}");

            PromptDialog.Choice(context, this.OnRecapConfirmation, new[] { "Souhlasí", "Nesouhlasí" },"Je to správně?", Resources.RetryText, MaxAttempts);
        }

        private async Task OnRecapConfirmation(IDialogContext context, IAwaitable<string> result)
        {
            try
            {
                await result;
            }
            catch (TooManyAttemptsException e)
            {
                context.Call(this.dialogFactory.Create<HelpDialog, bool>(false), null);
                return;
            }
            var lower = (await result).ToLower();
            
            if (lower.Contains("nesouhlas") || !(lower.Contains("souhlasí") || lower.Contains("souhlasi")|| lower.Contains("jo") || lower.Contains("ano") || lower.Contains("správně") || lower.Contains("spravne")))
            {
                context.Call(this.dialogFactory.Create<EditDetailsDialog, Person>(order.CustomerDetails), OnPersonalDetailsCorrected);
                return;
            }
            await context.SayAsync($"Dobře, momentík podívám se kdo z kolegů je volný.");
            context.Call(this.dialogFactory.Create<AvailibleOperatorsDialog>(), OnAvailibleOperatorsResponse);
        }

        private async Task OnAvailibleOperatorsResponse(IDialogContext context, IAwaitable<AvailableOperatorInfo> result)
        {
            var selectedOperator = await result;

            if (selectedOperator == null)
            {
                PromptDialog.Choice(context, this.OnOfflineContactSelected, new[] {"Telefonicky", "Emailem", Resources.cancel_order}, 
                    Resources.OperatorSelection_none_availible_prompt, Resources.RetryText, MaxAttempts);
                return;
            }

            await ConnectToOperator(context, Resources.OperatorConnect_wait, selectedOperator.UserId);
        }

        private async Task OnOfflineContactSelected(IDialogContext context, IAwaitable<string> result)
        {
            try
            {
                await result;
            }
            catch (TooManyAttemptsException e)
            {
                context.Call(this.dialogFactory.Create<HelpDialog, bool>(false), null);
            }

            var response = await result;

            if (response.ToLower().Contains("mail") || response.ToLower().Contains("mejl"))
            {
                
            }
            if (response.ToLower().Contains("telefon") || response.ToLower().Contains("mobil") || response.ToLower().Contains("volat") || response.ToLower().Contains("volej")) 
            {
                
            }

            if (response.ToLower().Contains("zrušit") || response.ToLower().Contains("zrusit")  )
            {
                await context.SayAsync($"Je nám líto, že jste se rozhodl objednávku zrušit. Snad Vás brzy znovu uvidíme na našich stránkách :)");
            }
            
        }


        private async Task OnPersonalDetailsCorrected(IDialogContext context, IAwaitable<Person> result)
        {
            order.CustomerDetails = await result;
            context.UserData.SetValue(Resources.Person_Key, order.CustomerDetails);
            await this.AskVerification(context);
        }


        private async Task ConnectToOperator(IDialogContext context, string message, int? userID = null)
        {
            var data = JObject.Parse(@"{ ""Activity"": ""Forward"" }");
            if (userID != null)
            {
                data.Add("UserID", userID.Value);
            }
            
            var act = context.MakeMessage();
            act.ChannelData = data;
            act.Text = message;
            await context.PostAsync(act);
        }
    }
}