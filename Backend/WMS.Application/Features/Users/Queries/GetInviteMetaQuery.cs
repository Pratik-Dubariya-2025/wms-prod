using MediatR;
using System.Threading;
using System.Threading.Tasks;
using WMS.Application.Common.Attributes;
using WMS.Application.Common.Constants;
using WMS.Application.Common.Models;
using WMS.Application.Features.Users.DTOs;
using WMS.Application.Features.Roles.DTOs;
using WMS.Domain.Interfaces;

namespace WMS.Application.Features.Users.Queries
{
    [RequirePermission(PermissionCodes.UserRead)]
    public class GetInviteMetaQuery : IRequest<ApiResponse<InviteMetaDto>>
    {
    }

    public class GetInviteMetaQueryHandler(IUnitOfWork unitOfWork) 
        : IRequestHandler<GetInviteMetaQuery, ApiResponse<InviteMetaDto>>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;

        public async Task<ApiResponse<InviteMetaDto>> Handle(GetInviteMetaQuery request, CancellationToken cancellationToken)
        {
            var departmentsWithDesignations = await _unitOfWork.Department.IncludeAndGetAllAsync(
                d => !d.IsDeleted && d.IsActive,
                d => d.Designations
            );

            var departments = departmentsWithDesignations
                .Select(d => new DepartmentDto { Id = d.Id, Name = d.Name })
                .ToList();

            var designations = departmentsWithDesignations
                .SelectMany(d => d.Designations
                    .Where(des => !des.IsDeleted && des.IsActive)
                    .Select(des => new DesignationDto { Id = des.Id, DepartmentId = des.DepartmentId, Name = des.Name, Level = des.Level }))
                .ToList();

            var roles = await _unitOfWork.Role.GetAllAsync(
                r => new RoleDto { Id = r.Id, Name = r.Name, Code = r.Code, Priority = r.Priority, Description = r.Description },
                r => !r.IsDeleted
            );

            var metaDto = new InviteMetaDto
            {
                Departments = departments,
                Designations = designations,
                Roles = roles
            };

            return ApiResponse<InviteMetaDto>.Success(metaDto);
        }
    }
}
