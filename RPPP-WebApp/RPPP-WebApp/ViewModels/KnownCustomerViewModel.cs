using RPPP_WebApp.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Collections;

namespace RPPP_WebApp.ViewModels
{
    public class KnownCustomerViewModel
    {
        public IEnumerable<KnownCustomer> KnownCustomers { get; set; }
        public PagingInfo PagingInfo { get; set; }
        [DisplayName("Person ID"), Required(ErrorMessage = "Id Person is required")]
        public int IdPerson { get; set; }

        [DisplayName("First Name "), Required(ErrorMessage = "First Name is required")]
        public string FirstName { get; set; }
        [DisplayName("Last Name "), Required(ErrorMessage = "Last Name is required")]
        public string LastName { get; set; }
        public string Number { get; set; }

        public string Mail { get; set; }
        public string Adresse { get; set; }
        public virtual IEnumerable<Lease> Leases { get; set; }

        public virtual IEnumerable<RequestViewModel> Requests { get; set; } 

        public virtual IEnumerable<SaleViewModel> Sales { get; set; }

        public KnownCustomerViewModel()
        {
            this.Sales = new List<SaleViewModel>();
            this.Requests = new List<RequestViewModel>();
            //I don't do lease because We done nothing with ,Milane did nothing
        }
    }
}
