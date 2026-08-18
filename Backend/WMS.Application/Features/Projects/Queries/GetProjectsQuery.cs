using MediatR;
using WMS.Application.Common.Attributes;
using WMS.Application.Common.Constants;
using WMS.Application.Common.Interfaces;
using WMS.Application.Common.Models;
using WMS.Application.Common.Policies;
using WMS.Application.Features.Projects.DTOs;
using WMS.Domain.Interfaces;

namespace WMS.Application.Features.Projects.Queries
{
    [RequirePermission(PermissionCodes.ProjectRead)]
    public class GetProjectsQuery : IRequest<ApiResponse<PaginatedResult<ProjectListDto>>>
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? Search { get; set; }
        public string? Status { get; set; }
    }

    public class GetProjectsQueryHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        IPolicyEvaluationService policyEvaluationService)
        : IRequestHandler<GetProjectsQuery, ApiResponse<PaginatedResult<ProjectListDto>>>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly ICurrentUserService _currentUserService = currentUserService;
        private readonly IPolicyEvaluationService _policyEvaluationService = policyEvaluationService;

        public async Task<ApiResponse<PaginatedResult<ProjectListDto>>> Handle(
            GetProjectsQuery request, CancellationToken cancellationToken)
        {
            var currentUserId = _currentUserService.UserId;
            var currentUserRoles = _currentUserService.Roles;

            var (usePbac, pbacFilter) = await PbacRowScope.ResolveAsync<WMS.Domain.Models.Project>(
                _policyEvaluationService, currentUserId, "PROJECT_MGMT", "read");

            WMS.Domain.Common.Models.PaginationDTO<ProjectListDto> paginationDto = new()
            {
                PageNumber = request.PageNumber,
                ItemsPerPage = request.PageSize,
                Search = request.Search
            };

            WMS.Domain.Common.Models.PaginationDTO<ProjectListDto> result = await _unitOfWork.Project.GetPaginatedList(
                select: p => new ProjectListDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Status = p.Status,
                    Description = p.Description,
                    StartDate = p.StartDate,
                    EndDate = p.EndDate,
                    OwnerName = p.Owner.FirstName + " " + p.Owner.LastName,
                    TeamLeadName = p.TeamLead != null
                        ? p.TeamLead.FirstName + " " + p.TeamLead.LastName
                        : null,
                    DepartmentName = p.Department.Name,
                    MemberCount = p.Members.Count(m => !m.IsDeleted),
                    TaskCount = p.Tasks.Count(t => !t.IsDeleted),
                    CreatedAt = p.CreatedAt
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
                    else if (currentUserId.HasValue && !currentUserRoles.Contains("ADMIN"))
                    {
                        var userId = currentUserId.Value;
                        query = query.Where(p =>
                            p.OwnerId == userId ||
                            p.TeamLeadId == userId ||
                            p.Members.Any(m => m.UserId == userId && !m.IsDeleted)
                        );
                    }

                    // Text search
                    if (!string.IsNullOrWhiteSpace(request.Search))
                    {
                        string search = request.Search.ToLower();
                        query = query.Where(p =>
                            p.Name.ToLower().Contains(search) ||
                            (p.Description != null && p.Description.ToLower().Contains(search)));
                    }

                    // Status filter
                    if (!string.IsNullOrWhiteSpace(request.Status))
                    {
                        query = query.Where(p => p.Status == request.Status);
                    }

                    return query;
                }
            );

            PaginatedResult<ProjectListDto> paginatedResult = PaginatedResult<ProjectListDto>.Create(
                result.Records,
                result.TotalRecords,
                result.PageNumber,
                result.ItemsPerPage
            );

            return ApiResponse<PaginatedResult<ProjectListDto>>.Success(paginatedResult);
        }
    }
}
