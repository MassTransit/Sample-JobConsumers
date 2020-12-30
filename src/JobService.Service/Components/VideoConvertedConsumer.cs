namespace JobService.Service.Components
{
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using JobService.Components;
    using MassTransit;
    using Microsoft.Extensions.Logging;


    public class VideoConvertedConsumer :
        IConsumer<VideoConverted>
    {
        static readonly ConcurrentDictionary<string, GroupInfo> _groupInfo = new();

        readonly ILogger<VideoConvertedConsumer> _logger;

        public VideoConvertedConsumer(ILogger<VideoConvertedConsumer> logger)
        {
            _logger = logger;
        }

        public Task Consume(ConsumeContext<VideoConverted> context)
        {
            var groupInfo = _groupInfo.GetOrAdd(context.Message.GroupId, _ => new GroupInfo(context.Message.Count));

            var received = groupInfo.Increment(context.Message.Index);

            if (received == groupInfo.Count)
                _logger.LogInformation("Group Completed: {GroupId} ({Count})", context.Message.GroupId, groupInfo.Count);
            else if (groupInfo.Count - received < 10)
            {
                _logger.LogDebug("Group Remaining: {GroupId} ({Count}) {Remaining}", context.Message.GroupId, groupInfo.Count,
                    string.Join(", ", Enumerable.Range(0, groupInfo.Count).Except(groupInfo.Indices)));
            }
            else
                _logger.LogDebug("Group Progress: {GroupId} ({Received}/{Count})", context.Message.GroupId, received, groupInfo.Count);

            return Task.CompletedTask;
        }


        class GroupInfo
        {
            public readonly int Count;
            public readonly HashSet<int> Indices;
            public int Received;

            public GroupInfo(int count)
            {
                Count = count;
                Indices = new HashSet<int>();
            }

            public int Increment(int index)
            {
                Indices.Add(index);

                return Interlocked.Increment(ref Received);
            }
        }
    }
}