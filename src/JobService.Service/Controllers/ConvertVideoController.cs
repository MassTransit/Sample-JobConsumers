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
        readonly ILogger<ConvertVideoController> _logger;

        public ConvertVideoController(ILogger<ConvertVideoController> logger)
        {
            _logger = logger;
        }

        [HttpPost("{path}")]
        public async Task<IActionResult> SubmitJob(string path, [FromServices] IRequestClient<ConvertVideo> client)
        {
            _logger.LogInformation("Sending job: {Path}", path);

            var groupId = NewId.Next().ToString();

            Response<JobSubmissionAccepted> response = await client.GetResponse<JobSubmissionAccepted>(new
            {
                path,
                groupId,
                Index = 0,
                Count = 1,
                Details = new List<VideoDetail>
                {
                    new() { Value = "first" },
                    new() { Value = "second" }
                }
            });

            return Ok(new
            {
                response.Message.JobId,
                Path = path
            });
        }

        [HttpPut("{path}")]
        public async Task<IActionResult> FireAndForgetSubmitJob(string path, [FromServices] IPublishEndpoint publishEndpoint)
        {
            _logger.LogInformation("Sending job: {Path}", path);

            var jobId = NewId.NextGuid();
            var groupId = NewId.Next().ToString();

            await publishEndpoint.Publish<SubmitJob<ConvertVideo>>(new
            {
                JobId = jobId,
                Job = new
                {
                    path,
                    groupId,
                    Index = 0,
                    Count = 1,
                    Details = new VideoDetail[] { new() { Value = "first" }, new() { Value = "second" } }
                }
            });

            return Ok(new
            {
                jobId,
                Path = path
            });
        }

        [HttpPost("{count:int}")]
        public async Task<IActionResult> SubmitJob(int count, [FromServices] IRequestClient<ConvertVideo> client)
        {
            var jobIds = new List<Guid>(count);

            var groupId = NewId.Next().ToString();

            for (var i = 0; i < count; i++)
            {
                var path = NewId.Next() + ".txt";

                Response<JobSubmissionAccepted> response = await client.GetResponse<JobSubmissionAccepted>(new
                {
                    path,
                    groupId,
                    Index = i,
                    count
                });

                jobIds.Add(response.Message.JobId);
            }

            return Ok(new { jobIds });
        }

        [HttpGet("{jobId:guid}")]
        public async Task<IActionResult> GetJobState(Guid jobId, [FromServices] IRequestClient<GetJobState> client)
        {
            try
            {
                Response<JobState> response = await client.GetResponse<JobState>(new
                {
                    jobId,
                });

                return Ok(new
                {
                    jobId,
                    response.Message.CurrentState,
                    response.Message.Submitted,
                    response.Message.Started,
                    response.Message.Completed,
                    response.Message.Faulted,
                    response.Message.Reason,
                    response.Message.LastRetryAttempt,
                });
            }
            catch (Exception)
            {
                return NotFound();
            }
        }
    }
}