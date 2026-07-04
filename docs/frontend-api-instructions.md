# Rahal API frontend notes

Generated from `Rahal.Api/Controllers` on branch `feat/payments-module-sync`.

## Postman setup

- Import `docs/Rahal.API.postman_collection.json` into Postman.
- Set `baseUrl` to the API host. The collection default is `https://localhost:7143`.
- After login, copy the returned access and refresh tokens into `accessToken` and `refreshToken`.
- Requests marked `anonymous` disable bearer auth. Other requests inherit `Authorization: Bearer {{accessToken}}`.
- Replace ID variables such as `{{userId}}`, `{{placeId}}`, `{{explorerId}}`, `{{vendorId}}`, `{{postId}}`, and `{{couponId}}` before sending dependent requests.

## API conventions

- Most responses are wrapped in `ApiResponse<T>` with `data`, `isSuccess`, and `errorCode`.
- JSON examples use camelCase. ASP.NET model binding is case-insensitive, but frontend code should standardize on camelCase.
- Enums are serialized as strings by `JsonStringEnumConverter`; send values like `Explorer`, `Admin`, `Vendor`, `Male`, `Female`, `Verified`, `Failed`, `Percentage`, `FixedAmount`, `Xp`, `Visa`, `Image`, `Gif`, and `Video`.
- Offset pagination uses `page` and `pageSize`; cursor pagination uses `cursor` and `limit`.
- Admin/vendor/explorer role requirements are listed per endpoint below and in each Postman request description.

## Frontend flow notes

- Auth: use `POST /api/User/register`, `POST /api/EmailVerification/verify-email`, then `POST /api/User/login`. Refresh tokens with `POST /api/Auth/generate`.
- Profile setup: explorer and vendor profile create/update endpoints are `multipart/form-data`; include scalar DTO fields as form fields and `profilePicture` only when uploading an image.
- Social media uploads: call `POST /api/Media/signatures`, upload directly to Cloudinary with returned signature data, then pass pending `public_id` values as `mediaIds` to `POST /api/Posts`.
- Check-in flow: create a check-in with GPS/spoofing metadata, then create/validate check-in challenges as needed. Challenge validation uploads an `image` form-data file.
- Rewards payments: `POST /api/Subscription/purchase` is the user-facing subscription purchase endpoint. `POST /api/payments/test-intent` is anonymous and appears to be a dev/test helper.
- Stripe webhook: `POST /api/payments/webhooks/stripe` is for Stripe-to-backend delivery and requires `Stripe-Signature`; frontend clients should not call it.

## Endpoint inventory

Total controller actions: 183

