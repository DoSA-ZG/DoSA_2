using System.Numerics;
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

namespace RPPP_WebApp.Controllers
{
    public class RequestController : Controller
    {
        private readonly RPPP13Context ctx;
        private readonly ILogger<RequestController> logger;
        private readonly AppSettings appData;
        public RequestController(RPPP13Context ctx, IOptionsSnapshot<AppSettings>options, ILogger<RequestController> logger)
        {
            this.ctx = ctx;
            appData = options.Value;
            this.logger = logger;
        }
        /*public IActionResult Index()
        {
            var request = ctx.Requests
            .AsNoTracking()
            .OrderBy(d => d.CountryName)
            .ToList();
            return View("IndexSimple", request);
        }*/
        public IActionResult Index(int page = 1, int sort = 1, bool ascending = true)
        {
            int pagesize = appData.PageSize;
            var query = ctx.Requests
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

            query = query.ApplySort(sort,ascending);

            var requests = query
                            //.Include(a => a.KnownCustomers)
                            .Skip((page - 1) * pagesize)
                            .Take(pagesize)
                            .ToList();

            foreach (var request in requests)
            {
                 //request.KnownCustomers = ctx.KnownCustomers.Where(p => p.IdPerson == request.IdPerson).ToList();
            }

            var model = new RequestViewModel
            {
                Requests = requests,
                PagingInfo = pagingInfo
            };

            return View(model);
        }

