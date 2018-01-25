using System;
using System.Linq;
using System.Threading.Tasks;
using iCord.OnifWebLib.Linq;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using MluviiBot.Models;
using MluviiBot.Properties;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MluviiBot.Dialogs
{
    public class AvailibleOperatorsDialog : IDialog<AvailableOperatorInfo>
    {
        private readonly IMluviiBotDialogFactory dialogFactory;
        
        private int maxAttempts = 3;
        private GetAvailableOperatorsResponse availibleOperators;

        public AvailibleOperatorsDialog(IMluviiBotDialogFactory dialogFactory)
        {
            this.dialogFactory = dialogFactory;
        }

        public async Task StartAsync(IDialogContext context)
        {
            await AskServerForAvailableOperators(context);
            context.Wait(OnMessageRecieved);
        }


        private async Task OnMessageRecieved(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            var activity = await result;
            if (activity.AsEventActivity() != null && activity.ChannelData != null)
            {
                try
                {
                    var availibleOperatorsInfo = JsonConvert.DeserializeObject<GetAvailableOperatorsResponse>(activity.ChannelData);
                    await OnAvailibleOperatorsResponse(context, availibleOperatorsInfo);
                    return;
                }
                catch (Exception)
                {
                    context.Done<GetAvailableOperatorsResponse>(null);
                    return;
                }
            }

            if (maxAttempts > 0)
            {
                await context.SayAsync("Stále ještě hledám volné kolegy.");
                maxAttempts--;
                await AskServerForAvailableOperators(context);
                context.Wait(OnMessageRecieved);
                return;
            }
            
            await context.SayAsync(Resources.OperatorSelection_none_availible);
            context.Done<AvailableOperatorInfo>(null);
        }
        
        private async Task OnAvailibleOperatorsResponse(IDialogContext context, GetAvailableOperatorsResponse result)
        {
            availibleOperators = result;
            availibleOperators.AvailableOperators = availibleOperators.AvailableOperators.DistinctBy(ope => ope.DisplayName).ToList();
            if (availibleOperators.AvailableOperators.Count > 1)
            {
                var operatorNames = availibleOperators.AvailableOperators.Select(ope => ope.DisplayName).ToList();
                operatorNames.Sort();
                await context.SayAsync($"K dispozici jsou: {string.Join(", ", operatorNames.Take(operatorNames.Count - 1))} a {operatorNames.Last()}");
                operatorNames.Add(Resources.OperatorSelection_not_interesed);
                PromptDialog.Choice(context, this.OnOperatorSelected, operatorNames, $"S kým byste chtěl mluvit?", Resources.RetryText, 5);
                return;
                
            }
            if (availibleOperators.AvailableOperators.Count == 1)
            {
                var operatorName = availibleOperators.AvailableOperators.Single().DisplayName;
                PromptDialog.Choice(context, (dialogContext, subResult) => OnSingleOperatorConfirmed(dialogContext, subResult, operatorName),
                    new[] {"Mluvit", Resources.OperatorSelection_not_interesed},$"K dispozici je jen {operatorName}.", Resources.RetryText, 2 );
                return;
            }
            if (availibleOperators.AvailableOperators.Count > 0)
            {
                
            }
            await context.SayAsync(Resources.OperatorSelection_none_availible);
            context.Done<GetAvailableOperatorsResponse>(null);
        }

        private async Task OnSingleOperatorConfirmed(IDialogContext context, IAwaitable<string> result, string operatorName)
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

            var choice = await result;
            if (choice.ToLower() == "mluvit")
            {
                await OnOperatorSelected(context, new AwaitableFromItem<string>(operatorName));
            }
            else
            {
                await OnOperatorSelected(context, new AwaitableFromItem<string>(Resources.OperatorSelection_not_interesed));
            }
        }

        private async Task OnOperatorSelected(IDialogContext context, IAwaitable<string> result)
        {
            string selectedOpe = null;
            try
            {
                selectedOpe = await result;

            }
            catch (TooManyAttemptsException e)
            {
                context.Call(this.dialogFactory.Create<HelpDialog, bool>(false), null);
                return;
            }

            if (selectedOpe.Equals(Resources.OperatorSelection_not_interesed))
            {
                context.Done<GetAvailableOperatorsResponse>(null);
                return;
            }

            var selected = availibleOperators.AvailableOperators.SingleOrDefault(ope => ope.DisplayName.Equals(selectedOpe));

            context.Done(selected);
        }
        
        private async Task AskServerForAvailableOperators(IDialogContext context)
        {
            var a = context.Activity as Activity;
            var data = JObject.Parse(@"{ ""Activity"": ""GetAvailableOperators"" }");
            var act = context.MakeMessage();
            act.ChannelData = data;
            await context.PostAsync(act);
        }
    }
}