using System.Threading.Tasks;
using JobService.Components;
using MassTransit;
using MassTransit.Contracts.JobService;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace JobService.Service.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ConvertVideoController :
        ControllerBase
    {
        private readonly IRequestClient<IConvertVideo> _client;
        private readonly ILogger<ConvertVideoController> _logger;

        public ConvertVideoController(ILogger<ConvertVideoController> logger, IRequestClient<IConvertVideo> client)
        {
            _logger = logger;
            _client = client;
        }

        [HttpPost("{path}")]
        public async Task<IActionResult> Get(string path)
        {
            _logger.LogInformation("Sending job: {Path}", path);

            var response = await _client.GetResponse<JobSubmissionAccepted>(new
            {
                Path = path
            });

            return Ok(new
            {
                response.Message.JobId,
                Path = path
            });
        }
    }
}