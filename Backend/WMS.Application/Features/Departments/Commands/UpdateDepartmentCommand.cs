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
    [RequirePermission(PermissionCodes.DepartmentUpdate)]
    public class UpdateDepartmentCommand : IRequest<ApiResponse<bool>>
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public string Code { get; set; } = null!;
        public string? Description { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class UpdateDepartmentCommandHandler(IUnitOfWork unitOfWork)
        : IRequestHandler<UpdateDepartmentCommand, ApiResponse<bool>>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;

        public async Task<ApiResponse<bool>> Handle(
            UpdateDepartmentCommand request, CancellationToken cancellationToken)
        {
            var department = await _unitOfWork.Department.GetFirstOrDefaultAsync(d => d.Id == request.Id);
            if (department == null || department.IsDeleted)
            {
                return ApiResponse<bool>.Failure("Department not found.", statusCode: 404);
            }

            string cleanCode = request.Code.Trim().ToUpperInvariant();
            string cleanName = request.Name.Trim();

            var existingCode = await _unitOfWork.Department.GetFirstOrDefaultAsync(
                d => d.Id != request.Id && !d.IsDeleted && d.Code.ToUpper() == cleanCode
            );

            if (existingCode != null)
            {
                return ApiResponse<bool>.Failure($"Department code '{cleanCode}' is already in use.", statusCode: 400);
            }

            var existingName = await _unitOfWork.Department.GetFirstOrDefaultAsync(
                d => d.Id != request.Id && !d.IsDeleted && d.Name.ToLower() == cleanName.ToLower()
            );

            if (existingName != null)
            {
                return ApiResponse<bool>.Failure($"Department name '{cleanName}' already exists.", statusCode: 400);
            }

            department.Name = cleanName;
            department.Code = cleanCode;
            department.Description = request.Description?.Trim();
            department.IsActive = request.IsActive;

            _unitOfWork.Department.Update(department);
            await _unitOfWork.SaveChangesAsync();

            return ApiResponse<bool>.Success(true, "Department updated successfully.");
        }
    }
}
