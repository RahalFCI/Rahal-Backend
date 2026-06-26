using MassTransit;
using Shared.Application.Events.Posts;
using SocialMedia.Application.Interfaces;
using StackExchange.Redis;

namespace SocialMedia.Application.EventConsumers
{
    public class SocialMediaFanoutPostCreatedConsumer : IConsumer<PostCreatedEvent>
    {
        private const string AppendIfFeedExistsScript = """
            if redis.call('EXISTS', KEYS[1]) == 1 then
                redis.call('ZADD', KEYS[1], ARGV[1], ARGV[2])
                redis.call('ZREMRANGEBYRANK', KEYS[1], 0, -501)
                return 1
            end
            return 0
            """;

        private readonly IFollowRepository _followRepository;
        private readonly IConnectionMultiplexer _redis;

        public SocialMediaFanoutPostCreatedConsumer(
            IFollowRepository followRepository,
            IConnectionMultiplexer redis)
        {
            _followRepository = followRepository;
            _redis = redis;
        }

        public async Task Consume(ConsumeContext<PostCreatedEvent> context)
        {
            var message = context.Message;
            var followerIds = await _followRepository.GetFollowerIdsByFolloweeAsync(
                message.UserId,
                context.CancellationToken);
            var targetUserIds = followerIds
                .Append(message.UserId)
                .Distinct()
                .ToList();

            var db = _redis.GetDatabase();
            var batch = db.CreateBatch();
            var score = new DateTimeOffset(message.CreatedAt, TimeSpan.Zero).ToUnixTimeSeconds();
            var tasks = new List<Task<RedisResult>>(targetUserIds.Count);

            foreach (var userId in targetUserIds)
            {
                tasks.Add(batch.ScriptEvaluateAsync(
                    AppendIfFeedExistsScript,
                    new RedisKey[] { $"Feed:{userId}" },
                    new RedisValue[] { score, message.PostId.ToString() }));
            }

            batch.Execute();
            await Task.WhenAll(tasks);
        }
    }
}
