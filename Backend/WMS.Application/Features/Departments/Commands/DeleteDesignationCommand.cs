using MediatR;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using WMS.Application.Common.Attributes;
using WMS.Application.Common.Constants;
using WMS.Application.Common.Models;
using WMS.Domain.Interfaces;

namespace WMS.Application.Features.Departments.Commands
{
    [RequirePermission(PermissionCodes.DepartmentDelete)]
    public class DeleteDesignationCommand : IRequest<ApiResponse<bool>>
    {
        public Guid Id { get; set; }
    }

    public class DeleteDesignationCommandHandler(IUnitOfWork unitOfWork)
        : IRequestHandler<DeleteDesignationCommand, ApiResponse<bool>>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;

        public async Task<ApiResponse<bool>> Handle(DeleteDesignationCommand request, CancellationToken cancellationToken)
        {
            var designation = await _unitOfWork.Designation.GetFirstOrDefaultAsync(d => d.Id == request.Id && !d.IsDeleted);
            if (designation == null)
            {
                return ApiResponse<bool>.Failure("Designation not found.", new Dictionary<string, string[]>
                {
                    { "Id", ["Designation not found."] }
                }, 404);
            }

            var assignedUsers = await _unitOfWork.User.GetAllAsync(u => u.DesignationId == request.Id && !u.IsDeleted);
            if (assignedUsers.Count > 0)
            {
                return ApiResponse<bool>.Failure("Cannot delete designation with assigned users.", new Dictionary<string, string[]>
                {
                    { "Designation", [$"Cannot delete designation '{designation.Name}'. It has {assignedUsers.Count} active user(s) assigned to it."] }
                }, 400);
            }

            designation.IsDeleted = true;
            _unitOfWork.Designation.Update(designation);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return ApiResponse<bool>.Success(true, "Designation deleted successfully.");
        }
    }
}
