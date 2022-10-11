using BabyKusto.Server.Contract;
using BabyKusto.Server.Service;
using Microsoft.AspNetCore.Mvc;

namespace BabyKusto.SampleServer.Controllers
{
    [ApiController]
    public class QueryController : ControllerBase
    {
        private readonly QueryEndpointHelper _queryEndpointHelper;
        private readonly ILogger<QueryController> _logger;

        public QueryController(QueryEndpointHelper queryEndpointHelper, ILogger<QueryController> logger)
        {
            _queryEndpointHelper = queryEndpointHelper ?? throw new ArgumentNullException(nameof(queryEndpointHelper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpPost]
        [Route("/v1/rest/query")]
        public IActionResult Execute(KustoApiQueryRequestBody body)
        {
            if (body == null)
            {
                return this.BadRequest();
            }

            try
            {
                var result = _queryEndpointHelper.Process(body);
                return this.Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex, "Error processing query api request.");
                return this.BadRequest(ex.ToString());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Unexpected error processing query api request, input: {body.Csl}.");
                return this.BadRequest(ex.ToString());
            }
        }
    }
}