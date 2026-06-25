using MassTransit;
using Shared.Application.Events.Posts;
using SocialMedia.Application.Interfaces;
using StackExchange.Redis;

namespace SocialMedia.Application.EventConsumers
{
    public class SocialMediaFanoutPostDeletedConsumer : IConsumer<PostDeletedEvent>
    {
        private readonly IFollowRepository _followRepository;
        private readonly IConnectionMultiplexer _redis;

        public SocialMediaFanoutPostDeletedConsumer(
            IFollowRepository followRepository,
            IConnectionMultiplexer redis)
        {
            _followRepository = followRepository;
            _redis = redis;
        }

        public async Task Consume(ConsumeContext<PostDeletedEvent> context)
        {
            var message = context.Message;
            var followerIds = await _followRepository.GetFollowerIdsByFolloweeAsync(
                message.AuthorId,
                context.CancellationToken);

            if (followerIds.Count == 0)
            {
                return;
            }

            var db = _redis.GetDatabase();
            var batch = db.CreateBatch();
            var tasks = new List<Task<bool>>(followerIds.Count);
            var postId = message.PostId.ToString();

            foreach (var followerId in followerIds)
            {
                tasks.Add(batch.SortedSetRemoveAsync($"Feed:{followerId}", postId));
            }

            batch.Execute();
            await Task.WhenAll(tasks);
        }
    }
}
