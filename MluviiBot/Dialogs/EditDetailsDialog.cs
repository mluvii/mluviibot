using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.FormFlow;
using Microsoft.Bot.Builder.Location;
using Microsoft.Bot.Connector;
using MluviiBot.BotAssets.Dialogs;
using MluviiBot.Models;
using MluviiBot.Properties;

namespace MluviiBot.Dialogs
{
    public class EditDetailsDialog: IDialog<Person>
    {
        private Person person;
        private readonly IMluviiBotDialogFactory dialogFactory;


        public EditDetailsDialog(IMluviiBotDialogFactory dialogFactory, Person person = null)
        {
            this.dialogFactory = dialogFactory;
            this.person = person ?? new Person();
        }

        public async Task StartAsync(IDialogContext context)
        {
            var preferencesOptions = new []{
                Resources.EditDetailsDialog_option_name,
                Resources.EditDetailsDialog_option_email,
                Resources.EditDetailsDialog_option_phone,
                Resources.EditDetailsDialog_option_address,
                Resources.CancellableDialog_back,
            }.Except(new [] {""});
            
            CancelablePromptChoice<string>.Choice(
                context,
                this.ResumeAfterOptionSelected,
                preferencesOptions,
                Resources.EditDetailsDialogDialog_prompt);
        }

        private async Task ResumeAfterOptionSelected(IDialogContext context, IAwaitable<string> result)
        {
            try
            {
                var option = await result;

                if (option == null)
                {
                    context.Done<object>(null);
                    return;
                }

                if (option.Equals(Resources.EditDetailsDialog_option_address, StringComparison.OrdinalIgnoreCase))
                {
                    var locationDialog = dialogFactory.Create<LocationDialog>(
                        new Dictionary<string, object>()
                        {
                            { "channelId", context.Activity.ChannelId }
                        });

                    context.Call(locationDialog, this.OnAddressChanged);
                    return;
                }

                if (option.Equals(Resources.EditDetailsDialog_option_name, StringComparison.OrdinalIgnoreCase))
                {
                    person.FirstName = null;
                    person.LastName= null;
                }

                if (option.Equals(Resources.EditDetailsDialog_option_email, StringComparison.OrdinalIgnoreCase))
                {
                    person.Email = null;
                }
                if (option.Equals(Resources.EditDetailsDialog_option_phone, StringComparison.OrdinalIgnoreCase))
                {
                    person.Phone = null;
                }
                var form = new FormDialog<Person>(person, Person.BuildForm, FormOptions.PromptInStart);
                context.Call(form, OnPersonDetailsChanged);
            }
            catch (TooManyAttemptsException)
            {
                await this.StartAsync(context);
            }
        }

        private async Task OnAddressChanged(IDialogContext context, IAwaitable<Place> result)
        {
            var place = await result;
            var address = $"{place?.Address.StreetAddress}, {place?.Address.Locality} {place?.Address.PostalCode}, {place?.Address.Country}";;
            await context.SayAsync($"Adresa byla změněna na {address}");
            person.Address = address;
            AskForMore(context);
        }

        private async Task OnPersonDetailsChanged(IDialogContext context, IAwaitable<Person> result)
        {
            person = await result;
            await context.SayAsync("Údaje byly změněny");
            AskForMore(context);
        }

        private void AskForMore(IDialogContext context)
        {
            PromptDialog.Confirm(context, async (subContext, subResult) =>
            {
                if (await subResult)
                {
                    await StartAsync(subContext);
                }
                else
                {
                    subContext.Done(person);
                }
            }, new PromptOptions<string>("Přejete si změnit ještě další údaje?"));
        }
    }
}