using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using RPPP_WebApp.Util;

namespace RPPP_WebApp.ModelsPartial
{
    [NotMapped]
    public class PlantsDenorm
    {
        //Plot attribute
        public int IdPlot { get; set; }


        public string PlotName { get; set; }

        public string PlotGps { get; set; }

        public string Infrastructure { get; set; }

        public string Material { get; set; }

        public int? SunIntensity { get; set; }

        public int? IdLease { get; set; }
        public int IdSoil { get; set; }
        public int IdPlant { get; set; }


        //Plant attribute
        public string Species { get; set; }

        public string SpeciesGroup { get; set; }

        public string FruitVegetable { get; set; }

        public string Origin { get; set; }

        public int Quantity { get; set; }

        
        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public string Product { get; set; }

        public DateTime ProductDate { get; set; }


        [NotMapped] public string PlotUrl { get; set; }
    }
}
