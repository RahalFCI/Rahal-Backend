using MassTransit;
using Microsoft.EntityFrameworkCore;
using Shared.Application.Events.SocialMedia;
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
        private readonly IPublishEndpoint _publisher;

        public SocialMediaFanoutUserFollowedConsumer(
            ISocialMediaRepository<Post> postRepository,
            IConnectionMultiplexer redis,
            IPublishEndpoint publisher)
        {
            _postRepository = postRepository;
            _redis = redis;
            _publisher = publisher;
        }

        public async Task Consume(ConsumeContext<UserFollowedEvent> context)
        {
            if (context.Headers.TryGetHeader(SocialEventHeaders.SocialFanoutCompleted, out _))
            {
                return;
            }

            var message = context.Message;
            var db = _redis.GetDatabase();
            var feedKey = $"Feed:{message.FollowerId}";

            if (!await db.KeyExistsAsync(feedKey))
            {
                await PublishNotificationReadyEventAsync(message, context.CancellationToken);
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
                await PublishNotificationReadyEventAsync(message, context.CancellationToken);
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

            await PublishNotificationReadyEventAsync(message, context.CancellationToken);
        }

        private Task PublishNotificationReadyEventAsync(
            UserFollowedEvent message,
            CancellationToken cancellationToken)
        {
            return _publisher.Publish(
                message,
                publishContext => publishContext.Headers.Set(SocialEventHeaders.SocialFanoutCompleted, true),
                cancellationToken);
        }
    }
}
