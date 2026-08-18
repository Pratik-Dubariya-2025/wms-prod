using MediatR;
using WMS.Application.Common.Attributes;
using WMS.Application.Common.Constants;
using WMS.Application.Common.Interfaces;
using WMS.Application.Common.Models;
using WMS.Domain.Interfaces;

namespace WMS.Application.Features.Crm.Commands
{
    /// <summary>
    /// PATCH /api/crm/leads/{id}/stage
    /// BDE or Owner reviews lead and moves it through the pipeline:
    /// NewLead -> Qualified -> Proposal -> Negotiation -> ClosedWon / ClosedLost
    /// </summary>
    [RequirePermission(PermissionCodes.LeadsWrite)]
    public class UpdateLeadStageCommand : IRequest<ApiResponse<bool>>
    {
        public Guid Id { get; set; }
        public string Stage { get; set; } = null!;
    }

    public class UpdateLeadStageCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService)
        : IRequestHandler<UpdateLeadStageCommand, ApiResponse<bool>>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly ICurrentUserService _currentUserService = currentUserService;

        public async Task<ApiResponse<bool>> Handle(
            UpdateLeadStageCommand request, CancellationToken cancellationToken)
        {
            if (!_currentUserService.UserId.HasValue)
            {
                return ApiResponse<bool>.Failure("Unauthorized access.", null, 401);
            }

            var lead = await _unitOfWork.Lead.IncludeAndGetFirstOrDefaultAsync<object>(
                l => l.Id == request.Id && !l.IsDeleted,
                l => l.Owner
            );

            if (lead == null)
            {
                return ApiResponse<bool>.Failure("Lead not found.", null, 404);
            }

            // Check permissions or ownership
            var currentUserId = _currentUserService.UserId.Value;
            bool isAdmin = _currentUserService.Roles.Contains("ADMIN");
            bool isManager = _currentUserService.Roles.Contains("MANAGER");
            bool isBDE = _currentUserService.Roles.Contains("BDE");

            if (!isAdmin && !isManager && !isBDE && lead.OwnerId != currentUserId)
            {
                return ApiResponse<bool>.Failure("You do not have permission to update this lead.", null, 403);
            }

            // Validate stage transition
            string[] validStages = ["NewLead", "Qualified", "Proposal", "Negotiation", "ClosedWon", "ClosedLost"];
            if (!validStages.Contains(request.Stage))
            {
                return ApiResponse<bool>.Failure(
                    $"Invalid stage. Must be one of: {string.Join(", ", validStages)}", null, 400);
            }

            lead.Stage = request.Stage;
            if (request.Stage == "ClosedWon" || request.Stage == "ClosedLost")
            {
                lead.ClosedAt = DateTime.UtcNow;
            }
            else
            {
                lead.ClosedAt = null;
            }

            lead.ModifiedAt = DateTime.UtcNow;
            lead.ModifiedBy = _currentUserService.Username ?? "System";

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return ApiResponse<bool>.Success(true, $"Lead stage updated to {request.Stage} successfully.");
        }
    }
}
