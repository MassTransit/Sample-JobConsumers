namespace JobService.Service.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using JobService.Components;
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

            var groupId = NewId.Next().ToString();

            Response<JobSubmissionAccepted> response = await _client.GetResponse<JobSubmissionAccepted>(new
            {
                path,
                groupId,
                Index = 0,
                Count = 1
            });

            return Ok(new
            {
                response.Message.JobId,
                Path = path
            });
        }

        [HttpGet("{count}")]
        public async Task<IActionResult> Get(int count)
        {
            var jobIds = new List<Guid>(count);

            var groupId = NewId.Next().ToString();

            for (var i = 0; i < count; i++)
            {
                var path = NewId.Next() + ".txt";

                Response<JobSubmissionAccepted> response = await _client.GetResponse<JobSubmissionAccepted>(new
                {
                    path,
                    groupId,
                    Index = i,
                    count
                });

                jobIds.Add(response.Message.JobId);
            }

            return Ok(new {jobIds});
        }
    }
}