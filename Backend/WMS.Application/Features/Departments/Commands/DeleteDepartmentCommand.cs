using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;
using WMS.Application.Common.Attributes;
using WMS.Application.Common.Constants;
using WMS.Application.Common.Models;
using WMS.Domain.Interfaces;

namespace WMS.Application.Features.Departments.Commands
{
    [RequirePermission(PermissionCodes.DepartmentDelete)]
    public class DeleteDepartmentCommand : IRequest<ApiResponse<bool>>
    {
        public Guid Id { get; set; }
    }

    public class DeleteDepartmentCommandHandler(IUnitOfWork unitOfWork)
        : IRequestHandler<DeleteDepartmentCommand, ApiResponse<bool>>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;

        public async Task<ApiResponse<bool>> Handle(
            DeleteDepartmentCommand request, CancellationToken cancellationToken)
        {
            var department = await _unitOfWork.Department.GetFirstOrDefaultAsync(d => d.Id == request.Id);
            if (department == null || department.IsDeleted)
            {
                return ApiResponse<bool>.Failure("Department not found.", statusCode: 404);
            }

            // Check if active users are assigned to this department
            var assignedUser = await _unitOfWork.User.GetFirstOrDefaultAsync(
                u => u.DepartmentId == request.Id && !u.IsDeleted
            );

            if (assignedUser != null)
            {
                return ApiResponse<bool>.Failure(
                    "Cannot delete department because active users are assigned to it. Please reassign the users first.",
                    statusCode: 400
                );
            }

            department.IsDeleted = true;
            _unitOfWork.Department.Update(department);
            await _unitOfWork.SaveChangesAsync();

            return ApiResponse<bool>.Success(true, "Department deleted successfully.");
        }
    }
}
