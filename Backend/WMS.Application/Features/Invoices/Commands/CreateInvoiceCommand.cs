using MediatR;
using WMS.Application.Common.Attributes;
using WMS.Application.Common.Constants;
using WMS.Application.Common.Interfaces;
using WMS.Application.Common.Models;
using WMS.Domain.Interfaces;
using WMS.Domain.Models;

namespace WMS.Application.Features.Invoices.Commands
{
    /// <summary>
    /// POST /api/accounts/invoices
    /// Creates a new invoice from a closed-won crm deal.
    /// Permissions: invoices:write (Accounts and Admin)
    /// </summary>
    [RequirePermission(PermissionCodes.InvoicesWrite)]
    public class CreateInvoiceCommand : IRequest<ApiResponse<Guid>>
    {
        public Guid LeadId { get; set; }
        public decimal? Amount { get; set; }
        public int DueDays { get; set; } = 30; // Number of days for due date from now
    }

    public class CreateInvoiceCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService)
        : IRequestHandler<CreateInvoiceCommand, ApiResponse<Guid>>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly ICurrentUserService _currentUserService = currentUserService;

        public async Task<ApiResponse<Guid>> Handle(
            CreateInvoiceCommand request, CancellationToken cancellationToken)
        {
            if (!_currentUserService.UserId.HasValue)
            {
                return ApiResponse<Guid>.Failure("Unauthorized access.", null, 401);
            }

            // Find lead
            var lead = await _unitOfWork.Lead.GetFirstOrDefaultAsync(l => l.Id == request.LeadId && !l.IsDeleted);
            if (lead == null)
            {
                return ApiResponse<Guid>.Failure("Lead not found.", null, 404);
            }

            // Must be ClosedWon deal
            if (lead.Stage != "ClosedWon")
            {
                return ApiResponse<Guid>.Failure($"Cannot generate invoice for lead in '{lead.Stage}' stage. Lead must be in 'ClosedWon' stage.", null, 400);
            }

            // Check if already invoiced
            var existingInvoice = await _unitOfWork.Invoice.GetFirstOrDefaultAsync(i => i.LeadId == request.LeadId && !i.IsDeleted);
            if (existingInvoice != null)
            {
                return ApiResponse<Guid>.Failure($"An invoice ({existingInvoice.InvoiceNumber}) has already been generated for this lead.", null, 400);
            }

            // Determine Amount
            decimal amount = request.Amount ?? lead.EstimatedValue ?? 0;
            if (amount <= 0)
            {
                return ApiResponse<Guid>.Failure("Invoice amount must be greater than 0.", null, 400);
            }

            // Generate invoice number sequentially (approximate)
            int year = DateTime.UtcNow.Year;
            // Let's count existing invoices
            var paginationDto = new WMS.Domain.Common.Models.PaginationDTO<Invoice> { PageNumber = 1, ItemsPerPage = 1 };
            var listResult = await _unitOfWork.Invoice.GetPaginatedList(
                i => i,
                paginationDto,
                where: q => q.Where(i => !i.IsDeleted)
            );
            int invoiceCount = listResult.TotalRecords;
            string invoiceNumber = $"INV-{year}-{(invoiceCount + 1):D4}";

            // Date calculations
            DateTime issuedDate = DateTime.UtcNow.Date;
            DateTime dueDate = issuedDate.AddDays(request.DueDays > 0 ? request.DueDays : 30);

            Invoice invoice = new()
            {
                LeadId = request.LeadId,
                InvoiceNumber = invoiceNumber,
                Amount = amount,
                Status = "Sent",
                IssuedDate = issuedDate,
                DueDate = dueDate,
                CreatedBy = _currentUserService.Username ?? "System"
            };

            await _unitOfWork.Invoice.AddAsync(invoice);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return ApiResponse<Guid>.Success(invoice.Id, $"Invoice {invoiceNumber} generated successfully.", 201);
        }
    }
}
