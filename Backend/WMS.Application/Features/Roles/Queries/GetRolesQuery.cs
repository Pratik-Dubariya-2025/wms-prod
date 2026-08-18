using MediatR;
using WMS.Application.Common.Attributes;
using WMS.Application.Common.Constants;
using WMS.Application.Common.Interfaces;
using WMS.Application.Common.Models;
using WMS.Application.Common.Policies;
using WMS.Application.Features.Roles.DTOs;
using WMS.Domain.Common.Models;
using WMS.Domain.Interfaces;

namespace WMS.Application.Features.Roles.Queries
{
    [RequirePermission(PermissionCodes.RoleRead)]
    public class GetRolesQuery : IRequest<ApiResponse<PaginatedResult<RoleDto>>>
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? Search { get; set; }
    }

    public class GetRolesQueryHandler(
        IUnitOfWork unitOfWork,
        IPolicyEvaluationService policyEvaluationService,
        ICurrentUserService currentUserService) 
        : IRequestHandler<GetRolesQuery, ApiResponse<PaginatedResult<RoleDto>>>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly IPolicyEvaluationService _policyEvaluationService = policyEvaluationService;
        private readonly ICurrentUserService _currentUserService = currentUserService;

        public async Task<ApiResponse<PaginatedResult<RoleDto>>> Handle(GetRolesQuery request, CancellationToken cancellationToken)
        {
            var currentUserId = _currentUserService.UserId;
            var (usePbac, pbacFilter) = await PbacRowScope.ResolveAsync<WMS.Domain.Models.Role>(
                _policyEvaluationService, currentUserId, "ROLE_MGMT", "read");

            WMS.Domain.Common.Models.PaginationDTO<RoleDto> paginationDto = new()
            {
                PageNumber = request.PageNumber,
                ItemsPerPage = request.PageSize,
                Search = request.Search
            };

            var result = await _unitOfWork.Role.GetPaginatedList(
                select: r => new RoleDto
                {
                    Id = r.Id,
                    Name = r.Name,
                    Code = r.Code,
                    Priority = r.Priority,
                    Description = r.Description,
                    IsSystemRole = r.IsSystemRole
                },
                paginationDTO: paginationDto,
                where: q => {
                    q = q.Where(r => !r.IsDeleted);
                    if (usePbac && pbacFilter != null)
                    {
                        q = q.Where(pbacFilter);
                    }
                    if (!string.IsNullOrEmpty(request.Search))
                    {
                        string s = request.Search.ToLower();
                        q = q.Where(r => r.Name.ToLower().Contains(s) || (r.Description != null && r.Description.ToLower().Contains(s)));
                    }
                    return q.OrderBy(r => r.Name);
                }
            );

            var responseResult = PaginatedResult<RoleDto>.Create(
                result.Records,
                result.TotalRecords,
                result.PageNumber,
                result.ItemsPerPage
            );

            return ApiResponse<PaginatedResult<RoleDto>>.Success(responseResult, "Roles retrieved successfully.");
        }
    }
}
