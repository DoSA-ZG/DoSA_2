using RPPP_WebApp.Models;

namespace RPPP_WebApp.ViewModels
{
    public class SoilViewModel
    {
        public IEnumerable<Soil> Soil { get; set; }
        public PagingInfo PagingInfo { get; set; }

        public int IdSoil { get; set; }

        public string SoilName { get; set; }

        public virtual ICollection<Plot> Plots { get; set; } = new List<Plot>();
    }
}
