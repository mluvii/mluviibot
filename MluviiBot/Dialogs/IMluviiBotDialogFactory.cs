namespace ContosoFlowers.Dialogs
{
    using System.Collections.Generic;
    using BotAssets;
    using BotAssets.Dialogs;

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