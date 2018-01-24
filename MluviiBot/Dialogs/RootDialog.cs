
using System;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.ConnectorEx;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using MluviiBot.BotAssets.Extensions;
using MluviiBot.Properties;

namespace MluviiBot.Dialogs
{
    [Serializable]
    public class RootDialog : IDialog<object>
    {
        private readonly IMluviiBotDialogFactory dialogFactory;

        private Models.Order order;
        private ConversationReference conversationReference;
        
        public RootDialog(IMluviiBotDialogFactory dialogFactory)
        {
            this.dialogFactory = dialogFactory;
        }

        public async Task StartAsync(IDialogContext context)
        {
            await this.WelcomeMessageAsync(context);
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
                "Zájem o produkt",
                "Pouze dotaz",
            };
            reply.AddHeroCard(
                "Dobrý den, jak Vám mohu pomoci?",
                "",
                options,
                new[] { "https://media.licdn.com/mpr/mpr/shrink_200_200/AAEAAQAAAAAAAAy8AAAAJGVmNWQ3NjEwLWM3ZDQtNDg4Yy1hYjgxLTQ3NjMxYjUxMWI5ZA.png" });

            await context.PostAsync(reply);

            context.Call(this.dialogFactory.Create<MluviiDialog>(), this.AfterOrderCompleted);
        }

        private async Task AfterOrderCompleted(IDialogContext context, IAwaitable<Models.Order> result)
        {
            order = await result;
            context.EndConversation("");
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