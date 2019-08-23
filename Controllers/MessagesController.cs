using System;
using System.Threading.Tasks;
using System.Web.Http;
using System.Linq;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Builder.Dialogs;
using System.Web.Http.Description;
using System.Net.Http;
using Newtonsoft.Json;

namespace Microsoft.Bot.Sample.SimpleEchoBot
{
    [BotAuthentication]
    public class MessagesController : ApiController
    {
        /// <summary>
        /// POST: api/Messages
        /// receive a message from a user and send replies
        /// </summary>
        /// <param name="activity"></param>
        [ResponseType(typeof(void))]
        public virtual async Task<HttpResponseMessage> Post([FromBody] Activity activity)
        {
            // check if activity is of type message
            if (activity != null && new[] { ActivityTypes.Message, ActivityTypes.Event}.Contains(activity.GetActivityType()))
            {
                await Conversation.SendAsync(activity, () => new EchoDialog());
            }
            else
            {
                await HandleSystemMessage(activity);
            }
            return new HttpResponseMessage(System.Net.HttpStatusCode.Accepted);
        }

        private async Task<Activity> HandleSystemMessage(Activity message)
        {
            var logline = $"Received system message: Type: {message.Type}. Full json: {JsonConvert.SerializeObject(message)}";
            System.Diagnostics.Trace.WriteLine(logline);
            var client = new ConnectorClient(new Uri(message.ServiceUrl), new MicrosoftAppCredentials());
            var reply = message.CreateReply();
            reply.Text = logline;
            await client.Conversations.ReplyToActivityAsync(reply);
            
            if (message.Type == ActivityTypes.DeleteUserData)
            {
                // Implement user deletion here
                // If we handle user deletion, return a real message
            }
            else if (message.Type == ActivityTypes.ConversationUpdate)
            {
                // Handle conversation state changes, like members being added and removed
                // Use Activity.MembersAdded and Activity.MembersRemoved and Activity.Action for info
                // Not available in all channels
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
            
            return null;
        }
    }
}