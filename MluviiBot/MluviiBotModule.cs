using System.Configuration;
using Autofac;
using MluviiBot.Services.Models;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Internals.Fibers;
using Microsoft.Bot.Builder.Location;
using Microsoft.Bot.Builder.Scorables;
using Microsoft.Bot.Connector;
using MluviiBot.BotAssets;
using MluviiBot.BotAssets.Dialogs;
using MluviiBot.Dialogs;

namespace MluviiBot
{
    public class MluviiBotModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            base.Load(builder);

            builder.RegisterType<MluviiBotDialogFactory>()
                .Keyed<IMluviiBotDialogFactory>(FiberModule.Key_DoNotSerialize)
                .AsImplementedInterfaces()
                .InstancePerLifetimeScope();

            builder.RegisterType<RootDialog>()
                .As<IDialog<object>>()
                .InstancePerDependency();
            
            builder.RegisterType<MluviiDialog>()
                .InstancePerDependency();


            builder.RegisterType<EditDetailsDialog>()
             .InstancePerDependency();

            builder.RegisterType<HandoverDialog>()
             .InstancePerDependency();

            builder.RegisterType<HelpDialog>()
                .InstancePerDependency();

            builder.RegisterType<HelpScorable>()
                .As<IScorable<IActivity, double>>()
                .InstancePerLifetimeScope();
            
            // Location Dialog
            // ctor signature: LocationDialog(string apiKey, string channelId, string prompt, LocationOptions options = LocationOptions.None, LocationRequiredFields requiredFields = LocationRequiredFields.None, LocationResourceManager resourceManager = null);
            builder.RegisterType<LocationDialog>()
                .WithParameter("apiKey", ConfigurationManager.AppSettings["MicrosoftBingMapsKey"])
                .WithParameter("options", LocationOptions.UseNativeControl | LocationOptions.ReverseGeocode | LocationOptions.SkipFavorites)
                .WithParameter("requiredFields", LocationRequiredFields.StreetAddress | LocationRequiredFields.Locality | LocationRequiredFields.Country)
                .WithParameter("resourceManager", new MluviiLocationResourceManager())
                .WithParameter("prompt", "")
                .InstancePerDependency();
        }
    }
}