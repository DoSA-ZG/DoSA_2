using RPPP_WebApp.Models;

namespace RPPP_WebApp.ViewModels
{
    public class RequestViewModel
    {
        public IEnumerable<Request> Requests { get; set; }
        public PagingInfo PagingInfo { get; set; }
        public int IdRequest { get; set; }

        public string SpeciesAsked { get; set; }
        public DateTime DateRequest { get; set; }
        public int QuantityAsked { get; set; }
        public int PriceAsked { get; set; }
        public int? IdPerson { get; set; }
        public string StatusRequest { get; set; }
        public virtual KnownCustomer IdPersonNavigation { get; set; }
        public virtual ICollection<Person> People { get; set; } = new List<Person>();
        public virtual IEnumerable<KnownCustomerViewModel> KnownCustomers { get; set; }

        public RequestViewModel()
        {
            this.KnownCustomers = new List<KnownCustomerViewModel>();
        }
    }
}
