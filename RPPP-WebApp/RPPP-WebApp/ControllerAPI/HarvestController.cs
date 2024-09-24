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
    public class HarvestController : ControllerBase
    {

        private readonly RPPP13Context ctx;

        private static Dictionary<string, Expression<Func<Harvest, object>>> orderSelectors = new()
        {
            [nameof(HarvestViewModel2.IdHarvest).ToLower()] = c => c.IdHarvest,
            [nameof(HarvestViewModel2.IdPlant).ToLower()] = c => c.IdPlant,
            [nameof(HarvestViewModel2.IdSale).ToLower()] = c => c.IdSale,
            [nameof(HarvestViewModel2.CostHarvest).ToLower()] = c => c.CostHarvest,
            [nameof(HarvestViewModel2.QuantityHarvest).ToLower()] = c => c.QuantityHarvest,
            [nameof(HarvestViewModel2.StartHarvest).ToLower()] = c => c.StartHarvest,
            [nameof(HarvestViewModel2.EndHarvest).ToLower()] = c => c.EndHarvest,
            [nameof(HarvestViewModel2.NameHarvest).ToLower()] = c => c.NameHarvest,
            [nameof(HarvestViewModel2.UseHarvest).ToLower()] = c => c.UseHarvest,


        };

        public HarvestController(RPPP13Context ctx)
        {
            this.ctx = ctx;
        }

        private static Expression<Func<Harvest, HarvestViewModel2>> projection = c => new HarvestViewModel2
        {
            
            IdHarvest = c.IdHarvest,
            IdPlant = c.IdPlant,
            IdSale = c.IdSale,
            CostHarvest = c.CostHarvest,
            QuantityHarvest = c.QuantityHarvest,
            StartHarvest = c.StartHarvest,
            EndHarvest = c.EndHarvest,
            NameHarvest = c.NameHarvest,
            UseHarvest = c.UseHarvest



        };
        /// <summary>
        /// Get all Harvests
        /// </summary>
        /// <param name="loadParams"></param>
        /// <returns></returns>
        /*[HttpGet(Name = "GetHarvests")]
        public async Task<List<HarvestViewModel2>> GetAll([FromQuery] LoadParams loadParams)
        {
            var query =ctx.Harvests.AsQueryable();
            if (!string.IsNullOrWhiteSpace(loadParams.Filter))
            {
                query = query.Where(m => m.NameHarvest.Contains(loadParams.Filter));
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
        }*/
        /// <summary>
        /// Get harvest by his id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id}", Name = "GetHarvestById")]
        public async Task<ActionResult<HarvestViewModel2>> Get(int id)
        {
            var harvest = await ctx.Harvests.Where(m => m.IdHarvest == id)
                                       .Select(projection)
                                       .FirstOrDefaultAsync();

            if (harvest == null)
            {
                return Problem(statusCode: StatusCodes.Status404NotFound, detail: $"No datat for id = {id}");
            }
            else
            {
                return harvest;
            }
        }
        /// <summary>
        /// Add a harvest in the database
        /// </summary>
        /// <param name="model">Harvest id from route must match with id from the model>
        
        /// <returns></returns>
        [HttpPost(Name = "AddHarvest")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create(HarvestViewModel2 model, int id)
        {
            Harvest harvest = new()
            {
                

            IdHarvest = id,
            IdPlant = model.IdPlant,
            IdSale = model.IdSale,
            CostHarvest = model.CostHarvest,
            QuantityHarvest = model.QuantityHarvest,
            StartHarvest = model.StartHarvest,
            EndHarvest = model.EndHarvest,
            NameHarvest = model.NameHarvest,
            UseHarvest = model.UseHarvest
            };
            ctx.Add(harvest);
            await ctx.SaveChangesAsync();
            var AddedHarvest = await Get(harvest.IdHarvest);
            return CreatedAtAction(nameof(Get), new { id = harvest.IdHarvest }, AddedHarvest.Value);
        }

        /// <summary>
        /// Update Harvest
        /// </summary>
        /// <param name="id"></param>
        /// <param name="model">Harvest id from route must match with id from the model</param>
        /// <returns></returns>
        [HttpPut("{id}", Name = "UpdateHarvest")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Update(int id, HarvestViewModel2 model)
        {
            if (model.IdHarvest != id) //ModelState.IsValid i model != null are automatically evalued because of [ApiController]
            {
                return Problem(statusCode: StatusCodes.Status400BadRequest, detail: $"Different ids {id} vs {model.IdHarvest}");
            }
            else
            {
                var harvest = await ctx.Harvests.FindAsync(id);
                if (harvest == null)
                {
                    return Problem(statusCode: StatusCodes.Status404NotFound, detail: $"Invalid id = {id}");
                }
                harvest.IdHarvest = model.IdHarvest;
                harvest.IdPlant = model.IdPlant;
                harvest.IdSale = model.IdSale;
                harvest.CostHarvest = model.CostHarvest;
                harvest.QuantityHarvest = model.QuantityHarvest;
                harvest.StartHarvest = model.StartHarvest;
                harvest.EndHarvest = model.EndHarvest;
                harvest.NameHarvest = model.NameHarvest;
                harvest.UseHarvest = model.UseHarvest;


                await ctx.SaveChangesAsync();
                return NoContent();
            }
        }

        /// <summary>
        /// Delete a Harvest thanks to his id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        /// <reponse code ="204">if Harvest is deleted</reponse>
        /// <reponse code ="404">if Harvest doesn't</reponse>
        [HttpDelete("{id}", Name = "DeleteHarvest")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int id)
        {
            var harvest = await ctx.Harvests.FindAsync(id);
            if (harvest == null)
            {
                return NotFound();
            }
            else
            {
                ctx.Remove(harvest);
                await ctx.SaveChangesAsync();
                return NoContent();
            };
        }
    }
}
