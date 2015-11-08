using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc;
using TheWorld.Models;

// For more information on enabling Web API for empty projects,
// visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace TheWorld.Controllers.Api
{
    /// <summary>
    /// This specifies the root route for our API methods
    /// All the individual methods have to extend the path
    /// </summary>
    [Route("api/trips")]
    public class TripController : Controller
    {
        private readonly IWorldRepository _repository;

        public TripController(IWorldRepository repository)
        {
            _repository = repository;
        }

        [HttpGet("")]
        public JsonResult Get()
        {
            var results = _repository.GetAllTripsWithStops();
            //return Json(new {name = "Andre"});
            return Json(results);
        }

        [HttpPost("")]
        public JsonResult Post([FromBody]Trip newTrip)
        {
            if (ModelState.IsValid)
            {
                Response.StatusCode = (int)HttpStatusCode.Created;
                return Json(true);
            }

            Response.StatusCode = (int)HttpStatusCode.BadRequest;
            return Json("Failed");
        }
    }
}
