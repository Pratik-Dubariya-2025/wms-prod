using MediatR;
using WMS.Application.Common.Attributes;
using WMS.Application.Common.Constants;
using WMS.Application.Common.Models;
using WMS.Application.Features.Policies.DTOs;
using WMS.Domain.Interfaces;

namespace WMS.Application.Features.Policies.Queries
{
    /// <summary>
    /// Returns all modules with their available CRUD actions.
    /// Used by the frontend Policy Builder dropdown.
    /// </summary>
    [RequirePermission(PermissionCodes.PolicyRead)]
    public class GetModulesWithActionsQuery : IRequest<ApiResponse<List<ModuleWithActionsDto>>>
    {
    }

    public class GetModulesWithActionsQueryHandler(IUnitOfWork unitOfWork)
        : IRequestHandler<GetModulesWithActionsQuery, ApiResponse<List<ModuleWithActionsDto>>>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;

        public async Task<ApiResponse<List<ModuleWithActionsDto>>> Handle(
            GetModulesWithActionsQuery request, CancellationToken cancellationToken)
        {
            // Get all active modules
            var modules = await _unitOfWork.Module.GetAllAsync(m => !m.IsDeleted && m.IsActive);

            // Get all permissions to derive available actions per module
            var permissions = await _unitOfWork.Permission.GetAllAsync(
                p => new { p.ModuleId, p.Action },
                p => !p.IsDeleted);

            var permissionsByModule = permissions
                .GroupBy(p => p.ModuleId)
                .ToDictionary(g => g.Key, g => g.Select(p => p.Action).Distinct().OrderBy(a => a).ToList());

            var result = modules
                .OrderBy(m => m.DisplayOrder)
                .Select(m => new ModuleWithActionsDto
                {
                    Id = m.Id,
                    Code = m.Code,
                    Name = m.Name,
                    AvailableActions = permissionsByModule.TryGetValue(m.Id, out var actions)
                        ? actions
                        : ["Create", "Read", "Update", "Delete"]
                })
                .ToList();

            return ApiResponse<List<ModuleWithActionsDto>>.Success(
                result, "Modules with actions retrieved successfully.");
        }
    }
}
