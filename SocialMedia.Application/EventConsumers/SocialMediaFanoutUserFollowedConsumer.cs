using MassTransit;
using Microsoft.EntityFrameworkCore;
using Shared.Application.Events.Users;
using SocialMedia.Application.Interfaces;
using SocialMedia.Domain.Entities;
using StackExchange.Redis;

namespace SocialMedia.Application.EventConsumers
{
    public class SocialMediaFanoutUserFollowedConsumer : IConsumer<UserFollowedEvent>
    {
        private readonly ISocialMediaRepository<Post> _postRepository;
        private readonly IConnectionMultiplexer _redis;

        public SocialMediaFanoutUserFollowedConsumer(
            ISocialMediaRepository<Post> postRepository,
            IConnectionMultiplexer redis)
        {
            _postRepository = postRepository;
            _redis = redis;
        }

        public async Task Consume(ConsumeContext<UserFollowedEvent> context)
        {
            var message = context.Message;
            var db = _redis.GetDatabase();
            var feedKey = $"Feed:{message.FollowerId}";

            if (!await db.KeyExistsAsync(feedKey))
            {
                return;
            }

            var latestPosts = await _postRepository.GetTable()
                .AsNoTracking()
                .Where(post => post.UserId == message.FollowingId)
                .OrderByDescending(post => post.CreatedAt)
                .Take(20)
                .Select(post => new
                {
                    post.Id,
                    post.CreatedAt
                })
                .ToListAsync(context.CancellationToken);

            if (latestPosts.Count == 0)
            {
                return;
            }

            var entries = latestPosts
                .Select(post => new SortedSetEntry(
                    post.Id.ToString(),
                    new DateTimeOffset(post.CreatedAt, TimeSpan.Zero).ToUnixTimeSeconds()))
                .ToArray();

            var batch = db.CreateBatch();
            var addTask = batch.SortedSetAddAsync(feedKey, entries);
            var trimTask = batch.SortedSetRemoveRangeByRankAsync(feedKey, 0, -501);

            batch.Execute();
            await Task.WhenAll(addTask, trimTask);
        }
    }
}
