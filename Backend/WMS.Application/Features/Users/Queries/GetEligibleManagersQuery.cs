using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WMS.Application.Common.Attributes;
using WMS.Application.Common.Constants;
using WMS.Application.Common.Models;
using WMS.Application.Features.Users.DTOs;
using WMS.Domain.Interfaces;

namespace WMS.Application.Features.Users.Queries
{
    [RequirePermission(PermissionCodes.UserRead)]
    public class GetEligibleManagersQuery : IRequest<ApiResponse<List<EligibleUserDto>>>
    {
        public Guid DepartmentId { get; set; }
    }

    public class GetEligibleManagersQueryHandler(IUnitOfWork unitOfWork)
        : IRequestHandler<GetEligibleManagersQuery, ApiResponse<List<EligibleUserDto>>>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;

        public async Task<ApiResponse<List<EligibleUserDto>>> Handle(
            GetEligibleManagersQuery request, CancellationToken cancellationToken)
        {
            var managers = await _unitOfWork.User.GetEligibleManagersAsync(
                request.DepartmentId, cancellationToken);

            var eligibleManagers = managers
                .Select(u => new EligibleUserDto
                {
                    Id = u.Id,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    DesignationName = u.Designation.Name,
                    DesignationLevel = u.Designation.Level
                })
                .ToList();

            return ApiResponse<List<EligibleUserDto>>.Success(eligibleManagers);
        }
    }
}
