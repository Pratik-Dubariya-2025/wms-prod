using MediatR;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using WMS.Application.Common.Attributes;
using WMS.Application.Common.Constants;
using WMS.Application.Common.Models;
using WMS.Domain.Interfaces;
using WMS.Domain.Models;

namespace WMS.Application.Features.Departments.Commands
{
    [RequirePermission(PermissionCodes.DepartmentCreate)]
    public class CreateDesignationCommand : IRequest<ApiResponse<Guid>>
    {
        public Guid DepartmentId { get; set; }
        public string Name { get; set; } = null!;
        public string Code { get; set; } = null!;
        public string? Description { get; set; }
        public int Level { get; set; } = 1;
    }

    public class CreateDesignationCommandHandler(IUnitOfWork unitOfWork)
        : IRequestHandler<CreateDesignationCommand, ApiResponse<Guid>>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;

        public async Task<ApiResponse<Guid>> Handle(CreateDesignationCommand request, CancellationToken cancellationToken)
        {
            var dept = await _unitOfWork.Department.GetFirstOrDefaultAsync(d => d.Id == request.DepartmentId && !d.IsDeleted);
            if (dept == null)
            {
                return ApiResponse<Guid>.Failure("Department not found.", new Dictionary<string, string[]>
                {
                    { "DepartmentId", ["Department does not exist."] }
                }, 404);
            }

            var existingCode = await _unitOfWork.Designation.GetFirstOrDefaultAsync(
                d => d.DepartmentId == request.DepartmentId && d.Code.ToLower() == request.Code.ToLower() && !d.IsDeleted);

            if (existingCode != null)
            {
                return ApiResponse<Guid>.Failure("Validation failed.", new Dictionary<string, string[]>
                {
                    { "Code", ["A designation with this code already exists in this department."] }
                }, 400);
            }

            var designation = new Designation
            {
                Id = Guid.NewGuid(),
                DepartmentId = request.DepartmentId,
                Name = request.Name.Trim(),
                Code = request.Code.Trim().ToUpper(),
                Description = request.Description?.Trim(),
                Level = request.Level > 0 ? request.Level : 1,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Designation.AddAsync(designation);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return ApiResponse<Guid>.Success(designation.Id, "Designation created successfully.", 201);
        }
    }
}
