using MediatR;
using WMS.Application.Common.Attributes;
using WMS.Application.Common.Constants;
using WMS.Application.Common.Interfaces;
using WMS.Application.Common.Models;
using WMS.Application.Common.Policies;
using WMS.Application.Features.Crm.DTOs;
using WMS.Domain.Interfaces;

namespace WMS.Application.Features.Crm.Queries
{
    /// <summary>
    /// GET /api/crm/leads
    /// List CRM leads scoped by role:
    ///   - Admin/Manager/BDE/Accounts/HR: all leads
    ///   - BDA: own leads only
    /// </summary>
    [RequirePermission(PermissionCodes.LeadsRead)]
    public class GetLeadsQuery : IRequest<ApiResponse<PaginatedResult<LeadListDto>>>
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? Search { get; set; }
        public string? Stage { get; set; }
        public string? Region { get; set; }
    }

    public class GetLeadsQueryHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        IPolicyEvaluationService policyEvaluationService)
        : IRequestHandler<GetLeadsQuery, ApiResponse<PaginatedResult<LeadListDto>>>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly ICurrentUserService _currentUserService = currentUserService;
        private readonly IPolicyEvaluationService _policyEvaluationService = policyEvaluationService;

        public async Task<ApiResponse<PaginatedResult<LeadListDto>>> Handle(
            GetLeadsQuery request, CancellationToken cancellationToken)
        {
            var currentUserId = _currentUserService.UserId ?? Guid.Empty;
            var (usePbac, pbacFilter) = await PbacRowScope.ResolveAsync<WMS.Domain.Models.Lead>(
                _policyEvaluationService, _currentUserService.UserId, "CRM", "read");

            WMS.Domain.Common.Models.PaginationDTO<LeadListDto> paginationDto = new()
            {
                PageNumber = request.PageNumber,
                ItemsPerPage = request.PageSize,
                Search = request.Search
            };

            var result = await _unitOfWork.Lead.GetPaginatedList(
                select: l => new LeadListDto
                {
                    Id = l.Id,
                    CompanyName = l.CompanyName,
                    ContactName = l.ContactName,
                    ContactEmail = l.ContactEmail,
                    ContactPhone = l.ContactPhone,
                    Stage = l.Stage,
                    EstimatedValue = l.EstimatedValue,
                    OwnerId = l.OwnerId,
                    OwnerName = l.Owner.FirstName + " " + l.Owner.LastName,
                    Region = l.Region,
                    Source = l.Source,
                    ClosedAt = l.ClosedAt,
                    CreatedAt = l.CreatedAt
                },
                paginationDto,
                where: query =>
                {
                    if (usePbac)
                    {
                        if (pbacFilter != null)
                        {
                            query = query.Where(pbacFilter);
                        }
                    }
                    else
                    {
                        bool isAdmin = _currentUserService.Roles.Contains("ADMIN");
                        bool isManager = _currentUserService.Roles.Contains("MANAGER");
                        bool isBDE = _currentUserService.Roles.Contains("BDE");
                        bool isAccounts = _currentUserService.Roles.Contains("ACCOUNTS");
                        bool isHR = _currentUserService.Roles.Contains("HR");

                        // Row-level scoping: BDA can see only their own leads
                        if (!isAdmin && !isManager && !isBDE && !isAccounts && !isHR)
                        {
                            query = query.Where(l => l.OwnerId == currentUserId);
                        }
                    }

                    // Search filter
                    if (!string.IsNullOrWhiteSpace(request.Search))
                    {
                        string search = request.Search.ToLower();
                        query = query.Where(l =>
                            l.CompanyName.ToLower().Contains(search) ||
                            l.ContactName.ToLower().Contains(search) ||
                            (l.ContactEmail != null && l.ContactEmail.ToLower().Contains(search)) ||
                            (l.Region != null && l.Region.ToLower().Contains(search))
                        );
                    }

                    // Stage filter
                    if (!string.IsNullOrWhiteSpace(request.Stage))
                    {
                        query = query.Where(l => l.Stage == request.Stage);
                    }

                    // Region filter
                    if (!string.IsNullOrWhiteSpace(request.Region))
                    {
                        query = query.Where(l => l.Region == request.Region);
                    }

                    // Exclude soft-deleted
                    query = query.Where(l => !l.IsDeleted);

                    return query;
                }
            );

            PaginatedResult<LeadListDto> paginatedResult = PaginatedResult<LeadListDto>.Create(
                result.Records,
                result.TotalRecords,
                result.PageNumber,
                result.ItemsPerPage
            );

            return ApiResponse<PaginatedResult<LeadListDto>>.Success(paginatedResult);
        }
    }
}
