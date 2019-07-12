using System;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Builder.Dialogs;
using System.Net.Http;

namespace Microsoft.Bot.Sample.SimpleEchoBot
{
    [Serializable]
    public class EchoDialog : IDialog<object>
    {
        protected int count = 1;

        public async Task StartAsync(IDialogContext context)
        {
            context.Wait(MessageReceivedAsync);
        }

        public async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> argument)
        {
            var message = await argument;

            if (message.Text == "reset")
            {
                PromptDialog.Confirm(
                    context,
                    AfterResetAsync,
                    "Are you sure you want to reset the count?",
                    "Didn't get that!",
                    promptStyle: PromptStyle.Auto);
            }
            else if (message.Text == "debug")
            {
                ShowDebugMenu(context);
            }
            else if(message.ChannelData != null) 
            {
                await context.PostAsync($"Received channel data: {JsonConvert.SerializeObject(message.ChannelData)}");
                context.Wait(MessageReceivedAsync);
            }
            else
            {
                await context.PostAsync($"{this.count++}: You said {message.Text}");
                context.Wait(MessageReceivedAsync);
            }
        }
        
        private void ShowDebugMenu(IDialogContext context) {
            context.Call(new PromptDialog.PromptChoice<string>(
                new [] {"Send custom activity", "Mega echo"},
                "Make you choice",
                "Try again",
                3), OnDebugOptionSelected);
        }

        public async Task AfterResetAsync(IDialogContext context, IAwaitable<bool> argument)
        {
            var confirm = await argument;
            if (confirm)
            {
                this.count = 1;
                await context.PostAsync("Reset count.");
            }
            else
            {
                await context.PostAsync("Did not reset count.");
            }
            context.Wait(MessageReceivedAsync);
        }
        
        private async Task OnDebugOptionSelected(IDialogContext context, IAwaitable<string> result)
        {
            var response = await result;
            if (response == "Send custom activity")
            {
                context.Call(new PromptDialog.PromptString("Which mluvii activity should I send? Send activity name or full json", "thats wrong, try again", 3), onActivitySelected );
                return;
            }
            if(response == "Mega echo")
            {
                context.Call(new PromptDialog.PromptString("Speak", "thats wrong, try again", 3), onMegaEcho );
                return;
            }
        }
        
        private async Task onActivitySelected(IDialogContext context, IAwaitable<string> result)
        {
            var activityName = await result;
            await SendMluviiActivity(context, activityName);
            context.Wait(MessageReceivedAsync);
        }
        
        private async Task SendMluviiActivity(IDialogContext context, string activity)
        {
            JObject data = null;
            try
            {
                data = JObject.Parse(activity);
            }
            catch(Exception) {}
            
            if(data == null || data["Activity"] == null) {
                data = JObject.Parse($"{{ \"Activity\": \"{activity}\" }}");
            }
            var act = context.MakeMessage();
            act.ChannelData = data;
            await context.PostAsync($"Sending: {activity}...");
            await context.PostAsync(act);
        }
        
        private async Task onMegaEcho(IDialogContext context, IAwaitable<string> result)
        {
            var activityJson = JsonConvert.SerializeObject(context.Activity);
            await context.SayAsync(activityJson);
            context.Wait(MessageReceivedAsync);
        }

    }
}