using RPPP_WebApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using RPPP_WebApp.ViewModelsApi;
using RPPP_WebApp.ModelsAPI;
using OfficeOpenXml.FormulaParsing.Excel.Functions.DateTime;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Math;

namespace RPPP_WebApp.ControllerAPI
{
    [ApiController]
    [Route("[controller]")]
    public class PlantController : ControllerBase
    {
        private readonly RPPP13Context ctx;

        private static Dictionary<string, Expression<Func<Plant, object>>> orderSelectors = new()
        {
            [nameof(PlantViewModel.IdPlant).ToLower()] = c => c.IdPlant,
            [nameof(PlantViewModel.Species).ToLower()] = c => c.Species,
            [nameof(PlantViewModel.SpeciesGroup).ToLower()] = c => c.SpeciesGroup,
            [nameof(PlantViewModel.FruitVegetable).ToLower()] = c => c.FruitVegetable,
            [nameof(PlantViewModel.Origin).ToLower()] = c => c.Origin,
            [nameof(PlantViewModel.Quantity).ToLower()] = c => c.Quantity,
            [nameof(PlantViewModel.StartDate).ToLower()] = c => c.StartDate,
            [nameof(PlantViewModel.EndDate).ToLower()] = c => c.EndDate,
            [nameof(PlantViewModel.Product).ToLower()] = c => c.Product,
            [nameof(PlantViewModel.ProductDate).ToLower()] = c => c.ProductDate,
            [nameof(PlantViewModel.IdPlot).ToLower()] = c => c.IdPlot,

        };

        public PlantController(RPPP13Context ctx)
        {
            this.ctx = ctx;
        }

        private static Expression<Func<Plant, PlantViewModel>> projection = c => new PlantViewModel
        {
            IdPlant = c.IdPlant,
            IdPlot = c.IdPlot,
            Species = c.Species,
            SpeciesGroup = c.SpeciesGroup,
            FruitVegetable = c.FruitVegetable,
            Origin = c.Origin,
            Quantity = c.Quantity,
            StartDate = c.StartDate,
            EndDate = c.EndDate,
            Product = c.Product,
            ProductDate = c.ProductDate



        };
        /// <summary>
        /// Get all Plants
        /// </summary>
        /// <param name="loadParams"></param>
        /// <returns></returns>
        [HttpGet(Name = "GetPlants")]
        public async Task<List<PlantViewModel>> GetAll([FromQuery] LoadParams loadParams)
        {
            var query = ctx.Plants.AsQueryable();
            if (!string.IsNullOrWhiteSpace(loadParams.Filter))
            {
                query = query.Where(m => m.Species.Contains(loadParams.Filter));
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
        /// Get plant by his id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id}", Name = "GetPlantById")]
        public async Task<ActionResult<PlantViewModel>> Get(int id)
        {
            var plant = await ctx.Plants.Where(m => m.IdPlant == id)
                                       .Select(projection)
                                       .FirstOrDefaultAsync();

            if (plant == null)
            {
                return Problem(statusCode: StatusCodes.Status404NotFound, detail: $"No data for id = {id}");
            }
            else
            {
                return plant;
            }
        }
        /// <summary>
        /// Add a plant in the database
        /// </summary>
        /// <param name="model">Plant id from route must match with id from the model</param>
        /// <returns></returns>
        [HttpPost(Name = "AddPlant")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create(PlantViewModel model, int id)
        {
            Plant plant = new()
            {
                IdPlant = model.IdPlant,
                IdPlot = model.IdPlot,
                Species = model.Species,
                SpeciesGroup = model.SpeciesGroup,
                FruitVegetable = model.FruitVegetable,
                Origin = model.Origin,
                Quantity = model.Quantity,
                StartDate = model.StartDate,
                EndDate = model.EndDate,
                Product = model.Product,
                ProductDate = model.ProductDate
            };
            ctx.Add(plant);
            await ctx.SaveChangesAsync();
            var AddedPlant = await Get(plant.IdPlant);
            return CreatedAtAction(nameof(Get), new { id = plant.IdPlant }, AddedPlant.Value);
        }

        /// <summary>
        /// Update Plant
        /// </summary>
        /// <param name="id"></param>
        /// <param name="model">Plant id from route must match with id from the model</param>
        /// <returns></returns>
        [HttpPut("{id}", Name = "UpdatePlant")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Update(int id, PlantViewModel model)
        {
            if (model.IdPlant != id) //ModelState.IsValid i model != null are automatically evalued because of [ApiController]
            {
                return Problem(statusCode: StatusCodes.Status400BadRequest, detail: $"Different ids {id} vs {model.IdPlant}");
            }
            else
            {
                var plant = await ctx.Plants.FindAsync(id);
                if (plant == null)
                {
                    return Problem(statusCode: StatusCodes.Status404NotFound, detail: $"Invalid id = {id}");
                }
                plant.IdPlot = model.IdPlot;
                plant.Species = model.Species;
                plant.SpeciesGroup = model.SpeciesGroup;
                plant.FruitVegetable = model.FruitVegetable;
                plant.Origin = model.Origin;
                plant.Quantity = model.Quantity;
                plant.StartDate = model.StartDate;
                plant.EndDate = model.EndDate;
                plant.Product = model.Product;
                plant.ProductDate = model.ProductDate;

                await ctx.SaveChangesAsync();
                return NoContent();
            }
        }

        /// <summary>
        /// Delete a Plant thanks to his id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        /// <reponse code ="204">if Plant is deleted</reponse>
        /// <reponse code ="404">if Plant doesn't</reponse>
        [HttpDelete("{id}", Name = "DeletePlant")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int id)
        {
            var plant = await ctx.Plants.FindAsync(id);
            if (plant == null)
            {
                return NotFound();
            }
            else
            {
                ctx.Remove(plant);
                await ctx.SaveChangesAsync();
                return NoContent();
            };
        }
    }
}
