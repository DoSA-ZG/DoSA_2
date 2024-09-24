using RPPP_WebApp.Models;
using System.Linq.Expressions;

namespace RPPP_WebApp.Extensions.Selectors
{
    public static class PlotsSort
    {
        public static IQueryable<Plot> ApplySort(this IQueryable<Plot> query, int sort, bool ascending)
        {
            Expression<Func<Plot, object>> orderSelector = sort switch
            {
                1 => d => d.IdPlot,
                2 => d => d.PlotName,
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