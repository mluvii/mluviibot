using System.Collections.Generic;
using MluviiBot.BotAssets.Dialogs;

namespace MluviiBot.Dialogs
{
    public interface IMluviiBotDialogFactory : IDialogFactory
    {
        SavedAddressDialog CreateSavedAddressDialog(
            string prompt,
            string useSavedAddressPrompt,
            string saveAddressPrompt,
            IDictionary<string, string> savedAddresses,
            IEnumerable<string> saveOptionNames);
    }
}