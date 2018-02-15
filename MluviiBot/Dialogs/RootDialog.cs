
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

        private async Task WelcomeMessageAsync(IDialogContext context)
        {
            if (!context.UserData.ContainsKey(Resources.ClientID_Key))
                context.UserData.SetValue(Resources.ClientID_Key, Guid.NewGuid().ToString());

            context.Call(this.dialogFactory.Create<MluviiDialog>(), this.AfterOrderCompleted);
        }

        private async Task AfterOrderCompleted(IDialogContext context, IAwaitable<Models.Order> result)
        {
            context.EndConversation("0");
        }
    }
}