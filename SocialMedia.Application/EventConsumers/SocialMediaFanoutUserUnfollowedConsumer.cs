using MassTransit;
using Microsoft.EntityFrameworkCore;
using Shared.Application.Events.Users;
using SocialMedia.Application.Interfaces;
using SocialMedia.Domain.Entities;
using StackExchange.Redis;

namespace SocialMedia.Application.EventConsumers
{
    public class SocialMediaFanoutUserUnfollowedConsumer : IConsumer<UserUnfollowedEvent>
    {
        private const int RemoveChunkSize = 500;

        private readonly ISocialMediaRepository<Post> _postRepository;
        private readonly IConnectionMultiplexer _redis;

        public SocialMediaFanoutUserUnfollowedConsumer(
            ISocialMediaRepository<Post> postRepository,
            IConnectionMultiplexer redis)
        {
            _postRepository = postRepository;
            _redis = redis;
        }

        public async Task Consume(ConsumeContext<UserUnfollowedEvent> context)
        {
            var message = context.Message;
            var postIds = await _postRepository.GetTable()
                .AsNoTracking()
                .Where(post => post.UserId == message.FollowingId)
                .Select(post => post.Id)
                .ToListAsync(context.CancellationToken);

            if (postIds.Count == 0)
            {
                return;
            }

            var db = _redis.GetDatabase();
            var feedKey = (RedisKey)$"Feed:{message.FollowerId}";
            var batch = db.CreateBatch();
            var tasks = new List<Task<long>>();

            foreach (var chunk in postIds.Chunk(RemoveChunkSize))
            {
                var values = chunk
                    .Select(postId => (RedisValue)postId.ToString())
                    .ToArray();

                tasks.Add(batch.SortedSetRemoveAsync(feedKey, values));
            }

            batch.Execute();
            await Task.WhenAll(tasks);
        }
    }
}
