using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Internals.Fibers;
using MluviiBot.BotAssets.Dialogs;
using MluviiBot.Models;
using MluviiBot.Properties;

#pragma warning disable 1998
namespace MluviiBot.Dialogs
{
    [Serializable]
    public class HelpDialog : IDialog
    {
        private readonly bool canGoBack;
        private readonly IMluviiBotDialogFactory dialogFactory;
        private Person person;

        public HelpDialog(IMluviiBotDialogFactory dialogFactory, bool canGoBack = true)
        {
            this.canGoBack = canGoBack;
            SetField.NotNull(out this.dialogFactory, nameof(dialogFactory), dialogFactory);
        }

        public async Task StartAsync(IDialogContext context)
        {
            if (context.UserData.ContainsKey(Resources.Person_Key))
                person = context.UserData.GetValue<Person>(Resources.Person_Key);

            var preferencesOptions = new[]
            {
                Resources.HelpDialog_start_over,
                Resources.HelpDialog_connect_operator,
                person != null ? Resources.HelpDialog_edit_details : "",
                canGoBack ? Resources.CancellableDialog_back : ""
            }.Except(new[] {""});

            CancelablePromptChoice<string>.Choice(
                context,
                ResumeAfterOptionSelected,
                preferencesOptions,
                Resources.HelpDialog_Prompt);
        }

        private async Task ResumeAfterOptionSelected(IDialogContext context, IAwaitable<string> result)
        {
            var option = await result;

            if (option == null)
            {
                context.Done<object>(null);
                return;
            }

            if (option.Equals(Resources.HelpDialog_start_over, StringComparison.OrdinalIgnoreCase))
            {
                context.Call(dialogFactory.Create<IDialog<object>>(), null);
                return;
            }

            if (option.Equals(Resources.HelpDialog_connect_operator, StringComparison.OrdinalIgnoreCase))
            {
                context.Call(dialogFactory.Create<HandoverDialog>(), null);
                return;
            }

            if (option.Equals(Resources.HelpDialog_edit_details, StringComparison.OrdinalIgnoreCase))
            {
                context.Call(dialogFactory.Create<EditDetailsDialog, Person>(person), OnPersonalDetailsCorrected);
                return;
            }

            await StartAsync(context);
        }

        private async Task OnPersonalDetailsCorrected(IDialogContext context, IAwaitable<Person> result)
        {
            context.UserData.SetValue(Resources.Person_Key, await result);
            await StartAsync(context);
        }
    }
}