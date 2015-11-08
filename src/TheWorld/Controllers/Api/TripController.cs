using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc;
using TheWorld.Models;

// For more information on enabling Web API for empty projects,
// visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace TheWorld.Controllers.Api
{
    public class TripController : Controller
    {
        private readonly IWorldRepository _repository;

        public TripController(IWorldRepository repository)
        {
            _repository = repository;
        }

        [HttpGet("api/trips")]
        public JsonResult Get()
        {
            var results = _repository.GetAllTripsWithStops();
            //return Json(new {name = "Andre"});
            return Json(results);
        }
    }
}
