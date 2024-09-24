using static System.Runtime.InteropServices.JavaScript.JSType;

namespace RPPP_WebApp.ViewModels
{
    public class HarvestFilter
    {
        public int? IdHarvest { get; set; }


        public DateTime? StartHarvest { get; set; }

        public DateTime? EndHarvest { get; set; }

        public int? IdPlant { get; set; }

        public int? QuantityHarvest { get; set; }

        public bool IsEmpty()
        {
            bool active = IdHarvest.HasValue
                          || StartHarvest.HasValue
                          || EndHarvest.HasValue
                          || IdPlant.HasValue
                          || QuantityHarvest.HasValue;
            return !active;
        }
        public override string ToString()
        {
            return string.Format("{0}-{1}-{2}-{3}-{4}",
                IdHarvest,
                StartHarvest?.ToString("dd.MM.yyyy"),
                EndHarvest?.ToString("dd.MM.yyyy"),
                IdHarvest,
                QuantityHarvest);
        }


    }
}
