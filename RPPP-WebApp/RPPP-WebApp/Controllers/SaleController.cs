using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using RPPP_WebApp.Extensions;
using RPPP_WebApp.Extensions.Selectors;
using RPPP_WebApp.Models;
using RPPP_WebApp.ViewModels;
using static System.Reflection.Metadata.BlobBuilder;

namespace RPPP_WebApp.Controllers
{
    public class SaleController : Controller
    {
        private readonly RPPP13Context ctx;
        private readonly ILogger<SaleController> logger;
        private readonly AppSettings appData;
        public SaleController(RPPP13Context ctx, IOptionsSnapshot<AppSettings> options, ILogger<SaleController> logger)
        {
            this.ctx = ctx;
            appData = options.Value;
            this.logger = logger;
        }
        public IActionResult Index(int page = 1, int sort = 1, bool ascending = true)
        {
            int pagesize = appData.PageSize;

            var query = ctx.Sales
                .Include(h => h.IdHarvestNavigation)
               // .ThenInclude(a => a.IdSaleNavigation)
                .AsNoTracking();


            int count = query.Count();
            if (count == 0)
            {
                TempData[Constants.Message] = "No sale in the database";
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

            query = query.ApplySort(sort,ascending);

            var sales = query
                            .Include(a => a.Harvests)
                            .Skip((page - 1) * pagesize)
                            .Take(pagesize)
                            .ToList();
            foreach (var sale in sales)
            {
                sale.Harvests = ctx.Harvests.Where(p => p.IdHarvest == sale.IdHarvest).ToList();
            }
            var model = new SaleViewModel
            {
                Sales = sales,
                PagingInfo = pagingInfo
            };

            return View(model);
        }
        public async Task<IActionResult> Create(Sale sale)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var harvest = ctx.Harvests.Find(sale.IdHarvest);
                    harvest.Sales.Add(sale);
                    ctx.Add(sale);
                    ctx.Add(sale);
                    ctx.SaveChanges();
                    TempData[Constants.Message] =
                    $"Sale {sale.PlantSeedling} has been added.";
                    TempData[Constants.ErrorOccurred] = false;
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception exc)
                {
                    ModelState.AddModelError(string.Empty,
                    exc.CompleteExceptionMessage());
                    await PrepareDropDownListsPerson();
                    await PrepareDropDownListsHarvest();
                    return View(sale);
                }
            }
            else
            {
                await PrepareDropDownListsPerson();
                await PrepareDropDownListsHarvest();
                return View(sale);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            await PrepareDropDownListsPerson();
            await PrepareDropDownListsHarvest();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]

        public IActionResult Delete(int IdSale, int page = 1, int sort = 1, bool ascending = true)
        {
            var sale = ctx.Sales.Find(IdSale);
            if (sale != null)
            {
                try
                {
                    string SalePlantSeedling = sale.PlantSeedling;
                    ctx.Remove(sale);
                    ctx.SaveChanges();
                    string message = $"Sale {SalePlantSeedling} deleted";
                    logger.LogInformation(message);
                    TempData[Constants.Message] = message;
                    TempData[Constants.ErrorOccurred] = false;
                }
                catch (Exception exc)
                {
                    string message = "Error deleting the sale " + exc.CompleteExceptionMessage();
                    TempData[Constants.Message] = message;
                    TempData[Constants.ErrorOccurred] = true;
                    logger.LogError(message);
                }
            }
            else
            {
                string message = $"There is no sale with code {IdSale}";
                logger.LogWarning(message);
                TempData[Constants.Message] = message;
                TempData[Constants.ErrorOccurred] = true;
            }
            return RedirectToAction(nameof(Index), new { page, sort, ascending });
        }

