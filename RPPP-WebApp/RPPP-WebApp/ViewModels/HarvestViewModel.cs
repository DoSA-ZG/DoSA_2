using RPPP_WebApp.Models;
using RPPP_WebApp.Util;
using System.ComponentModel.DataAnnotations;

namespace RPPP_WebApp.ViewModels
{
    public class HarvestViewModel
    {
        public IEnumerable<Harvest> Harvests { get; set; }
        public PagingInfo PagingInfo { get; set; }

       
        public int IdHarvest { get; set; }

        [ExcelFormat("dd.mm.yyyy")]
        //[DisplayFormat(DataFormatString = "{0:dd.MM.yyyy.}", ApplyFormatInEditMode = false)]
        [DataType(DataType.Date)]
        public DateTime StartHarvest { get; set; }
        [ExcelFormat("dd.mm.yyyy")]
       // [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy.}", ApplyFormatInEditMode = false)]
        [DataType(DataType.Date)]
        public DateTime EndHarvest { get; set; }
        public string UseHarvest { get; set; }

        public int QuantityHarvest { get; set; }

        public int CostHarvest { get; set; }
        public int IdPlant { get; set; }

        public int? IdSale { get; set; }

        public string NameHarvest { get; set; }
        public  IEnumerable<SaleViewModel> Sales { get; set; } 

        public  IEnumerable<WorkerViewModel> Workers { get; set; }

        public HarvestViewModel()
        {
            this.Sales = new List<SaleViewModel>();
            this.Workers = new List<WorkerViewModel>();
        }



    }
}