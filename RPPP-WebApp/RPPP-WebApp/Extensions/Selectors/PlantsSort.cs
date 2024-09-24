using RPPP_WebApp.Models;
using System.Linq.Expressions;

namespace RPPP_WebApp.Extensions.Selectors
{
    public static class PlantsSort
    {
        public static IQueryable<Plant> ApplySort(this IQueryable<Plant> query, int sort, bool ascending)
        {
            string defaultValue = "No Harvest";

            Expression<Func<Plant, object>> orderSelector = sort switch
            {
                1 => d => d.IdPlant,
                2 => d => d.Species,
                3 => d => d.Quantity,
                4 => d => d.Origin,
                5 => d => d.Harvests.OrderBy(h => h.UseHarvest).Select(h => h.UseHarvest).FirstOrDefault(),

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