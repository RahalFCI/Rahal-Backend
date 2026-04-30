namespace Places.Application.DTOs.Place
{
    public class GetPlaceDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public Guid PlaceCategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public double TicketPrice { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public int GeoFenceRange { get; set; }
        public AddressDto? Address { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
