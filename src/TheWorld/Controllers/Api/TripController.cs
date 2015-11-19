using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNet.Mvc;
using Microsoft.Extensions.Logging;
using TheWorld.Models;
using TheWorld.ViewModels;

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
        private readonly ILogger<TripController> _logger;

        public TripController(
            IWorldRepository repository,
            ILogger<TripController> logger)
        {
            _repository = repository;
            // later we can decide where the logger logs its messages
            // it can be a database or a blob storage or an output window
            // the later is the default
            _logger = logger;
        }

        [HttpGet("")]
        public JsonResult Get()
        {
            var results = Mapper.Map<IEnumerable<TripViewModel>>(_repository.GetAllTripsWithStops());
            //return Json(new {name = "Andre"});
            return Json(results);
        }

        [HttpPost("")]
        public JsonResult Post([FromBody]TripViewModel vm)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var newTrip = Mapper.Map<Trip>(vm);

                    // Save to database
                    _logger.LogInformation("Attempting to save a new trip");
                    // Add the newTrip to the repository
                    _repository.AddTrip(newTrip);
                    // check if success
                    if (_repository.SaveAll())
                    {
                        Response.StatusCode = (int)HttpStatusCode.Created;
                        // we use the same Mapper to map the newTrip back to its viewmodel representation
                        return Json(Mapper.Map<TripViewModel>(newTrip));
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to save the new trip", ex);
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return Json(new { ex.Message });
            }

            // temporarily we can pass the ModelState
            // also we could make it dependent on the environment
            return Json(new { Message = "Failed", ModelState });
        }
    }
}