| Method | Path | Auth | Query | Body |
| --- | --- | --- | --- | --- |
| POST | `api/Auth/forgot-password` | anonymous | - | ForgotPasswordRequest |
| POST | `api/Auth/generate` | anonymous | - | TokenDto |
| POST | `api/Auth/reset-password` | anonymous | - | ResetPasswordRequest |
| GET | `api/Achievement` | auth | page, pageSize | - |
| POST | `api/Achievement` | roles:Admin | - | CreateAchievementDto |
| DELETE | `api/Achievement/{id}` | roles:Admin | - | - |
| GET | `api/Achievement/{id}` | auth | - | - |
| PUT | `api/Achievement/{id}` | roles:Admin | - | UpdateAchievementDto |
| DELETE | `api/Achievement/{id}/permanent` | roles:Admin | - | - |
| POST | `api/Achievement/{id}/restore` | roles:Admin | - | - |
| GET | `api/AchievementCriteriaType` | auth | - | - |
| POST | `api/AchievementCriteriaType` | roles:Admin | - | AddAchievementCriteriaTypeDto |
| DELETE | `api/AchievementCriteriaType/{id}` | roles:Admin | - | - |
| GET | `api/AchievementCriteriaType/{id}` | auth | - | - |
| PUT | `api/AchievementCriteriaType/{id}` | roles:Admin | - | UpdateAchievementCriteriaTypeDto |
| GET | `api/AchievementCriteriaType/name/{name}` | auth | - | - |
| GET | `api/Badge` | auth | page, pageSize | - |
| POST | `api/Badge` | roles:Admin | - | CreateBadgeDto |
| DELETE | `api/Badge/{id}` | roles:Admin | - | - |
| GET | `api/Badge/{id}` | auth | - | - |
| PUT | `api/Badge/{id}` | roles:Admin | - | UpdateBadgeDto |
| DELETE | `api/Badge/{id}/permanent` | roles:Admin | - | - |
| POST | `api/Badge/{id}/restore` | roles:Admin | - | - |
| GET | `api/Badge/name/{name}` | auth | - | - |
| GET | `api/Challenge` | auth | page, pageSize | - |
| POST | `api/Challenge` | roles:Admin | - | CreateChallengeDto |
| DELETE | `api/Challenge/{id}` | roles:Admin | - | - |
| GET | `api/Challenge/{id}` | auth | - | - |
| PUT | `api/Challenge/{id}` | roles:Admin | - | UpdateChallengeDto |
| DELETE | `api/Challenge/{id}/permanent` | roles:Admin | - | - |
| POST | `api/Challenge/{id}/restore` | roles:Admin | - | - |
| GET | `api/Challenge/name/{name}` | auth | - | - |
| GET | `api/Challenge/place/{placeId}` | auth | page, pageSize | - |
| POST | `api/CheckInChallenge` | roles:Explorer | - | CreateCheckInChallengeDto |
| DELETE | `api/CheckInChallenge/{id}` | roles:Admin | - | - |
| GET | `api/CheckInChallenge/{id}` | auth | - | - |
| DELETE | `api/CheckInChallenge/{id}/permanent` | roles:Admin | - | - |
| POST | `api/CheckInChallenge/{id}/restore` | roles:Admin | - | - |
| POST | `api/CheckInChallenge/{id}/validate` | roles:Explorer | - | form |
| GET | `api/CheckInChallenge/challenge/{challengeId}` | auth | page, pageSize | - |
| GET | `api/CheckInChallenge/checkin/{checkInId}` | auth | page, pageSize | - |
| GET | `api/ExplorerAchievement` | roles:Admin | page, pageSize | - |
| DELETE | `api/ExplorerAchievement/{id}` | roles:Explorer,Admin | - | - |
| GET | `api/ExplorerAchievement/{id}` | auth | - | - |
| DELETE | `api/ExplorerAchievement/{id}/permanent` | roles:Admin | - | - |
| POST | `api/ExplorerAchievement/{id}/restore` | roles:Admin,Explorer | - | - |
| GET | `api/ExplorerAchievement/achievement/{achievementId}` | roles:Admin | page, pageSize | - |
| POST | `api/ExplorerAchievement/create` | roles:Explorer | - | CreateExplorerAchievementDto |
| GET | `api/ExplorerAchievement/explorer/{explorerId}` | auth | page, pageSize | - |
| GET | `api/ExplorerProfile` | roles:Admin | page, pageSize | - |
| GET | `api/ExplorerProfile/{explorerId}` | auth | - | - |
| PUT | `api/ExplorerProfile/{explorerId}` | roles:Explorer | - | form |
| PUT | `api/ExplorerProfile/{explorerId}/update-picture` | roles:Explorer | - | form |
| POST | `api/ExplorerProfile/create` | auth | - | form |
| GET | `api/ExplorerProfile/deleted` | roles:Admin | page, pageSize | - |
| GET | `api/UserStats` | roles:Admin | page, pageSize | - |
| GET | `api/UserStats/{explorerId}` | auth | - | - |
| POST | `api/VendorBranch` | roles:Vendor,Admin | - | CreateVendorBranchDto |
| DELETE | `api/VendorBranch/{id}` | roles:Vendor,Admin | - | - |
| GET | `api/VendorBranch/{id}` | auth | - | - |
| PUT | `api/VendorBranch/{id}` | roles:Vendor,Admin | - | UpdateVendorBranchDto |
| GET | `api/VendorBranch/vendor/{vendorId}` | auth | page, pageSize | - |
| GET | `api/VendorCategory` | auth | - | - |
| POST | `api/VendorCategory` | roles:Admin | - | string |
| DELETE | `api/VendorCategory/{id}` | roles:Admin | - | - |
| GET | `api/VendorCategory/{id}` | auth | - | - |
| PUT | `api/VendorCategory/{id}` | roles:Admin | - | string |
| GET | `api/VendorCategory/name/{name}` | auth | - | - |
| GET | `api/VendorProfile` | roles:Admin | page, pageSize | - |
| GET | `api/VendorProfile/{vendorId}` | auth | - | - |
| PUT | `api/VendorProfile/{vendorId}` | roles:Vendor | - | form |
| POST | `api/VendorProfile/{vendorId}/approve` | roles:Admin | - | - |
| PUT | `api/VendorProfile/{vendorId}/update-picture` | roles:Vendor | - | form |
| POST | `api/VendorProfile/create` | roles:Vendor | - | form |
| GET | `api/VendorProfile/deleted` | roles:Admin | page, pageSize | - |
| GET | `api/VendorProfile/unapproved` | roles:Admin | page, pageSize | - |
| GET | `api/XpTransaction/explorer/{explorerId}` | auth | page, pageSize | - |
| GET | `api/notifications` | auth | cursor, limit | - |
| PATCH | `api/notifications/{notificationId:guid}/read` | auth | - | - |
| POST | `api/notifications/fcm-token` | auth | - | SetFcmTokenRequest |
| PATCH | `api/notifications/read-all` | auth | - | - |
| GET | `api/notifications/unread-count` | auth | - | - |
| POST | `api/payments/test-intent` | anonymous | - | CreateTestPaymentIntentRequest |
| POST | `api/payments/webhooks/stripe` | anonymous | - | - |
| GET | `api/CheckIn` | roles:Admin | page, pageSize | - |
| POST | `api/CheckIn/{explorerId}` | roles:Explorer | - | CheckInRequestDto |
| DELETE | `api/CheckIn/{explorerId}/{placeId}` | roles:Explorer,Admin | - | - |
| GET | `api/CheckIn/{explorerId}/{placeId}` | auth | - | - |
| PUT | `api/CheckIn/{explorerId}/{placeId}` | roles:Explorer,Admin | - | UpdateCheckInDto |
| GET | `api/CheckIn/explorer/{explorerId}` | auth | page, pageSize | - |
| GET | `api/CheckIn/pending` | roles:Admin | page, pageSize | - |
| GET | `api/CheckIn/place/{placeId}` | auth | page, pageSize | - |
| GET | `api/Place` | anonymous | page, pageSize | - |
| POST | `api/Place` | roles:Admin | - | CreatePlaceDto |
| DELETE | `api/Place/{id}` | roles:Admin | - | - |
| GET | `api/Place/{id}` | anonymous | - | - |
| PUT | `api/Place/{id}` | roles:Admin | - | UpdatePlaceDto |
| GET | `api/Place/category/{categoryId}` | anonymous | page, pageSize | - |
| POST | `api/Place/search` | anonymous | Latitude, Longitude, RadiusInMeters, offsetPaginationRequest.Page, offsetPaginationRequest.PageSize | - |
| GET | `api/PlaceCategory` | anonymous | - | - |
| POST | `api/PlaceCategory` | roles:Admin | - | CreatePlaceCategoryDto |
| DELETE | `api/PlaceCategory/{id}` | roles:Admin | - | - |
| GET | `api/PlaceCategory/{id}` | anonymous | - | - |
| PUT | `api/PlaceCategory/{id}` | roles:Admin | - | UpdatePlaceCategoryDto |
| POST | `api/PlacePhoto` | roles:Admin | - | form |
| POST | `api/PlacePhoto/batch` | anonymous | - | IEnumerable<Guid> |
| GET | `api/PlacePhoto/place/{placeId}` | anonymous | - | - |
| DELETE | `api/PlacePhoto/place/{placeId}/url` | roles:Admin | url | - |
| POST | `api/PlaceReview` | roles:Explorer | - | CreatePlaceReviewDto |
| DELETE | `api/PlaceReview/{explorerId}/{placeId}/{checkInId}` | roles:Explorer,Admin | - | - |
| GET | `api/PlaceReview/{explorerId}/{placeId}/{checkInId}` | anonymous | - | - |
| PUT | `api/PlaceReview/{explorerId}/{placeId}/{checkInId}` | roles:Explorer | - | UpdatePlaceReviewDto |
| GET | `api/PlaceReview/explorer/{explorerId}` | auth | - | - |
| GET | `api/PlaceReview/place/{placeId}` | anonymous | - | - |
| GET | `api/PlaceReview/verified/{placeId}` | anonymous | - | - |
| GET | `api/Coupon` | auth | page, pageSize | - |
| POST | `api/Coupon` | roles:Admin | - | CreateCouponDto |
| DELETE | `api/Coupon/{id}` | roles:Admin | - | - |
| GET | `api/Coupon/{id}` | auth | - | - |
| PUT | `api/Coupon/{id}` | roles:Admin | - | UpdateCouponDto |
| GET | `api/Coupon/search` | auth | Query, VendorId, DiscountType, MaxXpCost, IsActive, Page, PageSize | - |
| GET | `api/PlanTier` | auth | page, pageSize | - |
| POST | `api/PlanTier` | roles:Admin | - | CreatePlanTierDto |
| DELETE | `api/PlanTier/{id}` | roles:Admin | - | - |
| GET | `api/PlanTier/{id}` | auth | - | - |
| PUT | `api/PlanTier/{id}` | roles:Admin | - | UpdatePlanTierDto |
| DELETE | `api/PlanTier/permanent/{id}` | roles:Admin | - | - |
| GET | `api/Subscription/active` | roles:Explorer | - | - |
| PUT | `api/Subscription/cancel` | roles:Explorer | - | - |
| GET | `api/Subscription/mine` | roles:Explorer | page, pageSize | - |
| POST | `api/Subscription/purchase` | roles:Explorer | - | PurchaseSubscriptionDto |
| POST | `api/TravelPlan` | roles:Explorer | - | CreateTravelPlanDto |
| GET | `api/TravelPlan/{id}` | roles:Explorer | - | - |
| GET | `api/TravelPlan/mine` | roles:Explorer | page, pageSize | - |
| POST | `api/UserCoupon/claim/{couponId}` | roles:Explorer | - | - |
| GET | `api/UserCoupon/code/{code}` | roles:Vendor,Admin | - | - |
| GET | `api/UserCoupon/mine` | roles:Explorer | page, pageSize | - |
| POST | `api/UserCoupon/redeem` | roles:Vendor | - | RedeemCouponDto |
| GET | `api/Search/explorers` | anonymous | Query, Page, PageSize, Filter, SortBy | - |
| GET | `api/Search/health` | anonymous | - | - |
| GET | `api/Search/places` | anonymous | Query, Page, PageSize, Filter, SortBy | - |
| GET | `api/Search/users` | anonymous | Query, Page, PageSize, Filter, SortBy | - |
| GET | `api/Search/vendors` | anonymous | Query, Page, PageSize, Filter, SortBy | - |
| DELETE | `api/comments/{commentId:guid}` | roles:Explorer,Admin | - | - |
| PUT | `api/comments/{commentId:guid}` | roles:Explorer | - | EditCommentRequest |
| GET | `api/comments/{commentId:guid}/replies` | roles:Explorer | cursor, limit | - |
| POST | `api/Media/signatures` | auth | - | GenerateUploadSignaturesRequest |
| POST | `api/Posts` | roles:Explorer | - | CreatePostRequest |
| GET | `api/Posts/{id:guid}` | roles:Explorer | - | - |
| DELETE | `api/Posts/{postId:guid}` | roles:Explorer,Admin | - | - |
| GET | `api/Posts/{postId:guid}/comments` | roles:Explorer | cursor, limit | - |
| POST | `api/Posts/{postId:guid}/comments` | roles:Explorer | - | CreateCommentRequest |
| DELETE | `api/Posts/{postId:guid}/like` | roles:Explorer | - | - |
| POST | `api/Posts/{postId:guid}/like` | roles:Explorer | - | - |
| GET | `api/social-media/users` | auth | page, pageSize | - |
| GET | `api/social-media/users/{userId:guid}` | auth | - | - |
| DELETE | `api/users/{targetUserId:guid}/follow` | auth | - | - |
| POST | `api/users/{targetUserId:guid}/follow` | auth | - | - |
| GET | `api/users/{userId:guid}/feed` | auth | cursor, limit | - |
| GET | `api/users/{userId:guid}/followees` | auth | page, pageSize | - |
| GET | `api/users/{userId:guid}/followers` | auth | page, pageSize | - |
| GET | `api/users/{userId:guid}/posts` | auth | cursor, limit | - |
| POST | `api/EmailVerification/resend-verification` | anonymous | - | ResendOtpRequest |
| POST | `api/EmailVerification/verify-email` | anonymous | - | VerifyOtpRequest |
| GET | `api/User` | roles:Admin | page, pageSize | - |
| DELETE | `api/User/{id}` | roles:Admin | - | - |
| GET | `api/User/{id}` | auth | - | - |
| PUT | `api/User/{id}` | auth | - | BaseUserDto |
| GET | `api/User/admins` | roles:Admin | page, pageSize | - |
| GET | `api/User/admins-include-deleted` | roles:Admin | page, pageSize | - |
| GET | `api/User/explorers` | auth | page, pageSize | - |
| GET | `api/User/explorers-include-deleted` | roles:Admin | page, pageSize | - |
| POST | `api/User/google-signin` | anonymous | - | GoogleSignInRequest |
| GET | `api/User/include-deleted` | roles:Admin | page, pageSize | - |
| POST | `api/User/login` | anonymous | - | AuthRequestDto |
| POST | `api/User/logout` | auth | - | - |
| PUT | `api/User/password/{id}` | auth | - | UpdatePasswordDto |
| DELETE | `api/User/permanent/{id}` | roles:Admin | - | - |
| POST | `api/User/register` | anonymous | - | BaseRegisterDto |
| POST | `api/User/register-admin` | roles:Admin | - | BaseRegisterDto |
| PUT | `api/User/restore/{id}` | roles:Admin | - | - |
| GET | `api/User/vendors` | auth | page, pageSize | - |
| GET | `api/User/vendors-include-deleted` | roles:Admin | page, pageSize | - |

