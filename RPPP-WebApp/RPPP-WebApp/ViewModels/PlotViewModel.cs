using RPPP_WebApp.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace RPPP_WebApp.ViewModels
{
    public class PlotViewModel
    {
        public IEnumerable<Plot> Plots { get; set; }
        public PagingInfo PagingInfo { get; set; }
    
        public int IdPlot { get; set; }


        public string PlotName { get; set; }
        public string PlotGps { get; set; }

        public string Infrastructure { get; set; }

        public string Material { get; set; }

        public int? SunIntensity { get; set; }

        public int? IdLease { get; set; }
        public int IdSoil { get; set; }

        public virtual IEnumerable<PlantsViewModel> Plants { get; set; }
        public PlotViewModel()
        {
            this.Plants = new List<PlantsViewModel>();
        }

    }
}
