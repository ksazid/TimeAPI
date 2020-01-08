using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Diagnostics;
using TimeAPI.API.Filters;
using TimeAPI.API.Models;

namespace TimeAPI.API.Controllers
{
    //[ApiKeyAuth]
    //[EnableCors("CorsPolicy")]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {

          //var s4 =  UserHelpers.GoogleUrlShortener("AIzaSyDqzocx5sEPe83MAH_dC6NZqa0aR17RdsE", "https://www.azurefromthetrenches.com/build-elegant-rest-apis-with-azure-functions/");
          //  ////http://localhost:7071/api/SendSMS


            //ReverseGeoLookup.ReverseGeoLoc("25.1899157", "55.2634592", out string Address_ShortName,
            //                                                                      out string Address_country,
            //                                                                      out string Address_administrative_area_level_1,
            //                                                                      out string Address_administrative_area_level_2,
            //                                                                      out string Address_administrative_area_level_3,
            //                                                                      out string Address_colloquial_area,
            //                                                                      out string Address_locality,
            //                                                                      out string Address_sublocality,
            //                                                                      out string Address_neighborhood);
            _logger = logger;
        }


        [HttpGet]
        public ActionResult<IEnumerable<string>> Get()
        {
            return new string[] { "value1", "Test Data" };
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
