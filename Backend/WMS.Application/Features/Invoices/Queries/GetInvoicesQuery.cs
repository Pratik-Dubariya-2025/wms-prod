using MediatR;
using WMS.Application.Common.Attributes;
using WMS.Application.Common.Constants;
using WMS.Application.Common.Interfaces;
using WMS.Application.Common.Models;
using WMS.Application.Common.Policies;
using WMS.Application.Features.Invoices.DTOs;
using WMS.Domain.Interfaces;

namespace WMS.Application.Features.Invoices.Queries
{
    /// <summary>
    /// GET /api/accounts/invoices
    /// List financial invoices.
    /// Permissions: invoices:read:all (Accounts and Admin)
    /// </summary>
    [RequirePermission(PermissionCodes.InvoicesRead)]
    public class GetInvoicesQuery : IRequest<ApiResponse<PaginatedResult<InvoiceListDto>>>
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? Search { get; set; }
        public string? Status { get; set; }
    }

    public class GetInvoicesQueryHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        IPolicyEvaluationService policyEvaluationService)
        : IRequestHandler<GetInvoicesQuery, ApiResponse<PaginatedResult<InvoiceListDto>>>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly ICurrentUserService _currentUserService = currentUserService;
        private readonly IPolicyEvaluationService _policyEvaluationService = policyEvaluationService;

        public async Task<ApiResponse<PaginatedResult<InvoiceListDto>>> Handle(
            GetInvoicesQuery request, CancellationToken cancellationToken)
        {
            var currentUserId = _currentUserService.UserId;
            var (_, pbacFilter) = await PbacRowScope.ResolveAsync<WMS.Domain.Models.Invoice>(
                _policyEvaluationService, currentUserId, "ACCOUNTS", "read");

            WMS.Domain.Common.Models.PaginationDTO<InvoiceListDto> paginationDto = new()
            {
                PageNumber = request.PageNumber,
                ItemsPerPage = request.PageSize,
                Search = request.Search
            };

            var result = await _unitOfWork.Invoice.GetPaginatedList(
                select: i => new InvoiceListDto
                {
                    Id = i.Id,
                    InvoiceNumber = i.InvoiceNumber,
                    LeadId = i.LeadId,
                    CompanyName = i.Lead.CompanyName,
                    ContactName = i.Lead.ContactName,
                    Amount = i.Amount,
                    Status = i.Status,
                    IssuedDate = i.IssuedDate,
                    DueDate = i.DueDate,
                    CreatedAt = i.CreatedAt
                },
                paginationDto,
                where: query =>
                {
                    if (pbacFilter != null)
                    {
                        query = query.Where(pbacFilter);
                    }

                    // Search filter: invoice number, company name, contact name
                    if (!string.IsNullOrWhiteSpace(request.Search))
                    {
                        string search = request.Search.ToLower();
                        query = query.Where(i =>
                            i.InvoiceNumber.ToLower().Contains(search) ||
                            i.Lead.CompanyName.ToLower().Contains(search) ||
                            i.Lead.ContactName.ToLower().Contains(search)
                        );
                    }

                    // Status filter
                    if (!string.IsNullOrWhiteSpace(request.Status))
                    {
                        query = query.Where(i => i.Status == request.Status);
                    }

                    // Exclude soft-deleted
                    query = query.Where(i => !i.IsDeleted);

                    return query;
                }
            );

            PaginatedResult<InvoiceListDto> paginatedResult = PaginatedResult<InvoiceListDto>.Create(
                result.Records,
                result.TotalRecords,
                result.PageNumber,
                result.ItemsPerPage
            );

            return ApiResponse<PaginatedResult<InvoiceListDto>>.Success(paginatedResult);
        }
    }
}
