using RPPP_WebApp.Models;
using System.Linq.Expressions;

namespace RPPP_WebApp.Extensions.Selectors
{
    public static class KnownCustomerSort
    {
        public static IQueryable<KnownCustomer> ApplySort(this IQueryable<KnownCustomer> query, int sort, bool ascending)
        {
            Expression<Func<KnownCustomer, object>> orderSelector = sort switch
            {
                1 => d => d.IdPerson,
                2=>d =>d.IdPersonNavigation.FirstName,
                3 => d => d.IdPersonNavigation.LastName,

                

                _ => null
            };

            if (orderSelector != null)
            {
                query = ascending ?
                       query.OrderBy(orderSelector) :
                       query.OrderByDescending(orderSelector);
            }

            return query;
        }
    }
}
