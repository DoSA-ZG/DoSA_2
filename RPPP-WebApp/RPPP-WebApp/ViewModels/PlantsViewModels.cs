using RPPP_WebApp.Models;

namespace RPPP_WebApp.ViewModels
{
    public class PlantsViewModel
    {
        public IEnumerable<Plant> Plants { get; set; }

        public int IdPlant { get; set; }

        public string Species { get; set; }

        public string SpeciesGroup { get; set; }

        public string FruitVegetable { get; set; }

        public string Origin { get; set; }

        public int Quantity { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public string Product { get; set; }

        public DateTime ProductDate { get; set; }

        public int IdPlot { get; set; }
        public virtual ICollection<HarvestViewModel> Harvests { get; set; } 

        public PlantsViewModel()
        {
            this.Harvests = new List<HarvestViewModel>();
        }
        public PagingInfo PagingInfo { get; set; }
    }
}