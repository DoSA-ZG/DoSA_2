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
using OfficeOpenXml.FormulaParsing.Excel.Functions.Math;

namespace RPPP_WebApp.Controllers
{
    public class KnownCustomerController : Controller
    {
        private readonly RPPP13Context ctx;
        private readonly ILogger<KnownCustomerController> logger;
        private readonly AppSettings appData;
        public KnownCustomerController(RPPP13Context ctx, IOptionsSnapshot<AppSettings> options, ILogger<KnownCustomerController> logger)
        {
            this.ctx = ctx;
            this.logger = logger;
            appData = options.Value;
        }
        public IActionResult Index(int page = 1, int sort = 1, bool ascending = true)
        {
            int pagesize = appData.PageSize;

            var query = ctx.KnownCustomers
                            .Include(w => w.Sales)
                            .Include(l => l.Leases)
                            .Include(p => p.Requests)
                           .AsNoTracking();

            int count = query.Count();
            if (count == 0)
            {
                string message = "There is Known Customer in the database";
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

            var knownCustomers = query
                            .Include(w => w.Sales)
                            .Include(l => l.Leases)
                            .Include(p => p.Requests)
                            .Include(w => w.IdPersonNavigation)
                            .Skip((page - 1) * pagesize)
                            .Take(pagesize)
                            .ToList();

            var model = new KnownCustomerViewModel
            {
                KnownCustomers = knownCustomers,

                PagingInfo = pagingInfo
            };

            return View(model);
        }
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        public IActionResult Create(KnownCustomer knownCustomer)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    ctx.Add(knownCustomer);
                    ctx.SaveChanges();
                    TempData[Constants.Message] =
                    $"Harvest of {knownCustomer.IdPersonNavigation.LastName} has been added.";
                    TempData[Constants.ErrorOccurred] = false;
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception exc)
                {
                    ModelState.AddModelError(string.Empty,
                    exc.CompleteExceptionMessage());
                    return View(knownCustomer);
                }
            }
            else
            {
                return View(knownCustomer);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int IdPerson, int page = 1, int sort = 1, bool ascending = true)
        {
            var knownCustomer = ctx.KnownCustomers.Find(IdPerson);
            if (knownCustomer != null)
            {
                try
                {
                    int jsp = knownCustomer.IdPerson;
                    ctx.Remove(knownCustomer);
                    ctx.SaveChanges();
                    string message = $"Known Customer  {knownCustomer.IdPerson} deleted";
                    logger.LogInformation(message);
                    TempData[Constants.Message] = message;
                    TempData[Constants.ErrorOccurred] = false;
                }
                catch (Exception exc)
                {
                    string message = "Eror deleting the harvest " + exc.CompleteExceptionMessage();
                    TempData[Constants.Message] = message;
                    TempData[Constants.ErrorOccurred] = true;
                    logger.LogError(message);
                }
            }
            else
            {
                string message = $"There is no Known Customer with code {IdPerson}";
                logger.LogWarning(message);
                TempData[Constants.Message] = message;
                TempData[Constants.ErrorOccurred] = true;
            }
            return RedirectToAction(nameof(Index), new { page, sort, ascending });
        }
        [HttpGet]
        public  Task<IActionResult> Edit(int id, int page = 1, int sort = 1, bool ascending = true)
        {
           /* var knownCustomer = await ctx.KnownCustomers.Where(d => d.IdPerson == id)
                                                     .Select(d => new KnownCustomerViewModel
                                                     {
                                                         IdPerson = d.IdPerson,
                                                         FirstName = d.IdPersonNavigation.FirstName,
                                                         LastName = d.IdPersonNavigation.LastName,
                                                         Number = d.IdPersonNavigation.Number,
                                                         Mail = d.IdPersonNavigation.Mail,
                                                         Adresse = d.IdPersonNavigation.Address

                                                     }).FirstOrDefaultAsync();
            if (knownCustomer == null)
            {
                string message = $"There is no known customer with id {id}";
                logger.LogWarning(message);
                return NotFound(message);
            }
            else
            {
                ViewBag.Page = page;
                ViewBag.Sort = sort;
                ViewBag.Ascending = ascending;

                return View(knownCustomer);
            }*/
           return Detail(id, page, sort, ascending, viewName: nameof(Edit));
        }

