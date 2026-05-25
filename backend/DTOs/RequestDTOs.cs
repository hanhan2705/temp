namespace backend.DTOs
{
    public class CreateAllocationRequestDto
    {
        public long TargetEmployeeId { get; set; }
        public string? DeviceType { get; set; }
        public string? Reason { get; set; }
    }

    public class CreateRecoveryRequestDto
    {
        public long TargetEmployeeId { get; set; }
        public long DeviceId { get; set; }
        public string? Reason { get; set; }
    }

    public class ApproveAllocationRequestDto
    {
        public long DeviceId { get; set; }
        public string? Note { get; set; }
    }

    public class ApproveRecoveryRequestDto
    {
        public string? DeviceCondition { get; set; }
        public string? Note { get; set; }
    }

    public class RejectRequestDto
    {
        public string? Reason { get; set; }
    }
}
