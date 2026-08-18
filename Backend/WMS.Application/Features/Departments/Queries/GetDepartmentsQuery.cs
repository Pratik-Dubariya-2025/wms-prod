using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WMS.Application.Common.Attributes;
using WMS.Application.Common.Constants;
using WMS.Application.Common.Interfaces;
using WMS.Application.Common.Models;
using WMS.Application.Common.Policies;
using WMS.Application.Features.Departments.DTOs;
using WMS.Domain.Interfaces;

namespace WMS.Application.Features.Departments.Queries
{
    [RequirePermission(PermissionCodes.DepartmentRead)]
    public class GetDepartmentsQuery : IRequest<ApiResponse<List<DepartmentListDto>>>
    {
        public string? Search { get; set; }
        public string? Status { get; set; }
        public bool? IsActiveOnly { get; set; }
    }

    public class GetDepartmentsQueryHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        IPolicyEvaluationService policyEvaluationService)
        : IRequestHandler<GetDepartmentsQuery, ApiResponse<List<DepartmentListDto>>>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly ICurrentUserService _currentUserService = currentUserService;
        private readonly IPolicyEvaluationService _policyEvaluationService = policyEvaluationService;

        public async Task<ApiResponse<List<DepartmentListDto>>> Handle(
            GetDepartmentsQuery request, CancellationToken cancellationToken)
        {
            var departments = await _unitOfWork.Department.IncludeAndGetAllAsync(
                d => !d.IsDeleted,
                d => d.Users,
                d => d.Designations
            );

            IEnumerable<Domain.Models.Department> filtered = departments;

            var (_, pbacFilter) = await PbacRowScope.ResolveAsync<Domain.Models.Department>(
                _policyEvaluationService, _currentUserService.UserId, "DEPT_MGMT", "read");
            if (pbacFilter != null)
            {
                var compiled = pbacFilter.Compile();
                filtered = filtered.Where(compiled);
            }

            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                string search = request.Search.Trim().ToLower();
                filtered = filtered.Where(d =>
                    d.Name.ToLower().Contains(search) ||
                    d.Code.ToLower().Contains(search) ||
                    (d.Description != null && d.Description.ToLower().Contains(search)));
            }

            if (!string.IsNullOrWhiteSpace(request.Status))
            {
                if (request.Status.Equals("active", StringComparison.OrdinalIgnoreCase))
                {
                    filtered = filtered.Where(d => d.IsActive);
                }
                else if (request.Status.Equals("inactive", StringComparison.OrdinalIgnoreCase))
                {
                    filtered = filtered.Where(d => !d.IsActive);
                }
            }
            else if (request.IsActiveOnly.HasValue && request.IsActiveOnly.Value)
            {
                filtered = filtered.Where(d => d.IsActive);
            }

            var dtos = filtered
                .OrderBy(d => d.Name)
                .Select(d => new DepartmentListDto
                {
                    Id = d.Id,
                    Name = d.Name,
                    Code = d.Code,
                    Description = d.Description,
                    IsActive = d.IsActive,
                    MemberCount = d.Users.Count(u => !u.IsDeleted && u.IsActive),
                    DesignationCount = d.Designations.Count(des => !des.IsDeleted && des.IsActive),
                    CreatedAt = d.CreatedAt
                })
                .ToList();

            return ApiResponse<List<DepartmentListDto>>.Success(dtos);
        }
    }
}
