using AutoNex.DTOs.WorkshopInfo;

namespace AutoNex.Services.Interfaces;

public interface IWorkshopInfoService
{
    Task<WorkshopInfoResponse?> GetAsync(CancellationToken cancellationToken = default);
    Task<WorkshopInfoResponse> UpsertAsync(UpdateWorkshopInfoRequest request, CancellationToken cancellationToken = default);
}
