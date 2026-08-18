using MediatR;
using WMS.Application.Common.Attributes;
using WMS.Application.Common.Constants;
using WMS.Application.Common.Models;
using WMS.Application.Features.Policies.DTOs;
using WMS.Domain.Common.Models;
using WMS.Domain.Interfaces;

namespace WMS.Application.Features.Policies.Queries
{
    [RequirePermission(PermissionCodes.PolicyRead)]
    public class GetAccessPoliciesQuery : IRequest<ApiResponse<PaginatedResult<AccessPolicyListDto>>>
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? Search { get; set; }
        public Guid? ModuleId { get; set; }
        public Guid? RoleId { get; set; }
        public Guid? UserId { get; set; }
        public string? Effect { get; set; }
        public bool? IsActive { get; set; }
    }

    public class GetAccessPoliciesQueryHandler(IUnitOfWork unitOfWork)
        : IRequestHandler<GetAccessPoliciesQuery, ApiResponse<PaginatedResult<AccessPolicyListDto>>>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;

        public async Task<ApiResponse<PaginatedResult<AccessPolicyListDto>>> Handle(
            GetAccessPoliciesQuery request, CancellationToken cancellationToken)
        {
            WMS.Domain.Common.Models.PaginationDTO<AccessPolicyListDto> paginationDto = new()
            {
                PageNumber = request.PageNumber,
                ItemsPerPage = request.PageSize,
                Search = request.Search
            };

            var result = await _unitOfWork.AccessPolicy.GetPaginatedList(
                select: p => new AccessPolicyListDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    ModuleName = p.Module.Name,
                    ModuleCode = p.Module.Code,
                    Action = p.Action,
                    Effect = p.Effect,
                    TargetName = p.RoleId != null ? p.Role!.Name : (p.User != null ? p.User.FirstName + " " + p.User.LastName : null),
                    TargetType = p.RoleId != null ? "Role" : "User",
                    Priority = p.Priority,
                    IsActive = p.IsActive,
                    ExpiresAt = p.ExpiresAt,
                    ConditionCount = p.Conditions.Count(c => !c.IsDeleted),
                    FieldRestrictionCount = p.FieldRestrictions.Count(f => !f.IsDeleted)
                },
                paginationDTO: paginationDto,
                where: q =>
                {
                    q = q.Where(p => !p.IsDeleted);

                    if (!string.IsNullOrEmpty(request.Search))
                    {
                        string s = request.Search.ToLower();
                        q = q.Where(p => p.Name.ToLower().Contains(s)
                            || (p.Description != null && p.Description.ToLower().Contains(s)));
                    }

                    if (request.ModuleId.HasValue)
                        q = q.Where(p => p.ModuleId == request.ModuleId.Value);

                    if (request.RoleId.HasValue)
                        q = q.Where(p => p.RoleId == request.RoleId.Value);

                    if (request.UserId.HasValue)
                        q = q.Where(p => p.UserId == request.UserId.Value);

                    if (!string.IsNullOrEmpty(request.Effect))
                        q = q.Where(p => p.Effect == request.Effect);

                    if (request.IsActive.HasValue)
                        q = q.Where(p => p.IsActive == request.IsActive.Value);

                    return q.OrderBy(p => p.Priority).ThenBy(p => p.Name);
                }
            );

            var responseResult = PaginatedResult<AccessPolicyListDto>.Create(
                result.Records,
                result.TotalRecords,
                result.PageNumber,
                result.ItemsPerPage
            );

            return ApiResponse<PaginatedResult<AccessPolicyListDto>>.Success(
                responseResult, "Access policies retrieved successfully.");
        }
    }
}
