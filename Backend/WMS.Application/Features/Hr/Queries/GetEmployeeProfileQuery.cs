using MediatR;
using WMS.Application.Common.Attributes;
using WMS.Application.Common.Constants;
using WMS.Application.Common.Models;
using WMS.Application.Features.Hr.DTOs;
using WMS.Domain.Interfaces;

namespace WMS.Application.Features.Hr.Queries
{
    /// <summary>
    /// GET /api/hr/profiles/{userId}
    /// Returns extended HR profile (salary, bank details, etc.).
    /// Permission: salary.read (HR and Admin only per SRS).
    /// </summary>
    [RequirePermission(PermissionCodes.SalaryRead)]
    public class GetEmployeeProfileQuery : IRequest<ApiResponse<EmployeeProfileDto>>
    {
        public Guid UserId { get; set; }
    }

    public class GetEmployeeProfileQueryHandler(IUnitOfWork unitOfWork)
        : IRequestHandler<GetEmployeeProfileQuery, ApiResponse<EmployeeProfileDto>>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;

        public async Task<ApiResponse<EmployeeProfileDto>> Handle(
            GetEmployeeProfileQuery request, CancellationToken cancellationToken)
        {
            // Validate user exists
            var user = await _unitOfWork.User.GetFirstOrDefaultAsync(
                u => u.Id == request.UserId && !u.IsDeleted);
            if (user == null)
            {
                return ApiResponse<EmployeeProfileDto>.Failure("User not found.", null, 404);
            }

            // Check if profile exists
            var profile = await _unitOfWork.EmployeeProfile.GetFirstOrDefaultAsync(
                select: ep => new EmployeeProfileDto
                {
                    Id = ep.Id,
                    UserId = ep.UserId,
                    UserName = ep.User.FirstName + " " + ep.User.LastName,
                    UserEmail = ep.User.Email,
                    EmployeeCode = ep.User.EmployeeCode,
                    Salary = ep.Salary,
                    BankAccountNo = ep.BankAccountNo,
                    BankIfsc = ep.BankIfsc,
                    PanNumber = ep.PanNumber,
                    AadhaarLast4 = ep.AadhaarLast4,
                    EmergencyContactName = ep.EmergencyContactName,
                    EmergencyContactPhone = ep.EmergencyContactPhone,
                    BloodGroup = ep.BloodGroup,
                    Address = ep.Address,
                    CreatedAt = ep.CreatedAt,
                    ModifiedBy = ep.ModifiedBy,
                    ModifiedAt = ep.ModifiedAt
                },
                where: ep => ep.UserId == request.UserId && !ep.IsDeleted
            );

            if (profile == null)
            {
                // Return an empty profile for the user (profile not yet created)
                return ApiResponse<EmployeeProfileDto>.Success(new EmployeeProfileDto
                {
                    UserId = request.UserId,
                    UserName = user.FirstName + " " + user.LastName,
                    UserEmail = user.Email,
                    EmployeeCode = user.EmployeeCode,
                    Salary = 0
                }, "No HR profile exists yet for this user. You can create one by updating.");
            }

            return ApiResponse<EmployeeProfileDto>.Success(profile);
        }
    }
}
