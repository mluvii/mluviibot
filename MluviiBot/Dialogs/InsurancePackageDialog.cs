using MluviiBot.BLL;
using MluviiBot.Contracts;

namespace ContosoFlowers.Dialogs
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Threading.Tasks;
    using BotAssets.Dialogs;
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Connector;
    using Properties;
    using Services;
    using Services.Models;
    using UniqaFlowers;

    [Serializable]
    public class InsurancePackageDialog : PagedCarouselDialog<InsurancePackage>
    {
        private readonly Models.Order order;
        private readonly ICRMService crmService;
        private IList<InsurancePackage> packages;

        public InsurancePackageDialog(Models.Order order, ICRMService crmService)
        {
            this.crmService = crmService;
            this.order = order;
        }

        public override string Prompt => "Jaký pojištění by jsi chtěl?";

        public override PagedCarouselCards GetCarouselCards(int pageNumber, int pageSize)
        {
            var insuranceDemand = new InsuranceDemand()
            {
                ClientID = order.ClientID,
                Country = order.Country,
                DateFrom = order.DateFrom,
                DateTo = order.DateTo,
                PersonCount = order.PersonCount
            };
            packages = crmService.GetInsurancePackages(insuranceDemand);

            var carouselCards = packages.Select(it => new HeroCard
            {
                Title = it.Name,
                Subtitle = it.Price.ToString("C"),
                Images = new List<CardImage> { new CardImage(it.ImageUrl, it.Name) },
                Buttons = new List<CardAction> { new CardAction(ActionTypes.ImBack, "CHCI!", value: it.Name) }
            });

            return new PagedCarouselCards
            {
                Cards = carouselCards,
                TotalCount = packages.Count
            };
        }

        public override async Task ProcessMessageReceived(IDialogContext context, string bouquetName)
        {
            var selectedPackage = packages.FirstOrDefault(x => x.Name.Equals(bouquetName, StringComparison.InvariantCultureIgnoreCase));

            if (selectedPackage != null)
            {
                context.Done(selectedPackage);
            }
            else
            {
                await context.PostAsync(string.Format(CultureInfo.CurrentCulture, "Vybrals BLBĚ!", bouquetName));
                await this.ShowProducts(context);
                context.Wait(this.MessageReceivedAsync);
            }
        }
    }
}