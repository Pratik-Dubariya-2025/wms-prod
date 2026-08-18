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
    [RequirePermission(PermissionCodes.DepartmentUpdate)]
    public class UpdateDesignationCommand : IRequest<ApiResponse<bool>>
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public string Code { get; set; } = null!;
        public string? Description { get; set; }
        public int Level { get; set; } = 1;
        public bool IsActive { get; set; } = true;
    }

    public class UpdateDesignationCommandHandler(IUnitOfWork unitOfWork)
        : IRequestHandler<UpdateDesignationCommand, ApiResponse<bool>>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;

        public async Task<ApiResponse<bool>> Handle(UpdateDesignationCommand request, CancellationToken cancellationToken)
        {
            var designation = await _unitOfWork.Designation.GetFirstOrDefaultAsync(d => d.Id == request.Id && !d.IsDeleted);
            if (designation == null)
            {
                return ApiResponse<bool>.Failure("Designation not found.", new Dictionary<string, string[]>
                {
                    { "Id", ["Designation not found."] }
                }, 404);
            }

            var existingCode = await _unitOfWork.Designation.GetFirstOrDefaultAsync(
                d => d.DepartmentId == designation.DepartmentId && d.Id != request.Id && d.Code.ToLower() == request.Code.ToLower() && !d.IsDeleted);

            if (existingCode != null)
            {
                return ApiResponse<bool>.Failure("Validation failed.", new Dictionary<string, string[]>
                {
                    { "Code", ["A designation with this code already exists in this department."] }
                }, 400);
            }

            designation.Name = request.Name.Trim();
            designation.Code = request.Code.Trim().ToUpper();
            designation.Description = request.Description?.Trim();
            designation.Level = request.Level > 0 ? request.Level : 1;
            designation.IsActive = request.IsActive;

            _unitOfWork.Designation.Update(designation);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return ApiResponse<bool>.Success(true, "Designation updated successfully.");
        }
    }
}
