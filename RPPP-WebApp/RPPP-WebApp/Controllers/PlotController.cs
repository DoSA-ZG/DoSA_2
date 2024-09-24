using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Options;
using RPPP_WebApp.Extensions;
using RPPP_WebApp.Extensions.Selectors;
using RPPP_WebApp.Models;
using RPPP_WebApp.ViewModels;
using System.Diagnostics.Metrics;
using System.Reflection.Metadata;
using System.Text.Json;
using System.Xml.Linq;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using static System.Reflection.Metadata.BlobBuilder;



namespace RPPP_WebApp.Controllers
{
    public class PlotsController : Controller
    {
        private readonly RPPP13Context ctx;
        private readonly ILogger<PlotsController> logger;
        private readonly AppSettings appData;
        public PlotsController(RPPP13Context ctx, IOptionsSnapshot<AppSettings> options, ILogger<PlotsController> logger)
        {
            this.ctx = ctx;
            appData = options.Value;
            this.logger = logger;
        }
        //public IActionResult Index()
        //{
        //    var plots = ctx.Plots
        //               .AsNoTracking()
        //               .OrderBy(d => d.IdPlot)
        //               .ToList();
        //    return View("IndexSimple", plots);
        //}

        public IActionResult Index(int page = 1, int sort = 1, bool ascending = true)
        {
            int pagesize = appData.PageSize;
            var query = ctx.Plots
            .AsNoTracking();
            int count = query.Count();
            if (count == 0)
            {
                TempData[Constants.Message] = "No data in the database";
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
                return RedirectToAction(nameof(Index), new { page = pagingInfo.TotalPages, sort, ascending });
            }

            query = query.ApplySort(sort,
                                    ascending);

            var plots = query
                            .Skip((page - 1) * pagesize)
                            .Take(pagesize)
                            .ToList();
            foreach (var plot in plots)
            {
                plot.Plants = ctx.Plants.Where(p => p.IdPlot == plot.IdPlot).ToList();
            }
            var model = new PlotViewModel
            {
                Plots = plots,
                PagingInfo = pagingInfo
            };

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            await PrepareDropDownListsLease();
            await PrepareDropDownListsSoil();
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Plot plot)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var soil = ctx.Soils.Find(plot.IdSoil);
                    soil.Plots.Add(plot);
                    ctx.Add(plot);
                    ctx.SaveChanges();
                    TempData[Constants.Message] =
                    $"Plot {plot.PlotName} has been added.";
                    TempData[Constants.ErrorOccurred] = false;
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception exc)
                {
                    ModelState.AddModelError(string.Empty,
                    exc.CompleteExceptionMessage());
                    await PrepareDropDownListsLease();
                    await PrepareDropDownListsSoil();
                    return View(plot);
                }
            }
            else
            {
                await PrepareDropDownListsLease();
                await PrepareDropDownListsSoil();
                return View(plot);
            }
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int IdPlot, int page = 1, int sort = 1, bool ascending = true)
        {

            var plot = ctx.Plots.Find(IdPlot);
            if (plot != null)
            {
                try
                {
                    string PlotName = plot.PlotName;
                    ctx.Remove(plot);
                    ctx.SaveChanges();
                    string message = $"Plot {PlotName} deleted";
                    logger.LogInformation(message);
                    TempData[Constants.Message] = message;
                    TempData[Constants.ErrorOccurred] = false;
                }
                catch (Exception exc)
                {
                    string message = "Eror deleting the plot " + exc.CompleteExceptionMessage();
                    TempData[Constants.Message] = message;
                    TempData[Constants.ErrorOccurred] = true;
                    logger.LogError(message);
                }
            }
            else
            {
                string message = $"There is no plot with code {IdPlot}";
                logger.LogWarning(message);
                TempData[Constants.Message] = message;
                TempData[Constants.ErrorOccurred] = true;
            }
            return RedirectToAction(nameof(Index), new { page, sort, ascending });
        }

        public async Task<IActionResult> Details(int id,  int page = 1, int sort = 1, bool ascending = true, string viewName = nameof(Details))
        {
            var plot = await ctx.Plots
                                    .Where(d => d.IdPlot == id)
                                    .Select(d => new PlotViewModel
                                    {
                                        IdPlot = d.IdPlot,
                                        PlotName = d.PlotName,
                                        PlotGps = d.PlotGps,
                                        Infrastructure = d.Infrastructure,
                                        Material = d.Material,
                                        SunIntensity = d.SunIntensity,
                                        IdLease = d.IdLease,
                                        IdSoil = d.IdSoil                                        
                                    })
                                    .FirstOrDefaultAsync();
            if (plot == null)
            {
                return NotFound($"Plot {id} does not exist.");
            }
            else
            {
                
                //loading items
                var plant = await ctx.Plants
                                     .Where(s => s.IdPlot == plot.IdPlot)
                                     .OrderBy(s => s.IdPlant)
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
                                     .ToListAsync();
                plot.Plants = plant;

                

                ViewBag.Page = page;
                ViewBag.Sort = sort;
                ViewBag.Ascending = ascending;
                await PrepareDropDownListsLease();
                await PrepareDropDownListsSoil();

                return View(viewName, plot);
            }
        }

        [HttpGet]
        public  Task<IActionResult> Edit(int id, int page = 1, int sort = 1, bool ascending = true)
        {
            

           return Details(id,page, sort, ascending,viewName: nameof(Edit));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(PlotViewModel model,int id, int page = 1, int sort = 1, bool ascending = true)
        {
            //for different approaches (attach, update, only id) see
            //https://docs.microsoft.com/en-us/aspnet/core/data/ef-mvc/crud#update-the-edit-page

            try
            {
                var plot = await ctx.Plots
                                  .Include(d=>d.Plants)
                                  .Where(d => d.IdPlot == id)
                                  .FirstOrDefaultAsync();
                if (plot == null)
                {
                    return NotFound("Invalid plot id: " + id);
                }
                

                plot.PlotGps = model.PlotGps;
                plot.PlotName = model.PlotName;
                plot.Infrastructure = model.Infrastructure;
                plot.IdSoil = model.IdSoil;
                plot.IdLease = model.IdLease;
                plot.Material = model.Material;
                plot.SunIntensity = model.SunIntensity;
                   
                    List<int> plantsId = model.Plants
                                    .Where(s => s.IdPlant > 0)
                                    .Select(s => s.IdPlant)
                                    .ToList();

                //remove the line of the plant once the edit is saved
                ctx.RemoveRange(plot.Plants.Where(i => !plantsId.Contains(i.IdPlant)));


                    foreach(var plant in model.Plants)
                    {
                        Plant newOrUpdatedplant;
                        if (plant.IdPlant > 0)
                        {
                            newOrUpdatedplant = plot.Plants.First(s => s.IdPlant == plant.IdPlant);
                        }
                        else
                        {
                            newOrUpdatedplant = new();
                            plot.Plants.Add(newOrUpdatedplant);
                        }
                        newOrUpdatedplant.Quantity = plant.Quantity;
                        newOrUpdatedplant.Origin = plant.Origin;
                        newOrUpdatedplant.StartDate = plant.StartDate;
                        newOrUpdatedplant.EndDate = plant.EndDate;
                        newOrUpdatedplant.Product = plant.Product;
                        newOrUpdatedplant.ProductDate = plant.ProductDate;


                    }

                    ViewBag.Page = page;
                    ViewBag.Sort = sort;
                    ViewBag.Ascending = ascending;
                    try
                    {
                        await ctx.SaveChangesAsync();
                        TempData[Constants.Message] = $"Plot {plot.PlotName} updated.";
                        TempData[Constants.ErrorOccurred] = false;
                        return RedirectToAction(nameof(Details), new { page, sort, ascending, id = plot.IdPlot });
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


      
        private async Task PrepareDropDownListsSoil()
        {
            var soil = await ctx.Soils.OrderBy(d => d.SoilName)
            .Select(d => new { d.SoilName, d.IdSoil })
            .ToListAsync();
            ViewBag.Soil = new SelectList(soil,
            "IdSoil", "SoilName");
        }
        private async Task PrepareDropDownListsLease()
        {
            var lease = await ctx.Leases.OrderBy(d => d.IdLease)
            .Select(d => d.IdLease )
            .ToListAsync();
            ViewBag.Lease = new SelectList(lease,
            "IdLease");
        }

    }
}
