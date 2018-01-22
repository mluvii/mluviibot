using System.Collections.Generic;
using Autofac;
using MluviiBot.BotAssets.Dialogs;

namespace MluviiBot.Dialogs
{
    public class MluviiBotDialogFactory : DialogFactory, IMluviiBotDialogFactory
    {
        public MluviiBotDialogFactory(IComponentContext scope)
            : base(scope)
        {
        }

        public SavedAddressDialog CreateSavedAddressDialog(
            string prompt,
            string useSavedAddressPrompt,
            string saveAddressPrompt,
            IDictionary<string, string> savedAddresses,
            IEnumerable<string> saveOptionNames)
        {
            return this.Scope.Resolve<SavedAddressDialog>(
                new NamedParameter("prompt", prompt),
                new NamedParameter("useSavedAddressPrompt", useSavedAddressPrompt),
                new NamedParameter("saveAddressPrompt", saveAddressPrompt),
                TypedParameter.From(savedAddresses),
                TypedParameter.From(saveOptionNames));
        }
    }
}