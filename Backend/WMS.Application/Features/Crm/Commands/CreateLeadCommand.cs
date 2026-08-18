using MediatR;
using WMS.Application.Common.Attributes;
using WMS.Application.Common.Constants;
using WMS.Application.Common.Interfaces;
using WMS.Application.Common.Models;
using WMS.Domain.Interfaces;
using WMS.Domain.Models;

namespace WMS.Application.Features.Crm.Commands
{
    /// <summary>
    /// POST /api/crm/leads
    /// BDA creates a new lead.
    /// Permission: leads.write
    /// </summary>
    [RequirePermission(PermissionCodes.LeadsWrite)]
    public class CreateLeadCommand : IRequest<ApiResponse<Guid>>
    {
        public string CompanyName { get; set; } = null!;
        public string ContactName { get; set; } = null!;
        public string? ContactEmail { get; set; }
        public string? ContactPhone { get; set; }
        public decimal? EstimatedValue { get; set; }
        public string? Region { get; set; }
        public string? Source { get; set; }
    }

    public class CreateLeadCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService)
        : IRequestHandler<CreateLeadCommand, ApiResponse<Guid>>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly ICurrentUserService _currentUserService = currentUserService;

        public async Task<ApiResponse<Guid>> Handle(
            CreateLeadCommand request, CancellationToken cancellationToken)
        {
            if (!_currentUserService.UserId.HasValue)
            {
                return ApiResponse<Guid>.Failure("Unauthorized access.", null, 401);
            }

            if (string.IsNullOrWhiteSpace(request.CompanyName))
            {
                return ApiResponse<Guid>.Failure("Company name is required.", null, 400);
            }

            if (string.IsNullOrWhiteSpace(request.ContactName))
            {
                return ApiResponse<Guid>.Failure("Contact name is required.", null, 400);
            }

            Lead lead = new()
            {
                CompanyName = request.CompanyName.Trim(),
                ContactName = request.ContactName.Trim(),
                ContactEmail = request.ContactEmail?.Trim(),
                ContactPhone = request.ContactPhone?.Trim(),
                Stage = "NewLead",
                EstimatedValue = request.EstimatedValue,
                OwnerId = _currentUserService.UserId.Value,
                Region = request.Region?.Trim(),
                Source = request.Source?.Trim(),
                CreatedBy = _currentUserService.Username ?? "System"
            };

            await _unitOfWork.Lead.AddAsync(lead);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return ApiResponse<Guid>.Success(lead.Id, "Lead created successfully.", 201);
        }
    }
}
