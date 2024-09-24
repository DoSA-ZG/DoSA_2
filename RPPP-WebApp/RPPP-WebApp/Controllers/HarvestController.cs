using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using RPPP_WebApp.Extensions;
using RPPP_WebApp.Extensions.Selectors;
using RPPP_WebApp.Models;
using RPPP_WebApp.ViewModels;

namespace RPPP_WebApp.Controllers
{
    public class HarvestController : Controller
    {
        private readonly RPPP13Context ctx;
        private readonly ILogger<HarvestController> logger;
        private readonly AppSettings appData;
        public HarvestController(RPPP13Context ctx, IOptionsSnapshot<AppSettings> options, ILogger<HarvestController> logger)
        {
            this.ctx = ctx;
            this.logger = logger;
            appData = options.Value;
        }


        /*  public IActionResult Index()
          {
            var Harvest = ctx.Harvests
                            .AsNoTracking()
                            .OrderBy(d => d.IdHarvest)
                            .ToList();
            return View("Index", Harvest);
          }*/
        public async Task<IActionResult> Index(int page = 1, int sort = 1, bool ascending = true)
        {
            int pagesize = appData.PageSize;
            

            var query = ctx.Harvests
                            .Include(p => p.Workers)
                            .Include(a => a.Sales)
                            .ThenInclude(h => h.IdPersonNavigation)
                            .ThenInclude(y => y.IdPersonNavigation)
                           .AsNoTracking();

            int count = await query.CountAsync();
            if (count == 0)
            {
                string message = "There is no Harvest in the database";
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
            if (count > 0  && page < 1 || page > pagingInfo.TotalPages)
            {
                return RedirectToAction(nameof(Index), new { page = 1, sort, ascending });
            }

            query = query.ApplySort(sort, ascending);

            var harvests = query
                            .Include(h => h.Workers)
                            .ThenInclude(w => w.IdPersonNavigation)
                            .Include(a => a.Sales)
                            .ThenInclude(h => h.IdPersonNavigation)
                            .ThenInclude(y => y.IdPersonNavigation)
                            .Skip((page - 1) * pagesize)
                            .Take(pagesize)
                            .ToList();

            var model = new HarvestViewModel
            {
                Harvests = harvests,

                PagingInfo = pagingInfo
            };

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var harvest = new HarvestViewModel
            {

            };
            await PrepareDropDownListsSale();
            await PrepareDropDownListsPlant();
            return View(harvest);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(HarvestViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {

                    Harvest h = new Harvest();
                    h.IdHarvest = model.IdHarvest;
                    h.NameHarvest = model.NameHarvest;
                    h.UseHarvest = model.UseHarvest;
                    h.CostHarvest = model.CostHarvest;
                    h.StartHarvest = model.StartHarvest;
                    h.EndHarvest = model.EndHarvest;
                    h.CostHarvest = model.CostHarvest;
                    h.IdPlant = model.IdPlant;
                    h.IdSale = model.IdSale;
                    foreach (var workerFromModel in model.Workers)
                    {
                        Worker worker = new Worker();
                        worker.IdHarvest = workerFromModel.IdHarvest;
                        worker.IdPerson = workerFromModel.IdPerson;
                        worker.Salary = workerFromModel.Salary;
                        worker.Time = workerFromModel.Time;
                        worker.IdPersonNavigation.FirstName = workerFromModel.FirstName;
                        worker.IdPersonNavigation.LastName = workerFromModel.LastName;
                        worker.IdPersonNavigation.Address = workerFromModel.Adresse;
                        worker.IdPersonNavigation.Mail = workerFromModel.Mail;
                        worker.IdPersonNavigation.Number = workerFromModel.Number;
                    }
                    ctx.Add(h);
                    await ctx.SaveChangesAsync();
                    TempData[Constants.Message] =
                    $"Harvest of {h.IdPlant} has been added.";
                    TempData[Constants.ErrorOccurred] = false;
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception exc)
                {
                    ModelState.AddModelError(string.Empty,
                    exc.CompleteExceptionMessage());
                    await PrepareDropDownListsPlant();
                    await PrepareDropDownListsSale();
                    return View(model);
                }
            }
            else
            {
                await PrepareDropDownListsPlant();
                await PrepareDropDownListsSale();
                return View(model);
            }
        }




        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id, int page = 1, int sort = 1, bool ascending = true)
        {
            var harvest = await ctx.Harvests
                                .Where(d => d.IdHarvest == id)
                                .SingleOrDefaultAsync();

            if (harvest != null)
            {
                try
                {

                    ctx.Remove(harvest);
                    await ctx.SaveChangesAsync();
                    string message = $"Harvest {harvest.IdPlant} deleted";
                    TempData[Constants.Message] = $"Harvest {harvest.IdHarvest} deleted.";
                    TempData[Constants.ErrorOccurred] = false;
                }
                catch (Exception exc)
                {
                    TempData[Constants.Message] = $"Error deleting harvest {harvest.IdHarvest} {exc.CompleteExceptionMessage()}";
                    TempData[Constants.ErrorOccurred] = true;
                }
            }
            else
            {
                TempData[Constants.Message] = "Invalid harvest id: " + id;
                TempData[Constants.ErrorOccurred] = true;
            }
            return RedirectToAction(nameof(Index), new { page, sort, ascending });
        }

