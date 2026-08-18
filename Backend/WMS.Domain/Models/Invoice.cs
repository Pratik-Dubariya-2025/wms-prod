namespace WMS.Domain.Models
{
    /// <summary>
    /// Financial Invoice model generated from ClosedWon CRM leads.
    /// </summary>
    public class Invoice : BaseModel
    {
        public Guid LeadId { get; set; }
        public Lead Lead { get; set; } = null!;
        public string InvoiceNumber { get; set; } = null!;
        public decimal Amount { get; set; }
        public string Status { get; set; } = "Draft"; // Draft, Sent, Paid, Overdue, Cancelled
        public DateTime IssuedDate { get; set; }
        public DateTime DueDate { get; set; }
    }
}
