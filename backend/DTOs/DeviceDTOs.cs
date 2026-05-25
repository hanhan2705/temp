namespace backend.DTOs
{
    public class CreateDeviceDto
    {
        public string Name { get; set; } = null!;
        public string? Type { get; set; }
        public DateTime? PurchaseDate { get; set; }
        public decimal? OriginalCost { get; set; }
        public string Status { get; set; } = "AVAILABLE";
    }

    public class UpdateDeviceDto
    {
        public string Name { get; set; } = null!;
        public string? Type { get; set; }
        public DateTime? PurchaseDate { get; set; }
        public decimal? OriginalCost { get; set; }
        public string Status { get; set; } = "AVAILABLE";
    }
}
