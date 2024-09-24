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
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace RPPP_WebApp.Controllers
{
    public class WorkerController : Controller
    {
        private readonly RPPP13Context ctx;
        private readonly ILogger<WorkerController> logger;
        private readonly AppSettings appData;
        public WorkerController(RPPP13Context ctx, IOptionsSnapshot<AppSettings> options, ILogger<WorkerController> logger)
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
        public IActionResult Index(int page = 1, int sort = 1, bool ascending = true)
        {
            int pagesize = appData.PageSize;

            var query = ctx.Workers
                            
                           .AsNoTracking();

            int count = query.Count();
            if (count == 0)
            {
                string message = "There is no Worker in the database";
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

            var workers = query
                             .Include(w => w.IdPersonNavigation)
                            .Skip((page - 1) * pagesize)
                            .Take(pagesize)
                            .ToList();

            var model = new WorkerViewModel
            {
                Workers = workers,
                PagingInfo = pagingInfo
            };

            return View(model);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        public IActionResult Create(Worker worker)
        {
            if (ModelState.IsValid)
            {
                try
                {
                   /* var harvest = ctx.Harvests.Find(worker.IdHarvest);
                    harvest.Workers.Add(worker);*/

                   

                    ctx.Add(worker);
                    ctx.SaveChanges();
                    TempData[Constants.Message] =
                    $"Harvest of {worker.IdPerson} has been added.";
                    TempData[Constants.ErrorOccurred] = false;
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception exc)
                {
                    ModelState.AddModelError(string.Empty,
                    exc.CompleteExceptionMessage());
                    return View(worker);
                }
            }
            else
            {
                return View(worker);
            }
        }




        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int IdPerson, int page = 1, int sort = 1, bool ascending = true)
        {
            var worker = ctx.Workers.Find(IdPerson);
            if (worker!= null)
            {
                try
                {
                    int jsp = worker.IdPerson;
                    ctx.Remove(worker);
                    ctx.SaveChanges();
                    string message = $"Worker {worker.IdPerson} deleted";
                    logger.LogInformation(message);
                    TempData[Constants.Message] = message;
                    TempData[Constants.ErrorOccurred] = false;
                }
                catch (Exception exc)
                {
                    string message = "Eror deleting the worker " + exc.CompleteExceptionMessage();
                    TempData[Constants.Message] = message;
                    TempData[Constants.ErrorOccurred] = true;
                    logger.LogError(message);
                }
            }
            else
            {
                string message = $"There is no worker with code {IdPerson}";
                logger.LogWarning(message);
                TempData[Constants.Message] = message;
                TempData[Constants.ErrorOccurred] = true;
            }
            return RedirectToAction(nameof(Index), new { page, sort, ascending });
        }


        [HttpGet]
        public IActionResult Edit(int id, int page = 1, int sort = 1, bool ascending = true)
        {
            var worker = ctx.Workers
                            .AsNoTracking()
                            .Where(p => p.IdPerson == id)
                            .Select(p => new WorkerViewModel
                            {
                                IdPerson = p.IdPerson,
                                FirstName = p.IdPersonNavigation.FirstName,
                                LastName = p.IdPersonNavigation.LastName,
                                Number = p.IdPersonNavigation.Number,
                                Mail = p.IdPersonNavigation.Mail,
                                Adresse = p.IdPersonNavigation.Address,
                                IdHarvest = p.IdHarvest,
                                Time = p.Time,
                                Salary = p.Salary
                            })
                            .SingleOrDefault();
            if (worker == null)
            {
                string message = $"There is no worker with id {id}";
                logger.LogWarning(message);
                return NotFound(message);
            }
            else
            {
                ViewBag.Page = page;
                ViewBag.Sort = sort;
                ViewBag.Ascending = ascending;
                return View(worker);
            }
        }

        [HttpPost, ActionName("Edit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(WorkerViewModel updatedWorker, int page = 1, int sort = 1, bool ascending = true)
        {
            //for different approaches (attach, update, only id) see
            //https://docs.microsoft.com/en-us/aspnet/core/data/ef-mvc/crud#update-the-edit-page

            try
            {
                if (ModelState.IsValid)
                {
                    var worker = await ctx.Workers.Where(d => d.IdPerson == updatedWorker.IdPerson).Include(d => d.IdPersonNavigation).FirstOrDefaultAsync();
                    if (worker == null)
                    {
                        return NotFound("Invalid worker id: " + updatedWorker.IdPerson);
                    }


                    worker.IdPerson = updatedWorker.IdPerson;
                    worker.IdPersonNavigation.FirstName = updatedWorker.FirstName;
                    worker.IdPersonNavigation.LastName = updatedWorker.LastName;
                    worker.IdPersonNavigation.Number = updatedWorker.Number;
                    worker.IdPersonNavigation.Mail = updatedWorker.Mail;
                    worker.IdPersonNavigation.Address = updatedWorker.Adresse;
                    worker.Time = updatedWorker.Time;
                    worker.Salary = updatedWorker.Salary;
                    worker.IdHarvest= updatedWorker.IdHarvest;


                    ViewBag.Page = page;
                    ViewBag.Sort = sort;
                    ViewBag.Ascending = ascending;
                    try
                    {
                        await ctx.SaveChangesAsync();
                        TempData[Constants.Message] = "Worker updated.";
                        TempData[Constants.ErrorOccurred] = false;
                        return RedirectToAction(nameof(Index), new { page, sort, ascending });
                    }
                    catch (Exception exc)
                    {
                        ModelState.AddModelError(string.Empty, exc.CompleteExceptionMessage());
                        return View(updatedWorker);
                    }
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Cannot update model");
                    return View(updatedWorker);
                }
            }
            catch (Exception exc)
            {
                TempData[Constants.Message] = exc.CompleteExceptionMessage();
                TempData[Constants.ErrorOccurred] = true;
                return RedirectToAction(nameof(Edit), new { id = updatedWorker.IdPerson });
            }
        }
        
        public async Task<IActionResult> Detail(int id)
        {
            var worker = await ctx.Workers
                            .Where(p => p.IdPerson == id)
                            .Select(p => new WorkerViewModel
                            {
                                IdPerson = p.IdPerson,
                                FirstName = p.IdPersonNavigation.FirstName,
                                LastName = p.IdPersonNavigation.LastName,
                                Number = p.IdPersonNavigation.Number,
                                Mail = p.IdPersonNavigation.Mail,
                                Adresse = p.IdPersonNavigation.Address,
                                IdHarvest = p.IdHarvest,
                                Time = p.Time,
                                Salary = p.Salary
                            })
                            .FirstOrDefaultAsync();
            if (worker != null)
            {
                return View(worker);
            }
            else
            {
                return NotFound($"Invalid worker id: {id}");
            }

        }

    }
}
