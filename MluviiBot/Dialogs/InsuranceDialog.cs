using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using AutoMapper;
using ContosoFlowers.Models;
using ContosoFlowers.Properties;
using ContosoFlowers.Services;
using Microsoft.Bot.Builder.ConnectorEx;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.FormFlow;
using Microsoft.Bot.Builder.Location;
using Microsoft.Bot.Connector;
using MluviiBot.BLL;
using MluviiBot.Contracts;
using Newtonsoft.Json.Linq;
using UniqaFlowers;

namespace ContosoFlowers.Dialogs
{
    public class InsuranceDialog: IDialog<Models.Order>
    {
        private readonly string checkoutUriFormat;

        private const string RetryText = "Nerozuměl jsem, můžeš mi to napsat ještě jednou?";
        private const int MaxAttempts = 10;
        private ConversationReference conversationReference;
        private Models.Order order;
        private readonly ICRMService crmService;
        private readonly IMluviiBotDialogFactory dialogFactory;
        private readonly IOrdersService ordersService;


        public InsuranceDialog(string checkoutUriFormat, ICRMService crmService, IMluviiBotDialogFactory dialogFactory, IOrdersService ordersService)
        {
            this.crmService = crmService;
            this.dialogFactory = dialogFactory;
            this.ordersService = ordersService;
            this.checkoutUriFormat = checkoutUriFormat;
        }

        public async Task StartAsync(IDialogContext context)
        {
            if (!context.UserData.ContainsKey(Resources.ClientID_Key))
                context.UserData.SetValue(Resources.ClientID_Key, Guid.NewGuid().ToString());
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
            order.ClientID = context.UserData.GetValue<string>(Resources.ClientID_Key);
            PromptDialog.Text(context, this.OnCountrySelected, "Kam se chystáš vyrazit?");
        }

        private async Task OnCountrySelected(IDialogContext context, IAwaitable<string> result)
        {
            var message = await result;
            this.order.Country = message;
            //var data = JObject.Parse(@"{ ""Activity"": ""Forward"", ""OperatorGroupID"": ""183"" }");
            //var act = context.MakeMessage();
            //act.Text = "Prepojuju te na operatora!";
            //act.ChannelData = data;
            //await context.PostAsync(act);
            await context.PostAsync(string.Format(CultureInfo.CurrentCulture, $"Dobře, {this.order.Country}"));
            PromptDialog.Text(context, this.OnDateFromSelected, "Kdy chceš odjíždět? Napiš mi prosím datum ve formátu DD.MM.RRRR", RetryText, MaxAttempts);
        }

        private async Task OnDateFromSelected(IDialogContext context, IAwaitable<string> result)
        {
            var message = await result;
            DateTime? dateFrom = ParseDateTime(message);
            if (!dateFrom.HasValue)
            {
                PromptDialog.Text(context, this.OnDateFromSelected, "Promiň nerozuměl jsem. Napiš mi prosím datum ve formátu DD.MM.RRRR", RetryText, MaxAttempts);
                return;
            }
            if (dateFrom <= DateTime.Today)
            {
                PromptDialog.Text(context, this.OnDateFromSelected, "Cestovaní v čase nepojišťujeme. Napiš mi prosím datum ve formátu DD.MM.RRRR v budoucnosti", RetryText, MaxAttempts);
                return;
            }


            order.DateFrom = dateFrom.Value;
            PromptDialog.Text(context, this.OnDateToSelected, "Kdy se budeš vracet? Napiš mi prosím datum ve formátu DD.MM.RRRR", RetryText, MaxAttempts);
        }

       
        private async Task OnDateToSelected(IDialogContext context, IAwaitable<string> result)
        {
            var message = await result;
            DateTime? dateTo = ParseDateTime(message);
            if (!dateTo.HasValue)
            {
                PromptDialog.Text(context, this.OnDateToSelected, "Promiň nerozuměl jsem. Napiš mi prosím datum ve formátu DD.MM.RRRR", RetryText, MaxAttempts);
                return;
            }
            if (dateTo < order.DateFrom)
            {
                PromptDialog.Text(context, this.OnDateToSelected, "Cestovaní v čase nepojišťujeme. Napiš mi prosím datum návratu ve formátu DD.MM.RRRR po datu odjezdu", RetryText, MaxAttempts);
                return;
            }


            order.DateTo = dateTo.Value;
            await context.PostAsync(string.Format(CultureInfo.CurrentCulture, $"Takže příjezd {this.order.DateFrom:d} a odjezd {this.order.DateTo:d}"));
            PromptDialog.Choice(context, this.OnPartySelected, new[] { "Sám", "S někým" }, "Cestuješ sám nebo ještě s někým?", RetryText, MaxAttempts);
        }

