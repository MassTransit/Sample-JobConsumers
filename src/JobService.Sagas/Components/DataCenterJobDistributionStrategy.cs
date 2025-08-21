namespace JobService.Sagas.Components;

using MassTransit;
using MassTransit.Contracts.JobService;
using MassTransit.JobService;


public class DataCenterJobDistributionStrategy :
    IJobDistributionStrategy
{
    public Task<ActiveJob?> IsJobSlotAvailable(ConsumeContext<AllocateJobSlot> context, JobTypeInfo jobTypeInfo)
    {
        object? strategy = null;
        jobTypeInfo.Properties?.TryGetValue("DistributionStrategy", out strategy);

        return strategy switch
        {
            "DataCenter" => DataCenter(context, jobTypeInfo),
            _ => DefaultJobDistributionStrategy.Instance.IsJobSlotAvailable(context, jobTypeInfo)
        };
    }

    Task<ActiveJob?> DataCenter(ConsumeContext<AllocateJobSlot> context, JobTypeInfo jobTypeInfo)
    {
        var dataCenter = context.Message.JobProperties?.GetValueOrDefault("DataCenter") as string;

        LogContext.Info?.Log("Job for data center: {DataCenter}", dataCenter);

        var instances = from i in jobTypeInfo.Instances
            join a in jobTypeInfo.ActiveJobs on i.Key equals a.InstanceAddress into ai
            where (ai.Count() < jobTypeInfo.ConcurrentJobLimit && string.IsNullOrEmpty(dataCenter))
                || ((i.Value.Properties?.TryGetValue("DataCenter", out var dc) ?? false) && dc is string sdc && sdc == dataCenter)
            orderby ai.Count(), i.Value.Used
            select new
            {
                Instance = i.Value,
                InstanceAddress = i.Key,
                InstanceCount = ai.Count()
            };

        var firstInstance = instances.FirstOrDefault();
        if (firstInstance == null)
            return Task.FromResult<ActiveJob?>(null);

        return Task.FromResult<ActiveJob?>(new ActiveJob
        {
            JobId = context.Message.JobId,
            InstanceAddress = firstInstance.InstanceAddress
        });
    }
}