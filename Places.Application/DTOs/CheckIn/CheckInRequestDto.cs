namespace Places.Application.DTOs.CheckIn
{
    public class CheckInRequestDto
    {
        public Guid PlaceId { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public double AccuracyMeters { get; set; }
        public DateTime CapturedAt { get; set; }
        public bool IsMockLocation { get; set; }
        public bool IsJailbroken { get; set; }
    }
}
