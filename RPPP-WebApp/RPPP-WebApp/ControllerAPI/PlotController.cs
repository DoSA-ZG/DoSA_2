using RPPP_WebApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using RPPP_WebApp.ViewModelsApi;
using RPPP_WebApp.ModelsAPI;

namespace RPPP_WebApp.ControllerAPI


{

    [ApiController]
    [Route("[controller]")]
    public class PlotController : ControllerBase
    {

        private readonly RPPP13Context ctx;

        private static Dictionary<string, Expression<Func<Plot, object>>> orderSelectors = new()
        {
            [nameof(PlotViewModel.IdPlot).ToLower()] = c => c.IdPlot,
            [nameof(PlotViewModel.PlotName).ToLower()] = c => c.PlotName,
            [nameof(PlotViewModel.PlotGps).ToLower()] = c => c.PlotGps,
            [nameof(PlotViewModel.Infrastructure).ToLower()] = c => c.Infrastructure,
            [nameof(PlotViewModel.Material).ToLower()] = c => c.Material,
            [nameof(PlotViewModel.SunIntensity).ToLower()] = c => c.SunIntensity,
            [nameof(PlotViewModel.IdSoil).ToLower()] = c => c.IdSoil,
            [nameof(PlotViewModel.IdLease).ToLower()] = c => c.IdLease,
           
        };

        public PlotController(RPPP13Context ctx) {
            this.ctx = ctx;
        }

        private static Expression<Func<Plot, PlotViewModel>> projection = c => new PlotViewModel
        {
            IdPlot = c.IdPlot,
            PlotName = c.PlotName,
            PlotGps = c.PlotGps,
            Infrastructure = c.Infrastructure,
            Material = c.Material,
            SunIntensity = c.SunIntensity,
            IdSoil = c.IdSoil,
            IdLease = c.IdLease
            
            

        };
        /// <summary>
        /// Get all Plots
        /// </summary>
        /// <param name="loadParams"></param>
        /// <returns></returns>
        [HttpGet(Name ="GetPlots")]
        public async Task<List<PlotViewModel>> GetAll([FromQuery] LoadParams loadParams)
        {
            var query = ctx.Plots.Include(s=>s.Plants)
                .AsQueryable();
            if(!string.IsNullOrWhiteSpace(loadParams.Filter))
            {
                query = query.Where(m => m.PlotName.Contains(loadParams.Filter));
            }

            if (loadParams.SortColumn != null)
            {
                if (orderSelectors.TryGetValue(loadParams.SortColumn.ToLower(), out var expr))
                {
                    query = loadParams.Descending ? query.OrderByDescending(expr) : query.OrderBy(expr);
                }
            }
            var list = await query.Select(projection)
                                  .Skip(loadParams.StartIndex)
                                  .Take(loadParams.Rows)
                                    .ToListAsync();

            return list;
        }
        /// <summary>
        /// Get plot by his id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id}",Name="GetPlotById")]
        public async Task<ActionResult<PlotViewModel>> Get(int id)
        {
            var plot = await ctx.Plots.Where(m => m.IdPlot == id)
                                       .Select(projection)
                                       .FirstOrDefaultAsync();
            
            if(plot == null)
            {
                return Problem(statusCode: StatusCodes.Status404NotFound, detail: $"No datat for id = {id}");
            }else
            {
                return plot; 
            }
        }
        /// <summary>
        /// Add a plot in the database
        /// </summary>
        /// <param name="model">Plot id from route must match with id from the model
        /// If your plot don't have any lease remember to put it on null</param>
        /// <returns></returns>
        [HttpPost(Name ="AddPlot")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create(PlotViewModel model,int id)
        {
            Plot plot = new()
            {
               IdPlot = id,
                PlotName = model.PlotName,
                PlotGps = model.PlotGps,
                Infrastructure = model.Infrastructure,
                Material = model.Material,
                SunIntensity = model.SunIntensity,
                IdLease = model.IdLease,
                IdSoil = model.IdSoil
            };
            ctx.Add(plot);
            await ctx.SaveChangesAsync();
            var AddedPlot = await Get(plot.IdPlot);
            return CreatedAtAction(nameof(Get), new { id = plot.IdPlot }, AddedPlot.Value);
        }

        /// <summary>
        /// Update Plot
        /// </summary>
        /// <param name="id"></param>
        /// <param name="model">Plot id from route must match with id from the model</param>
        /// <returns></returns>
        [HttpPut("{id}", Name = "UpdatePlot")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Update(int id, PlotViewModel model)
        {
            if (model.IdPlot != id) //ModelState.IsValid i model != null are automatically evalued because of [ApiController]
            {
                return Problem(statusCode: StatusCodes.Status400BadRequest, detail: $"Different ids {id} vs {model.IdPlot}");
            }
            else
            {
                var plot = await ctx.Plots.FindAsync(id);
                if (plot == null)
                {
                    return Problem(statusCode: StatusCodes.Status404NotFound, detail: $"Invalid id = {id}");
                }
                plot.PlotName = model.PlotName;
                plot.PlotGps = model.PlotGps;
                plot.Infrastructure = model.Infrastructure;
                plot.Material = model.Material;
                plot.SunIntensity = model.SunIntensity;
                plot.IdLease = model.IdLease;
                plot.IdSoil = model.IdSoil;

                await ctx.SaveChangesAsync();
                return NoContent();
            }
        }

        /// <summary>
        /// Delete a Plot thanks to his id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        /// <reponse code ="204">if Plot is deleted</reponse>
        /// <reponse code ="404">if Plot doesn't</reponse>
        [HttpDelete("{id}", Name = "DeletePlot")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int id)
        {
            var plot = await ctx.Plots.FindAsync(id);
            if (plot == null)
            {
                return NotFound();
            }
            else
            {
                ctx.Remove(plot);
                await ctx.SaveChangesAsync();
                return NoContent();
            };
        }
    }
}
