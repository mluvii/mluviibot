using System;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using MluviiBot.Models;

namespace MluviiBot.Dialogs
{
    public class DebugDialog: IDialog
    {
        private readonly IMluviiBotDialogFactory dialogFactory;

        public DebugDialog(IMluviiBotDialogFactory dialogFactory)
        {
            this.dialogFactory = dialogFactory;
        }

        public async Task StartAsync(IDialogContext context)
        {
            PromptDialog.Choice(context, async (dialogContext, result) =>
                {
                    var choice = await result;
                    Enum.TryParse<DebugOptions>(choice, out var selected);
                    var dialog = dialogFactory.Create<MluviiDialog, DebugOptions>(selected);
                    dialogContext.Call(dialog, null);
            },
            new [] {"GotoFinalConfirmation","GotoOperatorSearch", "GotoMap"},
            "DEBUG MENU");
        }
    }
}