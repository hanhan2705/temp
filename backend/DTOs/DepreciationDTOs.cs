namespace backend.DTOs
{
    public class SetupDepreciationDto
    {
        public long DeviceId { get; set; }
        public string Method { get; set; } = "STRAIGHT_LINE";
        public decimal InitialValue { get; set; }
        public int UsefulLifeMonths { get; set; } = 36;
        public decimal SalvageValue { get; set; } = 0;
        public string Period { get; set; } = "MONTH";
    }
}
