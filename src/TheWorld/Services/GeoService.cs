using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.OptionsModel;
using Newtonsoft.Json.Linq;

namespace TheWorld.Services
{
    public class GeoService
    {
        private readonly ILogger<GeoService> _logger;
        private readonly IOptions<Startup.MyOptions> _options;

        public GeoService(
            ILogger<GeoService> logger,
            IOptions<Startup.MyOptions> options)
        {
            _logger = logger;
            _options = options;
        }

        // we need a method that returns a data structure
        // and we have to wrap it in an async Task for the http service
        public async Task<GeoServiceResult> Lookup(string location)
        {
            // we start with a dummy object with default failure conditions
            var result = new GeoServiceResult()
            {
                Success = false,
                Message = "Undetermined failure while looking up coordinates"
            };

            // lookup coordinates
            var encodedName = WebUtility.UrlEncode(location);
            // we can use the following setting if we have added our BingKey to our secret.json file
            var bingKey = _options.Value.BingKey;
            if (string.IsNullOrWhiteSpace(bingKey))
            {
                // we can use the following setting if we added "AppSettngs:BingKey" to Windows environment variables
                bingKey = Startup.Configuration["AppSettngs:BingKey"];
            }
            var url = $"http://dev.virtualearth.net/REST/v1/Locations?q={encodedName}&key={bingKey}";

            var client = new HttpClient();
            var json = await client.GetStringAsync(url);

            // We will parse json with Newtonsoft libraries
            // TODO: Fragile, might need to change if the Bing API changes
            // the result is a parsable object
            var results = JObject.Parse(json);
            var resources = results["resourceSets"][0]["resources"];
            if (!resources.HasValues)
            {
                result.Message = $"Could not find '{location}' as a location";
            }
            else
            {
                var confidence = (string)resources[0]["confidence"];
                if (confidence != "High")
                {
                    result.Message = $"Could not find a confident match for '{location}' as a location";
                }
                else
                {
                    var coords = resources[0]["geocodePoints"][0]["coordinates"];
                    result.Latitude = (double)coords[0];
                    result.Longitude = (double)coords[1];
                    result.Success = true;
                    result.Message = "Success";
                }
            }

            return result;
        }
    }
}
