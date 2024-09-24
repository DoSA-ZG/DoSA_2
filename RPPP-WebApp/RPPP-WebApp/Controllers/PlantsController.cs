using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.VisualBasic;
using RPPP_WebApp.Models;
using RPPP_WebApp.ViewModels;
using RPPP_WebApp.Extensions.Selectors;
using System.Text.Json;
using RPPP_WebApp.Extensions;
using static System.Reflection.Metadata.BlobBuilder;
using System.Numerics;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace RPPP_WebApp.Controllers
{
    public class PlantsController : Controller
    {
        private readonly RPPP13Context ctx;
        private readonly ILogger<PlantsController> logger;
        private readonly AppSettings appData;
        public PlantsController(RPPP13Context ctx, IOptionsSnapshot<AppSettings> options, ILogger<PlantsController> logger)
        {
            this.ctx = ctx;
            this.logger = logger;
            appData = options.Value;
        }


        /*  public IActionResult Index()
          {
            var Plant = ctx.Plants
                            .AsNoTracking()
                            .OrderBy(d => d.IdPlant)
                            .ToList();
            return View("Index", Plant);
          }*/
        public IActionResult Index(int page = 1, int sort = 1, bool ascending = true)
        {
            int pagesize = appData.PageSize;

            var query = ctx.Plants
                .Include(d =>d.IdPlotNavigation)
                           .AsNoTracking();

            int count = query.Count();
            if (count == 0)
            {
                string message = "There is no Plant in the database";
                logger.LogInformation(message);
                TempData[Constants.Message] = "message";
                TempData[Constants.ErrorOccurred] = false;
                return RedirectToAction(nameof(Create));
            }

            var pagingInfo = new PagingInfo
            {
                CurrentPage = page,
                Sort = sort,
                Ascending = ascending,
                ItemsPerPage = pagesize,
                TotalItems = count
            };
            if (page < 1 || page > pagingInfo.TotalPages)
            {
                return RedirectToAction(nameof(Index), new { page = 1, sort, ascending });
            }

            query = query.ApplySort(sort, ascending);

            var plants = query
                            .Skip((page - 1) * pagesize)
                            .Take(pagesize)
                            .ToList();

            foreach (var plant in plants)
            {
                plant.Harvests = ctx.Harvests.Where(p => p.IdPlant == plant.IdPlant).ToList();
            }

            var model = new PlantsViewModel
            {
                Plants = plants,
                PagingInfo = pagingInfo
            };

            return View(model);
        }


        public async Task<IActionResult> Create(Plant plant)
        {
            if (ModelState.IsValid)
            {
                try
                {

                    var plot = ctx.Plots.Find(plant.IdPlot);
                    plot.Plants.Add(plant);
                    ctx.Add(plant);
                    ctx.SaveChanges();
                    TempData[Constants.Message] =
                    $"Plant of {plant.IdPlant} has been added.";
                    TempData[Constants.ErrorOccurred] = false;
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception exc)
                {
                    ModelState.AddModelError(string.Empty,
                    exc.CompleteExceptionMessage());
                    await PrepareDropDownListsPlot();
                    return View(plant);
                }
            }
            else
            {
                return View(plant);
            }
        }
        [HttpGet]

        public async  Task<IActionResult> Create()
        {
            await PrepareDropDownListsPlot();
            return View();
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int IdPlant, int page = 1, int sort = 1, bool ascending = true)
        {
            var plant = ctx.Plants.Find(IdPlant);
            if (plant != null)
            {
                try
                {
                    int jsp = plant.IdPlant;
                    ctx.Remove(plant);
                    ctx.SaveChanges();
                    string message = $"Plant {plant.IdPlant} deleted";
                    logger.LogInformation(message);
                    TempData[Constants.Message] = message;
                    TempData[Constants.ErrorOccurred] = false;
                }
                catch (Exception exc)
                {
                    string message = "Eror deleting the plant " + exc.CompleteExceptionMessage();
                    TempData[Constants.Message] = message;
                    TempData[Constants.ErrorOccurred] = true;
                    logger.LogError(message);
                }
            }
            else
            {
                string message = $"There is no plant with code {IdPlant}";
                logger.LogWarning(message);
                TempData[Constants.Message] = message;
                TempData[Constants.ErrorOccurred] = true;
            }
            return RedirectToAction(nameof(Index), new { page, sort, ascending });
        }

        [HttpGet]
        public  Task<IActionResult> Edit(int id, int page = 1, int sort = 1, bool ascending = true)
        {
            //var plant = ctx.Plants.AsNoTracking().Where(d => d.IdPlant == id).SingleOrDefault();
            //if (plant == null)
            //{
            //    string message = $"There is no plant with id {id}";
            //    logger.LogWarning(message);
            //    return NotFound(message);
            //}
            //else
            //{
            //    ViewBag.Page = page;
            //    ViewBag.Sort = sort;
            //    ViewBag.Ascending = ascending;
            //    await PrepareDropDownListsPlot();
            //    return View(plant);
            //}
            return Details(id, page, sort, ascending, viewName: nameof(Edit));
        }

        [HttpPost,]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(PlantsViewModel model ,int id, int page = 1, int sort = 1, bool ascending = true)
        {
            //for different approaches (attach, update, only id) see
            //https://docs.microsoft.com/en-us/aspnet/core/data/ef-mvc/crud#update-the-edit-page

            try
            {
                var plant = await ctx.Plants
                                  .Include(d => d.Harvests)
                                  .Where(d => d.IdPlant == id)
                                  .FirstOrDefaultAsync();
                if (plant == null)
                {
                    return NotFound("Invalid plant id: " + id);
                }

                plant.Species = model.Species;
                plant.IdPlot = model.IdPlot;
                plant.EndDate = model.EndDate;
                plant.StartDate = model.StartDate;
                plant.ProductDate = model.ProductDate;
                plant.FruitVegetable = model.FruitVegetable;
                plant.SpeciesGroup = model.SpeciesGroup;
                plant.Origin = model.Origin;
                plant.Quantity = model.Quantity;
                plant.Product = model.Product;

                List<int> harvestId = model.Harvests
                    .Where(d => d.IdHarvest > 0)
                    .Select(d => d.IdHarvest)
                    .ToList();

                //remove the line of the harvest once the edit is saved
                ctx.RemoveRange(plant.Harvests.Where(i => !harvestId.Contains(i.IdHarvest)));
                int count = 0;
                foreach (var harvest in model.Harvests)
                {
                    count++;
                    Harvest newOrUpdatedharvest;
                    if (harvest.IdHarvest > 0)
                    {
                        count++;
                        newOrUpdatedharvest = plant.Harvests.First(s => s.IdHarvest == harvest.IdHarvest);
                    }
                    else
                    {
                        newOrUpdatedharvest = new();
                        plant.Harvests.Add(newOrUpdatedharvest);
                    }
                    newOrUpdatedharvest.QuantityHarvest = harvest.QuantityHarvest;
                    newOrUpdatedharvest.NameHarvest = harvest.NameHarvest;
                    newOrUpdatedharvest.UseHarvest = harvest.UseHarvest;
                    newOrUpdatedharvest.CostHarvest = harvest.CostHarvest;
                    newOrUpdatedharvest.StartHarvest = harvest.StartHarvest;
                    newOrUpdatedharvest.EndHarvest = harvest.EndHarvest;
                    


                }
                ViewBag.Page = page;
                    ViewBag.Sort = sort;
                    ViewBag.Ascending = ascending;
                    try
                    {
                        await ctx.SaveChangesAsync();
                        TempData[Constants.Message] = $"Plant updated. count {count}";
                        TempData[Constants.ErrorOccurred] = false;
                        return RedirectToAction(nameof(Details), new { page, sort, ascending, id = plant.IdPlant });
                    }
                    catch (Exception exc)
                    {
                        ModelState.AddModelError(string.Empty, exc.CompleteExceptionMessage());
                        return View(model);
                    }
            }
            catch (Exception exc)
            {
                TempData[Constants.Message] = exc.CompleteExceptionMessage();
                TempData[Constants.ErrorOccurred] = true;
                return RedirectToAction(nameof(Edit), id);
            }
        }

        public async Task<IActionResult> Details(int id, int page = 1, int sort = 1, bool ascending = true, string viewName = nameof(Details))
        {
            var plant = await ctx.Plants.Where(m => m.IdPlant == id)
            .Select(m => new PlantsViewModel
            {
                IdPlant = m.IdPlant,
                Species = m.Species,
                SpeciesGroup = m.SpeciesGroup,
                FruitVegetable = m.FruitVegetable,
                Origin = m.Origin,
                Quantity = m.Quantity,
                StartDate = m.StartDate,
                EndDate = m.EndDate,
                Product = m.Product,
                ProductDate = m.ProductDate,
                IdPlot = m.IdPlot
            })
            .SingleOrDefaultAsync();
            if (plant == null)
            {


                return NotFound($"Invalid plant id: {id}");
            }
            else
            {
                var harvest = await ctx.Harvests
                                       .Where(s => s.IdPlant == plant.IdPlant)
                                       .OrderBy(s => s.IdHarvest)
                                       .Select(m => new HarvestViewModel
                                       {
                                           IdSale = m.IdSale,
                                           IdPlant = m.IdPlant,
                                           IdHarvest = m.IdHarvest,
                                           StartHarvest = m.StartHarvest,
                                           EndHarvest = m.EndHarvest,
                                           UseHarvest = m.UseHarvest,
                                           QuantityHarvest = m.QuantityHarvest,
                                           CostHarvest = m.CostHarvest,
                                           NameHarvest = m.NameHarvest,
                                       })
                                       .ToListAsync();
                plant.Harvests = harvest;
                ViewBag.Page = page;
                ViewBag.Sort = sort;
                ViewBag.Ascending = ascending;
                await PrepareDropDownListsPlot();
                return View(viewName, plant);
            }

        }

        private async Task PrepareDropDownListsPlot()
        {
            var plot = await ctx.Plots.OrderBy(d => d.PlotName)
            .Select(d => new { d.PlotName, d.IdPlot })
            .ToListAsync();
            ViewBag.Plot = new SelectList(plot,
            "IdPlot", "PlotName");
        }


    }
}


 