        private async Task OnPartySelected(IDialogContext context, IAwaitable<string> result)
        {
            var message = await result;

            if (new[] { "sam" , "sám"}.Contains(message.Trim(), StringComparer.InvariantCultureIgnoreCase))
            {
                AskInsurancePackage(context);
            }
            else
            {
                PromptDialog.Number(context, this.OnAdditionalPersons, "Můžes mi napsat jejich počet?", RetryText, MaxAttempts);
            }
        }

        private void AskInsurancePackage(IDialogContext context)
        {
            context.Call(this.dialogFactory.Create<InsurancePackageDialog, Models.Order>(order), this.AfterInsurancePackageSelected);
        }

        private async Task OnAdditionalPersons(IDialogContext context, IAwaitable<long> result)
        {
            int count = (int)await result;
            order.PersonCount = count;

            await context.PostAsync(string.Format(CultureInfo.CurrentCulture, $"{count} děkuji, teď bych potřeboval znát jejich jména a datumy narození."));
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

        private async Task AfterInsurancePackageSelected(IDialogContext context, IAwaitable<InsurancePackage> result)
        {
            var package = await result;

            order.InsurancePackage = package;
            await context.PostAsync($"Super, takže {package.Name}");
            await context.PostAsync($"Takže celkem to bude {package.Price} Kč, abych mohl sjednat pojištění budu potřebovat pár dalších údajů o tobě");
            order.CustomerDetails = new Models.Person();
            PromptDialog.Text(context, this.OnNameGiven, "Můžeš mi napsat tvé jméno", RetryText, MaxAttempts);
        }

        private async Task OnNameGiven(IDialogContext context, IAwaitable<string> result)
        {
            var name = await result;
            order.CustomerDetails.FirstName = name;
            PromptDialog.Text(context, this.OnSurnameNameGiven, $"{name} mám to, napiš mi tvoje příjmení", RetryText, MaxAttempts);

        }

        private async Task OnSurnameNameGiven(IDialogContext context, IAwaitable<string> result)
        {
            var surname = await result;
            order.CustomerDetails.LastName = surname;
            await context.PostAsync(string.Format(CultureInfo.CurrentCulture, $"Díky, takže {order.CustomerDetails.FirstName} {order.CustomerDetails.LastName}"));
            PromptDialog.Text(context, this.OnBirthDateGiven, $"Teď bych potřeboval tvé datum narození, napiš mi ho prosím ve tvaru DD.MM.RRRR", RetryText, MaxAttempts);
        }

        private async Task OnBirthDateGiven(IDialogContext context, IAwaitable<string> result)
        {
            var message = await result;
            DateTime? birthDate = ParseDateTime(message);
            if (!birthDate.HasValue)
            {
                PromptDialog.Text(context, this.OnBirthDateGiven, "Promiň nerozuměl jsem. Napiš mi prosím datum ve formátu DD.MM.RRRR", RetryText, MaxAttempts);
                return;
            }
            if ((DateTime.Today - birthDate).Value.TotalDays > 365 * 100)
            {
                PromptDialog.Text(context, this.OnBirthDateGiven, "Zombie nepojišťujeme. Napiš mi prosím datum narození někoho živého", RetryText, MaxAttempts);
                return;
            }

            // BotBuilder's LocationDialog
            // Leverage DI to inject other parameters
                        var locationDialog = this.dialogFactory.Create<LocationDialog>(
                            new Dictionary<string, object>()
                            {
                                { "prompt", "" },
                                { "channelId", context.Activity.ChannelId },
                                { "options", LocationOptions.SkipFavorites | LocationOptions.SkipFinalConfirmation },
                            });

            context.Call(locationDialog, this.AfterLocation);
        }

        private async Task AfterLocation(IDialogContext context, IAwaitable<Place> result)
        {
            var place = await result;
            order.BillingAddress = place.Address.FormattedAddress;
            await context.PostAsync(string.Format(CultureInfo.CurrentCulture, $"Díky moc, teď už mám všechno, připravím rekapitulaci tvého pojištění, kde ho můžeš rovnou i zaplatit.Po zaplacení ti vše pošlu do emailu."));
            await PaymentSelectionAsync(context);
        }

        private async Task PaymentSelectionAsync(IDialogContext context)
        {
            var paymentReply = context.MakeMessage();

            var serviceModel = Mapper.Map<Services.Models.Order>(this.order);
            if (this.order.OrderID == null)
            {
                this.order.OrderID = this.ordersService.PlacePendingOrder(serviceModel);
            }

            var checkoutUrl = this.BuildCheckoutUrl(this.order.OrderID);
            paymentReply.Attachments = new List<Attachment>
            {
                new HeroCard()
                {
                    Text = string.Format(CultureInfo.CurrentCulture, Resources.RootDialog_Checkout_Prompt, this.order.InsurancePackage.Price.ToString("C")),
                    Buttons = new List<CardAction>
                    {
                        new CardAction(ActionTypes.OpenUrl, Resources.RootDialog_Checkout_Continue, value: checkoutUrl),
                        new CardAction(ActionTypes.ImBack, Resources.RootDialog_Checkout_Cancel, value: Resources.RootDialog_Checkout_Cancel)
                    }
                }.ToAttachment()
            };

            await context.PostAsync(paymentReply);

            context.Wait(this.AfterPaymentSelection);
        }

        private string BuildCheckoutUrl(string orderID)
        {
            var uriBuilder = new UriBuilder(this.checkoutUriFormat);

            var query = HttpUtility.ParseQueryString(uriBuilder.Query);
            query["orderID"] = orderID;
            query["botId"] = this.conversationReference.Bot.Id;
            query["channelId"] = this.conversationReference.ChannelId;
            query["conversationId"] = this.conversationReference.Conversation.Id;
            query["serviceUrl"] = this.conversationReference.ServiceUrl;
            query["userId"] = this.conversationReference.User.Id;

            uriBuilder.Query = query.ToString();
            var checkoutUrl = uriBuilder.Uri.ToString();

            return checkoutUrl;
        }

        private async Task AfterPaymentSelection(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            var selection = await result;

            if (selection.Text == Resources.RootDialog_Checkout_Cancel)
            {
                var options = new[] { Resources.RootDialog_Menu_StartOver, Resources.RootDialog_Menu_Cancel, Resources.RootDialog_Welcome_Support };
                PromptDialog.Choice(context, this.AfterChangedMyMind, options, Resources.RootDialog_Menu_Prompt);
            }
            else
            {
                var serviceOrder = this.ordersService.RetrieveOrder(selection.Text);
                if (serviceOrder == null || !serviceOrder.Payed)
                {
                    await context.PostAsync(string.Format(CultureInfo.CurrentCulture, Resources.RootDialog_Checkout_Error, selection.Text));
                    await this.PaymentSelectionAsync(context);
                    return;
                }

                var message = context.MakeMessage();
                message.Text = string.Format(
                    CultureInfo.CurrentCulture,
                    Resources.RootDialog_Receipt_Text,
                    selection.Text,
                    this.order.InsurancePackage.Name,
                    this.order.CustomerDetails.FirstName,
                    this.order.CustomerDetails.LastName,
                    "");
                message.Attachments.Add(this.GetReceiptCard());

                await context.PostAsync(string.Format(CultureInfo.CurrentCulture, $"Díky, takže {order.CustomerDetails.FirstName} {order.CustomerDetails.LastName}"));
            }
        }

        private async Task AfterChangedMyMind(IDialogContext context, IAwaitable<string> result)
        {
            try
            {
                var option = await result;

                if (option == Resources.RootDialog_Menu_StartOver)
                {
                    context.Done(order);
                }
                else if (option == Resources.RootDialog_Menu_Cancel)
                {
                    await this.PaymentSelectionAsync(context);
                }
                else
                {
                    await this.AfterPayment(context);
                }
            }
            catch (TooManyAttemptsException)
            {
                await this.AfterPayment(context);
            }
        }

        private Attachment GetReceiptCard()
        {
            var order = this.ordersService.RetrieveOrder(this.order.OrderID);
            var creditCardOffuscated = order.PaymentDetails.CreditCardNumber.Substring(0, 4) + "-****";
            var receiptCard = new ReceiptCard
            {
                Title = Resources.RootDialog_Receipt_Title,
                Facts = new List<Fact>
                {
                    new Fact(Resources.RootDialog_Receipt_OrderID, order.OrderID),
                    new Fact(Resources.RootDialog_Receipt_PaymentMethod, creditCardOffuscated)
                },
                Items = new List<ReceiptItem>
                {
                    new ReceiptItem(
                        title: order.FlowerCategoryName,
                        subtitle: order.Bouquet.Name,
                        price: order.Bouquet.Price.ToString("C"),
                        image: new CardImage(order.Bouquet.ImageUrl)),
                },
                Total = order.Bouquet.Price.ToString("C")
            };

            return receiptCard.ToAttachment();
        }

        private async Task AfterPayment(IDialogContext context)
        {
            await context.PostAsync($"Diky {order.CustomerDetails.FirstName}, měj se.");
            context.Done(this.order);
        }

        private DateTime? ParseDateTime(string text)
        {
            DateTime date1;
            if (DateTime.TryParseExact(text, "dd.MM.yyyy", CultureInfo.CurrentCulture, DateTimeStyles.AssumeLocal, out date1))
                return date1;
            DateTime date2;
            if (DateTime.TryParseExact(text, "d.M.yyyy", CultureInfo.CurrentCulture, DateTimeStyles.AssumeLocal, out date2))
                return date2;

            return null;
        }

    }
}