using RPPP_WebApp.Models;
using System.Diagnostics.Metrics;
using System.Linq.Expressions;

namespace RPPP_WebApp.Extensions.Selectors;

public static class HarvestSort
{
    public static IQueryable<Harvest> ApplySort(this IQueryable<Harvest> query, int sort, bool ascending)
    {
        Expression<Func<Harvest, object>> orderSelector = sort switch
        {
            1 => d => d.IdHarvest,
            2 => d => d.NameHarvest,
            3 => d => d.Workers
                        .OrderBy(w => w.IdPersonNavigation.FirstName)
                        .ThenBy(w => w.IdPersonNavigation.LastName)
                        .FirstOrDefault(),

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
