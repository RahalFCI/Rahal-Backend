using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Shared.Application.Interfaces;
using Shared.Infrastructure.Settings;

namespace Shared.Infrastructure.Notifications
{
    public class FcmNotificationService : IFcmNotificationService
    {
        private const int MaxTokensPerBatch = 500;

        private static readonly object FirebaseAppLock = new();

        private readonly FirebaseSettings _settings;
        private readonly ILogger<FcmNotificationService> _logger;

        public FcmNotificationService(
            IOptions<FirebaseSettings> settings,
            ILogger<FcmNotificationService> logger)
        {
            _settings = settings.Value;
            _logger = logger;
        }

        public async Task SendMulticastAsync(
            IReadOnlyList<string> deviceTokens,
            string title,
            string body,
            Dictionary<string, string> dataPayload)
        {
            if (deviceTokens.Count == 0)
            {
                return;
            }

            var tokens = deviceTokens
                .Where(token => !string.IsNullOrWhiteSpace(token))
                .Distinct(StringComparer.Ordinal)
                .ToArray();

            if (tokens.Length == 0)
            {
                return;
            }

            var messaging = GetFirebaseMessagingOrDefault();
            if (messaging is null)
            {
                _logger.LogWarning("Skipping FCM push notification because Firebase is not configured.");
                return;
            }

            foreach (var tokenBatch in tokens.Chunk(MaxTokensPerBatch))
            {
                try
                {
                    var message = new MulticastMessage
                    {
                        Tokens = tokenBatch,
                        Notification = new FirebaseAdmin.Messaging.Notification
                        {
                            Title = title,
                            Body = body
                        },
                        Data = dataPayload
                    };

                    var response = await messaging.SendEachForMulticastAsync(message);

                    if (response.FailureCount > 0)
                    {
                        LogFailedTokens(tokenBatch, response);
                    }
                }
                catch (FirebaseMessagingException ex)
                {
                    _logger.LogError(
                        ex,
                        "Firebase failed to send multicast push notification to a batch of {TokenCount} device tokens.",
                        tokenBatch.Length);
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        "Unexpected failure while sending multicast push notification to a batch of {TokenCount} device tokens.",
                        tokenBatch.Length);
                }
            }
        }

        private FirebaseMessaging? GetFirebaseMessagingOrDefault()
        {
            try
            {
                var app = FirebaseApp.DefaultInstance ?? CreateFirebaseApp();
                return FirebaseMessaging.GetMessaging(app);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Firebase initialization failed.");
                return null;
            }
        }

        private FirebaseApp CreateFirebaseApp()
        {
            lock (FirebaseAppLock)
            {
                if (FirebaseApp.DefaultInstance is not null)
                {
                    return FirebaseApp.DefaultInstance;
                }

                if (string.IsNullOrWhiteSpace(_settings.ServiceAccountJson))
                {
                    _logger.LogWarning(
                        "Firebase service account JSON is not configured. Falling back to Google Application Default Credentials.");

                    return FirebaseApp.Create();
                }

                return FirebaseApp.Create(new AppOptions
                {
                    Credential = CredentialFactory
                        .FromJson(_settings.ServiceAccountJson, JsonCredentialParameters.ServiceAccountCredentialType)
                });
            }
        }

        private void LogFailedTokens(string[] tokenBatch, BatchResponse response)
        {
            for (var index = 0; index < response.Responses.Count; index++)
            {
                var sendResponse = response.Responses[index];
                if (sendResponse.IsSuccess)
                {
                    continue;
                }

                _logger.LogWarning(
                    sendResponse.Exception,
                    "FCM failed for device token at batch index {Index}. Token: {DeviceToken}",
                    index,
                    tokenBatch[index]);
            }
        }
    }
}
