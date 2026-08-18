using MediatR;
using System;
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
    public class CreateDepartmentCommand : IRequest<ApiResponse<Guid>>
    {
        public string Name { get; set; } = null!;
        public string Code { get; set; } = null!;
        public string? Description { get; set; }
    }

    public class CreateDepartmentCommandHandler(IUnitOfWork unitOfWork)
        : IRequestHandler<CreateDepartmentCommand, ApiResponse<Guid>>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;

        public async Task<ApiResponse<Guid>> Handle(
            CreateDepartmentCommand request, CancellationToken cancellationToken)
        {
            string cleanCode = request.Code.Trim().ToUpperInvariant();
            string cleanName = request.Name.Trim();

            var existingCode = await _unitOfWork.Department.GetFirstOrDefaultAsync(
                d => !d.IsDeleted && d.Code.ToUpper() == cleanCode
            );

            if (existingCode != null)
            {
                return ApiResponse<Guid>.Failure($"Department code '{cleanCode}' is already in use.", statusCode: 400);
            }

            var existingName = await _unitOfWork.Department.GetFirstOrDefaultAsync(
                d => !d.IsDeleted && d.Name.ToLower() == cleanName.ToLower()
            );

            if (existingName != null)
            {
                return ApiResponse<Guid>.Failure($"Department name '{cleanName}' already exists.", statusCode: 400);
            }

            var department = new Department
            {
                Id = Guid.NewGuid(),
                Name = cleanName,
                Code = cleanCode,
                Description = request.Description?.Trim(),
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Department.AddAsync(department);
            await _unitOfWork.SaveChangesAsync();

            return ApiResponse<Guid>.Success(department.Id, "Department created successfully.", 201);
        }
    }
}
