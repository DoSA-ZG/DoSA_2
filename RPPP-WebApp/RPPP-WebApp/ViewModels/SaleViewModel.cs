using RPPP_WebApp.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace RPPP_WebApp.ViewModels
{
    public class SaleViewModel
    {
        public IEnumerable<Sale> Sales { get; set; }
        public PagingInfo PagingInfo { get; set; }
        public int? IdSale { get; set; }

        public string PlantSeedling { get; set; }
        public int QuantitySale { get; set; }
        public int PriceSale { get; set; }
        public int IdHarvest { get; set; }
        public int? IdPerson { get; set; }
        public int? IdAnonymous { get; set; }

        public virtual ICollection<HarvestViewModel> Harvests { get; set; } = new List<HarvestViewModel>();

        public virtual Anonymou IdAnonymousNavigation { get; set; }

        public virtual Harvest IdHarvestNavigation { get; set; }

        public virtual KnownCustomer IdPersonNavigation { get; set; }
    }
}
