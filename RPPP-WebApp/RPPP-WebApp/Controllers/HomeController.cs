using Microsoft.AspNetCore.Mvc;

namespace RPPP_WebApp.Controllers
{
  public class HomeController : Controller
  {
    public IActionResult Index()
    {
      return View();
    }
    public IActionResult GetStarted()
    {
        return View();
    }
  }
}