        /* [HttpPost]
         [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(KnownCustomerViewModel updatedCustomer, int id, int page = 1, int sort = 1, bool ascending = true)
         {
             //for different approaches (attach, update, only id) see
             //https://docs.microsoft.com/en-us/aspnet/core/data/ef-mvc/crud#update-the-edit-page

             try
             {
                 if (ModelState.IsValid)
                 {
                     var knownCustomer = await ctx.KnownCustomers.Where(d => d.IdPerson == updatedCustomer.IdPerson).Include(d => d.IdPersonNavigation).FirstOrDefaultAsync();
                     if (knownCustomer == null)
                     {
                         return NotFound("Invalid known Customer id: " + updatedCustomer.IdPerson);
                     }


                     knownCustomer.IdPerson = updatedCustomer.IdPerson;
                     knownCustomer.IdPersonNavigation.FirstName = updatedCustomer.FirstName;
                     knownCustomer.IdPersonNavigation.LastName = updatedCustomer.LastName;
                     knownCustomer.IdPersonNavigation.Number = updatedCustomer.Number;
                     knownCustomer.IdPersonNavigation.Mail = updatedCustomer.Mail;
                     knownCustomer.IdPersonNavigation.Address = updatedCustomer.Adresse;









                     ViewBag.Page = page;
                     ViewBag.Sort = sort;
                     ViewBag.Ascending = ascending;
                     try
                     {
                         await ctx.SaveChangesAsync();
                         TempData[Constants.Message] = "Known Customer updated.";
                         TempData[Constants.ErrorOccurred] = false;
                         return RedirectToAction(nameof(Index), new { page, sort, ascending });
                     }
                     catch (Exception exc)
                     {
                         ModelState.AddModelError(string.Empty, exc.CompleteExceptionMessage());
                         return View(updatedCustomer);
                     }
                 }
                 else
                 {
                     ModelState.AddModelError(string.Empty, "Cannot update model");
                     return View(updatedCustomer);
                 }
             }
             catch (Exception exc)
             {
                 TempData[Constants.Message] = exc.CompleteExceptionMessage();
                 TempData[Constants.ErrorOccurred] = true;
                 return RedirectToAction(nameof(Edit), new { id = updatedCustomer.IdPerson });
             }
         }
         */
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(KnownCustomerViewModel model, int id, int page = 1, int sort = 1, bool ascending = true)
        {
            ViewBag.Page = page;
            ViewBag.Sort = sort;
            ViewBag.Ascending = ascending;

            if (ModelState.IsValid)
            {
                var knowncustomer = await ctx.KnownCustomers
                                            .Include(d=>d.Requests)
                                         .Include(d => d.IdPersonNavigation)
                                        .Where(d => d.IdPerson == id)
                                        .FirstOrDefaultAsync();
                if (knowncustomer == null)
                {
                    return NotFound("There is no harvest with id: " + id);
                }

                knowncustomer.IdPerson = model.IdPerson;
                knowncustomer.IdPersonNavigation.FirstName = model.FirstName;
                knowncustomer.IdPersonNavigation.LastName = model.LastName;
                knowncustomer.IdPersonNavigation.Number = model.Number;
                knowncustomer.IdPersonNavigation.Mail = model.Mail;
                knowncustomer.IdPersonNavigation.Address = model.Adresse;

                List<int> requestid = model.Requests
                                           .Where(d => d.IdRequest > 0)
                                          .Select(s => s.IdRequest)
                                          .ToList();
                //remove all not anymore in the model


                ctx.RemoveRange(knowncustomer.Requests.Where(i => !requestid.Contains(i.IdRequest)));
                int count = 0;
                foreach (var request in model.Requests)
                {
                    count++;
                    //update current ones and add new
                    Request newOrUpdatedRequest;
                    if (request.IdRequest > 0)
                    {
                        count++;
                        newOrUpdatedRequest = knowncustomer.Requests.First(s => s.IdRequest == request.IdRequest);
                    }
                    else
                    {
                        newOrUpdatedRequest = new();
                        knowncustomer.Requests.Add(newOrUpdatedRequest);
                    }
                    newOrUpdatedRequest.StatusRequest = request.StatusRequest;
                    newOrUpdatedRequest.SpeciesAsked = request.SpeciesAsked;
                    newOrUpdatedRequest.PriceAsked = request.PriceAsked;
                    newOrUpdatedRequest.QuantityAsked = request.QuantityAsked;
                    newOrUpdatedRequest.DateRequest = request.DateRequest;
                    newOrUpdatedRequest.IdPerson = request.IdPerson;
                    newOrUpdatedRequest.IdRequest = request.IdRequest;

                }


                try
                {
                    await ctx.SaveChangesAsync();

                    TempData[Constants.Message] = $"Known Customer {knowncustomer.IdPerson} updated. count : {count}";
                    TempData[Constants.ErrorOccurred] = false;
                    return RedirectToAction(nameof(Detail), new
                    {
                        id = knowncustomer.IdPerson,

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


            var knowcustomer = await ctx.KnownCustomers.Where(m => m.IdPerson == id)


           .Select(m => new KnownCustomerViewModel
           {
               IdPerson = m.IdPerson,
               FirstName = m.IdPersonNavigation.FirstName,
               LastName = m.IdPersonNavigation.LastName,
               Number = m.IdPersonNavigation.Number,
               Adresse = m.IdPersonNavigation.Address,
               Mail = m.IdPersonNavigation.Mail,
           }).FirstOrDefaultAsync();
            if (knowcustomer == null)
            {
                return NotFound($"knowncustomer {id} does not exist.");
            }
            else
            {
                var request = await ctx.Requests
                    .Where(s => s.IdPerson == knowcustomer.IdPerson)
                    .OrderBy(s => s.IdPerson)
                    .Select(v => new RequestViewModel
                    {
                        IdRequest = (int)v.IdRequest,
                        SpeciesAsked = v.SpeciesAsked,
                        DateRequest = (DateTime)v.DateRequest,
                        QuantityAsked = v.QuantityAsked,
                        PriceAsked = v.PriceAsked,
                        IdPerson = (int)v.IdPerson,
                        StatusRequest = v.StatusRequest
                    }).ToListAsync();
                knowcustomer.Requests = request;
                ViewBag.Page = page;
                ViewBag.Sort = sort;
                ViewBag.Ascending = ascending;
                return View(viewName, knowcustomer);



            }
        }

    }
}
