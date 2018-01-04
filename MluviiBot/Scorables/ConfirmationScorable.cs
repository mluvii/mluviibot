using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs.Internals;
using Microsoft.Bot.Builder.Internals.Fibers;
using Microsoft.Bot.Builder.Scorables.Internals;
using Microsoft.Bot.Connector;
using Newtonsoft.Json.Linq;

namespace ContosoFlowers.Scorables
{
    public class ConfirmationScorable: ScorableBase<IActivity, string, double>
    {
        
        private readonly IDialogTask task;

        public ConfirmationScorable(IDialogTask task)
        {
            SetField.NotNull(out this.task, nameof(task), task);
        }

        protected override async Task<string> PrepareAsync(IActivity item, CancellationToken token)
        {
            if (item is IMessageActivity message && !string.IsNullOrWhiteSpace(message.Text))
            {
                if (new [] {"ano", "jo", "spravne", "správně", "je"}.Contains(message.Text.ToLower()))
                {
                    return "yes";
                }
                if (new [] {"ne", "nikoli", "spatne", "špatně", "neni", "není"}.Contains(message.Text.ToLower()))
                {
                    return "no";
                }
            }

            return null;
        }

        protected override bool HasScore(IActivity item, string state)
        {
            return state != null;
        }

        protected override double GetScore(IActivity item, string state)
        {
            return 1.0;
        }

        protected override async Task PostAsync(IActivity item, string state, CancellationToken token)
        {
            if (item is IMessageActivity activity)
            {
                var data = JObject.Parse(@"{ ""Activity"": ""Forward"" }");
                IMessageActivity message = Activity.CreateMessageActivity();
                message.ChannelId = activity.ChannelId;
                message.From = activity.From;
                message.Recipient = activity.Recipient;
                message.Conversation = activity.Conversation;
                message.Text = state;
                message.ChannelData = ActionTypes.MessageBack;
                message.Locale = activity.Locale;
                this.task.Post(message, () => { this.task.PollAsync(token); });
//                await this.task.PollAsync(token);
            }
        }

        protected override Task DoneAsync(IActivity item, string state, CancellationToken token)
        {
            return Task.CompletedTask;
        }
    }
}