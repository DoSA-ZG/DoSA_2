using RPPP_WebApp.Models;
using System.ComponentModel.DataAnnotations.Schema;

namespace RPPP_WebApp.ViewModels
{
    public class WorkerViewModel
    {
        public IEnumerable<Worker> Workers { get; set; }
        public PagingInfo PagingInfo { get; set; }

        public int IdPerson { get; set; }

        public int? Salary { get; set; }

        public int? Time { get; set; }

        public int? IdHarvest { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Number { get; set; }

        public string Mail { get; set; }
        public string Adresse { get; set; }



    }
}
