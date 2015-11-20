using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNet.Mvc;
using Microsoft.Extensions.Logging;
using TheWorld.Models;
using TheWorld.Services;
using TheWorld.ViewModels;
using Microsoft.AspNet.Authorization;

namespace TheWorld.Controllers.Api
{
    [Authorize]
    [Route("api/trips/{tripName}/stops")]
    public class StopController : Controller
    {
        private readonly IWorldRepository _repository;
        private readonly ILogger<StopController> _logger;
        private readonly GeoService _geoService;

        public StopController(
            IWorldRepository repository,
            ILogger<StopController> logger,
            GeoService geoService)
        {
            _repository = repository;
            _logger = logger;
            _geoService = geoService;
        }

        [HttpGet("")]
        public JsonResult Get(string tripName)
        {
            try
            {
                // temporarily decoding via WebUtility because of a bug in Beta 8
                var decodedName = WebUtility.UrlDecode(tripName);

                // we include the user name just in case, because the trip itself can
                // accidentally have another user's name
                var results = _repository.GetTripByName(decodedName, User.Identity.Name);

                return Json(
                    results == null
                    ? null
                    : Mapper.Map<IEnumerable<StopViewModel>>(results.Stops.OrderBy(s => s.Order)));
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to get stops for trip {tripName}", ex);
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return Json("Error occurred finding trip name");
            }
        }

        [HttpPost("")]
        public async Task<JsonResult> Post(string tripName, [FromBody]StopViewModel vm)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    // temporarily decoding via WebUtility because of a bug in Beta 8
                    var decodedTripName = WebUtility.UrlDecode(tripName);
                    // map to the Entity
                    var newStop = Mapper.Map<Stop>(vm);

                    // look up Geocoordinates
                    var geoResult = await _geoService.Lookup(newStop.Name);

                    if (!geoResult.Success)
                    {
                        Response.StatusCode = (int)HttpStatusCode.Created;
                        Json(geoResult.Message);
                    }

                    newStop.Longitude = geoResult.Longitude;
                    newStop.Latitude = geoResult.Latitude;

                    // save to the database
                    // Add the newStop to the repository
                    _logger.LogInformation("Attempting to save a new stop");
                    _repository.AddStop(decodedTripName, User.Identity.Name, newStop);
                    // check if success
                    if (_repository.SaveAll())
                    {
                        Response.StatusCode = (int)HttpStatusCode.Created;
                        // we use the same Mapper to map the newStop back to its viewmodel representation
                        return Json(Mapper.Map<StopViewModel>(newStop));
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to save the new stop", ex);
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return Json(new { ex.Message });
            }

            // temporarily we can pass the ModelState
            // also we could make it dependent on the environment
            return Json(new { Message = "Failed", ModelState });
        }
    }
}
