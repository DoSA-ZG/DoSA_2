using RPPP_WebApp.Models;
using System.Linq.Expressions;

namespace RPPP_WebApp.Extensions.Selectors
{
    public static class WorkerSort
    {
        public static IQueryable<Worker> ApplySort(this IQueryable<Worker> query, int sort, bool ascending)
        {
            Expression<Func<Worker, object>> orderSelector = sort switch
            {
                1 => d => d.IdHarvest,
                2 => d => d.IdPerson,
                3 => d => d.Time,
                4 => d => d.Salary,


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
