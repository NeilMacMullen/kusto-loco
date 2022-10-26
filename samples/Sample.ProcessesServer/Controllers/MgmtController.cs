using BabyKusto.Server.Contract;
using BabyKusto.Server.Service;
using Microsoft.AspNetCore.Mvc;

namespace BabyKusto.SampleServer.Controllers
{
    [ApiController]
    public class MgmtController : ControllerBase
    {
        private readonly ManagementEndpointHelper _managementEndpointHelper;
        private readonly ILogger<MgmtController> _logger;

        public MgmtController(ManagementEndpointHelper managementEndpointHelper, ILogger<MgmtController> logger)
        {
            _managementEndpointHelper = managementEndpointHelper ?? throw new ArgumentNullException(nameof(managementEndpointHelper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpPost]
        [Route("/v1/rest/mgmt")]
        public IActionResult Execute(KustoApiMgmtRequestBody body)
        {
            if (body == null)
            {
                return this.BadRequest();
            }

            try
            {
                var result = _managementEndpointHelper.Process(body);
                return this.Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex, "Error processing mgmt api request.");
                return this.BadRequest(ex.ToString());
            }
        }
    }
}