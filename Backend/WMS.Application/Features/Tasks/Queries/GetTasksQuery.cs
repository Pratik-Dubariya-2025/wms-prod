using MediatR;
using WMS.Application.Common.Attributes;
using WMS.Application.Common.Constants;
using WMS.Application.Common.Models;
using WMS.Application.Common.Policies;
using WMS.Application.Features.Tasks.DTOs;
using WMS.Application.Common.Interfaces;
using WMS.Domain.Interfaces;

namespace WMS.Application.Features.Tasks.Queries
{
    [RequirePermission(PermissionCodes.TaskRead)]
    public class GetTasksQuery : IRequest<ApiResponse<PaginatedResult<TaskListDto>>>
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? Search { get; set; }
        public string? Status { get; set; }
        public string? Priority { get; set; }
        public Guid? ProjectId { get; set; }
        public Guid? AssigneeId { get; set; }
    }

    public class GetTasksQueryHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        IPolicyEvaluationService policyEvaluationService)
        : IRequestHandler<GetTasksQuery, ApiResponse<PaginatedResult<TaskListDto>>>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly ICurrentUserService _currentUserService = currentUserService;
        private readonly IPolicyEvaluationService _policyEvaluationService = policyEvaluationService;

        public async Task<ApiResponse<PaginatedResult<TaskListDto>>> Handle(
            GetTasksQuery request, CancellationToken cancellationToken)
        {
            var currentUserId = _currentUserService.UserId;
            bool isAdmin = _currentUserService.Roles.Contains("ADMIN");

            var (usePbac, pbacFilter) = await PbacRowScope.ResolveAsync<WMS.Domain.Models.TaskItem>(
                _policyEvaluationService, currentUserId, "TASK_MGMT", "read");

            WMS.Domain.Common.Models.PaginationDTO<TaskListDto> paginationDto = new()
            {
                PageNumber = request.PageNumber,
                ItemsPerPage = request.PageSize,
                Search = request.Search
            };

            WMS.Domain.Common.Models.PaginationDTO<TaskListDto> result = await _unitOfWork.TaskItem.GetPaginatedList(
                select: t => new TaskListDto
                {
                    Id = t.Id,
                    Title = t.Title,
                    Status = t.Status,
                    Priority = t.Priority,
                    DueDate = t.DueDate,
                    EstimatedHours = t.EstimatedHours,
                    ProjectName = t.Project.Name,
                    TeamName = t.Team.Name,
                    AssigneeName = t.Assignee != null
                        ? t.Assignee.FirstName + " " + t.Assignee.LastName
                        : null,
                    AssigneeId = t.AssigneeId,
                    CreatedByName = t.CreatedByUser.FirstName + " " + t.CreatedByUser.LastName,
                    CreatedAt = t.CreatedAt
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
                    else if (!isAdmin && currentUserId.HasValue)
                    {
                        var userId = currentUserId.Value;
                        query = query.Where(t =>
                            t.CreatedById == userId ||
                            t.AssigneeId == userId ||
                            (t.Project != null && (t.Project.OwnerId == userId || t.Project.TeamLeadId == userId || t.Project.Members.Any(m => m.UserId == userId && !m.IsDeleted)))
                        );
                    }

                    // Text search across title
                    if (!string.IsNullOrWhiteSpace(request.Search))
                    {
                        string search = request.Search.ToLower();
                        query = query.Where(t =>
                            t.Title.ToLower().Contains(search) ||
                            (t.Description != null && t.Description.ToLower().Contains(search)));
                    }

                    // Status filter
                    if (!string.IsNullOrWhiteSpace(request.Status))
                    {
                        query = query.Where(t => t.Status == request.Status);
                    }

                    // Priority filter
                    if (!string.IsNullOrWhiteSpace(request.Priority))
                    {
                        query = query.Where(t => t.Priority == request.Priority);
                    }

                    // Project filter
                    if (request.ProjectId.HasValue)
                    {
                        query = query.Where(t => t.ProjectId == request.ProjectId.Value);
                    }

                    // Assignee filter
                    if (request.AssigneeId.HasValue)
                    {
                        query = query.Where(t => t.AssigneeId == request.AssigneeId.Value);
                    }

                    return query;
                }
            );

            PaginatedResult<TaskListDto> paginatedResult = PaginatedResult<TaskListDto>.Create(
                result.Records,
                result.TotalRecords,
                result.PageNumber,
                result.ItemsPerPage
            );

            return ApiResponse<PaginatedResult<TaskListDto>>.Success(paginatedResult);
        }
    }
}
