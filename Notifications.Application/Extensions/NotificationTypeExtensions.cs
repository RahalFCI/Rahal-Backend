using Notifications.Domain.Enums;

namespace Notifications.Application.Extensions
{
    public static class NotificationTypeExtensions
    {
        public static string ToStoredValue(this NotificationType type)
        {
            return type switch
            {
                NotificationType.SocialPostLike => "Social.PostLike",
                NotificationType.SocialFollow => "Social.Follow",
                NotificationType.SocialPostComment => "Social.PostComment",
                NotificationType.SocialNewPost => "Social.NewPost",
                _ => throw new ArgumentOutOfRangeException(nameof(type), type, "Unsupported notification type")
            };
        }

        public static NotificationType? ToNotificationType(this string? value)
        {
            return value switch
            {
                "Social.PostLike" => NotificationType.SocialPostLike,
                "Social.Follow" => NotificationType.SocialFollow,
                "Social.PostComment" => NotificationType.SocialPostComment,
                "Social.NewPost" => NotificationType.SocialNewPost,
                _ => null
            };
        }
    }
}
