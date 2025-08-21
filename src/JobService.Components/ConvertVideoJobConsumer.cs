namespace JobService.Components;

using System;
using System.Threading.Tasks;
using Contracts;
using MassTransit;
using Microsoft.Extensions.Logging;


public class ConvertVideoJobConsumer :
    IJobConsumer<ConvertVideo>
{
    readonly ILogger<ConvertVideoJobConsumer> _logger;

    public ConvertVideoJobConsumer(ILogger<ConvertVideoJobConsumer> logger)
    {
        _logger = logger;
    }

    public async Task Run(JobContext<ConvertVideo> context)
    {
        var variance = context.TryGetJobState(out ConsumerState? state)
            ? TimeSpan.FromMilliseconds(state!.Variance)
            : TimeSpan.FromMilliseconds(Random.Shared.Next(8399, 28377));

        _logger.LogInformation("Converting Video: {GroupId} {Path}", context.Job.GroupId, context.Job.Path);

        try
        {
            await context.SetJobProgress(0, (long)variance.TotalMilliseconds);

            await Task.Delay(variance, context.CancellationToken);

            await context.SetJobProgress((long)variance.TotalMilliseconds, (long)variance.TotalMilliseconds);

            await context.Publish<VideoConverted>(context.Job);

            _logger.LogInformation("Converted Video: {GroupId} {Path}", context.Job.GroupId, context.Job.Path);
        }
        catch (OperationCanceledException) when (context.CancellationToken.IsCancellationRequested)
        {
            await context.SaveJobState(new ConsumerState { Variance = (long)variance.TotalMilliseconds });

            throw;
        }
    }
}


class ConsumerState
{
    public long Variance { get; set; }
}