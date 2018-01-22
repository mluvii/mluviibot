using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Autofac;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Internals;
using Microsoft.Bot.Connector;
using MluviiBot.BotAssets.Extensions;

namespace MluviiBot.Controllers
{
    [BotAuthentication]
    public class MessagesController : ApiController
    {
        /// <summary>
        /// POST: api/Messages
        /// Receive a message from a user and reply to it
        /// </summary>
        public async Task<HttpResponseMessage> Post([FromBody]Activity activity)
        {
            if (activity.Type == ActivityTypes.Message)
            {
                using (var scope = DialogModule.BeginLifetimeScope(Conversation.Container, activity))
                {
                    var dialog = scope.Resolve<IDialog<object>>();
                    await Conversation.SendAsync(activity, () => dialog);
                }
            }
            else
            {
                await this.HandleSystemMessage(activity);
            }

            var response = Request.CreateResponse(HttpStatusCode.OK);
            return response;
        }

        private async Task HandleSystemMessage(Activity message)
        {
            if (message.Type == ActivityTypes.DeleteUserData)
            {
                // Implement user deletion here
                // If we handle user deletion, return a real message
            }
            else if (message.Type == ActivityTypes.ConversationUpdate)
            {
                if (message.MembersAdded.Any(o => o.Id == message.Recipient.Id))
                {
                    var reply = message.CreateReply();
                    using (var scope = DialogModule.BeginLifetimeScope(Conversation.Container, message))
                    {
                        var dialog = scope.Resolve<IDialog<object>>();
                        await Conversation.SendAsync(message, () => dialog);
                    }
//                    var options = new[]
//                    {
//                        "Zájem o produkt",
//                        "Pouze dotaz",
//                    };
//                    reply.AddHeroCard(
//                        "Dobrý den, jak Vám mohu pomoci?",
//                        "",
//                        options,
//                        new[] { "https://media.licdn.com/mpr/mpr/shrink_200_200/AAEAAQAAAAAAAAy8AAAAJGVmNWQ3NjEwLWM3ZDQtNDg4Yy1hYjgxLTQ3NjMxYjUxMWI5ZA.png" });
//                    ConnectorClient connector = new ConnectorClient(new Uri(message.ServiceUrl));

//                    await connector.Conversations.ReplyToActivityAsync(reply);
                }
            }
            else if (message.Type == ActivityTypes.ContactRelationUpdate)
            {
                // Handle add/remove from contact lists
                // Activity.From + Activity.Action represent what happened
            }
            else if (message.Type == ActivityTypes.Typing)
            {
                // Handle knowing tha the user is typing
            }
            else if (message.Type == ActivityTypes.Ping)
            {
            }
            else if (message.Type == ActivityTypes.DeleteUserData)
            {
                
            }
        }
    }
}