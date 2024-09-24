using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using RPPP_WebApp.Extensions;
using RPPP_WebApp.Extensions.Selectors;
using RPPP_WebApp.Models;
using RPPP_WebApp.ViewModels;
using System.Diagnostics.Metrics;
using System.Text.Json;
using static System.Reflection.Metadata.BlobBuilder;

namespace RPPP_WebApp.Controllers
{
    public class SoilController : Controller
    {
        private readonly RPPP13Context ctx;
        private readonly ILogger<SoilController> logger;
        private readonly AppSettings appData;
        public SoilController(RPPP13Context ctx, IOptionsSnapshot<AppSettings> options, ILogger<SoilController> logger)
        {
            this.ctx = ctx;
            appData = options.Value;
            this.logger = logger;
        }
       
        public IActionResult Index(int page = 1, int sort = 1, bool ascending = true)
        {
            int pagesize = appData.PageSize;
            var query = ctx.Soils
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
                return RedirectToAction(nameof(Index), new { page = 1, sort, ascending });
            }

            query = query.ApplySort(sort,
                                    ascending);

            var soil = query
                            .Skip((page - 1) * pagesize)
                            .Take(pagesize)
                            .ToList();
            foreach (var soils in soil)
            {
                soils.Plots = ctx.Plots.Where(p => p.IdSoil == soils.IdSoil).ToList();
            }
            var model = new SoilViewModel
            {
                Soil = soil,
                PagingInfo = pagingInfo
            };

            return View(model);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Soil soil)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    ctx.Add(soil);
                    ctx.SaveChanges();
                    TempData[Constants.Message] =
                    $"Soil {soil.SoilName} has been added.";
                    TempData[Constants.ErrorOccurred] = false;
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception exc)
                {
                    ModelState.AddModelError(string.Empty,
                    exc.CompleteExceptionMessage());
                    return View(soil);
                }
            }
            else
            {
                return View(soil);
            }
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int IdSoil, int page = 1, int sort = 1, bool ascending = true)
        {
            var soil = ctx.Soils.Find(IdSoil);
            if (soil != null)
            {
                try
                {
                    string SoilName = soil.SoilName;
                    ctx.Remove(soil);
                    ctx.SaveChanges();
                    string message = $"Soil {SoilName} deleted";
                    logger.LogInformation(message);
                    TempData[Constants.Message] = message;
                    TempData[Constants.ErrorOccurred] = false;
                }
                catch (Exception exc)
                {
                    string message = "Eror deleting the soil " + exc.CompleteExceptionMessage();
                    TempData[Constants.Message] = message;
                    TempData[Constants.ErrorOccurred] = true;
                    logger.LogError(message);
                }
            }
            else
            {
                string message = $"There is no soil with code {IdSoil}";
                logger.LogWarning(message);
                TempData[Constants.Message] = message;
                TempData[Constants.ErrorOccurred] = true;
            }
            return RedirectToAction(nameof(Index), new { page, sort, ascending });
        }


        [HttpGet]
        public IActionResult Edit(int id, int page = 1, int sort = 1, bool ascending = true)
        {
            var soil = ctx.Soils.AsNoTracking().Where(d => d.IdSoil == id).SingleOrDefault();
            if (soil == null)
            {
                string message = $"There is no soil with id {id}";
                logger.LogWarning(message);
                return NotFound(message);
            }
            else
            {
                ViewBag.Page = page;
                ViewBag.Sort = sort;
                ViewBag.Ascending = ascending;
                return View(soil);
            }
        }

        [HttpPost, ActionName("Edit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(int id, int page = 1, int sort = 1, bool ascending = true)
        {
            //for different approaches (attach, update, only id) see
            //https://docs.microsoft.com/en-us/aspnet/core/data/ef-mvc/crud#update-the-edit-page

            try
            {
                Soil soil = await ctx.Soils
                                  .Where(d => d.IdSoil == id)
                                  .FirstOrDefaultAsync();
                if (soil == null)
                {
                    return NotFound("Invalid soil id: " + id);
                }

                if (await TryUpdateModelAsync<Soil>(soil, "",
                    d => d.SoilName
                ))
                {
                    ViewBag.Page = page;
                    ViewBag.Sort = sort;
                    ViewBag.Ascending = ascending;
                    try
                    {
                        await ctx.SaveChangesAsync();
                        TempData[Constants.Message] = "Soil updated.";
                        TempData[Constants.ErrorOccurred] = false;
                        return RedirectToAction(nameof(Index), new { page, sort, ascending });
                    }
                    catch (Exception exc)
                    {
                        ModelState.AddModelError(string.Empty, exc.CompleteExceptionMessage());
                        return View(soil);
                    }
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Cannot update model");
                    return View(soil);
                }
            }
            catch (Exception exc)
            {
                TempData[Constants.Message] = exc.CompleteExceptionMessage();
                TempData[Constants.ErrorOccurred] = true;
                return RedirectToAction(nameof(Edit), id);
            }
        }
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var soil = await ctx.Soils.Where(m => m.IdSoil == id)


           .Select(m => new SoilViewModel
           {
              IdSoil = m.IdSoil,
              SoilName = m.SoilName,
              Plots = m.Plots
           })
           .SingleOrDefaultAsync();
            if (soil != null)
            {
                return View(soil);
            }
            else
            {
                return NotFound($"Invalid soil id: {id}");
            }

        }

    }
}
