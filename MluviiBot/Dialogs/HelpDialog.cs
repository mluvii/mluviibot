using System;
using System.Linq;
using System.Threading.Tasks;
using MluviiBot.BotAssets.Dialogs;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Internals.Fibers;
using MluviiBot.Models;
using MluviiBot.Properties;

namespace MluviiBot.Dialogs
{
    [Serializable]
    public class HelpDialog : IDialog
    {
        private Person person;
        private readonly IMluviiBotDialogFactory dialogFactory;

        private string selectedAddressToUpdate;

        public HelpDialog(IMluviiBotDialogFactory dialogFactory)
        {
            SetField.NotNull(out this.dialogFactory, nameof(dialogFactory), dialogFactory);
        }

        public async Task StartAsync(IDialogContext context)
        {
            if (context.UserData.ContainsKey(Resources.Person_Key))
            {
                person = context.UserData.GetValue<Person>(Resources.Person_Key);
            }
           
            var preferencesOptions = new []{
                    Resources.HelpDialog_start_over,
                    Resources.HelpDialog_connect_operator,
                    person != null ? Resources.HelpDialog_edit_details : "",
                Resources.CancellableDialog_back,
                }.Except(new [] {""});
            
            PromptDialog.Choice(
                context,
                this.ResumeAfterOptionSelected,
                preferencesOptions,
                Resources.HelpDialog_Prompt);
        }

        private async Task ResumeAfterOptionSelected(IDialogContext context, IAwaitable<string> result)
        {
            var option = await result;

            if (option.Equals(Resources.HelpDialog_start_over, StringComparison.OrdinalIgnoreCase))
            {
                context.Call(this.dialogFactory.Create<IDialog<object>>(), null);
                return;
            }
            if (option.Equals(Resources.HelpDialog_connect_operator, StringComparison.OrdinalIgnoreCase))
            {
                context.Call(this.dialogFactory.Create<HandoverDialog>(), null);
                return;
            }
            if (option.Equals(Resources.HelpDialog_edit_details, StringComparison.OrdinalIgnoreCase))
            {
                context.Call(this.dialogFactory.Create<EditDetailsDialog, Person>(person), OnPersonDetailsEdited);
                return;
            }
            if (option.Equals(Resources.CancellableDialog_back, StringComparison.OrdinalIgnoreCase))
            {
                context.Done<object>(null);
                return;
            }
            await StartAsync(context);
        }

        private async Task OnPersonDetailsEdited(IDialogContext context, IAwaitable<Person> result)
        {
            context.UserData.SetValue(Resources.Person_Key, await result);
            await this.StartAsync(context);
        }
    }
}