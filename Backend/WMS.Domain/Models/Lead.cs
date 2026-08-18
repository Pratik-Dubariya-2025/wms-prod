namespace WMS.Domain.Models
{
    /// <summary>
    /// CRM Lead model.
    /// Maps to the SRS "leads" table (Section 4.6.1).
    /// </summary>
    public class Lead : BaseModel
    {
        public string CompanyName { get; set; } = null!;
        public string ContactName { get; set; } = null!;
        public string? ContactEmail { get; set; }
        public string? ContactPhone { get; set; }
        public string Stage { get; set; } = "NewLead"; // NewLead, Qualified, Proposal, Negotiation, ClosedWon, ClosedLost
        public decimal? EstimatedValue { get; set; }
        public Guid OwnerId { get; set; }
        public User Owner { get; set; } = null!;
        public string? Region { get; set; }
        public string? Source { get; set; }
        public DateTime? ClosedAt { get; set; }
    }
}
