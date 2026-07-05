using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shared.Application.Events.Posts;
using Shared.Application.Interfaces;
using SocialMedia.Application.Interfaces;
using SocialMedia.Domain.Entities;
using StackExchange.Redis;

namespace SocialMedia.Application.EventConsumers
{
    public class SocialMediaFanoutPostDeletedConsumer : IConsumer<PostDeletedEvent>
    {
        private readonly IFollowRepository _followRepository;
        private readonly ILikeRepository _likeRepository;
        private readonly ISocialMediaRepository<Post> _postRepository;
        private readonly IObjectStorageService _storageService;
        private readonly IConnectionMultiplexer _redis;
        private readonly ILogger<SocialMediaFanoutPostDeletedConsumer> _logger;

        public SocialMediaFanoutPostDeletedConsumer(
            IFollowRepository followRepository,
            ILikeRepository likeRepository,
            ISocialMediaRepository<Post> postRepository,
            IObjectStorageService storageService,
            IConnectionMultiplexer redis,
            ILogger<SocialMediaFanoutPostDeletedConsumer> logger)
        {
            _followRepository = followRepository;
            _likeRepository = likeRepository;
            _postRepository = postRepository;
            _storageService = storageService;
            _redis = redis;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<PostDeletedEvent> context)
        {
            var message = context.Message;
            var followerIds = await _followRepository.GetFollowerIdsByFolloweeAsync(
                message.AuthorId,
                context.CancellationToken);
            var targetUserIds = followerIds
                .Append(message.AuthorId)
                .Distinct()
                .ToList();
            var likedUserIds = await _likeRepository.GetUserIdsWhoLikedPostAsync(
                message.PostId,
                context.CancellationToken);
            var mediaUrls = await _postRepository.GetTable()
                .IgnoreQueryFilters()
                .AsNoTracking()
                .Where(post => post.Id == message.PostId)
                .Select(post => post.MediaUrls)
                .FirstOrDefaultAsync(context.CancellationToken) ?? new List<string>();

            var db = _redis.GetDatabase();
            var batch = db.CreateBatch();
            var tasks = new List<Task>(1 + targetUserIds.Count + likedUserIds.Count);
            var postId = message.PostId.ToString();

            tasks.Add(batch.KeyDeleteAsync($"Post:{message.PostId}"));

            foreach (var userId in targetUserIds)
            {
                tasks.Add(batch.SortedSetRemoveAsync($"Feed:{userId}", postId));
            }

            foreach (var userId in likedUserIds.Distinct())
            {
                tasks.Add(batch.SetRemoveAsync($"UserLikes:{userId}", postId));
            }

            batch.Execute();
            await Task.WhenAll(tasks);

            foreach (var mediaUrl in mediaUrls)
            {
                try
                {
                    await _storageService.DeleteMediaAsync(mediaUrl, context.CancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        "Failed to delete Cloudinary media {MediaUrl} for deleted post {PostId} - non-fatal",
                        mediaUrl,
                        message.PostId);
                }
            }
        }
    }
}
