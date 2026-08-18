using MediatR;
using WMS.Application.Common.Attributes;
using WMS.Application.Common.Constants;
using WMS.Application.Common.Interfaces;
using WMS.Application.Common.Models;
using WMS.Domain.Interfaces;
using WMS.Domain.Models;

namespace WMS.Application.Features.Hr.Commands
{
    /// <summary>
    /// PUT /api/hr/profiles/{userId}
    /// Create or update extended HR profile (salary, bank details, etc.).
    /// Permission: salary.write (HR and Admin only per SRS).
    /// </summary>
    [RequirePermission(PermissionCodes.SalaryWrite)]
    public class UpdateEmployeeProfileCommand : IRequest<ApiResponse<bool>>
    {
        public Guid UserId { get; set; }
        public decimal Salary { get; set; }
        public string? BankAccountNo { get; set; }
        public string? BankIfsc { get; set; }
        public string? PanNumber { get; set; }
        public string? AadhaarLast4 { get; set; }
        public string? EmergencyContactName { get; set; }
        public string? EmergencyContactPhone { get; set; }
        public string? BloodGroup { get; set; }
        public string? Address { get; set; }
    }

    public class UpdateEmployeeProfileCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService)
        : IRequestHandler<UpdateEmployeeProfileCommand, ApiResponse<bool>>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly ICurrentUserService _currentUserService = currentUserService;

        public async Task<ApiResponse<bool>> Handle(
            UpdateEmployeeProfileCommand request, CancellationToken cancellationToken)
        {
            if (!_currentUserService.UserId.HasValue)
            {
                return ApiResponse<bool>.Failure("Unauthorized access.", null, 401);
            }

            // Validate user exists
            var user = await _unitOfWork.User.GetFirstOrDefaultAsync(
                u => u.Id == request.UserId && !u.IsDeleted);
            if (user == null)
            {
                return ApiResponse<bool>.Failure("User not found.", null, 404);
            }

            // Validate salary
            if (request.Salary < 0)
            {
                return ApiResponse<bool>.Failure("Salary cannot be negative.", null, 400);
            }

            // Validate AadhaarLast4 format
            if (!string.IsNullOrEmpty(request.AadhaarLast4) && request.AadhaarLast4.Length != 4)
            {
                return ApiResponse<bool>.Failure("Aadhaar last 4 must be exactly 4 characters.", null, 400);
            }

            // Validate blood group
            if (!string.IsNullOrEmpty(request.BloodGroup))
            {
                string[] validBloodGroups = ["A+", "A-", "B+", "B-", "AB+", "AB-", "O+", "O-"];
                if (!validBloodGroups.Contains(request.BloodGroup))
                {
                    return ApiResponse<bool>.Failure(
                        $"Invalid blood group. Must be one of: {string.Join(", ", validBloodGroups)}", null, 400);
                }
            }

            // Check if profile already exists (upsert pattern)
            var existingProfile = await _unitOfWork.EmployeeProfile.GetFirstOrDefaultAsync(
                ep => ep.UserId == request.UserId && !ep.IsDeleted);

            if (existingProfile != null)
            {
                // Update existing profile
                existingProfile.Salary = request.Salary;
                existingProfile.BankAccountNo = request.BankAccountNo;
                existingProfile.BankIfsc = request.BankIfsc;
                existingProfile.PanNumber = request.PanNumber;
                existingProfile.AadhaarLast4 = request.AadhaarLast4;
                existingProfile.EmergencyContactName = request.EmergencyContactName;
                existingProfile.EmergencyContactPhone = request.EmergencyContactPhone;
                existingProfile.BloodGroup = request.BloodGroup;
                existingProfile.Address = request.Address;
                existingProfile.ModifiedBy = _currentUserService.Username ?? "System";
                existingProfile.ModifiedAt = DateTime.UtcNow;

                _unitOfWork.EmployeeProfile.Update(existingProfile);
            }
            else
            {
                // Create new profile
                EmployeeProfile newProfile = new()
                {
                    UserId = request.UserId,
                    Salary = request.Salary,
                    BankAccountNo = request.BankAccountNo,
                    BankIfsc = request.BankIfsc,
                    PanNumber = request.PanNumber,
                    AadhaarLast4 = request.AadhaarLast4,
                    EmergencyContactName = request.EmergencyContactName,
                    EmergencyContactPhone = request.EmergencyContactPhone,
                    BloodGroup = request.BloodGroup,
                    Address = request.Address,
                    CreatedBy = _currentUserService.Username ?? "System"
                };

                await _unitOfWork.EmployeeProfile.AddAsync(newProfile);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return ApiResponse<bool>.Success(true, existingProfile != null
                ? "Employee profile updated successfully."
                : "Employee profile created successfully.");
        }
    }
}