        public async Task<IActionResult> Create(Request request)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var person = ctx.KnownCustomers.Find(request.IdPerson);
                    person.Requests.Add(request);
                    ctx.Add(request);
                    ctx.SaveChanges();
                    TempData[Constants.Message] =
                    $"Request {request.SpeciesAsked} has been added.";
                    TempData[Constants.ErrorOccurred] = false;
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception exc)
                {
                    ModelState.AddModelError(string.Empty,
                    exc.CompleteExceptionMessage());
                    await PrepareDropDownListsPerson();
                    return View(request);
                }
            }
            else
            {
                await PrepareDropDownListsPerson();
                return View(request);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            await PrepareDropDownListsPerson();
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]

        public IActionResult Delete(int IdRequest, int page = 1, int sort = 1, bool ascending = true)
        {
        var request = ctx.Requests.Find(IdRequest);
             if (request != null)
               {
                  try
                  {
                        string RequestSpecies = request.SpeciesAsked;
                        ctx.Remove(request);
                        ctx.SaveChanges();
                        string message = $"Request {RequestSpecies} deleted";
                        logger.LogInformation(message);
                        TempData[Constants.Message] = message;
                        TempData[Constants.ErrorOccurred] = false;
                  }
                    catch (Exception exc)
                  {
                        string message = "Error deleting the request " + exc.CompleteExceptionMessage();
                        TempData[Constants.Message] = message;
                        TempData[Constants.ErrorOccurred] = true;
                        logger.LogError(message);
                  }
               }
               else
               {
                    string message = $"There is no request with code {IdRequest}";
                    logger.LogWarning(message);
                    TempData[Constants.Message] = message;
                    TempData[Constants.ErrorOccurred] = true;
               }
                return RedirectToAction(nameof(Index), new { page, sort, ascending });
        }

        /*[HttpGet]
        public IActionResult Edit(int id, int page = 1, int sort = 1, bool ascending = true)
        {
            var request = ctx.Requests.AsNoTracking().Where(d => d.IdRequest == id).SingleOrDefault();
            if (request == null)
            {
                string message = $"There is no request with id {id}";
                logger.LogWarning(message);
                return NotFound(message);
            }
            else
            {
                ViewBag.Page = page;
                ViewBag.Sort = sort;
                ViewBag.Ascending = ascending;
                return View(request);
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
                Request request = await ctx.Requests
                                  .Where(d => d.IdRequest == id)
                                  .FirstOrDefaultAsync();
                if (request == null)
                {
                    return NotFound("Invalid request id: " + id);
                }

                if (await TryUpdateModelAsync<Request>(request, "",
                    d => d.SpeciesAsked,
                    d => d.DateRequest,
                    d => d.QuantityAsked,
                    d => d.PriceAsked,
                    d => d.IdPerson,
                    d => d.StatusRequest

                ))
                {
                    ViewBag.Page = page;
                    ViewBag.Sort = sort;
                    ViewBag.Ascending = ascending;
                    try
                    {
                        await ctx.SaveChangesAsync();
                        TempData[Constants.Message] = "Request " +id +"  updated.";
                        TempData[Constants.ErrorOccurred] = false;
                        return RedirectToAction(nameof(Index), new { page, sort, ascending });
                    }
                    catch (Exception exc)
                    {
                        ModelState.AddModelError(string.Empty, exc.CompleteExceptionMessage());
                        return View(request);
                    }
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Cannot update model");
                    return View(request);
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
            var request = await ctx.Requests.Where(m => m.IdRequest == id)

           .Select(m => new RequestViewModel
           {
               IdRequest = m.IdRequest,
               SpeciesAsked = m.SpeciesAsked,
               DateRequest = m.DateRequest,
               QuantityAsked = m.QuantityAsked,
               PriceAsked = m.PriceAsked,
               IdPerson = (int)m.IdPerson,
               StatusRequest = m.StatusRequest,
               //KnownCustomers = m.KnownCustomers.Select(v => new KnownCustomerViewModel
               //{
              //     IdPerson = v.IdPerson,
               //}).ToList()
           })
           .SingleOrDefaultAsync();
            if (request != null)
            {
                return View(request);
            }
            else
            {
                return NotFound($"Invalid request id: {id}");
            }

        }*/

        public async Task<IActionResult> Details(int id, int page = 1, int sort = 1, bool ascending = true, string viewName = nameof(Details))
        {
            var request = await ctx.Requests
                                    .Where(d => d.IdRequest == id)
                                    .Select(d => new RequestViewModel
                                    {
                                        IdRequest = d.IdRequest,
                                        SpeciesAsked = d.SpeciesAsked,
                                        DateRequest = d.DateRequest,
                                        QuantityAsked = d.QuantityAsked,
                                        PriceAsked = d.PriceAsked,
                                        IdPerson = d.IdPerson,
                                        StatusRequest = d.StatusRequest,
                                    })
                                    .FirstOrDefaultAsync();
            if (request == null)
            {
                return NotFound($"Request {id} does not exist.");
            }
            else
            {

                //loading items
                var person = await ctx.People
                                     .Where(s => s.IdPerson == request.IdPerson)
                                     .OrderBy(s => s.IdPerson)
                                     .Select(m => new KnownCustomerViewModel
                                     {
                                         IdPerson = m.IdPerson,
                                         FirstName = m.FirstName,
                                         LastName = m.LastName,
                                         Number = m.Number,
                                         Mail = m.Mail,
                                         Adresse = m.Address,
                                     })
                                     .ToListAsync();
                request.KnownCustomers = person;



                ViewBag.Page = page;
                ViewBag.Sort = sort;
                ViewBag.Ascending = ascending;
                await PrepareDropDownListsPerson();

                return View(viewName, request);
            }
        }

        [HttpGet]
        public Task<IActionResult> Edit(int id, int page = 1, int sort = 1, bool ascending = true)
        {
            return Details(id, page, sort, ascending, viewName: nameof(Edit));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(RequestViewModel model, int id, int page = 1, int sort = 1, bool ascending = true)
        {
            //for different approaches (attach, update, only id) see
            //https://docs.microsoft.com/en-us/aspnet/core/data/ef-mvc/crud#update-the-edit-page

            try
            {
                var request = await ctx.Requests
                                  //.Include(d => d.KnownCustomers)
                                  .Where(d => d.IdPerson == id)
                                  .FirstOrDefaultAsync();
                if (request == null)
                {
                    return NotFound("Invalid request id: " + id);
                }


                request.SpeciesAsked = model.SpeciesAsked;
                request.DateRequest = model.DateRequest;
                request.QuantityAsked = model.QuantityAsked;
                request.PriceAsked = model.PriceAsked;
                request.IdPerson = model.IdPerson;

                List<int> knownCustomersId = model.KnownCustomers
                                .Where(s => s.IdPerson > 0)
                                .Select(s => s.IdPerson)
                                .ToList();

                //remove all not anymore in the model
                //ctx.RemoveRange(request.KnownCustomers.Where(i => !knownCustomersId.Contains(i.IdPerson)));


                foreach (var person in model.KnownCustomers)
                {
                    KnownCustomer newOrUpdatedperson;
                    if (person.IdPerson > 0)
                    {
                        //newOrUpdatedperson = request.KnownCustomers.First(s => s.IdPerson == person.IdPerson);
                    }
                    else
                    {
                        newOrUpdatedperson = new();
                       // request.KnownCustomers.Add(newOrUpdatedperson);
                    }
                    //newOrUpdatedperson.IdPerson = person.IdPerson;
                }

                ViewBag.Page = page;
                ViewBag.Sort = sort;
                ViewBag.Ascending = ascending;
                try
                {
                    await ctx.SaveChangesAsync();
                    TempData[Constants.Message] = $"Request {request.IdRequest} updated.";
                    TempData[Constants.ErrorOccurred] = false;
                    return RedirectToAction(nameof(Details), new { page, sort, ascending, id = request.IdRequest });
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

