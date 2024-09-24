using RPPP_WebApp.Models;
using System.Linq.Expressions;

namespace RPPP_WebApp.Extensions.Selectors
{
    public static class SaleSort
    {
        public static IQueryable<Sale> ApplySort(this IQueryable<Sale> query, int sort, bool ascending)
        {
            Expression<Func<Sale, object>> orderSelector = sort switch
            {
                1 => d => d.IdSale,
                2 => d => d.PlantSeedling,
                3 => d => d.QuantitySale,
                4 => d => d.PriceSale,
                5 => d => d.IdHarvest,
                6 => d => d.IdPerson,
                7 => d => d.IdAnonymous,
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
