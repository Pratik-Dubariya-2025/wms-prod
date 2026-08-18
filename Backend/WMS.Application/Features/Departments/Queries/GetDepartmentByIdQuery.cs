using MediatR;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WMS.Application.Common.Attributes;
using WMS.Application.Common.Constants;
using WMS.Application.Common.Models;
using WMS.Application.Features.Departments.DTOs;
using WMS.Domain.Interfaces;

namespace WMS.Application.Features.Departments.Queries
{
    [RequirePermission(PermissionCodes.DepartmentRead)]
    public class GetDepartmentByIdQuery : IRequest<ApiResponse<DepartmentDetailDto>>
    {
        public Guid Id { get; set; }
    }

    public class GetDepartmentByIdQueryHandler(IUnitOfWork unitOfWork)
        : IRequestHandler<GetDepartmentByIdQuery, ApiResponse<DepartmentDetailDto>>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;

        public async Task<ApiResponse<DepartmentDetailDto>> Handle(
            GetDepartmentByIdQuery request, CancellationToken cancellationToken)
        {
            var department = await _unitOfWork.Department.IncludeAndGetFirstOrDefaultAsync<object>(
                d => d.Id == request.Id && !d.IsDeleted,
                d => d.Users,
                d => d.Designations
            );

            if (department == null)
            {
                return ApiResponse<DepartmentDetailDto>.Failure("Department not found.", statusCode: 404);
            }

            // Get users with details
            var departmentUsers = await _unitOfWork.User.IncludeAndGetAllAsync(
                u => u.DepartmentId == request.Id && !u.IsDeleted,
                u => u.Designation,
                u => u.UserRoles
            );

            var userDtos = departmentUsers
                .OrderBy(u => u.FirstName)
                .Select(u => new DepartmentUserDto
                {
                    Id = u.Id,
                    EmployeeCode = u.EmployeeCode,
                    FullName = $"{u.FirstName} {u.LastName}".Trim(),
                    Email = u.Email,
                    DesignationName = u.Designation?.Name,
                    RoleName = string.Join(", ", u.UserRoles.Where(ur => ur.Role != null).Select(ur => ur.Role.Name)),
                    IsActive = u.IsActive
                })
                .ToList();

            var designationDtos = department.Designations
                .Where(des => !des.IsDeleted)
                .OrderBy(des => des.Level)
                .ThenBy(des => des.Name)
                .Select(des => new DepartmentDesignationDto
                {
                    Id = des.Id,
                    Name = des.Name,
                    Level = des.Level,
                    IsActive = des.IsActive
                })
                .ToList();

            var detailDto = new DepartmentDetailDto
            {
                Id = department.Id,
                Name = department.Name,
                Code = department.Code,
                Description = department.Description,
                IsActive = department.IsActive,
                CreatedAt = department.CreatedAt,
                Users = userDtos,
                Designations = designationDtos
            };

            return ApiResponse<DepartmentDetailDto>.Success(detailDto);
        }
    }
}
