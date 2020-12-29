namespace JobService.Service.Controllers
{
    using System.Threading.Tasks;
    using Components;
    using MassTransit;
    using MassTransit.Contracts.JobService;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;


    [ApiController]
    [Route("[controller]")]
    public class ConvertVideoController :
        ControllerBase
    {
        readonly IRequestClient<ConvertVideo> _client;
        readonly ILogger<ConvertVideoController> _logger;

        public ConvertVideoController(ILogger<ConvertVideoController> logger, IRequestClient<ConvertVideo> client)
        {
            _logger = logger;
            _client = client;
        }

        [HttpPost("{path}")]
        public async Task<IActionResult> Get(string path)
        {
            _logger.LogInformation("Sending job: {Path}", path);

            Response<JobSubmissionAccepted> response = await _client.GetResponse<JobSubmissionAccepted>(new {Path = path});

            return Ok(new
            {
                response.Message.JobId,
                Path = path
            });
        }
    }
}