        /*[HttpGet]
        public async Task<IActionResult> Edit(int id, int page = 1, int sort = 1, bool ascending = true)
        {
            var sale = ctx.Sales.AsNoTracking().Where(d => d.IdSale == id).SingleOrDefault();
            if (sale == null)
            {
                string message = $"There is no sale with id {id}";
                logger.LogWarning(message);
                return NotFound(message);
            }
            else
            {
                ViewBag.Page = page;
                ViewBag.Sort = sort;
                ViewBag.Ascending = ascending;
                await PrepareDropDownListsHarvest();
                return View(sale);
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
                Sale sale = await ctx.Sales
                                  .Where(d => d.IdSale == id)
                                  .FirstOrDefaultAsync();
                if (sale == null)
                {
                    return NotFound("Invalid sale id: " + id);
                }

                if (await TryUpdateModelAsync<Sale>(sale, "",
                    d => d.IdSale,
                    d => d.PlantSeedling,
                    d => d.QuantitySale,
                    d => d.PriceSale,
                    d => d.IdHarvestNavigation,
                    d => d.IdPerson,
                    d => d.IdAnonymous
                ))
                {
                    ViewBag.Page = page;
                    ViewBag.Sort = sort;
                    ViewBag.Ascending = ascending;
                    try
                    {
                        await ctx.SaveChangesAsync();
                        TempData[Constants.Message] = "Sale " +id +" updated.";
                        TempData[Constants.ErrorOccurred] = false;
                        return RedirectToAction(nameof(Index), new { page, sort, ascending });
                    }
                    catch (Exception exc)
                    {
                        ModelState.AddModelError(string.Empty, exc.CompleteExceptionMessage());
                        return View(sale);
                    }
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Cannot update model");
                    return View(sale);
                }
            }
            catch (Exception exc)
            {
                TempData[Constants.Message] = exc.CompleteExceptionMessage();
                TempData[Constants.ErrorOccurred] = true;
                return RedirectToAction(nameof(Edit), id);
            }
        }


        public async Task<IActionResult> Details(int id)
        {
                var sale = await ctx.Sales.Where(m => m.IdSale == id)
               .Select(m => new SaleViewModel
               {
                   IdSale = m.IdSale,
                   PlantSeedling = m.PlantSeedling,
                   QuantitySale = (int) m.QuantitySale,
                   PriceSale = (int)m.PriceSale,
                   IdHarvest = (int)m.IdHarvest,
                   IdPerson =(int) m.IdPerson,
                   IdAnonymous = (int)m.IdAnonymous,
                   Harvests = m.Harvests.Select(v => new HarvestViewModel
                   {
                       IdHarvest = v.IdHarvest,
                       IdSale = v.IdSale,
                   }).ToList()
               })
               .SingleOrDefaultAsync();

                if (sale != null)
                {
                    return View(sale);
                }
                else
                {
                    return NotFound($"Invalid sale id: {id}");
                }

        }*/

        public async Task<IActionResult> Details(int id, int page = 1, int sort = 1, bool ascending = true, string viewName = nameof(Details))
        {
            var sale = await ctx.Sales
                                    .Where(d => d.IdSale == id)
                                    .Select(d => new SaleViewModel
                                    {
                                        IdSale = d.IdSale,
                                        PlantSeedling = d.PlantSeedling,
                                        QuantitySale = (int)d.QuantitySale,
                                        PriceSale = (int)d.PriceSale,
                                        IdHarvest = (int)d.IdHarvest,
                                        IdPerson = d.IdPerson,
                                        IdAnonymous = d.IdAnonymous,
                                    })
                                    .FirstOrDefaultAsync();
            if (sale == null)
            {
                return NotFound($"Sale {id} does not exist.");
            }
            else
            {

                //loading items
                var harvest = await ctx.Harvests
                                     .Where(s => s.IdHarvest == sale.IdHarvest)
                                     .OrderBy(s => s.IdHarvest)
                                     .Select(m => new HarvestViewModel
                                     {
                                         IdHarvest = m.IdHarvest,
                                         StartHarvest = m.StartHarvest,
                                         EndHarvest = m.EndHarvest,
                                         UseHarvest = m.UseHarvest,
                                         QuantityHarvest = m.QuantityHarvest,
                                         CostHarvest = m.CostHarvest,
                                         IdPlant = m.IdPlant,
                                         IdSale = m.IdSale,
                                         NameHarvest = m.NameHarvest,
                                     })
                                     .ToListAsync();
                sale.Harvests = harvest;



                ViewBag.Page = page;
                ViewBag.Sort = sort;
                ViewBag.Ascending = ascending;
                await PrepareDropDownListsHarvest();
                await PrepareDropDownListsPerson();

                return View(viewName, sale);
            }
        }

