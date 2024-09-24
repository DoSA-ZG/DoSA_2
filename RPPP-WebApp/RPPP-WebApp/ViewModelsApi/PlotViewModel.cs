using RPPP_WebApp.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace RPPP_WebApp.ViewModelsApi
{
    public class PlotViewModel
    {
        [DisplayName("Plot ID"), Required(ErrorMessage = "Id Plot is required")]
        public int IdPlot { get; set; }

        [DisplayName("Plot Name"), Required(ErrorMessage = "Plot name is required")]
        public string PlotName { get; set; }

        [DisplayName("Plot Gps"), Required(ErrorMessage = "Plot Gps is required")]
        public string PlotGps { get; set; }

        public string Infrastructure { get; set; }

        public string Material { get; set; }

        public int? SunIntensity { get; set; }

        public int? IdLease { get; set; }
        [DisplayName("IdSoil"), Required(ErrorMessage = "A soil is required")]
        public int IdSoil { get; set; }
    }
}
