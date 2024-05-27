using KustoLoco.Core;
using KustoLoco.FileFormats;
using Microsoft.AspNetCore.Mvc;

namespace WebApp.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class KqlController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<KqlController> _logger;
        private readonly KustoQueryContext context;

        public KqlController(ILogger<KqlController> logger)
        {
            _logger = logger;
            var data =
                Enumerable.Range(1, 1000).Select(index => new WeatherForecast
                    {
                        Date = DateTime.Now.AddDays(index),
                        TemperatureC = Random.Shared.Next(-20, 55),
                        Summary = Summaries[Random.Shared.Next(Summaries.Length)]
                    })
                    .ToArray();
            context = new KustoQueryContext();
            context.CopyDataIntoTable("data", data);
        }

        [HttpGet(Name = "GetWeatherForecast")]
        public async Task<KustoResultDto> Get(string query)
        {
            var result = await context.RunQuery(query);
            var dto = await ParquetResultSerializer.Default.Serialize(result);
            return dto;
        }
    }
}
