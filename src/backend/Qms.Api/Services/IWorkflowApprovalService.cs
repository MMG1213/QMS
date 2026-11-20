
using Qms.Api.DTOs;

namespace Qms.Api.Services;

public interface IWorkflowApprovalService
{
    Task ApproveOrRejectAsync(
        int dcrRequestId,
        DCRApprovalDto request);
}
