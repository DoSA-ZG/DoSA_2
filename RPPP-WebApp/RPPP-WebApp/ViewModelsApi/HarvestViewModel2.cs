using RPPP_WebApp.Models;
using RPPP_WebApp.Util;
using System.ComponentModel.DataAnnotations;

namespace RPPP_WebApp.ViewModelsApi
{
    public class HarvestViewModel2
    {
        public int IdHarvest { get; set; }

       // [ExcelFormat("dd.mm.yyyy")]
        //[DisplayFormat(DataFormatString = "{0:dd.MM.yyyy.}", ApplyFormatInEditMode = false)]
        //[DataType(DataType.Date)]
        public DateTime StartHarvest { get; set; }
       // [ExcelFormat("dd.mm.yyyy")]
        // [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy.}", ApplyFormatInEditMode = false)]
        //[DataType(DataType.Date)]
        public DateTime EndHarvest { get; set; }
        public string UseHarvest { get; set; }

        public int QuantityHarvest { get; set; }

        public int CostHarvest { get; set; }
        public int IdPlant { get; set; }

        public int? IdSale { get; set; }

        public string NameHarvest { get; set; }
    }
}