        [HttpGet]
        public Task<IActionResult> Edit(int id, int page = 1, int sort = 1, bool ascending = true)
        {
            return Detail(id, page, sort, ascending, viewName: nameof(Edit));
        }

        /* [HttpPost]
         [ValidateAntiForgeryToken]
         public async Task<IActionResult> Update(HarvestViewModel model,int id, int page = 1, int sort = 1, bool ascending = true)
         {
             //for different approaches (attach, update, only id) see
             //https://docs.microsoft.com/en-us/aspnet/core/data/ef-mvc/crud#update-the-edit-page

             try
             {
                  Harvest harvest= await ctx.Harvests
                                   .Where(d => d.IdHarvest == id)
                                   .Include(d=>d.Workers)
                                   .FirstOrDefaultAsync();
                 if (harvest == null)
                 {
                     return NotFound("Invalid harvest id: " + id);
                 }

                 if (await TryUpdateModelAsync<Harvest>(harvest, "",
                     d => d.StartHarvest,
                     d => d.IdPlant,
                     d => d.EndHarvest,
                     d => d.UseHarvest,
                     d => d.QuantityHarvest,
                     d => d.CostHarvest,
                     d => d.IdSale,
                     d => d.NameHarvest



                 ))
                 {
                     model.Workers??= new List<WorkerViewModel>();

                     //// Ensure plot.Plants is not null
                     harvest.Workers ??= new List<Worker>();
                     List<int> workerid = model.Workers
                                     .Where(s => s.IdPerson > 0)
                                     .Select(s => s.IdPerson)
                                     .ToList();
                     //remove all not anymore in the model
                     //ctx.RemoveRange(plot.Plants.Where(i => !plantsId.Contains(i.IdPlant)));
                     foreach (var worker in model.Workers)
                     {
                         //update current ones and add new
                         Worker newOrUpdatedworker;
                         if (worker.IdPerson > 0)
                         {
                             newOrUpdatedworker = harvest.Workers.First(s => s.IdPerson == worker.IdPerson);
                         }
                         else
                         {
                             newOrUpdatedworker = new();
                             harvest.Workers.Add(newOrUpdatedworker);
                         }
                         newOrUpdatedworker.IdPersonNavigation.FirstName = worker.FirstName;
                         newOrUpdatedworker.IdPersonNavigation.LastName = worker.LastName;
                         newOrUpdatedworker.IdPersonNavigation.Mail = worker.Mail;
                         newOrUpdatedworker.IdPersonNavigation.Number = worker.Number;
                         newOrUpdatedworker.IdPersonNavigation.Address = worker.Adresse;
                         newOrUpdatedworker.Salary = worker.Salary;
                         newOrUpdatedworker.Time = worker.Time;

                     }
                     ViewBag.Page = page;
                     ViewBag.Sort = sort;
                     ViewBag.Ascending = ascending;
                     try
                     {
                         await ctx.SaveChangesAsync();
                         TempData[Constants.Message] = "Harvest updated.";
                         TempData[Constants.ErrorOccurred] = false;
                         return RedirectToAction(nameof(Index), new { page, sort, ascending,id = harvest.IdHarvest });
                     }
                     catch (Exception exc)
                     {
                         ModelState.AddModelError(string.Empty, exc.CompleteExceptionMessage());

                         return View(model);
                     }
                 }
                 else
                 {
                     ModelState.AddModelError(string.Empty, "Cannot update model");
                     return View(model);
                 }
             }
             catch (Exception exc)
             {
                 TempData[Constants.Message] = exc.CompleteExceptionMessage();
                 TempData[Constants.ErrorOccurred] = true;
                 return RedirectToAction(nameof(Edit), id);
             }
         }*/

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(HarvestViewModel model,int id, int page = 1, int sort = 1, bool ascending = true)
        {
            ViewBag.Page = page;
            ViewBag.Sort = sort;
            ViewBag.Ascending = ascending;

            if (ModelState.IsValid)
            {
                var harvest = await ctx.Harvests
                                        .Include(d => d.Workers)
                                        .ThenInclude(w => w.IdPersonNavigation)
                                        .Where(d => d.IdHarvest == id)
                                        .FirstOrDefaultAsync();
                if (harvest == null)
                {
                    return NotFound("There is no harvest with id: " + id);
                }



                harvest.UseHarvest = model.UseHarvest;
                harvest.StartHarvest = model.StartHarvest;
                harvest.EndHarvest = model.EndHarvest;
                harvest.CostHarvest = model.CostHarvest;
                harvest.QuantityHarvest = model.QuantityHarvest;
                harvest.IdPlant = model.IdPlant;
                harvest.NameHarvest = model.NameHarvest;
                harvest.IdHarvest = model.IdHarvest;
                harvest.IdSale = model.IdSale;

                List<int> workerid = model.Workers
                                           .Where(d=> d.IdPerson >0)
                                          .Select(s => s.IdPerson)
                                          .ToList();
                //remove all not anymore in the model
                

               ctx.RemoveRange(harvest.Workers.Where(i => !workerid.Contains(i.IdPerson)));
               int count  = 0;
                foreach (var worker in model.Workers)
                {
                    count++;
                    //update current ones and add new
                    Worker newOrUpdatedWorker;
                    if (worker.IdPerson > 0)
                    {
                        count++;
                        newOrUpdatedWorker = harvest.Workers.First(s => s.IdPerson == worker.IdPerson);
                    }
                    else
                    {
                        newOrUpdatedWorker = new();
                        harvest.Workers.Add(newOrUpdatedWorker);
                    }
                    newOrUpdatedWorker.Salary = worker.Salary;
                    newOrUpdatedWorker.Time = worker.Time;
                    newOrUpdatedWorker.IdPersonNavigation.Mail = worker.Mail;
                    newOrUpdatedWorker.IdPersonNavigation.Number = worker.Number;
                    newOrUpdatedWorker.IdPersonNavigation.Address = worker.Adresse;
                    newOrUpdatedWorker.IdPerson = worker.IdPerson;

                }


                try
                {
                    await ctx.SaveChangesAsync();

                    TempData[Constants.Message] = $"Harvest {harvest.IdHarvest} updated. count : {count}";
                    TempData[Constants.ErrorOccurred] = false;
                    return RedirectToAction(nameof(Detail), new
                    {
                        id = harvest.IdHarvest,

                        page,
                        sort,
                        ascending
                    });

                }
                catch (Exception exc)
                {
                    ModelState.AddModelError(string.Empty, exc.CompleteExceptionMessage());
                    return View(model);
                }
            }
            else
            {
                return View(model);
            }
        }
        public async Task<IActionResult> Detail(int id, int page = 1, int sort = 1, bool ascending = true, string viewName = nameof(Detail))
        {
            var haverst = await ctx.Harvests.Where(m => m.IdHarvest == id)
            .Select(m => new HarvestViewModel
            {

                IdHarvest = m.IdHarvest,
                NameHarvest = m.NameHarvest,
                StartHarvest = m.StartHarvest,
                EndHarvest = m.EndHarvest,
                CostHarvest = m.CostHarvest,
                UseHarvest = m.UseHarvest,
                QuantityHarvest = m.QuantityHarvest,
                IdPlant = m.IdPlant,
                IdSale = m.IdSale,

            }).FirstOrDefaultAsync();



            if (haverst == null)
            {
                return NotFound($"harvest {id} does not exist.");
            }
            else
            {
                var worker = await ctx.Workers
                    .Where(s => s.IdHarvest == haverst.IdHarvest)
                    .OrderBy(s => s.IdPerson)
                    .Select(v => new WorkerViewModel
                    {
                        IdPerson = v.IdPerson,
                        FirstName = v.IdPersonNavigation.FirstName,
                        LastName = v.IdPersonNavigation.LastName,
                        Mail = v.IdPersonNavigation.Mail,
                        Number = v.IdPersonNavigation.Number,
                        Adresse = v.IdPersonNavigation.Address,
                        Salary = v.Salary,
                        Time = v.Time
                    }).ToListAsync();
                haverst.Workers = worker;


                ViewBag.Page = page;
                ViewBag.Sort = sort;
                ViewBag.Ascending = ascending;
                await PrepareDropDownListsSale();
                await PrepareDropDownListsPlant();

                return View(viewName, haverst);

            }
        }
        private async Task PrepareDropDownListsPlant()
        {
            var plant = await ctx.Plants.OrderBy(d => d.Species)
            .Select(d => new { d.IdPlant, d.Species })
            .ToListAsync();
            ViewBag.Plant = new SelectList(plant,
            "IdPlant", "Species");
        }
        private async Task PrepareDropDownListsSale()
        {
            var sales = await ctx.Sales
         .Include(s => s.IdPersonNavigation.IdPersonNavigation)
         .OrderBy(d => d.IdSale)
         .Select(d => new
         {
             d.IdSale,
             FullName = $"{d.IdPersonNavigation.IdPersonNavigation.LastName} {d.IdPersonNavigation.IdPersonNavigation.FirstName}"
         })
         .ToListAsync();

            ViewBag.Sale = new SelectList(sales, "IdSale", "FullName", "FullName");
        }


    }
}
