using RPPP_WebApp.Models;
using System.Linq.Expressions;

namespace RPPP_WebApp.Extensions.Selectors
{
    public static class RequestSort
    {
        public static IQueryable<Request> ApplySort(this IQueryable<Request> query, int sort, bool ascending)
        {
            Expression<Func<Request, object>> orderSelector = sort switch
            {
                1 => d => d.IdRequest,
                2 => d => d.SpeciesAsked,
                3 => d => d.QuantityAsked,
                4 => d => d.PriceAsked,
                5 => d => d.IdPerson,
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
