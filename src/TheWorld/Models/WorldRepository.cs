using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Data.Entity;
using Microsoft.Extensions.Logging;

namespace TheWorld.Models
{
    /// <summary>
    /// With the Repository pattern we will be able to expose
    /// an API for our data
    /// First, we create a class and methods and then extract the interface
    /// </summary>
    public class WorldRepository : IWorldRepository
    {
        private readonly WorldContext _context;
        private readonly ILogger<WorldRepository> _logger;

        /// <summary>
        /// The context here is the DbContext for our real database
        /// The logger gets the logging object as a parameter,
        /// in our case it is WorldRepository itself.
        /// But now we are able to catch exceptions.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="logger"></param>
        public WorldRepository(
            WorldContext context,
            ILogger<WorldRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public Trip GetTripByName(string tripName)
        {
            return _context.Trips
                .Include(t => t.Stops)
                .FirstOrDefault(t => t.Name == tripName);
        }

        public IEnumerable<Trip> GetAllTrips()
        {
            var trips = new List<Trip>();

            try
            {
                trips = _context.Trips
                    .OrderBy(t => t.Name)
                    .ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError("Could not get trips from database", ex);

                /** here we could also finish like this:
                 * - throw;
                 * - return null;
                 * - return new List<Trip>();
                 * it all depends on what we want to log and let the user see
                 */
            }

            return trips;
        }

        public IEnumerable<Trip> GetAllTripsWithStops()
        {
            var trips = new List<Trip>();

            try
            {
                trips = _context.Trips
                    .Include(t => t.Stops)
                    .OrderBy(t => t.Name)
                    .ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError("Could not get trips with stops from database", ex);
            }

            return trips;
        }

        public void AddTrip(Trip newTrip)
        {
            // here we can also set some default properties if we need them
            // like Created or UserName
            _context.Add(newTrip);
        }

        public void AddStop(string tripName, Stop newStop)
        {
            var theTrip = GetTripByName(tripName);
            // here we can also set some default properties if we need them
            newStop.Order = theTrip.Stops.Max(s => s.Order) + 1;
            // we are adding the newStop to the Stops list
            theTrip.Stops.Add(newStop);
            _context.Stops.Add(newStop);
        }

        public bool SaveAll()
        {
            // SaveChanges return the number of rows (SQL)
            // or documents (noSQL) successfully inserted
            return _context.SaveChanges() > 0;
        }
    }
}
