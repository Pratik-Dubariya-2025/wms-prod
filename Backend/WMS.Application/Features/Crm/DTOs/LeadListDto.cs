namespace WMS.Application.Features.Crm.DTOs
{
    public class LeadListDto
    {
        public Guid Id { get; set; }
        public string CompanyName { get; set; } = null!;
        public string ContactName { get; set; } = null!;
        public string? ContactEmail { get; set; }
        public string? ContactPhone { get; set; }
        public string Stage { get; set; } = null!;
        public decimal? EstimatedValue { get; set; }
        public Guid OwnerId { get; set; }
        public string OwnerName { get; set; } = null!;
        public string? Region { get; set; }
        public string? Source { get; set; }
        public DateTime? ClosedAt { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