        [HttpGet]
        public Task<IActionResult> Edit(int id, int page = 1, int sort = 1, bool ascending = true)
        {
            return Details(id, page, sort, ascending, viewName: nameof(Edit));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(SaleViewModel model, int id, int page = 1, int sort = 1, bool ascending = true)
        {
            //for different approaches (attach, update, only id) see
            //https://docs.microsoft.com/en-us/aspnet/core/data/ef-mvc/crud#update-the-edit-page

            try
            {
                var sale = await ctx.Sales
                                  .Include(d => d.Harvests)
                                  .Where(d => d.IdSale == id)
                                  .FirstOrDefaultAsync();
                if (sale == null)
                {
                    return NotFound("Invalid sale id: " + id);
                }

                sale.PlantSeedling = model.PlantSeedling;
                sale.QuantitySale = model.QuantitySale;
                sale.PriceSale = model.PriceSale;
                sale.IdHarvest = model.IdHarvest;
                sale.IdPerson = model.IdPerson;
                sale.IdAnonymous = model.IdAnonymous;

                    List<int> harvestsId = model.Harvests
                                .Where(s => s.IdHarvest > 0)
                                .Select(s => s.IdHarvest)
                                .ToList();

                //remove all not anymore in the model
                ctx.RemoveRange(sale.Harvests.Where(i => !harvestsId.Contains(i.IdHarvest)));


                foreach (var harvest in model.Harvests)
                {
                    Harvest newOrUpdatedharvest;
                    if (harvest.IdHarvest > 0)
                    {
                        newOrUpdatedharvest = sale.Harvests.First(s => s.IdHarvest == harvest.IdHarvest);
                    }
                    else
                    {
                        newOrUpdatedharvest = new();
                        sale.Harvests.Add(newOrUpdatedharvest);
                    }
                    newOrUpdatedharvest.StartHarvest = harvest.StartHarvest;
                    newOrUpdatedharvest.EndHarvest = harvest.EndHarvest;
                    newOrUpdatedharvest.UseHarvest = harvest.UseHarvest;
                    newOrUpdatedharvest.QuantityHarvest = harvest.QuantityHarvest;
                    newOrUpdatedharvest.CostHarvest = harvest.CostHarvest;
                    newOrUpdatedharvest.NameHarvest = harvest.NameHarvest;

                }

                ViewBag.Page = page;
                ViewBag.Sort = sort;
                ViewBag.Ascending = ascending;
                try
                {
                    await ctx.SaveChangesAsync();
                    TempData[Constants.Message] = $"Sale {sale.IdSale} updated.";
                    TempData[Constants.ErrorOccurred] = false;
                    return RedirectToAction(nameof(Details), new { page, sort, ascending, id = sale.IdSale });
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
        private async Task PrepareDropDownListsHarvest()
        {
            var harvest = await ctx.Harvests.OrderBy(d => d.NameHarvest)
            .Select(d => new { d.IdHarvest, d.NameHarvest })
            .ToListAsync();
            ViewBag.Harvest = new SelectList(harvest,
            "IdHarvest", "NameHarvest");
        }
        private async Task PrepareDropDownListsPerson()
        {
            var person = await ctx.People.OrderBy(d => d.LastName)
            .Select(d => new { d.IdPerson, d.LastName })
            .ToListAsync();
            ViewBag.People = new SelectList(person,
            "IdPerson", "Name");
        }
    }
}