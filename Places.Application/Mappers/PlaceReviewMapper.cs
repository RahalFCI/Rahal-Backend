using Places.Application.DTOs.PlaceReview;
using Places.Domain.Entities;

namespace Places.Application.Mappers
{
    public static class PlaceReviewMapper
    {
        public static GetPlaceReviewDto ToGetDto(PlaceReview review)
        {
            return new GetPlaceReviewDto
            {
                ExplorerId = review.ExplorerId,
                PlaceId = review.PlaceId,
                CheckInId = review.CheckInId,
                Rating = review.Rating,
                Comment = review.Comment,
                IsVerified = review.IsVerified,
                PlaceName = review.Place?.Name ?? string.Empty
            };
        }

        public static PlaceReview ToEntity(CreatePlaceReviewDto dto)
        {
            return new PlaceReview
            {
                ExplorerId = dto.ExplorerId,
                PlaceId = dto.PlaceId,
                CheckInId = dto.CheckInId,
                Rating = dto.Rating,
                Comment = dto.Comment,
                IsVerified = false
            };
        }

        public static void UpdateEntity(PlaceReview review, UpdatePlaceReviewDto dto)
        {
            review.Rating = dto.Rating;
            review.Comment = dto.Comment;
        }

        public static IEnumerable<GetPlaceReviewDto> ToGetDtos(IEnumerable<PlaceReview> reviews)
        {
            return reviews.Select(ToGetDto);
        }
    }
}
