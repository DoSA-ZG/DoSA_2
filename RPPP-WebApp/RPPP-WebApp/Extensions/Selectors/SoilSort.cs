using RPPP_WebApp.Models;
using System.Linq.Expressions;

namespace RPPP_WebApp.Extensions.Selectors
{
    public static class SoilSort
    {
        public static IQueryable<Soil> ApplySort(this IQueryable<Soil> query, int sort, bool ascending)
        {
            Expression<Func<Soil, object>> orderSelector = sort switch
            {
                1 => d => d.IdSoil,
                2 => d => d.SoilName,
                3=> d => d.Plots,
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
