using MediatR;
using WMS.Application.Common.Attributes;
using WMS.Application.Common.Constants;
using WMS.Application.Common.Interfaces;
using WMS.Application.Common.Models;
using WMS.Application.Common.Policies;
using WMS.Application.Features.Users.DTOs;
using WMS.Domain.Interfaces;

namespace WMS.Application.Features.Users.Queries
{
    [RequirePermission(PermissionCodes.UserRead)]
    public class GetUsersQuery : IRequest<ApiResponse<PaginatedResult<UserListDto>>>
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? Search { get; set; }
        public string? Role { get; set; }
        public bool? IsActive { get; set; }
        public Guid? TeamId { get; set; }
        public Guid? DepartmentId { get; set; }
    }
    public class GetUsersQueryHandler(
        IUnitOfWork unitOfWork,
        IPolicyEvaluationService policyEvaluationService,
        ICurrentUserService currentUserService)
        : IRequestHandler<GetUsersQuery, ApiResponse<PaginatedResult<UserListDto>>>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly IPolicyEvaluationService _policyEvaluationService = policyEvaluationService;
        private readonly ICurrentUserService _currentUserService = currentUserService;

        public async Task<ApiResponse<PaginatedResult<UserListDto>>> Handle(
            GetUsersQuery request, CancellationToken cancellationToken)
        {
            var currentUserId = _currentUserService.UserId;
            var currentUserRoles = _currentUserService.Roles;

            var (usePbac, pbacFilter) = await PbacRowScope.ResolveAsync<WMS.Domain.Models.User>(
                _policyEvaluationService, currentUserId, "USER_MGMT", "read");

            WMS.Domain.Common.Models.PaginationDTO<UserListDto> paginationDto = new()
            {
                PageNumber = request.PageNumber,
                ItemsPerPage = request.PageSize,
                Search = request.Search
            };
            WMS.Domain.Common.Models.PaginationDTO<UserListDto> result = await _unitOfWork.User.GetPaginatedList(
                select: u => new UserListDto()
                {
                    Id = u.Id,
                    EmployeeCode = u.EmployeeCode,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    Email = u.Email,
                    PhoneNumber = u.PhoneNumber,
                    IsActive = u.IsActive,
                    DepartmentName = u.Department.Name,
                    DesignationName = u.Designation.Name,
                    Roles = u.UserRoles.Select(ur => ur.Role.Name).ToList()
                },
                paginationDto,
                where: query =>
                {
                    if (usePbac && pbacFilter != null)
                    {
                        query = query.Where(pbacFilter);
                    }

                    if (!string.IsNullOrWhiteSpace(request.Search))
                    {
                        string search = request.Search.ToLower();
                        query = query.Where(u =>
                            u.FirstName.ToLower().Contains(search) ||
                            u.LastName.ToLower().Contains(search) ||
                            u.Email.ToLower().Contains(search) ||
                            u.EmployeeCode.ToLower().Contains(search));
                    }
                    if (!string.IsNullOrWhiteSpace(request.Role))
                    {
                        query = query.Where(u =>
                            u.UserRoles.Any(ur => ur.Role.Name.ToLower() == request.Role.ToLower()));
                    }
                    if (request.IsActive.HasValue)
                    {
                        query = query.Where(u => u.IsActive == request.IsActive.Value);
                    }
                    if (request.TeamId.HasValue)
                    {
                        query = query.Where(u => u.TeamId == request.TeamId.Value);
                    }
                    if (request.DepartmentId.HasValue)
                    {
                        query = query.Where(u => u.DepartmentId == request.DepartmentId.Value);
                    }

                    return query;
                }
            );
            PaginatedResult<UserListDto> paginatedResult = PaginatedResult<UserListDto>.Create(
                result.Records,
                result.TotalRecords,
                result.PageNumber,
                result.ItemsPerPage
            );
            return ApiResponse<PaginatedResult<UserListDto>>.Success(paginatedResult);
        }
    }
}
