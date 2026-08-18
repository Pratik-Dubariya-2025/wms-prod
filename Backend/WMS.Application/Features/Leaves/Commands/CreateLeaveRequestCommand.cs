using MediatR;
using WMS.Application.Common.Attributes;
using WMS.Application.Common.Constants;
using WMS.Application.Common.Interfaces;
using WMS.Application.Common.Models;
using WMS.Domain.Interfaces;
using WMS.Domain.Models;

namespace WMS.Application.Features.Leaves.Commands
{
    /// <summary>
    /// POST /api/leaves
    /// Employee submits a leave request. Per SRS 3.5.1, the direct manager
    /// approves or rejects it.
    /// Permission: leave.write
    /// </summary>
    [RequirePermission(PermissionCodes.LeaveWrite)]
    public class CreateLeaveRequestCommand : IRequest<ApiResponse<Guid>>
    {
        public string LeaveType { get; set; } = null!;
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public decimal DaysCount { get; set; }
        public string? Reason { get; set; }
    }

    public class CreateLeaveRequestCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService)
        : IRequestHandler<CreateLeaveRequestCommand, ApiResponse<Guid>>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly ICurrentUserService _currentUserService = currentUserService;

        public async Task<ApiResponse<Guid>> Handle(
            CreateLeaveRequestCommand request, CancellationToken cancellationToken)
        {
            if (!_currentUserService.UserId.HasValue)
            {
                return ApiResponse<Guid>.Failure("Unauthorized access.", null, 401);
            }

            var currentUserId = _currentUserService.UserId.Value;

            // Validate leave type
            string[] validLeaveTypes = ["Annual", "Sick", "Casual", "Maternity", "Paternity", "Unpaid"];
            if (!validLeaveTypes.Contains(request.LeaveType))
            {
                return ApiResponse<Guid>.Failure(
                    $"Invalid leave type. Must be one of: {string.Join(", ", validLeaveTypes)}", null, 400);
            }

            // Validate date range
            if (request.FromDate > request.ToDate)
            {
                return ApiResponse<Guid>.Failure("From date cannot be after To date.", null, 400);
            }

            // Validate from date is not in the past (allow today)
            if (request.FromDate.Date < DateTime.UtcNow.Date)
            {
                return ApiResponse<Guid>.Failure("Cannot submit leave for a date in the past.", null, 400);
            }

            // Validate days count
            if (request.DaysCount <= 0)
            {
                return ApiResponse<Guid>.Failure("Days count must be greater than 0.", null, 400);
            }

            // Validate days count is reasonable for the date range
            var maxDays = (request.ToDate.Date - request.FromDate.Date).TotalDays + 1;
            if (request.DaysCount > (decimal)maxDays)
            {
                return ApiResponse<Guid>.Failure(
                    $"Days count ({request.DaysCount}) cannot exceed the date range ({maxDays} days).", null, 400);
            }

            // Check for overlapping leave requests
            var overlapping = await _unitOfWork.LeaveRequest.GetFirstOrDefaultAsync(
                lr => lr.UserId == currentUserId
                    && !lr.IsDeleted
                    && lr.Status != "Rejected"
                    && lr.Status != "Cancelled"
                    && lr.FromDate <= request.ToDate.Date
                    && lr.ToDate >= request.FromDate.Date);

            if (overlapping != null)
            {
                return ApiResponse<Guid>.Failure(
                    "You already have a leave request that overlaps with these dates.", null, 400);
            }

            LeaveRequest leaveRequest = new()
            {
                UserId = currentUserId,
                LeaveType = request.LeaveType,
                FromDate = request.FromDate.Date,
                ToDate = request.ToDate.Date,
                DaysCount = request.DaysCount,
                Reason = request.Reason,
                Status = "Pending",
                CreatedBy = _currentUserService.Username ?? "System"
            };

            await _unitOfWork.LeaveRequest.AddAsync(leaveRequest);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return ApiResponse<Guid>.Success(leaveRequest.Id, "Leave request submitted successfully.", 201);
        }
    }
}
