using MluviiBot.BLL;

namespace ContosoFlowers.Dialogs
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Threading.Tasks;
    using System.Web;
    using AutoMapper;
    using BotAssets.Dialogs;
    using BotAssets.Extensions;
    using Microsoft.Bot.Builder.ConnectorEx;
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Builder.FormFlow;
    using Microsoft.Bot.Builder.Location;
    using Microsoft.Bot.Connector;
    using Models;
    using Properties;
    using Services;
    using Services.Models;

    [Serializable]
    public class RootDialog : IDialog<object>
    {
        private readonly string checkoutUriFormat;
        private readonly IMluviiBotDialogFactory dialogFactory;
        private readonly IOrdersService ordersService;

        private Models.Order order;
        private ConversationReference conversationReference;
        
        public RootDialog(string checkoutUriFormat, IMluviiBotDialogFactory dialogFactory, IOrdersService ordersService)
        {
            this.checkoutUriFormat = checkoutUriFormat;
            this.dialogFactory = dialogFactory;
            this.ordersService = ordersService;
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

            await this.WelcomeMessageAsync(context);
        }

        private async Task WelcomeMessageAsync(IDialogContext context)
        {
            if (!context.UserData.ContainsKey(Resources.ClientID_Key))
                context.UserData.SetValue(Resources.ClientID_Key, Guid.NewGuid().ToString());

            var reply = context.MakeMessage();

            var options = new[]
            {
                "Sjednat cestovní pojištění",
                "Nahlásit pojistnou událost",
            };
            reply.AddHeroCard(
                "Ahoj jak ti mohu pomoci?",
                "",
                options,
                new[] { "https://media.licdn.com/mpr/mpr/shrink_200_200/AAEAAQAAAAAAAAy8AAAAJGVmNWQ3NjEwLWM3ZDQtNDg4Yy1hYjgxLTQ3NjMxYjUxMWI5ZA.png" });

            await context.PostAsync(reply);
            

            context.Call(this.dialogFactory.Create<InsuranceDialog>(), this.AfterOrderCompleted);
        }

        private async Task AfterOrderCompleted(IDialogContext context, IAwaitable<Models.Order> result)
        {
            order = await result;
            await context.SayAsync("Nevim co dal protoze sem mrzak. Ahoj.");
            context.Done(order);
        }

        private async Task StartOverAsync(IDialogContext context, string text)
        {
            var message = context.MakeMessage();
            message.Text = text;
            await this.StartOverAsync(context, message);
        }

        private async Task StartOverAsync(IDialogContext context, IMessageActivity message)
        {
            await context.PostAsync(message);
            this.order = new Models.Order();
            await this.WelcomeMessageAsync(context);
        }
    }
}